namespace Reveche.MazeRunner;

public static class MazeRunner
{
    public static void Main(string[] args)
    {
        Console.Title = "Maze Runner";
        GameMenu.StartMenu();
        
        Console.ReadKey();
    }
}