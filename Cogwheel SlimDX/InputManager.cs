using System.Collections.Generic;
using System.Windows.Forms;
using BeeDevelopment.Sega8Bit;
using CogwheelSlimDX.JoystickInput;
using System.IO;
using System;
using System.Configuration;
using System.Xml.Serialization;

namespace CogwheelSlimDX {

	#region General Input Management

	/// <summary>
	/// Defines the interface that input sources must expose.
	/// </summary>
	public interface IInputSource {

		/// <summary>Gets the state of the player 1 TL button (1).</summary>
		bool Player1TL { get; }
		/// <summary>Gets the state of the player 1 TR button (2).</summary>
		bool Player1TR { get; }
		/// <summary>Gets the state of the player 1 up button.</summary>
		bool Player1Up { get; }
		/// <summary>Gets the state of the player 1 down button.</summary>
		bool Player1Down { get; }
		/// <summary>Gets the state of the player 1 left button.</summary>
		bool Player1Left { get; }
		/// <summary>Gets the state of the player 1 right button.</summary>
		bool Player1Right { get; }

		/// <summary>Gets the state of the player 2 TL button (1).</summary>
		bool Player2TL { get; }
		/// <summary>Gets the state of the player 2 TL button (2).</summary>
		bool Player2TR { get; }
		/// <summary>Gets the state of the player 2 up button.</summary>
		bool Player2Up { get; }
		/// <summary>Gets the state of the player 2 down button.</summary>
		bool Player2Down { get; }
		/// <summary>Gets the state of the player 2 left button.</summary>
		bool Player2Left { get; }
		/// <summary>Gets the state of the player 2 right button.</summary>
		bool Player2Right { get; }

		/// <summary>Gets the state of the pause button.</summary>
		bool Pause { get; }
		/// <summary>Gets the state of the reset button.</summary>
		bool Reset { get; }
		/// <summary>Gets the state of the start button.</summary>
		bool Start { get; }

		/// <summary>This method is called immediately before reading the individual button states.</summary>
		void Poll();

		/// <summary>Reloads settings from the project settings file.</summary>
		void ReloadSettings();

		/// <summary>Saves settings to the project settings file.</summary>
		void UpdateSettings();

	}

	/// <summary>
	/// Provides methods for collecting input from multiple sources.
	/// </summary>
	public class InputManager : IInputSource {

		/// <summary>
		/// Gets a list of <see cref="IInputSource"/> sources.
		/// </summary>
		public List<IInputSource> Sources { get; private set; }

		/// <summary>
		/// Creates an instance of <see cref="InputManager"/>.
		/// </summary>
		public InputManager() {
			this.Sources = new List<IInputSource>();
		}


		/// <summary>
		/// Updates the input state of an <see cref="Emulator"/> instance.
		/// </summary>
		/// <param name="emulator">The emulator to update.</param>
		/// <remarks>This polls the input devices and sets the input state of the emulator for you.</remarks>
		public void UpdateEmulatorState(Emulator emulator) {
			emulator.SegaPorts[0].TL.State = !this.Player1TL;
			emulator.SegaPorts[0].TR.InputState = !this.Player1TR;
			emulator.SegaPorts[0].Up.State = !this.Player1Up;
			emulator.SegaPorts[0].Down.State = !this.Player1Down;
			emulator.SegaPorts[0].Left.State = !this.Player1Left;
			emulator.SegaPorts[0].Right.State = !this.Player1Right;
			emulator.SegaPorts[1].TL.State = !this.Player2TL;
			emulator.SegaPorts[1].TR.InputState = !this.Player2TR;
			emulator.SegaPorts[1].Up.State = !this.Player2Up;
			emulator.SegaPorts[1].Down.State = !this.Player2Down;
			emulator.SegaPorts[1].Left.State = !this.Player2Left;
			emulator.SegaPorts[1].Right.State = !this.Player2Right;
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

		public void Poll() { foreach (var Source in this.Sources) Source.Poll(); }

		public void ReloadSettings() { foreach (var Source in this.Sources) Source.ReloadSettings(); }
		public void UpdateSettings() { foreach (var Source in this.Sources) Source.UpdateSettings(); }

		#endregion
	}


	#endregion

	#region Keyboard

	/// <summary>
	/// Retrieves input from a keyboard.
	/// </summary>
	public class KeyboardInputSource : IInputSource {

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

		public void Poll() { }

		#endregion

