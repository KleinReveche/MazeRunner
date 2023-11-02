using Alsa.Net;
using NAudio.Wave;
using NLayer.NAudioSupport;
using Reveche.MazeRunner.Serializable;

namespace Reveche.MazeRunner.Sound;

public class MusicPlayerLinux(OptionsState optionsState) : IDisposable
{
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public void PlaySound(Stream sound)
    {
        var waveStream = ConvertToWaveFormat(sound);

        try
        {
            using var alsaDevice = AlsaDeviceBuilder.Create(new SoundDeviceSettings());
            alsaDevice.Play(waveStream);
        }
        catch
        {
            optionsState.IsSoundOn = false;
            optionsState.IsSoundFxOn = false;
            OptionsManager.SaveOptions(optionsState);
        }
    }

    private static MemoryStream ConvertToWaveFormat(Stream stream)
    {
        var outfile = new MemoryStream();
        using var reader = new Mp3FileReaderBase(stream, wf => new Mp3FrameDecompressor(wf));
        WaveFileWriter.WriteWavFileToStream(outfile, reader);

        return outfile;
    }
}