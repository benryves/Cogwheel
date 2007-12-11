using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Drawing.Drawing2D;
using Cogwheel.Devices.Input;

namespace CogwheelGui {
	public partial class CogwheelHost : UserControl {

		public Cogwheel.Emulation.Sega8Bit Emulator { get; private set; }
		
		private DateTime LastEmulated;
		private Bitmap VerticalBlankBitmap;
		private Joypad PlayerAPad;

		public CogwheelHost() {
			InitializeComponent();

			this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
			
			this.Emulator = new Cogwheel.Emulation.Sega8Bit();

			this.Emulator.ControllerPortA = (PlayerAPad = new Joypad(this.Emulator));

			this.Emulator.VDP.GenerateOutputBitmap = true;

			Application.Idle += (sender, e) => {
				Thread.Sleep(2);
				this.Emulator.RunFrame();
			};


			this.Emulator.VDP.VerticalBlank += (sender, e) => {
				this.VerticalBlankBitmap = e.Output;
				this.Invalidate();
			};

			this.Disposed += (sender, e) => this.VerticalBlankBitmap.Dispose();

			

		}

		protected override bool IsInputKey(Keys keyData) {
			switch (keyData) {
				case Keys.Up:
				case Keys.Down:
				case Keys.Left:
				case Keys.Right:
				case Keys.Z:
				case Keys.X:
				case Keys.Space:
					return true;
			}
			return base.IsInputKey(keyData);
		}


		protected override void OnKeyDown(KeyEventArgs e) {
			switch (e.KeyData) {
				case Keys.Up: this.PlayerAPad.ButtonUp = true; break;
				case Keys.Down: this.PlayerAPad.ButtonDown = true; break;
				case Keys.Left: this.PlayerAPad.ButtonLeft = true; break;
				case Keys.Right: this.PlayerAPad.ButtonRight = true; break;
				case Keys.Z: this.PlayerAPad.Button1 = true; break;
				case Keys.X: this.PlayerAPad.Button2 = true; break;
				case Keys.Space: this.Emulator.PinNonMaskableInterrupt = true; break;
			}
			base.OnKeyDown(e);
		}


		protected override void OnKeyUp(KeyEventArgs e) {
			switch (e.KeyData) {
				case Keys.Up: this.PlayerAPad.ButtonUp = false; break;
				case Keys.Down: this.PlayerAPad.ButtonDown = false; break;
				case Keys.Left: this.PlayerAPad.ButtonLeft = false; break;
				case Keys.Right: this.PlayerAPad.ButtonRight = false; break;
				case Keys.Z: this.PlayerAPad.Button1 = false; break;
				case Keys.X: this.PlayerAPad.Button2 = false; break;
				case Keys.Space: this.Emulator.PinNonMaskableInterrupt = false; break;
			}
			base.OnKeyDown(e);
		}

		protected override void OnPaint(PaintEventArgs e) {
			e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
			e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;



			if (this.VerticalBlankBitmap != null) {
				e.Graphics.DrawImage(this.VerticalBlankBitmap, this.ClientRectangle);
			} else {
				e.Graphics.Clear(Color.Gray);
			}
		}
	}
}
