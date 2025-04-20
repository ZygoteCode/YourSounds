using System;

public class YourSoundsBitCrusher : YourSoundsEffect
{
    int bitDepth;

    public YourSoundsBitCrusher(int bitDepth = 8)
    {
        this.bitDepth = bitDepth;
    }

    public override void CompleteProcess(float[] theBuffer, int samplesRead, int offset, int count)
    {
        for (int n = 0; n < samplesRead; n++)
        {
            float originalSample = theBuffer[offset + n];
            float crushedSample = (float)Math.Floor(originalSample * (1 << bitDepth)) / (1 << bitDepth);
            theBuffer[offset + n] = crushedSample;
        }
    }
}