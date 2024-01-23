using NAudio.Wave;
using System.Collections.Generic;
using System.Windows.Forms;

public class YourSoundsWaveProvider : ISampleProvider
{
    private readonly ISampleProvider sourceProvider;
    private readonly List<YourSoundsEffect> effects = new List<YourSoundsEffect>();
    public WaveFormat WaveFormat => sourceProvider.WaveFormat;

    public YourSoundsWaveProvider(ISampleProvider sourceProvider)
    {
        this.sourceProvider = sourceProvider;
    }

    public int Read(float[] buffer, int offset, int count)
    {
        int samplesRead = sourceProvider.Read(buffer, offset, count);

        foreach (YourSoundsEffect effect in effects)
        {
            effect.Process(buffer, samplesRead);
            effect.CompleteProcess(buffer, samplesRead, offset, count);
        }

        return samplesRead;
    }

    public void AddEffect(YourSoundsEffect effect)
    {
        effects.Add(effect);
    }

    public void ClearEffects()
    {
        effects.Clear();
    }
}