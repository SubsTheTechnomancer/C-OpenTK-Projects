using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
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

    public class GWindow : GameWindow
    {

        [StructLayout(LayoutKind.Sequential)]
        public struct _DWM_BLURBEHIND
        {
            public uint dwFlags;
            public bool fEnable;
            public IntPtr hRgnBlur;
            public bool fTransitionOnMaximized;
        }

        [DllImport("dwmapi.dll")]
        static extern int DwmEnableBlurBehindWindow(IntPtr hWnd,ref _DWM_BLURBEHIND pBlurBehind);
        [DllImport("user32.dll")]
        static extern IntPtr GetActiveWindow();
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateRectRgn(int x1, int y1, int x2, int y2);

        float[] vertices =
        {
            -0.5f,-0.5f, 0.0f,
            0.5f, -0.5f, 0.0f,
            0.0f, 0.5f, 0.0f
        };

        int VertexBufferObject;
        int VertexArrayObject;

        Shader shader;

        public GWindow(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings()
        {
            Size = (width, height),
            Title = title
        }
        )
        {}

        protected override void OnLoad()
        {
            base.OnLoad();
            
            _DWM_BLURBEHIND bb;
            bb.dwFlags = 0x00000001 | 0x00000002;    //DWM_BB_ENABLE
            bb.fEnable = true;
            bb.hRgnBlur = CreateRectRgn(0,0,-1,-1);
            bb.fTransitionOnMaximized = false;

            IntPtr hWnd = GetActiveWindow();
            DwmEnableBlurBehindWindow(hWnd,ref bb);

            GL.ClearColor(0.0f,0.0f,0.0f,0.0f);

            //Generate Buffers
            VertexBufferObject = GL.GenBuffer();
            VertexArrayObject = GL.GenVertexArray();

            GL.BindVertexArray(VertexArrayObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            
            GL.VertexAttribPointer(0,3,VertexAttribPointerType.Float,false,3*sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            shader = new Shader("shader.vert", "shader.frag");
            shader.Use();

        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0,0,e.Width,e.Height);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            shader.Use();
            GL.BindVertexArray(VertexArrayObject);
            GL.DrawArrays(PrimitiveType.Triangles,0,3);

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            KeyboardState input = KeyboardState;

            if(input.IsKeyDown(Keys.Escape))
            {
                Close();
            }

        }

        protected override void OnUnload()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer,0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            GL.DeleteBuffer(VertexBufferObject);
            GL.DeleteVertexArray(VertexArrayObject);

            shader.Dispose();
            
            base.OnUnload();
        }

    }
}