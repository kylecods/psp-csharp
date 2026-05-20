// samples/AudioTest/Program.cs
// Audio synthesis demo using Psp.Audio.
//
// Generates a multi-voice synthesiser in pure C# and plays it via
// sceAudio through the PSPDNA internal call layer.
//
// Voice layout:
//   Voice 0 — sawtooth wave, root note
//   Voice 1 — sine wave,     fifth above root
//   Voice 2 — square wave,   octave above root (gated)
//
// Controls:
//   Up / Down        — raise / lower the root pitch by one semitone
//   Left / Right     — slower / faster LFO on volume
//   Triangle         — toggle voice 2 (octave gate)
//   Circle           — waveform cycle on voice 0 (sine → saw → square)
//   Square           — all notes off (silence)
//   START            — exit

using System;
using Psp;

class Program
{
    const int SampleRate  = 44100;
    const int BufferSize  = 512;     // samples per submission (must be multiple of 64)
    const int ChannelId   = -1;      // auto-assign

    // Twelve-tone equal temperament: freq = 440 * 2^((note-69)/12)
    static float NoteToHz(int midiNote)
    {
        return 440f * (float)Math.Pow(2.0, (midiNote - 69) / 12.0);
    }

    // ── Oscillator state ──────────────────────────────────────────────────
    static float s_phase0 = 0f;
    static float s_phase1 = 0f;
    static float s_phase2 = 0f;
    static float s_lfoPhase = 0f;

    enum Waveform { Sine, Saw, Square }
    static Waveform s_waveform0 = Waveform.Sine;

    static float Oscillate(Waveform w, float phase)
    {
        switch (w)
        {
            case Waveform.Sine:
                return (float)Math.Sin(phase);
            case Waveform.Saw:
                // Sawtooth: linear ramp 0→1 per cycle, centred at 0
                float norm = (phase % (2f * 3.14159265f)) / (2f * 3.14159265f);
                return norm * 2f - 1f;
            default: // Square
                return (float)Math.Sin(phase) >= 0f ? 1f : -1f;
        }
    }

    static short[] s_buf = Audio.AllocStereoBuffer(BufferSize);

    static void FillBuffer(float freq0, float freq1, float freq2,
                           float vol0, float vol1, float vol2,
                           float lfoAmt)
    {
        float step0   = 2f * 3.14159265f * freq0   / SampleRate;
        float step1   = 2f * 3.14159265f * freq1   / SampleRate;
        float step2   = 2f * 3.14159265f * freq2   / SampleRate;
        float lfoStep = 2f * 3.14159265f * 3f       / SampleRate; // 3 Hz LFO

        for (int i = 0; i < BufferSize; i++)
        {
            float lfo  = (1f - lfoAmt) + lfoAmt * (0.5f + 0.5f * (float)Math.Sin(s_lfoPhase));
            float v0   = Oscillate(s_waveform0, s_phase0) * vol0 * lfo;
            float v1   = (float)Math.Sin(s_phase1)         * vol1;
            float v2   = ((float)Math.Sin(s_phase2) >= 0f ? 1f : -1f) * vol2;

            float mix  = (v0 + v1 + v2) * 0.33f;         // normalise 3 voices
            short pcm  = (short)(mix * 22000f);           // scale to 16-bit

            s_buf[i * 2]     = pcm;  // left
            s_buf[i * 2 + 1] = pcm;  // right

            s_phase0   += step0;
            s_phase1   += step1;
            s_phase2   += step2;
            s_lfoPhase += lfoStep;
        }

        // Wrap phases
        float twoPi = 2f * 3.14159265f;
        if (s_phase0   > twoPi * 1000f) s_phase0   -= twoPi * 1000f;
        if (s_phase1   > twoPi * 1000f) s_phase1   -= twoPi * 1000f;
        if (s_phase2   > twoPi * 1000f) s_phase2   -= twoPi * 1000f;
        if (s_lfoPhase > twoPi)         s_lfoPhase -= twoPi;
    }

