using JetBrains.Annotations;
using MoreRimefellerCompat;

namespace VanillaAncientsPatch
{
    public static class StaticInit
    {
        [UsedImplicitly]
        public static void Init() => JunctionToPipeNetComp.OnPostSpawnSetup = comp => new JunctionToPipeNetImpl(comp);
    }
}
