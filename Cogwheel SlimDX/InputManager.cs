using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using BeeDevelopment.Sega8Bit;

namespace CogwheelSlimDX {

	interface IInputSource {

		bool Player1TL { get; }
		bool Player1TR { get; }
		bool Player1Up { get; }
		bool Player1Down { get; }
		bool Player1Left { get; }
		bool Player1Right { get; }

		bool Player2TL { get; }
		bool Player2TR { get; }
		bool Player2Up { get; }
		bool Player2Down { get; }
		bool Player2Left { get; }
		bool Player2Right { get; }

		bool Pause { get; }
		bool Reset { get; }
		bool Start { get; }

	}

	class InputManager : IInputSource {

		public List<IInputSource> Sources { get; private set; }

		public InputManager() {
			this.Sources = new List<IInputSource>();
		}


		public void UpdateEmulatorState(Emulator emulator) {
			emulator.Ports[0].TL.State = !this.Player1TL;
			emulator.Ports[0].TR.InputState = !this.Player1TR;
			emulator.Ports[0].Up.State = !this.Player1Up;
			emulator.Ports[0].Down.State = !this.Player1Down;
			emulator.Ports[0].Left.State = !this.Player1Left;
			emulator.Ports[0].Right.State = !this.Player1Right;
			emulator.Ports[1].TL.State = !this.Player2TL;
			emulator.Ports[1].TR.InputState = !this.Player2TR;
			emulator.Ports[1].Up.State = !this.Player2Up;
			emulator.Ports[1].Down.State = !this.Player2Down;
			emulator.Ports[1].Left.State = !this.Player2Left;
			emulator.Ports[1].Right.State = !this.Player2Right;
			if (emulator.HasPauseButton) emulator.PauseButton = this.Pause;
			if (emulator.HasResetButton) emulator.ResetButton = this.Reset;
			if (emulator.HasStartButton) emulator.StartButton = this.Start;
			
		}

		#region IInputSource Members

		public bool Player1TL { get { foreach (var Source in this.Sources) { if (Source.Player1TL) return true; } return false; } }
		public bool Player1TR { get { foreach (var Source in this.Sources) { if (Source.Player1TR) return true; } return false; } }
		public bool Player1Up { get { foreach (var Source in this.Sources) { if (Source.Player1Up) return true; } return false; } }
		public bool Player1Down { get { foreach (var Source in this.Sources) { if (Source.Player1Down) return true; } return false; } }
		public bool Player1Left { get { foreach (var Source in this.Sources) { if (Source.Player1Left) return true; } return false; } }
		public bool Player1Right { get { foreach (var Source in this.Sources) { if (Source.Player1Right) return true; } return false; } }

		public bool Player2TL { get { foreach (var Source in this.Sources) { if (Source.Player2TL) return true; } return false; } }
		public bool Player2TR { get { foreach (var Source in this.Sources) { if (Source.Player2TR) return true; } return false; } }
		public bool Player2Up { get { foreach (var Source in this.Sources) { if (Source.Player2Up) return true; } return false; } }
		public bool Player2Down { get { foreach (var Source in this.Sources) { if (Source.Player2Down) return true; } return false; } }
		public bool Player2Left { get { foreach (var Source in this.Sources) { if (Source.Player2Left) return true; } return false; } }
		public bool Player2Right { get { foreach (var Source in this.Sources) { if (Source.Player2Right) return true; } return false; } }

		public bool Pause { get { foreach (var Source in this.Sources) { if (Source.Pause) return true; } return false; } }
		public bool Reset { get { foreach (var Source in this.Sources) { if (Source.Reset) return true; } return false; } }
		public bool Start { get { foreach (var Source in this.Sources) { if (Source.Start) return true; } return false; } }

		#endregion
	}


	class KeyboardInputSource : IInputSource {

		#region IInputSource Members

		public bool Player1TL { get; set; }
		public bool Player1TR { get; set; }
		public bool Player1Up { get; set; }
		public bool Player1Down { get; set; }
		public bool Player1Left { get; set; }
		public bool Player1Right { get; set; }

		public bool Player2TL { get; set; }
		public bool Player2TR { get; set; }
		public bool Player2Up { get; set; }
		public bool Player2Down { get; set; }
		public bool Player2Left { get; set; }
		public bool Player2Right { get; set; }

