namespace PspSdk.Core;

/// <summary>
/// PSP error codes follow the convention: 0 = success, negative int = error.
/// Upper 16 bits encode the subsystem; lower 16 bits are the specific error number.
/// </summary>
public enum PspErrorCode : int
{
    Success         = 0,
    InvalidArgument = unchecked((int)0x80000107),
    NoMemory        = unchecked((int)0x80000002),
    Busy            = unchecked((int)0x80000203),
    NotFound        = unchecked((int)0x80000204),
    // Audio-specific
    AudioNoChannel  = unchecked((int)0x80260001),
    AudioBadSample  = unchecked((int)0x80260002),
}

/// <summary>
/// Thrown when a PSP native function returns a non-zero (negative) error code.
/// Uses a primary constructor (C# 12) — educational: concise syntax for simple exception types.
/// </summary>
public sealed class PspException(string message, int nativeCode) : Exception(message)
{
    /// <summary>Raw int error code returned by the native function.</summary>
    public int NativeCode { get; } = nativeCode;

    /// <summary>Subsystem identifier encoded in the upper 16 bits.</summary>
    public int Subsystem => (NativeCode >> 16) & 0xFFFF;

    /// <summary>Specific error number in the lower 16 bits.</summary>
    public int SpecificCode => NativeCode & 0xFFFF;

    /// <summary>
    /// Call a native function that returns int; throw PspException if result is negative.
    /// Centralises the error-check pattern so every safe wrapper stays one-liner.
    /// </summary>
    public static void ThrowIfError(int result, string operationName)
    {
        if (result < 0)
            throw new PspException(
                $"PSP operation '{operationName}' failed with code 0x{result:X8}",
                result);
    }
}
