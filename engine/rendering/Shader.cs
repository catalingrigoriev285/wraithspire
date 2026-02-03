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
layout(location=1) in vec3 in_normal;

uniform mat4 u_proj;
uniform mat4 u_view;
uniform mat4 u_model;

out vec3 v_normal;
out vec3 v_fragPos;

void main(){
  v_fragPos = vec3(u_model * vec4(in_pos, 1.0));
  v_normal = mat3(transpose(inverse(u_model))) * in_normal;
  gl_Position = u_proj * u_view * vec4(v_fragPos, 1.0);
}";

            const string fs = @"#version 330 core
out vec4 out_color;

in vec3 v_normal;
in vec3 v_fragPos;

uniform vec3 u_viewPos;
uniform vec3 u_color;

// Light struct
struct Light {
    vec3 position;
    vec3 color;
    float intensity;
};
uniform Light u_light;

void main(){
  // Ambient
  float ambientStrength = 0.1;
  vec3 ambient = ambientStrength * u_light.color * u_light.intensity;
  
  // Diffuse
  vec3 norm = normalize(v_normal);
  vec3 lightDir = normalize(u_light.position - v_fragPos);
  float diff = max(dot(norm, lightDir), 0.0);
  vec3 diffuse = diff * u_light.color * u_light.intensity;
  
  // Specular
  float specularStrength = 0.5;
  vec3 viewDir = normalize(u_viewPos - v_fragPos);
  vec3 reflectDir = reflect(-lightDir, norm);
  float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
  vec3 specular = specularStrength * spec * u_light.color * u_light.intensity;
  
  vec3 result = (ambient + diffuse + specular) * u_color;
  out_color = vec4(result, 1.0);
}";
            return new Shader(vs, fs);
        }
    }
}
