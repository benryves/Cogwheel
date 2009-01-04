using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using BeeDevelopment.Sega8Bit;
using BeeDevelopment.Sega8Bit.Hardware.Controllers;
using CogwheelSlimDX.JoystickInput;
using SlimDX.XInput;

namespace CogwheelSlimDX {

	#region Types

	/// <summary>
	/// Defines the various input buttons for a single player.
	/// </summary>
	public enum InputButton {
		/// <summary>No input.</summary>
		None,
		/// <summary>The "up" direction.</summary>
		Up,
		/// <summary>The "down" direction.</summary>
		Down,
		/// <summary>The "left" direction.</summary>
		Left,
		/// <summary>The "right" direction.</summary>
		Right,
		/// <summary>The first trigger button.</summary>
		Trigger1,
		/// <summary>The second trigger button.</summary>
		Trigger2,
		/// <summary>The third trigger button.</summary>
		Trigger3,
		/// <summary>The fourth trigger button.</summary>
		Trigger4,
		/// <summary>The Pause button.</summary>
		Pause,
		/// <summary>The Reset button.</summary>
		Reset,
		/// <summary>The Start button.</summary>
		Start,
		/// <summary>The 0 number button.</summary>
		Number0,
		/// <summary>The 1 number button.</summary>
		Number1,
		/// <summary>The 2 number button.</summary>
		Number2,
		/// <summary>The 3 number button.</summary>
		Number3,
		/// <summary>The 4 number button.</summary>
		Number4,
		/// <summary>The 5 number button.</summary>
		Number5,
		/// <summary>The 6 number button.</summary>
		Number6,
		/// <summary>The 7 number button.</summary>
		Number7,
		/// <summary>The 8 number button.</summary>
		Number8,
		/// <summary>The 9 number button.</summary>
		Number9,
		/// <summary>The Star (*) button.</summary>
		Star,
		/// <summary>The Hash (#) button.</summary>
		Hash,
		/// <summary>The 0 key on the SC-3000 keyboard.</summary>
		KeyboardD0,
		/// <summary>The 1 key on the SC-3000 keyboard.</summary>
		KeyboardD1,
		/// <summary>The 2 key on the SC-3000 keyboard.</summary>
		KeyboardD2,
		/// <summary>The 3 key on the SC-3000 keyboard.</summary>
		KeyboardD3,
		/// <summary>The 4 key on the SC-3000 keyboard.</summary>
		KeyboardD4,
		/// <summary>The 5 key on the SC-3000 keyboard.</summary>
		KeyboardD5,
		/// <summary>The 6 key on the SC-3000 keyboard.</summary>
		KeyboardD6,
		/// <summary>The 7 key on the SC-3000 keyboard.</summary>
		KeyboardD7,
		/// <summary>The 8 key on the SC-3000 keyboard.</summary>
		KeyboardD8,
		/// <summary>The 9 key on the SC-3000 keyboard.</summary>
		KeyboardD9,
		/// <summary>The A key on the SC-3000 keyboard.</summary>
		KeyboardA,
		/// <summary>The B key on the SC-3000 keyboard.</summary>
		KeyboardB,
		/// <summary>The C key on the SC-3000 keyboard.</summary>
		KeyboardC,
		/// <summary>The D key on the SC-3000 keyboard.</summary>
		KeyboardD,
		/// <summary>The E key on the SC-3000 keyboard.</summary>
		KeyboardE,
		/// <summary>The F key on the SC-3000 keyboard.</summary>
		KeyboardF,
		/// <summary>The G key on the SC-3000 keyboard.</summary>
		KeyboardG,
		/// <summary>The H key on the SC-3000 keyboard.</summary>
		KeyboardH,
		/// <summary>The I key on the SC-3000 keyboard.</summary>
		KeyboardI,
		/// <summary>The J key on the SC-3000 keyboard.</summary>
		KeyboardJ,
		/// <summary>The K key on the SC-3000 keyboard.</summary>
		KeyboardK,
		/// <summary>The L key on the SC-3000 keyboard.</summary>
		KeyboardL,
		/// <summary>The M key on the SC-3000 keyboard.</summary>
		KeyboardM,
		/// <summary>The N key on the SC-3000 keyboard.</summary>
		KeyboardN,
		/// <summary>The O key on the SC-3000 keyboard.</summary>
		KeyboardO,
		/// <summary>The P key on the SC-3000 keyboard.</summary>
		KeyboardP,
		/// <summary>The Q key on the SC-3000 keyboard.</summary>
		KeyboardQ,
		/// <summary>The R key on the SC-3000 keyboard.</summary>
		KeyboardR,
		/// <summary>The S key on the SC-3000 keyboard.</summary>
		KeyboardS,
		/// <summary>The T key on the SC-3000 keyboard.</summary>
		KeyboardT,
		/// <summary>The U key on the SC-3000 keyboard.</summary>
		KeyboardU,
		/// <summary>The V key on the SC-3000 keyboard.</summary>
		KeyboardV,
		/// <summary>The W key on the SC-3000 keyboard.</summary>
		KeyboardW,
		/// <summary>The X key on the SC-3000 keyboard.</summary>
		KeyboardX,
		/// <summary>The Y key on the SC-3000 keyboard.</summary>
		KeyboardY,
		/// <summary>The Z key on the SC-3000 keyboard.</summary>
		KeyboardZ,
		/// <summary>The Eng Dier's key on the SC-3000 keyboard.</summary>
		KeyboardEngDiers,
		/// <summary>The Space key on the SC-3000 keyboard.</summary>
		KeyboardSpace,
		/// <summary>The Home/Clr key on the SC-3000 keyboard.</summary>
		KeyboardHomeClr,
		/// <summary>The Ins/Del key on the SC-3000 keyboard.</summary>
		KeyboardInsDel,
		/// <summary>The Comma key on the SC-3000 keyboard.</summary>
		KeyboardComma,
		/// <summary>The FullStop key on the SC-3000 keyboard.</summary>
		KeyboardPeriod,
		/// <summary>The Slash key on the SC-3000 keyboard.</summary>
		KeyboardSlash,
		/// <summary>The Pi key on the SC-3000 keyboard.</summary>
		KeyboardPi,
		/// <summary>The up cursor key on the SC-3000 keyboard.</summary>
		KeyboardUp,
		/// <summary>The down cursor key on the SC-3000 keyboard.</summary>
		KeyboardDown,
		/// <summary>The left cursor key on the SC-3000 keyboard.</summary>
		KeyboardLeft,
		/// <summary>The right cursor key on the SC-3000 keyboard.</summary>
		KeyboardRight,
		/// <summary>The semicolon key on the SC-3000 keyboard.</summary>
		KeyboardSemicolon,
		/// <summary>The colon key on the SC-3000 keyboard.</summary>
		KeyboardColon,
		/// <summary>The right bracket key on the SC-3000 keyboard.</summary>
		KeyboardCloseBrackets,
		/// <summary>The return key on the SC-3000 keyboard.</summary>
		KeyboardCarriageReturn,
		/// <summary>The at (@) key on the SC-3000 keyboard.</summary>
		KeyboardAtSign,
		/// <summary>The left bracket key on the SC-3000 keyboard.</summary>
		KeyboardOpenBrackets,
		/// <summary>The - key on the SC-3000 keyboard.</summary>
		KeyboardMinus,
		/// <summary>The caret (^) key on the SC-3000 keyboard.</summary>
		KeyboardCaret,
		/// <summary>The Yen key on the SC-3000 keyboard.</summary>
		KeyboardYen,
		/// <summary>The break key on the SC-3000 keyboard.</summary>
		KeyboardBreak,
		/// <summary>The graph key on the SC-3000 keyboard.</summary>
		KeyboardGraph,
		/// <summary>The control key on the SC-3000 keyboard.</summary>
		KeyboardControl,
		/// <summary>The func key on the SC-3000 keyboard.</summary>
		KeyboardFunc,
		/// <summary>The shift key on the SC-3000 keyboard.</summary>
		KeyboardShift,
	}

