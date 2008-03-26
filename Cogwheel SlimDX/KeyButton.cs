using System.Windows.Forms;
using System;
using System.Drawing;
using System.Collections.Generic;

namespace CogwheelSlimDX {
	class KeyButton : CheckBox {

		private ContextMenu JoystickTriggerOptions;

		public enum InputMode {
			Keyboard,
			Joystick,
		}

		public JoystickInputSource JoystickSource { get; set; }

		public event EventHandler SettingChanged;
		protected virtual void OnSettingChanged(EventArgs e) {
			if (this.SettingChanged != null) this.SettingChanged(this, e);
		}
		

		private InputMode mode = InputMode.Keyboard;
		public InputMode Mode {
			get { return this.mode; }
			set { this.mode = value; this.UpdateText(); }
		}

		private void UpdateText() {
			switch (this.Mode) {
				case InputMode.Keyboard:
					switch (this.key) {
						case Keys.Back:
							base.Text = "Backspace"; break;
						case Keys.Menu:
							base.Text = "Alt"; break;
						case Keys.ControlKey:
							base.Text = "Ctrl"; break;
						case Keys.ShiftKey:
							base.Text = "Shift"; break;
						case Keys.Capital:
							base.Text = "CapsLock"; break;
						default:
							base.Text = Key.ToString();
							break;
					}
					break;
				case InputMode.Joystick:
					base.Text = JoystickTriggerToString(this.JoystickTrigger);
					break;
			}
		}

		private Keys key;
		public Keys Key {
			get { return this.key; }
			set { this.key = value;  this.UpdateText();}
		}

		private JoystickInputSource.InputTrigger trigger;
		public JoystickInputSource.InputTrigger JoystickTrigger {
			get { return this.trigger; }
			set { this.trigger = value; this.UpdateText(); }
		}

		public override string Text {
			get {
				return base.Text;
			}
			set { }
		}

		public KeyButton() {
			this.Key = Keys.None;
			this.JoystickTrigger = JoystickInputSource.InputTrigger.None;
			this.Mode = InputMode.Keyboard;
			this.Appearance = Appearance.Button;
			this.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.AutoCheck = false;
			this.JoystickTriggerOptions = new ContextMenu();
		}

		protected override void OnLostFocus(System.EventArgs e) {
			this.Checked = false;
			base.OnLostFocus(e);
		}

		protected override void OnLeave(EventArgs e) {
			this.Checked = false;
			base.OnLeave(e);
		}

		private static string JoystickTriggerToString(JoystickInputSource.InputTrigger trigger) {
			return trigger.ToString().Replace("Button", "Button ").Replace("Axis", "-Axis").Replace("Increase", " Increase").Replace("Decrease", " Decrease");
		}

		protected override void OnMouseDown(MouseEventArgs mevent) {
			switch (mevent.Button) {
				case MouseButtons.Left:
					
					this.Checked ^= true;

					if (this.Checked && this.Mode == InputMode.Joystick) {

						this.Checked = false;

						var ToDispose = new List<MenuItem>();
						foreach (MenuItem OldItem in this.JoystickTriggerOptions.MenuItems) ToDispose.Add(OldItem);

						foreach (var OldItem in ToDispose) {
							this.JoystickTriggerOptions.MenuItems.Remove(OldItem);
							OldItem.Dispose();
						}

						if (this.JoystickSource != null) {
							foreach (JoystickInputSource.InputTrigger Trigger in Enum.GetValues(typeof(JoystickInputSource.InputTrigger))) {
								if (Trigger ==  JoystickInputSource.InputTrigger.None || JoystickSource.SupportsTrigger(Trigger)) {
									this.JoystickTriggerOptions.MenuItems.Add(new MenuItem(
										JoystickTriggerToString(Trigger),
										(sender, e) => { this.JoystickTrigger = (JoystickInputSource.InputTrigger)((MenuItem)sender).Tag; this.OnSettingChanged(new EventArgs()); }
									) {
										Checked = Trigger == this.JoystickTrigger,
										RadioCheck = true,
										Tag = Trigger
									});
								}
							}
						}

						this.JoystickTriggerOptions.Show(this, new Point(0, this.ClientSize.Height));
					}
					break;
				case MouseButtons.Right:
					this.Checked = false;
					this.Key = Keys.None;
					this.JoystickTrigger = JoystickInputSource.InputTrigger.None;
					this.OnSettingChanged(new EventArgs());
					break;
			}
		}

		protected override bool IsInputKey(Keys keyData) {
			return this.Checked;
		}

		protected override void OnKeyDown(KeyEventArgs e) {
			if (this.Checked) {
				e.Handled = true;
				this.Key = e.KeyCode;
				this.Checked = false;
				this.OnSettingChanged(new EventArgs());
			}
			base.OnKeyDown(e);
		}


	}
}
