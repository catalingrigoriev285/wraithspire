using OpenTK.Mathematics;

namespace wraithspire.engine.components
{
    public class Transform : Component
    {
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Vector3 Rotation { get; set; } = Vector3.Zero; // Euler angles in degrees
        public Vector3 Scale { get; set; } = Vector3.One;

        public Matrix4 GetModelMatrix()
        {
            var model = Matrix4.Identity;
            model *= Matrix4.CreateScale(Scale);
            model *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(Rotation.X));
            model *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(Rotation.Y));
            model *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(Rotation.Z));
            model *= Matrix4.CreateTranslation(Position);
            return model;
        }

        public Vector3 Forward
        {
            get
            {
                var rotation = Quaternion.FromEulerAngles(
                    MathHelper.DegreesToRadians(Rotation.X),
                    MathHelper.DegreesToRadians(Rotation.Y),
                    MathHelper.DegreesToRadians(Rotation.Z)
                );
                return -Vector3.Transform(Vector3.UnitZ, rotation);
            }
        }
        
        public Vector3 Right
        {
            get
            {
                var rotation = Quaternion.FromEulerAngles(
                    MathHelper.DegreesToRadians(Rotation.X),
                    MathHelper.DegreesToRadians(Rotation.Y),
                    MathHelper.DegreesToRadians(Rotation.Z)
                );
                return Vector3.Transform(Vector3.UnitX, rotation);
            }
        }
        
        public Vector3 Up
        {
            get
            {
                var rotation = Quaternion.FromEulerAngles(
                    MathHelper.DegreesToRadians(Rotation.X),
                    MathHelper.DegreesToRadians(Rotation.Y),
                    MathHelper.DegreesToRadians(Rotation.Z)
                );
                return Vector3.Transform(Vector3.UnitY, rotation);
            }
        }

        public void Translate(Vector3 translation)
        {
            Position += translation;
        }
    }
}
