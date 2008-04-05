﻿using System;
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
				} else if (Source is JoystickInputSource) {
					var JoystickSource = (JoystickInputSource)Source;
					this.InputTabs.TabPages.Add(new TabPage() {
						Text = JoystickSource.Joystick.Name,
						Tag = JoystickSource,
						ImageIndex = 1,
						UseVisualStyleBackColor = true,
					});
				}
			}

			this.InputTabs.SelectedIndexChanged += (sender, e) => {
				this.JoystickEventPoller.Stop();
				this.InputTabs.SelectedTab.Controls.Add(this.EditorPanel);
				this.SetKeyButtonValues();
				if (this.InputTabs.SelectedTab.Tag is JoystickInputSource) {
					this.JoystickPollCount = 0;
					this.JoystickEventPoller.Start();
				}
			};

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
							CurrentInputSource.SetTrigger(Sender.ControllerIndex, Sender.InputButton, Sender.KeyboardTrigger);
						} else if (CurrentInputSource is JoystickInputSource) {
							CurrentInputSource.SetTrigger(Sender.ControllerIndex, Sender.InputButton, Sender.JoystickTrigger);
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
						KeyButton.KeyboardTrigger = (Keys)CurrentInputSource.GetTrigger(KeyButton.ControllerIndex, KeyButton.InputButton);
						KeyButton.Mode = KeyButton.Modes.Keyboard;
					} else if (CurrentInputSource is JoystickInputSource) {
						KeyButton.JoystickTrigger = (JoystickInputSource.InputTrigger)CurrentInputSource.GetTrigger(KeyButton.ControllerIndex, KeyButton.InputButton);
						KeyButton.Mode = KeyButton.Modes.Joystick;
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

		private int JoystickPollCount = 0;
		private void JoystickEventPoller_Tick(object sender, EventArgs e) {
			if (this.InputTabs.SelectedTab.Tag is JoystickInputSource) {
				var JoystickEventSource = (JoystickInputSource)this.InputTabs.SelectedTab.Tag;
				var Events = JoystickEventSource.GetTriggeredEvents();
				if (JoystickPollCount > 2) {
					foreach (var Event in Events) {
						if (Event.Value) {
							foreach (var PossibleButtonMatch in this.KeyButtons) {
								if (PossibleButtonMatch.Checked) {
									PossibleButtonMatch.JoystickTrigger = Event.Key;
									PossibleButtonMatch.Checked = false;
									JoystickEventSource.SetTrigger(PossibleButtonMatch.ControllerIndex, PossibleButtonMatch.InputButton, PossibleButtonMatch.JoystickTrigger);
								}
							}
						}	
					}					
				}
				++JoystickPollCount;
			}
		}

	}
}