	#endregion

	#region General Input Management

	/// <summary>
	/// Defines the interface that input sources must expose.
	/// </summary>
	public interface IInputSource {

		/// <summary>
		/// Gets the state of an individual button.
		/// </summary>
		/// <param name="controllerIndex">The index of the controller.</param>
		/// <param name="button">The button to get the state of.</param>
		/// <returns>True if the button is pressed; false otherwise.</returns>
		bool GetButtonState(int controllerIndex, InputButton button);

		/// <summary>Release all held keys.</summary>
		void ReleaseAll();

		/// <summary>This method is called immediately before reading the individual button states.</summary>
		void Poll();

		/// <summary>Reloads settings from the project settings file.</summary>
		void ReloadSettings();

		/// <summary>Saves settings to the project settings file.</summary>
		void UpdateSettings();

		/// <summary>
		/// Sets a particular trigger for a particular input button.
		/// </summary>
		/// <param name="controllerIndex">The index of the controller to set the trigger for.</param>
		/// <param name="button">The button on the controller you wish to set the trigger for.</param>
		/// <param name="trigger">The trigger to set.</param>
		void SetTrigger(int controllerIndex, InputButton button, object trigger);

		/// <summary>
		/// Gets a particular trigger for a particular input button.
		/// </summary>
		/// <param name="controllerIndex">The index of the controller to get the trigger for.</param>
		/// <param name="button">The button on the controller you wish to get the trigger for.</param>
		/// <returns>The corresponding trigger.</returns>
		object GetTrigger(int controllerIndex, InputButton button);

	}

	/// <summary>
	/// Provides methods for collecting input from multiple sources.
	/// </summary>
	public class InputManager : IInputSource {

		/// <summary>
		/// Gets a list of <see cref="IInputSource"/> sources.
		/// </summary>
		public List<IInputSource> Sources { get; private set; }

		public string ProfileDirectory { get; private set; }

		/// <summary>
		/// Creates an instance of <see cref="InputManager"/>.
		/// </summary>
		public InputManager(string profileDirectory) {
			this.ProfileDirectory = profileDirectory;
			this.Sources = new List<IInputSource>();
		}

		public bool GetButtonState(int controllerIndex, InputButton button) {
			foreach (var Source in Sources) if (Source.GetButtonState(controllerIndex, button)) return true;
			return false;
		}


