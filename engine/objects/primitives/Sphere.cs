using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace wraithspire.engine.objects.primitives
{
    internal sealed class Sphere : Primitive
    {
        private readonly int _segments;
        private readonly int _rings;

        public Sphere(string name = "Sphere", int segments = 16, int rings = 16)
        {
            Name = name;
            Color = new Vector3(0.9f, 0.4f, 0.3f);
            _segments = Math.Max(3, segments);
            _rings = Math.Max(2, rings);
        }

        public override void Initialize()
        {
            CreateBasicShader();
            BuildMesh();
        }

        private void BuildMesh()
        {
            // Unit sphere (radius 0.5 to match Cube size roughly)
            float radius = 0.5f;

            var verts = new System.Collections.Generic.List<float>();
            var inds = new System.Collections.Generic.List<int>();

            for (int r = 0; r <= _rings; r++)
            {
                float v = r / (float)_rings;
                float phi = MathF.PI * v;
                float y = MathF.Cos(phi);
                float sinPhi = MathF.Sin(phi);

                for (int s = 0; s <= _segments; s++)
                {
                    float u = s / (float)_segments;
                    float theta = 2f * MathF.PI * u;
                    float x = sinPhi * MathF.Cos(theta);
                    float z = sinPhi * MathF.Sin(theta);

                    verts.Add(x * radius);
                    verts.Add(y * radius);
                    verts.Add(z * radius);
                }
            }

            int stride = _segments + 1;
            for (int r = 0; r < _rings; r++)
            {
                for (int s = 0; s < _segments; s++)
                {
                    int i0 = r * stride + s;
                    int i1 = i0 + 1;
                    int i2 = i0 + stride;
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
