using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BeeDevelopment.Sega8Bit.Emulation;
using System.Drawing.Drawing2D;
using BeeDevelopment.Sega8Bit.Devices.Input;

namespace BeeDevelopment.Cogwheel {
	public partial class Sega8BitHost : UserControl {

		private Bitmap LastBuffer;

		private Emulator emulator;
		public Emulator Emulator {
			get { return this.emulator; }
			set {
				this.emulator = value;
				if (this.emulator != null) {
					this.emulator.VideoProcessor.GenerateOutputBitmap = true;
					this.emulator.VideoProcessor.VerticalBlank += (sender, e) => { LastBuffer = e.Output; this.Invalidate(); };
				}
			}
		}

		public Sega8BitHost() {
			InitializeComponent();
			this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
		}

		protected override void OnPaint(PaintEventArgs e) {
			if (this.emulator == null || this.LastBuffer == null) {
				e.Graphics.Clear(Color.Black);
			} else {
				e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
				e.Graphics.DrawImage(this.LastBuffer, this.ClientRectangle);
			}
		}


		public Joypad PlayerA { get; set; }
		public Joypad PlayerB { get; set; }

		private Dictionary<Keys, KeyTarget> KeyMapping;


		public void LoadKeyMappings() {
			this.KeyMapping = new Dictionary<Keys, KeyTarget>();
			this.KeyMapping.Add(Properties.Settings.Default.KeyP1Left, new KeyTarget() { Pin = Controller.Pins.Left, PlayerIndex = 1 });
			this.KeyMapping.Add(Properties.Settings.Default.KeyP1Right, new KeyTarget() { Pin = Controller.Pins.Right, PlayerIndex = 1 });
			this.KeyMapping.Add(Properties.Settings.Default.KeyP1Up, new KeyTarget() { Pin = Controller.Pins.Up, PlayerIndex = 1 });
			this.KeyMapping.Add(Properties.Settings.Default.KeyP1Down, new KeyTarget() { Pin = Controller.Pins.Down, PlayerIndex = 1 });
			this.KeyMapping.Add(Properties.Settings.Default.KeyP1B1, new KeyTarget() { Pin = Controller.Pins.TL, PlayerIndex = 1 });
			this.KeyMapping.Add(Properties.Settings.Default.KeyP1B2, new KeyTarget() { Pin = Controller.Pins.TR, PlayerIndex = 1 });
		}


		protected override bool IsInputKey(Keys keyData) {
			if (this.KeyMapping != null && this.KeyMapping.ContainsKey(keyData)) return true;
			return base.IsInputKey(keyData);
		}

		private void SetJoypadState(KeyTarget target, bool state) {
			var ActiveController = target.PlayerIndex == 1 ? PlayerA : PlayerB;
			switch (target.Pin) {
				case Controller.Pins.Up: ActiveController.ButtonUp = state; break;
				case Controller.Pins.Down: ActiveController.ButtonDown = state; break;
				case Controller.Pins.Left: ActiveController.ButtonLeft = state; break;
				case Controller.Pins.Right: ActiveController.ButtonRight = state; break;
				case Controller.Pins.TL: ActiveController.Button1 = state; break;
				case Controller.Pins.TR: ActiveController.Button2 = state; break;
			}
		}

		protected override void OnKeyDown(KeyEventArgs e) {
			KeyTarget Target;
			if (this.KeyMapping != null && this.KeyMapping.TryGetValue(e.KeyCode, out Target)) {
				SetJoypadState(Target, true);
				e.Handled = true;
			}
			base.OnKeyDown(e);
		}

		protected override void OnKeyUp(KeyEventArgs e) {
			KeyTarget Target;
			if (this.KeyMapping != null && this.KeyMapping.TryGetValue(e.KeyCode, out Target)) {
				SetJoypadState(Target, false);
				e.Handled = true;
			}
			base.OnKeyUp(e);
		}

	}
}
