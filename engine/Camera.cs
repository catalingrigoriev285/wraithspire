using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace wraithspire.engine
{
    internal sealed class Camera
    {
        private float _yaw = -MathHelper.PiOver2; // along +Z
        private float _pitch = -0.3f;
        public Vector3 Position { get; private set; } = new Vector3(0f, 8f, -12f);
        private Vector2 _lastMouse;
        private bool _rotating;

        public Matrix4 View { get; private set; } = Matrix4.Identity;
        public Matrix4 Projection { get; private set; } = Matrix4.Identity;

        public void Update(GameWindow window, int vpWidth, int vpHeight, bool processInput = true)
        {
            var kb = window.KeyboardState;
            var mouse = window.MouseState;

            float dt = 1f / 60f; // approximate
            float speed = kb.IsKeyDown(Keys.LeftShift) ? 10f : 5f;
            Vector3 forward = new Vector3(MathF.Cos(_yaw) * MathF.Cos(_pitch), MathF.Sin(_pitch), MathF.Sin(_yaw) * MathF.Cos(_pitch));
            forward = Vector3.Normalize(forward);
            Vector3 right = Vector3.Normalize(Vector3.Cross(forward, Vector3.UnitY));

            if (processInput)
            {
                if (kb.IsKeyDown(Keys.W)) Position += forward * speed * dt;
                if (kb.IsKeyDown(Keys.S)) Position -= forward * speed * dt;
                if (kb.IsKeyDown(Keys.A)) Position -= right * speed * dt;
                if (kb.IsKeyDown(Keys.D)) Position += right * speed * dt;
                if (kb.IsKeyDown(Keys.Space)) Position += Vector3.UnitY * speed * dt;
                if (kb.IsKeyDown(Keys.LeftControl)) Position -= Vector3.UnitY * speed * dt;

                bool rightDown = mouse.IsButtonDown(MouseButton.Right);
                var curMouse = new Vector2(mouse.X, mouse.Y);
                if (rightDown)
                {
                    if (!_rotating)
                    {
                        _rotating = true;
                        _lastMouse = curMouse;
                    }
                    else
                    {
                        var delta = curMouse - _lastMouse;
                        _lastMouse = curMouse;
                        float sens = 0.0035f;
                        _yaw += delta.X * sens;
                        _pitch -= delta.Y * sens;
                        _pitch = MathHelper.Clamp(_pitch, -MathHelper.PiOver2 + 0.01f, MathHelper.PiOver2 - 0.01f);
                    }
                }
                else
                {
                    _rotating = false;
                }
            }

            Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60f), vpWidth / (float)vpHeight, 0.1f, 2000f);
            Vector3 target = Position + forward;
            View = Matrix4.LookAt(Position, target, Vector3.UnitY);

        }

    }
}
