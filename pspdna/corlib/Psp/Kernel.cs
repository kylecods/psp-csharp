using System.Runtime.CompilerServices;

namespace Psp
{
    public static class Kernel
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        extern public static void ExitGame();

        /// <summary>
        /// Suspend the current thread for the specified number of microseconds.
        /// Wraps sceKernelDelayThread(microseconds).
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        extern public static void Sleep(int microseconds);
    }
}
