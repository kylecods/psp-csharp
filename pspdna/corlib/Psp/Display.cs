// Psp/Display.cs
// Wraps sceDisplay* functions via internal calls.
//
// Display synchronisation is the most common use — games call
// WaitVblankStart() once per frame so rendering is tied to the 60 Hz
// screen refresh and the screen never shows a partially-written buffer.

using System.Runtime.CompilerServices;

namespace Psp
{
    public static class Display
    {
        /// <summary>
        /// Block until the start of the next vertical blanking interval.
        /// Call once per frame after all draw calls but before SwapBuffers
        /// to cap the frame rate at 60 fps and prevent tearing.
        /// Wraps sceDisplayWaitVblankStart().
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void WaitVblankStart();

        /// <summary>
        /// Returns true if the display is currently in the vblank period.
        /// Non-blocking alternative to WaitVblankStart().
        /// Wraps sceDisplayIsVblank() != 0.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool IsVblank();

        /// <summary>
        /// Returns the number of vblank pulses since the PSP booted.
        /// Useful for frame counting and profiling.
        /// Wraps sceDisplayGetVcount().
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern uint GetVcount();
    }
}