		/// <summary>
		/// Updates the input state of an <see cref="Emulator"/> instance.
		/// </summary>
		/// <param name="emulator">The emulator to update.</param>
		public void UpdateEmulatorState(Emulator emulator) {

			for (int Player = 0; Player< 2; ++Player) {

				#region Common D-Pad and Triggers

				// Get the state of common buttons:
				var Trigger1 = this.GetButtonState(Player, InputButton.Trigger1);
				var Trigger2 = this.GetButtonState(Player, InputButton.Trigger2);
				var Up = this.GetButtonState(Player, InputButton.Up);
				var Down = this.GetButtonState(Player, InputButton.Down);
				var Left = this.GetButtonState(Player, InputButton.Left);
				var Right = this.GetButtonState(Player, InputButton.Right);

				// Assign to the state of the Sega controller ports:
				emulator.SegaPorts[Player].TL.State = !Trigger1;
				emulator.SegaPorts[Player].TR.InputState = !Trigger2;
				emulator.SegaPorts[Player].Up.State = !Up;
				emulator.SegaPorts[Player].Down.State = !Down;
				emulator.SegaPorts[Player].Left.State = !Left;
				emulator.SegaPorts[Player].Right.State = !Right;

				// Assign to the state of the ColecoVision controller ports:
				emulator.ColecoVisionPorts[Player].Fire1 = Trigger1;
				emulator.ColecoVisionPorts[Player].Fire2 = Trigger2;
				emulator.ColecoVisionPorts[Player].Up = Up;
				emulator.ColecoVisionPorts[Player].Down = Down;
				emulator.ColecoVisionPorts[Player].Left = Left;
				emulator.ColecoVisionPorts[Player].Right = Right;

				#endregion

				#region ColecoVision Number Pad

				emulator.ColecoVisionPorts[Player].Number0 = this.GetButtonState(Player, InputButton.Number0);
				emulator.ColecoVisionPorts[Player].Number1 = this.GetButtonState(Player, InputButton.Number1);
				emulator.ColecoVisionPorts[Player].Number2 = this.GetButtonState(Player, InputButton.Number2);
				emulator.ColecoVisionPorts[Player].Number3 = this.GetButtonState(Player, InputButton.Number3);
				emulator.ColecoVisionPorts[Player].Number4 = this.GetButtonState(Player, InputButton.Number4);
				emulator.ColecoVisionPorts[Player].Number5 = this.GetButtonState(Player, InputButton.Number5);
				emulator.ColecoVisionPorts[Player].Number6 = this.GetButtonState(Player, InputButton.Number6);
				emulator.ColecoVisionPorts[Player].Number7 = this.GetButtonState(Player, InputButton.Number7);
				emulator.ColecoVisionPorts[Player].Number8 = this.GetButtonState(Player, InputButton.Number8);
				emulator.ColecoVisionPorts[Player].Number9 = this.GetButtonState(Player, InputButton.Number9);
				emulator.ColecoVisionPorts[Player].Star = this.GetButtonState(Player, InputButton.Star);
				emulator.ColecoVisionPorts[Player].Hash = this.GetButtonState(Player, InputButton.Hash);

				#endregion

			}

			// Special non-player-specific buttons.
			if (emulator.HasPauseButton) emulator.PauseButton = this.GetButtonState(0, InputButton.Pause);
			if (emulator.HasResetButton) emulator.ResetButton = this.GetButtonState(0, InputButton.Reset);
			if (emulator.HasStartButton) emulator.StartButton = this.GetButtonState(0, InputButton.Start);

			// SC-3000 keyboard.
			if (emulator.Family == HardwareFamily.SC3000) {

				foreach (SC3000Keyboard.Keys Key in Enum.GetValues(typeof(SC3000Keyboard.Keys))) {
					emulator.Keyboard.SetKeyState(Key, this.GetButtonState(0, KeyboardToInputButton(Key)));	
				}

				emulator.Keyboard.UpdateState();
			}

		}

		public void Poll() { foreach (var Source in this.Sources) Source.Poll(); }

		public void ReloadSettings() { foreach (var Source in this.Sources) Source.ReloadSettings(); }
		public void UpdateSettings() { foreach (var Source in this.Sources) Source.UpdateSettings(); }

		public void ReleaseAll() { foreach (var Source in this.Sources) Source.ReleaseAll(); }

		public void SetTrigger(int controllerIndex, InputButton button, object trigger) { }

		public object GetTrigger(int controllerIndex, InputButton button) { return null; }

