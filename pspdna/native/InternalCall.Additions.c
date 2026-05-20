/* native/InternalCall.Additions.c
 * Registration entries for the new PSP modules added by this project.
 *
 * HOW TO ADD A NEW INTERNAL CALL:
 *
 *  1. Declare [MethodImpl(MethodImplOptions.InternalCall)] extern in C# (corlib/Psp/*.cs).
 *  2. Write the C implementation in the matching Psp.*.c file.
 *  3. Add the function prototype to the matching Psp.*.h header.
 *  4. Add the registration entry below.
 *
 * Key format:  "Namespace.TypeName::MethodName"
 *   - Namespace is the C# namespace string (e.g. "Psp")
 *   - TypeName is the C# class name (e.g. "Audio")
 *   - MethodName is the exact C# method name (case-sensitive)
 *
 * Value: a function pointer cast to (fnInternalCall).
 *   DNA calls the function with arguments matching the C# parameter list.
 *   Static methods receive no implicit 'this'.
 *   Instance methods receive HEAP_PTR pThis_ as the first argument.
 *
 * MERGE INSTRUCTIONS for PSPDNA:
 *   In the original PSPDNA repository, open native/InternalCall.c and
 *   append all entries from the table below into the existing
 *   internalCalls[] array BEFORE the { NULL, NULL } sentinel.
 *
 *   Then add the following includes at the top of that file:
 *     #include "Psp.Audio.h"
 *     #include "Psp.Gu.h"
 *     #include "Psp.Gum.h"
 */

#include "Psp.Audio.h"
#include "Psp.Gu.h"
#include "Psp.Gum.h"

/*
 * Paste the entries below into the internalCalls[] table in InternalCall.c.
 *
 * typedef tAsyncException (*fnInternalCall)(...);
 *
 * { "Key",  (fnInternalCall)FunctionPointer },
 */

/* ── Psp.Audio ──────────────────────────────────────────────────────────── */
/* { "Psp.Audio::ChReserve",            (fnInternalCall)Psp_Audio_ChReserve_impl            }, */
/* { "Psp.Audio::ChRelease",            (fnInternalCall)Psp_Audio_ChRelease_impl            }, */
/* { "Psp.Audio::OutputBlocking",       (fnInternalCall)Psp_Audio_OutputBlocking_impl       }, */
/* { "Psp.Audio::Output",               (fnInternalCall)Psp_Audio_Output_impl               }, */
/* { "Psp.Audio::OutputPanned",         (fnInternalCall)Psp_Audio_OutputPanned_impl         }, */
/* { "Psp.Audio::ChangeChannelVolume",  (fnInternalCall)Psp_Audio_ChangeChannelVolume_impl  }, */
/* { "Psp.Audio::GetChannelRestLen",    (fnInternalCall)Psp_Audio_GetChannelRestLen_impl    }, */

