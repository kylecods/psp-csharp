/* native/Psp.Gum.c
 * Internal call implementations for Psp.Gum.
 *
 * PspFVector3 struct passing:
 *   When C# passes a 'ref PspFVector3' to an internal call, DNA resolves
 *   the managed struct pointer and passes the raw struct address.
 *   The C implementation receives it as a ScePspFVector3* (three consecutive
 *   floats in memory, exactly matching PspFVector3's layout).
 *
 *   Alternative: if DNA passes value structs by exploding their fields,
 *   we accept three separate F32 arguments and rebuild the C struct on
 *   the stack.  Both _impl signatures are provided; use whichever DNA
 *   actually calls (check the InternalCall registration).
 *
 * Matrix stack depth:
 *   GUM provides a 4-deep matrix stack.  PushMatrix/PopMatrix on Projection,
 *   View, Model, and Texture matrices each use one stack slot.  Never push
 *   more than 4 matrices per mode without a matching pop.
 */

#include <pspgum.h>
#include "Types.h"
#include "Psp.Gum.h"

/* ── Matrix mode ────────────────────────────────────────────────────────── */

void Psp_Gum_MatrixMode_impl(I32 mode)
{
    sceGumMatrixMode(mode);
}

/* ── Stack operations ───────────────────────────────────────────────────── */

void Psp_Gum_LoadIdentity_impl(void)
{
    sceGumLoadIdentity();
}

void Psp_Gum_PushMatrix_impl(void)
{
    sceGumPushMatrix();
}

void Psp_Gum_PopMatrix_impl(void)
{
    sceGumPopMatrix();
}

/* ── Transforms ─────────────────────────────────────────────────────────── */

/*
 * DNA passes the PspFVector3 struct's three float fields as three
 * consecutive F32 arguments (struct explosion for small value types).
 * We reconstruct the ScePspFVector3 on the stack and pass by pointer.
 */

void Psp_Gum_Translate_impl(F32 x, F32 y, F32 z)
{
    ScePspFVector3 v = { x, y, z };
    sceGumTranslate(&v);
}

void Psp_Gum_Scale_impl(F32 x, F32 y, F32 z)
{
    ScePspFVector3 v = { x, y, z };
    sceGumScale(&v);
}

void Psp_Gum_RotateX_impl(F32 angle)
{
    sceGumRotateX(angle);
}

void Psp_Gum_RotateY_impl(F32 angle)
{
    sceGumRotateY(angle);
}

void Psp_Gum_RotateZ_impl(F32 angle)
{
    sceGumRotateZ(angle);
}

/* ── Projection ─────────────────────────────────────────────────────────── */

void Psp_Gum_Perspective_impl(F32 fovY, F32 aspect, F32 near, F32 far)
{
    sceGumPerspective(fovY, aspect, near, far);
}

void Psp_Gum_Ortho_impl(F32 left, F32 right, F32 bottom, F32 top,
                         F32 near, F32 far)
{
    sceGumOrtho(left, right, bottom, top, near, far);
}

/* ── Draw ───────────────────────────────────────────────────────────────── */

void Psp_Gum_DrawArrayRaw_impl(I32 prim, I32 vtype, I32 count, PTR vertices)
{
    sceGumDrawArray(prim, vtype, count, NULL, (const void*)(uintptr_t)vertices);
}
