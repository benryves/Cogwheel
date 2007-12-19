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
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using BeeDevelopment.Sega8Bit.Devices;

namespace BeeDevelopment.Cogwheel {
	public partial class Sega8BitHost : UserControl {

		private int[] LastBuffer;
		private Bitmap BackBuffer;
		private Size BackBufferResolution;

		private Emulator emulator;
		public Emulator Emulator {
			get { return this.emulator; }
			set {
				

				this.emulator = value;
				if (this.emulator != null) {

					this.emulator.VideoProcessor.GenerateOutputBitmap = false;
					this.emulator.VideoProcessor.VerticalBlank += (sender, e) => { LastBuffer = e.Pixels; this.Invalidate(); };
					this.emulator.VideoProcessor.ResolutionChange += new BeeDevelopment.Sega8Bit.Devices.VideoDisplayProcessor.ResolutionChangeEventHandler(VideoProcessor_ResolutionChange);
				}
				this.VideoProcessor_ResolutionChange(null, new VideoDisplayProcessor.ResolutionChangeEventArgs(new Size(256, 192)));
			}
		}

		void VideoProcessor_ResolutionChange(object sender, BeeDevelopment.Sega8Bit.Devices.VideoDisplayProcessor.ResolutionChangeEventArgs e) {
			if (this.BackBuffer != null) this.BackBuffer.Dispose();
			this.BackBufferResolution = e.Resolution;
			this.BackBuffer = new Bitmap(e.Resolution.Width, e.Resolution.Height);
		}

		public Sega8BitHost() {
			InitializeComponent();
			this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
		}

		protected override void OnPaint(PaintEventArgs e) {
			if (this.emulator == null || this.BackBuffer == null) {
				e.Graphics.Clear(Color.FromKnownColor(KnownColor.Control));
			} else {
				
				e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
				e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;

				lock (this.BackBuffer) {
					var BD = this.BackBuffer.LockBits(new Rectangle(0, 0, this.BackBufferResolution.Width, this.BackBufferResolution.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
					Marshal.Copy(this.LastBuffer, 0, BD.Scan0, (BD.Stride * BD.Height) / 4);
					this.BackBuffer.UnlockBits(BD);
				}

				e.Graphics.DrawImage(this.BackBuffer, this.ClientRectangle);
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
