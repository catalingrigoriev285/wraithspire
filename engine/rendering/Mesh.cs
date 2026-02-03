using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace wraithspire.engine.rendering
{
    public class Mesh : IDisposable
    {
        public float[] Vertices { get; private set; }
        public float[] Normals { get; private set; }
        public uint[] Indices { get; private set; }

        private int _vao;
        private int _vbo;
        private int _nbo; // Normal Buffer Object
        private int _ebo;

        public Vector3 Min { get; private set; }
        public Vector3 Max { get; private set; }

        public Mesh(float[] vertices, float[] normals, uint[] indices)
        {
            Vertices = vertices;
            Normals = normals;
            Indices = indices;
            CalculateBounds();
            SetupMesh();
        }

        private void CalculateBounds()
        {
            float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;

            for (int i = 0; i < Vertices.Length; i += 3)
            {
                float x = Vertices[i];
                float y = Vertices[i + 1];
                float z = Vertices[i + 2];

                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
                if (z < minZ) minZ = z;
                if (z > maxZ) maxZ = z;
            }

            Min = new Vector3(minX, minY, minZ);
            Max = new Vector3(maxX, maxY, maxZ);
        }

        private void SetupMesh()
        {
            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * sizeof(float), Vertices, BufferUsageHint.StaticDraw);
            
            // Position attribute (Location 0)
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            if (Normals != null && Normals.Length > 0)
            {
                _nbo = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, _nbo);
                GL.BufferData(BufferTarget.ArrayBuffer, Normals.Length * sizeof(float), Normals, BufferUsageHint.StaticDraw);

                // Normal attribute (Location 1)
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
                GL.EnableVertexAttribArray(1);
            }

            _ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, Indices.Length * sizeof(uint), Indices, BufferUsageHint.StaticDraw);

            GL.BindVertexArray(0);
        }

        public void Bind()
        {
            GL.BindVertexArray(_vao);
        }

        public void Dispose()
        {
            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(_vbo);
            if (_nbo != 0) GL.DeleteBuffer(_nbo);
            GL.DeleteBuffer(_ebo);
        }
    }
}