		/// <summary>Defines the <see cref="Keys"/> to map to player 1 TL (1).</summary>
		public Keys KeyPlayer1TL { get; set; }
		/// <summary>Defines the <see cref="Keys"/> to map to player 1 TR (2).</summary>
		public Keys KeyPlayer1TR { get; set; }
		/// <summary>Defines the <see cref="Keys"/> to map to player 1 up.</summary>
		public Keys KeyPlayer1Up { get; set; }
		/// <summary>Defines the <see cref="Keys"/> to map to player 1 down.</summary>
		public Keys KeyPlayer1Down { get; set; }
		/// <summary>Defines the <see cref="Keys"/> to map to player 1 left.</summary>
		public Keys KeyPlayer1Left { get; set; }
		/// <summary>Defines the <see cref="Keys"/> to map to player 1 right.</summary>
		public Keys KeyPlayer1Right { get; set; }

		/// <summary>Defines the <see cref="Keys"/> to map to player 2 TL (1).</summary>
		public Keys KeyPlayer2TL { get; set; }
		/// <summary>Defines the <see cref="Keys"/> to map to player 2 TR (2).</summary>
		public Keys KeyPlayer2TR { get; set; }
		/// <summary>Defines the <see cref="Keys"/> to map to player 2 up.</summary>
		public Keys KeyPlayer2Up { get; set; }
		/// <summary>Defines the <see cref="Keys"/> to map to player 2 down.</summary>
		public Keys KeyPlayer2Down { get; set; }
		/// <summary>Defines the <see cref="Keys"/> to map to player 2 left.</summary>
		public Keys KeyPlayer2Left { get; set; }
		/// <summary>Defines the <see cref="Keys"/> to map to player 2 right.</summary>
		public Keys KeyPlayer2Right { get; set; }

		/// <summary>Defines the <see cref="Keys"/> to map to pause.</summary>
		public Keys KeyPause { get; set; }
		/// <summary>Defines the <see cref="Keys"/> to map to reset.</summary>
		public Keys KeyReset { get; set; }
		/// <summary>Defines the <see cref="Keys"/> to map to start.</summary>
		public Keys KeyStart { get; set; }

		/// <summary>
		/// Check whether a particular key is mapped as an input key.
		/// </summary>
		/// <param name="key">The key to check for.</param>
		/// <returns>True if the key is mapped as an input key, false otherwise.</returns>
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

		/// <summary>
		/// Process a key up or key down event.
		/// </summary>
		/// <param name="key">The <see cref="KeyEventArgs"/> referencing the key that was pressed.</param>
		/// <param name="pressed">True if the key was pressed, false if it was released.</param>
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

		/// <summary>
		/// Updates the internal key map from the project settings.
		/// </summary>
		public void ReloadSettings() {
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

		public void UpdateSettings() {
			Properties.Settings.Default.KeyP1TL = this.KeyPlayer1TL;
			Properties.Settings.Default.KeyP1TR = this.KeyPlayer1TR;
			Properties.Settings.Default.KeyP1Up= this.KeyPlayer1Up;
			Properties.Settings.Default.KeyP1Down=this.KeyPlayer1Down;
			Properties.Settings.Default.KeyP1Left = this.KeyPlayer1Left;
			Properties.Settings.Default.KeyP1Right= this.KeyPlayer1Right;
			Properties.Settings.Default.KeyP2TL = this.KeyPlayer2TL;
			Properties.Settings.Default.KeyP2TR = this.KeyPlayer2TR;
			Properties.Settings.Default.KeyP2Up = this.KeyPlayer2Up;
			Properties.Settings.Default.KeyP2Down = this.KeyPlayer2Down;
			Properties.Settings.Default.KeyP2Left = this.KeyPlayer2Left;
			Properties.Settings.Default.KeyP2Right = this.KeyPlayer2Right;
			Properties.Settings.Default.KeyPause = this.KeyPause;
			Properties.Settings.Default.KeyReset = this.KeyReset;
			Properties.Settings.Default.KeyStart=this.KeyStart;
		}
	}
	
	#endregion

	#region Joystick

	/// <summary>
	/// Retrieves input from a joystick.
	/// </summary>
	public class JoystickInputSource : IInputSource {

