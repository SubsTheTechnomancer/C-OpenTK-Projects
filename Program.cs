using TestWindow;
using OpenTK.Windowing.Desktop;

internal class Program
{

    private static void Main(string[] args)
    {
        int width = Monitors.GetPrimaryMonitor().HorizontalResolution;
        int height = Monitors.GetPrimaryMonitor().VerticalResolution;
        using (GWindow game = new GWindow(width, height, "Test"))
        {
            game.Run();
        }

    }

}