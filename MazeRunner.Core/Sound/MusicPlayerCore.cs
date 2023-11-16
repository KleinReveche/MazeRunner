using NetCoreAudio;
using Reveche.MazeRunner.Serializable;

namespace Reveche.MazeRunner.Sound;

public class MusicPlayerCore : IDisposable
{
    private readonly OptionsState _optionsState;
    private readonly Player _player = new();
    
    public MusicPlayerCore(OptionsState optionsState)
    {
        _optionsState = optionsState;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        CleanUp();
    }
    
    public void Dispose()
    {
        CleanUp();
        GC.SuppressFinalize(this);
    }

    ~MusicPlayerCore()
    {
        CleanUp();
    }

    private void CleanUp()
    {
        _player.Stop();
    }
    
    public void PlaySound(Stream sound)
    {
        try
        {
            _player.Play(CreateTempAudioFile(sound)).Wait();
        }
        catch
        {
            _optionsState.IsSoundOn = false;
            _optionsState.IsSoundFxOn = false;
            OptionsManager.SaveOptions(_optionsState);
        }
    }

    private static string CreateTempAudioFile(Stream sound)
    {
        var tempFile = Path.GetTempFileName() + ".mp3";
        using var fileStream = File.OpenWrite(tempFile);
        sound.CopyTo(fileStream);
        return tempFile;
    }
}