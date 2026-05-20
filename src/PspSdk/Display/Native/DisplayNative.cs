// Dual-mode: [DllImport] on desktop CLR, [MethodImpl(InternalCall)] under PSPDNA.
// See CtrlNative.cs for a full explanation of the #if PSPDNA pattern.

namespace PspSdk.Display.Native;

#if PSPDNA
using System.Runtime.CompilerServices;
#else
using System.Runtime.InteropServices;
#endif

/// <summary>
/// Raw native declarations for sceDisplay.
///
/// Educational highlights:
///
///   1. LibName is the native module name the PSP kernel exports.
///      On desktop this won't resolve at runtime (expected — no PSP hardware);
///      the project still compiles because DllImport is resolved lazily.
///
///   2. EntryPoint must match the C function name EXACTLY (case-sensitive).
///      The C# method name can be anything — here we strip the 'sceDisplay' prefix.
///
///   3. CallingConvention.Cdecl mirrors the MIPS O32 ABI used by the PSP SDK.
///
///   4. SetFrameBuf is declared TWICE with different parameter types for 'topaddr':
///      - IntPtr version: safe, no 'unsafe' needed, opaque pointer.
///      - unsafe void* version: allows pointer arithmetic and typed casts.
///      Both map to the same native symbol via EntryPoint — the runtime resolves
///      whichever overload is called.
///
///   5. IsVblank returns int (C int) not bool — demonstrated in DisplayModule
///      where we convert with '!= 0'.
///
///   6. Under PSPDNA: [DllImport] is replaced with [MethodImpl(InternalCall)].
///      The EntryPoint attribute is not needed — DNA looks up the method by its
///      fully-qualified name "PspSdk.Display.Native.DisplayNative::SetMode".
/// </summary>
internal static class DisplayNative
{
#if !PSPDNA
    private const string LibName = "sceDisplay";
#endif

    // ── SetMode ───────────────────────────────────────────────────────────

    /// <summary>
    /// int sceDisplaySetMode(int mode, int width, int height);
    /// Standard PSP resolution: width=480, height=272.
    /// </summary>
#if PSPDNA
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern int SetMode(int mode, int width, int height);
#else
    [DllImport(LibName,
        EntryPoint        = "sceDisplaySetMode",
        CallingConvention = CallingConvention.Cdecl)]
    internal static extern int SetMode(int mode, int width, int height);
#endif

    // ── SetFrameBuf: two overloads mapping to the same native function ────

    /// <summary>
    /// IntPtr overload — safe, no 'unsafe' keyword required.
    /// Use when the framebuffer address comes from native code (e.g., VRAM offset).
    /// </summary>
#if PSPDNA
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern int SetFrameBuf(IntPtr topaddr, int bufferwidth,
                                           int pixelformat, int sync);
#else
    [DllImport(LibName,
        EntryPoint        = "sceDisplaySetFrameBuf",
        CallingConvention = CallingConvention.Cdecl)]
    internal static extern int SetFrameBuf(IntPtr topaddr, int bufferwidth,
                                           int pixelformat, int sync);
#endif

    /// <summary>
    /// unsafe void* overload — requires 'unsafe' context.
    /// Use when the caller already has a raw pointer (e.g., fixed managed array).
    /// </summary>
#if PSPDNA
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern unsafe int SetFrameBuf(void* topaddr, int bufferwidth,
                                                  int pixelformat, int sync);
#else
    [DllImport(LibName,
        EntryPoint        = "sceDisplaySetFrameBuf",
        CallingConvention = CallingConvention.Cdecl)]
    internal static extern unsafe int SetFrameBuf(void* topaddr, int bufferwidth,
                                                  int pixelformat, int sync);
#endif

    // ── Counters and sync ─────────────────────────────────────────────────

    /// <summary>
    /// unsigned int sceDisplayGetVcount();
    /// Monotonically increasing frame counter — no error return possible.
    /// Returns uint directly; no PspResult wrapping needed.
    /// </summary>
#if PSPDNA
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern uint GetVcount();
#else
    [DllImport(LibName,
        EntryPoint        = "sceDisplayGetVcount",
        CallingConvention = CallingConvention.Cdecl)]
    internal static extern uint GetVcount();
#endif

    /// <summary>
    /// int sceDisplayWaitVblankStart();
    /// Blocks until the start of the next vertical blank interval (vsync).
    /// </summary>
#if PSPDNA
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern int WaitVblankStart();
#else
    [DllImport(LibName,
        EntryPoint        = "sceDisplayWaitVblankStart",
        CallingConvention = CallingConvention.Cdecl)]
    internal static extern int WaitVblankStart();
#endif

    /// <summary>
    /// int sceDisplayIsVblank();
    /// Returns non-zero if currently in vblank, 0 otherwise.
    /// C uses int-as-bool; DisplayModule converts to C# bool with '!= 0'.
    /// </summary>
#if PSPDNA
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern int IsVblank();
#else
    [DllImport(LibName,
        EntryPoint        = "sceDisplayIsVblank",
        CallingConvention = CallingConvention.Cdecl)]
    internal static extern int IsVblank();
#endif
}
