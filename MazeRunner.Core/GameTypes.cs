namespace Reveche.MazeRunner;

public static class MazeIcons
{
    public const char Wall = '*';
    public const char Border = '#';
    public const char Exit = 'E';
    public const char Enemy = 'V';
    public const char Empty = ' ';
    public const char Bomb = 'B';
    public const char Candle = 'C';
    public const char Treasure = 'T';
    public const char Fog = '@';
    public const char LostFog = '~';
    public const char Goblin = 'G';
    public const char Ogre = 'O';
    public const char Dragon = 'D';
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

public enum MazeDifficulty
{
    Easy,
    Normal,
    Hard,
    Insanity,
    AsciiInsanity
}

public enum GameMode
{
    Classic,
    Endless
}

public enum Enemy
{
    Goblin,
    Ogre,
    Dragon,
    None
}