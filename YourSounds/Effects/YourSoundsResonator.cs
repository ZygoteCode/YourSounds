using System.Windows.Forms;

public class YourSoundsResonator : YourSoundsEffect
{
    private readonly float[] reverbBuffer;
    private int reverbBufferIndex;
    private readonly float reverbDelay;
    private readonly float reverbDecay;

    public YourSoundsResonator(float reverbDelay = 3000, float reverbDecay = 0.8F)
    {
        this.reverbDelay = reverbDelay;
        this.reverbDecay = reverbDecay;
        this.reverbBuffer = new float[(int) reverbDelay];
    }

    public override void CompleteProcess(float[] buffer, int samplesRead, int offset, int count)
    {
        for (int i = 0; i < samplesRead; i++)
        {
            float reverbSample = reverbBuffer[reverbBufferIndex];
            buffer[i + offset] += reverbSample;
            reverbBuffer[reverbBufferIndex] = buffer[i + offset] * reverbDecay;

            reverbBufferIndex++;
            if (reverbBufferIndex >= reverbDelay)
            {
                reverbBufferIndex = 0;
            }
        }
    }
}