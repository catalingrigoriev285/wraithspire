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

        public Action? OnCreateCube { get; set; }
        public Action? OnCreateSphere { get; set; }
        public Action? OnCreateCapsule { get; set; }
        public Action? OnCreateCylinder { get; set; }
        public Action? OnCreatePlane { get; set; }
        public List<Primitive> SceneObjects { get; set; } = new List<Primitive>();

        public Matrix4 CameraView => _camera.View;
        public Matrix4 CameraProjection => _camera.Projection;

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
            if (ImGui.Begin("Toolbar", ImGuiWindowFlags.None))
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
            if (ImGui.Begin("Hierarchy", ImGuiWindowFlags.None))
            {
                ImGui.Text("Scene Hierarchy");
                ImGui.Separator();
                if (ImGui.TreeNodeEx("Root", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.PushID("Camera"); ImGui.BulletText("Camera"); ImGui.PopID();
                    ImGui.PushID("DirectionalLight"); ImGui.BulletText("Directional Light"); ImGui.PopID();
                    
                    // Display scene objects
                    for (int i = 0; i < SceneObjects.Count; i++)
                    {
                        var obj = SceneObjects[i];
                        ImGui.PushID(i);
                        bool isSelected = (_selectedObject == obj);
                        if (ImGui.Selectable(obj.Name, isSelected))
                        {
                            _selectedObject = obj;
                        }
                        ImGui.PopID();
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
                    if (ImGui.BeginMenu("Objects"))
                    {
                        if (ImGui.MenuItem("Cube"))
                        {
                            OnCreateCube?.Invoke();
                        }
                        if (ImGui.MenuItem("Sphere"))
                        {
                            OnCreateSphere?.Invoke();
                        }
                        if (ImGui.MenuItem("Capsule"))
                        {
                            OnCreateCapsule?.Invoke();
                        }
                        if (ImGui.MenuItem("Cylinder"))
                        {
                            OnCreateCylinder?.Invoke();
                        }
                        if (ImGui.MenuItem("Plane"))
                        {
                            OnCreatePlane?.Invoke();
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
            if (ImGui.Begin("Project", ImGuiWindowFlags.None))
            {
                ImGui.Text("Project Files");
                ImGui.Separator();
                if (ImGui.BeginTable("ProjectTable", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
                {
                    ImGui.TableSetupColumn("Assets");
                    ImGui.TableSetupColumn("Type");
                    ImGui.TableHeadersRow();

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0); ImGui.Text("scene.main");
                    ImGui.TableSetColumnIndex(1); ImGui.Text("Scene");

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0); ImGui.Text("textures/brick.png");
                    ImGui.TableSetColumnIndex(1); ImGui.Text("Texture");

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0); ImGui.Text("scripts/player.cs");
                    ImGui.TableSetColumnIndex(1); ImGui.Text("Script");

                    ImGui.EndTable();
                }
            }
            ImGui.End();

            // Inspector
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(leftWidth + centerWidth, topMargin), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(rightWidth, centerHeight), ImGuiCond.Always);
            if (ImGui.Begin("Inspector", ImGuiWindowFlags.None))
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
