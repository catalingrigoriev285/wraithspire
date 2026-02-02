using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace wraithspire.engine.objects.primitives
{
    internal abstract class Primitive : IDisposable
    {
        public string Name { get; set; } = "Primitive";
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Vector3 Rotation { get; set; } = Vector3.Zero;
        public Vector3 Scale { get; set; } = Vector3.One;
        public Vector3 Color { get; set; } = new Vector3(1f, 1f, 1f);

        protected int _vao;
        protected int _vbo;
        protected int _ebo;
        protected int _shader;
        protected int _projLoc;
        protected int _viewLoc;
        protected int _modelLoc;
        protected int _colorLoc;
        protected int _indexCount;

        public abstract void Initialize();
        public abstract void Render(Matrix4 proj, Matrix4 view);
        public abstract void Dispose();

        protected void CreateBasicShader()
        {
            const string vs = "#version 330 core\n" +
                              "layout(location=0) in vec3 in_pos;\n" +
                              "uniform mat4 u_proj;\n" +
                              "uniform mat4 u_view;\n" +
                              "uniform mat4 u_model;\n" +
                              "void main(){\n" +
                              "  gl_Position = u_proj * u_view * u_model * vec4(in_pos, 1.0);\n" +
                              "}";

            const string fs = "#version 330 core\n" +
                              "out vec4 out_color;\n" +
                              "uniform vec3 u_color;\n" +
                              "void main(){\n" +
                              "  out_color = vec4(u_color, 1.0);\n" +
                              "}";

            int vert = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vert, vs);
            GL.CompileShader(vert);
            
            int frag = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(frag, fs);
            GL.CompileShader(frag);
            
            _shader = GL.CreateProgram();
            GL.AttachShader(_shader, vert);
            GL.AttachShader(_shader, frag);
            GL.LinkProgram(_shader);
            
            GL.DeleteShader(vert);
            GL.DeleteShader(frag);
            
            _projLoc = GL.GetUniformLocation(_shader, "u_proj");
            _viewLoc = GL.GetUniformLocation(_shader, "u_view");
            _modelLoc = GL.GetUniformLocation(_shader, "u_model");
            _colorLoc = GL.GetUniformLocation(_shader, "u_color");
        }

        protected Matrix4 GetModelMatrix()
        {
            var model = Matrix4.Identity;
            model *= Matrix4.CreateScale(Scale);
            model *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(Rotation.X));
            model *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(Rotation.Y));
            model *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(Rotation.Z));
            model *= Matrix4.CreateTranslation(Position);
            return model;
        }
    }
}
