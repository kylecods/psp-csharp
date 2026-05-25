namespace PspSdk.Samples;

using PspSdk.Ctrl;

/// <summary>
/// Shows how callers use the Ctrl safe API.
/// No unsafe, no pointers, no raw ints — just typed enums and PspResult&lt;T&gt;.
///
/// Two patterns for reading input:
///   1. ReadBufferPositive — full current state, useful for held-button logic.
///   2. ReadLatch — transition data, useful for "just pressed once" events.
/// </summary>
public static class CtrlSample
{
    public static void RunInputLoop()
    {
        CtrlModule.SetSamplingMode(SamplingMode.Analog);

        // Pattern 1: TryGetValue avoids exceptions on transient read errors.
        if (CtrlModule.ReadBufferPositive().TryGetValue(out var pad))
        {
            // IsPressed tests ALL bits — useful for combos
            if (pad.IsPressed(CtrlButtons.LTrigger | CtrlButtons.RTrigger))
                Console.WriteLine("Both triggers held");

            // HasFlag tests a single bit
            if (pad.PressedButtons.HasFlag(CtrlButtons.Cross))
                Console.WriteLine("Cross pressed");

            // Analog sticks: 0-255, centre ~128
            float lx = (pad.Lx - 128) / 128f;
            float ly = (pad.Ly - 128) / 128f;
            Console.WriteLine($"Left stick: ({lx:F2}, {ly:F2})");
        }

        // Pattern 2: Latch data — fires ONCE per button press/release transition.
        if (CtrlModule.ReadLatch().TryGetValue(out var latch))
        {
            if (latch.NewlyPressed.HasFlag(CtrlButtons.Start))
                Console.WriteLine("Start was just pressed this frame");

            if (latch.NewlyReleased.HasFlag(CtrlButtons.Cross))
                Console.WriteLine("Cross was just released");
        }
    }
}
