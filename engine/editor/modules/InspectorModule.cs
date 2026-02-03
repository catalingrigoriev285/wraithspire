using ImGuiNET;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using System.Numerics;
using wraithspire.engine.components;
using wraithspire.engine.rendering;

namespace wraithspire.engine.editor.modules
{
    internal class InspectorModule : EditorModule
    {
        public override string Name => "Inspector";

        public override void Render(GameWindow window)
        {
            if (!IsVisible || _editor == null) return;

            float width = window.ClientSize.X;
            float height = window.ClientSize.Y;
            float rightWidth = MathF.Round(width * 0.25f);
            float leftWidth = MathF.Round(width * 0.20f);
            float centerWidth = width - leftWidth - rightWidth;
            float activeHeight = height - _editor.MainMenuBarHeight;
            float centerHeight = activeHeight - MathF.Round(height * 0.30f); // Match Hierarchy height

            ImGui.SetNextWindowPos(new System.Numerics.Vector2(leftWidth + centerWidth, _editor.MainMenuBarHeight), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(rightWidth, centerHeight), ImGuiCond.Always);

            if (ImGui.Begin(Name, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove))
            {
                ImGui.Text("Inspector");
                ImGui.Separator();
                
                var selectedObject = _editor.SelectedObject;

                if (selectedObject != null)
                {
                    ImGui.Text($"Selected: {selectedObject.Name}");
                    
                    bool isActive = selectedObject.IsActive;
                    if (ImGui.Checkbox("Active", ref isActive))
                    {
                         selectedObject.IsActive = isActive;
                    }

                    var transform = selectedObject.Transform;
                    
                    var pos = new System.Numerics.Vector3(transform.Position.X, transform.Position.Y, transform.Position.Z);
                    if (ImGui.DragFloat3("Position", ref pos, 0.1f))
                    {
                        transform.Position = new OpenTK.Mathematics.Vector3(pos.X, pos.Y, pos.Z);
                    }
                    
                    var rot = new System.Numerics.Vector3(transform.Rotation.X, transform.Rotation.Y, transform.Rotation.Z);
                    if (ImGui.DragFloat3("Rotation", ref rot, 1f))
                    {
                        transform.Rotation = new OpenTK.Mathematics.Vector3(rot.X, rot.Y, rot.Z);
                    }
                    
                    var scale = new System.Numerics.Vector3(transform.Scale.X, transform.Scale.Y, transform.Scale.Z);
                    if (ImGui.DragFloat3("Scale", ref scale, 0.01f))
                    {
                        transform.Scale = new OpenTK.Mathematics.Vector3(scale.X, scale.Y, scale.Z);
                    }
                    
                    // Show Components
                    ImGui.Separator();
                    ImGui.Text("Components");
                    
                    var renderer = selectedObject.GetComponent<MeshRenderer>();
                    if (renderer != null && renderer.Material != null)
                    {
                        if (ImGui.TreeNode("Mesh Renderer"))
                        {
                            var color = new System.Numerics.Vector3(renderer.Material.Color.X, renderer.Material.Color.Y, renderer.Material.Color.Z);
                            if (ImGui.ColorEdit3("Color", ref color))
                            {
                                renderer.Material.Color = new OpenTK.Mathematics.Vector3(color.X, color.Y, color.Z);
                            }
                            ImGui.TreePop();
                        }
                    }

                    var lightComp = selectedObject.GetComponent<LightComponent>();
                    if (lightComp != null)
                    {
                         if (ImGui.TreeNode("Light Component"))
                         {
                             var color = new System.Numerics.Vector3(lightComp.Color.X, lightComp.Color.Y, lightComp.Color.Z);
                             if (ImGui.ColorEdit3("Color", ref color))
                             {
                                 lightComp.Color = new OpenTK.Mathematics.Vector3(color.X, color.Y, color.Z);
                             }
                             
                             float intensity = lightComp.Intensity;
                             if (ImGui.DragFloat("Intensity", ref intensity, 0.1f, 0f, 100f))
                             {
                                 lightComp.Intensity = intensity;
                             }
                             ImGui.TreePop();
                         }
                    }
                }
                else
                {
                    ImGui.Text("No object selected");
                }
            }
            ImGui.End();
        }
    }
}
