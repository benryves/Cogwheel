using System;
using System.Collections.Generic;

namespace BeeDevelopment.Sega8Bit.Hardware.Controllers {

	public class ATDevice {

		private bool myData;
		private bool theirData;
		public bool Data {
			get { return this.myData && this.theirData; }
			set { this.theirData = value; }
		}

		private bool myClock;
		private bool theirClock;
		public bool Clock {
			get { return this.myClock && this.theirClock; }
			set { this.theirClock = value; }
		}

		public virtual void Reset() {
			this.myClock = this.theirClock = this.myData = this.theirData = true;
			this.OutputQueue = new Queue<bool[]>();
			this.CurrentOutputStream = null;
			this.ReceiveBuffer = new List<bool>();
		}

		public ATDevice() {
			this.Reset();
		}

		private Queue<bool[]> OutputQueue;
		private bool[] CurrentOutputStream;
		private int CurrentOutputStreamIndex;
		private bool Receiving;
		private List<bool> ReceiveBuffer;

		protected void EnqueueByte(byte value) {
			var BitStream = new List<bool>(11);
			BitStream.Add(false);
			var Parity = true;
			for (int i = 0; i < 8; ++i) {
				var ValueToAdd = ((value >> i) & 1) != 0;
				BitStream.Add(ValueToAdd);
				Parity ^= ValueToAdd;
			}
			BitStream.Add(Parity);
			BitStream.Add(true);
			this.OutputQueue.Enqueue(BitStream.ToArray());
		}

		protected void EnqueueBytes(IEnumerable<byte> values) {
			var BitStream = new List<bool>(11);
			foreach (var value in values) {
				BitStream.Add(false);
				var Parity = false;
				for (int i = 0; i < 8; ++i) {
					var ValueToAdd = ((value >> i) & 1) != 0;
					BitStream.Add(ValueToAdd);
					Parity ^= ValueToAdd;
				}
				BitStream.Add(Parity);
				BitStream.Add(true);
			}
			this.OutputQueue.Enqueue(BitStream.ToArray());
		}

		public void ClearBufferedData() {
			this.OutputQueue.Clear();
		}

		public int BufferedDataCount {
			get { return this.OutputQueue.Count; }
		}

		public bool HasBufferedData {
			get {
				return this.BufferedDataCount > 0;
			}
		}

		/// <summary>
		/// Runs the AT emulator for 40 microseconds.
		/// </summary>
		public virtual void Tick() {
			if (!this.theirClock) {
				if ((this.CurrentOutputStreamIndex % 11) != 0) {
					this.CurrentOutputStreamIndex = 0;
				}
				this.myClock = true;
				this.myData = true;
				if (!this.theirData && !this.Receiving) {
					this.Receiving = true;
					this.ReceiveBuffer.Clear();
				}
				return;
			}

			if (this.Receiving) {
				this.myClock ^= true;
				if (this.myClock) {
					this.ReceiveBuffer.Add(this.theirData);
				}
				if (this.ReceiveBuffer.Count == 10) {
					this.myData = false; // ACK
				} else if (this.ReceiveBuffer.Count == 11) {
					this.myData = true;
					this.Receiving = false;
					byte Received = 0;
					for (int i = 0; i < 8; ++i) {
						if (this.ReceiveBuffer[i]) Received |= (byte)(1 << i);
					}
					this.OnByteReceived(Received);
				}
				return;
			}

			if (this.CurrentOutputStream == null && this.OutputQueue.Count > 0) {
				this.CurrentOutputStream = this.OutputQueue.Dequeue();
				this.CurrentOutputStreamIndex = 0;
			}

			if (this.CurrentOutputStream != null) {
				if (this.myClock) {
					this.myData = this.CurrentOutputStream[this.CurrentOutputStreamIndex++];
				} else {
					if (this.CurrentOutputStreamIndex == this.CurrentOutputStream.Length) {
						this.CurrentOutputStream = null;
					}
				}
				this.myClock ^= true;
			}
		}