    static void Main()
    {
        Controls.SetSamplingMode(PspCtrlMode.Analog);
        BasicGraphics2.Init();

        int ch = Audio.ChReserve(ChannelId, BufferSize, Audio.FormatStereo);
        if (ch < 0)
        {
            BasicGraphics2.DrawText(10, 10, "Audio reserve failed: " + ch, Color.Red);
            BasicGraphics2.SwapBuffers();
            Kernel.Sleep(3000);
            Kernel.ExitGame();
            return;
        }

        // Initial state
        int  rootNote  = 60;          // middle C
        bool voice2On  = true;
        float lfoAmt   = 0.4f;
        float masterVol = 0.8f;

        while (true)
        {
            Controls.PollPad();
            Controls.PollLatch();

            if (Controls.IsKeyDown(PspCtrlButtons.Start))   break;

            // Pitch change
            if (Controls.IsKeyDown(PspCtrlButtons.Up))    rootNote = Math.Min(rootNote + 1, 84);
            if (Controls.IsKeyDown(PspCtrlButtons.Down))  rootNote = Math.Max(rootNote - 1, 36);

            // LFO depth
            if (Controls.IsKeyHeld(PspCtrlButtons.Right)) lfoAmt = Math.Min(lfoAmt + 0.01f, 1f);
            if (Controls.IsKeyHeld(PspCtrlButtons.Left))  lfoAmt = Math.Max(lfoAmt - 0.01f, 0f);

            // Voice 2 gate
            if (Controls.IsKeyDown(PspCtrlButtons.Triangle)) voice2On = !voice2On;

            // Waveform cycle
            if (Controls.IsKeyDown(PspCtrlButtons.Circle))
            {
                s_waveform0 = (Waveform)(((int)s_waveform0 + 1) % 3);
            }

            // Silence
            if (Controls.IsKeyHeld(PspCtrlButtons.Square)) masterVol = 0f;
            else                                             masterVol = 0.8f;

            // Compute note frequencies
            float f0 = NoteToHz(rootNote);           // root
            float f1 = NoteToHz(rootNote + 7);       // perfect fifth
            float f2 = NoteToHz(rootNote + 12);      // octave

            float v2 = voice2On ? masterVol * 0.5f : 0f;

            // Fill and submit
            FillBuffer(f0, f1, f2,
                       masterVol, masterVol * 0.6f, v2,
                       lfoAmt);
            Audio.OutputBlocking(ch, Audio.VolumeMax, s_buf, 0, BufferSize);

            // HUD (every frame — BasicGraphics2 draws into the GU back buffer)
            BasicGraphics2.Clear(new Color(10, 10, 30));

            string[] noteNames = {"C","C#","D","D#","E","F","F#","G","G#","A","A#","B"};
            string noteName = noteNames[rootNote % 12] + (rootNote / 12 - 1);

            BasicGraphics2.DrawText(10, 10,  "PSPDNA Audio Synthesiser",        Color.White);
            BasicGraphics2.DrawText(10, 30,  "Root: " + noteName + " (" + rootNote + ")", new Color(100,255,100));
            BasicGraphics2.DrawText(10, 50,  "Freq: " + (int)f0 + " Hz",        new Color(100,200,255));
            BasicGraphics2.DrawText(10, 70,  "Voice 0 wave: " + s_waveform0,    new Color(255,200,100));
            BasicGraphics2.DrawText(10, 90,  "Voice 2 (oct): " + (voice2On ? "ON" : "off"), new Color(200,100,255));
            BasicGraphics2.DrawText(10, 110, "LFO depth: " + (int)(lfoAmt * 100) + "%", new Color(255,150,150));

            BasicGraphics2.DrawText(10, 150, "Up/Down = pitch  Tri = oct gate",  new Color(180,180,180));
            BasicGraphics2.DrawText(10, 165, "Circle = waveform  Sq = mute",     new Color(180,180,180));
            BasicGraphics2.DrawText(10, 180, "Left/Right = LFO depth",           new Color(180,180,180));
            BasicGraphics2.DrawText(10, 200, "START = exit",                     new Color(180,180,180));

            // Volume bar
            int barW = (int)(300f * masterVol);
            BasicGraphics2.DrawRect(10, 240, barW, 12, new Color(80, 200, 80));
            BasicGraphics2.DrawText(10, 255, "Vol", new Color(200,200,200));

            BasicGraphics2.SwapBuffers();
        }

        Audio.ChRelease(ch);
        BasicGraphics2.Term();
        Kernel.ExitGame();
    }
}
