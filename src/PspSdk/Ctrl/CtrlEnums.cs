namespace PspSdk.Ctrl;

/// <summary>
/// PSP controller button bitmask constants (PSP_CTRL_*).
///
/// [Flags] attribute teaches three things:
///   1. Enum.ToString() prints "Cross | Circle" instead of "24576" for combined values.
///   2. Bitwise OR combinations (Cross | Circle) are valid enum values.
///   3. HasFlag() tests individual bits in a combined value.
///
/// Values match the bit positions in SceCtrlData.Buttons from the SDK header.
/// </summary>
[Flags]
public enum CtrlButtons : uint
{
    None      = 0,
    Select    = 0x000001,
    Start     = 0x000008,
    Up        = 0x000010,
    Right     = 0x000020,
    Down      = 0x000040,
    Left      = 0x000080,
    LTrigger  = 0x000100,
    RTrigger  = 0x000200,
    Triangle  = 0x001000,
    Circle    = 0x002000,
    Cross     = 0x004000,
    Square    = 0x008000,
    Home      = 0x010000,
    Hold      = 0x020000,
    /// <summary>All four d-pad directions combined.</summary>
    DPad        = Up | Down | Left | Right,
    /// <summary>All four face buttons combined.</summary>
    FaceButtons = Triangle | Circle | Cross | Square,
    /// <summary>Both shoulder triggers combined.</summary>
    BothTriggers = LTrigger | RTrigger,
}

/// <summary>
/// Controller sampling mode (PSP_CTRL_MODE_*).
/// Digital disables the analog stick (reads as 0 or max); Analog enables it.
/// </summary>
public enum SamplingMode : int
{
    Digital = 0,
    Analog  = 1,
}
