using ImGuiNET;
using OpenTK.Windowing.Desktop;
using System.Numerics;

namespace wraithspire.engine.editor.modules
{
    internal class ProjectModule : EditorModule
    {
        public override string Name => "Project";
        public override string Description => "Manage project scenes and assets.";
        public override string Category => "Core";
        private string _newSceneName = "New Scene";

        public override void Render(GameWindow window)
        {
             if (!IsVisible || _manager == null || _editor == null) return;

            float width = window.ClientSize.X;
            float height = window.ClientSize.Y;
            float bottomHeight = MathF.Round(height * 0.30f);
            // float topMargin = height - bottomHeight; 

            ImGui.SetNextWindowPos(new Vector2(0, height - bottomHeight), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new Vector2(width, bottomHeight), ImGuiCond.Always);

            if (ImGui.Begin(Name, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove))
            {
                ImGui.Text("Project - Scenes");
                ImGui.SameLine();
                if (ImGui.Button("+"))
                {
                    ImGui.OpenPopup("CreateScenePopup");
                }

                if (ImGui.BeginPopup("CreateScenePopup"))
                {
                    ImGui.InputText("Name", ref _newSceneName, 32);
                    if (ImGui.Button("Create"))
                    {
                        _manager?.CreateScene(_newSceneName);
                        ImGui.CloseCurrentPopup();
                    }
                    ImGui.EndPopup();
                }

                ImGui.Separator();
                if (ImGui.BeginTable("ProjectTable", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
                {
                    ImGui.TableSetupColumn("Name");
                    ImGui.TableSetupColumn("Type");
                    ImGui.TableHeadersRow();

                    if (_manager != null)
                    {
                        foreach (var sceneName in _manager.Scenes.Keys)
                        {
                            ImGui.PushID(sceneName);
                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);
                            
                            bool isSelected = _manager.ActiveScene?.Name == sceneName;
                            if (ImGui.Selectable(sceneName, isSelected, ImGuiSelectableFlags.SpanAllColumns))
                            {
                                // Selection logic
                            }
                            if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                            {
                                _manager.LoadScene(sceneName);
                                _editor.SelectedObject = null;
                            }
                            
                            if (ImGui.BeginPopupContextItem())
                            {
                                if (ImGui.MenuItem("Load"))
                                {
                                    _manager.LoadScene(sceneName);
                                    _editor.SelectedObject = null;
                                }
                                if (ImGui.MenuItem("Delete"))
                                {
                                    _manager.DeleteScene(sceneName);
                                }
                                ImGui.EndPopup();
                            }

                            ImGui.TableSetColumnIndex(1); ImGui.Text("Scene");
                            ImGui.PopID();
                        }
                    }

                    ImGui.EndTable();
                }
            }
            ImGui.End();
        }
    }
}