		public static InputButton KeyboardToInputButton(SC3000Keyboard.Keys button) {
			switch (button) {

				case SC3000Keyboard.Keys.A: return InputButton.KeyboardA;
				case SC3000Keyboard.Keys.B: return InputButton.KeyboardB;
				case SC3000Keyboard.Keys.C: return InputButton.KeyboardC;
				case SC3000Keyboard.Keys.D: return InputButton.KeyboardD;
				case SC3000Keyboard.Keys.E: return InputButton.KeyboardE;
				case SC3000Keyboard.Keys.F: return InputButton.KeyboardF;
				case SC3000Keyboard.Keys.G: return InputButton.KeyboardG;
				case SC3000Keyboard.Keys.H: return InputButton.KeyboardH;
				case SC3000Keyboard.Keys.I: return InputButton.KeyboardI;
				case SC3000Keyboard.Keys.J: return InputButton.KeyboardJ;
				case SC3000Keyboard.Keys.K: return InputButton.KeyboardK;
				case SC3000Keyboard.Keys.L: return InputButton.KeyboardL;
				case SC3000Keyboard.Keys.M: return InputButton.KeyboardM;
				case SC3000Keyboard.Keys.N: return InputButton.KeyboardN;
				case SC3000Keyboard.Keys.O: return InputButton.KeyboardO;
				case SC3000Keyboard.Keys.P: return InputButton.KeyboardP;
				case SC3000Keyboard.Keys.Q: return InputButton.KeyboardQ;
				case SC3000Keyboard.Keys.R: return InputButton.KeyboardR;
				case SC3000Keyboard.Keys.S: return InputButton.KeyboardS;
				case SC3000Keyboard.Keys.T: return InputButton.KeyboardT;
				case SC3000Keyboard.Keys.U: return InputButton.KeyboardU;
				case SC3000Keyboard.Keys.V: return InputButton.KeyboardV;
				case SC3000Keyboard.Keys.W: return InputButton.KeyboardW;
				case SC3000Keyboard.Keys.X: return InputButton.KeyboardX;
				case SC3000Keyboard.Keys.Y: return InputButton.KeyboardY;
				case SC3000Keyboard.Keys.Z: return InputButton.KeyboardZ;

				case SC3000Keyboard.Keys.D0: return InputButton.KeyboardD0;
				case SC3000Keyboard.Keys.D1: return InputButton.KeyboardD1;
				case SC3000Keyboard.Keys.D2: return InputButton.KeyboardD2;
				case SC3000Keyboard.Keys.D3: return InputButton.KeyboardD3;
				case SC3000Keyboard.Keys.D4: return InputButton.KeyboardD4;
				case SC3000Keyboard.Keys.D5: return InputButton.KeyboardD5;
				case SC3000Keyboard.Keys.D6: return InputButton.KeyboardD6;
				case SC3000Keyboard.Keys.D7: return InputButton.KeyboardD7;
				case SC3000Keyboard.Keys.D8: return InputButton.KeyboardD8;
				case SC3000Keyboard.Keys.D9: return InputButton.KeyboardD9;

				case SC3000Keyboard.Keys.Up: return InputButton.KeyboardUp;
				case SC3000Keyboard.Keys.Down: return InputButton.KeyboardDown;
				case SC3000Keyboard.Keys.Left: return InputButton.KeyboardLeft;
				case SC3000Keyboard.Keys.Right: return InputButton.KeyboardRight;
				
				case SC3000Keyboard.Keys.AtSign: return InputButton.KeyboardAtSign;
				case SC3000Keyboard.Keys.Break: return InputButton.KeyboardBreak;
				case SC3000Keyboard.Keys.Caret: return InputButton.KeyboardCaret;
				case SC3000Keyboard.Keys.Colon: return InputButton.KeyboardColon;
				case SC3000Keyboard.Keys.Comma: return InputButton.KeyboardComma;
				case SC3000Keyboard.Keys.Control: return InputButton.KeyboardControl;
				case SC3000Keyboard.Keys.EngDiers: return InputButton.KeyboardEngDiers;
				case SC3000Keyboard.Keys.Period: return InputButton.KeyboardPeriod;
				case SC3000Keyboard.Keys.Func: return InputButton.KeyboardFunc;
				case SC3000Keyboard.Keys.Graph: return InputButton.KeyboardGraph;
				case SC3000Keyboard.Keys.HomeClr: return InputButton.KeyboardHomeClr;
				case SC3000Keyboard.Keys.InsDel: return InputButton.KeyboardInsDel;
				case SC3000Keyboard.Keys.OpenBrackets: return InputButton.KeyboardOpenBrackets;
				case SC3000Keyboard.Keys.Minus: return InputButton.KeyboardMinus;
				case SC3000Keyboard.Keys.Pi: return InputButton.KeyboardPi;
				case SC3000Keyboard.Keys.CarriageReturn: return InputButton.KeyboardCarriageReturn;
				case SC3000Keyboard.Keys.CloseBrackets: return InputButton.KeyboardCloseBrackets;
				case SC3000Keyboard.Keys.Semicolon: return InputButton.KeyboardSemicolon;
				case SC3000Keyboard.Keys.Shift: return InputButton.KeyboardShift;
				case SC3000Keyboard.Keys.Slash: return InputButton.KeyboardSlash;
				case SC3000Keyboard.Keys.Space: return InputButton.KeyboardSpace;
				case SC3000Keyboard.Keys.Yen: return InputButton.KeyboardYen;

				default: return InputButton.None;
			}
		}
	}

	#endregion

	public abstract class TriggeredInputSource<T> : IInputSource {

		protected Dictionary<T, KeyValuePair<int, InputButton>[]> KeyMapping;
		protected Dictionary<InputButton, bool>[] CurrentStates;

		public virtual void Poll() { }

		public abstract string SettingsFilename { get; }

		public InputManager Manager { get; private set; }

