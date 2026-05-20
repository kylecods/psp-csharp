namespace PspSdk.Gu.Native;

using System.Runtime.InteropServices;

/// <summary>
/// Raw P/Invoke declarations for sceGu.
///
/// Key pattern: sceGuDrawArray takes two void* parameters (indices + vertices).
/// We provide dual overloads — IntPtr (safe) and unsafe void* — for each.
///
/// Display list pointer (void* list in sceGuStart) is always IntPtr here because
/// callers allocate it from VRAM or a fixed native buffer and only hold an address.
/// </summary>
internal static class GuNative
{
    private const string LibName = "sceGu";

    [DllImport(LibName, EntryPoint = "sceGuInit",  CallingConvention = CallingConvention.Cdecl)]
    internal static extern void Init();

    [DllImport(LibName, EntryPoint = "sceGuTerm",  CallingConvention = CallingConvention.Cdecl)]
    internal static extern void Term();

    /// <summary>
    /// void sceGuStart(int cid, void* list);
    /// list is the display list buffer allocated in VRAM.
    /// IntPtr is the natural C# type for an opaque native pointer.
    /// </summary>
    [DllImport(LibName, EntryPoint = "sceGuStart", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void Start(int cid, IntPtr list);

    [DllImport(LibName, EntryPoint = "sceGuFinish",     CallingConvention = CallingConvention.Cdecl)]
    internal static extern int Finish();

    [DllImport(LibName, EntryPoint = "sceGuSync",       CallingConvention = CallingConvention.Cdecl)]
    internal static extern int Sync(int mode, int what);

    [DllImport(LibName, EntryPoint = "sceGuSwapBuffers",CallingConvention = CallingConvention.Cdecl)]
    internal static extern void SwapBuffers();

    [DllImport(LibName, EntryPoint = "sceGuDisplay",    CallingConvention = CallingConvention.Cdecl)]
    internal static extern int Display(int state);

    [DllImport(LibName, EntryPoint = "sceGuDispBuffer", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void DispBuffer(int width, int height, IntPtr dispbp, int dispbw);

    [DllImport(LibName, EntryPoint = "sceGuDrawBuffer", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void DrawBuffer(int psm, IntPtr fbp, int fbw);

    [DllImport(LibName, EntryPoint = "sceGuDepthBuffer",CallingConvention = CallingConvention.Cdecl)]
    internal static extern void DepthBuffer(IntPtr zbp, int zbw);

    [DllImport(LibName, EntryPoint = "sceGuEnable",     CallingConvention = CallingConvention.Cdecl)]
    internal static extern void Enable(int state);

    [DllImport(LibName, EntryPoint = "sceGuDisable",    CallingConvention = CallingConvention.Cdecl)]
    internal static extern void Disable(int state);

    [DllImport(LibName, EntryPoint = "sceGuClear",      CallingConvention = CallingConvention.Cdecl)]
    internal static extern void Clear(int flags);

    [DllImport(LibName, EntryPoint = "sceGuClearColor", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void ClearColor(uint color);

    [DllImport(LibName, EntryPoint = "sceGuClearDepth", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void ClearDepth(uint depth);

    // ── DrawArray: the key function showing dual void* parameters ──────────

    /// <summary>
    /// void sceGuDrawArray(int prim, int vtype, int count, const void* indices, const void* vertices);
    ///
    /// IntPtr overload — safe. Pass IntPtr.Zero for indices when not using indexed drawing.
    /// </summary>
    [DllImport(LibName, EntryPoint = "sceGuDrawArray", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void DrawArray(int prim, int vtype, int count, IntPtr indices, IntPtr vertices);

    /// <summary>
    /// unsafe void* overload — allows passing typed pointers from 'fixed' blocks.
    /// Enables the generic GuModule.DrawArray&lt;TVertex&gt;() without boxing.
    /// </summary>
    [DllImport(LibName, EntryPoint = "sceGuDrawArray", CallingConvention = CallingConvention.Cdecl)]
    internal static extern unsafe void DrawArray(int prim, int vtype, int count, void* indices, void* vertices);

    // ── Render state ────────────────────────────────────────────────────────

    [DllImport(LibName, EntryPoint = "sceGuBlendFunc",  CallingConvention = CallingConvention.Cdecl)]
    internal static extern void BlendFunc(int op, int src, int dest, uint srcFix, uint destFix);

    [DllImport(LibName, EntryPoint = "sceGuDepthFunc",  CallingConvention = CallingConvention.Cdecl)]
    internal static extern void DepthFunc(int func);

    [DllImport(LibName, EntryPoint = "sceGuShadeModel", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void ShadeModel(int mode);

    [DllImport(LibName, EntryPoint = "sceGuScissor",    CallingConvention = CallingConvention.Cdecl)]
    internal static extern void Scissor(int x, int y, int w, int h);

    [DllImport(LibName, EntryPoint = "sceGuViewport",   CallingConvention = CallingConvention.Cdecl)]
    internal static extern void Viewport(int cx, int cy, int width, int height);

    // ── Texturing ───────────────────────────────────────────────────────────

    [DllImport(LibName, EntryPoint = "sceGuTexMode",    CallingConvention = CallingConvention.Cdecl)]
    internal static extern void TexMode(int tpsm, int maxmips, int a2, int swizzle);

    [DllImport(LibName, EntryPoint = "sceGuTexImage",   CallingConvention = CallingConvention.Cdecl)]
    internal static extern void TexImage(int mipmap, int width, int height, int tbw, IntPtr tbp);

    [DllImport(LibName, EntryPoint = "sceGuTexFunc",    CallingConvention = CallingConvention.Cdecl)]
    internal static extern void TexFunc(int tfx, int tcc);
}
