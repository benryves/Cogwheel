using System.Collections.Generic;
using System.Windows.Forms;
using BeeDevelopment.Sega8Bit;
using CogwheelSlimDX.JoystickInput;
using System.IO;
using System;
using System.Configuration;
using System.Xml.Serialization;

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

		/// <summary>
		/// Creates an instance of <see cref="InputManager"/>.
		/// </summary>
		public InputManager() {
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

		}

		public void Poll() { foreach (var Source in this.Sources) Source.Poll(); }

		public void ReloadSettings() { foreach (var Source in this.Sources) Source.ReloadSettings(); }
		public void UpdateSettings() { foreach (var Source in this.Sources) Source.UpdateSettings(); }

		public void ReleaseAll() { foreach (var Source in this.Sources) Source.ReleaseAll(); }

		public void SetTrigger(int controllerIndex, InputButton button, object trigger) { }

		public object GetTrigger(int controllerIndex, InputButton button) { return null; }

		#endregion
	}

	#region Keyboard

	/// <summary>
	/// Retrieves input from a keyboard.
	/// </summary>
	public class KeyboardInputSource : IInputSource {

		private Dictionary<Keys, KeyValuePair<int, InputButton>[]> KeyMapping;
		private Dictionary<InputButton, bool>[] CurrentKeyStates;

		#region Input Source Methods

		/// <summary>
		/// Do nothing (keyboards aren't polled).
		/// </summary>
		public void Poll() { }

		/// <summary>
		/// Loads all settings from the settings file.
		/// </summary>
		public void ReloadSettings() {
			this.KeyMapping.Clear();

			// Check if the default mapping exists. If not, dump in settings from the project resources.
			if (!File.Exists(this.SettingsPath)) {
				try {
					File.WriteAllText(this.SettingsPath, Properties.Resources.Config_DefaultKeyMapping);
				} catch { }
			}

			if (File.Exists(this.SettingsPath)) {
				foreach (var ConfigLine in File.ReadAllLines(this.SettingsPath)) {

					// Skip comments.
					if (ConfigLine.TrimStart().StartsWith(";")) continue; 

					// Split into two halves -- Key=>Mapping
					var KeyComponents = ConfigLine.Split(new[] { "=>" }, StringSplitOptions.RemoveEmptyEntries);

					// Check that it's a valid pair, otherwise skip.
					if (KeyComponents.Length != 2) continue;

					// Quick-and-dirty conversion.
					try {
						Keys MappedKey = (Keys)Enum.Parse(typeof(Keys), KeyComponents[0]);
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
						string.Join(";", Array.ConvertAll(Mapping.Value, C=>string.Format("{0}.{1}", C.Key,C.Value))))
				);
			}
			File.WriteAllLines(this.SettingsPath, OutputLines.ToArray());
		}

		public bool GetButtonState(int controllerIndex, InputButton button) {
			if (controllerIndex < 0 || controllerIndex > 1) return false;
			bool Result = false;
			return this.CurrentKeyStates[controllerIndex].TryGetValue(button, out Result) ? Result : false;
		}

		public void ReleaseAll() {
			this.CurrentKeyStates = new Dictionary<InputButton, bool>[2];
			for (int Player = 0; Player < 2; ++Player) {
				this.CurrentKeyStates[Player] = new Dictionary<InputButton, bool>();
				foreach (InputButton Button in Enum.GetValues(typeof(InputButton))) {
					this.CurrentKeyStates[Player].Add(Button, false);
				}
			}
		}

		public void SetTrigger(int controllerIndex, InputButton button, object trigger) {
			if (trigger is Keys) {
				Keys Trigger = (Keys)trigger;
				
				// Remove any existing matching trigger.
				var NewMapping = new Dictionary<Keys, KeyValuePair<int, InputButton>[]>();
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
				if (Trigger != Keys.None) {
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
			return Keys.None;
		}

		#endregion

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
					this.CurrentKeyStates[MappedButton.Key][MappedButton.Value] = state;
				}
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Gets the full path to the settings file.
		/// </summary>
		private string SettingsPath {
			get {
				string DirectoryPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Application.CompanyName), "Cogwheel");
				if (!Directory.Exists(DirectoryPath)) Directory.CreateDirectory(DirectoryPath);
				return Path.Combine(DirectoryPath, "Keyboard.config");
			}
		}

		#endregion

		public KeyboardInputSource() {
			this.KeyMapping = new Dictionary<Keys, KeyValuePair<int, InputButton>[]>(32);
			this.ReleaseAll();
			
		}

	}
	
	#endregion

}
