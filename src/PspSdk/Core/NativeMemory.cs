namespace PspSdk.Core;

using System.Runtime.InteropServices;

/// <summary>
/// Utilities for working with unmanaged memory in P/Invoke contexts.
///
/// Three techniques demonstrated:
///   1. GCHandle.Alloc(Pinned) — pin a managed array so the GC cannot move it
///      during a native call. Without pinning, the GC may relocate the array
///      mid-call, causing the native code to read/write wrong memory.
///   2. IntPtr — the safe, opaque representation of a native pointer in C#.
///   3. PinnedArray&lt;T&gt; : IDisposable — RAII wrapper so pinning is always paired
///      with unpinning, even in the presence of exceptions.
/// </summary>
public static class NativeMemoryHelper
{
    /// <summary>
    /// Pins a managed array and returns its address as IntPtr plus the GCHandle.
    /// IMPORTANT: the caller is responsible for calling handle.Free() when done.
    /// Prefer using PinnedArray&lt;T&gt; (see below) to avoid handle leaks.
    /// </summary>
    public static (IntPtr address, GCHandle handle) PinArray<T>(T[] array)
        where T : unmanaged
    {
        var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
        return (handle.AddrOfPinnedObject(), handle);
    }

    /// <summary>
    /// Returns a using-scope-safe PinnedArray&lt;T&gt; that unpins on Dispose.
    /// </summary>
    public static PinnedArray<T> Pin<T>(T[] array) where T : unmanaged => new(array);
}

/// <summary>
/// RAII (Resource Acquisition Is Initialisation) wrapper over a pinned GCHandle.
/// Usage:
///   using var pinned = NativeMemoryHelper.Pin(myArray);
///   NativeFunc(pinned.Pointer);
/// The array is unpinned automatically when the using block exits.
/// </summary>
public sealed class PinnedArray<T> : IDisposable where T : unmanaged
{
    private GCHandle _handle;
    private bool     _disposed;

    internal PinnedArray(T[] array)
    {
        _handle = GCHandle.Alloc(array, GCHandleType.Pinned);
        Pointer = _handle.AddrOfPinnedObject();
    }

    /// <summary>The native address of the pinned array's first element.</summary>
    public IntPtr Pointer { get; }

    /// <summary>
    /// Unsafe typed pointer to the first element.
    /// Requires an 'unsafe' context at the call site.
    /// </summary>
    public unsafe T* UnsafePointer => (T*)Pointer.ToPointer();

    public void Dispose()
    {
        if (!_disposed && _handle.IsAllocated)
        {
            _handle.Free();
            _disposed = true;
        }
    }
}
