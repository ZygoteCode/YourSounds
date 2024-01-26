using NAudio.Wave;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Forms;
using System;
using NAudio.Wave.SampleProviders;
using Guna.UI2.WinForms;

public partial class MainForm : MetroSuite.MetroForm
{
    private string[] files = new string[]
    {
        "data",
        "data\\sounds",
        "data\\utils",
        "data\\output_devices.txt",
        "data\\volume.txt"
    };

    private List<WaveOutEvent> events = new List<WaveOutEvent>();
    private List<int> deviceNumbers = new List<int>();
    private List<AudioFileReader> readers = new List<AudioFileReader>();

    private List<VolumeSampleProvider> volumeSampleProviders = new List<VolumeSampleProvider>();
    private List<YourSoundsWaveProvider> providers = new List<YourSoundsWaveProvider>();

    public MainForm()
    {
        InitializeComponent();
        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
        CheckForIllegalCrossThreadCalls = false;
        TriggerFileChecking();

        for (int waveOutDevice = 0; waveOutDevice < WaveOut.DeviceCount; waveOutDevice++)
        {
            guna2ComboBox1.Items.Add(WaveOut.GetCapabilities(waveOutDevice).ProductName);
        }

        guna2ComboBox1.SelectedIndex = 0;

        Thread updateAllThread = new Thread(UpdateAll);
        updateAllThread.Priority = ThreadPriority.Highest;
        updateAllThread.Start();

        UpdateSounds();

        foreach (string line in File.ReadAllLines("data\\output_devices.txt"))
        {
            for (int waveOutDevice = 0; waveOutDevice < WaveOut.DeviceCount; waveOutDevice++)
            {
                if (WaveOut.GetCapabilities(waveOutDevice).ProductName == line)
                {
                    listBox1.Items.Add(line);
                    break;
                }
            }
        }

        UpdateDevices();
        string volumeStr = File.ReadAllText("data\\volume.txt");

        if (volumeStr == "")
        {
            volumeStr = "1";
        }

        File.WriteAllText("data\\volume.txt", volumeStr);
        guna2TrackBar1.Value = int.Parse(volumeStr);
        label1.Text = $"Current volume level: {guna2TrackBar1.Value}%";
        guna2ComboBox2.SelectedIndex = 0;
        guna2ComboBox3.SelectedIndex = 0;
        guna2ComboBox4.SelectedIndex = 0;

        foreach (Control control in Controls)
        {
            control.AllowDrop = true;

            control.DragEnter += (s, e) =>
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    e.Effect = DragDropEffects.Copy;
                }
            };

