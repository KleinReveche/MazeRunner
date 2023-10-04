namespace Reveche.MazeRunner;

public static class MazeRunner
{
    public static void Main(string[] args)
    {
        var gameState = new GameState();
        Game.Play(gameState);
        Console.ReadKey();
    }
}