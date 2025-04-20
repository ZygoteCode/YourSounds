using NAudio.Dsp;

public class YourSoundsEqualizer : YourSoundsEffect
{
    private BiQuadFilter[] _filters;
    private int _bandCount;

    public YourSoundsEqualizer(YourSoundsEqualizerBand[] bands)
    {
        _bandCount = bands.Length;
        _filters = new BiQuadFilter[bands.Length];

        for (int bandIndex = 0; bandIndex < _bandCount; bandIndex++)
        {
            var band = bands[bandIndex];
            _filters[bandIndex] = BiQuadFilter.PeakingEQ(44100, band.Frequency, band.Bandwidth, band.Gain);
        }
    }

    public YourSoundsEqualizer(float frequency, float gain, float bandwidth = 1.0F)
    {
        YourSoundsEqualizerBand[] bands = new YourSoundsEqualizerBand[1] { new YourSoundsEqualizerBand(frequency, gain, bandwidth) };
        _bandCount = bands.Length;
        _filters = new BiQuadFilter[bands.Length];

        for (int bandIndex = 0; bandIndex < _bandCount; bandIndex++)
        {
            var band = bands[bandIndex];
            _filters[bandIndex] = BiQuadFilter.PeakingEQ(44100, band.Frequency, band.Bandwidth, band.Gain);
        }
    }

    public override void Process(float[] buffer, int samplesRead)
    {
        for (int i = 0; i < samplesRead; i++)
        {
            for (int band = 0; band < _bandCount; band++)
            {
                buffer[i] = _filters[band].Transform(buffer[i]);
            }
        }
    }
}