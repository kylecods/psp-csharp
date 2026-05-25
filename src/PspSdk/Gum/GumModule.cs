namespace PspSdk.Gum;

using PspSdk.Core;
using PspSdk.Gu;
using PspSdk.Gum.Native;

/// <summary>
/// Safe public API for the PSP Graphics Utility Math (GUM) library.
///
/// GUM wraps the GE matrix hardware. The typical per-frame sequence is:
///   GumModule.MatrixMode(MatrixMode.Projection);
///   GumModule.LoadIdentity();
///   GumModule.Perspective(75f, 16f/9f, 0.5f, 1000f);
///   GumModule.MatrixMode(MatrixMode.View);
///   GumModule.LoadIdentity();
///   GumModule.MatrixMode(MatrixMode.Model);
///   GumModule.LoadIdentity();
///   GumModule.Translate(new PspFVector3(0, 0, -5));
///   GumModule.RotateY(angle);
///   GumModule.DrawArray&lt;MyVertex&gt;(...);
///
/// Scale and Translate demonstrate the 'ref' parameter pattern:
/// the caller passes a PspFVector3 by value; inside the method we take
/// a local copy and pass it as 'ref' to GumNative, which passes its address
/// to the native function. No 'unsafe' visible to the caller.
/// </summary>
public static class GumModule
{
    public static void MatrixMode(MatrixMode mode) => GumNative.MatrixMode((int)mode);
    public static void LoadIdentity()              => GumNative.LoadIdentity();
    public static void PushMatrix()                => GumNative.PushMatrix();
    public static void PopMatrix()                 => GumNative.PopMatrix();
    public static void RotateX(float angle)        => GumNative.RotateX(angle);
    public static void RotateY(float angle)        => GumNative.RotateY(angle);
    public static void RotateZ(float angle)        => GumNative.RotateZ(angle);

    /// <summary>
    /// Scales the current matrix by vector v.
    /// v is passed by value and forwarded as 'ref' — the runtime pins the local
    /// copy on the stack and passes its address to the native function.
    /// </summary>
    public static void Scale(PspFVector3 v)     => GumNative.Scale(ref v);

    /// <summary>Translates the current matrix by vector v (same ref pattern).</summary>
    public static void Translate(PspFVector3 v) => GumNative.Translate(ref v);

    /// <summary>Sets up a perspective projection matrix.</summary>
    /// <param name="fovy">Field of view in degrees (Y axis).</param>
    /// <param name="aspect">Aspect ratio (width / height); PSP is 480/272 ≈ 1.764.</param>
    /// <param name="near">Near clipping plane distance.</param>
    /// <param name="far">Far clipping plane distance.</param>
    public static void Perspective(float fovy, float aspect, float near, float far) =>
        GumNative.Perspective(fovy, aspect, near, far);

    /// <summary>Sets up an orthographic projection matrix.</summary>
    public static void Ortho(float left, float right, float bottom, float top, float near, float far) =>
        GumNative.Ortho(left, right, bottom, top, near, far);

    /// <summary>
    /// Generic DrawArray — transforms vertices through the GUM matrix stack before rendering.
    /// The 'unmanaged' constraint is the same as in GuModule.DrawArray&lt;TVertex&gt;.
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
                GumNative.DrawArray((int)prim, (int)vtype, count, null, vptr);
            else
                fixed (int* iptr = indices)
                    GumNative.DrawArray((int)prim, (int)vtype, count, iptr, vptr);
        }
    }
}
