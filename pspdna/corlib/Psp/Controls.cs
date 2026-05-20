// PspCtrlButtons and PspCtrlMode are defined in PspCtrlButtons.cs.
// This file contains only the Controls class (InternalCall wrappers).

using System.Runtime.CompilerServices;

namespace Psp
{
	public static class Controls
    {
        /// <summary>
        /// Set the controller sampling mode.
        /// Digital = D-pad only (analog stick clamped to 0/128/255).
        /// Analog  = full 0-255 range from the analog stick.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int SetSamplingMode(PspCtrlMode mode);

		[MethodImpl(MethodImplOptions.InternalCall)]
		extern public static int GetJoystickX();


	    [MethodImpl(MethodImplOptions.InternalCall)]
		extern public static int GetJoystickY();


		[MethodImpl(MethodImplOptions.InternalCall)]
		extern public static void PollPad();

		[MethodImpl(MethodImplOptions.InternalCall)]
		extern public static void PollLatch();


		[MethodImpl(MethodImplOptions.InternalCall)]
		extern static int NativeIsKeyHold(int key); // returns 1(true) if key is down

		public static bool IsKeyHeld(PspCtrlButtons button)
        {
			return (NativeIsKeyHold((int)button) > 0);
        }

		[MethodImpl(MethodImplOptions.InternalCall)]
		extern static int NativeIsKeyDown(int key);

		public static bool IsKeyDown(PspCtrlButtons button)
		{
			return (NativeIsKeyDown((int)button) > 0);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		extern static int NativeIsKeyUp(int key);

		public static bool IsKeyUp(PspCtrlButtons button)
		{
			return (NativeIsKeyUp((int)button) > 0);
		}
	}
}
