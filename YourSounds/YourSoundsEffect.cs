public class YourSoundsEffect
{
    public virtual void Process(float[] buffer, int samplesRead) { }
    public virtual void CompleteProcess(float[] buffer, int samplesRead, int offset, int count) { }
}