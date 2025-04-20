public class YourSoundsVolumeAmplification : YourSoundsEffect
{
    private float _volume;

    public YourSoundsVolumeAmplification(float volume)
    {
        _volume = volume;
    }

    public override void Process(float[] buffer, int samplesRead)
    {
        if (_volume == 1.0F)
        {
            return;
        }

        for (int i = 0; i < samplesRead; i++)
        {
            buffer[i] *= _volume;
        }
    }
}