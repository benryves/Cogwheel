using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace CogwheelSlimDX.JoystickInput {

	/// <summary>
	/// Provides methods to access the joysticks connected to the system.
	/// </summary>
	public class JoystickCollection {

		/// <summary>
		/// Gets an array of available <see cref="Joystick"/> instances.
		/// </summary>
		public Joystick[] Joysticks { get; private set; }

		/// <summary>
		/// Creates an instance of the <see cref="JoystickCollection"/> class.
		/// </summary>
		public JoystickCollection() {
			var SearchJoysticks = new List<Joystick>();
			int MaxJoysticks = WinMM.joyGetNumDevs();
			for (int i = 0; i < MaxJoysticks; ++i) {
				WinMM.JOYCAPS Caps = new WinMM.JOYCAPS();
				if (WinMM.joyGetDevCaps(i, ref Caps, Marshal.SizeOf(Caps)) == WinMM.Result.JOYERR_NOERROR) {
					SearchJoysticks.Add(new Joystick(i));
				}
			}
			this.Joysticks = SearchJoysticks.ToArray();
		}

	}
}
