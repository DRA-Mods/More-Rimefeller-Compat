#if IDEOLOGY
using System;
using System.Linq;
using Rimefeller;
using RimWorld;
using VanillaPowerExpanded;
using Verse;

namespace MoreRimefellerCompat.HarmonyPatches;

internal static class Harmony_ChemfuelPump
{
    public static bool Prefix(ThingComp __instance)
    {
        if (__instance is not CompChemfuelPump pump)
            return true;

        pump.ticksCounter++;

        if (pump.ticksCounter <= pump.ticksInADay * pump.Props.fuelInterval)
            return false;

        pump.chemfuelPond = (Building_ChemfuelPond)pump.parent.Map.thingGrid.ThingAt(pump.parent.Position, ThingDef.Named("VPE_ChemfuelPond"));
        if (pump.chemfuelPond.fuelLeft <= 0)
            return false;

        pump.chemfuelPond.fuelLeft -= pump.Props.fuelProduced;
        pump.ticksCounter = 0;

        var pipe = pump.parent.GetComp<CompPipe>();
        if (pipe == null)
        {
            SpawnChemfuel(pump, pump.Props.fuelProduced);
            return false;
        }

        var total = pump.Props.fuelProduced;
        var maxToRemove = (int)pipe.pipeNet.FuelStorage.Where(x => x.space > 0 && !x.DrainTank).Sum(x => x.space);
        total = Math.Min(total, maxToRemove);

        var remainder = (int)pipe.pipeNet.PushFuel(total);
        if (remainder > 0 || total < pump.Props.fuelProduced)
            SpawnChemfuel(pump, pump.Props.fuelProduced - (total - remainder));

        return false;
    }

    private static void SpawnChemfuel(ThingComp comp, int value)
    {
        var thing = ThingMaker.MakeThing(ThingDefOf.Chemfuel);
        thing.stackCount = value; // comp.Props.fuelProduced;
        GenPlace.TryPlaceThing(thing, comp.parent.Position, comp.parent.Map, ThingPlaceMode.Near);
    }
}
#endif