		protected virtual void OnByteReceived(byte value) {

		}

	}
	public class Keyboard : ATDevice {

		#region Types

		class KeyTimeRecord {
			public uint TimePressed { get; set; }
			public bool Repeating { get; set; }
		}

		enum PendingData {
			None,
			Led,
		}

		#endregion

		private Dictionary<uint, KeyTimeRecord> PressedScanCodes;
		private uint Ticks;
		private PendingData Pending;

		public Keyboard()
			: base() {
			this.Reset();
		}

		#region Properties

		/// <summary>
		/// Gets or sets the delay before the key repeats.
		/// </summary>
		public TimeSpan TypematicDelay { get; set; }

		/// <summary>
		/// Gets or sets the number of characters typed per second when a key is held.
		/// </summary>
		public double TypematicRate { get; set; }

		/// <summary>
		/// Gets the state of the keyboard's Num Lock LED.
		/// </summary>
		public bool NumLock { get; private set; }

		/// <summary>
		/// Gets the state of the keyboard's Caps Lock LED.
		/// </summary>
		public bool CapsLock { get; private set; }

		/// <summary>
		/// Gets the state of the keyboard's Scroll Lock LED.
		/// </summary>
		public bool ScrollLock { get; private set; }

		#endregion

		#region Overidden Methods

		/// <summary>
		/// Resets the keyboard.
		/// </summary>
		public override void Reset() {
			base.Reset();
			this.PressedScanCodes = new Dictionary<uint, KeyTimeRecord>();
			this.TypematicDelay = TimeSpan.FromMilliseconds(500.0d);
			this.TypematicRate = 10.9d;
			this.Pending = PendingData.None;
			this.NumLock = false;
			this.CapsLock = false;
			this.ScrollLock = false;
			this.EnqueueByte(0xAA);
		}

		/// <summary>
		/// Emulates the keyboard for 40uS.
		/// </summary>
		public override void Tick() {
			++this.Ticks;
			foreach (var Repeater in this.PressedScanCodes) {
				if (Repeater.Value.Repeating) {
					if ((Repeater.Value.TimePressed + (25000.0d / this.TypematicRate)) <= this.Ticks) {
						this.EnqueueScancode(Repeater.Key, true);
						Repeater.Value.TimePressed = this.Ticks;
					}
				} else {
					if ((uint)(Repeater.Value.TimePressed + this.TypematicDelay.TotalMilliseconds * 25.0d) <= this.Ticks) {
						this.EnqueueScancode(Repeater.Key, true);
						Repeater.Value.Repeating = true;
						Repeater.Value.TimePressed = this.Ticks;
					}
				}
			}
			base.Tick();
		}

		protected override void OnByteReceived(byte value) {
			switch (value) {
				case 0xFF:
					this.EnqueueByte(0xFA);
					this.Reset();
					break;
				default:
					base.OnByteReceived(value);
					break;
			}
		}

		#endregion

		#region Private Methods

