using System;
using System.Linq;
using HarmonyLib;
using Rimefeller;
using RimWorld;
using VanillaPowerExpanded;
using Verse;

namespace VanillaPowerPatch
{
    [HarmonyPatch(typeof(CompChemfuelPump), nameof(CompChemfuelPump.CompTick))]
    internal static class PatchCompChemfuelPump
    {
        private static bool Prefix(CompChemfuelPump __instance)
        {
            __instance.ticksCounter++;

            if (__instance.ticksCounter <= __instance.ticksInADay * __instance.Props.fuelInterval)
                return false;

            __instance.chemfuelPond = (Building_ChemfuelPond)__instance.parent.Map.thingGrid.ThingAt(__instance.parent.Position, ThingDef.Named("VPE_ChemfuelPond"));
            if (__instance.chemfuelPond.fuelLeft <= 0)
                return false;

            __instance.chemfuelPond.fuelLeft -= __instance.Props.fuelProduced;
            __instance.ticksCounter = 0;

            var pipe = __instance.parent.GetComp<CompPipe>();
            if (pipe == null)
            {
                SpawnChemfuel(__instance, __instance.Props.fuelProduced);
                return false;
            }

            var total = __instance.Props.fuelProduced;
            var maxToRemove = (int)pipe.pipeNet.FuelStorage.Where(x => x.space > 0 && !x.DrainTank).Sum(x => x.space);
            total = Math.Min(total, maxToRemove);

            var remainder = (int)pipe.pipeNet.PushFuel(total);
            if (remainder > 0 || total < __instance.Props.fuelProduced)
                SpawnChemfuel(__instance, __instance.Props.fuelProduced - (total - remainder));

            return false;
        }

        private static void SpawnChemfuel(CompChemfuelPump comp, int value)
        {
            var thing = ThingMaker.MakeThing(ThingDefOf.Chemfuel);
            thing.stackCount = value; // comp.Props.fuelProduced;
            GenPlace.TryPlaceThing(thing, comp.parent.Position, comp.parent.Map, ThingPlaceMode.Near);
        }
    }
}
