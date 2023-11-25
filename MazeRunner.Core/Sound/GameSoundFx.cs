namespace Reveche.MazeRunner.Sound;

public class GameSoundFx(OptionsState optionsState)
{
    private const string ResLoc = "Reveche.MazeRunner.Resources.Music.";
    public void PlayFx(SoundFx soundFx)
    {
        if (!optionsState.IsSoundFxOn) return;

        var soundFxThread = new Thread(() =>
        {
            using var sound =
                typeof(GameSoundFx).Assembly.GetManifestResourceStream(ResLoc + soundFx + ".mp3")!;

            var player = new MusicPlayerCore(optionsState);
            player.PlaySound(sound, soundFx + ".mp3");
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