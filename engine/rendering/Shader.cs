using System;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace wraithspire.engine.rendering
{
    public class Shader : IDisposable
    {
        public int Handle { get; private set; }

        public Shader(string vertexShaderSource, string fragmentShaderSource)
        {
            int vertexShader = CompileShader(ShaderType.VertexShader, vertexShaderSource);
            int fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentShaderSource);

            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);
            GL.LinkProgram(Handle);

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
        }

        private int CompileShader(ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);

            // Check for errors
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error compiling {type}: {infoLog}");
            }

            return shader;
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        public void SetMatrix4(string name, Matrix4 matrix)
        {
            int location = GL.GetUniformLocation(Handle, name);
            GL.UniformMatrix4(location, false, ref matrix);
        }
        
        public void SetVector3(string name, Vector3 vector)
        {
            int location = GL.GetUniformLocation(Handle, name);
            GL.Uniform3(location, vector);
        }

        public void Dispose()
        {
            GL.DeleteProgram(Handle);
        }

        // --- Default Shaders ---
        public static Shader CreateBasicShader()
        {
            const string vs = @"#version 330 core
layout(location=0) in vec3 in_pos;
uniform mat4 u_proj;
uniform mat4 u_view;
uniform mat4 u_model;
void main(){
  gl_Position = u_proj * u_view * u_model * vec4(in_pos, 1.0);
}";

            const string fs = @"#version 330 core
out vec4 out_color;
uniform vec3 u_color;
void main(){
  out_color = vec4(u_color, 1.0);
}";
            return new Shader(vs, fs);
        }
    }
}
