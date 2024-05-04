using HarmonyLib;
using MoreRimefellerCompat.Implementations;
using Verse;

namespace MoreRimefellerCompat;

public class MoreRimefellerCompatMod : Mod
{
    private static Harmony harmony;

    public static Harmony Harmony => harmony ??= new Harmony("Dra.MoreRimefellerCompat");

    public MoreRimefellerCompatMod(ModContentPack content) : base(content)
    {
        LongEventHandler.ExecuteWhenFinished(() =>
        {
            Harmony.PatchAll();
        });

        if (AccessTools.TypeByName("VFEAncients.Building_PipelineJunction") != null)
            JunctionToPipeNetComp.onPostSpawnSetup = comp => new JunctionToPipeNetImpl(comp);
    }
}