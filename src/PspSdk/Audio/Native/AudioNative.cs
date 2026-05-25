// Dual-mode: [DllImport] on desktop CLR, [MethodImpl(InternalCall)] under PSPDNA.
// See CtrlNative.cs for a full explanation of the #if PSPDNA pattern.

namespace PspSdk.Audio.Native;

#if PSPDNA
using System.Runtime.CompilerServices;
#else
using System.Runtime.InteropServices;
#endif

/// <summary>
/// Raw P/Invoke declarations for sceAudio.
///
/// Educational highlight — resource handle return values:
///   sceAudioChReserve returns a CHANNEL ID (>= 0) on success, negative on error.
///   This is different from typical "0 = ok" functions — the success value is
///   meaningful data (the channel identifier), not just a status flag.
///   AudioModule.ReserveChannel wraps this as PspResult&lt;int&gt; where Value = channelId.
///
/// Audio buffer parameters (void* buf) are shown in two forms:
///   - IntPtr: opaque address, safe overload.
///   - unsafe void*: direct pointer for callers pinning managed arrays.
/// </summary>
internal static class AudioNative
{
#if !PSPDNA
    private const string LibName = "sceAudio";
#endif

    /// <summary>
    /// int sceAudioChReserve(int channel, int sampleCount, int format);
    /// Returns the reserved channel ID (&gt;= 0), or a negative error code.
    /// </summary>
#if PSPDNA
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern int ChReserve(int channel, int sampleCount, int format);
#else
    [DllImport(LibName, EntryPoint = "sceAudioChReserve", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int ChReserve(int channel, int sampleCount, int format);
#endif

    /// <summary>int sceAudioChRelease(int channel);</summary>
#if PSPDNA
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern int ChRelease(int channel);
#else
    [DllImport(LibName, EntryPoint = "sceAudioChRelease", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int ChRelease(int channel);
#endif

    /// <summary>
    /// int sceAudioOutput(int channel, int vol, void* buf);
    /// Non-blocking: returns immediately; audio plays in the background.
    /// IntPtr overload — safe.
    /// </summary>
#if PSPDNA
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern int Output(int channel, int vol, IntPtr buf);
#else
    [DllImport(LibName, EntryPoint = "sceAudioOutput", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int Output(int channel, int vol, IntPtr buf);
#endif

    /// <summary>unsafe void* overload of sceAudioOutput.</summary>
#if PSPDNA
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern unsafe int Output(int channel, int vol, void* buf);
#else
    [DllImport(LibName, EntryPoint = "sceAudioOutput", CallingConvention = CallingConvention.Cdecl)]
    internal static extern unsafe int Output(int channel, int vol, void* buf);
#endif

    /// <summary>
    /// int sceAudioOutputBlocking(int channel, int vol, void* buf);
    /// Blocking: does not return until the buffer has finished playing.
    /// </summary>
#if PSPDNA
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern int OutputBlocking(int channel, int vol, IntPtr buf);
#else
    [DllImport(LibName, EntryPoint = "sceAudioOutputBlocking", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int OutputBlocking(int channel, int vol, IntPtr buf);
#endif

    /// <summary>
    /// int sceAudioOutputPanned(int channel, int leftVol, int rightVol, void* buf);
    /// Non-blocking output with independent left/right volume.
    /// </summary>
#if PSPDNA
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern int OutputPanned(int channel, int leftVol, int rightVol, IntPtr buf);
#else
    [DllImport(LibName, EntryPoint = "sceAudioOutputPanned", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int OutputPanned(int channel, int leftVol, int rightVol, IntPtr buf);
#endif

    /// <summary>int sceAudioChangeChannelVolume(int channel, int leftVol, int rightVol);</summary>
#if PSPDNA
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern int ChangeChannelVolume(int channel, int leftVol, int rightVol);
#else
    [DllImport(LibName, EntryPoint = "sceAudioChangeChannelVolume", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int ChangeChannelVolume(int channel, int leftVol, int rightVol);
#endif

    /// <summary>
    /// int sceAudioGetChannelRestLen(int channel);
    /// Returns the number of samples remaining in the channel's current buffer.
    /// </summary>
#if PSPDNA
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern int GetChannelRestLen(int channel);
#else
    [DllImport(LibName, EntryPoint = "sceAudioGetChannelRestLen", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int GetChannelRestLen(int channel);
#endif
}
