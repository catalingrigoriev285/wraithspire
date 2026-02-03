using ImGuiNET;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using System.Numerics;
using System.Collections.Generic;

namespace wraithspire.engine.editor.modules
{
    internal class HierarchyModule : EditorModule
    {
        public override string Name => "Hierarchy";
        private string _newSceneName = "New Scene";

        public override void Render(GameWindow window)
        {
            if (!IsVisible || _manager == null || _editor == null) return;

            float width = window.ClientSize.X;
            float height = window.ClientSize.Y;
            float leftWidth = MathF.Round(width * 0.20f);
            float activeHeight = height - _editor.MainMenuBarHeight;
            float centerHeight = activeHeight - MathF.Round(height * 0.30f); // Approximate for now

            ImGui.SetNextWindowPos(new System.Numerics.Vector2(0, _editor.MainMenuBarHeight), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(leftWidth, centerHeight), ImGuiCond.Always);

            if (ImGui.Begin(Name, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove))
            {
                var activeScene = _manager?.ActiveScene;
                ImGui.Text(activeScene != null ? $"Hierarchy - {activeScene.Name}" : "Hierarchy");
                ImGui.Separator();
                
                if (ImGui.TreeNodeEx("Root", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.PushID("Camera"); ImGui.BulletText("Camera"); ImGui.PopID();
                    ImGui.PushID("DirectionalLight"); ImGui.BulletText("Directional Light"); ImGui.PopID();
                    
                    if (activeScene != null)
                    {
                        var objects = activeScene.GameObjects;
                        for (int i = 0; i < objects.Count; i++)
                        {
                            var obj = objects[i];
                            ImGui.PushID(i);
                            bool isSelected = (_editor.SelectedObject == obj);
                            if (ImGui.Selectable(obj.Name, isSelected))
                            {
                                _editor.SelectedObject = obj;
                            }
                            ImGui.PopID();
                        }
                    }
                    ImGui.TreePop();
                }

                ShowContextMenu(activeScene);
            }
            ImGui.End();
        }

        private void ShowContextMenu(Scene activeScene)
        {
             if (ImGui.IsWindowHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Right))
             {
                 ImGui.OpenPopup("HierarchyContext");
             }

             if (ImGui.BeginPopup("HierarchyContext"))
             {
                 ImGui.Text("Scenes");
                 ImGui.Separator();
                 
                 ImGui.InputText("##NewSceneName", ref _newSceneName, 32);
                 ImGui.SameLine();
                 if (ImGui.Button("Create"))
                 {
                     _manager?.CreateScene(_newSceneName);
                 }

                 if (ImGui.BeginMenu("Switch To..."))
                 {
                     if (_manager != null)
                     {
                         foreach (var sceneName in _manager.Scenes.Keys)
                         {
                             bool isCurrent = _manager.ActiveScene?.Name == sceneName;
                             if (ImGui.MenuItem(sceneName, "", isCurrent))
                             {
                                 _manager.LoadScene(sceneName);
                                 _editor.SelectedObject = null;
                             }
                         }
                     }
                     ImGui.EndMenu();
                 }

                 ImGui.Separator();
                 ImGui.Text("Objects");
                 if (ImGui.BeginMenu("Create Object"))
                 {
                     if (ImGui.MenuItem("Cube")) activeScene?.CreateCube();
                     if (ImGui.MenuItem("Sphere")) activeScene?.CreateSphere();
                     if (ImGui.MenuItem("Light")) activeScene?.CreateLight();
                     ImGui.EndMenu();
                 }
                 ImGui.EndPopup();
             }
        }
    }
}
