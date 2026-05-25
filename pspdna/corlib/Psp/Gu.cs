// Psp/Gu.cs
// Direct wrapper around the PSP Graphics Utility (sceGu*) library.
//
// Unlike BasicGraphics2 (which manages GU state internally), this module
// exposes the full GU pipeline so you can build custom renderers.
//
// The display list is managed internally in the C native layer:
//   static unsigned int __attribute__((aligned(16))) g_list[262144];
// C# code calls Gu.StartFrame() which calls sceGuStart(GU_DIRECT, g_list).
//
// Typical per-frame sequence:
//   Gu.StartFrame();
//     Gu.ClearColor(color);
//     Gu.ClearDepth(0);
//     Gu.Clear(GuClearFlags.ColorBuffer | GuClearFlags.DepthBuffer);
//     // set matrices via Gum.*
//     // draw calls via Gu.DrawArray*
//   Gu.EndFrame();   // finish + sync + vblank + swap

using System.Runtime.CompilerServices;

namespace Psp
{
    public static unsafe class Gu
    {
        // ── Lifecycle ─────────────────────────────────────────────────────

        /// <summary>
        /// Initialise the GU and allocate VRAM framebuffers.
        /// Sets up double buffering with an 8888 colour buffer and a 4444 depth buffer.
        /// Call once at startup; must be called before any other Gu.* method.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Init();

        /// <summary>
        /// Shut down the GU and release VRAM.  Call before Kernel.ExitGame().
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Term();

        // ── Per-frame ─────────────────────────────────────────────────────

        /// <summary>
        /// Begin recording GU commands into the internal display list.
        /// Wraps sceGuStart(GU_DIRECT, internal_list).
        /// Must be paired with EndFrame().
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void StartFrame();

        /// <summary>
        /// Finish recording, execute the display list on the GPU, wait for
        /// vblank, and swap front / back buffers.
        /// Wraps: sceGuFinish → sceGuSync → sceDisplayWaitVblankStart → sceGuSwapBuffers.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void EndFrame();

        // ── Clear ─────────────────────────────────────────────────────────

        /// <summary>
        /// Set the colour used by the next Clear() call.
        /// colour is in PSP native ABGR format; use Color.ToNative().
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void ClearColor(uint colour);

        /// <summary>Convenience overload — accepts a Color struct.</summary>
        public static void ClearColor(Color c) { ClearColor(c.ToNative()); }

        /// <summary>
        /// Set the depth value used to fill the depth buffer during Clear().
        /// PSP depth range is 0–65535; use 65535 for a standard reverse-Z setup
        /// where smaller depth value = closer to camera.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void ClearDepth(uint depth);

        /// <summary>
        /// Clear one or more buffers using the previously set clear colour/depth.
        /// Call after ClearColor() and ClearDepth() each frame.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Clear(GuClearFlags flags);

        // ── Render state ──────────────────────────────────────────────────

        /// <summary>Enable a render state.</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Enable(GuState state);

        /// <summary>Disable a render state.</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Disable(GuState state);

        /// <summary>Set the depth comparison function (default: LEqual).</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void SetDepthFunc(GuDepthFunc func);

        /// <summary>
        /// Specify which winding order is considered the front face.
        /// Use Cw for standard right-handed coordinates (PSP default).
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void SetFrontFace(GuFrontFace face);

        /// <summary>Set the shading model (Flat or Smooth/Gouraud).</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void SetShadeModel(GuShadeModel model);

        /// <summary>
        /// Set the scissor rectangle.  Only pixels inside this rectangle
        /// are affected by draw calls.  Default is the full screen.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void SetScissor(int x, int y, int width, int height);

        /// <summary>
        /// Set the depth range for depth buffer writes.
        /// near and far are raw 16-bit values (0–65535).
        /// Standard: near = 65535, far = 0 (reverse-Z).
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void SetDepthRange(int near, int far);

        /// <summary>Set the blend equation for alpha blending.</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void SetBlendFunc(GuBlendOp op,
                                               GuBlendFactor srcFactor,
                                               GuBlendFactor dstFactor,
                                               uint srcFix, uint dstFix);

        /// <summary>
        /// Convenience: enable standard alpha blending (src_alpha, 1-src_alpha).
        /// Call after Enable(GuState.Blend).
        /// </summary>
        public static void EnableAlphaBlend()
        {
            Enable(GuState.Blend);
            SetBlendFunc(GuBlendOp.Add,
                         GuBlendFactor.SrcAlpha,
                         GuBlendFactor.OneMinusSrcAlpha,
                         0, 0);
        }

        // ── Viewport ──────────────────────────────────────────────────────

