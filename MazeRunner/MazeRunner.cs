namespace Reveche.MazeRunner;

public static class MazeRunner
{
    public static void Main(string[] args)
    {
        Console.Title = "Maze Runner";
        
        var gameState = new GameState();
        var game = new Game(gameState);
        game.Play();
        
        Console.ReadKey();
    }
}