// Psp/GuEnums.cs
// Enumerations mirroring the #define constants in pspgu.h.
//
// Each enum is typed as int to match the parameter types in
// sceGu* function signatures (all int in the C API).
//
// [Flags] is used where values are combined with bitwise OR,
// matching the C pattern of composing flags at the call site.

namespace Psp
{
    // ── GU context / command list mode ────────────────────────────────────

    /// <summary>
    /// GU command list execution mode passed to sceGuStart().
    /// GU_DIRECT: commands are sent directly to the hardware queue.
    /// GU_CALL:   commands are recorded into a sub-list for later execution.
    /// </summary>
    public enum GuContext : int
    {
        Direct = 0,   // GU_DIRECT
        Call   = 1,   // GU_CALL
        Send   = 2,   // GU_SEND
    }

    // ── Pixel / framebuffer formats ───────────────────────────────────────

    /// <summary>
    /// Pixel format for framebuffers and textures.
    /// 8888 is the most common for games (full colour + alpha).
    /// </summary>
    public enum GuPixelMode : int
    {
        Psm5650 = 0,   // 16-bit: 5R 6G 5B, no alpha
        Psm5551 = 1,   // 16-bit: 5R 5G 5B 1A
        Psm4444 = 2,   // 16-bit: 4R 4G 4B 4A  ← used for depth buffer
        Psm8888 = 3,   // 32-bit: 8R 8G 8B 8A  ← most common
        PsmT4   = 4,   // 4-bit indexed (palette)
        PsmT8   = 5,   // 8-bit indexed (palette)
        PsmT16  = 6,   // 16-bit indexed (palette)
        PsmT32  = 7,   // 32-bit indexed (palette)
        PsmDxt1 = 8,   // DXT1 compressed
        PsmDxt3 = 9,   // DXT3 compressed
        PsmDxt5 = 10,  // DXT5 compressed
    }

    // ── Render state ──────────────────────────────────────────────────────

    /// <summary>
    /// States that can be toggled with Gu.Enable() / Gu.Disable().
    /// These map to GU_ALPHA_TEST, GU_DEPTH_TEST, etc. from pspgu.h.
    /// </summary>
    public enum GuState : int
    {
        AlphaTest       = 0,
        DepthTest       = 1,
        ScissorTest     = 2,
        StencilTest     = 3,
        Blend           = 4,
        CullFace        = 5,
        Dither          = 6,
        Fog             = 7,
        ClipPlanes      = 8,
        Texture2D       = 9,
        Lighting        = 10,
        Light0          = 11,
        Light1          = 12,
        Light2          = 13,
        Light3          = 14,
        LineSmooth      = 15,
        PatchCullFace   = 16,
        ColorTest       = 17,
        ColorLogicOp    = 18,
        FaceNormalReverse = 19,
        PatchFace       = 20,
        Fragment2X      = 21,
    }

    // ── Clear flags ───────────────────────────────────────────────────────

    /// <summary>
    /// Flags for Gu.Clear() — which buffers to wipe each frame.
    /// Almost always ColorBuffer | DepthBuffer.
    /// </summary>
    [System.Flags]
    public enum GuClearFlags : int
    {
        ColorBuffer   = 1,  // GU_COLOR_BUFFER_BIT
        StencilBuffer = 2,  // GU_STENCIL_BUFFER_BIT
        DepthBuffer   = 4,  // GU_DEPTH_BUFFER_BIT
    }

    // ── Primitive types ───────────────────────────────────────────────────

    /// <summary>
    /// Primitive type for Gu.DrawArray().
    /// The most common are Triangles (solid meshes) and Sprites (2D quads).
    /// </summary>
    public enum GuPrimitive : int
    {
        Points        = 0,  // GU_POINTS
        Lines         = 1,  // GU_LINES
        LineStrip     = 2,  // GU_LINE_STRIP
        Triangles     = 3,  // GU_TRIANGLES
        TriangleStrip = 4,  // GU_TRIANGLE_STRIP
        TriangleFan   = 5,  // GU_TRIANGLE_FAN
        Sprites       = 6,  // GU_SPRITES — axis-aligned quads, 2 vertices each
    }

    // ── Vertex format flags ───────────────────────────────────────────────

    /// <summary>
    /// Vertex component format flags, combined with bitwise OR.
    /// The value tells the GU how to interpret each field in your vertex struct.
    ///
    /// Field declaration order in your vertex struct MUST match:
    ///   texture → colour → normal → position
    ///
    /// Examples:
    ///   Position only 3D:    Vertex32Bit | Transform3D
    ///   Textured coloured:   Texture32Bit | Color8888 | Vertex32Bit | Transform3D
    /// </summary>
    [System.Flags]
    public enum GuVertexType : int
    {
        // Texture coordinate formats (bits 0-1)
        Texture8Bit  = (1 << 0),   // 8-bit fixed UV
        Texture16Bit = (2 << 0),   // 16-bit fixed UV
        Texture32Bit = (3 << 0),   // 32-bit float UV  ← most common

