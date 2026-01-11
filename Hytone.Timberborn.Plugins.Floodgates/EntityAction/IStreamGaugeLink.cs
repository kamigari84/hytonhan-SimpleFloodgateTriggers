using Timberborn.BaseComponentSystem;

namespace Hytone.Timberborn.Plugins.Floodgates.EntityAction
{
    public interface IStreamGaugeLink
    {
        bool DisableDuringBadtide { get; set; }
        bool DisableDuringDrought { get; set; }
        bool DisableDuringTemperate { get; set; }
        StreamGaugeMonoBehaviour StreamGauge { get; }
    }
}