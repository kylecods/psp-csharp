/* native/Psp.Gu.c
 * Internal call implementations for Psp.Gu.
 *
 * VRAM layout (double-buffered, 32-bit colour):
 *
 *   Offset 0x00000000  fbp0: draw buffer    512×272×4 = 557,056 bytes
 *   Offset 0x00088000  fbp1: display buffer 512×272×4 = 557,056 bytes
 *   Offset 0x00110000  zbp:  depth buffer   512×272×2 = 278,528 bytes (16-bit)
 *   Offset 0x00154000  available for textures
 *
 * The PSP GU uses 512 as the hardware row stride (BUF_WIDTH) even though
 * only 480 pixels are visible per row.  All buffer size calculations must
 * use 512, not 480.
 *
 * Display list:
 *   A static 1 MB array in BSS, 16-byte aligned.  All GU commands for one
 *   frame are recorded here and dispatched to the GE DMA engine by
 *   sceGuFinish/sceGuSync.
 *
 * PSP coordinate origin:
 *   The GU hardware coordinate system centres the viewport at (2048, 2048).
 *   sceGuOffset and sceGuViewport must be set accordingly.
 */

#include <pspkernel.h>
#include <pspdisplay.h>
#include <pspgu.h>
#include <pspgum.h>
#include <pspge.h>
#include <string.h>
#include "Types.h"
#include "Psp.Gu.h"

/* ── VRAM layout constants ──────────────────────────────────────────────── */
#define BUF_WIDTH   512   /* hardware row stride (pixels)    */
#define SCR_WIDTH   480   /* visible screen width (pixels)   */
#define SCR_HEIGHT  272   /* visible screen height (pixels)  */

#define VRAM_BASE   ((unsigned int)0x04000000)
#define VRAM_CACHED ((unsigned int)0x44000000)

/* Framebuffer byte sizes */
#define FB_SIZE_32  (BUF_WIDTH * SCR_HEIGHT * 4)   /* 32-bit colour: 557,056 */
#define FB_SIZE_16  (BUF_WIDTH * SCR_HEIGHT * 2)   /* 16-bit depth:  278,528 */

/* VRAM pointer arithmetic: returns cached-alias VRAM address at byte offset */
#define VRAM_PTR(offset) ((void*)((VRAM_CACHED) + (offset)))

/* ── Module-private state ───────────────────────────────────────────────── */

/* Display list: 1 MB, 16-byte aligned, in BSS */
static unsigned int __attribute__((aligned(16))) g_list[262144];

/* VRAM buffer pointers */
static void* g_fbp0 = NULL;   /* draw (back) buffer   */
static void* g_fbp1 = NULL;   /* display (front) buffer */
static void* g_zbp  = NULL;   /* depth buffer         */

/* ── Lifecycle ──────────────────────────────────────────────────────────── */

void Psp_Gu_Init_impl(void)
{
    /* Compute VRAM buffer pointers using cached alias */
    g_fbp0 = VRAM_PTR(0);
    g_fbp1 = VRAM_PTR(FB_SIZE_32);
    g_zbp  = VRAM_PTR(FB_SIZE_32 * 2);

    sceGuInit();

    /* Record the one-time GU setup commands */
    sceGuStart(GU_DIRECT, g_list);
        sceGuDrawBuffer(GU_PSM_8888, g_fbp0, BUF_WIDTH);
        sceGuDispBuffer(SCR_WIDTH, SCR_HEIGHT, g_fbp1, BUF_WIDTH);
        sceGuDepthBuffer(g_zbp, BUF_WIDTH);

        /* The PSP hardware viewport centres at (2048, 2048) */
        sceGuOffset(2048 - SCR_WIDTH / 2, 2048 - SCR_HEIGHT / 2);
        sceGuViewport(2048, 2048, SCR_WIDTH, SCR_HEIGHT);

        /* Reverse-Z: far = 0, near = 65535 */
        sceGuDepthRange(65535, 0);

        /* Scissor to screen bounds */
        sceGuScissor(0, 0, SCR_WIDTH, SCR_HEIGHT);
        sceGuEnable(GU_SCISSOR_TEST);

        /* Standard depth test */
        sceGuEnable(GU_DEPTH_TEST);
        sceGuDepthFunc(GU_LEQUAL);

        /* Back-face culling, clockwise front faces */
        sceGuEnable(GU_CULL_FACE);
        sceGuFrontFace(GU_CW);

        /* Gouraud shading */
        sceGuShadeModel(GU_SMOOTH);

        /* Clip to view volume */
        sceGuEnable(GU_CLIP_PLANES);
    sceGuFinish();
    sceGuSync(GU_SYNC_FINISH, GU_SYNC_WHAT_DONE);

    sceDisplayWaitVblankStart();
    sceGuDisplay(GU_TRUE);
}

