using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using Rimefeller;
using RimWorld;
using Verse;

namespace MoreRimefellerCompat.HarmonyPatches;

[HarmonyPatch]
public static class Harmony_BioReactor
{
    private static bool Prepare(MethodBase targetMethod)
    {
        if (targetMethod == null)
            return TargetMethod() != null;

        var type = targetMethod.DeclaringType!;
        // Only patch if there's any bioreactor ThingDef with a pipe comp.
        return DefDatabase<ThingDef>.AllDefsListForReading
            .Any(def => def.GetCompProperties<CompProperties_Pipe>() != null &&
                        type.IsAssignableFrom(def.thingClass));
    }

    private static MethodBase TargetMethod() 
        => AccessTools.DeclaredMethod("BioReactor.Building_BioReactor:MakeFuel");

    private static bool Prefix(Building __instance)
    {
        var pipe = __instance.GetComp<CompPipe>();
        if (pipe == null)
        {
            SpawnChemfuel(__instance, 35);
            return false;
        }

        var toRemove = (int)pipe.pipeNet.FuelStorage.Where(x => x.space > 0 && !x.DrainTank).Sum(x => x.space);
        toRemove = Math.Min(35, toRemove);

        var remainder = (int)pipe.pipeNet.PushFuel(toRemove);
        if (remainder > 0 || toRemove < 35)
            SpawnChemfuel(__instance, 35 - (toRemove - remainder));

        return false;
    }

    private static void SpawnChemfuel(Building building, int count)
    {
        var stuff = GenStuff.RandomStuffFor(ThingDefOf.Chemfuel);
        var thing = ThingMaker.MakeThing(ThingDefOf.Chemfuel, stuff);
        thing.stackCount = count;
        GenPlace.TryPlaceThing(thing, building.Position, building.Map, ThingPlaceMode.Near);
    }
}