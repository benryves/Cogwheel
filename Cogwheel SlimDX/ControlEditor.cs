using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CogwheelSlimDX.JoystickInput;

namespace CogwheelSlimDX {
	partial class ControlEditor : Form {

		InputManager Manager;

		public ControlEditor(InputManager manager) {

			InitializeComponent();

			this.Manager = manager;

			this.TabIcons.Images.Add(Properties.Resources.Icon_Keyboard);
			this.TabIcons.Images.Add(Properties.Resources.Icon_Joystick);

			foreach (var Source in this.Manager.Sources) {
				if (Source is JoystickInputSource) {
					var Stick = (JoystickInputSource)Source;
					var JoystickPage = new TabPage() {
						Text = Stick.Joystick.Name,
						ImageIndex = 1,
						Tag = Stick,
						Padding = new Padding(3),
						UseVisualStyleBackColor = true,
					};
					this.InputTabs.TabPages.Add(JoystickPage);
				} else if (Source is KeyboardInputSource) {
					this.KeyboardTab.Tag = Source;
				}
			}

			this.InputTabs_SelectedIndexChanged(null, null);
			this.AttachSettingsChangedEventHandler(this.ConfigurationPanel);
		}

		protected override void OnClosing(CancelEventArgs e) {
			this.Manager.UpdateSettings(); // Commit changes.
			base.OnClosing(e);
		}

		private void AttachSettingsChangedEventHandler(Control root) {
			if (root is KeyButton) (root as KeyButton).SettingChanged += new EventHandler(ControlEditor_SettingChanged);
			foreach (Control SubControl in root.Controls) AttachSettingsChangedEventHandler(SubControl);
		}

		void ControlEditor_SettingChanged(object sender, EventArgs e) {
			KeyButton KeySender = (sender as KeyButton);
			if (KeySender == null) return;
			if (this.InputTabs.SelectedTab.Tag is KeyboardInputSource) {
				var Source = this.InputTabs.SelectedTab.Tag as KeyboardInputSource;
				if (sender == this.Player1TL) Source.KeyPlayer1TL = KeySender.Key;
				if (sender == this.Player1TR) Source.KeyPlayer1TR = KeySender.Key;
				if (sender == this.Player1Up) Source.KeyPlayer1Up = KeySender.Key;
				if (sender == this.Player1Down) Source.KeyPlayer1Down = KeySender.Key;
				if (sender == this.Player1Left) Source.KeyPlayer1Left = KeySender.Key;
				if (sender == this.Player1Right) Source.KeyPlayer1Right = KeySender.Key;
				if (sender == this.Player2TL) Source.KeyPlayer2TL = KeySender.Key;
				if (sender == this.Player2TR) Source.KeyPlayer2TR = KeySender.Key;
				if (sender == this.Player2Up) Source.KeyPlayer2Up = KeySender.Key;
				if (sender == this.Player2Down) Source.KeyPlayer2Down = KeySender.Key;
				if (sender == this.Player2Left) Source.KeyPlayer2Left = KeySender.Key;
				if (sender == this.Player2Right) Source.KeyPlayer2Right = KeySender.Key;
				if (sender == this.ConsolePause) Source.KeyPause = KeySender.Key;
				if (sender == this.ConsoleReset) Source.KeyReset = KeySender.Key;
				if (sender == this.ConsoleStart) Source.KeyStart = KeySender.Key;
			} else if (this.InputTabs.SelectedTab.Tag is JoystickInputSource) {
				var Source = this.InputTabs.SelectedTab.Tag as JoystickInputSource;
				if (sender == this.Player1TL) Source.Mapping.TriggerPlayer1TL = KeySender.JoystickTrigger;
				if (sender == this.Player1TR) Source.Mapping.TriggerPlayer1TR = KeySender.JoystickTrigger;
				if (sender == this.Player1Up) Source.Mapping.TriggerPlayer1Up = KeySender.JoystickTrigger;
				if (sender == this.Player1Down) Source.Mapping.TriggerPlayer1Down = KeySender.JoystickTrigger;
				if (sender == this.Player1Left) Source.Mapping.TriggerPlayer1Left = KeySender.JoystickTrigger;
				if (sender == this.Player1Right) Source.Mapping.TriggerPlayer1Right = KeySender.JoystickTrigger;
				if (sender == this.Player2TL) Source.Mapping.TriggerPlayer2TL = KeySender.JoystickTrigger;
				if (sender == this.Player2TR) Source.Mapping.TriggerPlayer2TR = KeySender.JoystickTrigger;
				if (sender == this.Player2Up) Source.Mapping.TriggerPlayer2Up = KeySender.JoystickTrigger;
				if (sender == this.Player2Down) Source.Mapping.TriggerPlayer2Down = KeySender.JoystickTrigger;
				if (sender == this.Player2Left) Source.Mapping.TriggerPlayer2Left = KeySender.JoystickTrigger;
				if (sender == this.Player2Right) Source.Mapping.TriggerPlayer2Right = KeySender.JoystickTrigger;
				if (sender == this.ConsolePause) Source.Mapping.TriggerPause = KeySender.JoystickTrigger;
				if (sender == this.ConsoleReset) Source.Mapping.TriggerReset = KeySender.JoystickTrigger;
				if (sender == this.ConsoleStart) Source.Mapping.TriggerStart = KeySender.JoystickTrigger;
			}
		}

