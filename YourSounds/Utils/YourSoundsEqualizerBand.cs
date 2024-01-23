public class YourSoundsEqualizerBand
{
    public float Frequency { get; set; }
    public float Gain { get; set; }
    public float Bandwidth { get; set; }

    public YourSoundsEqualizerBand(float frequency, float gain, float bandwidth = 1.0F)
    {
        Frequency = frequency;
        Gain = gain;
        Bandwidth = bandwidth;
    }
}