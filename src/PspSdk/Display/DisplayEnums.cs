namespace PspSdk.Display;

/// <summary>
/// PSP pixel format constants (PSP_DISPLAY_PIXEL_FORMAT_*).
/// Used in sceDisplaySetFrameBuf to tell the hardware how to interpret framebuffer bytes.
/// </summary>
public enum PixelFormat : int
{
    /// <summary>16-bit: 5 bits Red, 6 Green, 5 Blue. No alpha.</summary>
    Format565  = 0,
    /// <summary>16-bit: 5 bits each RGB + 1 bit alpha.</summary>
    Format5551 = 1,
    /// <summary>16-bit: 4 bits each RGBA.</summary>
    Format4444 = 2,
    /// <summary>32-bit: 8 bits each RGBA. Most common for full-quality rendering.</summary>
    Format8888 = 3,
}

/// <summary>
/// Frame buffer sync mode (PSP_DISPLAY_SETBUF_*).
/// Controls when the hardware actually switches to the new framebuffer pointer.
/// </summary>
public enum SyncMode : int
{
    /// <summary>Switch on the next vsync interval (tear-free).</summary>
    NextFrame = 0,
    /// <summary>Switch immediately (may cause tearing mid-frame).</summary>
    Immediate = 1,
}

/// <summary>Display hardware mode.</summary>
public enum DisplayMode : int
{
    /// <summary>Standard LCD output — only valid mode on PSP.</summary>
    Lcd = 0,
}
