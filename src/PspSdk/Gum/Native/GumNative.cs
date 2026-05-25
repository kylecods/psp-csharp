namespace PspSdk.Gum.Native;

using System.Runtime.InteropServices;
using PspSdk.Core;

/// <summary>
/// Raw P/Invoke declarations for sceGum (matrix math layer).
///
/// Key pattern: Scale and Translate take ScePspFVector3* in C.
/// In C# we declare them as 'ref PspFVector3', which instructs the runtime
/// to automatically pass a pointer to the struct — no 'unsafe' keyword needed
/// at the declaration or call site.
///
/// This is the idiomatic safe alternative to 'unsafe PspFVector3*' for
/// single-struct pointer parameters where no pointer arithmetic is needed.
/// </summary>
internal static class GumNative
{
    private const string LibName = "sceGum";

    [DllImport(LibName, EntryPoint = "sceGumMatrixMode",  CallingConvention = CallingConvention.Cdecl)]
    internal static extern void MatrixMode(int mode);

    [DllImport(LibName, EntryPoint = "sceGumLoadIdentity",CallingConvention = CallingConvention.Cdecl)]
    internal static extern void LoadIdentity();

    [DllImport(LibName, EntryPoint = "sceGumPushMatrix",  CallingConvention = CallingConvention.Cdecl)]
    internal static extern void PushMatrix();

    [DllImport(LibName, EntryPoint = "sceGumPopMatrix",   CallingConvention = CallingConvention.Cdecl)]
    internal static extern void PopMatrix();

    [DllImport(LibName, EntryPoint = "sceGumRotateX",     CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RotateX(float angle);

    [DllImport(LibName, EntryPoint = "sceGumRotateY",     CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RotateY(float angle);

    [DllImport(LibName, EntryPoint = "sceGumRotateZ",     CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RotateZ(float angle);

    /// <summary>
    /// void sceGumScale(ScePspFVector3* v);
    /// 'ref PspFVector3' — runtime passes &amp;v automatically, no 'unsafe' needed.
    /// </summary>
    [DllImport(LibName, EntryPoint = "sceGumScale",       CallingConvention = CallingConvention.Cdecl)]
    internal static extern void Scale(ref PspFVector3 v);

    /// <summary>
    /// void sceGumTranslate(ScePspFVector3* v);
    /// Same 'ref' pattern as Scale.
    /// </summary>
    [DllImport(LibName, EntryPoint = "sceGumTranslate",   CallingConvention = CallingConvention.Cdecl)]
    internal static extern void Translate(ref PspFVector3 v);

    [DllImport(LibName, EntryPoint = "sceGumPerspective", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void Perspective(float fovy, float aspect, float near, float far);

    [DllImport(LibName, EntryPoint = "sceGumOrtho",       CallingConvention = CallingConvention.Cdecl)]
    internal static extern void Ortho(float left, float right, float bottom, float top, float near, float far);

    [DllImport(LibName, EntryPoint = "sceGumDrawArray",   CallingConvention = CallingConvention.Cdecl)]
    internal static extern void DrawArray(int prim, int vtype, int count, IntPtr indices, IntPtr vertices);

    [DllImport(LibName, EntryPoint = "sceGumDrawArray",   CallingConvention = CallingConvention.Cdecl)]
    internal static extern unsafe void DrawArray(int prim, int vtype, int count, void* indices, void* vertices);
}
