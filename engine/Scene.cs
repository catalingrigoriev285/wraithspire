using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using wraithspire.engine.objects;
using wraithspire.engine.objects.primitives;

namespace wraithspire.engine
{
    internal class Scene : IDisposable
    {
        public string Name { get; set; }
        public List<Primitive> Objects { get; private set; } = new List<Primitive>();
        public CheckboardTerrain? Terrain { get; private set; }

        public Scene(string name)
        {
            Name = name;
        }

        private int _cubeCounter = 0;
        private int _sphereCounter = 0;
        private int _capsuleCounter = 0;
        private int _cylinderCounter = 0;
        private int _planeCounter = 0;
        private int _spawnCounter = 0;

        public void Initialize()
        {
            Terrain = new CheckboardTerrain();
            Terrain.Initialize();
        }

        public void CreateCube()
        {
            string name = _cubeCounter == 0 ? "Cube" : $"Cube ({_cubeCounter})";
            _cubeCounter++;

            var cube = new Cube(name);
            cube.Position = GetNextSpawnPosition();
            cube.Initialize();
            Objects.Add(cube);
        }

        public void CreateSphere()
        {
            string name = _sphereCounter == 0 ? "Sphere" : $"Sphere ({_sphereCounter})";
            _sphereCounter++;

            var sphere = new Sphere(name);
            sphere.Position = GetNextSpawnPosition();
            sphere.Initialize();
            Objects.Add(sphere);
        }

        public void CreateCapsule()
        {
            string name = _capsuleCounter == 0 ? "Capsule" : $"Capsule ({_capsuleCounter})";
            _capsuleCounter++;

            var capsule = new Capsule(name);
            capsule.Position = GetNextSpawnPosition();
            capsule.Initialize();
            Objects.Add(capsule);
        }

        public void CreateCylinder()
        {
            string name = _cylinderCounter == 0 ? "Cylinder" : $"Cylinder ({_cylinderCounter})";
            _cylinderCounter++;

            var cylinder = new Cylinder(name);
            cylinder.Position = GetNextSpawnPosition();
            cylinder.Initialize();
            Objects.Add(cylinder);
        }

        public void CreatePlane()
        {
            string name = _planeCounter == 0 ? "Plane" : $"Plane ({_planeCounter})";
            _planeCounter++;

            var plane = new Plane(name);
            plane.Position = GetNextSpawnPosition();
            plane.Initialize();
            Objects.Add(plane);
        }

        private Vector3 GetNextSpawnPosition()
        {
            // Simple grid spawn pattern so newly created objects don't overlap.
            const float spacing = 2.0f;
            const int cols = 6;

            int i = _spawnCounter++;
            int x = i % cols;
            int z = i / cols;

            float worldX = (x - (cols - 1) * 0.5f) * spacing;
            float worldZ = z * spacing;
            float worldY = 1f; // above ground
            return new Vector3(worldX, worldY, worldZ);
        }

        public void Dispose()
        {
            Terrain?.Dispose();
            foreach (var obj in Objects)
            {
                obj.Dispose();
            }
            Objects.Clear();
        }
    }
}
