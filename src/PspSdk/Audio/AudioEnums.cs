namespace PspSdk.Audio;

/// <summary>PSP audio channel format flags (PSP_AUDIO_FORMAT_*).</summary>
public enum AudioFormat : int
{
    /// <summary>Stereo — interleaved left/right samples.</summary>
    Stereo = 0x00,
    /// <summary>Mono — single channel, duplicated to both speakers.</summary>
    Mono   = 0x10,
}

/// <summary>
/// Well-known audio channel constants.
/// The PSP hardware has 8 PCM output channels (0-7).
/// Pass AutoAssign (-1) to let the firmware pick a free channel.
/// </summary>
public static class AudioChannel
{
    public const int AutoAssign = -1;
    public const int Max        =  8;
}
