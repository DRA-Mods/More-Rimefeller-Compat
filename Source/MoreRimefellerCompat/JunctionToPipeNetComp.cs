using System;
using Verse;

namespace MoreRimefellerCompat
{
    public class JunctionToPipeNetComp : ThingComp
    {
        private IJunctionToPipeNet junctionToPipeNetComp;
        public static Func<JunctionToPipeNetComp, IJunctionToPipeNet> OnPostSpawnSetup;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            junctionToPipeNetComp = OnPostSpawnSetup?.Invoke(this);
            junctionToPipeNetComp?.PostSpawnSetup(respawningAfterLoad);
        }

        public override void ReceiveCompSignal(string signal) => junctionToPipeNetComp?.ReceiveCompSignal(signal);

        public override void CompTick() => junctionToPipeNetComp?.CompTick();

        public interface IJunctionToPipeNet
        {
            void PostSpawnSetup(bool respawningAfterLoad);

            void ReceiveCompSignal(string signal);

            void CompTick();
        }
    }
}
