namespace PspSdk.Tests.Core;

using PspSdk.Core;

/// <summary>
/// Unit tests for PspResult&lt;T&gt; and PspResult.
/// These run entirely on desktop — no PSP hardware or DllImport resolution needed.
///
/// They verify the Result pattern behaves correctly before any native code is involved.
/// </summary>
public class PspResultTests
{
    // ── PspResult<T> ─────────────────────────────────────────────────────────

    [Fact]
    public void GenericResult_Success_IsSuccess()
    {
        var result = new PspResult<int>(42, nativeCode: 0);
        Assert.True(result.IsSuccess);
        Assert.False(result.IsError);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void GenericResult_Error_IsError()
    {
        PspResult<int> result = unchecked((int)0x80000107); // implicit conversion
        Assert.True(result.IsError);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void GenericResult_Error_ValueThrows()
    {
        PspResult<int> result = -1;
        var ex = Assert.Throws<PspException>(() => result.Value);
        Assert.Equal(-1, ex.NativeCode);
    }

    [Fact]
    public void GenericResult_TryGetValue_ReturnsTrueOnSuccess()
    {
        var result = new PspResult<string>("hello", 0);
        Assert.True(result.TryGetValue(out var val));
        Assert.Equal("hello", val);
    }

    [Fact]
    public void GenericResult_TryGetValue_ReturnsFalseOnError()
    {
        PspResult<string> result = -5;
        Assert.False(result.TryGetValue(out var val));
        Assert.Null(val);
    }

    [Fact]
    public void GenericResult_ValueOrDefault_ReturnsDefaultOnError()
    {
        PspResult<int> result = -1;
        Assert.Equal(0, result.ValueOrDefault); // default(int) == 0
    }

    // ── PspResult (non-generic) ──────────────────────────────────────────────

    [Fact]
    public void Result_Success_IsSuccess()
    {
        PspResult result = 0;
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Result_Error_ThrowIfError()
    {
        PspResult result = -1;
        Assert.Throws<PspException>(() => result.ThrowIfError("TestOp"));
    }

    [Fact]
    public void Result_Success_ThrowIfError_DoesNotThrow()
    {
        PspResult result = 0;
        result.ThrowIfError("NoOp"); // should not throw
    }

    // ── PspException ──────────────────────────────────────────────────────────

    [Fact]
    public void PspException_SubsystemAndSpecificCode()
    {
        // Upper 16 bits = 0x8026 → Audio subsystem; lower 16 bits = 0x0001
        int code = unchecked((int)0x80260001);
        var ex = new PspException("test", code);

        Assert.Equal(code, ex.NativeCode);
        // Subsystem is masked by lower 16 bits of upper half (0x8026 & 0xFFFF = 0x8026)
        // But the property does (code >> 16) & 0xFFFF which for 0x80260001 is 0x8026
        Assert.Equal(0x8026, ex.Subsystem);
        Assert.Equal(0x0001, ex.SpecificCode);
    }

    [Fact]
    public void ThrowIfError_NegativeCode_Throws()
    {
        var ex = Assert.Throws<PspException>(() =>
            PspException.ThrowIfError(-1, "SomeOp"));
        Assert.Contains("SomeOp", ex.Message);
    }

    [Fact]
    public void ThrowIfError_ZeroCode_DoesNotThrow()
    {
        PspException.ThrowIfError(0, "SomeOp"); // no exception expected
    }
}
