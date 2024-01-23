using NAudio.Dsp;
using NAudio.Wave;
using System;

public class YourSoundsPitchShifter : YourSoundsEffect
{
    private float pitch = 1f;
    private readonly int fftSize;
    private readonly long osamp;
    private readonly SmbPitchShifter shifterLeft = new SmbPitchShifter();
    private readonly SmbPitchShifter shifterRight = new SmbPitchShifter();

    //Limiter constants
    const float LIM_THRESH = 0.95f;
    const float LIM_RANGE = (1f - LIM_THRESH);
    const float M_PI_2 = (float)(Math.PI / 2);

    /// <summary>
    /// Creates a new SMB Pitch Shifting Sample Provider with default settings
    /// </summary>
    /// <param name="sourceProvider">Source provider</param>
    public YourSoundsPitchShifter(float pitch = 1.0F)
        : this(4096, 4L, pitch)
    {
    }

    /// <summary>
    /// Creates a new SMB Pitch Shifting Sample Provider with custom settings
    /// </summary>
    /// <param name="sourceProvider">Source provider</param>
    /// <param name="fftSize">FFT Size (any power of two &lt;= 4096: 4096, 2048, 1024, 512, ...)</param>
    /// <param name="osamp">Oversampling (number of overlapping windows)</param>
    /// <param name="initialPitch">Initial pitch (0.5f = octave down, 1.0f = normal, 2.0f = octave up)</param>
    public YourSoundsPitchShifter(int fftSize, long osamp, float initialPitch)
    {
        this.fftSize = fftSize;
        this.osamp = osamp;
        PitchFactor = initialPitch;
    }

    /// <summary>
    /// Read from this sample provider
    /// </summary>
    public override void CompleteProcess(float[] buffer, int samplesRead, int offset, int count)
    {
        if (pitch == 1f)
        {
            return;
        }

        var left = new float[(samplesRead >> 1)];
        var right = new float[(samplesRead >> 1)];
        var index = 0;
        for (var sample = offset; sample <= samplesRead + offset - 1; sample += 2)
        {
            left[index] = buffer[sample];
            right[index] = buffer[sample + 1];
            index += 1;
        }
        shifterLeft.PitchShift(pitch, samplesRead >> 1, fftSize, osamp, 44100, left);
        shifterRight.PitchShift(pitch, samplesRead >> 1, fftSize, osamp, 44100, right);
        index = 0;
        for (var sample = offset; sample <= samplesRead + offset - 1; sample += 2)
        {
            buffer[sample] = Limiter(left[index]);
            buffer[sample + 1] = Limiter(right[index]);
            index += 1;
        }
    }

    /// <summary>
    /// WaveFormat
    /// </summary>

    /// <summary>
    /// Pitch Factor (0.5f = octave down, 1.0f = normal, 2.0f = octave up)
    /// </summary>
    public float PitchFactor
    {
        get { return pitch; }
        set { pitch = value; }
    }

    private float Limiter(float sample)
    {
        float res;
        if ((LIM_THRESH < sample))
        {
            res = (sample - LIM_THRESH) / LIM_RANGE;
            res = (float)((Math.Atan(res) / M_PI_2) * LIM_RANGE + LIM_THRESH);
        }
        else if ((sample < -LIM_THRESH))
        {
            res = -(sample + LIM_THRESH) / LIM_RANGE;
            res = -(float)((Math.Atan(res) / M_PI_2) * LIM_RANGE + LIM_THRESH);
        }
        else
        {
            res = sample;
        }
        return res;
    }
}