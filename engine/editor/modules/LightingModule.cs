using ImGuiNET;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using System.Numerics;
using wraithspire.engine.components;

namespace wraithspire.engine.editor.modules
{
    internal class LightingModule : EditorModule
    {
        public override string Name => "Lighting";
        public override string Description => "Manages lighting in the scene. Disabling this module turns off all lighting calculations.";
        public override string Category => "Rendering";
        public override string Version => "1.0.0";

        // Settings
        public bool ShowHelpers { get; set; } = true;
        
        public override bool IsVisible { get; set; } = true;
        
        public override void OnEnable()
        {
            if (_editor?.ManagerContext != null)
            {
               GlobalSettings.IsLightingEnabled = true;
            }
        }
        
        // Ensure constructor or initializer sets IsEnabled = false if we want it disabled by default?
        // EditorModule base class sets IsEnabled = true by default.
        // We need to override it or set it in constructor.
        public LightingModule()
        {
            IsEnabled = false;
        }

        public override void OnDisable()
        {
            GlobalSettings.IsLightingEnabled = false;
        }

        public override void Render(GameWindow window)
        {
             // This module doesn't have a specific window to render on screen all the time.
             // It just provides functionality.
             // BUT, we might want a panel for Global Light Settings if enabled.
        }

        public override void DrawSettings()
        {
            ImGui.Text("Lighting Settings");
            bool showHelpers = ShowHelpers;
            if (ImGui.Checkbox("Show Light Helpers", ref showHelpers))
            {
                ShowHelpers = showHelpers;
            }
        }
    }
    
    // Quick hack for global setting access across modules/engine without deep dependency injection Refactor
    internal static class GlobalSettings
    {
        public static bool IsLightingEnabled { get; set; } = false;
    }
}
