using System.Reflection;

namespace Reveche.WorcleQuest;

public static class Game
{
    public static void Start()
    {
        var gameState = new GameState();
        InitConsole();
        
        GameEngine.Play(gameState);
    }

    private static void InitConsole()
    {
        Console.WriteLine("Welcome to Worcle!");
    }

    
}