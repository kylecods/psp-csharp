/* native/Psp.Gu.h
 * Internal call declarations for Psp.Gu.
 *
 * The display list (g_list) and VRAM framebuffer offsets (g_fbp0, g_fbp1,
 * g_zbp) are module-private globals in Psp.Gu.c.  C# never touches them
 * directly — all state is managed through these internal calls.
 */

#pragma once
#include "Types.h"

/* ── Lifecycle ─────────────────────────────────────────────────────────── */
void Psp_Gu_Init_impl(void);
void Psp_Gu_Term_impl(void);

/* ── Per-frame ──────────────────────────────────────────────────────────── */
void Psp_Gu_StartFrame_impl(void);
void Psp_Gu_EndFrame_impl(void);

/* ── Clear ──────────────────────────────────────────────────────────────── */
void Psp_Gu_ClearColor_impl(U32 colour);
void Psp_Gu_ClearDepth_impl(U32 depth);
void Psp_Gu_Clear_impl(I32 flags);

/* ── Render state ───────────────────────────────────────────────────────── */
void Psp_Gu_Enable_impl(I32 state);
void Psp_Gu_Disable_impl(I32 state);
void Psp_Gu_SetDepthFunc_impl(I32 func);
void Psp_Gu_SetFrontFace_impl(I32 face);
void Psp_Gu_SetShadeModel_impl(I32 model);
void Psp_Gu_SetScissor_impl(I32 x, I32 y, I32 width, I32 height);
void Psp_Gu_SetDepthRange_impl(I32 near, I32 far);
void Psp_Gu_SetBlendFunc_impl(I32 op, I32 src, I32 dst, U32 srcFix, U32 dstFix);

/* ── Viewport ───────────────────────────────────────────────────────────── */
void Psp_Gu_SetViewport_impl(I32 cx, I32 cy, I32 width, I32 height);
void Psp_Gu_SetOffset_impl(I32 x, I32 y);

/* ── Texturing ──────────────────────────────────────────────────────────── */
void Psp_Gu_TexMode_impl(I32 format, I32 maxMips, I32 a2, I32 swizzle);
void Psp_Gu_TexImage_impl(I32 mipLevel, I32 width, I32 height, I32 bufWidth, PTR data);
void Psp_Gu_TexFunc_impl(I32 func, I32 colorComp);
void Psp_Gu_TexFilter_impl(I32 minFilter, I32 magFilter);
void Psp_Gu_TexWrap_impl(I32 u, I32 v);
void Psp_Gu_TexFlush_impl(void);

/* ── Draw calls ─────────────────────────────────────────────────────────── */
void Psp_Gu_DrawArrayRaw_impl(I32 prim, I32 vtype, I32 count, PTR vertices);
void Psp_Gu_DrawArrayIndexedRaw_impl(I32 prim, I32 vtype, I32 count,
                                     PTR indices, PTR vertices);

/* ── Colour ─────────────────────────────────────────────────────────────── */
void Psp_Gu_SetColor_impl(U32 colour);
