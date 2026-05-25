namespace PspSdk.Samples;

using PspSdk.Audio;
using PspSdk.Core;

/// <summary>
/// Shows the AudioChannelHandle RAII pattern and managed PCM buffer output.
/// Demonstrates:
///   • AudioChannelHandle.Reserve — RAII, channel released on Dispose.
///   • Output(short[]) — managed array pinned transparently inside AudioModule.
///   • PspResult error checking without exceptions.
/// </summary>
public static class AudioSample
{
    public static void PlayTone()
    {
        const int SampleCount = 512;
        const int Frequency   = 440; // Hz (concert A)

        // Generate a 440 Hz sine wave as 16-bit stereo PCM.
        // short[] because PSP audio DMA expects signed 16-bit samples.
        var samples = new short[SampleCount * 2]; // stereo: L+R interleaved
        for (int i = 0; i < SampleCount; i++)
        {
            short value = (short)(Math.Sin(i * Math.PI * 2 / SampleCount * Frequency) * 16000);
            samples[i * 2]     = value; // left channel
            samples[i * 2 + 1] = value; // right channel (same for mono tone)
        }

        // RAII: channel is released automatically when the 'using' block exits —
        // even if an exception is thrown. This is the IDisposable pattern applied
        // to a native integer resource handle.
        using var channel = AudioChannelHandle.Reserve(SampleCount, AudioFormat.Stereo);
        Console.WriteLine($"Reserved audio channel {channel.Id}");

        // Play 60 frames of the tone.
        for (int frame = 0; frame < 60; frame++)
        {
            // short[] is pinned inside AudioModule.Output — no unsafe here.
            PspResult result = channel.Output(samples);
            if (result.IsError)
            {
                Console.Error.WriteLine($"Audio error: 0x{result.NativeCode:X8}");
                break;
            }
        }

        // channel.Dispose() called here → sceAudioChRelease(channel.Id)
    }
}