		private static uint ToScanCode(SC3000Keyboard.Keys key) {
			switch (key) {
				case SC3000Keyboard.Keys.D1:
					return 0x16;
				case SC3000Keyboard.Keys.D2:
					return 0x1E;
				case SC3000Keyboard.Keys.D3:
					return 0x26;
				case SC3000Keyboard.Keys.D4:
					return 0x25;
				case SC3000Keyboard.Keys.D5:
					return 0x2E;
				case SC3000Keyboard.Keys.D6:
					return 0x36;
				case SC3000Keyboard.Keys.D7:
					return 0x3D;
				case SC3000Keyboard.Keys.D8:
					return 0x3E;
				case SC3000Keyboard.Keys.D9:
					return 0x46;
				case SC3000Keyboard.Keys.D0:
					return 0x45;
				case SC3000Keyboard.Keys.Q:
					return 0x15;
				case SC3000Keyboard.Keys.W:
					return 0x1D;
				case SC3000Keyboard.Keys.E:
					return 0x24;
				case SC3000Keyboard.Keys.R:
					return 0x2D;
				case SC3000Keyboard.Keys.T:
					return 0x2C;
				case SC3000Keyboard.Keys.Y:
					return 0x35;
				case SC3000Keyboard.Keys.U:
					return 0x3C;
				case SC3000Keyboard.Keys.I:
					return 0x43;
				case SC3000Keyboard.Keys.O:
					return 0x44;
				case SC3000Keyboard.Keys.P:
					return 0x4D;
				case SC3000Keyboard.Keys.A:
					return 0x1C;
				case SC3000Keyboard.Keys.S:
					return 0x1B;
				case SC3000Keyboard.Keys.D:
					return 0x23;
				case SC3000Keyboard.Keys.F:
					return 0x2B;
				case SC3000Keyboard.Keys.G:
					return 0x34;
				case SC3000Keyboard.Keys.H:
					return 0x33;
				case SC3000Keyboard.Keys.J:
					return 0x3B;
				case SC3000Keyboard.Keys.K:
					return 0x42;
				case SC3000Keyboard.Keys.L:
					return 0x4B;
				case SC3000Keyboard.Keys.Z:
					return 0x1A;
				case SC3000Keyboard.Keys.X:
					return 0x22;
				case SC3000Keyboard.Keys.C:
					return 0x21;
				case SC3000Keyboard.Keys.V:
					return 0x2A;
				case SC3000Keyboard.Keys.B:
					return 0x32;
				case SC3000Keyboard.Keys.N:
					return 0x31;
				case SC3000Keyboard.Keys.M:
					return 0x3A;
				case SC3000Keyboard.Keys.Shift:
					return 0x12;
				case SC3000Keyboard.Keys.Break:
					return 0x76;
				case SC3000Keyboard.Keys.Minus:
					return 0x4E;
				case SC3000Keyboard.Keys.Caret:
					return 0x55;
				case SC3000Keyboard.Keys.CarriageReturn:
					return 0x5A;
				case SC3000Keyboard.Keys.Comma:
					return 0x41;
				case SC3000Keyboard.Keys.Period:
					return 0x49;
				case SC3000Keyboard.Keys.Slash:
					return 0x4A;
				case SC3000Keyboard.Keys.Semicolon:
					return 0x4C;
				case SC3000Keyboard.Keys.Colon:
					return 0x52;
				case SC3000Keyboard.Keys.CloseBrackets:
					return 0x5D;
				case SC3000Keyboard.Keys.AtSign:
					return 0x54;
				case SC3000Keyboard.Keys.OpenBrackets:
					return 0x5B;
				case SC3000Keyboard.Keys.InsDel:
					return 0x66;
				case SC3000Keyboard.Keys.Up:
					return 0x75;
				case SC3000Keyboard.Keys.Down:
					return 0x72;
				case SC3000Keyboard.Keys.Left:
					return 0x6B;
				case SC3000Keyboard.Keys.Right:
					return 0x74;
				case SC3000Keyboard.Keys.Space:
					return 0x29;
				case SC3000Keyboard.Keys.Control:
					return 0x14;
				default:
					return 0;
					//throw new InvalidOperationException();
			}
		}

		public void EnqueueScancode(uint scancode, bool pressed) {
			var Sequence = new List<byte>();
			if (!pressed) Sequence.Add(0xF0);
			Sequence.Add((byte)scancode);
			this.EnqueueBytes(Sequence);
		}

		#endregion

		#region Public Methods

		public void PressKey(uint scancode) {
			if (scancode == 0 || PressedScanCodes.ContainsKey(scancode)) {
				return;
			} else {
				PressedScanCodes.Add(scancode, new KeyTimeRecord() { TimePressed = this.Ticks });
			}
			this.EnqueueScancode(scancode, true);
		}

		/// <summary>
		/// Simulates pressing a key on the emulated keyboard.
		/// </summary>
		/// <param name="key">The <see cref="Keys"/> that has been pressed.</param>
		public void PressKey(SC3000Keyboard.Keys key) {
			PressKey(ToScanCode(key));
		}

