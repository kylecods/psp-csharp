/* native/Psp.Gum.h
 * Internal call declarations for Psp.Gum.
 *
 * All sceGum* functions require THREAD_ATTR_VFPU in the main thread
 * attributes.  The C bootstrap (main.c) must include:
 *   PSP_MAIN_THREAD_ATTR(THREAD_ATTR_VFPU | THREAD_ATTR_USER);
 */

#pragma once
#include "Types.h"

void Psp_Gum_MatrixMode_impl(I32 mode);
void Psp_Gum_LoadIdentity_impl(void);
void Psp_Gum_PushMatrix_impl(void);
void Psp_Gum_PopMatrix_impl(void);

/* Translate / Scale receive a PspFVector3 struct (3 floats).
 * DNA passes value-type structs that fit in registers directly;
 * we receive the three float components as separate arguments. */
void Psp_Gum_Translate_impl(F32 x, F32 y, F32 z);
void Psp_Gum_Scale_impl(F32 x, F32 y, F32 z);

void Psp_Gum_RotateX_impl(F32 angle);
void Psp_Gum_RotateY_impl(F32 angle);
void Psp_Gum_RotateZ_impl(F32 angle);

void Psp_Gum_Perspective_impl(F32 fovY, F32 aspect, F32 near, F32 far);
void Psp_Gum_Ortho_impl(F32 left, F32 right, F32 bottom, F32 top,
                         F32 near, F32 far);

void Psp_Gum_DrawArrayRaw_impl(I32 prim, I32 vtype, I32 count, PTR vertices);
