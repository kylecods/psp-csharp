namespace PspSdk.Tests.Ctrl;

using System.Runtime.InteropServices;
using PspSdk.Ctrl;

/// <summary>
/// Verifies that C# struct layouts match the C struct memory layouts from the PSP SDK.
///
/// WHY THESE TESTS MATTER:
/// If Marshal.SizeOf returns the wrong number, P/Invoke will read/write the wrong
/// memory offsets when it marshals the struct to/from native code.
/// These tests act as a compile-time-to-runtime safety net — they catch layout
/// mistakes before they cause silent memory corruption on real hardware.
///
/// The sizes come directly from the PSP SDK header comments:
///   SceCtrlData  = 4 + 4 + 1 + 1 + 1 + 1 + 6 = 20 bytes
///   SceCtrlLatch = 4 + 4 + 4 + 4              = 16 bytes
/// </summary>
public class CtrlStructTests
{
    [Fact]
    public void CtrlData_SizeIs20Bytes()
    {
        Assert.Equal(20, Marshal.SizeOf<CtrlData>());
    }

    [Fact]
    public void CtrlLatch_SizeIs16Bytes()
    {
        Assert.Equal(16, Marshal.SizeOf<CtrlLatch>());
    }

    [Fact]
    public void CtrlData_FieldOffsets_AreCorrect()
    {
        // Verify each field is at the expected byte offset within the struct.
        Assert.Equal(0,  Marshal.OffsetOf<CtrlData>(nameof(CtrlData.TimeStamp)).ToInt32());
        Assert.Equal(4,  Marshal.OffsetOf<CtrlData>(nameof(CtrlData.Buttons)).ToInt32());
        Assert.Equal(8,  Marshal.OffsetOf<CtrlData>(nameof(CtrlData.Lx)).ToInt32());
        Assert.Equal(9,  Marshal.OffsetOf<CtrlData>(nameof(CtrlData.Ly)).ToInt32());
        Assert.Equal(10, Marshal.OffsetOf<CtrlData>(nameof(CtrlData.Rx)).ToInt32());
        Assert.Equal(11, Marshal.OffsetOf<CtrlData>(nameof(CtrlData.Ry)).ToInt32());
        // Reserved[6] starts at offset 12 → struct ends at 18... but uint padding makes it 20.
        // (The C compiler adds 2 bytes of tail padding to align the struct to 4 bytes.)
    }

    [Fact]
    public void CtrlLatch_FieldOffsets_AreCorrect()
    {
        Assert.Equal(0,  Marshal.OffsetOf<CtrlLatch>(nameof(CtrlLatch.Make)).ToInt32());
        Assert.Equal(4,  Marshal.OffsetOf<CtrlLatch>(nameof(CtrlLatch.Break)).ToInt32());
        Assert.Equal(8,  Marshal.OffsetOf<CtrlLatch>(nameof(CtrlLatch.Press)).ToInt32());
        Assert.Equal(12, Marshal.OffsetOf<CtrlLatch>(nameof(CtrlLatch.Release)).ToInt32());
    }

    // ── CtrlButtons [Flags] enum ─────────────────────────────────────────────

    [Fact]
    public void CtrlButtons_BitwiseCombine_Works()
    {
        var combo = CtrlButtons.Cross | CtrlButtons.Circle;
        Assert.True(combo.HasFlag(CtrlButtons.Cross));
        Assert.True(combo.HasFlag(CtrlButtons.Circle));
        Assert.False(combo.HasFlag(CtrlButtons.Triangle));
    }

    [Fact]
    public void CtrlButtons_DPad_ContainsAllDirections()
    {
        Assert.True(CtrlButtons.DPad.HasFlag(CtrlButtons.Up));
        Assert.True(CtrlButtons.DPad.HasFlag(CtrlButtons.Down));
        Assert.True(CtrlButtons.DPad.HasFlag(CtrlButtons.Left));
        Assert.True(CtrlButtons.DPad.HasFlag(CtrlButtons.Right));
    }

    [Fact]
    public void CtrlData_IsPressed_MatchesBitMask()
    {
        // Manually construct a CtrlData with Cross + Circle pressed.
        var pad = new CtrlData
        {
            Buttons = (uint)(CtrlButtons.Cross | CtrlButtons.Circle)
        };
        Assert.True(pad.IsPressed(CtrlButtons.Cross));
        Assert.True(pad.IsPressed(CtrlButtons.Circle));
        Assert.False(pad.IsPressed(CtrlButtons.Triangle));
        Assert.False(pad.IsPressed(CtrlButtons.Cross | CtrlButtons.Triangle)); // not ALL set
    }

    [Fact]
    public void CtrlData_IsAnyPressed_MatchesBitMask()
    {
        var pad = new CtrlData { Buttons = (uint)CtrlButtons.Cross };
        Assert.True(pad.IsAnyPressed(CtrlButtons.Cross | CtrlButtons.Triangle)); // Cross matches
        Assert.False(pad.IsAnyPressed(CtrlButtons.Triangle));
    }
}