		public void ReleaseKey(uint scancode) {
			if (scancode == 0 || !PressedScanCodes.ContainsKey(scancode)) {
				return;
			} else {
				PressedScanCodes.Remove(scancode);
			}
			this.EnqueueScancode(scancode, false);
		}

		/// <summary>
		/// Simulates releasing a key on the emulated keyboard.
		/// </summary>
		/// <param name="key">The <see cref="Keys"/> that has been released.</param>
		public void ReleaseKey(SC3000Keyboard.Keys key) {
			ReleaseKey(ToScanCode(key));
		}

		public void ReleaseAllKeys() {
			foreach (var item in this.PressedScanCodes) {
				this.EnqueueScancode(item.Key, false);
			}
			this.PressedScanCodes.Clear();
		}

		public void Type(char c) {
			this.ReleaseAllKeys();
			KeyValuePair<uint, bool> scancodeAndShifted;
			if (TryConvertCharacterToScancode(c, out scancodeAndShifted)) {
				this.SendCharacterScancode(scancodeAndShifted.Key, scancodeAndShifted.Value);
			}
		}

		public void Type(string s) {
			foreach (var c in s) {
				this.Type(c);
			}
		}

		bool TryConvertCharacterToScancode(char c, out KeyValuePair<uint, bool> scancodeAndShifted) {

			scancodeAndShifted = new KeyValuePair<uint, bool>();

			if (c == '\r') {
				scancodeAndShifted = new KeyValuePair<uint, bool>(0x5A, false);
				return true;
			}

			if (c < ' ' || c > 127) {
				return false;
			}

			scancodeAndShifted = (new[] {
				new KeyValuePair<uint, bool>(0x29, false), // ' '
				new KeyValuePair<uint, bool>(0x16, true),  // '!'
				new KeyValuePair<uint, bool>(0x1E, true),  // '"'
				new KeyValuePair<uint, bool>(0x5D, false), // '#'
				new KeyValuePair<uint, bool>(0x25, true),  // '$'
				new KeyValuePair<uint, bool>(0x2E, true),  // '%'
				new KeyValuePair<uint, bool>(0x3D, true),  // '&'
				new KeyValuePair<uint, bool>(0x52, false), // '''
				new KeyValuePair<uint, bool>(0x46, true),  // '('
				new KeyValuePair<uint, bool>(0x45, true),  // ')'
				new KeyValuePair<uint, bool>(0x3E, true),  // '*'
				new KeyValuePair<uint, bool>(0x55, true),  // '+'
				new KeyValuePair<uint, bool>(0x41, false), // ','
				new KeyValuePair<uint, bool>(0x4E, false), // '-'
				new KeyValuePair<uint, bool>(0x49, false), // '.'
				new KeyValuePair<uint, bool>(0x4A, false), // '/'
				new KeyValuePair<uint, bool>(0x45, false), // '0'
				new KeyValuePair<uint, bool>(0x16, false), // '1'
				new KeyValuePair<uint, bool>(0x1E, false), // '2'
				new KeyValuePair<uint, bool>(0x26, false), // '3'
				new KeyValuePair<uint, bool>(0x25, false), // '4'
				new KeyValuePair<uint, bool>(0x2E, false), // '5'
				new KeyValuePair<uint, bool>(0x36, false), // '6'
				new KeyValuePair<uint, bool>(0x3D, false), // '7'
				new KeyValuePair<uint, bool>(0x3E, false), // '8'
				new KeyValuePair<uint, bool>(0x46, false), // '9'
				new KeyValuePair<uint, bool>(0x4C, true),  // ':'
				new KeyValuePair<uint, bool>(0x4C, false), // ';'
				new KeyValuePair<uint, bool>(0x41, true),  // '<'
				new KeyValuePair<uint, bool>(0x55, false), // '='
				new KeyValuePair<uint, bool>(0x49, true),  // '>'
				new KeyValuePair<uint, bool>(0x4A, true),  // '?'
				new KeyValuePair<uint, bool>(0x52, true),  // '@'
				new KeyValuePair<uint, bool>(0x1C, true),  // 'A'
				new KeyValuePair<uint, bool>(0x32, true),  // 'B'
				new KeyValuePair<uint, bool>(0x21, true),  // 'C'
				new KeyValuePair<uint, bool>(0x23, true),  // 'D'
				new KeyValuePair<uint, bool>(0x24, true),  // 'E'
				new KeyValuePair<uint, bool>(0x2B, true),  // 'F'
				new KeyValuePair<uint, bool>(0x34, true),  // 'G'
				new KeyValuePair<uint, bool>(0x33, true),  // 'H'
				new KeyValuePair<uint, bool>(0x43, true),  // 'I'
				new KeyValuePair<uint, bool>(0x3B, true),  // 'J'
				new KeyValuePair<uint, bool>(0x42, true),  // 'K'
				new KeyValuePair<uint, bool>(0x4B, true),  // 'L'
				new KeyValuePair<uint, bool>(0x3A, true),  // 'M'
				new KeyValuePair<uint, bool>(0x31, true),  // 'N'
				new KeyValuePair<uint, bool>(0x44, true),  // 'O'
				new KeyValuePair<uint, bool>(0x4D, true),  // 'P'
				new KeyValuePair<uint, bool>(0x15, true),  // 'Q'
				new KeyValuePair<uint, bool>(0x2D, true),  // 'R'
				new KeyValuePair<uint, bool>(0x1B, true),  // 'S'
				new KeyValuePair<uint, bool>(0x2C, true),  // 'T'
				new KeyValuePair<uint, bool>(0x3C, true),  // 'U'
				new KeyValuePair<uint, bool>(0x2A, true),  // 'V'
				new KeyValuePair<uint, bool>(0x1D, true),  // 'W'
				new KeyValuePair<uint, bool>(0x22, true),  // 'X'
				new KeyValuePair<uint, bool>(0x35, true),  // 'Y'
				new KeyValuePair<uint, bool>(0x1A, true),  // 'Z'
				new KeyValuePair<uint, bool>(0x54, false), // '['
				new KeyValuePair<uint, bool>(0x61, false), // '\'
				new KeyValuePair<uint, bool>(0x5B, false), // ']'
				new KeyValuePair<uint, bool>(0x36, true),  // '^'
				new KeyValuePair<uint, bool>(0x4E, true),  // '_'
				new KeyValuePair<uint, bool>(0x0E, false), // '`'
				new KeyValuePair<uint, bool>(0x1C, false), // 'a'
				new KeyValuePair<uint, bool>(0x32, false), // 'b'
				new KeyValuePair<uint, bool>(0x21, false), // 'c'
				new KeyValuePair<uint, bool>(0x23, false), // 'd'
				new KeyValuePair<uint, bool>(0x24, false), // 'e'
				new KeyValuePair<uint, bool>(0x2B, false), // 'f'
				new KeyValuePair<uint, bool>(0x34, false), // 'g'
				new KeyValuePair<uint, bool>(0x33, false), // 'h'
				new KeyValuePair<uint, bool>(0x43, false), // 'i'
				new KeyValuePair<uint, bool>(0x3B, false), // 'j'
				new KeyValuePair<uint, bool>(0x42, false), // 'k'
				new KeyValuePair<uint, bool>(0x4B, false), // 'l'
				new KeyValuePair<uint, bool>(0x3A, false), // 'm'
				new KeyValuePair<uint, bool>(0x31, false), // 'n'
				new KeyValuePair<uint, bool>(0x44, false), // 'o'
				new KeyValuePair<uint, bool>(0x4D, false), // 'p'
				new KeyValuePair<uint, bool>(0x15, false), // 'q'
				new KeyValuePair<uint, bool>(0x2D, false), // 'r'
				new KeyValuePair<uint, bool>(0x1B, false), // 's'
				new KeyValuePair<uint, bool>(0x2C, false), // 't'
				new KeyValuePair<uint, bool>(0x3C, false), // 'u'
				new KeyValuePair<uint, bool>(0x2A, false), // 'v'
				new KeyValuePair<uint, bool>(0x1D, false), // 'w'
				new KeyValuePair<uint, bool>(0x22, false), // 'x'
				new KeyValuePair<uint, bool>(0x35, false), // 'y'
				new KeyValuePair<uint, bool>(0x1A, false), // 'z'
				new KeyValuePair<uint, bool>(0x54, true),  // '{'
				new KeyValuePair<uint, bool>(0x61, true),  // '|'
				new KeyValuePair<uint, bool>(0x5B, true),  // '}'
				new KeyValuePair<uint, bool>(0x5D, true),  // '~'
				new KeyValuePair<uint, bool>(0x00, false), // ' '
			})[c - ' '];
			return scancodeAndShifted.Key != 0;
		}

