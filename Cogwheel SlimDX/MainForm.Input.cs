using System;
using System.Windows.Forms;

namespace BeeDevelopment.Cogwheel {
	public partial class MainForm {

		private void ReinitialiseInput(string profileDirectory) {
			this.Input = new InputManager(profileDirectory);
			this.KeyboardInput = new KeyboardInputSource(Input);
			this.Input.Sources.Add(this.KeyboardInput);

			for (int i = 0; i < 4; i++) {
				var C = new SlimDX.XInput.Controller((SlimDX.XInput.UserIndex)i);
				if (C.IsConnected) this.Input.Sources.Add(new XInputSource(this.Input, (SlimDX.XInput.UserIndex)i));
			}

			this.Input.Sources.AddRange(Array.ConvertAll(new JoystickInput.JoystickCollection(Properties.Settings.Default.InputSkipDuplicatedXInputJoysticks).Joysticks, J => new JoystickInputSource(Input, J)));
			this.Input.ReloadSettings();

			if (this.Emulator != null && this.Emulator.HasPS2Keyboard) {
				// Re-attach keyboard handler.
				this.Emulator.PS2Keyboard.StatusLedsChanged -= PS2Keyboard_StatusLedsChanged;
				this.Emulator.PS2Keyboard.StatusLedsChanged += PS2Keyboard_StatusLedsChanged;
				this.StatusNumLock.Visible = this.StatusCapsLock.Visible = this.StatusScrollLock.Visible = true;
				this.PS2Keyboard_StatusLedsChanged(null, new EventArgs());
			} else {
				this.StatusNumLock.Visible = this.StatusCapsLock.Visible = this.StatusScrollLock.Visible = false;
			}
		}

		private void PS2Keyboard_StatusLedsChanged(object sender, EventArgs e) {
			this.StatusNumLock.Enabled = this.Emulator != null && this.Emulator.HasPS2Keyboard && this.Emulator.PS2Keyboard.NumLock;
			this.StatusCapsLock.Enabled = this.Emulator != null && this.Emulator.HasPS2Keyboard && this.Emulator.PS2Keyboard.CapsLock;
			this.StatusScrollLock.Enabled = this.Emulator != null && this.Emulator.HasPS2Keyboard && this.Emulator.PS2Keyboard.ScrollLock;
		}

		protected override void OnKeyDown(KeyEventArgs e) {
			this.KeyboardInput.KeyChange(e, true);
		}

		protected override void OnKeyUp(KeyEventArgs e) {
			this.KeyboardInput.KeyChange(e, false);
		}

		protected override bool IsInputKey(Keys keyData) {
			return this.KeyboardInput.IsInputKey(keyData);
		}

		// Process window messages to check for Alt+Space (ie, window menu) problems.
		protected override void WndProc(ref Message m) {
			switch (m.Msg) {
				case 0x112: // WM_SYSCOMMAND
					switch ((int)m.WParam & 0xFFF0) {
						case 0xF100: // SC_KEYMENU
							m.Result = IntPtr.Zero;
							break;
						default:
							base.WndProc(ref m);
							break;
					}
					break;
				default:
					base.WndProc(ref m);
					break;
			}
		}
	}
}
