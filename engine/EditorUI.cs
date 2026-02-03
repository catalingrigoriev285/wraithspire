using System;
using System.Collections.Generic;
using ImGuiNET;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using wraithspire.engine.objects;
using wraithspire.engine.components;
using wraithspire.engine.editor;
using wraithspire.engine.physics;
using wraithspire.engine.editor.modules;

namespace wraithspire.engine
{
    internal sealed class EditorUI : IDisposable
    {
        // Public state for modules
        public bool IsPlaying { get; set; }
        public bool IsPaused { get; set; }
        public GameObject? SelectedObject { get; set; } = null;
        public Camera Camera => _camera;
        public SceneManager? ManagerContext 
        { 
            get => _managerContext;
            set
            {
                _managerContext = value;
                InitializeModules();
            }
        }

        public Matrix4 CameraView => _camera.View;
        public Matrix4 CameraProjection => _camera.Projection;

        private SceneManager? _managerContext;
        private Camera _camera = new Camera();
        private GizmoController _gizmoController = new GizmoController();
        private List<IEditorModule> _modules = new List<IEditorModule>();

        private void InitializeModules()
        {
            if (_managerContext == null) return;

            _modules.Clear();
            _modules.Add(new ToolbarModule());
            _modules.Add(new HierarchyModule());
            _modules.Add(new ProjectModule());
            _modules.Add(new InspectorModule());

            foreach (var module in _modules)
            {
                module.Initialize(this, _managerContext);
            }
        }

        public float MainMenuBarHeight { get; private set; } = 0f;

        public void Render(GameWindow window)
        {
            float width = window.ClientSize.X;
            float height = window.ClientSize.Y;

            // Handle Picking and Gizmo (Core Editor logic)
            HandleInput(window);

            // Render Modules
            RenderLayoutBounds();
            
            // Main Menu Bar
            MainMenuBarHeight = 0f;
            if (ImGui.BeginMainMenuBar())
            {
                MainMenuBarHeight = ImGui.GetWindowSize().Y;
                if (ImGui.BeginMenu("View"))
                {
                    foreach (var module in _modules)
                    {
                        bool isVisible = module.IsVisible;
                        if (ImGui.MenuItem(module.Name, "", isVisible))
                        {
                            module.IsVisible = !isVisible;
                        }
                    }
                    ImGui.EndMenu();
                }
                ImGui.EndMainMenuBar();
            }

            foreach (var module in _modules)
            {
                module.Render(window);
            }
        }

        private void HandleInput(GameWindow window)
        {
            // Similar layout calcs for picking strict check if mouse is not over UI
            // But with modular windows, "isMouseInViewport" is harder to define strictly without proper docking.
            // For now, we rely on ImGui.GetIO().WantCaptureMouse to block picking.
            
            bool imGuiWantsInput = ImGui.GetIO().WantCaptureKeyboard || ImGui.GetIO().WantCaptureMouse;

            if (window.MouseState.IsButtonPressed(MouseButton.Left) && !imGuiWantsInput)
            {
                 if (ManagerContext?.ActiveScene != null)
                 {
                     float width = window.ClientSize.X;
                     float height = window.ClientSize.Y;
                     var mouse = window.MouseState;
                     Vector2 mousePos = new Vector2(mouse.X, mouse.Y);
                     
                     // Raycast
                     Ray ray = Physics.ScreenPointToRay(mousePos, new Vector2(width, height), _camera.View, _camera.Projection);
                     GameObject? hit = ManagerContext.ActiveScene.Raycast(ray);
                     if (hit != null)
                     {
                         SelectedObject = hit;
                     }
                 }
            }

            // Gizmo
            if (SelectedObject != null && !imGuiWantsInput)
            {
                // Note: Gizmo might need "WantCaptureMouse" check effectively too, but it handles its own usually.
                // We pass !imGuiWantsInput implicitly by usually checking it before calling this logic or relying on Gizmo to be smart.
                // The original code passed IsButtonPressed(Left) only if !imGuiWantsInput for picking, 
                // but Gizmo updating allows dragging.
                if (!ImGui.GetIO().WantCaptureMouse) // Basic check
                     _gizmoController.Update(SelectedObject, _camera, window.MouseState, new Vector2(window.ClientSize.X, window.ClientSize.Y));
                
                _gizmoController.Render(SelectedObject, _camera);
            }
            
            // Camera Logic (moved from Toolbar)
            // Assuming Camera.Update needs window inputs
             // Calculate "Center" viewport approximately or just use full window?
             // Original used centerWidth etc. Now we just assume full window for camera control when not clicking UI.
             float centerWidth = window.ClientSize.X; 
             float centerHeight = window.ClientSize.Y;
             _camera.Update(window, (int)centerWidth, (int)centerHeight, !ImGui.GetIO().WantCaptureKeyboard);
        }

        private void RenderLayoutBounds()
        {
            // If we needed to calculate layout areas for docking manually, we would do it here.
            // Current modules use relative sizing based on window size inside their Render methods.
        }

        public void Dispose()
        {
            _gizmoController.Dispose();
        }
    }
}
