using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CogwheelSlimDX {
	public partial class ControlEditor : Form {

		Joysticks Sticks;

		public ControlEditor() {
			InitializeComponent();

			this.TabIcons.Images.Add(Properties.Resources.Icon_Keyboard);
			this.TabIcons.Images.Add(Properties.Resources.Icon_Joystick);

			this.Player1TL.Key = Properties.Settings.Default.KeyP1TL;
			this.Player1TR.Key = Properties.Settings.Default.KeyP1TR;
			this.Player1Up.Key = Properties.Settings.Default.KeyP1Up;
			this.Player1Down.Key = Properties.Settings.Default.KeyP1Down;
			this.Player1Left.Key = Properties.Settings.Default.KeyP1Left;
			this.Player1Right.Key = Properties.Settings.Default.KeyP1Right;

			this.Player2TL.Key = Properties.Settings.Default.KeyP2TL;
			this.Player2TR.Key = Properties.Settings.Default.KeyP2TR;
			this.Player2Up.Key = Properties.Settings.Default.KeyP2Up;
			this.Player2Down.Key = Properties.Settings.Default.KeyP2Down;
			this.Player2Left.Key = Properties.Settings.Default.KeyP2Left;
			this.Player2Right.Key = Properties.Settings.Default.KeyP2Right;

			this.ConsolePause.Key = Properties.Settings.Default.KeyPause;
			this.ConsoleReset.Key = Properties.Settings.Default.KeyReset;
			this.ConsoleStart.Key = Properties.Settings.Default.KeyStart;

			this.Sticks = new Joysticks();

			foreach (var Stick in this.Sticks.Items) {
				var JoystickPage = new TabPage() {
					Text = string.Format(Stick.Name),
					ImageIndex = 1,
				};
				JoystickPage.Controls.Add(new Label() {
					Text = "Joystick support is not yet implemented.", 
					Anchor = AnchorStyles.None,
					TextAlign = ContentAlignment.MiddleCenter,
					AutoSize = false,
					Dock = DockStyle.Fill,
				});
				this.InputTabs.TabPages.Add(JoystickPage);
			}
		}

		protected override void OnClosing(CancelEventArgs e) {

			if (this.DialogResult == DialogResult.OK) {

				Properties.Settings.Default.KeyP1TL = this.Player1TL.Key;
				Properties.Settings.Default.KeyP1TR = this.Player1TR.Key;
				Properties.Settings.Default.KeyP1Up = this.Player1Up.Key;
				Properties.Settings.Default.KeyP1Down = this.Player1Down.Key;
				Properties.Settings.Default.KeyP1Left = this.Player1Left.Key;
				Properties.Settings.Default.KeyP1Right = this.Player1Right.Key;

				Properties.Settings.Default.KeyP2TL = this.Player2TL.Key;
				Properties.Settings.Default.KeyP2TR = this.Player2TR.Key;
				Properties.Settings.Default.KeyP2Up = this.Player2Up.Key;
				Properties.Settings.Default.KeyP2Down = this.Player2Down.Key;
				Properties.Settings.Default.KeyP2Left = this.Player2Left.Key;
				Properties.Settings.Default.KeyP2Right = this.Player2Right.Key;

				Properties.Settings.Default.KeyPause = this.ConsolePause.Key;
				Properties.Settings.Default.KeyReset = this.ConsoleReset.Key;
				Properties.Settings.Default.KeyStart = this.ConsoleStart.Key;

			}

			base.OnClosing(e);
		}

	}
}
