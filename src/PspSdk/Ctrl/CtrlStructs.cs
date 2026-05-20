namespace PspSdk.Ctrl;

using System.Runtime.InteropServices;

/// <summary>
/// Maps to C struct SceCtrlData:
///   typedef struct {
///       unsigned int  TimeStamp;   // 4 bytes
///       unsigned int  Buttons;     // 4 bytes
///       unsigned char Lx;          // 1 byte
///       unsigned char Ly;          // 1 byte
///       unsigned char Rx;          // 1 byte
///       unsigned char Ry;          // 1 byte
///       unsigned char reserved[6]; // 6 bytes  ← must be present for size=20
///   } SceCtrlData;                 // total: 20 bytes
///
/// Key lessons:
///   - LayoutKind.Sequential: fields laid out in declaration order with natural C alignment.
///     Without this attribute, the JIT may reorder fields for performance — breaking P/Invoke.
///   - 'fixed byte Reserved[6]': embeds 6 bytes inline.
///     Without 'fixed', C# would store a reference (8 bytes on 64-bit) instead of 6 bytes,
///     making Marshal.SizeOf return the wrong number and corrupting the struct on native reads.
///   - The struct is 'unsafe' because it contains a fixed-size buffer.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct CtrlData
{
    /// <summary>Hardware timestamp in microseconds since last call.</summary>
    public uint TimeStamp;

    /// <summary>
    /// Bitmask of currently pressed buttons.
    /// Cast to CtrlButtons for type-safe access: (CtrlButtons)Buttons.
    /// </summary>
    public uint Buttons;

    /// <summary>Left analog stick X axis. 0-255, center ~128.</summary>
    public byte Lx;
    /// <summary>Left analog stick Y axis. 0-255, center ~128.</summary>
    public byte Ly;
    /// <summary>Right analog stick X (PSP-2000+ only).</summary>
    public byte Rx;
    /// <summary>Right analog stick Y (PSP-2000+ only).</summary>
    public byte Ry;

    /// <summary>
    /// Reserved padding bytes from the C struct.
    /// 'fixed byte' embeds the bytes inline to match the C memory layout.
    /// Omitting this field would make the struct 14 bytes instead of 20,
    /// which would silently mis-read every field after TimeStamp.
    /// </summary>
    public fixed byte Reserved[6];

    // ── Convenience members ─────────────────────────────────────────────

    /// <summary>Typed button access without a cast at every call site.</summary>
    public readonly CtrlButtons PressedButtons => (CtrlButtons)Buttons;

    /// <summary>Returns true if ALL bits in <paramref name="button"/> are set.</summary>
    public readonly bool IsPressed(CtrlButtons button) =>
        (PressedButtons & button) == button;

    /// <summary>Returns true if ANY bit in <paramref name="button"/> is set.</summary>
    public readonly bool IsAnyPressed(CtrlButtons button) =>
        (PressedButtons & button) != CtrlButtons.None;
}

/// <summary>
/// Maps to C struct SceCtrlLatch:
///   typedef struct {
///       unsigned int uiMake;    // newly pressed since last read
///       unsigned int uiBreak;   // newly released since last read
///       unsigned int uiPress;   // currently held down
///       unsigned int uiRelease; // same as uiBreak in most firmware
///   } SceCtrlLatch;             // total: 16 bytes
///
/// Latched data captures edge transitions between reads — useful for
/// "just pressed" / "just released" logic without manual diffing.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct CtrlLatch
{
    public uint Make;    // newly pressed
    public uint Break;   // newly released
    public uint Press;   // currently held
    public uint Release; // firmware alias for Break

    public readonly CtrlButtons NewlyPressed  => (CtrlButtons)Make;
    public readonly CtrlButtons NewlyReleased => (CtrlButtons)Break;
    public readonly CtrlButtons HeldButtons   => (CtrlButtons)Press;
}
