using Reveche.MazeRunner.Classic;
using Reveche.MazeRunner.Console.Classic;
using Reveche.MazeRunner.Console.Screens;

namespace Reveche.MazeRunner.Console;

public class GameEngineConsole(GameState gameState, ClassicState classicState)
{
    private readonly ClassicEngine _classicEngine = new(classicState);

    public void Play()
    {
        var classicEndless = new ConsoleClassicGame(gameState, _classicEngine, classicState);
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