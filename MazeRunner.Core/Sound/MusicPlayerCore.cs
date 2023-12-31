﻿using NetCoreAudio;
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

    public void Dispose()
    {
        CleanUp();
        GC.SuppressFinalize(this);
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        CleanUp();
    }

    ~MusicPlayerCore()
    {
        CleanUp();
    }

    private void CleanUp()
    {
        _player.Stop();
    }

    public void PlaySound(Stream sound, string name)
    {
        try
        {
            _player.Play(CreateTempAudioFile(sound, name)).Wait();
        }
        catch
        {
            _optionsState.IsSoundOn = false;
            _optionsState.IsSoundFxOn = false;
            OptionsManager.SaveOptions(_optionsState);
        }
    }

    private static string CreateTempAudioFile(Stream sound, string name)
    {
        if (!name.Contains(".mp3")) throw new ArgumentException("The sound must be an MP3 file.");
        
        var tempFile = Path.Join(Path.GetTempPath(), $"MazeRunner_{name}");

        if (File.Exists(tempFile)) return tempFile;

        using var fileStream = File.OpenWrite(tempFile);
        sound.CopyTo(fileStream);
        return tempFile;
    }
}