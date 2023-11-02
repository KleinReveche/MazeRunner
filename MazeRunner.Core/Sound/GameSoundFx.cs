using System.Reflection;

namespace Reveche.MazeRunner.Sound;

public class GameSoundFx(OptionsState optionsState)
{
    private const string ResLoc = "Reveche.MazeRunner.Resources.Music.";

    private static readonly string[] SoundFxFiles =
    {
        "BombExplode.mp3",
        "BombPlace.mp3",
        "ItemPickup.mp3",
        "PlaceItem.mp3"
    };

    public void PlayFx(ConsoleGameSoundFx soundFx)
    {
        if (!optionsState.IsSoundFxOn) return;

        var soundFxThread = new Thread(() =>
        {
            using var sound = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(ResLoc + SoundFxFiles[(int)soundFx])!;

            if (OperatingSystem.IsWindows())
                MusicPlayer.PlayInWindows(sound);
            else if (OperatingSystem.IsLinux()) MusicPlayer.PlayInLinux(sound, optionsState);
        });

        soundFxThread.Start();
    }
}

public enum ConsoleGameSoundFx
{
    BombExplode,
    BombPlace,
    ItemPickup,
    PlaceItem
}