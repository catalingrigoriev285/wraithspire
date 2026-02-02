using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace wraithspire.engine.objects.primitives
{
    internal sealed class Plane : Primitive
    {
        public Plane(string name = "Plane")
        {
            Name = name;
            Color = new Vector3(0.7f, 0.7f, 0.7f);
        }

        public override void Initialize()
        {
            CreateBasicShader();
            BuildMesh();
        }

        private void BuildMesh()
        {
            float[] vertices = new float[]
            {
                -0.5f, 0f, -0.5f,
                 0.5f, 0f, -0.5f,
                 0.5f, 0f,  0.5f,
                -0.5f, 0f,  0.5f,
            };

            int[] indices = new int[]
            {
                0, 1, 2,
                0, 2, 3
            };

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