		void SendCharacterScancode(uint scancode, bool shifted) {
			if (shifted) this.EnqueueScancode(0x12, true);
			this.EnqueueScancode(scancode, true);
			this.EnqueueScancode(scancode, false);
			if (shifted) this.EnqueueScancode(0x12, false);
		}

		#endregion



	}

	/// <summary>
	/// Emulates a PS/2 keyboard.
	/// </summary>
	public class PS2Keyboard {

		private Keyboard keyboard;

		public void SetKeyState(SC3000Keyboard.Keys key, bool pressed) {
			if (pressed) {
				this.keyboard.PressKey(key);
			} else {
				this.keyboard.ReleaseKey(key);
			}
		}

		public void SetKeyState(uint scancode, bool pressed) {
			if (pressed) {
				this.keyboard.PressKey(scancode);
			} else {
				this.keyboard.ReleaseKey(scancode);
			}
		}

		/// <summary>
		/// Gets the <see cref="Emulator"/> that the keyboard is connected to.
		/// </summary>
		public Emulator Emulator { get; private set; }

		/// <summary>
		/// Creates an instance of the <see cref="PS2Keyboard"/> class.
		/// </summary>
		/// <param name="emulator">The <see cref="Emulator"/> instance that the keyboard is connected to.</param>
		public PS2Keyboard(Emulator emulator) {
			this.Emulator = emulator;
			this.keyboard = new Keyboard();
		}

