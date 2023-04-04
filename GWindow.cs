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
             0.5f,  0.5f, 0.5f,
             0.5f, -0.5f, 0.5f,
            -0.5f, -0.5f, 0.5f,
            -0.5f,  0.5f, 0.5f,
             0.5f,  0.5f, -0.5f,
             0.5f, -0.5f, -0.5f,
            -0.5f, -0.5f, -0.5f,
            -0.5f,  0.5f, -0.5f,
        };

        uint[] indices =
        {
            4,5,7,
            5,6,7,
            3,2,0,
            0,2,1,
            0,1,4,
            4,1,5,
            7,6,3,
            3,6,2,
            0,4,3,
            3,4,7,
            5,1,6,
            6,1,2
        };

        int VertexBufferObject;
        int ElementBufferObject;
        int VertexArrayObject;

        Stopwatch timer;

        Shader shader;

        public GWindow(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings()
        {
            Size = (height, height),        //Forcing 1:1 Aspect Ratio (TODO: This would not work in vertical setups)
            Title = title,
            WindowBorder = WindowBorder.Hidden,
            AspectRatio = (1,1),
            Location = new Vector2i(width/2-height/2,0)
        }
        )
        {
            timer = new Stopwatch();
            Console.WriteLine("{0:N},{1:N},{2:N}",width,height,width/2-height/2);
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            
            _DWM_BLURBEHIND bb;
            bb.dwFlags = 0x00000001 | 0x00000002;           //DWM_BB_ENABLE
            bb.fEnable = true;
            bb.hRgnBlur = CreateRectRgn(0,0,-1,-1);
            bb.fTransitionOnMaximized = false;

            IntPtr hWnd = GetActiveWindow();
            DwmEnableBlurBehindWindow(hWnd,ref bb);

            GL.ClearColor(0.0f,0.0f,0.0f,0.0f);
            GL.Enable(EnableCap.DepthTest);

            shader = new Shader("shader.vert", "shader.frag");
            //Generate Buffers
            VertexBufferObject = GL.GenBuffer();
            ElementBufferObject = GL.GenBuffer();
            VertexArrayObject = GL.GenVertexArray();

            GL.BindVertexArray(VertexArrayObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.DynamicDraw);
            
            GL.VertexAttribPointer(shader.GetAttribLocation("aPosition"),3,VertexAttribPointerType.Float,false,3*sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            shader.Use();

            timer.Start();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            shader.Use();

            float angle = (float)timer.Elapsed.TotalSeconds%360.0f;
            Matrix4 mMatrix = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(20*angle)) * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(10*angle));
            Matrix4 vMatrix = Matrix4.CreateTranslation(0.0f, 0.0f, -3.0f);
            Matrix4 pMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f),Size.X/Size.Y,0.1f,100.0f);
            shader.SetMatrix4("model",mMatrix);
            shader.SetMatrix4("view",vMatrix);
            shader.SetMatrix4("projection",pMatrix);

            GL.BindVertexArray(VertexArrayObject);
            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

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