using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using wraithspire.engine.components;

namespace wraithspire.engine
{
    internal class Renderer
    {
        public void Render(Scene scene, Matrix4 view, Matrix4 projection)
        {
            GL.Enable(EnableCap.DepthTest);
            
            // Render Terrain
            scene.Terrain?.Render(projection, view);

            // Render all scene objects
            foreach (var go in scene.GameObjects)
            {
                if (!go.IsActive) continue;
                
                var meshRenderer = go.GetComponent<MeshRenderer>();
                meshRenderer?.Render(view, projection);
            }
        }
    }
}
