using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using wraithspire.engine.objects;

namespace wraithspire.engine
{
    internal class Scene : IDisposable
    {
        public string Name { get; set; }
        public List<GameObject> GameObjects { get; private set; } = new List<GameObject>();
        public CheckboardTerrain? Terrain { get; private set; } // Terrain might need refactoring too, but keeping it for now

        public Scene(string name)
        {
            Name = name;
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
