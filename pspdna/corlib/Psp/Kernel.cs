// Psp/Kernel.cs
// Thin C# wrapper around PSP kernel lifecycle functions.
//
// [MethodImpl(MethodImplOptions.InternalCall)] declares that the method
// body is provided by the DNA runtime's internal call dispatch table
// (InternalCall.c), not by C# source.  At runtime, DNA looks up
// "Psp.Kernel::ExitGame" in the registration table and calls the
// registered C function pointer directly.
//
// This is the primary mechanism by which C# code calls PSP SDK functions
// in PSPDNA.  It is more reliable than [DllImport] in the DNA runtime
// because it bypasses the P/Invoke marshaling layer entirely.

using System.Runtime.CompilerServices;

namespace Psp
{
    public static class Kernel
    {
        /// <summary>
        /// Terminate the application and return control to the XMB.
        /// Wraps sceKernelExitGame().
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void ExitGame();

        /// <summary>
        /// Put the current thread to sleep for the given number of microseconds.
        /// Wraps sceKernelDelayThread(usec).
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void DelayThread(uint microseconds);

        /// <summary>
        /// Convenience: sleep for the given number of milliseconds.
        /// Implemented in C# on top of DelayThread.
        /// </summary>
        public static void Sleep(uint milliseconds)
        {
            DelayThread(milliseconds * 1000u);
        }
    }
}
