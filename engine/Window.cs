using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using wraithspire.engine.subsystems;
using wraithspire.engine.objects.primitives;
using ImGuiNET;

namespace wraithspire.engine
{
    internal sealed class Window : IDisposable
    {
        private readonly GameWindow _window;
        private ImGuiController? _imgui;
        private EditorUI? _editorUI;
        private Scene? _scene;
        private Renderer? _renderer;

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
            
            _scene = new Scene();
            _scene.Initialize();

            _renderer = new Renderer();

            _editorUI = new EditorUI();
            _editorUI.SceneContext = _scene;
        }

        private void OnUpdateFrame(FrameEventArgs args)
        {
            _imgui?.Update((float)args.Time);
        }

        private void OnRenderFrame(FrameEventArgs args)
        {
            GL.Viewport(0, 0, _window.ClientSize.X, _window.ClientSize.Y);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (_renderer != null && _scene != null && _editorUI != null)
            {
                // The renderer needs view/proj from the editor/camera
                // We should calculate the viewport for the "Game View" or "Scene View"
                
                // Keep the same viewport calculation logic if possible, or pass it to Renderer
                // But Renderer.Render expects whole screen/framebuffer commands usually for now let's just use what was there
                // Actually the previous code did a Viewport calculation.
                
                float width = _window.ClientSize.X;
                float height = _window.ClientSize.Y;
                float topMargin = 0f;
                float leftWidth = MathF.Round(width * 0.20f);
                float rightWidth = MathF.Round(width * 0.25f);
                float bottomHeight = MathF.Round(height * 0.30f);
                float centerWidth = width - leftWidth - rightWidth;
                float centerHeight = height - bottomHeight - topMargin;
                // int viewportWidth = (int)centerWidth;
                // int viewportHeight = (int)(centerHeight - 100f); 

                var proj = _editorUI.CameraProjection;
                var view = _editorUI.CameraView;
                
                _renderer.Render(_scene, view, proj);
            }

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
            _scene?.Dispose();
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
