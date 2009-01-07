using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace BeeDevelopment.Cogwheel.JoystickInput {

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
		/// <param name="skipXInputDevices">Set to true to ignore XInput devices.</param>
		public JoystickCollection(bool skipXInputDevices) {
			var SearchJoysticks = new List<Joystick>();
			int MaxJoysticks = WinMM.joyGetNumDevs();
			for (int i = 0; i < MaxJoysticks; ++i) {
				WinMM.JOYCAPS Caps = new WinMM.JOYCAPS();
				if (WinMM.joyGetDevCaps(i, ref Caps, Marshal.SizeOf(Caps)) == WinMM.Result.JOYERR_NOERROR) {
					var J = new Joystick(i);
					if (!(skipXInputDevices && J.IsXInputDevice)) SearchJoysticks.Add(J);
				}
			}
			this.Joysticks = SearchJoysticks.ToArray();
		}


		/// <summary>
		/// Creates an instance of the <see cref="JoystickCollection"/> class.
		/// </summary>
		public JoystickCollection()
			: this(false) {
		}

	}
}
