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
        private EditorUI? _editorUI;
        private SceneManager? _sceneManager;
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
            _window.TextInput += OnTextInput;
            _window.Unload += OnUnload;
        }

        private void OnTextInput(TextInputEventArgs e)
        {
            _imgui?.PressChar((char)e.Unicode);
        }

        private void OnLoad()
        {
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1f);
            
            _imgui = new ImGuiController(_window);
            
            _sceneManager = new SceneManager();
            _sceneManager.Initialize();

            _renderer = new Renderer();

            _editorUI = new EditorUI();
            _editorUI.ManagerContext = _sceneManager;
        }


        private void OnUpdateFrame(FrameEventArgs args)
        {
            Time.DeltaTime = (float)args.Time;
            Time.TotalTime += Time.DeltaTime;

            _imgui?.Update((float)args.Time);
            
            _sceneManager?.ActiveScene?.Update();
        }


        private void OnRenderFrame(FrameEventArgs args)
        {
            GL.Viewport(0, 0, _window.ClientSize.X, _window.ClientSize.Y);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (_renderer != null && _sceneManager != null && _sceneManager.ActiveScene != null && _editorUI != null)
            {
                float width = _window.ClientSize.X;
                float height = _window.ClientSize.Y;
                
                var proj = _editorUI.CameraProjection;
                var view = _editorUI.CameraView;
                
                _renderer.Render(_sceneManager.ActiveScene, view, proj);
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
            _sceneManager?.Dispose();
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
