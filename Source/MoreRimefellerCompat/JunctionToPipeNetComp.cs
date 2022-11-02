using System;
using Verse;

namespace MoreRimefellerCompat;

public class JunctionToPipeNetComp : ThingComp
{
    private IJunctionToPipeNet junctionToPipeNetComp;
    public static Func<JunctionToPipeNetComp, IJunctionToPipeNet> onPostSpawnSetup = _ => new DummyImplementation();

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        junctionToPipeNetComp = onPostSpawnSetup?.Invoke(this);
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
        
    private class DummyImplementation : IJunctionToPipeNet
    {
        public void PostSpawnSetup(bool respawningAfterLoad)
        {
        }

        public void ReceiveCompSignal(string signal)
        {
        }

        public void CompTick()
        {
        }
    }
}