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

namespace wraithspire.engine
{
    internal sealed class EditorUI : IDisposable
    {
        private bool _isPlaying;
        private bool _isPaused;
        private Camera _camera = new Camera();
        private GameObject? _selectedObject = null;

        private GizmoController _gizmoController = new GizmoController();

        public SceneManager? ManagerContext { get; set; }

        public Matrix4 CameraView => _camera.View;
        public Matrix4 CameraProjection => _camera.Projection;

        private string _newSceneName = "New Scene";

        public void Render(GameWindow window)
        {
            float width = window.ClientSize.X;
            float height = window.ClientSize.Y;

            float topMargin = 0f;
            float leftWidth = MathF.Round(width * 0.20f);
            float rightWidth = MathF.Round(width * 0.25f);
            float bottomHeight = MathF.Round(height * 0.30f);
            float centerWidth = width - leftWidth - rightWidth;
            float centerHeight = height - bottomHeight - topMargin;

            // Handle Object Picking (Left Ctrl + Click or just Click if not on UI/Gizmo)
            // Just basic click for now.
            // Check if mouse is within viewport area
            var mouse = window.MouseState;
            Vector2 mousePos = new Vector2(mouse.X, mouse.Y);
            
            bool isMouseInViewport = mousePos.X > leftWidth && mousePos.X < (leftWidth + centerWidth) &&
                                     mousePos.Y > topMargin && mousePos.Y < (topMargin + centerHeight);
            
            bool imGuiWantsInput = ImGui.GetIO().WantCaptureKeyboard || ImGui.GetIO().WantCaptureMouse;

            if (window.MouseState.IsButtonPressed(MouseButton.Left) && !imGuiWantsInput && isMouseInViewport)
            {
                // Only pick if not interacting with gizmo (GizmoController dragging logic is separate)
                // But we don't know if GizmoController consumed the click unless we query it or if we check intersection with gizmo first.
                // For simplicity: If Gizmo is hovered, don't pick.
                
                // We'd need to expose IsHovering from GizmoController.
                // Or just do picking. Selecting the same object again is fine.
                // But selecting another object behind the gizmo is annoying if we meant to grab the axis.
                
                // Let's defer to GizmoController first. But GizmoController is internal state.
                // Assuming Gizmo handles dragging in Update, but for initial click:
                // We can't easily check _gizmoController state here publicly.
                
                // Let's just implement picking. If user clicks axis, they might also "pick" the object behind or the same object.
                // If we pick a DIFFERENT object while clicking axis, that's bad.
                
                // Solution: Raycast scene.
                if (ManagerContext?.ActiveScene != null)
                {
                     Ray ray = Physics.ScreenPointToRay(mousePos, new Vector2(width, height), _camera.View, _camera.Projection);
                     GameObject? hit = ManagerContext.ActiveScene.Raycast(ray);
                     if (hit != null)
                     {
                         _selectedObject = hit;
                     }
                     else 
                     {
                         // Optional: Deselect if clicked empty space?
                         // _selectedObject = null; 
                     }
                }
            }

            // Update Gizmo
            // imGuiWantsInput was calculated above
            if (!imGuiWantsInput && _selectedObject != null)
            {
                if (!ImGui.GetIO().WantCaptureMouse)
                    _gizmoController.Update(_selectedObject, _camera, window.MouseState, new Vector2(width, height));
            }
            
            // Render Gizmo
            if (_selectedObject != null)
            {
                 _gizmoController.Render(_selectedObject, _camera);
            }

            // Toolbar
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(leftWidth, topMargin), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(centerWidth, 100f), ImGuiCond.Always);
            if (ImGui.Begin("Toolbar", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove))
            {
                if (ImGui.Button(_isPlaying ? "Stop" : "Play"))
                {
                    _isPlaying = !_isPlaying;
                    if (!_isPlaying) _isPaused = false;
                }
                ImGui.SameLine();
                if (ImGui.Button(_isPaused ? "Resume" : "Pause"))
                {
                    if (_isPlaying)
                        _isPaused = !_isPaused;
                }
                ImGui.SameLine();
                ImGui.Text(_isPlaying ? (_isPaused ? "Paused" : "Playing") : "Stopped");
                // Update camera controls using window input
                _camera.Update(window, (int)centerWidth, (int)(centerHeight - 100f), !ImGui.GetIO().WantCaptureKeyboard);
            }
            ImGui.End();

