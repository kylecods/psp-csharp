// Psp/BasicGraphics2.cs
// High-level 2D / sprite rendering layer.
// All drawing ultimately calls sceGu* via internal calls on the C side.
//
// This module handles GU initialisation, VRAM allocation, and
// double-buffered rendering internally so game code never has to
// deal with display lists, buffer offsets, or sceGuStart/Finish.
//
// For 3D rendering or fine-grained GU control, use Psp.Gu directly.

using System.Runtime.CompilerServices;

namespace Psp
{
    public static class BasicGraphics2
    {
        // ── Lifecycle ─────────────────────────────────────────────────────

        /// <summary>
        /// Initialise the GU, allocate framebuffers in VRAM, and enable
        /// the display.  Call once at startup before any drawing.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Init();

        /// <summary>
        /// Shut down the GU.  Call before ExitGame().
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Term();

        // ── Per-frame ─────────────────────────────────────────────────────

        /// <summary>
        /// Present the back buffer to the display, wait for vblank, and
        /// swap draw and display buffers.  Call at the end of each frame.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void SwapBuffers();

        // ── Drawing ───────────────────────────────────────────────────────

        /// <summary>
        /// Fill the entire back buffer with a solid colour.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Clear(Color color);

        /// <summary>
        /// Draw a solid filled rectangle on the back buffer.
        /// Coordinates are screen-space pixels (0,0 = top-left).
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void DrawRect(int x, int y, int width, int height, Color color);

        /// <summary>
        /// Draw a horizontal line.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void DrawLine(int x1, int y1, int x2, int y2, Color color);

        /// <summary>
        /// Render a null-terminated ASCII string at pixel position (x, y)
        /// using the embedded 8×8 bitmap font.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void DrawText(int x, int y, string text, Color color);

        // ── Texture / surface ─────────────────────────────────────────────

        /// <summary>
        /// Load an image from the memory stick into a Surface object.
        /// Supported formats: BMP, PNG (24-bit or 32-bit).
        /// Path is relative to the EBOOT.PBP directory (ms0:/PSP/GAME/MyApp/).
        /// Returns null if the file cannot be loaded.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern Surface LoadSurface(string path);

        /// <summary>
        /// Upload a Surface to GPU-accessible VRAM as a Texture.
        /// Dimensions must be powers of 2 (hardware limitation).
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern Texture CreateTexture(Surface surface);

        /// <summary>
        /// Draw a Texture at screen position (x, y) scaled to (w, h) pixels.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void DrawTexture(Texture texture, int x, int y, int width, int height);

        /// <summary>
        /// Draw a sub-region of a Texture.
        /// srcX/srcY/srcW/srcH are texel coordinates inside the texture.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void DrawTextureRegion(
            Texture texture,
            int srcX, int srcY, int srcWidth, int srcHeight,
            int dstX, int dstY, int dstWidth, int dstHeight);

        // ── Screen constants (pure C#) ────────────────────────────────────

        /// <summary>Screen width in pixels.</summary>
        public const int ScreenWidth  = 480;

        /// <summary>Screen height in pixels.</summary>
        public const int ScreenHeight = 272;

        /// <summary>Screen centre X.</summary>
        public const int CentreX = ScreenWidth / 2;

        /// <summary>Screen centre Y.</summary>
        public const int CentreY = ScreenHeight / 2;
    }

    // ── Opaque handle types ───────────────────────────────────────────────
    // These are managed wrappers around C-side pointers.
    // The actual data lives in native memory; C# holds a HEAP_PTR to a
    // wrapper object allocated by the internal call.

    public class Surface
    {
        // Opaque — the native side reads internal fields it allocated.
        // C# code should only pass Surface instances to CreateTexture or
        // DrawText (for font surfaces).
    }

    public class Texture
    {
        // Opaque — holds a reference to VRAM pixel data.
        // Width and Height are exposed for layout calculations.
        public int Width  { get { return GetWidth();  } }
        public int Height { get { return GetHeight(); } }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int GetWidth();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int GetHeight();
    }
}
