using NAudio.Dsp;
using NAudio.Wave;

namespace TTS_Service.Services;

public class HighPassFilter : ISampleProvider
{
    private readonly BiQuadFilter filter;

    public WaveFormat WaveFormat => source.WaveFormat;

    private readonly ISampleProvider source;

    public HighPassFilter(ISampleProvider source)
    {
        this.source = source;
        filter = BiQuadFilter.HighPassFilter(source.WaveFormat.SampleRate, 1000, 0.1f);
    }

    public int Read(float[] buffer, int offset, int count)
    {
        var samplesRead = source.Read(buffer, offset, count);
        for (var i = 0; i < samplesRead; i++)
        {
            buffer[offset + i] = filter.Transform(buffer[offset + i]);
        }
        return samplesRead;
    }
}
