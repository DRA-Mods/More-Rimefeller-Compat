#if DEBUG
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Mono.Cecil;
using Verse;

namespace MoreRimefellerCompat
{
    // Taken from, slightly modified:
    // https://github.com/rwmt/Multiplayer-Compatibility/blob/d30d441a6bcbe0861c595757b30c96d055e43f56/Source/ReferenceBuilder.cs
    internal static class ReferenceBuilder
    {
        public static void Restore(ModContentPack content, bool replaceExisting = false)
        {
            var requestedFiles = ModContentPack.GetAllFilesForModPreserveOrder(content, "References/", f => f.ToLower() == ".txt");

            foreach (var (_, file) in requestedFiles)
            {
                if (replaceExisting || !File.Exists(Path.Combine(file.Directory!.FullName, $"{Path.GetFileNameWithoutExtension(file.Name)}.dll")))
                    BuildReference(file);
            }
        }

        private static void BuildReference(FileInfo request)
        {
            var asmId = Path.GetFileNameWithoutExtension(request.Name);
            var refsFolder = request.Directory;

            var assembly = LoadedModManager.RunningModsListForReading
                .SelectMany(m => ModContentPack.GetAllFilesForModPreserveOrder(m, "Assemblies/", f => f.ToLower() == ".dll"))
                .FirstOrDefault(f => f.Item2.Name == asmId + ".dll")?.Item2;

            if (assembly == null || refsFolder == null)
                return;

            var hash = ComputeHash(assembly.FullName);
            var hashFile = Path.Combine(refsFolder.FullName, asmId + ".txt");

            if (File.Exists(hashFile) && File.ReadAllText(hashFile) == hash)
                return;

            Log.Warning($"MpCompat References: Writing {asmId}.dll");

            var outFile = Path.Combine(refsFolder.FullName, asmId + ".dll");
            var asmDef = AssemblyDefinition.ReadAssembly(assembly.FullName);

            foreach (var t in asmDef.MainModule.GetTypes())
            {
                if (t.IsNested)
                    t.IsNestedPublic = true;
                else
                    t.IsPublic = true;

                foreach (var m in t.Methods)
                {
                    m.IsPublic = true;
                    m.Body = new Mono.Cecil.Cil.MethodBody(m);
                }

                foreach (var f in t.Fields)
                {
                    f.IsInitOnly = false;
                    f.IsPublic = true;
                }
            }

            asmDef.Write(outFile);
            File.WriteAllText(hashFile, hash);
        }

        private static string ComputeHash(string assemblyPath)
        {
            var res = new StringBuilder();

            using var hash = SHA1.Create();
            using var file = File.Open(assemblyPath, FileMode.Open, FileAccess.Read);

            hash.ComputeHash(file);

            foreach (var b in hash.Hash)
                res.Append(b.ToString("X2"));

            return res.ToString();
        }
    }
}
#endif