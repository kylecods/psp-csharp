namespace PspSdk.Core;

/// <summary>
/// Wraps a PSP native return value that is either a success value or an error code.
/// PSP convention: negative int = error, non-negative = a meaningful value or zero.
///
/// Use this instead of exceptions when callers may want to inspect the error code
/// without try/catch — a lighter-weight "Railway Oriented" style for interop.
/// </summary>
public readonly struct PspResult<T>
{
    private readonly T? _value;

    public int  NativeCode { get; }
    public bool IsSuccess  => NativeCode >= 0;
    public bool IsError    => NativeCode < 0;

    /// <summary>Constructor for a successful result that carries a value.</summary>
    public PspResult(T value, int nativeCode = 0)
    {
        _value     = value;
        NativeCode = nativeCode;
    }

    /// <summary>Constructor for an error result (no value).</summary>
    public PspResult(int errorCode)
    {
        _value     = default;
        NativeCode = errorCode;
    }

    /// <summary>Returns the value; throws PspException if this is an error result.</summary>
    public T Value => IsSuccess
        ? _value!
        : throw new PspException(
            $"Cannot access Value of failed PspResult (0x{NativeCode:X8})", NativeCode);

    /// <summary>Returns the value or default(T) without throwing.</summary>
    public T? ValueOrDefault => _value;

    /// <summary>Try-pattern: returns false and leaves value at default on error.</summary>
    public bool TryGetValue(out T value)
    {
        value = _value!;
        return IsSuccess;
    }

    /// <summary>Allows writing: PspResult&lt;int&gt; r = errorCode;</summary>
    public static implicit operator PspResult<T>(int errorCode) => new(errorCode);
}

/// <summary>
/// Non-generic variant for "void" native operations (return is only success/error).
/// </summary>
public readonly struct PspResult
{
    public int  NativeCode { get; }
    public bool IsSuccess  => NativeCode >= 0;
    public bool IsError    => NativeCode < 0;

    public PspResult(int code) => NativeCode = code;

    public void ThrowIfError(string operationName) =>
        PspException.ThrowIfError(NativeCode, operationName);

    public static implicit operator PspResult(int code) => new(code);
}
