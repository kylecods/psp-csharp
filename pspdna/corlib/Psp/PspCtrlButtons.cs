// Psp/PspCtrlButtons.cs
// Maps to the PSP_CTRL_* bitmask constants in pspctrl.h.
// Each flag corresponds to a physical button or switch on the PSP.
//
// [Flags] allows combining values: e.g. Cross | Triangle.
// In C# 7.3 with DNA corlib, this works without the BCL because
// [Flags] is a declaration attribute, not a runtime feature — the
// DNA runtime reads it during metadata parsing.

namespace Psp
{
    [System.Flags]
    public enum PspCtrlButtons : uint
    {
        None     = 0,
        Select   = 0x000001,
        Start    = 0x000008,
        Up       = 0x000010,
        Right    = 0x000020,
        Down     = 0x000040,
        Left     = 0x000080,
        LTrigger = 0x000100,
        RTrigger = 0x000200,
        Triangle = 0x001000,
        Circle   = 0x002000,
        Cross    = 0x004000,
        Square   = 0x008000,
        Home     = 0x010000,  // intercepted by firmware (triggers exit callback)
        Hold     = 0x020000,  // hold switch active
        Note     = 0x800000,  // music/note button (PSP Go)

        // Convenience combinations
        FaceButtons = Triangle | Circle | Cross | Square,
        DPad        = Up | Down | Left | Right,
        Triggers    = LTrigger | RTrigger,
    }

    public enum PspCtrlMode : int
    {
        Digital = 0,   // analog stick reports only 0/128/255
        Analog  = 1,   // full 0–255 analog range
    }
}
