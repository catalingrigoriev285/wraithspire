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

            // Find Main Light
            rendering.Light mainLight;
            
            if (wraithspire.engine.editor.modules.GlobalSettings.IsLightingEnabled)
            {
                mainLight = new rendering.Light(new Vector3(0, 5, 0), Vector3.One);
                LightComponent? sceneLight = null;
                // Iterate to find first active light
                foreach (var go in scene.GameObjects)
                {
                    if (!go.IsActive) continue;
                    var lightComp = go.GetComponent<LightComponent>();
                    if (lightComp != null)
                    {
                        sceneLight = lightComp;
                        break;
                    }
                }

                if (sceneLight != null)
                {
                    mainLight = new rendering.Light(sceneLight.Transform.Position, sceneLight.Color, sceneLight.Intensity);
                }
            }
            else
            {
                // Disable lighting (use 1.0 intensity to mimic unlit/ambient)
                mainLight = new rendering.Light(Vector3.Zero, Vector3.One, 1.0f);
            }

            // Render all scene objects
            bool lightingEnabled = wraithspire.engine.editor.modules.GlobalSettings.IsLightingEnabled;
            foreach (var go in scene.GameObjects)
            {
                if (!go.IsActive) continue;
                
                var meshRenderer = go.GetComponent<MeshRenderer>();
                meshRenderer?.Render(view, projection, scene.Camera.Position, mainLight, lightingEnabled);
            }
        }
    }
}
