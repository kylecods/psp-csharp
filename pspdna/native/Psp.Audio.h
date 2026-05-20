/* native/Psp.Audio.h
 * Internal call declarations for Psp.Audio.
 *
 * Each function matches one [MethodImpl(InternalCall)] method in
 * pspdna/corlib/Psp/Audio.cs.  The registration key in InternalCall.c
 * must be "Psp.Audio::<MethodName>" exactly (case-sensitive).
 *
 * The short[] array parameters (buf) are passed as HEAP_PTR — a managed
 * heap reference.  The C implementation must call DNA's array accessor
 * to get the raw data pointer, then offset by (offset * sizeof(short))
 * and pass count * sizeof(short) * channels bytes to sceAudio.
 */

#pragma once
#include "Types.h"

/* int ChReserve(int channel, int sampleCount, int format) */
I32 Psp_Audio_ChReserve_impl(I32 channel, I32 sampleCount, I32 format);

/* int ChRelease(int channel) */
I32 Psp_Audio_ChRelease_impl(I32 channel);

/* void OutputBlocking(int ch, int vol, short[] buf, int offset, int count) */
void Psp_Audio_OutputBlocking_impl(I32 channel, I32 vol,
                                   HEAP_PTR buf, I32 offset, I32 count);

/* void Output(int ch, int vol, short[] buf, int offset, int count) */
void Psp_Audio_Output_impl(I32 channel, I32 vol,
                           HEAP_PTR buf, I32 offset, I32 count);

/* void OutputPanned(int ch, int lv, int rv, short[] buf, int offset, int count) */
void Psp_Audio_OutputPanned_impl(I32 channel, I32 leftVol, I32 rightVol,
                                 HEAP_PTR buf, I32 offset, I32 count);

/* int ChangeChannelVolume(int channel, int leftVol, int rightVol) */
I32 Psp_Audio_ChangeChannelVolume_impl(I32 channel, I32 leftVol, I32 rightVol);

/* int GetChannelRestLen(int channel) */
I32 Psp_Audio_GetChannelRestLen_impl(I32 channel);
