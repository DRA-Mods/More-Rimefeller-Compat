using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using Mono.Cecil;
using Verse;

namespace MoreRimefellerCompat
{
    internal static class AssemblyLoader
    {
        public static void LoadAssembly(IEnumerable<Tuple<string, FileInfo>> assemblies, string targetAssembly, string requiredModId)
        {
            if (requiredModId == null || !IsModRunning(requiredModId))
                return;

            var path = assemblies.FirstOrDefault(f => string.Equals(f.Item2.Name, targetAssembly, StringComparison.InvariantCultureIgnoreCase))?.Item2;

            if (path == null)
            {
                Log.Error($"Could not find target assembly: {targetAssembly} - patch is not going to work!");
                return;
            }

            var assembly = AssemblyDefinition.ReadAssembly(path.FullName);
            using var stream = new MemoryStream();
            assembly.Write(stream);

            var result = AppDomain.CurrentDomain.Load(stream.ToArray());
            MoreRimefellerCompatMod.Harmony.PatchAll(result);
            var type = result.GetTypes().FirstOrDefault(x => x.Name == "StaticInit");
            if (type != null)
                AccessTools.Method(type, "Init", Array.Empty<Type>())?.Invoke(null, Array.Empty<object>());
        }

        private static bool IsModRunning(string modId) =>
            LoadedModManager.RunningMods.FirstOrDefault(m => string.Equals(NoModIdSuffix(m.PackageId), modId, StringComparison.CurrentCultureIgnoreCase)) != null;

        private static string NoModIdSuffix(string modId)
        {
            const string steamSuffix = "_steam";
            const string copySuffix = "_copy";

            while (true)
            {
                if (modId.EndsWith(steamSuffix))
                {
                    modId = modId.Substring(0, modId.Length - steamSuffix.Length);
                    continue;
                }

                if (modId.EndsWith(copySuffix))
                {
                    modId = modId.Substring(0, modId.Length - copySuffix.Length);
                    continue;
                }

                return modId;
            }
        }
    }
}