            // Hierarchy
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(0, topMargin), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(leftWidth, centerHeight), ImGuiCond.Always);
            if (ImGui.Begin("Hierarchy", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove))
            {
                var activeScene = ManagerContext?.ActiveScene;
                ImGui.Text(activeScene != null ? $"Hierarchy - {activeScene.Name}" : "Hierarchy");
                ImGui.Separator();
                if (ImGui.TreeNodeEx("Root", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.PushID("Camera"); ImGui.BulletText("Camera"); ImGui.PopID();
                    ImGui.PushID("DirectionalLight"); ImGui.BulletText("Directional Light"); ImGui.PopID();
                    
                    // Display scene objects
                    if (activeScene != null)
                    {
                        var objects = activeScene.GameObjects;
                        for (int i = 0; i < objects.Count; i++)
                        {
                            var obj = objects[i];
                            ImGui.PushID(i);
                            bool isSelected = (_selectedObject == obj);
                            if (ImGui.Selectable(obj.Name, isSelected))
                            {
                                _selectedObject = obj;
                            }
                            ImGui.PopID();
                        }
                    }
                    
                    ImGui.TreePop();
                }

                // Right-click context menu
                if (ImGui.IsWindowHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Right))
                {
                    ImGui.OpenPopup("HierarchyContext");
                }

                if (ImGui.BeginPopup("HierarchyContext"))
                {
                    ImGui.Text("Scenes");
                    ImGui.Separator();
                    
                    // Scene Creation
                    ImGui.InputText("##NewSceneName", ref _newSceneName, 32);
                    ImGui.SameLine();
                    if (ImGui.Button("Create"))
                    {
                        ManagerContext?.CreateScene(_newSceneName);
                    }

                    // Scene Selection
                    if (ImGui.BeginMenu("Switch To..."))
                    {
                        if (ManagerContext != null)
                        {
                            foreach (var sceneName in ManagerContext.Scenes.Keys)
                            {
                                bool isCurrent = ManagerContext.ActiveScene?.Name == sceneName;
                                if (ImGui.MenuItem(sceneName, "", isCurrent))
                                {
                                    ManagerContext.LoadScene(sceneName);
                                    _selectedObject = null;
                                }
                            }
                        }
                        ImGui.EndMenu();
                    }

                    ImGui.Separator();
                    ImGui.Text("Objects");
                    if (ImGui.BeginMenu("Create Object"))
                    {
                        if (ImGui.MenuItem("Cube"))
                        {
                            activeScene?.CreateCube();
                        }
                        if (ImGui.MenuItem("Sphere"))
                        {
                            activeScene?.CreateSphere();
                        }
                        if (ImGui.MenuItem("Light"))
                        {
                            activeScene?.CreateLight();
                        }
                        ImGui.EndMenu();
                    }
                    ImGui.EndPopup();
                }
            }
            ImGui.End();

            // Project
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(0, topMargin + centerHeight), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(width, bottomHeight), ImGuiCond.Always);
            if (ImGui.Begin("Project", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove))
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
                        ManagerContext?.CreateScene(_newSceneName);
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

                    if (ManagerContext != null)
                    {
                        foreach (var sceneName in ManagerContext.Scenes.Keys)
                        {
                            ImGui.PushID(sceneName);
                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);
                            
                            bool isSelected = ManagerContext.ActiveScene?.Name == sceneName;
                            if (ImGui.Selectable(sceneName, isSelected, ImGuiSelectableFlags.SpanAllColumns))
                            {
                                // Single click selects? Or maybe just highlights.
                            }
                            if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                            {
                                ManagerContext.LoadScene(sceneName);
                                _selectedObject = null; // Deselect object on scene switch
                            }
                            
                            // Context menu for deletion
                            if (ImGui.BeginPopupContextItem())
                            {
                                if (ImGui.MenuItem("Load"))
                                {
                                    ManagerContext.LoadScene(sceneName);
                                    _selectedObject = null;
                                }
                                if (ImGui.MenuItem("Delete"))
                                {
                                    ManagerContext.DeleteScene(sceneName);
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

            // Inspector
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(leftWidth + centerWidth, topMargin), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(rightWidth, centerHeight), ImGuiCond.Always);
            if (ImGui.Begin("Inspector", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove))
            {
                ImGui.Text("Inspector");
                ImGui.Separator();
                
                if (_selectedObject != null)
                {
                    ImGui.Text($"Selected: {_selectedObject.Name}");
                    
                    // Access field directly now that it is public
                    ImGui.Checkbox("Active", ref _selectedObject.IsActive);

                    var transform = _selectedObject.Transform;
                    
                    var pos = new System.Numerics.Vector3(transform.Position.X, transform.Position.Y, transform.Position.Z);
                    if (ImGui.DragFloat3("Position", ref pos, 0.1f))
                    {
                        transform.Position = new Vector3(pos.X, pos.Y, pos.Z);
                    }
                    
                    var rot = new System.Numerics.Vector3(transform.Rotation.X, transform.Rotation.Y, transform.Rotation.Z);
                    if (ImGui.DragFloat3("Rotation", ref rot, 1f))
                    {
                        transform.Rotation = new Vector3(rot.X, rot.Y, rot.Z);
                    }
                    
                    var scale = new System.Numerics.Vector3(transform.Scale.X, transform.Scale.Y, transform.Scale.Z);
                    if (ImGui.DragFloat3("Scale", ref scale, 0.01f))
                    {
                        transform.Scale = new Vector3(scale.X, scale.Y, scale.Z);
                    }
                    
                    // Show Components
                    ImGui.Separator();
                    ImGui.Text("Components");
                    
                    var renderer = _selectedObject.GetComponent<MeshRenderer>();
                    if (renderer != null && renderer.Material != null)
                    {
                        if (ImGui.TreeNode("Mesh Renderer"))
                        {
                            var color = new System.Numerics.Vector3(renderer.Material.Color.X, renderer.Material.Color.Y, renderer.Material.Color.Z);
                            if (ImGui.ColorEdit3("Color", ref color))
                            {
                                renderer.Material.Color = new Vector3(color.X, color.Y, color.Z);
                            }
                            ImGui.TreePop();
                        }
                    }

                    var lightComp = _selectedObject.GetComponent<LightComponent>();
                    if (lightComp != null)
                    {
                         if (ImGui.TreeNode("Light Component"))
                         {
                             var color = new System.Numerics.Vector3(lightComp.Color.X, lightComp.Color.Y, lightComp.Color.Z);
                             if (ImGui.ColorEdit3("Color", ref color))
                             {
                                 lightComp.Color = new Vector3(color.X, color.Y, color.Z);
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

        public void Dispose()
        {
            _gizmoController.Dispose();
        }
    }
}
