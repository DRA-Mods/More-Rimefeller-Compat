using System.Linq;
using HarmonyLib;
using Rimefeller;
using RimWorld;
using Verse;

namespace MoreRimefellerCompat
{
    [HarmonyPatch(typeof(CompSpawner), nameof(CompSpawner.TryDoSpawn))]
    internal static class PatchCompSpawner
    {
        public static bool Prefix(CompSpawner __instance)
        {
            if (!__instance.parent.Spawned)
                return false;

            if (__instance.PropsSpawner.thingToSpawn != ThingDefOf.Chemfuel)
                return false;

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
}
