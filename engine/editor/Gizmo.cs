using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using wraithspire.engine.rendering;

namespace wraithspire.engine.editor
{
    public class Gizmo : IDisposable
    {
        private int _vao;
        private int _vbo;
        private int _shader;
        private int _projLoc;
        private int _viewLoc;
        private int _modelLoc;
        private int _colorLoc;

        private const float AxisLength = 2.0f;
        private const float AxisThickness = 0.05f;

        private int _coneVao;
        private int _coneVbo;
        private int _coneIndicesCount;

        public Gizmo()
        {
            Initialize();
        }

        private void Initialize()
        {
            // Simple shader that ignores depth test
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

            // --- Cube (Lines) ---
            float[] vertices = new float[]
            {
                0f, 0f, 0f, 1f, 0f, 0f, 1f, 1f, 0f, 0f, 1f, 0f,
                0f, 0f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 0f, 1f, 1f
            };
            int[] indices = new int[]
            {
                0,1,2, 0,2,3, 1,5,6, 1,6,2, 5,4,7, 5,7,6,
                4,0,3, 4,3,7, 3,2,6, 3,6,7, 4,5,1, 4,1,0
            };

            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            int ebo = GL.GenBuffer();

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(int), indices, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.BindVertexArray(0);

            // --- Cone (Arrowhead) ---
            // Simple pyramid: Base is a square (or triangle)
            // Base center (0,0,0) -> Tip (1,0,0) (Pointing +X)
            // Let's make it point UP (+Y) by default for easier sizing, then rotate.
            // Vertices: Tip(0,1,0), Base corners...
            
            float cw = 0.15f; // Cone width radius
            float ch = 0.4f;  // Cone height
            
            float[] coneVerts = new float[]
            {
                // Base
                -cw, 0, -cw,
                 cw, 0, -cw,
                 cw, 0,  cw,
                -cw, 0,  cw,
                // Tip
                0, ch, 0
            };
            // 4 Base indices (2 tris), 4 Side indices (4 tris)
            // Indices:
            // Base: 0,2,1, 0,3,2
            // Sides: 0,1,4, 1,2,4, 2,3,4, 3,0,4
            
            int[] coneInds = new int[]
            {
                0,2,1, 0,3,2,
                0,1,4, 1,2,4, 2,3,4, 3,0,4
            };
            _coneIndicesCount = coneInds.Length;

            _coneVao = GL.GenVertexArray();
            _coneVbo = GL.GenBuffer();
            int coneEbo = GL.GenBuffer();

            GL.BindVertexArray(_coneVao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _coneVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, coneVerts.Length * sizeof(float), coneVerts, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, coneEbo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, coneInds.Length * sizeof(int), coneInds, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.BindVertexArray(0);
        }

        public void Render(Vector3 position, Matrix4 view, Matrix4 projection, int highlightAxis = -1)
        {
            GL.UseProgram(_shader);
            GL.Disable(EnableCap.DepthTest); 

            GL.UniformMatrix4(_viewLoc, false, ref view);
            GL.UniformMatrix4(_projLoc, false, ref projection);
            
            // --- Axes Lines ---
            GL.BindVertexArray(_vao);
            
            // X Axis (Red)
            var modelX = Matrix4.CreateScale(AxisLength, AxisThickness, AxisThickness) * Matrix4.CreateTranslation(position);
            GL.UniformMatrix4(_modelLoc, false, ref modelX);
            GL.Uniform3(_colorLoc, highlightAxis == 0 ? new Vector3(1f, 1f, 0f) : new Vector3(1f, 0f, 0f));
            GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, 0);

            // Y Axis (Green)
            var modelY = Matrix4.CreateScale(AxisThickness, AxisLength, AxisThickness) * Matrix4.CreateTranslation(position);
            GL.UniformMatrix4(_modelLoc, false, ref modelY);
            GL.Uniform3(_colorLoc, highlightAxis == 1 ? new Vector3(1f, 1f, 0f) : new Vector3(0f, 1f, 0f));
            GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, 0);

             // Z Axis (Blue)
            var modelZ = Matrix4.CreateScale(AxisThickness, AxisThickness, AxisLength) * Matrix4.CreateTranslation(position);
            GL.UniformMatrix4(_modelLoc, false, ref modelZ);
            GL.Uniform3(_colorLoc, highlightAxis == 2 ? new Vector3(1f, 1f, 0f) : new Vector3(0f, 0f, 1f));
            GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, 0);

            // --- Cones ---
            GL.BindVertexArray(_coneVao);

            // X Cone (Rotated to point to +X) - Original points +Y
            // Rotate -90 Z
            var coneX = Matrix4.CreateRotationZ(-MathHelper.PiOver2) * Matrix4.CreateTranslation(position + Vector3.UnitX * AxisLength);
            GL.UniformMatrix4(_modelLoc, false, ref coneX);
            GL.Uniform3(_colorLoc, highlightAxis == 0 ? new Vector3(1f, 1f, 0f) : new Vector3(1f, 0f, 0f));
            GL.DrawElements(PrimitiveType.Triangles, _coneIndicesCount, DrawElementsType.UnsignedInt, 0);

            // Y Cone (Points +Y default)
            var coneY = Matrix4.CreateTranslation(position + Vector3.UnitY * AxisLength);
            GL.UniformMatrix4(_modelLoc, false, ref coneY);
            GL.Uniform3(_colorLoc, highlightAxis == 1 ? new Vector3(1f, 1f, 0f) : new Vector3(0f, 1f, 0f));
            GL.DrawElements(PrimitiveType.Triangles, _coneIndicesCount, DrawElementsType.UnsignedInt, 0);

            // Z Cone (Rotated to point +Z) - Original +Y
            // Rotate 90 X
            var coneZ = Matrix4.CreateRotationX(MathHelper.PiOver2) * Matrix4.CreateTranslation(position + Vector3.UnitZ * AxisLength);
            GL.UniformMatrix4(_modelLoc, false, ref coneZ);
            GL.Uniform3(_colorLoc, highlightAxis == 2 ? new Vector3(1f, 1f, 0f) : new Vector3(0f, 0f, 1f));
            GL.DrawElements(PrimitiveType.Triangles, _coneIndicesCount, DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(0);
            GL.Enable(EnableCap.DepthTest);
            GL.UseProgram(0);
        }

        public void Dispose()
        {
            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(_vbo);
            GL.DeleteVertexArray(_coneVao);
            GL.DeleteBuffer(_coneVbo);
            GL.DeleteProgram(_shader);
        }
    }
}
