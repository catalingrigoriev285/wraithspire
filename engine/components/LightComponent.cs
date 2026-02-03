using OpenTK.Mathematics;

namespace wraithspire.engine.components
{
    public class LightComponent : Component
    {
        public Vector3 Color { get; set; } = Vector3.One;
        public float Intensity { get; set; } = 1.0f;
    }
}