        // Colour formats (bits 2-6, encoded as (value << 2))
        Color5650    = (4 << 2),
        Color5551    = (5 << 2),
        Color4444    = (6 << 2),
        Color8888    = (7 << 2),   // 32-bit ABGR     ← most common

        // Normal formats (bits 5-6)
        Normal8Bit   = (1 << 5),
        Normal16Bit  = (2 << 5),
        Normal32Bit  = (3 << 5),   // 32-bit float normals

        // Position formats (bits 7-8)
        Vertex8Bit   = (1 << 7),
        Vertex16Bit  = (2 << 7),
        Vertex32Bit  = (3 << 7),   // 32-bit float positions ← most common

        // Transform mode (bit 23)
        Transform3D  = 0,            // GU_TRANSFORM_3D: apply model/view/proj matrices
        Transform2D  = (1 << 23),    // GU_TRANSFORM_2D: raw screen coordinates
    }

    // ── Sync modes ────────────────────────────────────────────────────────

    /// <summary>
    /// Synchronisation mode for Gu.Sync().
    /// Always use Finish + WaitDone after each frame's command list.
    /// </summary>
    public enum GuSyncMode : int
    {
        Finish = 0,   // GU_SYNC_FINISH — wait for the current list to complete
        List   = 1,   // GU_SYNC_LIST
        Send   = 2,   // GU_SYNC_SEND
        Done   = 3,   // GU_SYNC_DONE
    }

    public enum GuSyncBehavior : int
    {
        WaitDone = 0,   // GU_SYNC_WHAT_DONE — block until done
    }

    // ── Depth comparison functions ────────────────────────────────────────

    public enum GuDepthFunc : int
    {
        Never    = 0,   // GU_NEVER
        Always   = 1,   // GU_ALWAYS
        Equal    = 2,   // GU_EQUAL
        NotEqual = 3,   // GU_NOTEQUAL
        Less     = 4,   // GU_LESS
        LEqual   = 5,   // GU_LEQUAL  ← standard for depth test
        Greater  = 6,   // GU_GREATER
        GEqual   = 7,   // GU_GEQUAL
    }

    // ── Face culling ──────────────────────────────────────────────────────

    public enum GuFrontFace : int
    {
        Cw  = 0,   // GU_CW  — clockwise winding is front face
        Ccw = 1,   // GU_CCW — counter-clockwise winding is front face
    }

    // ── Shade model ───────────────────────────────────────────────────────

    public enum GuShadeModel : int
    {
        Flat   = 0,   // GU_FLAT   — constant colour per primitive
        Smooth = 1,   // GU_SMOOTH — interpolated colours (Gouraud shading)
    }

    // ── Blend operation ───────────────────────────────────────────────────

    public enum GuBlendOp : int
    {
        Add             = 0,
        Subtract        = 1,
        ReverseSubtract = 2,
        Min             = 3,
        Max             = 4,
        AbsoluteValue   = 5,
    }

    public enum GuBlendFactor : int
    {
        SrcColor        = 0,
        OneMinusSrcColor = 1,
        SrcAlpha        = 2,
        OneMinusSrcAlpha = 3,
        DstColor        = 4,
        OneMinusDstColor = 5,
        DstAlpha        = 6,
        OneMinusDstAlpha = 7,
        Fix             = 10,
    }

    // ── Texture functions ─────────────────────────────────────────────────

    public enum GuTexFunc : int
    {
        Modulate  = 0,   // GU_TFX_MODULATE  — texture × vertex colour
        Decal     = 1,   // GU_TFX_DECAL     — texture replaces colour
        Blend     = 2,   // GU_TFX_BLEND
        Replace   = 3,   // GU_TFX_REPLACE
        Add       = 4,   // GU_TFX_ADD
    }

    public enum GuTexColorComponent : int
    {
        Rgb  = 0,   // GU_TCC_RGB  — ignore texture alpha
        Rgba = 1,   // GU_TCC_RGBA — use texture alpha
    }

    public enum GuTexFilter : int
    {
        Nearest              = 0,  // GU_NEAREST
        Linear               = 1,  // GU_LINEAR              ← bilinear
        NearestMipmapNearest = 4,
        LinearMipmapNearest  = 5,
        NearestMipmapLinear  = 6,
        LinearMipmapLinear   = 7,
    }

    public enum GuTexWrap : int
    {
        Repeat = 0,   // GU_REPEAT
        Clamp  = 1,   // GU_CLAMP
    }
}