		public bool Pause { get; set; }
		public bool Reset { get; set; }
		public bool Start { get; set; }

		#endregion

		public Keys KeyPlayer1TL { get; set; }
		public Keys KeyPlayer1TR { get; set; }
		public Keys KeyPlayer1Up { get; set; }
		public Keys KeyPlayer1Down { get; set; }
		public Keys KeyPlayer1Left { get; set; }
		public Keys KeyPlayer1Right { get; set; }

		public Keys KeyPlayer2TL { get; set; }
		public Keys KeyPlayer2TR { get; set; }
		public Keys KeyPlayer2Up { get; set; }
		public Keys KeyPlayer2Down { get; set; }
		public Keys KeyPlayer2Left { get; set; }
		public Keys KeyPlayer2Right { get; set; }

		public Keys KeyPause { get; set; }
		public Keys KeyReset { get; set; }
		public Keys KeyStart { get; set; }

		public bool IsInputKey(Keys key) {
			return key == KeyPlayer1TL
				|| key == KeyPlayer1TR
				|| key == KeyPlayer1Up
				|| key == KeyPlayer1Down
				|| key == KeyPlayer1Left
				|| key == KeyPlayer1Right
				|| key == KeyPlayer2TL
				|| key == KeyPlayer2TR
				|| key == KeyPlayer2Up
				|| key == KeyPlayer2Down
				|| key == KeyPlayer2Left
				|| key == KeyPlayer2Right
				|| key == KeyPause
				|| key == KeyReset
				|| key == KeyStart;
		}

		public void KeyChange(KeyEventArgs key, bool pressed) {

			if (key.KeyCode == this.KeyPlayer1TL) this.Player1TL = pressed;
			if (key.KeyCode == this.KeyPlayer1TR) this.Player1TR = pressed;
			if (key.KeyCode == this.KeyPlayer1Up) this.Player1Up = pressed;
			if (key.KeyCode == this.KeyPlayer1Down) this.Player1Down = pressed;
			if (key.KeyCode == this.KeyPlayer1Left) this.Player1Left = pressed;
			if (key.KeyCode == this.KeyPlayer1Right) this.Player1Right = pressed;

			if (key.KeyCode == this.KeyPlayer2TL) this.Player2TL = pressed;
			if (key.KeyCode == this.KeyPlayer2TR) this.Player2TR = pressed;
			if (key.KeyCode == this.KeyPlayer2Up) this.Player2Up = pressed;
			if (key.KeyCode == this.KeyPlayer2Down) this.Player2Down = pressed;
			if (key.KeyCode == this.KeyPlayer2Left) this.Player2Left = pressed;
			if (key.KeyCode == this.KeyPlayer2Right) this.Player2Right = pressed;

			if (key.KeyCode == this.KeyPause) this.Pause = pressed;
			if (key.KeyCode == this.KeyReset) this.Reset = pressed;
			if (key.KeyCode == this.KeyStart) this.Start = pressed;

			key.Handled = IsInputKey(key.KeyCode);
			
		}

		public void LoadKeymapFromSettings() {
			this.KeyPlayer1TL = Properties.Settings.Default.KeyP1TL;
			this.KeyPlayer1TR = Properties.Settings.Default.KeyP1TR;
			this.KeyPlayer1Up = Properties.Settings.Default.KeyP1Up;
			this.KeyPlayer1Down = Properties.Settings.Default.KeyP1Down;
			this.KeyPlayer1Left = Properties.Settings.Default.KeyP1Left;
			this.KeyPlayer1Right = Properties.Settings.Default.KeyP1Right;
			this.KeyPlayer2TL = Properties.Settings.Default.KeyP2TL;
			this.KeyPlayer2TR = Properties.Settings.Default.KeyP2TR;
			this.KeyPlayer2Up = Properties.Settings.Default.KeyP2Up;
			this.KeyPlayer2Down = Properties.Settings.Default.KeyP2Down;
			this.KeyPlayer2Left = Properties.Settings.Default.KeyP2Left;
			this.KeyPlayer2Right = Properties.Settings.Default.KeyP2Right;
			this.KeyPause = Properties.Settings.Default.KeyPause;
			this.KeyReset = Properties.Settings.Default.KeyReset;
			this.KeyStart = Properties.Settings.Default.KeyStart;
		}
	}
}
