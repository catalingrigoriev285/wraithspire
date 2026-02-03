using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using wraithspire.engine.rendering;
using wraithspire.engine.objects;
using wraithspire.engine.physics;
using wraithspire.engine.components;

namespace wraithspire.engine
{
    internal class Scene : IDisposable
    {
        public string Name { get; set; }
        public List<GameObject> GameObjects { get; private set; } = new List<GameObject>();
        public CheckboardTerrain? Terrain { get; private set; } // Terrain might need refactoring too, but keeping it for now
        public Camera Camera { get; private set; }
        public List<GameObject> RootObjects { get; private set; } = new List<GameObject>();

        public Scene(string name)
        {
            Name = name;
            Camera = new Camera();
            
            // Add a default light
            CreateLight();
        }

        private int _cubeCounter = 0;
        private int _sphereCounter = 0;
        private int _spawnCounter = 0;

        public void Initialize()
        {
            Terrain = new CheckboardTerrain();
            Terrain.Initialize();
            
            // Ensure shader init
            PrimitiveFactory.Initialize();
        }

        public void CreateCube()
        {
            string name = _cubeCounter == 0 ? "Cube" : $"Cube ({_cubeCounter})";
            _cubeCounter++;

            var cube = PrimitiveFactory.CreateCube(name);
            cube.Transform.Position = GetNextSpawnPosition();
            GameObjects.Add(cube);
        }

        public void CreateSphere()
        {
            string name = _sphereCounter == 0 ? "Sphere" : $"Sphere ({_sphereCounter})";
            _sphereCounter++;

            var sphere = PrimitiveFactory.CreateSphere(name);
            sphere.Transform.Position = GetNextSpawnPosition();
            GameObjects.Add(sphere);
        }

        public void CreateLight()
        {
            var lightObj = new GameObject("Point Light");
            // Default position
            lightObj.Transform.Position = new Vector3(2f, 4f, 2f);
            
            // Add LightComponent
            lightObj.AddComponent<LightComponent>();
            
            // Add a small debug sphere representation? 
            // Maybe not essential for now, but helpful to see where it is
            // We can reuse primitive factory for a small sphere representation if we want, or rely on a "Gizmo" later
            // For now, let's just make it an empty object with a component.
            // Actually, let's give it a visual representation so we can click it!
            var visual = PrimitiveFactory.CreateSphere("LightVisual", 8, 8);
            // We want the visual to be part of the lightObj ideally, but our system is simple.
            // Let's just add the mesh components to the lightObj itself.
            var meshFilter = visual.GetComponent<MeshFilter>();
            var meshRenderer = visual.GetComponent<MeshRenderer>();
            
            if (meshFilter != null) lightObj.AddComponent<MeshFilter>().Mesh = meshFilter.Mesh;
            if (meshRenderer != null) 
            {
                var newRenderer = lightObj.AddComponent<MeshRenderer>();
                newRenderer.Material = meshRenderer.Material;
                newRenderer.Material!.Color = new Vector3(1f, 1f, 0.5f); // Light bulb color
                // TODO: Make this unlit or emissive later so it looks bright
            }
            
            lightObj.Transform.Scale = new Vector3(0.2f); // Small bulb

            GameObjects.Add(lightObj);
        }

        // Placeholder for other shapes until they are ported to PrimitiveFactory
        public void CreateCapsule() { }
        public void CreateCylinder() { }
        public void CreatePlane() { }

        private Vector3 GetNextSpawnPosition()
        {
            // Simple grid spawn pattern
            const float spacing = 2.0f;
            const int cols = 6;

            int i = _spawnCounter++;
            int x = i % cols;
            int z = i / cols;

            float worldX = (x - (cols - 1) * 0.5f) * spacing;
            float worldZ = z * spacing;
            float worldY = 0.5f; // Center of object usually (radius 0.5)
            return new Vector3(worldX, worldY, worldZ);
        }
        
        public void Update()
        {
            foreach (var go in GameObjects)
            {
                go.Update();
            }
        }

        public GameObject? Raycast(Ray ray)
        {
            GameObject? closestObj = null;
            float closestDist = float.MaxValue;

            foreach (var go in GameObjects)
            {
                if (!go.IsActive) continue;

                var meshFilter = go.GetComponent<MeshFilter>();
                if (meshFilter == null || meshFilter.Mesh == null) continue;

                // Transform ray to local space
                Matrix4 model = go.Transform.GetModelMatrix();
                Matrix4 invModel = Matrix4.Invert(model);

                Vector4 localOrigin4 = new Vector4(ray.Origin, 1.0f) * invModel;
                Vector4 localDir4 = new Vector4(ray.Direction, 0.0f) * invModel;
                
                Vector3 localOrigin = localOrigin4.Xyz; // w should be 1
                Vector3 localDir = localDir4.Xyz; // w should be 0

                // RayIntersectsBox expects unit direction for correct T?
                // Actually my implementation of RayIntersectsBox is purely algebraic:
                // t = (min - origin) / dir
                // So if dir is scaled, t is inversely scaled.
                // Point = Origin + Dir * t
                // It holds regardless of length of Dir.

                Ray localRay = new Ray(localOrigin, localDir);

                float? t = Physics.RayIntersectsBox(localRay, meshFilter.Mesh.Min, meshFilter.Mesh.Max);

                if (t.HasValue && t.Value > 0)
                {
                    // Calculate world distance
                    Vector3 hitLocal = localOrigin + localDir * t.Value;
                    Vector4 hitWorld4 = new Vector4(hitLocal, 1.0f) * model;
                    Vector3 hitWorld = hitWorld4.Xyz;

                    float dist = (hitWorld - ray.Origin).Length;
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestObj = go;
                    }
                }
            }

            return closestObj;
        }

        public void Dispose()
        {
            Terrain?.Dispose();
            foreach (var go in GameObjects)
            {
                go.Dispose();
            }
            GameObjects.Clear();
        }
    }
}
