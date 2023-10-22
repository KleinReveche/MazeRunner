using Reveche.MazeRunner.Console.Screens;

namespace Reveche.MazeRunner.Console;

public class GameEngineConsole(GameState gameState)
{
    private readonly GameEngine _gameEngine = new (gameState);

    public void Play()
    {
        var classicEndless = new ConsoleClassicGame(_gameEngine, gameState);
        switch (gameState.GameMode)
        {
            case GameMode.Classic:
                classicEndless.Play();
                break;
            case GameMode.Campaign:
                System.Console.WriteLine("Campaign mode is not yet implemented. Stay Tuned!");
                break;
            case GameMode.Endless:
                classicEndless.Play();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        System.Console.ReadKey();
        MainScreen.StartMenu();
    }
}