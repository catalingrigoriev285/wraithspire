using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using wraithspire.engine.rendering;

namespace wraithspire.engine.components
{
    public class MeshRenderer : Component
    {
        public Material? Material { get; set; }

        public void Render(Matrix4 view, Matrix4 projection, Vector3 viewPos, Light light)
        {
            var meshFilter = GameObject.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.Mesh == null || Material == null) return;

            Matrix4 model = Transform.GetModelMatrix();
            Material.Use(model, view, projection, viewPos, light);

            meshFilter.Mesh.Bind();
            GL.DrawElements(PrimitiveType.Triangles, meshFilter.Mesh.Indices.Length, DrawElementsType.UnsignedInt, 0);
            
            // Unbind vao
            GL.BindVertexArray(0);
        }
    }
}
