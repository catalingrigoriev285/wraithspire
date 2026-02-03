using ImGuiNET;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Numerics;

namespace wraithspire.engine.editor.modules
{
    internal class ToolbarModule : EditorModule
    {
        public override string Name => "Toolbar";
        
        // These control the position relative to the main layout
        // For now, hardcoded similar to original EditorUI, but ideally dynamic.
        
        public override void Render(GameWindow window)
        {
            if (!IsVisible || _editor == null) return;
            
            // Access layout parameters from EditorUI if possible, or just calculate locally for now.
            // Original code:
            float width = window.ClientSize.X;
            float height = window.ClientSize.Y;
            float leftWidth = MathF.Round(width * 0.20f);
            float topMargin = _editor.MainMenuBarHeight;
            float centerWidth = width - MathF.Round(width * 0.20f) - MathF.Round(width * 0.25f);
            
            ImGui.SetNextWindowPos(new Vector2(leftWidth, topMargin), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new Vector2(centerWidth, 100f), ImGuiCond.Always);
            
            if (ImGui.Begin(Name, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar)) // NoTitleBar for toolbar look? Original had title.
            {
                // Play/Pause controls
                bool isPlaying = _editor.IsPlaying;
                bool isPaused = _editor.IsPaused;

                if (ImGui.Button(isPlaying ? "Stop" : "Play"))
                {
                    _editor.IsPlaying = !isPlaying;
                    if (!_editor.IsPlaying) _editor.IsPaused = false;
                }
                ImGui.SameLine();
                if (ImGui.Button(isPaused ? "Resume" : "Pause"))
                {
                    if (isPlaying)
                        _editor.IsPaused = !isPaused;
                }
                ImGui.SameLine();
                ImGui.Text(isPlaying ? (isPaused ? "Paused" : "Playing") : "Stopped");
                
                // Camera controls update is handled in EditorUI Logic usually, but here we can just display info?
                // The original code called _camera.Update() inside Render loop which is bad practice but it was there.
                // We should move _camera.Update() to EditorUI.Render() or Update() and keep this just for UI.
            }
            ImGui.End();
        }
    }
}