            control.DragDrop += (s, e) =>
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] files = ((string[])e.Data.GetData(DataFormats.FileDrop));

                    foreach (string file in files)
                    {
                        string filePath = Path.GetFullPath(file);

                        if (!IsMediaFileValid(filePath))
                        {
                            continue;
                        }

                        CreateAudioFile(filePath);
                    }

                    UpdateSounds();
                }
            };
        }
    }

    private void ClearEffects()
    {
        foreach (YourSoundsWaveProvider provider in providers)
        {
            provider.ClearEffects();
        }
    }

    private void AddEffect(YourSoundsEffect effect)
    {
        foreach (YourSoundsWaveProvider provider in providers)
        {
            provider.AddEffect(effect);
        }
    }

    private float ParseFloat(Guna2TextBox textBox)
    {
        return float.Parse(textBox.Text.Replace(".", ","));
    }

    private void ApplyCurrentEffects()
    {
        ClearEffects();
        List<YourSoundsEffect> effects = new List<YourSoundsEffect>();

        try
        {
            if (guna2CheckBox1.Checked)
            {
                effects.Add(new YourSoundsHighPassFilter(ParseFloat(guna2TextBox1)));
            }
        }
        catch
        {

        }

        try
        {
            if (guna2CheckBox2.Checked)
            {
                effects.Add(new YourSoundsLowPassFilter(ParseFloat(guna2TextBox2)));
            }
        }
        catch
        {

        }

        try
        {
            if (guna2CheckBox3.Checked)
            {
                effects.Add(new YourSoundsNotchFilter(ParseFloat(guna2TextBox3)));
            }
        }
        catch
        {

        }

        try
        {
            if (guna2ComboBox2.SelectedIndex != 0)
            {
                YourSoundsDistortionMethod method = YourSoundsDistortionMethod.Method1;

                if (guna2ComboBox2.SelectedIndex == 1)
                {
                    method = YourSoundsDistortionMethod.Method1;
                }
                else if (guna2ComboBox2.SelectedIndex == 1)
                {
                    method = YourSoundsDistortionMethod.Method2;
                }
                else if (guna2ComboBox2.SelectedIndex == 2)
                {
                    method = YourSoundsDistortionMethod.Method3;
                }
                else if (guna2ComboBox2.SelectedIndex == 3)
                {
                    method = YourSoundsDistortionMethod.Method4;
                }

                effects.Add(new YourSoundsDistortion(ParseFloat(guna2TextBox4), method));
            }
        }
        catch
        {

        }

        try
        {
            if (guna2CheckBox4.Checked)
            {
                effects.Add(new YourSoundsSuperEqualizer(ParseFloat(guna2TextBox5), ParseFloat(guna2TextBox6), ParseFloat(guna2TextBox8), ParseFloat(guna2TextBox7), ParseFloat(guna2TextBox10), ParseFloat(guna2TextBox9)));
            }
        }
        catch
        {

        }

        try
        {
            if (guna2ComboBox3.SelectedIndex != 0)
            {
                bool bassBoost = false, freshAir = false;
                List<YourSoundsEqualizerBand> bands = new List<YourSoundsEqualizerBand>();

                if (guna2ComboBox3.SelectedIndex == 1)
                {
                    bassBoost = true;
                }
                else if (guna2ComboBox3.SelectedIndex == 2)
                {
                    freshAir = true;
                }
                else if (guna2ComboBox3.SelectedIndex == 3)
                {
                    bassBoost = true;
                    freshAir = true;
                }

                if (bassBoost)
                {
                    bands.Add(new YourSoundsEqualizerBand(120, ParseFloat(guna2TextBox11), 1.2F));
                }

                if (freshAir)
                {
                    bands.Add(new YourSoundsEqualizerBand(6000, ParseFloat(guna2TextBox11), 4.0F));
                }

                effects.Add(new YourSoundsEqualizer(bands.ToArray()));
            }
        }
        catch
        {

        }

        try
        {
            if (guna2CheckBox5.Checked)
            {
                effects.Add(new YourSoundsSimpleCompressor(ParseFloat(guna2TextBox17), ParseFloat(guna2TextBox16), ParseFloat(guna2TextBox15), ParseFloat(guna2TextBox14), ParseFloat(guna2TextBox13)));
            }
        }
        catch
        {

        }

        try
        {
            if (guna2CheckBox6.Checked)
            {
                effects.Add(new YourSoundsTremolo(ParseFloat(guna2TextBox12)));
            }
        }
        catch
        { 
        
        }

        try
        {
            if (guna2CheckBox7.Checked)
            {
                effects.Add(new YourSoundsDelay(delayMs: ParseFloat(guna2TextBox18)));
            }
        }
        catch
        {

        }

        try
        {
            if (guna2CheckBox8.Checked)
            {
                effects.Add(new YourSoundsDelayPong(delayMs: ParseFloat(guna2TextBox19)));
            }
        }
        catch
        {

        }

        try
        {
            if (guna2CheckBox9.Checked)
            {
                effects.Add(new YourSoundsFlanger(lengthMs: ParseFloat(guna2TextBox20)));
            }
        }
        catch
        {

        }

        try
        {
            if (guna2CheckBox10.Checked)
            {
                effects.Add(new YourSoundsChorus(chorusLengthMs: ParseFloat(guna2TextBox21), numberOfVoices: ParseFloat(guna2TextBox22)));
            }
        }
        catch
        {

        }

        try
        {
            if (guna2CheckBox11.Checked)
            {
                effects.Add(new YourSoundsResonator(ParseFloat(guna2TextBox23), ParseFloat(guna2TextBox24)));
            }
        }
        catch
        {

        }

        try
        {
            if (guna2CheckBox12.Checked)
            {
                effects.Add(new YourSoundsPitchShifter(ParseFloat(guna2TextBox26)));
            }
        }
        catch
        {

        }

        try
        {
            if (guna2ComboBox4.SelectedIndex != 0)
            {
                if (guna2ComboBox4.SelectedIndex == 1)
                {
                    effects.Add(new YourSoundsAcrusher());
                }
                else if (guna2ComboBox4.SelectedIndex == 2)
                {
                    effects.Add(new YourSoundsBitCrusher((int)(ParseFloat(guna2TextBox25))));
                }
            }
        }
        catch
        {

        }

        foreach (YourSoundsEffect effect in effects)
        {
            AddEffect(effect);
        }
    }

    private void TriggerFileChecking()
    {
        foreach (string file in files)
        {
            if (file.Contains("."))
            {
                if (!File.Exists(file))
                {
                    File.Create(file).Close();
                }
            }
            else
            {
                if (!Directory.Exists(file))
                {
                    Directory.CreateDirectory(file);
                }
            }
        }
    }

    private void RemoveSelectedItemsFromListBox(ListBox listBox, bool deleteFiles = false)
    {
        List<string> items = new List<string>();

        foreach (string item in listBox.SelectedItems)
        {
            items.Add(item);
        }

        foreach (string item in items)
        {
            listBox.Items.Remove(item);
        }

        if (deleteFiles)
        {
            foreach (string item in items)
            {
                File.Delete($"data\\sounds\\{item}");
            }
        }
    }

    private void ClearAllDevices()
    {
        ClearListBox(listBox1);
        UpdateDevices();
    }

    private void UpdateDevices()
    {
        string content = "";
        deviceNumbers.Clear();

        foreach (string item in listBox1.Items)
        {
            if (content == "")
            {
                content = item;
            }
            else
            {
                content += $"\r\n{item}";
            }


            for (int waveOutDevice = 0; waveOutDevice < WaveOut.DeviceCount; waveOutDevice++)
            {
                if (WaveOut.GetCapabilities(waveOutDevice).ProductName == item)
                {
                    deviceNumbers.Add(waveOutDevice);
                    break;
                }
            }
        }

        TriggerFileChecking();
        File.WriteAllText("data\\output_devices.txt", content);
    }

    private void UpdateAll()
    {
        while (true)
        {
            Thread.Sleep(1);
            guna2Button1.Enabled = !listBox1.Items.Contains(guna2ComboBox1.SelectedItem);
        }
    }

    private string[] GetSelectedSounds()
    {
        List<string> sounds = new List<string>();

        foreach (string sound in listBox2.SelectedItems)
        {
            sounds.Add($"{Path.GetFullPath("data\\sounds")}\\{sound}");
        }

        return sounds.ToArray();
    }

    private string[] GetAllSounds()
    {
        List<string> sounds = new List<string>();

        foreach (string sound in listBox2.Items)
        {
            sounds.Add($"{Path.GetFullPath("data\\sounds")}\\{sound}");
        }

        return sounds.ToArray();
    }

    private float GetVolume()
    {
        return (float)((float)guna2TrackBar1.Value / 100.0F);
    }

    private void PlaySelectedSounds()
    {
        foreach (string sound in GetSelectedSounds())
        {
            foreach (int deviceNumber in deviceNumbers)
            {
                try
                {
                    WaveOutEvent waveOutEvent = new WaveOutEvent() { DeviceNumber = deviceNumber };
                    AudioFileReader audioFileReader = new AudioFileReader(sound);
                    readers.Add(audioFileReader);

                    VolumeSampleProvider volumeProvider = new VolumeSampleProvider(audioFileReader);
                    volumeProvider.Volume = GetVolume();
                    YourSoundsWaveProvider provider = new YourSoundsWaveProvider(volumeProvider);

                    providers.Add(provider);
                    volumeSampleProviders.Add(volumeProvider);

                    waveOutEvent.Init(provider);
                    events.Add(waveOutEvent);
                    waveOutEvent.Play();
                }
                catch
                {

                }
            }
        }
    }

    private void PlayAllSounds()
    {
        foreach (string sound in GetAllSounds())
        {
            foreach (int deviceNumber in deviceNumbers)
            {
                try
                {
                    WaveOutEvent waveOutEvent = new WaveOutEvent() { DeviceNumber = deviceNumber };
                    AudioFileReader audioFileReader = new AudioFileReader(sound);
                    readers.Add(audioFileReader);

                    VolumeSampleProvider volumeProvider = new VolumeSampleProvider(audioFileReader);
                    volumeProvider.Volume = GetVolume();
                    YourSoundsWaveProvider provider = new YourSoundsWaveProvider(volumeProvider);

                    providers.Add(provider);
                    volumeSampleProviders.Add(volumeProvider);

                    waveOutEvent.Init(new SampleToWaveProvider(provider));
                    events.Add(waveOutEvent);
                    waveOutEvent.Play();
                }
                catch
                {

                }
            }
        }
    }

    private void StopAllSounds()
    {
        foreach (WaveOutEvent waveOutEvent in events)
        {
            try
            {
                waveOutEvent.Stop();
            }
            catch
            {

            }

            try
            {
                waveOutEvent.Dispose();
            }
            catch
            {

            }
        }

        foreach (AudioFileReader reader in readers)
        {
            try
            {
                reader.Close();
            }
            catch
            {

            }

            try
            {
                reader.Dispose();
            }
            catch
            {

            }
        }

        events.Clear();
        readers.Clear();
        volumeSampleProviders.Clear();
        providers.Clear();
    }

    private void PauseAllSounds()
    {
        foreach (WaveOutEvent waveOutEvent in events)
        {
            try
            {
                waveOutEvent.Pause();
            }
            catch
            {

            }
        }
    }

    private void ResumeAllSounds()
    {
        foreach (WaveOutEvent waveOutEvent in events)
        {
            try
            {
                waveOutEvent.Play();
            }
            catch
            {

            }
        }
    }

    private bool IsMediaFileValid(string filePath)
    {
        TriggerFileChecking();
        string fileName = $"C:\\verification{Path.GetExtension(filePath)}";
        
        while (true)
        {
            try
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                File.Copy(filePath, fileName);
                break;
            }
            catch
            {

            }
        }

        Process process = Process.Start(new ProcessStartInfo
        {
            FileName = Path.GetFullPath("data\\utils\\ffprobe.exe"),
            WorkingDirectory = Path.GetFullPath("data\\utils"),
            Arguments = $"-v error -select_streams a:0 -show_entries stream=codec_type -of default=noprint_wrappers=1 -i \"{fileName}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        });

        process.WaitForExit();

        while (true)
        {
            try
            {
                File.Delete(fileName);
                break;
            }
            catch
            {

            }
        }

        return process.ExitCode == 0 && process.StandardOutput.ReadToEnd().Contains("codec_type=audio");
    }

    private void CreateAudioFile(string filePath)
    {
        TriggerFileChecking();
        string outputPath = Path.GetFullPath("data\\sounds");
        string fileName = $"{Path.GetFileNameWithoutExtension(filePath)}.wav";
        int counter = 1;

        while (File.Exists($"data\\sounds\\{fileName}"))
        {
            fileName = $"{counter}-{Path.GetFileNameWithoutExtension(filePath)}.wav";
            counter++;
        }

        outputPath += $"\\{fileName}";

        Process.Start(new ProcessStartInfo
        {
            FileName = Path.GetFullPath("data\\utils\\ffmpeg.exe"),
            WorkingDirectory = Path.GetFullPath("data\\utils"),
            Arguments = $"-threads {Environment.ProcessorCount} -i \"{filePath}\" -c:a pcm_s16le -ar 44100 -ac 2 \"{outputPath}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        }).WaitForExit();

        while (!File.Exists(outputPath))
        {
            Thread.Sleep(1);
        }

        while (!IsMediaFileValid(outputPath))
        {
            Thread.Sleep(1);
        }
    }

    private void AddNewSounds()
    {
        if (!openFileDialog1.ShowDialog().Equals(DialogResult.OK))
        {
            return;
        }

        foreach (string file in openFileDialog1.FileNames)
        {
            string filePath = Path.GetFullPath(file);

            if (!IsMediaFileValid(filePath))
            {
                continue;
            }

            CreateAudioFile(filePath);
        }

        UpdateSounds();
    }

    private void UpdateSounds()
    {
        listBox2.Items.Clear();

        foreach (string file in Directory.GetFiles("data\\sounds"))
        {
            listBox2.Items.Add(Path.GetFileName(file));
        }
    }

    private void MainForm_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
    {
        Process.GetCurrentProcess().Kill();
    }

    private void guna2Button5_Click(object sender, System.EventArgs e)
    {
        PlaySelectedSounds();
        ApplyCurrentEffects();
    }

    private void listBox2_DoubleClick(object sender, System.EventArgs e)
    {
        PlaySelectedSounds();
        ApplyCurrentEffects();
    }

    private void guna2Button2_Click(object sender, System.EventArgs e)
    {
        AddNewSounds();
    }

    private void guna2Button3_Click(object sender, System.EventArgs e)
    {
        RemoveSelectedItemsFromListBox(listBox2, true);
    }

    private void guna2Button4_Click(object sender, System.EventArgs e)
    {
        try
        {
            Directory.Delete("data\\sounds", true);
            Directory.CreateDirectory("data\\sounds");
            ClearListBox(listBox2);
        }
        catch
        {

        }
    }

    private void guna2Button1_Click(object sender, System.EventArgs e)
    {
        if (listBox1.Items.Contains(guna2ComboBox1.SelectedItem))
        {
            return;
        }

        listBox1.Items.Add(guna2ComboBox1.SelectedItem);
        UpdateDevices();
    }

    private void guna2Button6_Click(object sender, System.EventArgs e)
    {
        RemoveSelectedItemsFromListBox(listBox1);
        UpdateDevices();
    }

    private void guna2Button7_Click(object sender, System.EventArgs e)
    {
        ClearAllDevices();
    }

    private void ClearListBox(ListBox listBox)
    {
        listBox.Items.Clear();
    }

    private void listBox1_DoubleClick(object sender, System.EventArgs e)
    {
        ClearAllDevices();
    }

    private void guna2Button8_Click(object sender, System.EventArgs e)
    {
        StopAllSounds();
    }

    private void guna2Button9_Click(object sender, System.EventArgs e)
    {
        PauseAllSounds();
    }

    private void guna2Button10_Click(object sender, System.EventArgs e)
    {
        ResumeAllSounds();
    }

    private void guna2Button11_Click(object sender, EventArgs e)
    {
        PlayAllSounds();
        ApplyCurrentEffects();
    }

    private void guna2TrackBar1_Scroll(object sender, ScrollEventArgs e)
    {
        TriggerFileChecking();
        label1.Text = $"Current volume level: {guna2TrackBar1.Value}%";
        File.WriteAllText("data\\volume.txt", guna2TrackBar1.Value.ToString());

        foreach (VolumeSampleProvider volumeSampleProvider in volumeSampleProviders)
        {
            try
            {
                volumeSampleProvider.Volume = GetVolume();
            }
            catch
            {

            }
        }
    }

    private void guna2Button12_Click(object sender, EventArgs e)
    {
        ApplyCurrentEffects();
    }
}