		private void InputTabs_SelectedIndexChanged(object sender, EventArgs e) {
			this.InputTabs.SelectedTab.Controls.Clear();
			this.InputTabs.SelectedTab.Controls.Add(this.ConfigurationPanel);
			UpdateButtons(this.ConfigurationPanel);
			if (this.InputTabs.SelectedTab.Tag is KeyboardInputSource) {
				var Source = (KeyboardInputSource)this.InputTabs.SelectedTab.Tag;
				this.Player1TL.Key = Source.KeyPlayer1TL;
				this.Player1TR.Key = Source.KeyPlayer1TR;
				this.Player1Up.Key = Source.KeyPlayer1Up;
				this.Player1Down.Key = Source.KeyPlayer1Down;
				this.Player1Left.Key = Source.KeyPlayer1Left;
				this.Player1Right.Key = Source.KeyPlayer1Right;
				this.Player2TL.Key = Source.KeyPlayer2TL;
				this.Player2TR.Key = Source.KeyPlayer2TR;
				this.Player2Up.Key = Source.KeyPlayer2Up;
				this.Player2Down.Key = Source.KeyPlayer2Down;
				this.Player2Left.Key = Source.KeyPlayer2Left;
				this.Player2Right.Key = Source.KeyPlayer2Right;
				this.ConsolePause.Key = Source.KeyPause;
				this.ConsoleReset.Key = Source.KeyReset;
				this.ConsoleStart.Key = Source.KeyStart;
			} else if (this.InputTabs.SelectedTab.Tag is JoystickInputSource) {
				var Source = (JoystickInputSource)this.InputTabs.SelectedTab.Tag;
				this.Player1TL.JoystickTrigger = Source.Mapping.TriggerPlayer1TL;
				this.Player1TR.JoystickTrigger = Source.Mapping.TriggerPlayer1TR;
				this.Player1Up.JoystickTrigger = Source.Mapping.TriggerPlayer1Up;
				this.Player1Down.JoystickTrigger = Source.Mapping.TriggerPlayer1Down;
				this.Player1Left.JoystickTrigger = Source.Mapping.TriggerPlayer1Left;
				this.Player1Right.JoystickTrigger = Source.Mapping.TriggerPlayer1Right;
				this.Player2TL.JoystickTrigger = Source.Mapping.TriggerPlayer2TL;
				this.Player2TR.JoystickTrigger = Source.Mapping.TriggerPlayer2TR;
				this.Player2Up.JoystickTrigger = Source.Mapping.TriggerPlayer2Up;
				this.Player2Down.JoystickTrigger = Source.Mapping.TriggerPlayer2Down;
				this.Player2Left.JoystickTrigger = Source.Mapping.TriggerPlayer2Left;
				this.Player2Right.JoystickTrigger = Source.Mapping.TriggerPlayer2Right;
				this.ConsolePause.JoystickTrigger = Source.Mapping.TriggerPause;
				this.ConsoleReset.JoystickTrigger = Source.Mapping.TriggerReset;
				this.ConsoleStart.JoystickTrigger = Source.Mapping.TriggerStart;
			}
		}

		private void UpdateButtons(Control root) {
			if (root is KeyButton) {
				if (this.InputTabs.SelectedTab.Tag is JoystickInputSource) {
					(root as KeyButton).Mode = KeyButton.InputMode.Joystick;
					(root as KeyButton).JoystickSource = (JoystickInputSource)this.InputTabs.SelectedTab.Tag;
				} else {
					(root as KeyButton).Mode = KeyButton.InputMode.Keyboard;
				}
				
			}
			foreach (Control SubControl in root.Controls) UpdateButtons(SubControl);
		}
	}
}
