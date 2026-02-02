using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using wraithspire.engine.subsystems;
using ImGuiNET;

namespace wraithspire.engine
{
    internal sealed class Window : IDisposable
    {
        private readonly GameWindow _window;
        private ImGuiController? _imgui;
        private objects.CheckboardTerrain? _terrain;
        private EditorUI? _editorUI;

        public Window(int width = 1280, int height = 720, string title = "Wraithspire Engine")
        {
            var nativeSettings = new NativeWindowSettings
            {
                ClientSize = new Vector2i(width, height),
                Title = title
            };

            var gwSettings = new GameWindowSettings
            {
                UpdateFrequency = 60
            };

            _window = new GameWindow(gwSettings, nativeSettings);
            _window.WindowState = WindowState.Maximized;

            _window.Load += OnLoad;
            _window.UpdateFrame += OnUpdateFrame;
            _window.RenderFrame += OnRenderFrame;
            _window.Resize += OnResize;
            _window.Unload += OnUnload;
        }

        private void OnLoad()
        {
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1f);
            _imgui = new ImGuiController(_window);
            _editorUI = new EditorUI();
            _terrain = new objects.CheckboardTerrain();
            _terrain.Initialize();
        }

        private void OnUpdateFrame(FrameEventArgs args)
        {
            _imgui?.Update((float)args.Time);
        }

        private void OnRenderFrame(FrameEventArgs args)
        {
            GL.Viewport(0, 0, _window.ClientSize.X, _window.ClientSize.Y);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            RenderTerrain();
            _editorUI?.Render(_window);

            _imgui?.Render();
            _window.SwapBuffers();
        }

        private void OnResize(ResizeEventArgs size)
        {
            _imgui?.WindowResized(size.Width, size.Height);
        }

        private void OnUnload()
        {
            _imgui?.Dispose();
            _terrain?.Dispose();
        }

        private void RenderTerrain()
        {
            if (_terrain == null) return;
            if (_editorUI == null) return;

            // Compute center viewport below toolbar using same layout values
            float width = _window.ClientSize.X;
            float height = _window.ClientSize.Y;
            float topMargin = 0f;
            float leftWidth = MathF.Round(width * 0.20f);
            float rightWidth = MathF.Round(width * 0.25f);
            float bottomHeight = MathF.Round(height * 0.30f);
            float centerWidth = width - leftWidth - rightWidth;
            float centerHeight = height - bottomHeight - topMargin;
            int viewportWidth = (int)centerWidth;
            int viewportHeight = (int)(centerHeight - 100f); // subtract toolbar height
            GL.Enable(EnableCap.DepthTest);
            var proj = _editorUI.CameraProjection;
            var view = _editorUI.CameraView;
            _terrain.Render(proj, view);
        }

        public void Run()
        {
            _window.Run();
        }

        public void Dispose()
        {
            _window?.Dispose();
        }
    }
}
