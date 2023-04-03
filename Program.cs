using TestWindow;

internal class Program
{

    private static void Main(string[] args)
    {

        using (GWindow game = new GWindow(800, 800, "Test"))
        {
            game.Run();
        }

    }

}