		/// <summary>
		/// Defines a trigger for a joystick event.
		/// </summary>
		public enum InputTrigger {
			/// <summary>No trigger is selected.</summary>
			None,
			/// <summary>Button 1.</summary>
			Button1,
			/// <summary>Button 2.</summary>
			Button2,
			/// <summary>Button 3.</summary>
			Button3,
			/// <summary>Button 4.</summary>
			Button4,
			/// <summary>Button 5.</summary>
			Button5,
			/// <summary>Button 6.</summary>
			Button6,
			/// <summary>Button 7.</summary>
			Button7,
			/// <summary>Button 8.</summary>
			Button8,
			/// <summary>Button 9.</summary>
			Button9,
			/// <summary>Button 10.</summary>
			Button10,
			/// <summary>Button 11.</summary>
			Button11,
			/// <summary>Button 12.</summary>
			Button12,
			/// <summary>Button 13.</summary>
			Button13,
			/// <summary>Button 14.</summary>
			Button14,
			/// <summary>Button 15.</summary>
			Button15,
			/// <summary>Button 16.</summary>
			Button16,
			/// <summary>Button 17.</summary>
			Button17,
			/// <summary>Button 18.</summary>
			Button18,
			/// <summary>Button 19.</summary>
			Button19,
			/// <summary>Button 20.</summary>
			Button20,
			/// <summary>Button 21.</summary>
			Button21,
			/// <summary>Button 22.</summary>
			Button22,
			/// <summary>Button 23.</summary>
			Button23,
			/// <summary>Button 24.</summary>
			Button24,
			/// <summary>Button 25.</summary>
			Button25,
			/// <summary>Button 26.</summary>
			Button26,
			/// <summary>Button 27.</summary>
			Button27,
			/// <summary>Button 28.</summary>
			Button28,
			/// <summary>Button 29.</summary>
			Button29,
			/// <summary>Button 30.</summary>
			Button30,
			/// <summary>Button 31.</summary>
			Button31,
			/// <summary>Button 32.</summary>
			Button32,
			/// <summary>The value of the X axis is decreased past the threshold.</summary>
			XAxisDecrease,
			/// <summary>The value of the X axis is increased past the threshold.</summary>
			XAxisIncrease,
			/// <summary>The value of the Y axis is decreased past the threshold.</summary>
			YAxisDecrease,
			/// <summary>The value of the Y axis is increased past the threshold.</summary>
			YAxisIncrease,
			/// <summary>The value of the Z axis is decreased past the threshold.</summary>
			ZAxisDecrease,
			/// <summary>The value of the Z axis is increased past the threshold.</summary>
			ZAxisIncrease,
			/// <summary>The value of the R axis is decreased past the threshold.</summary>
			RudderDecrease,
			/// <summary>The value of the R axis is increased past the threshold.</summary>
			RudderIncrease,
			/// <summary>The value of the U axis is decreased past the threshold.</summary>
			UAxisDecrease,
			/// <summary>The value of the U axis is increased past the threshold.</summary>
			UAxisIncrease,
			/// <summary>The value of the V axis is decreased past the threshold.</summary>
			VAxisDecrease,
			/// <summary>The value of the V axis is increased past the threshold.</summary>
			VAxisIncrease,
		}

		/// <summary>
		/// Gets the joystick that this <see cref="JoystickInputSource"/> polls for data.
		/// </summary>
		public Joystick Joystick { get; private set; }

		/// <summary>
		/// Gets or sets the threshold used.
		/// </summary>
		public float Threshold { get; private set; }

		/// <summary>
		/// Creates an instance of <see cref="JoystickInputSource"/>.
		/// </summary>
		/// <param name="joystick">The <see cref="Joystick"/> to poll for data.</param>
		public JoystickInputSource(Joystick joystick) {
			this.Mapping = new Mappings();
			this.Joystick = joystick; 
			this.Threshold = 0.3f; 
		}

		public bool SupportsTrigger(InputTrigger trigger) {
			if (trigger >= InputTrigger.Button1 && trigger <= InputTrigger.Button32) {
				return (int)trigger <= this.Joystick.ButtonCount;
			} else {
				switch (trigger) {
					case InputTrigger.XAxisDecrease:
					case InputTrigger.XAxisIncrease:
						return this.Joystick.HasXAxis;
					case InputTrigger.YAxisDecrease:
					case InputTrigger.YAxisIncrease:
						return this.Joystick.HasYAxis;
					case InputTrigger.ZAxisDecrease:
					case InputTrigger.ZAxisIncrease:
						return this.Joystick.HasZAxis;
					case InputTrigger.RudderDecrease:
					case InputTrigger.RudderIncrease:
						return this.Joystick.HasRudder;
					case InputTrigger.UAxisDecrease:
					case InputTrigger.UAxisIncrease:
						return this.Joystick.HasUAxis;
					case InputTrigger.VAxisDecrease:
					case InputTrigger.VAxisIncrease:
						return this.Joystick.HasVAxis;

				}
			}
			return false;
		}

		#region Mappings

