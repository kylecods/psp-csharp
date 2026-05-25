// Psp/Gum.cs
// Wrapper around the PSP Graphics Utility Math (sceGum*) library.
//
// GUM provides a matrix stack for each of the four GPU coordinate spaces:
//   Projection — perspective or orthographic camera
//   View       — camera position and orientation
//   Model      — object-to-world transform
//   Texture    — UV transform
//
// The VFPU (Vector Floating-Point Unit) executes matrix operations in hardware.
// PSP_MAIN_THREAD_ATTR must include THREAD_ATTR_VFPU on the C bootstrap side
// for sceGum* functions to work correctly.
//
// Typical per-object rendering:
//   Gum.MatrixMode(MatrixMode.Projection);
//   Gum.LoadIdentity();
//   Gum.Perspective(75f, 480f/272f, 0.5f, 1000f);
//
//   Gum.MatrixMode(MatrixMode.View);
//   Gum.LoadIdentity();
//
//   Gum.MatrixMode(MatrixMode.Model);
//   Gum.LoadIdentity();
//   Gum.Translate(new PspFVector3(0f, 0f, -4f));
//   Gum.RotateY(angle);
//
//   Gu.DrawVertices(GuPrimitive.Triangles, verts, verts.Length);

using System.Runtime.CompilerServices;

namespace Psp
{
    public enum MatrixMode : int
    {
        Projection = 0,   // GU_PROJECTION — camera projection
        View       = 1,   // GU_VIEW       — camera transform
        Model      = 2,   // GU_MODEL      — object transform
        Texture    = 3,   // GU_TEXTURE    — UV transform
    }

    public static class Gum
    {
        // ── Matrix mode ───────────────────────────────────────────────────

        /// <summary>
        /// Select which matrix stack subsequent operations will affect.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void MatrixMode(MatrixMode mode);

        // ── Matrix operations ─────────────────────────────────────────────

        /// <summary>Replace the current matrix with the identity matrix.</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void LoadIdentity();

        /// <summary>
        /// Push the current matrix onto the stack, creating a save point.
        /// Pair with PopMatrix() to restore.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void PushMatrix();

        /// <summary>
        /// Pop the top matrix off the stack, restoring the previously saved state.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void PopMatrix();

        // ── Transforms ───────────────────────────────────────────────────

        /// <summary>
        /// Apply a translation.  Moves the object by (v.X, v.Y, v.Z) in model space.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Translate(ref PspFVector3 v);

        /// <summary>Convenience overload — pass by value.</summary>
        public static void Translate(PspFVector3 v) { Translate(ref v); }

        /// <summary>
        /// Apply a scale.  Multiplies each axis by the corresponding component of v.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Scale(ref PspFVector3 v);

        /// <summary>Convenience overload.</summary>
        public static void Scale(PspFVector3 v) { Scale(ref v); }

        /// <summary>Uniform scale on all three axes.</summary>
        public static void ScaleUniform(float s)
        {
            PspFVector3 v = new PspFVector3(s, s, s);
            Scale(ref v);
        }

        /// <summary>
        /// Rotate around the X axis by <paramref name="angle"/> radians.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void RotateX(float angle);

        /// <summary>Rotate around the Y axis by <paramref name="angle"/> radians.</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void RotateY(float angle);

        /// <summary>Rotate around the Z axis by <paramref name="angle"/> radians.</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void RotateZ(float angle);

        // ── Projection helpers ────────────────────────────────────────────

        /// <summary>
        /// Set up a perspective projection matrix.
        ///
        /// fovY   — vertical field of view in degrees.
        /// aspect — width / height.  For PSP full screen: 480.0f / 272.0f ≈ 1.7647.
        /// near   — near clip plane distance (positive).  Minimum practical: 0.5f.
        /// far    — far clip plane distance.  Too large increases depth precision issues.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Perspective(float fovY, float aspect, float near, float far);

        /// <summary>
        /// Set up an orthographic projection matrix.
        /// Useful for 2D rendering layers on top of a 3D scene, or for UI overlays.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Ortho(float left, float right, float bottom, float top,
                                        float near, float far);

        // ── Draw ──────────────────────────────────────────────────────────

        /// <summary>
        /// GUM-accelerated DrawArray — applies the current model/view/projection
        /// matrices via the VFPU before sending vertices to the GU.
        /// For most use cases, call Gu.DrawVertices() instead; GUM transforms
        /// are applied automatically by the hardware pipeline.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void DrawArrayRaw(GuPrimitive prim, int vtype,
                                               int count, System.IntPtr vertices);

        // ── Helpers (pure C#) ─────────────────────────────────────────────

        /// <summary>
        /// Configure the full standard rendering pipeline for 3D drawing.
        /// Sets projection + identity view, then leaves model matrix active.
        /// Call once per frame before setting per-object model transforms.
        /// </summary>
        public static void Setup3D(float fovY = 75f, float near = 0.5f, float far = 1000f)
        {
            MatrixMode(Psp.MatrixMode.Projection);
            LoadIdentity();
            Perspective(fovY, 480f / 272f, near, far);

            MatrixMode(Psp.MatrixMode.View);
            LoadIdentity();

            MatrixMode(Psp.MatrixMode.Model);
            LoadIdentity();
        }

        /// <summary>
        /// Configure a 2D orthographic projection covering the full PSP screen.
        /// Origin at top-left (0,0); right/bottom at (480, 272).
        /// </summary>
        public static void Setup2D()
        {
            MatrixMode(Psp.MatrixMode.Projection);
            LoadIdentity();
            Ortho(0f, 480f, 272f, 0f, -1f, 1f);

            MatrixMode(Psp.MatrixMode.View);
            LoadIdentity();

            MatrixMode(Psp.MatrixMode.Model);
            LoadIdentity();
        }
    }
}
