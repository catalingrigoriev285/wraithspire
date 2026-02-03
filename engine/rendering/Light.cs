using OpenTK.Mathematics;

namespace wraithspire.engine.rendering
{
    public struct Light
    {
        public Vector3 Position;
        public Vector3 Color;
        public float Intensity;

        public Light(Vector3 position, Vector3 color, float intensity = 1.0f)
        {
            Position = position;
            Color = color;
            Intensity = intensity;
        }
    }
}
