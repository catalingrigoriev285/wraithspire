using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace wraithspire.engine.objects.primitives
{
    internal sealed class Cylinder : Primitive
    {
        private readonly int _segments;

        public Cylinder(string name = "Cylinder", int segments = 24)
        {
            Name = name;
            Color = new Vector3(0.3f, 0.85f, 0.4f);
            _segments = Math.Max(3, segments);
        }

        public override void Initialize()
        {
            CreateBasicShader();
            BuildMesh();
        }

        private void BuildMesh()
        {
            float radius = 0.5f;
            float halfHeight = 0.5f;

            var verts = new System.Collections.Generic.List<float>();
            var inds = new System.Collections.Generic.List<int>();

            // Side vertices: top ring then bottom ring
            for (int i = 0; i < _segments; i++)
            {
                float a = 2f * MathF.PI * i / _segments;
                float x = MathF.Cos(a) * radius;
                float z = MathF.Sin(a) * radius;

                verts.Add(x); verts.Add(halfHeight); verts.Add(z);
                verts.Add(x); verts.Add(-halfHeight); verts.Add(z);
            }

            // Side triangles
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

            // Caps: center vertices
            int topCenterIndex = verts.Count / 3;
            verts.Add(0f); verts.Add(halfHeight); verts.Add(0f);
            int bottomCenterIndex = verts.Count / 3;
            verts.Add(0f); verts.Add(-halfHeight); verts.Add(0f);

            // Top cap
            for (int i = 0; i < _segments; i++)
            {
                int next = (i + 1) % _segments;
                int vi = i * 2;
                int vnext = next * 2;
                inds.Add(topCenterIndex);
                inds.Add(vnext);
                inds.Add(vi);
            }

            // Bottom cap
            for (int i = 0; i < _segments; i++)
            {
                int next = (i + 1) % _segments;
                int vi = i * 2 + 1;
                int vnext = next * 2 + 1;
                inds.Add(bottomCenterIndex);
                inds.Add(vi);
                inds.Add(vnext);
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
