using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using wraithspire.engine.components;
using wraithspire.engine.rendering;

namespace wraithspire.engine
{
    public static class PrimitiveFactory
    {
        private static Shader _defaultShader;
        
        public static void Initialize()
        {
            _defaultShader = Shader.CreateBasicShader();
        }

        public static GameObject CreateCube(string name = "Cube")
        {
            if (_defaultShader == null) Initialize();

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
                -0.5f,  0.5f, -0.5f,
                // Right face
                 0.5f, -0.5f,  0.5f,
                 0.5f, -0.5f, -0.5f,
                 0.5f,  0.5f, -0.5f,
                 0.5f,  0.5f,  0.5f,
                // Left face
                -0.5f, -0.5f,  0.5f,
                -0.5f, -0.5f, -0.5f,
                -0.5f,  0.5f, -0.5f,
                -0.5f,  0.5f,  0.5f,
                // Top face
                -0.5f,  0.5f,  0.5f,
                 0.5f,  0.5f,  0.5f,
                 0.5f,  0.5f, -0.5f,
                -0.5f,  0.5f, -0.5f,
                // Bottom face
                -0.5f, -0.5f,  0.5f,
                 0.5f, -0.5f,  0.5f,
                 0.5f, -0.5f, -0.5f,
                -0.5f, -0.5f, -0.5f
            };

            float[] normals = new float[]
            {
                // Front
                0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 1.0f,
                // Back
                0.0f, 0.0f, -1.0f,
                0.0f, 0.0f, -1.0f,
                0.0f, 0.0f, -1.0f,
                0.0f, 0.0f, -1.0f,
                // Right
                1.0f, 0.0f, 0.0f,
                1.0f, 0.0f, 0.0f,
                1.0f, 0.0f, 0.0f,
                1.0f, 0.0f, 0.0f,
                // Left
                -1.0f, 0.0f, 0.0f,
                -1.0f, 0.0f, 0.0f,
                -1.0f, 0.0f, 0.0f,
                -1.0f, 0.0f, 0.0f,
                // Top
                0.0f, 1.0f, 0.0f,
                0.0f, 1.0f, 0.0f,
                0.0f, 1.0f, 0.0f,
                0.0f, 1.0f, 0.0f,
                // Bottom
                0.0f, -1.0f, 0.0f,
                0.0f, -1.0f, 0.0f,
                0.0f, -1.0f, 0.0f,
                0.0f, -1.0f, 0.0f
            };

            uint[] indices = new uint[]
            {
                // Front
                0, 1, 2, 0, 2, 3,
                // Back
                5, 4, 7, 5, 7, 6,
                // Right
                8, 9, 10, 8, 10, 11,
                // Left
                12, 15, 14, 12, 14, 13,
                // Top
                16, 17, 18, 16, 18, 19,
                // Bottom
                20, 23, 22, 20, 22, 21
            };

            return CreateGameObject(name, vertices, normals, indices, new Vector3(0.3f, 0.6f, 0.9f));
        }

        public static GameObject CreateSphere(string name = "Sphere", int segments = 16, int rings = 16)
        {
            if (_defaultShader == null) Initialize();

            float radius = 0.5f;
            var verts = new List<float>();
            var norms = new List<float>();
            var inds = new List<uint>();

            for (int r = 0; r <= rings; r++)
            {
                float v = r / (float)rings;
                float phi = MathF.PI * v;
                float y = MathF.Cos(phi);
                float sinPhi = MathF.Sin(phi);

                for (int s = 0; s <= segments; s++)
                {
                    float u = s / (float)segments;
                    float theta = 2f * MathF.PI * u;
                    float x = sinPhi * MathF.Cos(theta);
                    float z = sinPhi * MathF.Sin(theta);

                    verts.Add(x * radius);
                    verts.Add(y * radius);
                    verts.Add(z * radius);

                    norms.Add(x);
                    norms.Add(y);
                    norms.Add(z);
                }
            }

            int stride = segments + 1;
            for (int r = 0; r < rings; r++)
            {
                for (int s = 0; s < segments; s++)
                {
                    int i0 = r * stride + s;
                    int i1 = i0 + 1;
                    int i2 = i0 + stride;
                    int i3 = i2 + 1;

                    inds.Add((uint)i0);
                    inds.Add((uint)i2);
                    inds.Add((uint)i1);

                    inds.Add((uint)i1);
                    inds.Add((uint)i2);
                    inds.Add((uint)i3);
                }
            }

            return CreateGameObject(name, verts.ToArray(), norms.ToArray(), inds.ToArray(), new Vector3(0.9f, 0.4f, 0.3f));
        }

        private static GameObject CreateGameObject(string name, float[] vertices, float[] normals, uint[] indices, Vector3 color)
        {
            var go = new GameObject(name);
            var meshFilter = go.AddComponent<MeshFilter>();
            meshFilter.Mesh = new Mesh(vertices, normals, indices);

            var meshRenderer = go.AddComponent<MeshRenderer>();
            meshRenderer.Material = new Material(_defaultShader);
            meshRenderer.Material.Color = color;

            return go;
        }

        public static void Dispose()
        {
            _defaultShader?.Dispose();
        }
    }
}
