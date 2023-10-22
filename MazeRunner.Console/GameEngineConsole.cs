using Reveche.MazeRunner.Console.Screens;

namespace Reveche.MazeRunner.Console;

public class GameEngineConsole
{
    private readonly GameEngine _gameEngine;
    private readonly GameState _gameState;

    public GameEngineConsole(GameState gameState)
    {
        _gameState = gameState;
        _gameEngine = new GameEngine(_gameState);
    }

    public void Play()
    {
        var classicEndless = new ConsoleClassicGame(_gameEngine, _gameState);
        switch (_gameState.GameMode)
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