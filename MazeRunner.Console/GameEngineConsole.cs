using Reveche.MazeRunner.Classic;
using Reveche.MazeRunner.Console.Classic;

namespace Reveche.MazeRunner.Console;

public class GameEngineConsole(OptionsState optionsState, ClassicState classicState)
{
    private readonly ClassicEngine _classicEngine = new(optionsState, classicState);

    public void Play()
    {
        var classicEndless = new ConsoleClassicGame(optionsState, _classicEngine, classicState);
        switch (optionsState.GameMode)
        {
            case GameMode.Classic:
                classicEndless.Play();
                break;
            case GameMode.Endless:
                classicEndless.Play();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}