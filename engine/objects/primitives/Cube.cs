using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace wraithspire.engine.objects.primitives
{
    internal sealed class Cube : Primitive
    {
        public Cube(string name = "Cube")
        {
            Name = name;
            Color = new Vector3(0.3f, 0.6f, 0.9f); // Nice blue color
        }

        public override void Initialize()
        {
            CreateBasicShader();
            BuildMesh();
        }

        private void BuildMesh()
        {
            // Cube vertices (8 corners)
            float[] vertices = new float[]
            {
                // Front face
                -0.5f, -0.5f,  0.5f,
                 0.5f, -0.5f,  0.5f,
                 0.5f,  0.5f,  0.5f,
                -0.5f,  0.5f,  0.5f,
                // Back face
                -0.5f, -0.5f, -0.5f,
                 0.5f, -0.5f, -0.5f,
                 0.5f,  0.5f, -0.5f,
                -0.5f,  0.5f, -0.5f
            };

            // Cube indices (36 indices for 12 triangles, 2 per face)
            int[] indices = new int[]
            {
                // Front
                0, 1, 2, 0, 2, 3,
                // Right
                1, 5, 6, 1, 6, 2,
                // Back
                5, 4, 7, 5, 7, 6,
                // Left
                4, 0, 3, 4, 3, 7,
                // Top
                3, 2, 6, 3, 6, 7,
                // Bottom
                4, 5, 1, 4, 1, 0
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