/* ── Psp.Gu ─────────────────────────────────────────────────────────────── */
/* { "Psp.Gu::Init",                 (fnInternalCall)Psp_Gu_Init_impl                 }, */
/* { "Psp.Gu::Term",                 (fnInternalCall)Psp_Gu_Term_impl                 }, */
/* { "Psp.Gu::StartFrame",           (fnInternalCall)Psp_Gu_StartFrame_impl           }, */
/* { "Psp.Gu::EndFrame",             (fnInternalCall)Psp_Gu_EndFrame_impl             }, */
/* { "Psp.Gu::ClearColor",           (fnInternalCall)Psp_Gu_ClearColor_impl           }, */
/* { "Psp.Gu::ClearDepth",           (fnInternalCall)Psp_Gu_ClearDepth_impl           }, */
/* { "Psp.Gu::Clear",                (fnInternalCall)Psp_Gu_Clear_impl                }, */
/* { "Psp.Gu::Enable",               (fnInternalCall)Psp_Gu_Enable_impl               }, */
/* { "Psp.Gu::Disable",              (fnInternalCall)Psp_Gu_Disable_impl              }, */
/* { "Psp.Gu::SetDepthFunc",         (fnInternalCall)Psp_Gu_SetDepthFunc_impl         }, */
/* { "Psp.Gu::SetFrontFace",         (fnInternalCall)Psp_Gu_SetFrontFace_impl         }, */
/* { "Psp.Gu::SetShadeModel",        (fnInternalCall)Psp_Gu_SetShadeModel_impl        }, */
/* { "Psp.Gu::SetScissor",           (fnInternalCall)Psp_Gu_SetScissor_impl           }, */
/* { "Psp.Gu::SetDepthRange",        (fnInternalCall)Psp_Gu_SetDepthRange_impl        }, */
/* { "Psp.Gu::SetBlendFunc",         (fnInternalCall)Psp_Gu_SetBlendFunc_impl         }, */
/* { "Psp.Gu::SetViewport",          (fnInternalCall)Psp_Gu_SetViewport_impl          }, */
/* { "Psp.Gu::SetOffset",            (fnInternalCall)Psp_Gu_SetOffset_impl            }, */
/* { "Psp.Gu::TexMode",              (fnInternalCall)Psp_Gu_TexMode_impl              }, */
/* { "Psp.Gu::TexImage",             (fnInternalCall)Psp_Gu_TexImage_impl             }, */
/* { "Psp.Gu::TexFunc",              (fnInternalCall)Psp_Gu_TexFunc_impl              }, */
/* { "Psp.Gu::TexFilter",            (fnInternalCall)Psp_Gu_TexFilter_impl            }, */
/* { "Psp.Gu::TexWrap",              (fnInternalCall)Psp_Gu_TexWrap_impl              }, */
/* { "Psp.Gu::TexFlush",             (fnInternalCall)Psp_Gu_TexFlush_impl             }, */
/* { "Psp.Gu::DrawArrayRaw",         (fnInternalCall)Psp_Gu_DrawArrayRaw_impl         }, */
/* { "Psp.Gu::DrawArrayIndexedRaw",  (fnInternalCall)Psp_Gu_DrawArrayIndexedRaw_impl  }, */
/* { "Psp.Gu::SetColor",             (fnInternalCall)Psp_Gu_SetColor_impl             }, */

/* ── Psp.Gum ────────────────────────────────────────────────────────────── */
/* { "Psp.Gum::MatrixMode",    (fnInternalCall)Psp_Gum_MatrixMode_impl    }, */
/* { "Psp.Gum::LoadIdentity",  (fnInternalCall)Psp_Gum_LoadIdentity_impl  }, */
/* { "Psp.Gum::PushMatrix",    (fnInternalCall)Psp_Gum_PushMatrix_impl    }, */
/* { "Psp.Gum::PopMatrix",     (fnInternalCall)Psp_Gum_PopMatrix_impl     }, */
/* { "Psp.Gum::Translate",     (fnInternalCall)Psp_Gum_Translate_impl     }, */
/* { "Psp.Gum::Scale",         (fnInternalCall)Psp_Gum_Scale_impl         }, */
/* { "Psp.Gum::RotateX",       (fnInternalCall)Psp_Gum_RotateX_impl       }, */
/* { "Psp.Gum::RotateY",       (fnInternalCall)Psp_Gum_RotateY_impl       }, */
/* { "Psp.Gum::RotateZ",       (fnInternalCall)Psp_Gum_RotateZ_impl       }, */
/* { "Psp.Gum::Perspective",   (fnInternalCall)Psp_Gum_Perspective_impl   }, */
/* { "Psp.Gum::Ortho",         (fnInternalCall)Psp_Gum_Ortho_impl         }, */
/* { "Psp.Gum::DrawArrayRaw",  (fnInternalCall)Psp_Gum_DrawArrayRaw_impl  }, */
