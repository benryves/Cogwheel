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
			/// <summary>The 1 key.</summary>
			D1 = 8 * 0,
			/// <summary>The 2 key.</summary>
			D2,
			/// <summary>The 3 key.</summary>
			D3,
			/// <summary>The 4 key.</summary>
			D4,
			/// <summary>The 5 key.</summary>
			D5,
			/// <summary>The 6 key.</summary>
			D6,
			/// <summary>The 7 key.</summary>
			D7,
			/// <summary>The Q key.</summary>
			Q = 8 * 1,
			/// <summary>The W key.</summary>
			W,
			/// <summary>The E key.</summary>
			E,
			/// <summary>The R key.</summary>
			R,
			/// <summary>The T key.</summary>
			T,
			/// <summary>The Y key.</summary>
			Y,
			/// <summary>The U key.</summary>
			U,
			/// <summary>The A key.</summary>
			A = 8 * 2,
			/// <summary>The S key.</summary>
			S,
			/// <summary>The D key.</summary>
			D,
			/// <summary>The F key.</summary>
			F,
			/// <summary>The G key.</summary>
			G,
			/// <summary>The H key.</summary>
			H,
			/// <summary>The J key.</summary>
			J,
			/// <summary>The Z key.</summary>
			Z = 8 * 3,
			/// <summary>The X key.</summary>
			X,
			/// <summary>The C key.</summary>
			C,
			/// <summary>The V key.</summary>
			V,
			/// <summary>The B key.</summary>
			B,
			/// <summary>The N key.</summary>
			N,
			/// <summary>The M key.</summary>
			M,
			/// <summary>The Eng Dier's key.</summary>
			EngDiers = 8 * 4,
			/// <summary>The space bar key.</summary>
			Space,
			/// <summary>The Home/Clr key.</summary>
			HomeClr,
			/// <summary>The Ins/Del key.</summary>
			InsDel,
			/// <summary>The comma (,) key.</summary>
			Comma = 8 * 5,
			/// <summary>The period (.) key.</summary>
			Period,
			/// <summary>The slash (/) key.</summary>
			Slash,
			/// <summary>The Pi (π) key.</summary>
			Pi,
			/// <summary>The down arrow key.</summary>
			Down,
			/// <summary>The left arrow key.</summary>
			Left,
			/// <summary>The right arrow key.</summary>
			Right,
			/// <summary>The K key.</summary>
			K = 8 * 6,
			/// <summary>The L key.</summary>
			L,
			/// <summary>The semicolon (;) key.</summary>
			Semicolon,
			/// <summary>The colon (:) key.</summary>
			Colon,
			/// <summary>The right bracket key.</summary>
			RightBracket,
			/// <summary>The carriage return key.</summary>
			CarriageReturn,
			/// <summary>The up arrow key.</summary>
			Up,
			/// <summary>The I key.</summary>
			I = 8 * 7,
			/// <summary>The O key.</summary>
			O,
			/// <summary>The P key.</summary>
			P,
			/// <summary>The at sign (@) key.</summary>
			AtSign,
			/// <summary>The left bracket key.</summary>
			LeftBracket,
			/// <summary>The 8 key.</summary>
			D8 = 8 * 8,
			/// <summary>The 9 key.</summary>
			D9,
			/// <summary>The 0 key.</summary>
			D0,
			/// <summary>The minus (-) key.</summary>
			Minus,
			/// <summary>The caret (^) key.</summary>
			Caret,
			/// <summary>The Yen (¥) key.</summary>
			Yen,
			/// <summary>The Break key.</summary>
			Break,
			/// <summary>The Graph key.</summary>
			Graph = 8 * 9 + 6,
			/// <summary>The Control key.</summary>
			Control = 8 * 10 + 6,
			/// <summary>The Func key.</summary>
			Func = 8 * 11 + 5,
			/// <summary>The Shift key.</summary>
			Shift,
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