        /// <summary>
        /// Set the GU viewport.  The PSP's hardware coordinate origin is at
        /// (2048, 2048) — this is a fixed-function quirk of the VFPU matrix unit.
        ///
        /// Standard call for full-screen 480×272:
        ///   Gu.SetOffset(2048 - 240, 2048 - 136);
        ///   Gu.SetViewport(2048, 2048, 480, 272);
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void SetViewport(int cx, int cy, int width, int height);

        /// <summary>
        /// Set the screen-space origin offset.
        /// Must be called before SetViewport.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void SetOffset(int x, int y);

        // ── Texturing ─────────────────────────────────────────────────────

        /// <summary>
        /// Set the texture pixel format and mipmap settings.
        /// Most common: TexMode(GuPixelMode.Psm8888, 0, 0, 0).
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void TexMode(GuPixelMode format, int maxMips, int a2, int swizzle);

        /// <summary>
        /// Upload a texture mipmap level.
        /// data must point to GPU-accessible memory (VRAM or uncached RAM).
        /// width and height must be powers of 2.  bufferWidth is the stride.
        ///
        /// In DNA, pass the data pointer as a System.IntPtr or raw void*:
        ///   fixed (uint* p = pixels) { Gu.TexImage(0, 256, 256, 256, (IntPtr)p); }
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void TexImage(int mipLevel, int width, int height,
                                           int bufferWidth, System.IntPtr data);

        /// <summary>Set the texture blending function.</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void TexFunc(GuTexFunc func, GuTexColorComponent colorComp);

        /// <summary>Set texture filtering (minification and magnification).</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void TexFilter(GuTexFilter min, GuTexFilter mag);

        /// <summary>Set UV wrap mode (Repeat or Clamp).</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void TexWrap(GuTexWrap u, GuTexWrap v);

        /// <summary>
        /// Flush the texture cache so that the GPU sees the latest texture data.
        /// Call after writing new pixel data before the next draw call that uses it.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void TexFlush();

        // ── Draw calls ────────────────────────────────────────────────────

        /// <summary>
        /// Submit a vertex array for rendering.
        ///
        /// prim     — primitive type (Triangles, TriangleStrip, Sprites, etc.)
        /// vtype    — vertex format flags describing the layout of each vertex struct
        /// count    — number of vertices
        /// vertices — raw pointer to the vertex array.
        ///
        /// In your game code use the typed helpers DrawVertices / DrawSprites
        /// rather than calling this directly.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void DrawArrayRaw(GuPrimitive prim, int vtype,
                                               int count, System.IntPtr vertices);

        /// <summary>
        /// Submit an indexed vertex array.
        /// indices — pointer to byte[] or short[] index buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void DrawArrayIndexedRaw(GuPrimitive prim, int vtype,
                                                      int count,
                                                      System.IntPtr indices,
                                                      System.IntPtr vertices);

        // ── Typed draw helpers (unsafe, pin arrays inline) ────────────────

        /// <summary>
        /// Draw an array of Vertex3D structs (position only, 3D transformed).
        /// </summary>
        public static void DrawVertices(GuPrimitive prim, Vertex3D[] verts, int count)
        {
            fixed (Vertex3D* p = verts)
            {
                DrawArrayRaw(prim,
                             (int)(GuVertexType.Vertex32Bit | GuVertexType.Transform3D),
                             count,
                             new System.IntPtr(p));
            }
        }

        /// <summary>
        /// Draw an array of VertexTC structs (texture + colour + position, 3D).
        /// </summary>
        public static void DrawVerticesTC(GuPrimitive prim, VertexTC[] verts, int count)
        {
            fixed (VertexTC* p = verts)
            {
                DrawArrayRaw(prim,
                             (int)(GuVertexType.Texture32Bit |
                                   GuVertexType.Color8888    |
                                   GuVertexType.Vertex32Bit  |
                                   GuVertexType.Transform3D),
                             count,
                             new System.IntPtr(p));
            }
        }

        /// <summary>
        /// Draw 2D sprites (axis-aligned quads).  Each sprite is defined by
        /// two Vertex2D structs: top-left corner and bottom-right corner.
        /// verts.Length must be a multiple of 2.
        /// </summary>
        public static void DrawSprites(Vertex2D[] verts, int count)
        {
            fixed (Vertex2D* p = verts)
            {
                DrawArrayRaw(GuPrimitive.Sprites,
                             (int)(GuVertexType.Vertex32Bit | GuVertexType.Transform2D),
                             count,
                             new System.IntPtr(p));
            }
        }

        // ── Colour ────────────────────────────────────────────────────────

        /// <summary>
        /// Set the global draw colour (modulates vertex colours).
        /// colour is ABGR; use Color.ToNative().
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void SetColor(uint colour);

        /// <summary>Convenience overload.</summary>
        public static void SetColor(Color c) { SetColor(c.ToNative()); }
    }
}
