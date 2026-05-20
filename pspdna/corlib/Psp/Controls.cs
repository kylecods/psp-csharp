// Psp/Controls.cs
// Wraps sceCtrl* functions via internal calls.
//
// Usage pattern (each frame):
//   Controls.PollPad();    // read current hardware state
//   Controls.PollLatch();  // compute pressed/released transitions
//   if (Controls.IsKeyDown(PspCtrlButtons.Cross)) { ... }
//
// PollPad reads the raw button bitmask from sceCtrlReadBufferPositive.
// PollLatch wraps sceCtrlReadLatch which gives edge-detection
// (which buttons changed state since the last poll).

using System.Runtime.CompilerServices;

namespace Psp
{
    public static class Controls
    {
        // ── Initialisation ────────────────────────────────────────────────

        /// <summary>
        /// Set the sampling mode.  Call once at startup.
        /// Analog mode gives full 0–255 stick range; digital gives only 0/128/255.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void SetSamplingMode(PspCtrlMode mode);

        // ── Per-frame polling ─────────────────────────────────────────────

        /// <summary>
        /// Read the current controller state into an internal buffer.
        /// Call at the top of each frame before querying button states.
        /// Wraps sceCtrlReadBufferPositive().
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void PollPad();

        /// <summary>
        /// Read latch (transition) data. Records which buttons changed
        /// state since the previous PollLatch call.
        /// Wraps sceCtrlReadLatch().
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void PollLatch();

        // ── State queries ─────────────────────────────────────────────────

        /// <summary>
        /// Returns true if the button was just pressed this frame
        /// (rising edge — was not held last frame, is held this frame).
        /// Uses latch data — requires PollLatch() to have been called.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool IsKeyDown(PspCtrlButtons button);

        /// <summary>
        /// Returns true while the button is physically held down.
        /// Uses pad data — requires PollPad() to have been called.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool IsKeyHeld(PspCtrlButtons button);

        /// <summary>
        /// Returns true if the button was just released this frame
        /// (falling edge — was held last frame, is released this frame).
        /// Uses latch data — requires PollLatch() to have been called.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool IsKeyUp(PspCtrlButtons button);

        // ── Analog stick ──────────────────────────────────────────────────

        /// <summary>
        /// Left analog stick X axis.
        /// Returns 0–255 (128 = centre).  Requires analog sampling mode.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetStickX();

        /// <summary>
        /// Left analog stick Y axis.
        /// Returns 0–255 (128 = centre).  Requires analog sampling mode.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetStickY();

        // ── Convenience helpers (pure C#) ─────────────────────────────────

        /// <summary>
        /// Returns stick X as a float in the range -1.0 to +1.0.
        /// Dead zone of ±0.1 centred at 128 is applied automatically.
        /// </summary>
        public static float StickXNormalised()
        {
            float raw = (GetStickX() - 128) / 128.0f;
            return (raw > -0.1f && raw < 0.1f) ? 0f : raw;
        }

        /// <summary>
        /// Returns stick Y as a float in the range -1.0 to +1.0.
        /// Dead zone of ±0.1 centred at 128 is applied automatically.
        /// </summary>
        public static float StickYNormalised()
        {
            float raw = (GetStickY() - 128) / 128.0f;
            return (raw > -0.1f && raw < 0.1f) ? 0f : raw;
        }
    }
}