		public void Tick() {
			this.keyboard.Tick();
		}

		/// <summary>
		/// Updates the state of the emulator's controller ports to reflect the current keyboard state.
		/// </summary>
		public void UpdateState() {
			if (this.Emulator.SegaPorts[0].TH.Direction == PinDirection.Output) {
				this.keyboard.Data = this.Emulator.SegaPorts[0].TH.OutputState;
			} else {
				this.keyboard.Data = true;
				this.Emulator.SegaPorts[0].TH.InputState = this.keyboard.Data;
			}
			if (this.Emulator.SegaPorts[0].TR.Direction == PinDirection.Output) {
				this.keyboard.Clock = this.Emulator.SegaPorts[0].TR.OutputState;
			} else {
				this.keyboard.Clock = true;
				this.Emulator.SegaPorts[0].TR.InputState = this.keyboard.Clock;
			}
		}

		public void Type(char c) {
			this.keyboard.Type(c);
		}
		public void Type(string s) {
			this.keyboard.Type(s);
		}

		public void PressKey(SC3000Keyboard.Keys key) {
			this.keyboard.PressKey(key);
		}

		public void ReleaseKey(SC3000Keyboard.Keys key) {
			this.keyboard.ReleaseKey(key);
		}

		public void PressKey(uint scancode) {
			this.keyboard.PressKey(scancode);
		}

		public void ReleaseKey(uint scancode) {
			this.keyboard.ReleaseKey(scancode);
		}

		public void ClearBufferedData() {
			this.keyboard.ClearBufferedData();
		}

		public int BufferedDataCount {
			get { return this.keyboard.BufferedDataCount; }
		}

		public bool HasBufferedData {
			get { return this.keyboard.HasBufferedData; }
		}

	}
}
