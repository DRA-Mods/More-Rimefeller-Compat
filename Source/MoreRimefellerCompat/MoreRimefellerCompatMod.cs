using HarmonyLib;
#if IDEOLOGY
using MoreRimefellerCompat.HarmonyPatches;
#endif
using MoreRimefellerCompat.Implementations;
using Verse;

namespace MoreRimefellerCompat;

public class MoreRimefellerCompatMod : Mod
{
    private static Harmony harmony;

    public static Harmony Harmony => harmony ??= new Harmony("Dra.MoreRimefellerCompat");

    public MoreRimefellerCompatMod(ModContentPack content) : base(content)
    {
        Harmony.PatchAll();

        if (AccessTools.TypeByName("VFEAncients.Building_PipelineJunction") != null)
            JunctionToPipeNetComp.onPostSpawnSetup = comp => new JunctionToPipeNetImpl(comp);

#if IDEOLOGY
            var method = AccessTools.Method("VanillaPowerExpanded.CompChemfuelPump:CompTick");
            if (method != null)
                Harmony.Patch(method, prefix: new HarmonyMethod(typeof(Harmony_ChemfuelPump), nameof(Harmony_ChemfuelPump.Prefix)));
#endif
    }
}