using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace TestWindow
{

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

        public GWindow(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings()
        {
            Size = (width, height),
            Title = title
        }
        ){}

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
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit);

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
    }
}