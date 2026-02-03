using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

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
            foreach (var obj in scene.Objects)
            {
                obj.Render(projection, view);
            }
        }
    }
}
