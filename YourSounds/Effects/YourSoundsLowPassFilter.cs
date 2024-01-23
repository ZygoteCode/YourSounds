using NAudio.Dsp;

public class YourSoundsLowPassFilter : YourSoundsEffect
{
    private BiQuadFilter _filter;

    public YourSoundsLowPassFilter(float cutoffFrequency, float bandwidth = 1.0F)
    {
        _filter = BiQuadFilter.LowPassFilter(44100, cutoffFrequency, bandwidth);
    }

    public override void Process(float[] buffer, int samplesRead)
    {
        for (int i = 0; i < samplesRead; i++)
        {
            buffer[i] = _filter.Transform(buffer[i]);
        }
    }
}