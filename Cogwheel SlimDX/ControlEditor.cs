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

		private List<KeyButton> KeyButtons;

		private class ComboBoxItem {
			public string Name { get; set; }
			public object Tag { get; set; }
			public override string ToString() { return Name; }
		}

		public ControlEditor(InputManager manager) {

			InitializeComponent();

			this.Manager = manager;

			this.TabIcons.Images.Add(Properties.Resources.Icon_Keyboard);
			this.TabIcons.Images.Add(Properties.Resources.Icon_Joystick);

			foreach (var Source in this.Manager.Sources) {
				if (Source is KeyboardInputSource) {
					this.KeyboardTab.Tag = Source;
				}
			}

			// Gather up all of the config buttons
			this.KeyButtons = new List<KeyButton>();
			foreach (var Item in this.JoypadTable.Controls) if (Item is KeyButton) KeyButtons.Add((KeyButton)Item);
			foreach (var Item in this.ConsoleTable.Controls) if (Item is KeyButton) KeyButtons.Add((KeyButton)Item);
			foreach (var Item in this.ColecoVisionTable.Controls) if (Item is KeyButton) KeyButtons.Add((KeyButton)Item);

			// Now, iterate over each and attach a settings-changed event handler.
			foreach (var Button in this.KeyButtons) {
				Button.SettingChanged += (sender, e) => {
					var CurrentInputSource = this.InputTabs.SelectedTab.Tag as IInputSource;
					var Sender = sender as KeyButton;
					if (CurrentInputSource != null && Sender != null) {
						if (CurrentInputSource is KeyboardInputSource) {
							CurrentInputSource.SetTrigger(Sender.ControllerIndex, Sender.InputButton, Sender.Key);
						}
					}
				};
			}

			this.ControllerEditing.Items.Add(new ComboBoxItem(){ Name = "Standard Controls", Tag = this.StandardConfigurationPanel });
			this.ControllerEditing.Items.Add(new ComboBoxItem() { Name = "ColecoVision Number Pads", Tag = this.ColecoVisionConfigurationPanel });

			this.ControllerEditing.SelectedItem = this.ControllerEditing.Items[0];

			this.SetKeyButtonValues();
		}

		private void SetKeyButtonValues() {
			var CurrentInputSource = this.InputTabs.SelectedTab.Tag as IInputSource;
			if (CurrentInputSource != null) {
				foreach (var KeyButton in this.KeyButtons) {
					if (CurrentInputSource is KeyboardInputSource) {
						KeyButton.Key = (Keys)CurrentInputSource.GetTrigger(KeyButton.ControllerIndex, KeyButton.InputButton);
					}
				}
			}
		}

		protected override void OnClosing(CancelEventArgs e) {
			this.Manager.UpdateSettings(); // Commit changes.
			base.OnClosing(e);
		}

		private void ControllerEditing_SelectedIndexChanged(object sender, EventArgs e) {
			if (ControllerEditing.SelectedItem != null) ((Control)((ComboBoxItem)ControllerEditing.SelectedItem).Tag).BringToFront();
		}

	}
}
