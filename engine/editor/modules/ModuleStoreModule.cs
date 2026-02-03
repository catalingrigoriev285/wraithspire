using ImGuiNET;
using OpenTK.Windowing.Desktop;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace wraithspire.engine.editor.modules
{
    internal class ModuleStoreModule : EditorModule
    {
        public override string Name => "Module Store";
        public override string Description => "Manage and configure editor modules.";
        public override string Category => "Core";
        
        private string _selectedCategory = "All";
        private IEditorModule? _selectedModule = null;

        public override bool IsVisible { get; set; } = false;

        public override void Render(GameWindow window)
        {
            if (!IsVisible || _editor == null) return;

            // Centered window for the store
            float width = window.ClientSize.X;
            float height = window.ClientSize.Y;
            float margin = 50f;
            
            ImGui.SetNextWindowPos(new Vector2(margin, margin + _editor.MainMenuBarHeight), ImGuiCond.FirstUseEver); // Allow moving
            ImGui.SetNextWindowSize(new Vector2(width - margin * 2, height - margin * 2 - _editor.MainMenuBarHeight), ImGuiCond.FirstUseEver);
            
            bool isVisible = IsVisible;
            if (ImGui.Begin("Module Store", ref isVisible, ImGuiWindowFlags.NoCollapse))
            {
                IsVisible = isVisible;
                // Layout: Left sidebar (Categories), Center (Modules List), Right (Details)
                
                float sidebarWidth = 150f;
                float detailsWidth = 250f;
                float contentWidth = ImGui.GetContentRegionAvail().X - sidebarWidth - detailsWidth;
                float contentHeight = ImGui.GetContentRegionAvail().Y;

                // 1. Sidebar - Categories
                ImGui.BeginChild("Categories", new Vector2(sidebarWidth, contentHeight), ImGuiChildFlags.Borders, ImGuiWindowFlags.None);
                ImGui.TextDisabled("CATEGORIES");
                ImGui.Separator();
                
                if (ImGui.Selectable("All", _selectedCategory == "All")) _selectedCategory = "All";
                
                var categories = _editor.Modules.Select(m => m.Category).Distinct().OrderBy(c => c);
                foreach (var cat in categories)
                {
                     if (ImGui.Selectable(cat, _selectedCategory == cat)) _selectedCategory = cat;
                }
                ImGui.EndChild();

                ImGui.SameLine();

                // 2. Center - Module Grid/List
                ImGui.BeginChild("Modules", new Vector2(contentWidth, contentHeight), ImGuiChildFlags.Borders, ImGuiWindowFlags.None);
                ImGui.TextDisabled("MODULES");
                ImGui.Separator();
                
                var filteredModules = _editor.Modules.Where(m => _selectedCategory == "All" || m.Category == _selectedCategory);
                
                foreach (var module in filteredModules)
                {
                    bool isSelected = _selectedModule == module;
                    string status = module.IsEnabled ? "(Active)" : "(Disabled)";
                    if (ImGui.Selectable($"{module.Name} {status}", isSelected))
                    {
                        _selectedModule = module;
                    }
                }
                ImGui.EndChild();
                
                ImGui.SameLine();
                
                // 3. Right - Details
                ImGui.BeginChild("Details", new Vector2(detailsWidth, contentHeight), ImGuiChildFlags.Borders, ImGuiWindowFlags.None);
                if (_selectedModule != null)
                {
                    ImGui.TextColored(new Vector4(0.4f, 0.8f, 1f, 1f), _selectedModule.Name);
                    ImGui.TextDisabled($"Version: {_selectedModule.Version}");
                    ImGui.Separator();
                    
                    ImGui.TextWrapped(_selectedModule.Description);
                    
                    ImGui.Dummy(new Vector2(0, 10));
                    
                    bool enabled = _selectedModule.IsEnabled;
                    if (ImGui.Checkbox("Enable Module", ref enabled))
                    {
                        _selectedModule.IsEnabled = enabled;
                    }
                    
                    ImGui.Dummy(new Vector2(0, 20));
                    ImGui.TextDisabled("SETTINGS");
                    ImGui.Separator();
                    _selectedModule.DrawSettings();
                }
                else
                {
                    ImGui.TextDisabled("Select a module to view details.");
                }
                ImGui.EndChild();

            }
            ImGui.End();
        }
    }
}
