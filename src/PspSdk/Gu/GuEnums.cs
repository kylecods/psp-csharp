namespace PspSdk.Gu;

/// <summary>GU render state toggle flags (GU_*).</summary>
public enum GuState : int
{
    AlphaTest         = 0,
    DepthTest         = 1,
    ScissorTest       = 2,
    StencilTest       = 3,
    Blend             = 10,
    CullFace          = 11,
    Dither            = 12,
    Fog               = 13,
    ClipPlanes        = 14,
    Texture2D         = 20,
    Lighting          = 21,
    Light0            = 22,
    Light1            = 23,
    Light2            = 24,
    Light3            = 25,
    LineSmooth        = 26,
    PatchCullFace     = 27,
    ColorTest         = 28,
    ColorLogicOp      = 29,
    FaceNormalReverse = 30,
    PatchFace         = 31,
    Fragment2X        = 32,
}

/// <summary>
/// Framebuffer / texture pixel formats (PSM = Pixel Storage Mode).
/// Passed to sceGuDrawBuffer, sceGuDispBuffer, sceGuTexMode.
/// </summary>
public enum GuPixelMode : int
{
    Psm5650 = 0,   // RGB 565  — 16-bit, no alpha
    Psm5551 = 1,   // RGBA 5551 — 16-bit, 1-bit alpha
    Psm4444 = 2,   // RGBA 4444 — 16-bit
    Psm8888 = 3,   // RGBA 8888 — 32-bit, full quality
    PsmT4   = 4,   // 4-bit palette indexed
    PsmT8   = 5,   // 8-bit palette indexed
    PsmT16  = 6,   // 16-bit palette indexed
    PsmT32  = 7,   // 32-bit palette indexed
    PsmDxt1 = 8,
    PsmDxt3 = 9,
    PsmDxt5 = 10,
}

/// <summary>Primitive topology for sceGuDrawArray.</summary>
public enum GuPrimitive : int
{
    Points        = 0,
    Lines         = 1,
    LineStrip     = 2,
    Triangles     = 3,
    TriangleStrip = 4,
    TriangleFan   = 5,
    /// <summary>PSP-specific 2D sprite quads (axis-aligned rectangles).</summary>
    Sprites       = 6,
}

/// <summary>
/// Clear buffer selection flags. [Flags] so callers can OR them together:
///   GuModule.Clear(GuClearFlags.Color | GuClearFlags.Depth)
/// </summary>
[Flags]
public enum GuClearFlags : int
{
    Color   = 0x1,
    Stencil = 0x2,
    Depth   = 0x4,
    All     = Color | Stencil | Depth,
}

/// <summary>
/// Vertex type descriptor flags. Composable bitmask passed to sceGuDrawArray's vtype parameter.
/// OR the format bits together to describe your vertex layout:
///   GuVertexType.PositionFloat | GuVertexType.ColorABGR8888 | GuVertexType.Transform3D
/// </summary>
[Flags]
public enum GuVertexType : int
{
    None           = 0,
    // Texture coordinate formats (bits 0-1)
    TextureFixed8  = 0x0001,
    TextureFixed16 = 0x0002,
    TextureFloat   = 0x0003,
    // Color formats (bits 2-6)
    ColorBGR5650   = 0x0040,
    ColorABGR5551  = 0x0050,
    ColorABGR4444  = 0x0060,
    ColorABGR8888  = 0x0070,
    // Normal formats (bits 7-8)
    NormalFixed8   = 0x0100,
    NormalFixed16  = 0x0200,
    NormalFloat    = 0x0300,
    // Position formats (bits 9-10)
    PositionFixed8 = 0x0400,
    PositionFixed16= 0x0800,
    PositionFloat  = 0x0C00,
    // Coordinate space
    Transform3D    = 0,
    Transform2D    = 1 << 23,
}

public enum GuSyncMode     : int { Finish = 0, Signal = 1, Done = 2, SendList = 3, DrawDone = 4 }
public enum GuSyncBehavior : int { Blocking = 0, NonBlocking = 1 }
public enum GuDisplayState : int { Off = 0, On = 1 }
public enum GuBlendOp      : int { Add = 0, Subtract = 1, ReverseSubtract = 2, Min = 3, Max = 4, AbsoluteDiff = 5 }
public enum GuBlendFactor  : int { SrcColor = 0, OneMinusSrcColor = 1, SrcAlpha = 2, OneMinusSrcAlpha = 3, DstAlpha = 4, OneMinusDstAlpha = 5, Fix = 10 }
public enum GuDepthFunc    : int { Never = 0, Always = 1, Equal = 2, NotEqual = 3, Less = 4, LessEqual = 5, Greater = 6, GreaterEqual = 7 }
public enum GuShadeModel   : int { Flat = 0, Smooth = 1 }
public enum GuTexFunc      : int { Modulate = 0, Decal = 1, Blend = 2, Replace = 3, Add = 4 }
