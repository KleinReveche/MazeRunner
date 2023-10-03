namespace Reveche.MazeRunner;

public static class MazeRunner
{
    public static void Main(string[] args)
    {
        var gameState = new GameState();
        Program.Play(gameState);
        Console.ReadKey();
    }
}