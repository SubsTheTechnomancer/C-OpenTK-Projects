using TestWindow;
using System.Runtime.InteropServices;

internal class Program
{

    private static void Main(string[] args)
    {

        using (GWindow game = new GWindow(800, 600, "Test"))
        {
            game.Run();
        }

    }

}