using System.Linq;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using Rimefeller;
using RimWorld;
using Verse;

namespace MoreRimefellerCompat.HarmonyPatches;

[HarmonyPatch(typeof(CompSpawner), nameof(CompSpawner.TryDoSpawn))]
internal static class Harmony_CompSpawner
{
    private static bool Prepare(MethodBase targetMethod)
    {
        if (targetMethod != null)
            return true;

        // Only patch if there's any building using CompSpawner that spawns chemfuel and it has CompPipe.
        return DefDatabase<ThingDef>.AllDefsListForReading
            .Any(def => def.GetCompProperties<CompProperties_Pipe>() != null &&
                        def.comps.Any(comp => comp is CompProperties_Spawner spawner &&
                                              spawner.thingToSpawn == ThingDefOf.Chemfuel));
    }

    private static bool Prefix(CompSpawner __instance)
    {
        if (!__instance.parent.Spawned)
            return false;

        if (__instance.PropsSpawner.thingToSpawn != ThingDefOf.Chemfuel)
            return true;

        var pipe = __instance.parent.GetComp<CompPipe>();
        if (pipe == null)
            return true;

        var total = __instance.PropsSpawner.spawnCount;
        var maxToRemove = (int)pipe.pipeNet.FuelStorage.Where(x => x.space > 0 && !x.DrainTank).Sum(x => x.space);

        if (total > maxToRemove)
            return true;

        var remainder = (int)pipe.pipeNet.PushFuel(total);
        if (remainder <= 0)
        {
            if (__instance.PropsSpawner.showMessageIfOwned && __instance.parent.Faction == Faction.OfPlayer)
            {
                Messages.Message("MessageCompSpawnerSpawnedItem".Translate(__instance.PropsSpawner.thingToSpawn.LabelCap), __instance.parent, MessageTypeDefOf.PositiveEvent);
            }

            return false;
        }

        pipe.pipeNet.PullFuel(maxToRemove - remainder);
        return true;
    }
}