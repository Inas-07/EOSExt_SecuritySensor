using System.Collections.Generic;
using GameData;

namespace EOSExt.SecuritySensor.Definition
{
    public class SensorGroupSettings
    {
        // index used by ToggleSensorGroupState / ToggleAllSensorGroups; defaults to build order
        public uint Index { set; get; } = uint.MaxValue;

        public List<SensorSettings> SensorGroup { set; get; } = new() { new() };

        public List<WardenObjectiveEventData> EventsOnTrigger { set; get; } = new() { };
    }
}
