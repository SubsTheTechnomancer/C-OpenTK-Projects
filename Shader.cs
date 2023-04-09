using System.Runtime.InteropServices;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace TestWindow
{
    public class Shader: IDisposable
    {
        int Handle;

        public Shader(string vertexPath, string fragmentPath)
        {
            string VertexShaderSource = File.ReadAllText(vertexPath);
            string FragmentShaderSource = File.ReadAllText(fragmentPath);

            var VertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexShader, VertexShaderSource);
            GL.CompileShader(VertexShader);

            GL.GetShader(VertexShader,ShaderParameter.CompileStatus,out int success);
            if(success==0) Console.WriteLine(GL.GetShaderInfoLog(VertexShader));

            var FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentShader, FragmentShaderSource);
            GL.CompileShader(FragmentShader);

            GL.GetShader(FragmentShader,ShaderParameter.CompileStatus,out success);
            if(success==0) Console.WriteLine(GL.GetShaderInfoLog(FragmentShader));

            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, VertexShader);
            GL.AttachShader(Handle, FragmentShader);

            GL.LinkProgram(Handle);

            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out success);
            if(success==0) Console.WriteLine(GL.GetProgramInfoLog(Handle));

            //Cleanup
            GL.DetachShader(Handle, VertexShader);
            GL.DetachShader(Handle,FragmentShader);
            GL.DeleteShader(VertexShader);
            GL.DeleteShader(FragmentShader);

        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(Handle, attribName);
        }

        public void SetMatrix4(string uniformName, Matrix4 matrix)
        {
            GL.UniformMatrix4(GL.GetUniformLocation(Handle,uniformName), true, ref matrix);
        }

        public void SetVec3(string uniformName, Vector3 vector)
        {
            GL.Uniform3(GL.GetUniformLocation(Handle,uniformName),vector.X, vector.Y, vector.Z);
        }

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if(!disposedValue)
            {
                GL.DeleteProgram(Handle);
                disposedValue = true;
            }
        }

        ~Shader()
        {
            GL.DeleteProgram(Handle);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}