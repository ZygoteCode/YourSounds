using NAudio.Dsp;

public class YourSoundsHighPassFilter : YourSoundsEffect
{
    private BiQuadFilter _filter;

    public YourSoundsHighPassFilter(float cutoffFrequency, float bandwidth = 1.0F)
    {
        _filter = BiQuadFilter.HighPassFilter(44100, cutoffFrequency, bandwidth);
    }

    public override void Process(float[] buffer, int samplesRead)
    {
        for (int i = 0; i < samplesRead; i++)
        {
            buffer[i] = _filter.Transform(buffer[i]);
        }
    }
}