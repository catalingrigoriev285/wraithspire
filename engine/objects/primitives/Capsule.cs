using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace wraithspire.engine.objects.primitives
{
    internal sealed class Capsule : Primitive
    {
        private readonly int _segments;
        private readonly int _hemisphereRings;

        public Capsule(string name = "Capsule", int segments = 18, int hemisphereRings = 8)
        {
            Name = name;
            Color = new Vector3(0.8f, 0.8f, 0.25f);
            _segments = Math.Max(3, segments);
            _hemisphereRings = Math.Max(2, hemisphereRings);
        }

        public override void Initialize()
        {
            CreateBasicShader();
            BuildMesh();
        }

        private void BuildMesh()
        {
            // Simple capsule built from a cylinder + two hemispheres.
            float radius = 0.5f;
            float cylinderHalfHeight = 0.35f;

            var verts = new System.Collections.Generic.List<float>();
            var inds = new System.Collections.Generic.List<int>();

            // Cylinder rings (top & bottom)
            for (int i = 0; i < _segments; i++)
            {
                float a = 2f * MathF.PI * i / _segments;
                float x = MathF.Cos(a) * radius;
                float z = MathF.Sin(a) * radius;

                verts.Add(x); verts.Add(cylinderHalfHeight); verts.Add(z);
                verts.Add(x); verts.Add(-cylinderHalfHeight); verts.Add(z);
            }

            // Cylinder sides
            for (int i = 0; i < _segments; i++)
            {
                int i0 = i * 2;
                int i1 = i0 + 1;
                int j0 = ((i + 1) % _segments) * 2;
                int j1 = j0 + 1;

                inds.Add(i0);
                inds.Add(i1);
                inds.Add(j0);

                inds.Add(j0);
                inds.Add(i1);
                inds.Add(j1);
            }

            // Hemispheres mesh shares the same radius. We'll generate vertices for top and bottom.
            // Top hemisphere: phi from 0..PI/2 (0 at +Y pole)
            int topStart = verts.Count / 3;
            for (int r = 0; r <= _hemisphereRings; r++)
            {
                float v = r / (float)_hemisphereRings;
                float phi = (MathF.PI * 0.5f) * v;
                float y = MathF.Cos(phi) * radius;
                float sinPhi = MathF.Sin(phi) * radius;

                for (int s = 0; s <= _segments; s++)
                {
                    float u = s / (float)_segments;
                    float theta = 2f * MathF.PI * u;
                    float x = MathF.Cos(theta) * sinPhi;
                    float z = MathF.Sin(theta) * sinPhi;

                    verts.Add(x);
                    verts.Add(cylinderHalfHeight + y);
                    verts.Add(z);
                }
            }

            int topStride = _segments + 1;
            for (int r = 0; r < _hemisphereRings; r++)
            {
                for (int s = 0; s < _segments; s++)
                {
                    int i0 = topStart + r * topStride + s;
                    int i1 = i0 + 1;
                    int i2 = i0 + topStride;
                    int i3 = i2 + 1;

                    inds.Add(i0);
                    inds.Add(i2);
                    inds.Add(i1);

                    inds.Add(i1);
                    inds.Add(i2);
                    inds.Add(i3);
                }
            }

            // Bottom hemisphere: phi from PI/2..PI (PI at -Y pole)
            int bottomStart = verts.Count / 3;
            for (int r = 0; r <= _hemisphereRings; r++)
            {
                float v = r / (float)_hemisphereRings;
                float phi = (MathF.PI * 0.5f) + (MathF.PI * 0.5f) * v;
                float y = MathF.Cos(phi) * radius;
                float sinPhi = MathF.Sin(phi) * radius;

                for (int s = 0; s <= _segments; s++)
                {
                    float u = s / (float)_segments;
                    float theta = 2f * MathF.PI * u;
                    float x = MathF.Cos(theta) * sinPhi;
                    float z = MathF.Sin(theta) * sinPhi;

                    verts.Add(x);
                    verts.Add(-cylinderHalfHeight + y);
                    verts.Add(z);
                }
            }

            int bottomStride = _segments + 1;
            for (int r = 0; r < _hemisphereRings; r++)
            {
                for (int s = 0; s < _segments; s++)
                {
                    int i0 = bottomStart + r * bottomStride + s;
                    int i1 = i0 + 1;
                    int i2 = i0 + bottomStride;
                    int i3 = i2 + 1;

                    inds.Add(i0);
                    inds.Add(i2);
                    inds.Add(i1);

                    inds.Add(i1);
                    inds.Add(i2);
                    inds.Add(i3);
                }
            }

            float[] vertices = verts.ToArray();
            int[] indices = inds.ToArray();

            _indexCount = indices.Length;

            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            _ebo = GL.GenBuffer();

            GL.BindVertexArray(_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(int), indices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            GL.BindVertexArray(0);
        }

        public override void Render(Matrix4 proj, Matrix4 view)
        {
            if (_vao == 0) return;

            GL.UseProgram(_shader);

            var model = GetModelMatrix();
            GL.UniformMatrix4(_projLoc, false, ref proj);
            GL.UniformMatrix4(_viewLoc, false, ref view);
            GL.UniformMatrix4(_modelLoc, false, ref model);
            GL.Uniform3(_colorLoc, Color);

            GL.BindVertexArray(_vao);
            GL.DrawElements(PrimitiveType.Triangles, _indexCount, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }

        public override void Dispose()
        {
            if (_vao != 0) GL.DeleteVertexArray(_vao);
            if (_vbo != 0) GL.DeleteBuffer(_vbo);
            if (_ebo != 0) GL.DeleteBuffer(_ebo);
            if (_shader != 0) GL.DeleteProgram(_shader);
            _vao = _vbo = _ebo = _shader = 0;
        }
    }
}
