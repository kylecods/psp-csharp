// Psp/Audio.cs
// Wraps sceAudio* via internal calls.
//
// The PSP audio hardware has 8 PCM channels (0–7).  Each channel holds
// one buffer of samples that plays through the speaker or headphone jack.
//
// Typical stereo usage:
//   int ch = Audio.ChReserve(-1, 512, Audio.FormatStereo);
//   // fill short[] buffer with 512 stereo pairs
//   Audio.OutputBlocking(ch, Audio.VolumeMax, buf, 0, 512);
//   Audio.ChRelease(ch);
//
// Buffer alignment:
//   PSP DMA requires audio buffers to be 64-byte aligned.
//   In DNA, declare buffers as short[] at class level (not local) so
//   the GC can align them; or use the AlignedBuffer helper below.

using System.Runtime.CompilerServices;

namespace Psp
{
    public static class Audio
    {
        // ── Constants ─────────────────────────────────────────────────────

        /// <summary>Stereo channel format (left + right samples interleaved).</summary>
        public const int FormatStereo = 0x00;

        /// <summary>Mono channel format.</summary>
        public const int FormatMono   = 0x10;

        /// <summary>Maximum volume for OutputBlocking / Output (0x8000 = 32768).</summary>
        public const int VolumeMax    = 0x8000;

        /// <summary>
        /// Auto-assign a channel ID.  Pass to ChReserve as the channel argument.
        /// </summary>
        public const int AutoChannel  = -1;

        // ── Channel lifecycle ─────────────────────────────────────────────

        /// <summary>
        /// Reserve an audio channel.
        /// Returns the channel ID (>= 0) on success, or a negative PSP error code.
        ///
        /// channel     — 0–7 for a specific channel, or AutoChannel (-1) to pick one.
        /// sampleCount — samples per buffer submission. Must be a multiple of 64
        ///               and in the range [64, 65472].
        /// format      — FormatStereo or FormatMono.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int ChReserve(int channel, int sampleCount, int format);

        /// <summary>
        /// Release a previously reserved channel so it can be reused.
        /// Returns 0 on success, negative on error.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int ChRelease(int channel);

        // ── Playback ──────────────────────────────────────────────────────

        /// <summary>
        /// Submit a buffer for playback and BLOCK until it has finished playing.
        /// This is the simplest and most reliable playback method.
        ///
        /// channel — channel ID returned by ChReserve.
        /// vol     — volume 0–VolumeMax.
        /// buf     — short[] containing PCM samples.
        ///           Stereo: samples are interleaved [L0, R0, L1, R1, …].
        ///           Mono:   samples are [S0, S1, S2, …].
        /// offset  — index into buf where audio data starts.
        /// count   — number of stereo pairs (or mono samples) to play.
        ///           Must match the sampleCount passed to ChReserve.
        ///
        /// Internally calls sceAudioOutputBlocking() with the pinned buffer pointer.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void OutputBlocking(int channel, int vol, short[] buf, int offset, int count);

        /// <summary>
        /// Submit a buffer for playback without blocking.
        /// Returns immediately; the previous buffer must have finished before calling again.
        /// Check remaining samples with GetChannelRestLen().
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Output(int channel, int vol, short[] buf, int offset, int count);

        /// <summary>
        /// Submit with independent left and right volumes (stereo pan).
        /// leftVol and rightVol are each in the range 0–VolumeMax.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void OutputPanned(int channel, int leftVol, int rightVol,
                                               short[] buf, int offset, int count);

        // ── Channel control ───────────────────────────────────────────────

        /// <summary>
        /// Dynamically update the volume of a reserved channel without
        /// resubmitting a buffer.  Useful for fade-in/fade-out effects.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int ChangeChannelVolume(int channel, int leftVol, int rightVol);

        /// <summary>
        /// Returns the number of samples remaining in the channel's current buffer.
        /// Returns 0 when the channel is idle and ready to accept a new buffer.
        /// Returns a negative value on error.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetChannelRestLen(int channel);

        // ── Helpers (pure C#) ─────────────────────────────────────────────

        /// <summary>
        /// Allocate a stereo PCM buffer sized for the given sample count.
        /// Returns a short[] of length sampleCount × 2 (interleaved L/R).
        /// </summary>
        public static short[] AllocStereoBuffer(int sampleCount)
        {
            return new short[sampleCount * 2];
        }

        /// <summary>
        /// Allocate a mono PCM buffer for the given sample count.
        /// </summary>
        public static short[] AllocMonoBuffer(int sampleCount)
        {
            return new short[sampleCount];
        }

        /// <summary>
        /// Fill buf[offset .. offset + count*2] with a sine wave at the
        /// given frequency (Hz) and amplitude (0.0–1.0).
        /// Advances phase by the correct amount per sample and returns the new phase.
        /// </summary>
        public static float FillSineWave(short[] buf, int offset, int count,
                                         float frequency, float amplitude, float phase)
        {
            float step = 2.0f * 3.14159265f * frequency / 44100.0f;
            float scale = amplitude * 32767.0f;
            int i = offset;
            for (int s = 0; s < count; s++)
            {
                short v = (short)(System.Math.Sin(phase) * scale);
                buf[i++] = v; // left
                buf[i++] = v; // right
                phase += step;
                if (phase >= 2.0f * 3.14159265f)
                    phase -= 2.0f * 3.14159265f;
            }
            return phase;
        }
    }
}
