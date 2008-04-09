using System;

namespace BeeDevelopment.Sega8Bit.Hardware.Controllers {
	/// <summary>
	/// Emulates a SC-3000 keyboard.
	/// </summary>
	public class SC3000Keyboard {

		/// <summary>
		/// Defines a key on the <see cref="SC3000Keyboard"/>.
		/// </summary>
		public enum Keys {
			D1 = 8 * 0, D2, D3, D4, D5, D6, D7,
			Q = 8 * 1, W, E, R, T, Y, U,
			A = 8 * 2, S, D, F, G, H, J,
			Z = 8 * 3, X, C, V, B, N, M,
			EngDiers = 8 * 4, Space, HomeClr, InsDel,
			Comma = 8 * 5, FullStop, Slash, Pi, Down, Left, Right,
			K = 8 * 6, L, SemiColon, Colon, RightBracket, Return, Up,
			I = 8 * 7, O, P, AtSign, LeftBracket,
			D8 = 8 * 8, D9, D0, Minus, Caret, Yen, Break,
			Graph = 8 * 9 + 6,
			Ctrl = 8 * 10 + 6,
			Func = 8 * 11 + 5, Shift
		}

		private bool[,] KeyStatus = new bool[12, 8];

		/// <summary>
		/// Sets whether a particular key is pressed or released.
		/// </summary>
		/// <param name="key">The key to mark as pressed or released.</param>
		/// <param name="pressed">True to set the key as pressed; false to set it as released.</param>
		public void SetKeyState(Keys key, bool pressed) {
			if ((int)key < 0 || (int)key >= 12 * 8) throw new Exception("Invalid key number.");
			this.KeyStatus[(int)key / 8, (int)key & 7] = pressed;
			this.UpdateState();
		}

		/// <summary>
		/// Gets the <see cref="Emulator"/> that the keyboard is connected to.
		/// </summary>
		public Emulator Emulator { get; private set; }

		/// <summary>
		/// Creates an instance of the <see cref="SC300Keyboard"/> class.
		/// </summary>
		/// <param name="emulator">The <see cref="Emulator"/> instance that the keyboard is connected to.</param>
		public SC3000Keyboard(Emulator emulator) {
			this.Emulator = emulator;
		}

		/// <summary>
		/// Updates the state of the emulator's main PPI to reflect the current keyboard state.
		/// </summary>
		public void UpdateState() {
			int State = 0;
			int Row = this.Emulator.MainPPI.PortCOutput & 7;
			if (Row == 7) {
				State = 0;
				if (this.Emulator.SegaPorts[0].Up.State) State |= (1 << 0);
				if (this.Emulator.SegaPorts[0].Down.State) State |= (1 << 1);
				if (this.Emulator.SegaPorts[0].Left.State) State |= (1 << 2);
				if (this.Emulator.SegaPorts[0].Right.State) State |= (1 << 3);
				if (this.Emulator.SegaPorts[0].TL.State) State |= (1 << 4);
				if (this.Emulator.SegaPorts[0].TR.InputState) State |= (1 << 5);
				if (this.Emulator.SegaPorts[1].Up.State) State |= (1 << 6);
				if (this.Emulator.SegaPorts[1].Down.State) State |= (1 << 7);
				if (this.Emulator.SegaPorts[1].Left.State) State |= (1 << 8);
				if (this.Emulator.SegaPorts[1].Right.State) State |= (1 << 9);
				if (this.Emulator.SegaPorts[1].TL.State) State |= (1 << 10);
				if (this.Emulator.SegaPorts[1].TR.State) State |= (1 << 11);
			} else {
				for (int i = 0; i < 12; ++i) {
					State >>= 1;
					if (!this.KeyStatus[i, Row]) State |= 0x0800;
				}
			}
			this.Emulator.MainPPI.PortAInput = (byte)(State & 0xFF);
			this.Emulator.MainPPI.PortBInput = (byte)((this.Emulator.MainPPI.PortBInput & 0xF0) | ((State >> 8) & 0x0F));
		}

	}
}
