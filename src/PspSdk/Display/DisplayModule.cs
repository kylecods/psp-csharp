namespace PspSdk.Display;

using PspSdk.Core;
using PspSdk.Display.Native;

/// <summary>
/// Safe public API for PSP display control.
///
/// Patterns demonstrated:
///   • Typed enum parameters replace raw ints (DisplayMode, PixelFormat, SyncMode).
///   • Two SetFrameBuf overloads: one for IntPtr (safe), one for managed uint[] (uses 'fixed').
///   • 'fixed' statement is confined inside this method — callers never see 'unsafe'.
///   • IsVblank() maps int-as-bool to C# bool.
///   • WaitVblankStart returns PspResult so callers can inspect or ignore the error code.
/// </summary>
public static class DisplayModule
{
    /// <summary>PSP native screen width in pixels.</summary>
    public const int ScreenWidth = 480;
    /// <summary>PSP native screen height in pixels.</summary>
    public const int ScreenHeight = 272;

    /// <summary>
    /// Sets the display output mode.
    /// Throws PspException on native error.
    /// </summary>
    public static void SetMode(
        DisplayMode mode   = DisplayMode.Lcd,
        int         width  = ScreenWidth,
        int         height = ScreenHeight)
    {
        int result = DisplayNative.SetMode((int)mode, width, height);
        PspException.ThrowIfError(result, nameof(SetMode));
    }

    /// <summary>
    /// Sets the framebuffer from an IntPtr (opaque native address).
    /// Use when the pointer comes from native VRAM allocation.
    /// </summary>
    public static void SetFrameBuf(
        IntPtr      topaddr,
        int         bufferWidth = ScreenWidth,
        PixelFormat pixelFormat = PixelFormat.Format8888,
        SyncMode    sync        = SyncMode.NextFrame)
    {
        int result = DisplayNative.SetFrameBuf(
            topaddr, bufferWidth, (int)pixelFormat, (int)sync);
        PspException.ThrowIfError(result, nameof(SetFrameBuf));
    }

    /// <summary>
    /// Sets the framebuffer from a managed uint[] array.
    ///
    /// Demonstrates the 'fixed' statement:
    ///   • 'fixed (uint* ptr = pixels)' pins the array for the duration of the block.
    ///   • Without pinning the GC could move the array mid-call, making the native
    ///     code read/write wrong memory.
    ///   • The 'unsafe' keyword is confined here; callers see a clean managed API.
    /// </summary>
    public static unsafe void SetFrameBuf(
        uint[]      pixels,
        int         bufferWidth = ScreenWidth,
        PixelFormat pixelFormat = PixelFormat.Format8888,
        SyncMode    sync        = SyncMode.NextFrame)
    {
        fixed (uint* ptr = pixels)
        {
            int result = DisplayNative.SetFrameBuf(
                ptr, bufferWidth, (int)pixelFormat, (int)sync);
            PspException.ThrowIfError(result, nameof(SetFrameBuf));
        }
    }

    /// <summary>
    /// Returns the current frame (vertical) counter.
    /// No error possible — returned directly as uint.
    /// </summary>
    public static uint GetVcount() => DisplayNative.GetVcount();

    /// <summary>
    /// Blocks until the next vsync start.
    /// Returns PspResult so callers choose between exception and error-code inspection.
    /// </summary>
    public static PspResult WaitVblankStart()
    {
        int result = DisplayNative.WaitVblankStart();
        return new PspResult(result);
    }

    /// <summary>
    /// Returns true if the display is currently in the vertical blank interval.
    /// Demonstrates int-to-bool conversion: C functions use 0/non-zero for booleans.
    /// </summary>
    public static bool IsVblank() => DisplayNative.IsVblank() != 0;
}
