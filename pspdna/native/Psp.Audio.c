/* native/Psp.Audio.c
 * Internal call implementations for Psp.Audio.
 *
 * Audio buffer management:
 *   The C# short[] is a DNA managed array.  DNA lays arrays out in its GC heap as:
 *     [header (HEAP_PTR)] [length (U32)] [element0] [element1] ...
 *   The data starts at HEAP_PTR + 8 bytes (past the vtable pointer and length).
 *   We cast the managed pointer to a byte* and advance past the header.
 *
 *   PSP DMA requires the audio buffer to be 64-byte aligned.  DNA's GC heap
 *   allocates arrays on 8-byte boundaries.  For reliable PSP operation, the
 *   managed array should be declared as a class-level field (not local) so it
 *   is not relocated.  If alignment issues arise, use memalign(64, size) on
 *   the C side and pass the raw address as System.IntPtr from C#.
 *
 * sceAudio sample count rules:
 *   - Must be a multiple of 64
 *   - Must be in the range [64, 65472]
 *   - Stereo buffer size = sampleCount * 2 * sizeof(short)
 *   - Mono  buffer size = sampleCount * sizeof(short)
 */

#include <pspaudio.h>
#include <string.h>
#include "Types.h"
#include "Psp.Audio.h"

/* ── DNA heap layout for a managed array ────────────────────────────────── *
 * Offset 0: vtable pointer  (4 bytes)                                       *
 * Offset 4: array length    (4 bytes, U32)                                  *
 * Offset 8: element[0]      (elements start here)                           *
 * ─────────────────────────────────────────────────────────────────────────*/
#define DNA_ARRAY_DATA_OFFSET 8

/* Extract the raw data pointer from a DNA managed short[]. */
static short* GetArrayData(HEAP_PTR arr, I32 offset)
{
    if (arr == 0) return NULL;
    /* Cast HEAP_PTR to byte* and skip the DNA array header. */
    unsigned char* base = (unsigned char*)(uintptr_t)arr;
    short* data = (short*)(base + DNA_ARRAY_DATA_OFFSET);
    return data + offset;
}

/* ── Internal call implementations ─────────────────────────────────────── */

I32 Psp_Audio_ChReserve_impl(I32 channel, I32 sampleCount, I32 format)
{
    return sceAudioChReserve(channel, sampleCount, format);
}

I32 Psp_Audio_ChRelease_impl(I32 channel)
{
    return sceAudioChRelease(channel);
}

void Psp_Audio_OutputBlocking_impl(I32 channel, I32 vol,
                                   HEAP_PTR buf, I32 offset, I32 count)
{
    /* count is in stereo pairs; sceAudioOutputBlocking expects the raw pointer.
     * The hardware reads sampleCount * 2 * 2 bytes (stereo, 16-bit each). */
    short* data = GetArrayData(buf, offset);
    sceAudioOutputBlocking(channel, vol, data);
}

void Psp_Audio_Output_impl(I32 channel, I32 vol,
                           HEAP_PTR buf, I32 offset, I32 count)
{
    short* data = GetArrayData(buf, offset);
    sceAudioOutput(channel, vol, data);
}

void Psp_Audio_OutputPanned_impl(I32 channel, I32 leftVol, I32 rightVol,
                                 HEAP_PTR buf, I32 offset, I32 count)
{
    short* data = GetArrayData(buf, offset);
    sceAudioOutputPanned(channel, leftVol, rightVol, data);
}

I32 Psp_Audio_ChangeChannelVolume_impl(I32 channel, I32 leftVol, I32 rightVol)
{
    return sceAudioChangeChannelVolume(channel, leftVol, rightVol);
}

I32 Psp_Audio_GetChannelRestLen_impl(I32 channel)
{
    return sceAudioGetChannelRestLen(channel);
}
