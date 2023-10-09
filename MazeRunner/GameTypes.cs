namespace Reveche.MazeRunner;

public class MazeIcons
{
    private readonly GameState _gameState;

    public MazeIcons(GameState gameState)
    {
        _gameState = gameState;
    }

    public string Wall => _gameState.IsUtf8 ? "🟪" : "*";
    public string Border => _gameState.IsUtf8 ? "🟦" : "#";
    public string Exit => _gameState.IsUtf8 ? "🚪" : "E";
    public string Enemy => _gameState.IsUtf8 ? "👾" : "V";

    public string Empty => _gameState.IsUtf8 ? "  " : " ";
    public string Bomb => _gameState.IsUtf8 ? "💣" : "B";
    public string Candle => _gameState.IsUtf8 ? "🕯️" : "C";
    public string Treasure => _gameState.IsUtf8 ? "📦" : "T";
    public string Darkness => _gameState.IsUtf8 ? "🟫" : "@";
}

public enum TreasureType
{
    Bomb,
    Candle,
    Life,
    IncreasedVisibilityEffect,
    TemporaryInvulnerabilityEffect,
    AtAGlanceEffect,
    None
}