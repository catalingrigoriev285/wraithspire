using System;
using System.Collections.Generic;
using ImGuiNET;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using wraithspire.engine.objects.primitives;

namespace wraithspire.engine
{
    internal sealed class EditorUI
    {
        private bool _isPlaying;
        private bool _isPaused;
        private Camera _camera = new Camera();
        private Primitive? _selectedObject = null;

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
                _camera.Update(window, (int)centerWidth, (int)(centerHeight - 100f));
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
                        var objects = activeScene.Objects;
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
                        if (ImGui.MenuItem("Capsule"))
                        {
                            activeScene?.CreateCapsule();
                        }
                        if (ImGui.MenuItem("Cylinder"))
                        {
                            activeScene?.CreateCylinder();
                        }
                        if (ImGui.MenuItem("Plane"))
                        {
                            activeScene?.CreatePlane();
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
                    
                    var pos = new System.Numerics.Vector3(_selectedObject.Position.X, _selectedObject.Position.Y, _selectedObject.Position.Z);
                    if (ImGui.DragFloat3("Position", ref pos, 0.1f))
                    {
                        _selectedObject.Position = new Vector3(pos.X, pos.Y, pos.Z);
                    }
                    
                    var rot = new System.Numerics.Vector3(_selectedObject.Rotation.X, _selectedObject.Rotation.Y, _selectedObject.Rotation.Z);
                    if (ImGui.DragFloat3("Rotation", ref rot, 1f))
                    {
                        _selectedObject.Rotation = new Vector3(rot.X, rot.Y, rot.Z);
                    }
                    
                    var scale = new System.Numerics.Vector3(_selectedObject.Scale.X, _selectedObject.Scale.Y, _selectedObject.Scale.Z);
                    if (ImGui.DragFloat3("Scale", ref scale, 0.01f))
                    {
                        _selectedObject.Scale = new Vector3(scale.X, scale.Y, scale.Z);
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
