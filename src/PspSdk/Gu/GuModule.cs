namespace PspSdk.Gu;

using PspSdk.Core;
using PspSdk.Gu.Native;

/// <summary>
/// Safe public API for the PSP Graphics Utility (GU) library.
///
/// Key pattern here: DrawArray&lt;TVertex&gt; is a generic method constrained to
/// 'unmanaged' types. This means TVertex must be a value type with no managed
/// references — exactly the constraint needed to pin and pass to native code.
///
/// The generic 'fixed (TVertex* vptr = vertices)' avoids boxing and works for
/// any vertex struct the caller defines, without knowing its layout in advance.
/// </summary>
public static class GuModule
{
    public static void Init()  => GuNative.Init();
    public static void Term()  => GuNative.Term();

    /// <summary>
    /// Begins recording a display list. list is a VRAM-allocated buffer.
    /// contextId = 0 (GU_DIRECT) submits commands directly to the hardware.
    /// </summary>
    public static void Start(int contextId, IntPtr displayList) =>
        GuNative.Start(contextId, displayList);

    public static void Finish()
    {
        int r = GuNative.Finish();
        PspException.ThrowIfError(r, nameof(Finish));
    }

    public static void Sync(
        GuSyncMode     mode     = GuSyncMode.Finish,
        GuSyncBehavior behavior = GuSyncBehavior.Blocking)
    {
        int r = GuNative.Sync((int)mode, (int)behavior);
        PspException.ThrowIfError(r, nameof(Sync));
    }

    public static void SwapBuffers() => GuNative.SwapBuffers();

    public static void Display(GuDisplayState state)
    {
        int r = GuNative.Display((int)state);
        PspException.ThrowIfError(r, nameof(Display));
    }

    public static void DispBuffer(int width, int height, IntPtr dispbp, int dispbw) =>
        GuNative.DispBuffer(width, height, dispbp, dispbw);

    public static void DrawBuffer(GuPixelMode psm, IntPtr fbp, int fbw) =>
        GuNative.DrawBuffer((int)psm, fbp, fbw);

    public static void DepthBuffer(IntPtr zbp, int zbw) => GuNative.DepthBuffer(zbp, zbw);

    public static void Enable(GuState state)  => GuNative.Enable((int)state);
    public static void Disable(GuState state) => GuNative.Disable((int)state);

    public static void Clear(GuClearFlags flags) => GuNative.Clear((int)flags);
    public static void ClearColor(uint color)    => GuNative.ClearColor(color);
    public static void ClearDepth(uint depth)    => GuNative.ClearDepth(depth);

    /// <summary>
    /// Non-generic DrawArray — callers manage vertex/index pointers themselves.
    /// Pass IntPtr.Zero for indices when drawing non-indexed primitives.
    /// </summary>
    public static void DrawArray(
        GuPrimitive  prim,
        GuVertexType vtype,
        int          count,
        IntPtr       indices,
        IntPtr       vertices)
    {
        GuNative.DrawArray((int)prim, (int)vtype, count, indices, vertices);
    }

    /// <summary>
    /// Generic DrawArray — takes a managed vertex array and optional index array.
    ///
    /// 'where TVertex : unmanaged' is the C# constraint that guarantees TVertex
    /// contains no managed references, making it safe to pin and pass to native code.
    /// This is enforced at compile-time; no runtime check needed.
    ///
    /// The nested 'fixed' statements pin both arrays within a single unsafe block,
    /// minimising the pinning window (GC cannot collect or move pinned objects).
    /// </summary>
    public static unsafe void DrawArray<TVertex>(
        GuPrimitive  prim,
        GuVertexType vtype,
        int          count,
        TVertex[]    vertices,
        int[]?       indices = null)
        where TVertex : unmanaged
    {
        fixed (TVertex* vptr = vertices)
        {
            if (indices is null)
            {
                GuNative.DrawArray((int)prim, (int)vtype, count, null, vptr);
            }
            else
            {
                fixed (int* iptr = indices)
                    GuNative.DrawArray((int)prim, (int)vtype, count, iptr, vptr);
            }
        }
    }

    public static void BlendFunc(
        GuBlendOp     op,
        GuBlendFactor src,
        GuBlendFactor dest,
        uint          srcFix  = 0,
        uint          destFix = 0)
    {
        GuNative.BlendFunc((int)op, (int)src, (int)dest, srcFix, destFix);
    }

    public static void DepthFunc(GuDepthFunc func)  => GuNative.DepthFunc((int)func);
    public static void ShadeModel(GuShadeModel mode) => GuNative.ShadeModel((int)mode);

    public static void Scissor(int x, int y, int w, int h) => GuNative.Scissor(x, y, w, h);
    public static void Viewport(int cx, int cy, int width, int height) =>
        GuNative.Viewport(cx, cy, width, height);

    public static void TexMode(GuPixelMode psm, int maxMips, int a2, bool swizzle) =>
        GuNative.TexMode((int)psm, maxMips, a2, swizzle ? 1 : 0);

    public static void TexImage(int mipmap, int width, int height, int tbw, IntPtr tbp) =>
        GuNative.TexImage(mipmap, width, height, tbw, tbp);

    /// <summary>
    /// Sets the texture environment function.
    /// useAlpha true = GU_TCC_RGBA (modulate alpha), false = GU_TCC_RGB.
    /// Demonstrates bool → int conversion for a C int-flag parameter.
    /// </summary>
    public static void TexFunc(GuTexFunc tfx, bool useAlpha = true) =>
        GuNative.TexFunc((int)tfx, useAlpha ? 1 : 0);
}
