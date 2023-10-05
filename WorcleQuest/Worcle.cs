namespace Reveche.WorcleQuest;

public static class Game
{
    public static bool Start()
    {
        Console.Title = "Worcle";
        Console.SetCursorPosition(6, 16);
        Console.WriteLine("Worcle!");
        Console.SetCursorPosition(6, 17);
        var gameState = new GameState();
        var gameEngine = new GameEngine(gameState);
        gameEngine.Play(out var win);

        return win;
    }
}