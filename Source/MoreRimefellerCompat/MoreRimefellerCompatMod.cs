using HarmonyLib;
using Verse;

namespace MoreRimefellerCompat
{
    public class MoreRimefellerCompatMod : Mod
    {
        private static Harmony harmony;

        public static Harmony Harmony => harmony ??= new Harmony("Dra.MoreRimefellerCompat");

        public MoreRimefellerCompatMod(ModContentPack content) : base(content)
        {
            var assembliesDirectory = ModContentPack.GetAllFilesForModPreserveOrder(content, "Referenced/", f => f.ToLower() == ".dll");

            Harmony.PatchAll();
            AssemblyLoader.LoadAssembly(assembliesDirectory, "VanillaPowerPatch.dll", "vanillaexpanded.vfepower");
            AssemblyLoader.LoadAssembly(assembliesDirectory, "VanillaAncientsPatch.dll", "vanillaexpanded.vfea");

#if DEBUG
            ReferenceBuilder.Restore(content);
#endif
        }
    }
}