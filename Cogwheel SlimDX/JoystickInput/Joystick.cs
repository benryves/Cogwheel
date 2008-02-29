using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace CogwheelSlimDX.JoystickInput {
	/// <summary>
	/// Provides methods for retrieving the state from a joystick.
	/// </summary>
	public class Joystick {

		/// <summary>
		/// Gets the identifier of the <see cref="Joystick"/>.
		/// </summary>
		/// <remarks>This is the identifier as used by the Windows multimedia joystick functions.</remarks>
		public int Id { get; private set; }

		/// <summary>
		/// Gets the name of the joystick.
		/// </summary>
		public string Name { get; private set; }

		private WinMM.JOYCAPS Caps { get; set; }

		/// <summary>
		/// Creates an instance of the <see cref="Joystick"/> class.
		/// </summary>
		/// <param name="id">The identifier of the joystick.</param>
		public Joystick(int id) {
			this.Id = id;
			WinMM.JOYCAPS Caps = new WinMM.JOYCAPS();
			if (WinMM.joyGetDevCaps(this.Id, ref Caps, Marshal.SizeOf(Caps)) != WinMM.Result.JOYERR_NOERROR) throw new InvalidOperationException();
			this.Caps = Caps;
			this.Name = Caps.szPname;
		}

		/// <summary>
		/// Gets the state of the joystick.
		/// </summary>
		/// <returns>The state of the joystick or <c>null</c>.</returns>
		public JoystickState GetState() {
			var	Info = new WinMM.JOYINFOEX();
			Info.Size = Marshal.SizeOf(Info);
			Info.Flags = WinMM.InfoFlags.JOY_RETURNBUTTONS;
			var Result = WinMM.joyGetPosEx(this.Id, ref Info);
			if (Result != WinMM.Result.JOYERR_NOERROR) return null;
			var State = new JoystickState();
			State.Buttons = (Buttons)Info.Buttons;

			State.XAxis = this.Caps.wNumAxes > 0 ? GetNormalisedAxisPoint(Info.X, this.Caps.wXmin, this.Caps.wXmax) : 0f;
			State.YAxis = this.Caps.wNumAxes > 1 ? GetNormalisedAxisPoint(Info.Y, this.Caps.wYmin, this.Caps.wYmax) : 0f;
			State.ZAxis = this.Caps.wNumAxes > 2 ? GetNormalisedAxisPoint(Info.Z, this.Caps.wZmin, this.Caps.wZmax) : 0f;
			State.Rudder = this.Caps.wNumAxes > 3 ? GetNormalisedAxisPoint(Info.Rudder, this.Caps.wRmin, this.Caps.wRmax) : 0f;
			State.UAxis = this.Caps.wNumAxes > 4 ? GetNormalisedAxisPoint(Info.U, this.Caps.wUmin, this.Caps.wUmax) : 0f;
			State.VAxis = this.Caps.wNumAxes > 5 ? GetNormalisedAxisPoint(Info.V, this.Caps.wVmin, this.Caps.wVmax) : 0f;

			return State;
		}

		private static float GetNormalisedAxisPoint(int value, uint min, uint max) {
			return Math.Max(-1f, Math.Min(+1f, (float)(value - min) / (float)(max - min) * 2f - 1f));
		}

	}
}
