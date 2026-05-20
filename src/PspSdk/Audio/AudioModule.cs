namespace PspSdk.Audio;

using PspSdk.Core;
using PspSdk.Audio.Native;

/// <summary>
/// Safe public API for PSP audio output.
///
/// Patterns demonstrated:
///   • Resource-handle return: ReserveChannel returns PspResult&lt;int&gt; where Value = channelId.
///   • Managed array pinning: Output(short[]) uses 'fixed' to pin PCM samples.
///   • IDisposable resource handle: AudioChannelHandle releases the channel automatically.
/// </summary>
public static class AudioModule
{
    /// <summary>Maximum volume level (PSP_AUDIO_VOLUME_MAX = 0x8000).</summary>
    public const int MaxVolume = 0x8000;

    /// <summary>
    /// Reserves an audio channel and returns its ID as PspResult&lt;int&gt;.
    ///
    /// This demonstrates the "resource handle" return pattern:
    /// unlike most PSP functions where success = 0, here success = the channel ID.
    /// PspResult&lt;int&gt; makes this explicit: result.Value IS the channel ID.
    /// </summary>
    public static PspResult<int> ReserveChannel(
        int         sampleCount,
        AudioFormat format,
        int         channel = AudioChannel.AutoAssign)
    {
        int result = AudioNative.ChReserve(channel, sampleCount, (int)format);
        return result < 0
            ? new PspResult<int>(result)
            : new PspResult<int>(result, result);   // value IS the channel ID
    }

    /// <summary>Releases a previously reserved channel. Throws on error.</summary>
    public static void ReleaseChannel(int channelId)
    {
        int r = AudioNative.ChRelease(channelId);
        PspException.ThrowIfError(r, nameof(ReleaseChannel));
    }

    /// <summary>Non-blocking audio output from an IntPtr buffer.</summary>
    public static PspResult Output(int channelId, IntPtr buffer, int volume = MaxVolume)
    {
        int r = AudioNative.Output(channelId, volume, buffer);
        return new PspResult(r);
    }

    /// <summary>
    /// Non-blocking output from a managed short[] PCM buffer.
    ///
    /// 'fixed (short* ptr = pcmBuffer)' pins the managed array for the call duration.
    /// The GC cannot relocate the array while the native audio DMA reads from it.
    /// The 'unsafe' is hidden here — callers pass a plain short[].
    /// </summary>
    public static unsafe PspResult Output(int channelId, short[] pcmBuffer, int volume = MaxVolume)
    {
        fixed (short* ptr = pcmBuffer)
        {
            int r = AudioNative.Output(channelId, volume, ptr);
            return new PspResult(r);
        }
    }

    /// <summary>Blocking output — returns after the buffer finishes playing.</summary>
    public static PspResult OutputBlocking(int channelId, IntPtr buffer, int volume = MaxVolume)
    {
        int r = AudioNative.OutputBlocking(channelId, volume, buffer);
        return new PspResult(r);
    }

    /// <summary>Non-blocking panned output with independent L/R volumes.</summary>
    public static PspResult OutputPanned(int channelId, IntPtr buffer, int leftVol, int rightVol)
    {
        int r = AudioNative.OutputPanned(channelId, leftVol, rightVol, buffer);
        return new PspResult(r);
    }

    /// <summary>Changes channel volume without submitting a new buffer.</summary>
    public static PspResult ChangeVolume(int channelId, int leftVol, int rightVol)
    {
        int r = AudioNative.ChangeChannelVolume(channelId, leftVol, rightVol);
        return new PspResult(r);
    }

    /// <summary>
    /// Returns remaining sample count in the channel buffer.
    /// Non-negative value = sample count; negative = error code.
    /// </summary>
    public static PspResult<int> GetRestLength(int channelId)
    {
        int r = AudioNative.GetChannelRestLen(channelId);
        return r < 0 ? new PspResult<int>(r) : new PspResult<int>(r, r);
    }
}

/// <summary>
/// RAII wrapper for a PSP audio channel handle.
///
/// Demonstrates the IDisposable pattern over a native integer resource handle.
/// The pattern is common whenever native code allocates a resource identified by an int ID:
///   using var ch = AudioChannelHandle.Reserve(512, AudioFormat.Stereo);
///   ch.Output(buffer);
/// // sceAudioChRelease called automatically here
/// </summary>
public sealed class AudioChannelHandle : IDisposable
{
    private bool _disposed;

    /// <summary>The native channel ID (>= 0 when valid).</summary>
    public int Id { get; }

    public bool IsValid => Id >= 0;

    private AudioChannelHandle(int id) => Id = id;

    /// <summary>
    /// Reserves a channel and returns the handle.
    /// Throws PspException if no channel is available.
    /// </summary>
    public static AudioChannelHandle Reserve(
        int         sampleCount,
        AudioFormat format           = AudioFormat.Stereo,
        int         preferredChannel = AudioChannel.AutoAssign)
    {
        var result = AudioModule.ReserveChannel(sampleCount, format, preferredChannel);
        if (result.IsError)
            throw new PspException(
                $"Failed to reserve audio channel (0x{result.NativeCode:X8})",
                result.NativeCode);
        return new AudioChannelHandle(result.Value);
    }

    public PspResult Output(IntPtr buffer, int volume = AudioModule.MaxVolume) =>
        AudioModule.Output(Id, buffer, volume);

    public PspResult Output(short[] pcmBuffer, int volume = AudioModule.MaxVolume) =>
        AudioModule.Output(Id, pcmBuffer, volume);

    public PspResult OutputBlocking(IntPtr buffer, int volume = AudioModule.MaxVolume) =>
        AudioModule.OutputBlocking(Id, buffer, volume);

    public void Dispose()
    {
        if (!_disposed)
        {
            if (IsValid)
                AudioModule.ReleaseChannel(Id);
            _disposed = true;
        }
    }
}
