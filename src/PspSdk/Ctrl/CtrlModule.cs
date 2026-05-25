namespace PspSdk.Ctrl;

using PspSdk.Core;
using PspSdk.Ctrl.Native;

/// <summary>
/// Safe public API for PSP controller input.
///
/// Design goals demonstrated here:
///   • No 'unsafe' visible to callers — raw pointer patterns are hidden inside CtrlNative.
///   • Typed SamplingMode enum replaces the raw int parameter.
///   • Return values are PspResult&lt;T&gt; so callers can decide whether to throw or inspect.
///   • int error codes never leak into the calling code.
/// </summary>
public static class CtrlModule
{
    /// <summary>
    /// Sets the controller sampling mode (digital or analog).
    /// Throws PspException on native error.
    /// </summary>
    public static void SetSamplingMode(SamplingMode mode)
    {
        int result = CtrlNative.SetSamplingMode((int)mode);
        PspException.ThrowIfError(result, nameof(SetSamplingMode));
    }

    /// <summary>
    /// Blocking read: waits until new controller data is available, then returns it.
    /// Uses the 'ref' pattern (Pattern A) — no unsafe at the call site.
    /// </summary>
    public static PspResult<CtrlData> ReadBufferPositive()
    {
        CtrlData data   = default;
        int      result = CtrlNative.ReadBufferPositive(ref data, 1);
        return result < 0
            ? new PspResult<CtrlData>(result)
            : new PspResult<CtrlData>(data, result);
    }

    /// <summary>
    /// Non-blocking read: returns the most recently sampled state immediately.
    /// </summary>
    public static PspResult<CtrlData> PeekBufferPositive()
    {
        CtrlData data   = default;
        int      result = CtrlNative.PeekBufferPositive(ref data, 1);
        return result < 0
            ? new PspResult<CtrlData>(result)
            : new PspResult<CtrlData>(data, result);
    }

    /// <summary>
    /// Reads transition (latch) data: which buttons were newly pressed or released
    /// since the last call to ReadLatch.
    /// Uses the '[Out] out' pattern (Pattern C).
    /// </summary>
    public static PspResult<CtrlLatch> ReadLatch()
    {
        int result = CtrlNative.ReadLatch(out CtrlLatch latch);
        return result < 0
            ? new PspResult<CtrlLatch>(result)
            : new PspResult<CtrlLatch>(latch, result);
    }
}
