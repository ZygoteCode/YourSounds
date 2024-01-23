using System;

public class YourSoundsAcrusher : YourSoundsEffect
{
    int quantization;
    float compressionRatio;
    float dryMix;
    YourSoundsACrusherMethod method;

    public YourSoundsAcrusher(float dryMix = 0.1F, float compressionRatio = 1.0F, int quantization = 64, float minCompression = 0, YourSoundsACrusherMethod method = YourSoundsACrusherMethod.Standard)
    {
        this.dryMix = dryMix;
        this.compressionRatio = compressionRatio;
        this.quantization = quantization;
        this.method = method;
    }

    public override void CompleteProcess(float[] theBuffer, int samplesRead, int offset, int count)
    {
        for (int n = 0; n < samplesRead; n++)
        {
            if (method.Equals(YourSoundsACrusherMethod.Standard))
            {
                float sample = theBuffer[offset + n];
                float crushedSample = (float)Math.Floor(sample * quantization) / quantization;
                theBuffer[offset + n] = (dryMix * sample) + ((1 - dryMix) * crushedSample);
            }
            else if (method.Equals(YourSoundsACrusherMethod.Log))
            {
                float sample = theBuffer[offset + n];
                float crushedSample = (float)(Math.Sign(sample) * Math.Log(1 + Math.Abs(sample) * compressionRatio) / Math.Log(1 + compressionRatio));
                theBuffer[offset + n] = (dryMix * sample) + ((1 - dryMix) * crushedSample);
            }

            /*float originalSample = theBuffer[offset + n];
            float crushedSample = (float)Math.Floor(originalSample * (1 << 3)) / (1 << 3);
            theBuffer[offset + n] = crushedSample;*/
        }
    }
}

public enum YourSoundsACrusherMethod
{
    Standard,
    Log
}