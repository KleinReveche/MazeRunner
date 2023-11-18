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

    public void PlayFx(SoundFx soundFx)
    {
        if (!optionsState.IsSoundFxOn) return;

        var soundFxThread = new Thread(() =>
        {
            using var sound =
                typeof(GameSoundFx).Assembly.GetManifestResourceStream(ResLoc + SoundFxFiles[(int)soundFx])!;

            var player = new MusicPlayerCore(optionsState);
            player.PlaySound(sound);
        });

        soundFxThread.Start();
    }
}

public enum SoundFx
{
    BombExplode,
    BombPlace,
    ItemPickup,
    PlaceItem
}