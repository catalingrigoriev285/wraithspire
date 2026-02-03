using System;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using wraithspire.engine.physics;

namespace wraithspire.engine.editor
{
    internal class GizmoController : IDisposable
    {
        private Gizmo _ui = new Gizmo();
        
        // 0=X, 1=Y, 2=Z, -1=None
        private int _hoverAxis = -1;
        private int _dragAxis = -1;
        
        private Vector3 _dragOffset; // Offset from object center to hit point at start of drag
        private Vector3 _startObjPos;

        public void Update(GameObject? selected, Camera camera, MouseState mouse, Vector2 windowSize)
        {
            if (selected == null)
            {
                _hoverAxis = -1;
                _dragAxis = -1;
                return;
            }

            Vector2 mousePos = new Vector2(mouse.X, mouse.Y);
            Ray ray = Physics.ScreenPointToRay(mousePos, windowSize, camera.View, camera.Projection);
            Vector3 pos = selected.Transform.Position;

            float length = 2.0f;
            float thickness = 0.2f;

            // --- Hover Logic ---
            if (_dragAxis == -1)
            {
                Ray localRay = new Ray(ray.Origin - pos, ray.Direction);
                float? hitX = Physics.RayIntersectsBox(localRay, new Vector3(0, -thickness/2, -thickness/2), new Vector3(length, thickness/2, thickness/2));
                float? hitY = Physics.RayIntersectsBox(localRay, new Vector3(-thickness/2, 0, -thickness/2), new Vector3(thickness/2, length, thickness/2));
                float? hitZ = Physics.RayIntersectsBox(localRay, new Vector3(-thickness/2, -thickness/2, 0), new Vector3(thickness/2, thickness/2, length));

                _hoverAxis = -1;
                float minDist = float.MaxValue;

                if (hitX.HasValue && hitX.Value < minDist) { minDist = hitX.Value; _hoverAxis = 0; }
                if (hitY.HasValue && hitY.Value < minDist) { minDist = hitY.Value; _hoverAxis = 1; }
                if (hitZ.HasValue && hitZ.Value < minDist) { minDist = hitZ.Value; _hoverAxis = 2; }
            }

            // --- Drag Logic ---
            if (mouse.IsButtonDown(MouseButton.Left))
            {
                if (_dragAxis == -1 && _hoverAxis != -1)
                {
                    // Start Drag
                    _dragAxis = _hoverAxis;
                    _startObjPos = selected.Transform.Position;
                    
                    // Calculate initial hit point on the axis to maintain offset
                    // We need a plane to raycast against.
                    Vector3 axisDir = GetAxisVector(_dragAxis);
                    Vector3 planeNormal = GetBestPlaneNormal(_dragAxis, camera.View); // Plane normal
                    
                    // Ray intersects Plane?
                    Vector3? hit = IntersectRayPlane(ray, pos, planeNormal);
                    if (hit.HasValue)
                    {
                        // Project hit onto axis line
                         Vector3 projected = ProjectPointOnLine(hit.Value, pos, axisDir);
                         _dragOffset = projected - pos;
                    }
                    else
                    {
                        _dragOffset = Vector3.Zero;
                    }
                }
                
                if (_dragAxis != -1)
                {
                    // Continue Drag
                    Vector3 axisDir = GetAxisVector(_dragAxis);
                    Vector3 planeNormal = GetBestPlaneNormal(_dragAxis, camera.View);

                    Vector3? hit = IntersectRayPlane(ray, _startObjPos, planeNormal); // Plane at original object position
                    if (hit.HasValue)
                    {
                        // Project hit onto axis line passing through original position
                        Vector3 projected = ProjectPointOnLine(hit.Value, _startObjPos, axisDir);
                        
                        // New position is projected point minus original offset
                        // Wait, we want to move the object so that (NewPos + Offset) = ProjectedPoint
                        // So NewPos = ProjectedPoint - Offset
                        
                        Vector3 newPos = projected - _dragOffset;
                        
                        // Constrain: We only want to update the component matching the axis
                        // Actually, ProjectPointOnLine already gives us a point on the infinite axis line.
                        // So if we drag X, newPos will vary only in X relative to _startObjPos.
                        // But _startObjPos is fixed. Use that.
                        
                        if (_dragAxis == 0) selected.Transform.Position = new Vector3(newPos.X, _startObjPos.Y, _startObjPos.Z);
                        if (_dragAxis == 1) selected.Transform.Position = new Vector3(_startObjPos.X, newPos.Y, _startObjPos.Z);
                        if (_dragAxis == 2) selected.Transform.Position = new Vector3(_startObjPos.X, _startObjPos.Y, newPos.Z);
                    }
                }
            }
            else
            {
                _dragAxis = -1;
            }
        }

