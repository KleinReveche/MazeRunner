using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Reveche.MazeRunner.Sound;

// This was made by Mark Heath at https://markheath.net/post/fire-and-forget-audio-playback-with
// with my modifications
public class AudioPlaybackEngine : IDisposable
{
    public static readonly AudioPlaybackEngine Instance = new();
    private readonly MixingSampleProvider _mixer;
    private readonly List<ISampleProvider> _inputs = new();

    private AudioPlaybackEngine(int sampleRate = 44100, int channelCount = 2)
    {
        var outputDevice = new WaveOutEvent();
        _mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount))
        {
            ReadFully = true
        };
        outputDevice.Init(_mixer);
        outputDevice.Play();
    }

    public void Dispose()
    {
        foreach (var input in _inputs)
        {
            if (input is IDisposable disposable) disposable.Dispose();
        }
        GC.SuppressFinalize(this);
    }

    public void RemoveAll()
    {
        _mixer.RemoveAllMixerInputs();
        _inputs.Clear();
    }
    public void RemoveLast()
    {
        if (_inputs.Count == 0) return;
        var last = _inputs.Last();
        _mixer.RemoveMixerInput(last);
        _inputs.Remove(last);
    }

    public void PlaySound(string fileName)
    {
        var input = new AudioFileReader(fileName);
        _inputs.Add(input);
        AddMixerInput(new AutoDisposeFileReader(input));
    }

    private ISampleProvider ConvertToRightChannelCount(ISampleProvider input)
    {
        if (input.WaveFormat.Channels == _mixer.WaveFormat.Channels) return input;
        if (input.WaveFormat.Channels == 1 && _mixer.WaveFormat.Channels == 2)
            return new MonoToStereoSampleProvider(input);
        throw new NotImplementedException("Not yet implemented this channel count conversion");
    }

    public void PlaySound(CachedSound sound)
    {
        var input = new CachedSoundSampleProvider(sound);
        _inputs.Add(input);
        AddMixerInput(input);
    }

    private void AddMixerInput(ISampleProvider input)
    {
        _mixer.AddMixerInput(ConvertToRightChannelCount(input));
    }
}

public class CachedSound
{
    public CachedSound(Stream sound)
    {
        using var audioFileReader = new Mp3FileReader(sound);
        WaveFormat = audioFileReader.Mp3WaveFormat;
        var sp = audioFileReader.ToSampleProvider();
        var wholeFile = new List<float>((int)(audioFileReader.Length / 4));
        var sourceSamples = (int)(audioFileReader.Length / (audioFileReader.WaveFormat.BitsPerSample / 8));
        var sampleData = new float[sourceSamples];
        int samplesRead;
        while ((samplesRead = sp.Read(sampleData, 0, sourceSamples)) > 0)
        {
            wholeFile.AddRange(sampleData.Take(samplesRead));
        }
        AudioData = wholeFile.ToArray();
    }

    public float[] AudioData { get; }
    public WaveFormat WaveFormat { get; }
}

internal class CachedSoundSampleProvider(CachedSound sound) : ISampleProvider
{
    private long _position;

    public int Read(float[] buffer, int offset, int count)
    {
        var availableSamples = sound.AudioData.Length - _position;
        var samplesToCopy = Math.Min(availableSamples, count);
        Array.Copy(sound.AudioData, _position, buffer, offset, samplesToCopy);
        _position += samplesToCopy;
        return (int)samplesToCopy;
    }

    public WaveFormat WaveFormat => sound.WaveFormat;
}

internal class AutoDisposeFileReader(AudioFileReader reader) : ISampleProvider
{
    private bool _isDisposed;

    public int Read(float[] buffer, int offset, int count)
    {
        if (_isDisposed)
            return 0;
        var read = reader.Read(buffer, offset, count);
        if (read != 0) return read;
        reader.Dispose();
        _isDisposed = true;

        return read;
    }

    public WaveFormat WaveFormat { get; } = reader.WaveFormat;
}