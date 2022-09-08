using System;
using System.Linq;
using MoreRimefellerCompat;
using Rimefeller;
using RimWorld;
using Verse;
using VFEAncients;

namespace VanillaAncientsPatch
{
    public class JunctionToPipeNetImpl : JunctionToPipeNetComp.IJunctionToPipeNet
    {
        private bool isFull = false;
        private bool isHackable = true;
        private readonly JunctionToPipeNetComp comp;
        private CompPipe compPipe;

        private CompPipe CompPipe => compPipe ??= comp.parent.GetComp<CompPipe>();

        public JunctionToPipeNetImpl(JunctionToPipeNetComp comp) => this.comp = comp;

        public void PostSpawnSetup(bool respawningAfterLoad)
        {
            isHackable = comp.parent.GetComp<CompHackable>() != null;
            if (isHackable)
                isFull = comp.parent is Building_PipelineJunction {HasAnyContents: true};
        }

        public void ReceiveCompSignal(string signal)
        {
            if (signal != CompHackable.HackedSignal)
                return;

            isFull = true;
        }

        public void CompTick()
        {
            if ((!isFull && isHackable) || !comp.parent.IsHashIntervalTick(60))
                return;

            if (comp.parent is not Building_PipelineJunction { HasAnyContents: true } building || CompPipe == null)
            {
                isFull = false;
                return;
            }

            var total = building.innerContainer.TotalStackCount;
            var toRemove = (int)CompPipe.pipeNet.FuelStorage.Where(x => x.space > 0 && !x.DrainTank).Sum(x => x.space);
            toRemove = Math.Min(toRemove, total);
            toRemove = Math.Min(toRemove, 5);

            var remainder = CompPipe.pipeNet.PushFuel(toRemove);
            if ((int)remainder == toRemove)
                return;
            if (remainder < 1 && toRemove == total)
            {
                building.innerContainer.ClearAndDestroyContents();
                building.ticksTillRefill = building.RefillTime;
                building.contentsKnown = true;
                return;
            }

            toRemove = (int)(toRemove - remainder);

            for (var i = building.innerContainer.Count - 1; i >= 0 && toRemove > 0; i--)
            {
                var thing = building.innerContainer.GetAt(i);

                var removeCount = Math.Min(toRemove, thing.stackCount);
                toRemove -= removeCount;
                var removedThing = building.innerContainer.Take(thing, removeCount);
                removedThing.Destroy();
            }

            if (!building.HasAnyContents)
            {
                building.ticksTillRefill = building.RefillTime;
                building.contentsKnown = true;
            }
        }
    }
}
