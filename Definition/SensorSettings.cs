using System.Collections.Generic;
using ExtraObjectiveSetup.Utils;
using Localization;
namespace EOSExt.SecuritySensor.Definition
{
    public enum SensorType
    {
        BASIC,
        MOVABLE,
    }

    public class SensorSettings
    {
        public Vec3 Position { get; set; } = new Vec3();

        public float Radius { get; set; } = 2.3f;

        public EOSColor Color { get; set; } = new() { r = 0.9339623f, g = 0.1055641f, b = 0f, a = 0.2627451f };

        public LocalizedText Text { get; set; } = new() { Id = 0, UntranslatedText = "S:_EC/uR_ITY S:/Ca_N" };

        public EOSColor TextColor { get; set; } = new() { r = 226f / 255f, g = 230f / 255f, b = 229 / 255f, a = 181f / 255f };

        public SensorType SensorType { get; set; } = SensorType.BASIC;

        public float MovingSpeedMulti { get; set; } = 1.0f;

        public List<Vec3> MovingPosition { get; set; } = new() { new() }; 
    }
}