		[Serializable]
		public class Mappings {
			/// <summary>Defines the <see cref="InputTrigger"/> to map to player 1 TL (1).</summary>
			public InputTrigger TriggerPlayer1TL { get; set; }
			/// <summary>Defines the <see cref="InputTrigger"/> to map to player 1 TR (2).</summary>
			public InputTrigger TriggerPlayer1TR { get; set; }
			/// <summary>Defines the <see cref="InputTrigger"/> to map to player 1 up.</summary>
			public InputTrigger TriggerPlayer1Up { get; set; }
			/// <summary>Defines the <see cref="InputTrigger"/> to map to player 1 down.</summary>
			public InputTrigger TriggerPlayer1Down { get; set; }
			/// <summary>Defines the <see cref="InputTrigger"/> to map to player 1 left.</summary>
			public InputTrigger TriggerPlayer1Left { get; set; }
			/// <summary>Defines the <see cref="InputTrigger"/> to map to player 1 right.</summary>
			public InputTrigger TriggerPlayer1Right { get; set; }

			/// <summary>Defines the <see cref="InputTrigger"/> to map to player 2 TL (1).</summary>
			public InputTrigger TriggerPlayer2TL { get; set; }
			/// <summary>Defines the <see cref="InputTrigger"/> to map to player 2 TR (2).</summary>
			public InputTrigger TriggerPlayer2TR { get; set; }
			/// <summary>Defines the <see cref="InputTrigger"/> to map to player 2 up.</summary>
			public InputTrigger TriggerPlayer2Up { get; set; }
			/// <summary>Defines the <see cref="InputTrigger"/> to map to player 2 down.</summary>
			public InputTrigger TriggerPlayer2Down { get; set; }
			/// <summary>Defines the <see cref="InputTrigger"/> to map to player 2 left.</summary>
			public InputTrigger TriggerPlayer2Left { get; set; }
			/// <summary>Defines the <see cref="InputTrigger"/> to map to player 2 right.</summary>
			public InputTrigger TriggerPlayer2Right { get; set; }

			/// <summary>Defines the <see cref="InputTrigger"/> to map to pause.</summary>
			public InputTrigger TriggerPause { get; set; }
			/// <summary>Defines the <see cref="InputTrigger"/> to map to reset.</summary>
			public InputTrigger TriggerReset { get; set; }
			/// <summary>Defines the <see cref="InputTrigger"/> to map to start.</summary>
			public InputTrigger TriggerStart { get; set; }
		}

		public Mappings Mapping { get; private set; }

		#endregion

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


		public void Poll() {
			var State = this.Joystick.GetState();
			if (State != null) {
				this.Player1TL = this.TranslateState(State, this.Mapping.TriggerPlayer1TL);
				this.Player1TR = this.TranslateState(State, this.Mapping.TriggerPlayer1TR);
				this.Player1Up = this.TranslateState(State, this.Mapping.TriggerPlayer1Up);
				this.Player1Down = this.TranslateState(State, this.Mapping.TriggerPlayer1Down);
				this.Player1Left = this.TranslateState(State, this.Mapping.TriggerPlayer1Left);
				this.Player1Right = this.TranslateState(State, this.Mapping.TriggerPlayer1Right);
				this.Player2TL = this.TranslateState(State, this.Mapping.TriggerPlayer2TL);
				this.Player2TR = this.TranslateState(State, this.Mapping.TriggerPlayer2TR);
				this.Player2Up = this.TranslateState(State, this.Mapping.TriggerPlayer2Up);
				this.Player2Down = this.TranslateState(State, this.Mapping.TriggerPlayer2Down);
				this.Player2Left = this.TranslateState(State, this.Mapping.TriggerPlayer2Left);
				this.Player2Right = this.TranslateState(State, this.Mapping.TriggerPlayer2Right);
				this.Pause = this.TranslateState(State, this.Mapping.TriggerPause);
				this.Reset = this.TranslateState(State, this.Mapping.TriggerReset);
				this.Start = this.TranslateState(State, this.Mapping.TriggerStart);
			}
		}

