namespace PspSdk.Ctrl.Native;

// ── Dual-mode P/Invoke vs InternalCall ───────────────────────────────────────
//
// #if PSPDNA (defined when building for PSP via the PSPDNA runtime):
//   Uses [MethodImpl(InternalCall)] — the DNA interpreter dispatches the call
//   directly to the registered C function in InternalCall.c.  No marshaling
//   overhead.  Most reliable mechanism in the DNA runtime.
//
// #else (building for desktop CLR — tests, tools, documentation):
//   Uses [DllImport] — standard P/Invoke.  Allows running on Windows/Linux/macOS
//   where the PSP firmware libraries are not present, but lets the struct layout
//   tests (Marshal.SizeOf) and integration tests run normally.
//
// The public API in CtrlModule.cs is identical regardless of which path is
// compiled.  The switch is invisible to the caller.

#if PSPDNA
using System.Runtime.CompilerServices;
#else
using System.Runtime.InteropServices;
#endif

/// <summary>
/// Raw native declarations for sceCtrl.
///
/// Three interop patterns are demonstrated in this file:
///
///   Pattern A — ref parameter:
///     Passes a pointer to the struct automatically.
///     No 'unsafe' needed at declaration or call site.
///
///   Pattern B — unsafe struct* parameter:
///     Requires 'unsafe'; allows count &gt; 1 or pointer arithmetic.
///
///   Pattern C — [Out] out parameter:
///     Marks the struct as write-only; marshaler skips copying on entry.
///
/// All methods are internal — callers use CtrlModule, the safe public wrapper.
/// </summary>
internal static class CtrlNative
{
#if !PSPDNA
    private const string LibName = "sceCtrl";
#endif

    // ── SetSamplingMode ───────────────────────────────────────────────────

    /// <summary>
    /// int sceCtrlSetSamplingMode(int mode);
    /// Simple scalar — no special marshaling needed in either mode.
    /// </summary>
#if PSPDNA
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern int SetSamplingMode(int mode);
#else
    [DllImport(LibName,
        EntryPoint        = "sceCtrlSetSamplingMode",
        CallingConvention = CallingConvention.Cdecl)]
    internal static extern int SetSamplingMode(int mode);
#endif

    // ── ReadBufferPositive ────────────────────────────────────────────────

    /// <summary>
    /// int sceCtrlReadBufferPositive(SceCtrlData* pad_data, int count);
    ///
    /// Pattern A: 'ref CtrlData' — runtime passes &amp;padData automatically.
    /// Safe, no unsafe keyword, works fine for the common count=1 case.
    /// </summary>
#if PSPDNA
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern int ReadBufferPositive(ref CtrlData padData, int count);
#else
    [DllImport(LibName,
        EntryPoint        = "sceCtrlReadBufferPositive",
        CallingConvention = CallingConvention.Cdecl)]
    internal static extern int ReadBufferPositive(ref CtrlData padData, int count);
#endif

    /// <summary>
    /// Pattern B: unsafe CtrlData* — lets callers pass into an array for count &gt; 1.
    /// </summary>
#if PSPDNA
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern unsafe int ReadBufferPositive(CtrlData* padData, int count);
#else
    [DllImport(LibName,
        EntryPoint        = "sceCtrlReadBufferPositive",
        CallingConvention = CallingConvention.Cdecl)]
    internal static extern unsafe int ReadBufferPositive(CtrlData* padData, int count);
#endif

    // ── PeekBufferPositive ────────────────────────────────────────────────

    /// <summary>
    /// int sceCtrlPeekBufferPositive(SceCtrlData* pad_data, int count);
    /// Non-blocking: returns the most recently sampled data without waiting.
    /// </summary>
#if PSPDNA
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern int PeekBufferPositive(ref CtrlData padData, int count);
#else
    [DllImport(LibName,
        EntryPoint        = "sceCtrlPeekBufferPositive",
        CallingConvention = CallingConvention.Cdecl)]
    internal static extern int PeekBufferPositive(ref CtrlData padData, int count);
#endif

    // ── ReadLatch ─────────────────────────────────────────────────────────

    /// <summary>
    /// int sceCtrlReadLatch(SceCtrlLatch* latch);
    ///
    /// Pattern C: [Out] + out parameter.
    /// [Out] signals the marshaler that latch is write-only on entry.
    /// 'out' enforces definite assignment in C#.
    /// </summary>
#if PSPDNA
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern int ReadLatch(out CtrlLatch latch);
#else
    [DllImport(LibName,
        EntryPoint        = "sceCtrlReadLatch",
        CallingConvention = CallingConvention.Cdecl)]
    internal static extern int ReadLatch([Out] out CtrlLatch latch);
#endif
}