		private string SettingsPath {
			get {
				string DirectoryPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Application.CompanyName), "Cogwheel");
				if (!string.IsNullOrEmpty(this.Manager.ProfileDirectory)) DirectoryPath = Path.Combine(DirectoryPath, this.Manager.ProfileDirectory);
				if (!Directory.Exists(DirectoryPath)) Directory.CreateDirectory(DirectoryPath);
				return Path.Combine(DirectoryPath, this.SettingsFilename);
			}
		}

		public virtual string DefaultSettingsFile {
			get { return null; }
		}

		/// <summary>
		/// Loads all settings from the settings file.
		/// </summary>
		public void ReloadSettings() {
			this.KeyMapping.Clear();
			if (this.DefaultSettingsFile != null) this.LoadSettingsFromLines(DefaultSettingsFile.Split('\n'));
			if (File.Exists(this.SettingsPath)) {
				this.LoadSettingsFromLines(File.ReadAllLines(this.SettingsPath));
			}
		}

		private void LoadSettingsFromLines(string[] lines) {
			foreach (var ConfigLine in lines) {

				// Skip comments.
				if (ConfigLine.TrimStart().StartsWith(";")) continue;

				// Split into two halves -- Key=>Mapping
				var KeyComponents = ConfigLine.Split(new[] { "=>" }, StringSplitOptions.RemoveEmptyEntries);

				// Check that it's a valid pair, otherwise skip.
				if (KeyComponents.Length != 2) continue;

				// Quick-and-dirty conversion.
				try {
					T MappedKey = (T)Enum.Parse(typeof(T), KeyComponents[0]);
					var KeyTargets = Array.ConvertAll(KeyComponents[1].Split(';'), C => {
						var SubComponents = C.Split('.');
						return new KeyValuePair<int, InputButton>(int.Parse(SubComponents[0]), (InputButton)Enum.Parse(typeof(InputButton), SubComponents[1]));
					});
					if (KeyTargets != null && KeyTargets.Length != 0) {
						if (this.KeyMapping.ContainsKey(MappedKey)) {
							this.KeyMapping.Remove(MappedKey);
						}
						this.KeyMapping.Add(MappedKey, KeyTargets);
					}
				} catch { }

			}
		}

		/// <summary>
		/// Saves all settings to the settings file.
		/// </summary>
		public void UpdateSettings() {
			var OutputLines = new List<string>();
			foreach (var Mapping in this.KeyMapping) {
				OutputLines.Add(
					string.Format(
						"{0}=>{1}",
						Mapping.Key,
						string.Join(";", Array.ConvertAll(Mapping.Value, C => string.Format("{0}.{1}", C.Key, C.Value))))
				);
			}
			File.WriteAllLines(this.SettingsPath, OutputLines.ToArray());
		}

		public bool GetButtonState(int controllerIndex, InputButton button) {
			if (controllerIndex < 0 || controllerIndex > 1) return false;
			bool Result = false;
			return this.CurrentStates[controllerIndex].TryGetValue(button, out Result) ? Result : false;
		}

		public void ReleaseAll() {
			this.CurrentStates = new Dictionary<InputButton, bool>[2];
			for (int Player = 0; Player < 2; ++Player) {
				this.CurrentStates[Player] = new Dictionary<InputButton, bool>();
				foreach (InputButton Button in Enum.GetValues(typeof(InputButton))) {
					this.CurrentStates[Player].Add(Button, false);
				}
			}
		}



		public void SetTrigger(int controllerIndex, InputButton button, object trigger) {
			if (trigger is T) {
				T Trigger = (T)trigger;

				// Remove any existing matching trigger.
				var NewMapping = new Dictionary<T, KeyValuePair<int, InputButton>[]>();
				foreach (var Mapping in this.KeyMapping) {
					var CleanedMapping = new List<KeyValuePair<int, InputButton>>();
					foreach (var ExistingMapping in Mapping.Value) {
						if (ExistingMapping.Key != controllerIndex || ExistingMapping.Value != button) {
							CleanedMapping.Add(ExistingMapping);
						}
					}
					if (CleanedMapping.Count > 0) NewMapping.Add(Mapping.Key, CleanedMapping.ToArray());
				}
				this.KeyMapping = NewMapping;


				// If it's a key of any worth, add it.
				if (Convert.ToInt32(Trigger) != 0) {
					var AddedTrigger = new List<KeyValuePair<int, InputButton>>();
					if (this.KeyMapping.ContainsKey(Trigger)) {
						AddedTrigger.AddRange(this.KeyMapping[Trigger]);
					} else {
						this.KeyMapping.Add(Trigger, null);
					}
					AddedTrigger.Add(new KeyValuePair<int, InputButton>(controllerIndex, button));
					this.KeyMapping[Trigger] = AddedTrigger.ToArray();

				}
			}
		}

		public object GetTrigger(int controllerIndex, InputButton button) {
			foreach (var MappedKey in this.KeyMapping) {
				foreach (var TargetButtons in MappedKey.Value) {
					if (TargetButtons.Key == controllerIndex && TargetButtons.Value == button) return MappedKey.Key;
				}
			}
			return default(T);
		}

		public TriggeredInputSource(InputManager manager) {
			this.Manager = manager;
			this.KeyMapping = new Dictionary<T, KeyValuePair<int, InputButton>[]>(32);
			this.ReleaseAll();
		}


	}

	#region Keyboard

	/// <summary>
	/// Retrieves input from a keyboard.
	/// </summary>
	public class KeyboardInputSource : TriggeredInputSource<Keys> {

		#region Public Methods

		/// <summary>
		/// Checks whether a particular keyboard key is mapped to a button.
		/// </summary>
		/// <param name="key">The key to check.</param>
		/// <returns>True if the key is a valid input key; false otherwise.</returns>
		public bool IsInputKey(Keys key) {
			return this.KeyMapping.ContainsKey(key);
		}

		public void KeyChange(KeyEventArgs key, bool state) {
			KeyValuePair<int, InputButton>[] MappedButtons;
			if (this.KeyMapping.TryGetValue(key.KeyCode, out MappedButtons)) {
				foreach (var MappedButton in MappedButtons) {
					this.CurrentStates[MappedButton.Key][MappedButton.Value] = state;
				}
			}
		}

		public override string DefaultSettingsFile {
			get {
				return !string.IsNullOrEmpty(this.Manager.ProfileDirectory) && Path.GetFileName(this.Manager.ProfileDirectory).Contains("3000")
					? Properties.Resources.Config_SC3000KeyMapping
					: Properties.Resources.Config_DefaultKeyMapping;
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Gets the full path to the settings file.
		/// </summary>
		public override string SettingsFilename {
			get { return "Keyboard.config"; }
		}

		#endregion

		public KeyboardInputSource(InputManager manager)
			: base(manager) {
		}

	}
	
	#endregion

	#region Joystick

	/// <summary>
	/// Provides input from a joystick.
	/// </summary>
	public class JoystickInputSource : TriggeredInputSource<JoystickInputSource.InputTrigger> {

		#region Types

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
			/// <summary>The POV hat is pointing North.</summary>
			PovN,
			/// <summary>The POV hat is pointing North-East.</summary>
			PovNE,
			/// <summary>The POV hat is pointing East.</summary>
			PovE,
			/// <summary>The POV hat is pointing South-East.</summary>
			PovSE,
			/// <summary>The POV hat is pointing South.</summary>
			PovS,
			/// <summary>The POV hat is pointing South-West.</summary>
			PovSW,
			/// <summary>The POV hat is pointing West.</summary>
			PovW,
			/// <summary>The POV hat is pointing North-West.</summary>
			PovNW,
		}

		#endregion

		#region Private Fields

		/// <summary>
		/// The current joystick state.
		/// </summary>
		private JoystickState State = new JoystickState();

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets the <see cref="Joystick"/> that this input source retrieves input from.
		/// </summary>
		public Joystick Joystick { get; private set; }

		#endregion

		#region Public Methods

		public KeyValuePair<InputTrigger, bool>[] GetTriggeredEvents() {

			var Events = new List<KeyValuePair<InputTrigger, bool>>(8);

			var NewState = this.Joystick.GetState();
			if (NewState != null && this.State != null) {

				// Buttons:
				for (int i = 0; i < 32; ++i) {
					if (((int)this.State.Buttons & (1 << i)) != ((int)NewState.Buttons & (1 << i))) {
						Events.Add(new KeyValuePair<InputTrigger, bool>((InputTrigger)(i + 1), ((int)NewState.Buttons & (1 << i)) != 0));
					}
				}

				// Axes:
				if (this.Joystick.HasXAxis) AddAxisEvent(Events, this.State.XAxis, NewState.XAxis, InputTrigger.XAxisIncrease, InputTrigger.XAxisDecrease);
				if (this.Joystick.HasYAxis) AddAxisEvent(Events, this.State.YAxis, NewState.YAxis, InputTrigger.YAxisIncrease, InputTrigger.YAxisDecrease);
				if (this.Joystick.HasZAxis) AddAxisEvent(Events, this.State.ZAxis, NewState.ZAxis, InputTrigger.ZAxisIncrease, InputTrigger.ZAxisDecrease);
				if (this.Joystick.HasUAxis) AddAxisEvent(Events, this.State.UAxis, NewState.UAxis, InputTrigger.UAxisIncrease, InputTrigger.UAxisDecrease);
				if (this.Joystick.HasVAxis) AddAxisEvent(Events, this.State.VAxis, NewState.VAxis, InputTrigger.VAxisIncrease, InputTrigger.VAxisDecrease);
				if (this.Joystick.HasRudder) AddAxisEvent(Events, this.State.Rudder, NewState.Rudder, InputTrigger.RudderIncrease, InputTrigger.RudderDecrease);

				// POV hat.
				if (NewState.PointOfView.HasValue != State.PointOfView.HasValue) {
					if (NewState.PointOfView.HasValue) {
						// POV hat moved from neutral to point at something.
						Events.Add(new KeyValuePair<InputTrigger, bool>(BearingToPovTrigger(NewState.PointOfView.Value), true));
					} else {
						// POV hat moved from something to neutral.
						Events.Add(new KeyValuePair<InputTrigger, bool>(BearingToPovTrigger(State.PointOfView.Value), false));
					}
				} else if (NewState.PointOfView.HasValue && NewState.PointOfView != State.PointOfView) {
					// POV hat moved from one bearing to another without returning to neutral.
					InputTrigger NewPovTrigger = BearingToPovTrigger(NewState.PointOfView.Value);
					InputTrigger OldPovTrigger = BearingToPovTrigger(State.PointOfView.Value);
					if (NewPovTrigger != OldPovTrigger) {
						Events.Add(new KeyValuePair<InputTrigger, bool>(OldPovTrigger, false));
						Events.Add(new KeyValuePair<InputTrigger, bool>(NewPovTrigger, true));
					}
				}

				this.State = NewState;
			}

			return Events.ToArray();
		}

		static InputTrigger BearingToPovTrigger(float bearing) {
			for (int i = 0; i < 8; ++i) {
				float Comparison = i * 45f;
				if (AngleIsInRange(bearing, Comparison - 22.5f, Comparison + 22.5f)) {
					return InputTrigger.PovN + i;
				}
			}
			return InputTrigger.None;
		}


		static float NormaliseAngle(float a) {
			a += 180f;
			while (a < 0f) a += 360f;
			while (a >= 360f) a -= 360f;
			a -= 180f;
			return a;
		}

		static bool AngleIsInRange(float angle, float min, float max) {
			angle = angle - min; max = max - min; min = 0f;
			angle = NormaliseAngle(angle);
			min = NormaliseAngle(min);
			max = NormaliseAngle(max);
			return min <= angle && angle <= max;
		}

		static float PovTriggerToBearing(InputTrigger trigger) {
			switch (trigger) {
				case InputTrigger.PovN:  return 000f;
				case InputTrigger.PovNE: return 045f;
				case InputTrigger.PovE:  return 090f;
				case InputTrigger.PovSE: return 135f;
				case InputTrigger.PovS:  return 180f;
				case InputTrigger.PovSW: return 225f;
				case InputTrigger.PovW:  return 270f;
				case InputTrigger.PovNW: return 315f;
			}
			return float.NaN;
		}


		private void AddAxisEvent(List<KeyValuePair<InputTrigger, bool>> triggers, float oldAxisValue, float newAxisValue, InputTrigger increased, InputTrigger decreased) {
			float Threshold = 0.3f;
			if (oldAxisValue < +Threshold && newAxisValue > +Threshold) triggers.Add(new KeyValuePair<InputTrigger, bool>(increased, true));
			if (newAxisValue < +Threshold && oldAxisValue > +Threshold) triggers.Add(new KeyValuePair<InputTrigger, bool>(increased, false));
			if (oldAxisValue > -Threshold && newAxisValue < -Threshold) triggers.Add(new KeyValuePair<InputTrigger, bool>(decreased, true));
			if (newAxisValue > -Threshold && oldAxisValue < -Threshold) triggers.Add(new KeyValuePair<InputTrigger, bool>(decreased, false));
		}

		public override void Poll() {
			var State = this.Joystick.GetState();
			if (State == null) return;
			foreach (var MappedButton in this.KeyMapping) {
				bool Triggered = false;
				if (MappedButton.Key >= InputTrigger.Button1 && MappedButton.Key <= InputTrigger.Button32) {
					Triggered = ((int)State.Buttons & (1 << (((int)MappedButton.Key) - 1))) != 0;
				} else if (MappedButton.Key >= InputTrigger.XAxisDecrease && MappedButton.Key <= InputTrigger.VAxisIncrease) {
					float Threshold = 0.3f;
					switch (MappedButton.Key) {
						case InputTrigger.XAxisIncrease: Triggered = this.Joystick.HasXAxis && State.XAxis > +Threshold; break;
						case InputTrigger.XAxisDecrease: Triggered = this.Joystick.HasXAxis && State.XAxis < -Threshold; break;
						case InputTrigger.YAxisIncrease: Triggered = this.Joystick.HasYAxis && State.YAxis > +Threshold; break;
						case InputTrigger.YAxisDecrease: Triggered = this.Joystick.HasYAxis && State.YAxis < -Threshold; break;
						case InputTrigger.ZAxisIncrease: Triggered = this.Joystick.HasZAxis && State.ZAxis > +Threshold; break;
						case InputTrigger.ZAxisDecrease: Triggered = this.Joystick.HasZAxis && State.ZAxis < -Threshold; break;
						case InputTrigger.UAxisIncrease: Triggered = this.Joystick.HasUAxis && State.UAxis > +Threshold; break;
						case InputTrigger.UAxisDecrease: Triggered = this.Joystick.HasUAxis && State.UAxis < -Threshold; break;
						case InputTrigger.VAxisIncrease: Triggered = this.Joystick.HasVAxis && State.VAxis > +Threshold; break;
						case InputTrigger.VAxisDecrease: Triggered = this.Joystick.HasVAxis && State.VAxis < -Threshold; break;
						case InputTrigger.RudderIncrease: Triggered = this.Joystick.HasRudder && State.Rudder > +Threshold; break;
						case InputTrigger.RudderDecrease: Triggered = this.Joystick.HasRudder && State.Rudder < -Threshold; break;
					}
				} else if (MappedButton.Key >= InputTrigger.PovN && MappedButton.Key <= InputTrigger.PovNW) {
					if (State.PointOfView.HasValue) {
						var Bearing = PovTriggerToBearing(MappedButton.Key);
						Triggered = AngleIsInRange(State.PointOfView.Value, Bearing - 45.0f, Bearing + 45.0f);
					}
				}
				foreach (var TargetButton in MappedButton.Value) {
					this.CurrentStates[TargetButton.Key][TargetButton.Value] = Triggered;
				}
			}
		}

		#endregion

		#region Constructor

		public JoystickInputSource(InputManager manager, Joystick joystick)
			: base(manager) {
			this.Joystick = joystick;
		}

		#endregion

		public override string SettingsFilename {
			get { return string.Format("Joystick.{0:X4}.{1:X4}.config", this.Joystick.VendorId, this.Joystick.ProductId); }
		}
	}

	#endregion

	#region XInput

	/// <summary>
	/// Provides input from an XInput device.
	/// </summary>
	public class XInputSource : TriggeredInputSource<XInputSource.InputTrigger> {

		#region Types

		public enum InputTrigger {
			/// <summary>No trigger is selected.</summary>
			None,
			/// <summary>Up on the D-pad.</summary>
			DPadUp = 1,
			/// <summary>Down on the D-pad.</summary>
			DPadDown = 2,
			/// <summary>Left on the D-pad.</summary>
			DPadLeft = 3,
			/// <summary>Right on the D-pad.</summary>
			DPadRight = 4,
			/// <summary>Start button.</summary>
			Start = 5,
			/// <summary>Back button.</summary>
			Back = 6,
			/// <summary>Left thumbstick button.</summary>
			LeftThumb = 7,
			/// <summary>Right thumbstick button.</summary>
			RightThumb = 8,
			/// <summary>Left shoulder button.</summary>
			LeftShoulder = 9,
			/// <summary>Right shoulder button.</summary>
			RightShoulder = 10,
			/// <summary>"A" button.</summary>
			A = 13,
			/// <summary>"B" button.</summary>
			B = 14,
			/// <summary>"X" button.</summary>
			X = 15,
			/// <summary>"Y" button.</summary>
			Y = 16,
			/// <summary>The left thumbstick is moved up.</summary>
			LeftThumbUp,
			/// <summary>The left thumbstick is moved down.</summary>
			LeftThumbDown,
			/// <summary>The left thumbstick is moved left.</summary>
			LeftThumbLeft,
			/// <summary>The left thumbstick is moved right.</summary>
			LeftThumbRight,
			/// <summary>The right thumbstick is moved up.</summary>
			RightThumbUp,
			/// <summary>The right thumbstick is moved down.</summary>
			RightThumbDown,
			/// <summary>The right thumbstick is moved left.</summary>
			RightThumbLeft,
			/// <summary>The right thumbstick is moved right.</summary>
			RightThumbRight,
			/// <summart>The left trigger is pulled.
			LeftTrigger,
			/// <summart>The right trigger is pulled.
			RightTrigger,
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the XInput <see cref="Controller"/> that this source retrieves input from.
		/// </summary>
		public Controller Controller { get; private set; }

		/// <summary>
		/// Gets the <see cref="UserIndex"/> of the controller.
		/// </summary>
		public UserIndex UserIndex { get; private set;}

		#endregion

		#region Fields

		private State State;

		#endregion

		public XInputSource(InputManager manager, UserIndex userIndex)
			: base(manager) {
			this.UserIndex = userIndex;
			this.Controller = new Controller(this.UserIndex);
		}

		#region Methods

		private void AddAxisEvent(List<KeyValuePair<InputTrigger, bool>> events, short oldAxisValue, short newAxisValue, InputTrigger increased, InputTrigger decreased) {
			short Threshold = 10000;
			if (oldAxisValue < +Threshold && newAxisValue > +Threshold) events.Add(new KeyValuePair<InputTrigger, bool>(increased, true));
			if (newAxisValue < +Threshold && oldAxisValue > +Threshold) events.Add(new KeyValuePair<InputTrigger, bool>(increased, false));
			if (oldAxisValue > -Threshold && newAxisValue < -Threshold) events.Add(new KeyValuePair<InputTrigger, bool>(decreased, true));
			if (newAxisValue > -Threshold && oldAxisValue < -Threshold) events.Add(new KeyValuePair<InputTrigger, bool>(decreased, false));
		}

		public KeyValuePair<InputTrigger, bool>[] GetTriggeredEvents() {
			var Events = new List<KeyValuePair<InputTrigger, bool>>();

			var NewState = this.Controller.GetState();
			if (this.State != null) {
				// Button states.
				foreach (GamepadButtonFlags Button in Enum.GetValues(typeof(GamepadButtonFlags))) {
					if ((NewState.Gamepad.Buttons & Button) != (State.Gamepad.Buttons & Button)) {
						Events.Add(new KeyValuePair<InputTrigger, bool>(
							(InputTrigger)Enum.Parse(typeof(InputTrigger), Button.ToString()),
							(NewState.Gamepad.Buttons & Button) != 0
						));
					}
				}
				// Axes.
				AddAxisEvent(Events, State.Gamepad.LeftThumbX, NewState.Gamepad.LeftThumbX, InputTrigger.LeftThumbRight, InputTrigger.LeftThumbLeft);
				AddAxisEvent(Events, State.Gamepad.LeftThumbY, NewState.Gamepad.LeftThumbY, InputTrigger.LeftThumbUp, InputTrigger.LeftThumbDown);
				AddAxisEvent(Events, State.Gamepad.RightThumbX, NewState.Gamepad.RightThumbX, InputTrigger.RightThumbRight, InputTrigger.RightThumbLeft);
				AddAxisEvent(Events, State.Gamepad.RightThumbY, NewState.Gamepad.RightThumbY, InputTrigger.RightThumbUp, InputTrigger.RightThumbDown);

				// Triggers.
				if (State.Gamepad.LeftTrigger < 128 && NewState.Gamepad.LeftTrigger > 127) Events.Add(new KeyValuePair<InputTrigger, bool>(InputTrigger.LeftTrigger, true));
				if (State.Gamepad.LeftTrigger > 127 && NewState.Gamepad.LeftTrigger < 128) Events.Add(new KeyValuePair<InputTrigger, bool>(InputTrigger.LeftTrigger, false));
				if (State.Gamepad.RightTrigger < 128 && NewState.Gamepad.RightTrigger > 127) Events.Add(new KeyValuePair<InputTrigger, bool>(InputTrigger.RightTrigger, true));
				if (State.Gamepad.RightTrigger > 127 && NewState.Gamepad.RightTrigger < 128) Events.Add(new KeyValuePair<InputTrigger, bool>(InputTrigger.RightTrigger, false));
			}
			this.State = NewState;

			return Events.ToArray();
		}

		public override void Poll() {
			var State = this.Controller.GetState();
			foreach (var MappedButton in this.KeyMapping) {
				bool Triggered = false;
				if (MappedButton.Key >= InputTrigger.DPadUp && MappedButton.Key <= InputTrigger.Y) {
					Triggered = ((int)State.Gamepad.Buttons & (1 << (((int)MappedButton.Key) - 1))) != 0;
				} else if (MappedButton.Key >= InputTrigger.LeftThumbUp && MappedButton.Key <= InputTrigger.RightThumbRight) {
					short Threshold = 10000;
					switch (MappedButton.Key) {
						case InputTrigger.LeftThumbLeft: Triggered = State.Gamepad.LeftThumbX < -Threshold; break;
						case InputTrigger.LeftThumbRight: Triggered = State.Gamepad.LeftThumbX > +Threshold; break;
						case InputTrigger.LeftThumbUp: Triggered = State.Gamepad.LeftThumbY > +Threshold; break;
						case InputTrigger.LeftThumbDown: Triggered = State.Gamepad.LeftThumbY < -Threshold; break;
						case InputTrigger.RightThumbLeft: Triggered = State.Gamepad.RightThumbX < -Threshold; break;
						case InputTrigger.RightThumbRight: Triggered = State.Gamepad.RightThumbX > +Threshold; break;
						case InputTrigger.RightThumbUp: Triggered = State.Gamepad.RightThumbY > +Threshold; break;
						case InputTrigger.RightThumbDown: Triggered = State.Gamepad.RightThumbY < -Threshold; break;
					}
				} else if (MappedButton.Key == InputTrigger.LeftTrigger) {
					Triggered = State.Gamepad.LeftTrigger > 127;
				} else if (MappedButton.Key == InputTrigger.RightTrigger) {
					Triggered = State.Gamepad.RightTrigger > 127;
				}
				foreach (var TargetButton in MappedButton.Value) {
					this.CurrentStates[TargetButton.Key][TargetButton.Value] = Triggered;
				}
			}
		}

		#endregion

		public override string SettingsFilename {
			get { return string.Format("XInput.{0}.config", this.UserIndex); }
		}
	}

	#endregion



}
