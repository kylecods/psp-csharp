namespace PspSdk.Samples;

using PspSdk.Display;

/// <summary>
/// Shows the display initialisation and VSync loop pattern.
/// Demonstrates:
///   • Typed enum parameters (no magic ints).
///   • SetFrameBuf accepting a managed uint[] (fixed pinning happens inside DisplayModule).
///   • IsVblank() returning C# bool (int-as-bool conversion hidden in module).
///   • WaitVblankStart returning PspResult for flexible error handling.
/// </summary>
public static class DisplaySample
{
    public static void InitDisplay()
    {
        // SetMode throws PspException on error — no return value to check.
        DisplayModule.SetMode(DisplayMode.Lcd, DisplayModule.ScreenWidth, DisplayModule.ScreenHeight);

        // Managed pixel buffer — pinned transparently inside SetFrameBuf.
        var framebuffer = new uint[DisplayModule.ScreenWidth * DisplayModule.ScreenHeight];
        DisplayModule.SetFrameBuf(framebuffer, DisplayModule.ScreenWidth,
            PixelFormat.Format8888, SyncMode.NextFrame);

        // VSync loop
        for (int frame = 0; frame < 60; frame++)
        {
            // IsVblank() is a plain C# bool — no '!= 0' at the call site.
            if (!DisplayModule.IsVblank())
            {
                // WaitVblankStart returns PspResult — caller decides how to handle errors.
                var syncResult = DisplayModule.WaitVblankStart();
                if (syncResult.IsError)
                {
                    Console.Error.WriteLine($"VSync error: 0x{syncResult.NativeCode:X8}");
                    break;
                }
            }

            // GetVcount returns uint directly — no error code wrapping needed.
            uint vcount = DisplayModule.GetVcount();
            Console.WriteLine($"Frame {frame}: vcount = {vcount}");
        }
    }
}