void Psp_Gu_Term_impl(void)
{
    sceGuDisplay(GU_FALSE);
    sceGuTerm();
}

/* ── Per-frame ──────────────────────────────────────────────────────────── */

void Psp_Gu_StartFrame_impl(void)
{
    sceGuStart(GU_DIRECT, g_list);
}

void Psp_Gu_EndFrame_impl(void)
{
    sceGuFinish();
    sceGuSync(GU_SYNC_FINISH, GU_SYNC_WHAT_DONE);
    sceDisplayWaitVblankStart();
    sceGuSwapBuffers();
}

/* ── Clear ──────────────────────────────────────────────────────────────── */

void Psp_Gu_ClearColor_impl(U32 colour)
{
    sceGuClearColor(colour);
}

void Psp_Gu_ClearDepth_impl(U32 depth)
{
    sceGuClearDepth(depth);
}

void Psp_Gu_Clear_impl(I32 flags)
{
    sceGuClear(flags);
}

/* ── Render state ───────────────────────────────────────────────────────── */

void Psp_Gu_Enable_impl(I32 state)  { sceGuEnable(state);  }
void Psp_Gu_Disable_impl(I32 state) { sceGuDisable(state); }

void Psp_Gu_SetDepthFunc_impl(I32 func)
{
    sceGuDepthFunc(func);
}

void Psp_Gu_SetFrontFace_impl(I32 face)
{
    sceGuFrontFace(face);
}

void Psp_Gu_SetShadeModel_impl(I32 model)
{
    sceGuShadeModel(model);
}

void Psp_Gu_SetScissor_impl(I32 x, I32 y, I32 width, I32 height)
{
    sceGuScissor(x, y, width, height);
}

void Psp_Gu_SetDepthRange_impl(I32 near, I32 far)
{
    sceGuDepthRange(near, far);
}

void Psp_Gu_SetBlendFunc_impl(I32 op, I32 src, I32 dst, U32 srcFix, U32 dstFix)
{
    sceGuBlendFunc(op, src, dst, srcFix, dstFix);
}

/* ── Viewport ───────────────────────────────────────────────────────────── */

void Psp_Gu_SetViewport_impl(I32 cx, I32 cy, I32 width, I32 height)
{
    sceGuViewport(cx, cy, width, height);
}

void Psp_Gu_SetOffset_impl(I32 x, I32 y)
{
    sceGuOffset(x, y);
}

/* ── Texturing ──────────────────────────────────────────────────────────── */

void Psp_Gu_TexMode_impl(I32 format, I32 maxMips, I32 a2, I32 swizzle)
{
    sceGuTexMode(format, maxMips, a2, swizzle);
}

void Psp_Gu_TexImage_impl(I32 mipLevel, I32 width, I32 height, I32 bufWidth, PTR data)
{
    sceGuTexImage(mipLevel, width, height, bufWidth, (const void*)(uintptr_t)data);
}

void Psp_Gu_TexFunc_impl(I32 func, I32 colorComp)
{
    sceGuTexFunc(func, colorComp);
}

void Psp_Gu_TexFilter_impl(I32 minFilter, I32 magFilter)
{
    sceGuTexFilter(minFilter, magFilter);
}

void Psp_Gu_TexWrap_impl(I32 u, I32 v)
{
    sceGuTexWrap(u, v);
}

void Psp_Gu_TexFlush_impl(void)
{
    sceGuTexFlush();
}

/* ── Draw calls ─────────────────────────────────────────────────────────── */

void Psp_Gu_DrawArrayRaw_impl(I32 prim, I32 vtype, I32 count, PTR vertices)
{
    /* NULL indices = no index buffer (non-indexed drawing) */
    sceGuDrawArray(prim, vtype, count, NULL, (const void*)(uintptr_t)vertices);
}

void Psp_Gu_DrawArrayIndexedRaw_impl(I32 prim, I32 vtype, I32 count,
                                     PTR indices, PTR vertices)
{
    sceGuDrawArray(prim, vtype, count,
                   (const void*)(uintptr_t)indices,
                   (const void*)(uintptr_t)vertices);
}

/* ── Colour ─────────────────────────────────────────────────────────────── */

void Psp_Gu_SetColor_impl(U32 colour)
{
    sceGuColor(colour);
}
