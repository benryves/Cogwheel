using System;
using System.Collections.Generic;
using System.Text;

namespace BeeDevelopment.Sega8Bit {
	public class ColecoVisionControllerPort {

		/// <summary>Gets or sets whether the joystick is pointed up.</summary>
		public bool Up { get; set; }
		/// <summary>Gets or sets whether the joystick is pointed down.</summary>
		public bool Down { get; set; }
		/// <summary>Gets or sets whether the joystick is pointed left.</summary>
		public bool Left { get; set; }
		/// <summary>Gets or sets whether the joystick is pointed right.</summary>
		public bool Right { get; set; }

		/// <summary>Gets or sets whether the joystick's first fire button is pressed.</summary>
		public bool Fire1 { get; set; }
		/// <summary>Gets or sets whether the joystick's second fire button ("arm") is pressed.</summary>
		public bool Fire2 { get; set; }
		/// <summary>Gets or sets whether the joystick's third fire button is pressed.</summary>
		public bool Fire3 { get; set; }
		/// <summary>Gets or sets whether the joystick's fourth button is pressed.</summary>
		public bool Fire4 { get; set; }

		/// <summary>Gets or sets whether the number-pad 0 is pressed.</summary>
		public bool Number0 { get; set; }
		/// <summary>Gets or sets whether the number-pad 1 is pressed.</summary>
		public bool Number1 { get; set; }
		/// <summary>Gets or sets whether the number-pad 2 is pressed.</summary>
		public bool Number2 { get; set; }
		/// <summary>Gets or sets whether the number-pad 3 is pressed.</summary>
		public bool Number3 { get; set; }
		/// <summary>Gets or sets whether the number-pad 4 is pressed.</summary>
		public bool Number4 { get; set; }
		/// <summary>Gets or sets whether the number-pad 5 is pressed.</summary>
		public bool Number5 { get; set; }
		/// <summary>Gets or sets whether the number-pad 6 is pressed.</summary>
		public bool Number6 { get; set; }
		/// <summary>Gets or sets whether the number-pad 7 is pressed.</summary>
		public bool Number7 { get; set; }
		/// <summary>Gets or sets whether the number-pad 8 is pressed.</summary>
		public bool Number8 { get; set; }
		/// <summary>Gets or sets whether the number-pad 9 is pressed.</summary>
		public bool Number9 { get; set; }

		/// <summary>Gets or sets whether the star button is pressed.</summary>
		public bool Star { get; set; }
		/// <summary>Gets or sets whether the hash button is pressed.</summary>
		public bool Hash { get; set; }

		/// <summary>
		/// Reads the state of the controller's joystick.
		/// </summary>
		public byte ReadJoystick() {
			return (byte)(
				(this.Up ? 0x7E : 0x7F) &
				(this.Down ? 0x7B : 0x7F) &
				(this.Left ? 0x77 : 0x7F) &
				(this.Right ? 0x7D : 0x7F) &
				(this.Fire1 ? 0x3F : 0x7F)
			);
		}

		/// <summary>
		/// Reads the state of the controller's number pad.
		/// </summary>
		public byte ReadNumberPad() {

			byte Key = 0x0;

			//                      0x0;
			if (this.Number8) Key = 0x1;
			if (this.Number4) Key = 0x2;
			if (this.Number5) Key = 0x3;
			//                      0x4;
			if (this.Number7) Key = 0x5;
			if (this.Hash)    Key = 0x6;
			if (this.Number2) Key = 0x7;
			//                      0x8;
			if (this.Star)    Key = 0x9;
			if (this.Number0) Key = 0xA;
			if (this.Number9) Key = 0xB;
			if (this.Number3) Key = 0xC;
			if (this.Number1) Key = 0xD;
			if (this.Number6) Key = 0xE;
			
			return (byte)(Key | (this.Fire2 ? 0x30 : 0x70));
		}

	}
}
