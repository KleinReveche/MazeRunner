using System.Reflection;

namespace Reveche.WorcleQuest;

public static class Game
{
    public static void Start()
    {
        var gameState = new GameState();
        InitConsole();

        string guess;
        
        while (true)
        {
            guess = Console.ReadLine() ?? string.Empty;
            if (guess != string.Empty && guess.Length == 5) break;
        }

        gameState.Guess = guess.ToCharArray();
        
        GameEngine.Play(gameState);
    }

    private static void InitConsole()
    {
        Console.WriteLine("Welcome to Worcle!");
    }

    
}