        private Vector3 GetAxisVector(int axis)
        {
            if (axis == 0) return Vector3.UnitX;
            if (axis == 1) return Vector3.UnitY;
            return Vector3.UnitZ;
        }

        private Vector3 GetBestPlaneNormal(int axis, Matrix4 view)
        {
            // View direction is -Z in view space. In world space, it's roughly the inverse of view matrix forward?
            // Actually, we just need the "most perpendicular" plane.
            // If dragging X (1,0,0), we can use XY (Normal Z) or XZ (Normal Y).
            // Compare View Forward with Y and Z.
            
            // Extract camera forward from View matrix.
            // View matrix = Rotation * Translation. Inverse Rotation gives world axes.
            // Forward is row 2 (or col 2 depending on notation).
            // Let's rely on View Direction.
            
            // Simplification: Use generic logic.
            // Best plane for Axis A is formed by A and B, where B is the axis most perpendicular to ViewDir.
            
            // Or simpler: Plane Normal should be the axis (Y or Z for X-drag) that is closest to "facing" the camera.
            // Maximizing Dot(Normal, ViewDir).
            
            // Actually, we want the plane to be as "flat" to the camera as possible to maximize ray hit precision.
            // So plane normal should be parallel to view direction.
            // If dragging X, Plane Normal must be perp to X (so Y or Z).
            // Choose Normal = Y if |ViewDir.Y| > |ViewDir.Z| ?
            
            // Let's retrieve camera View Direction (LookAt direction).
            // Since we passed View Matrix... 
            // Inverted View Matrix Column 2 is -Forward.
            Matrix4 invView = Matrix4.Invert(view);
            Vector3 camDir = -new Vector3(invView.Row2); // Forward
            
            if (axis == 0) // X Axis -> Plane normals Y or Z
               return MathF.Abs(Vector3.Dot(camDir, Vector3.UnitY)) > MathF.Abs(Vector3.Dot(camDir, Vector3.UnitZ)) ? Vector3.UnitY : Vector3.UnitZ;
            
            if (axis == 1) // Y Axis -> Plane normals X or Z
               return MathF.Abs(Vector3.Dot(camDir, Vector3.UnitX)) > MathF.Abs(Vector3.Dot(camDir, Vector3.UnitZ)) ? Vector3.UnitX : Vector3.UnitZ;
            
            // Z Axis -> Plane normals X or Y
            return MathF.Abs(Vector3.Dot(camDir, Vector3.UnitX)) > MathF.Abs(Vector3.Dot(camDir, Vector3.UnitY)) ? Vector3.UnitX : Vector3.UnitY;
        }

        private Vector3? IntersectRayPlane(Ray ray, Vector3 planePoint, Vector3 planeNormal)
        {
            float denom = Vector3.Dot(planeNormal, ray.Direction);
            if (MathF.Abs(denom) < 1e-6f) return null; // Parallel

            float t = Vector3.Dot(planePoint - ray.Origin, planeNormal) / denom;
            if (t < 0) return null; // Behind

            return ray.Origin + ray.Direction * t;
        }

        private Vector3 ProjectPointOnLine(Vector3 point, Vector3 lineOrigin, Vector3 lineDir)
        {
            float t = Vector3.Dot(point - lineOrigin, lineDir); // Assuming lineDir is normalized
            return lineOrigin + lineDir * t;
        }

        public void Render(GameObject? selected, Camera camera)
        {
            if (selected == null) return;
            _ui.Render(selected.Transform.Position, camera.View, camera.Projection, (_dragAxis != -1) ? _dragAxis : _hoverAxis);
        }

        public void Dispose()
        {
            _ui.Dispose();
        }
    }
}
