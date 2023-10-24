using System.Reflection;
using System.Runtime.Versioning;

namespace Reveche.MazeRunner.Sound;

public class GameSoundFx(GameState gameState)
{
    private const string ResLoc = "Reveche.MazeRunner.Resources.Music.";
    private static readonly string[] SoundFxFiles = 
    {
        "BombExplode.mp3", 
        "BombPlace.mp3",
        "ItemPickup.mp3"
    };
    
    public void PlayFx(ConsoleGameSoundFx soundFx)
    {
        if (!gameState.IsSoundFxOn) return;
        var selectedSoundFx = SoundFxFiles[(int) soundFx];
        using var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResLoc + selectedSoundFx);
        var sound = new CachedSound(resourceStream!);
        AudioPlaybackEngineWindows.Instance.PlaySound(sound);
    }
}

public enum ConsoleGameSoundFx
{
    BombExplode,
    BombPlace,
    ItemPickup
}