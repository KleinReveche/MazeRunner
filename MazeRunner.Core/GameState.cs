namespace Reveche.MazeRunner;

public class GameState
{
    public GameState()
    {
        var options = OptionsManager.LoadOptions();

        GameMode = options.GameMode;
        IsSoundOn = options.IsSoundOn;
        IsUtf8 = options.IsUtf8;
        MazeDifficulty = options.MazeDifficulty;
    }

    public bool IsSoundOn { get; set; }
    public bool IsUtf8 { get; set; }
    public MazeDifficulty MazeDifficulty { get; set; }
    public GameMode GameMode { get; set; }
    public bool IsCurrentlyPlaying { get; set; }
    public bool IsCampaignOngoing { get; set; }
}