		private bool TranslateState(JoystickState state, InputTrigger trigger) {
			switch (trigger) {
				case InputTrigger.Button1: return (state.Buttons & Buttons.Button1) != Buttons.None;
				case InputTrigger.Button2: return (state.Buttons & Buttons.Button2) != Buttons.None;
				case InputTrigger.Button3: return (state.Buttons & Buttons.Button3) != Buttons.None;
				case InputTrigger.Button4: return (state.Buttons & Buttons.Button4) != Buttons.None;
				case InputTrigger.Button5: return (state.Buttons & Buttons.Button5) != Buttons.None;
				case InputTrigger.Button6: return (state.Buttons & Buttons.Button6) != Buttons.None;
				case InputTrigger.Button7: return (state.Buttons & Buttons.Button7) != Buttons.None;
				case InputTrigger.Button8: return (state.Buttons & Buttons.Button8) != Buttons.None;
				case InputTrigger.Button9: return (state.Buttons & Buttons.Button9) != Buttons.None;
				case InputTrigger.Button10: return (state.Buttons & Buttons.Button10) != Buttons.None;
				case InputTrigger.Button11: return (state.Buttons & Buttons.Button11) != Buttons.None;
				case InputTrigger.Button12: return (state.Buttons & Buttons.Button12) != Buttons.None;
				case InputTrigger.Button13: return (state.Buttons & Buttons.Button13) != Buttons.None;
				case InputTrigger.Button14: return (state.Buttons & Buttons.Button14) != Buttons.None;
				case InputTrigger.Button15: return (state.Buttons & Buttons.Button15) != Buttons.None;
				case InputTrigger.Button16: return (state.Buttons & Buttons.Button16) != Buttons.None;
				case InputTrigger.Button17: return (state.Buttons & Buttons.Button17) != Buttons.None;
				case InputTrigger.Button18: return (state.Buttons & Buttons.Button18) != Buttons.None;
				case InputTrigger.Button19: return (state.Buttons & Buttons.Button19) != Buttons.None;
				case InputTrigger.Button20: return (state.Buttons & Buttons.Button20) != Buttons.None;
				case InputTrigger.Button21: return (state.Buttons & Buttons.Button21) != Buttons.None;
				case InputTrigger.Button22: return (state.Buttons & Buttons.Button22) != Buttons.None;
				case InputTrigger.Button23: return (state.Buttons & Buttons.Button23) != Buttons.None;
				case InputTrigger.Button24: return (state.Buttons & Buttons.Button24) != Buttons.None;
				case InputTrigger.Button25: return (state.Buttons & Buttons.Button25) != Buttons.None;
				case InputTrigger.Button26: return (state.Buttons & Buttons.Button26) != Buttons.None;
				case InputTrigger.Button27: return (state.Buttons & Buttons.Button27) != Buttons.None;
				case InputTrigger.Button28: return (state.Buttons & Buttons.Button28) != Buttons.None;
				case InputTrigger.Button29: return (state.Buttons & Buttons.Button29) != Buttons.None;
				case InputTrigger.Button30: return (state.Buttons & Buttons.Button30) != Buttons.None;
				case InputTrigger.Button31: return (state.Buttons & Buttons.Button31) != Buttons.None;
				case InputTrigger.XAxisDecrease: return state.XAxis < -this.Threshold;
				case InputTrigger.XAxisIncrease: return state.XAxis > +this.Threshold;
				case InputTrigger.YAxisDecrease: return state.YAxis < -this.Threshold;
				case InputTrigger.YAxisIncrease: return state.YAxis > +this.Threshold;
				case InputTrigger.ZAxisDecrease: return state.ZAxis < -this.Threshold;
				case InputTrigger.ZAxisIncrease: return state.ZAxis > +this.Threshold;
				case InputTrigger.RudderDecrease: return state.Rudder < -this.Threshold;
				case InputTrigger.RudderIncrease: return state.Rudder > +this.Threshold;
				case InputTrigger.UAxisDecrease: return state.UAxis < -this.Threshold;
				case InputTrigger.UAxisIncrease: return state.UAxis > +this.Threshold;
				case InputTrigger.VAxisDecrease: return state.VAxis < -this.Threshold;
				case InputTrigger.VAxisIncrease: return state.VAxis > +this.Threshold;
				default: return false;
			}
		}

		public void ReloadSettings() {
			this.Mapping = new Mappings();
			if (!File.Exists(GetSettingsPath())) return;
			using (FileStream Config = File.OpenRead(GetSettingsPath())) {
				XmlSerializer Deserialiser = new XmlSerializer(typeof(Mappings));
				try {
					this.Mapping = (Mappings)Deserialiser.Deserialize(Config);
				} catch {
					this.Mapping = new Mappings();
				}
			}
		}

		public void UpdateSettings() {
			using (FileStream Config = File.Create(GetSettingsPath())) {
				XmlSerializer Serialiser = new XmlSerializer(typeof(Mappings));
				Serialiser.Serialize(Config, this.Mapping);
			}
		}

		private string GetSettingsPath() {
			string DirectoryPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Application.CompanyName), "Cogwheel");
			if (!Directory.Exists(DirectoryPath)) Directory.CreateDirectory(DirectoryPath);
			return Path.Combine(DirectoryPath, string.Format("Joystick_{0:X4}_{1:X4}.config", this.Joystick.VendorId, this.Joystick.ProductId));
		}


		#endregion
	}
	#endregion


}
