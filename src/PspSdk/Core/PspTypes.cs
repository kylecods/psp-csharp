namespace PspSdk.Core;

using System.Runtime.InteropServices;

/// <summary>
/// ScePspFVector3 — maps to C struct { float x; float y; float z; }.
/// [StructLayout(Sequential)] guarantees fields are laid out in declaration order
/// with no reordering, matching the C struct memory layout for P/Invoke.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct PspFVector3
{
    public float X;
    public float Y;
    public float Z;

    public PspFVector3(float x, float y, float z) => (X, Y, Z) = (x, y, z);

    public static readonly PspFVector3 Zero = new(0f, 0f, 0f);
    public static readonly PspFVector3 One  = new(1f, 1f, 1f);
}

/// <summary>
/// ScePspFMatrix4 — 4×4 float matrix used by GUM.
/// Demonstrates 'fixed' keyword: embeds the array inline rather than as a managed reference.
/// Without 'fixed', C# would store a reference (pointer to heap), not the 16 floats themselves,
/// and the struct size would not match the C layout.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct PspFMatrix4
{
    // 'fixed float M[16]' allocates 64 bytes inline inside the struct.
    // Accessing M[i] inside an 'unsafe' block is required.
    public fixed float M[16];
}

/// <summary>
/// ScePspIVector4 — integer 4-vector used in some GU colour/index operations.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct PspIVector4
{
    public int X, Y, Z, W;
}
