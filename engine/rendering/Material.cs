using OpenTK.Mathematics;

namespace wraithspire.engine.rendering
{
    public class Material
    {
        public Shader Shader { get; set; }
        public Vector3 Color { get; set; } = Vector3.One;

        public Material(Shader shader)
        {
            Shader = shader;
        }

        public void Use(Matrix4 model, Matrix4 view, Matrix4 projection, Vector3 viewPos, Light light)
        {
            Shader.Use();
            Shader.SetMatrix4("u_model", model);
            Shader.SetMatrix4("u_view", view);
            Shader.SetMatrix4("u_proj", projection);
            Shader.SetVector3("u_color", Color);
            Shader.SetVector3("u_viewPos", viewPos);
            
            Shader.SetVector3("u_light.position", light.Position);
            Shader.SetVector3("u_light.color", light.Color);
            int location = OpenTK.Graphics.OpenGL4.GL.GetUniformLocation(Shader.Handle, "u_light.intensity");
            OpenTK.Graphics.OpenGL4.GL.Uniform1(location, light.Intensity);
        }
    }
}
