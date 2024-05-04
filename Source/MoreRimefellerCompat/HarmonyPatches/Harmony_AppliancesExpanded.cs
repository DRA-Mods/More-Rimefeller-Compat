using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Rimefeller;
using RimWorld;
using UnityEngine;
using Verse;

namespace MoreRimefellerCompat.HarmonyPatches;

[HarmonyPatch]
public static class Harmony_AppliancesExpanded
{
    public static bool Prepare(MethodBase targetMethod)
    {
        if (targetMethod != null)
        {
            // Only patch the method if there's a def defined that uses this
            // specific thingClass and has a CompRefuelable that accepts chemfuel.
            var type = targetMethod.DeclaringType!;
            return DefDatabase<ThingDef>.AllDefsListForReading
                .Any(def => type.IsAssignableFrom(def.thingClass) &&
                            def.comps.Any(comp => comp is CompProperties_Refuelable refuelable &&
                                                  refuelable.fuelFilter.Allows(ThingDefOf.Chemfuel)));
        }

        return TargetMethods().Any();
    }

    public static IEnumerable<MethodBase> TargetMethods()
        => new[]
        {
            AccessTools.DeclaredMethod($"AppliancesExpanded.Building_CoolerFueled:{nameof(Thing.TickRare)}"),
            AccessTools.DeclaredMethod($"AppliancesExpanded.Building_HeaterFueled:{nameof(Thing.TickRare)}"),
        }.Where(m => m != null);

    // A more universal approach would be to call GetComp on the instance,
    // but currently it's not needed and using the one already defined
    // will basically mean less processing is needed to handle this.
    public static void Prefix(Building __instance, CompRefuelable ___compRefuelable)
    {
        if (___compRefuelable == null || !___compRefuelable.Props.fuelFilter.Allows(ThingDefOf.Chemfuel))
            return;

        var pipeNet = __instance.GetComp<CompPipe>()?.pipeNet;
        if (pipeNet == null)
            return;

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        var target = ___compRefuelable.configuredTargetFuelLevel == -1f
            ? ___compRefuelable.Props.fuelCapacity
            : ___compRefuelable.TargetFuelLevel;
        var difference = target - ___compRefuelable.Fuel;

        if (difference >= 1f)
        {
            // Max refuel value is 250, since it's only refuelled once every 250 ticks
            // as opposed to once a tick for building ticking normally.
            var value = Mathf.Min(250f, difference);
            if (pipeNet.PullFuel(value))
                ___compRefuelable.Refuel(value);
        }
    }
}