
using System;
namespace CogwheelSlimDX.JoystickInput {

	[Flags]
	public enum Buttons {
		None = 0,
		Button1 = 1 << 0,
		Button2 = 1 << 1,
		Button3 = 1 << 2,
		Button4 = 1 << 3,
		Button5 = 1 << 4,
		Button6 = 1 << 5,
		Button7 = 1 << 6,
		Button8 = 1 << 7,
		Button9 = 1 << 8,
		Button10 = 1 << 9,
		Button11 = 1 << 10,
		Button12 = 1 << 11,
		Button13 = 1 << 12,
		Button14 = 1 << 13,
		Button15 = 1 << 14,
		Button16 = 1 << 15,
		Button17 = 1 << 16,
		Button18 = 1 << 17,
		Button19 = 1 << 18,
		Button20 = 1 << 19,
		Button21 = 1 << 20,
		Button22 = 1 << 21,
		Button23 = 1 << 22,
		Button24 = 1 << 23,
		Button25 = 1 << 24,
		Button26 = 1 << 25,
		Button27 = 1 << 26,
		Button28 = 1 << 27,
		Button29 = 1 << 28,
		Button30 = 1 << 29,
		Button31 = 1 << 30,
		Button32 = 1 << 31,
	}

	/// <summary>
	/// Represents the state of the joystick at a moment in time.
	/// </summary>
	public class JoystickState {

		/// <summary>
		/// Gets or sets the <see cref="Buttons"/> state.
		/// </summary>
		public Buttons Buttons { get; set; }

		/// <summary>Gets or sets the normalised value of the X axis.</summary>
		public float XAxis { get; set; }

		/// <summary>Gets or sets the normalised value of the Y axis.</summary>
		public float YAxis { get; set; }

		/// <summary>Gets or sets the normalised value of the Z axis.</summary>
		public float ZAxis { get; set; }

		/// <summary>Gets or sets the normalised value of the rudder axis.</summary>
		public float Rudder { get; set; }

		/// <summary>Gets or sets the normalised value of the U (rudder) axis.</summary>
		public float UAxis { get; set; }

		/// <summary>Gets or sets the normalised value of the V (rudder) axis.</summary>
		public float VAxis { get; set; }

		public override string ToString() {
			return string.Format("({0},{1})+{2}", this.XAxis, this.YAxis, this.Buttons);
		}

	}
}
