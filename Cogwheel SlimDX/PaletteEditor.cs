using BeeDevelopment.Sega8Bit;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BeeDevelopment.Cogwheel {
	public partial class PaletteEditor : Form {

		private Bitmap previewBitmap;

		public PaletteEditor() {
			InitializeComponent();
			this.Disposed += PaletteEditor_Disposed;
			this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.Opaque | ControlStyles.ResizeRedraw, true);
			this.ClientSize = new Size(512, 64);
		}

		private void PaletteEditor_Disposed(object sender, EventArgs e) {
			if (previewBitmap != null) {
				previewBitmap.Dispose();
				previewBitmap = null;
			}
		}

		public void Refresh(Emulator emulator) {
			// Dump the palette information into the Bitmap used to preview the palette.
			var palette = emulator.Video.Palette;
			if (palette != null) {
				var w = palette.Length;
				var h = 1;
				if (w > 16) {
					w /= 2;
					h *= 2;
				}
				if (this.previewBitmap == null || this.previewBitmap.Width != w || this.previewBitmap.Height != h) {
					if (this.previewBitmap != null) {
						this.previewBitmap.Dispose();
					}
					this.previewBitmap = new Bitmap(w, h, PixelFormat.Format32bppArgb);
				}
				var bd = this.previewBitmap.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
				try {
					Marshal.Copy(palette, 0, bd.Scan0, palette.Length);
				} finally {
					this.previewBitmap.UnlockBits(bd);
				}

			}
			this.Invalidate();
		}

		protected override void OnPaint(PaintEventArgs e) {
			// Repaint the current palette.
			e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
			e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;

			e.Graphics.Clear(Color.White);

			var clientRectangle = this.ClientRectangle;

			// Transparency pattern.
			var greyBrush = Brushes.LightGray;
			for (int y = 0; y < clientRectangle.Height; y += 8) {
				for (int x = 0; x < clientRectangle.Width; x += 8) {
					if (((x ^ y) & 8) != 0) {
						e.Graphics.FillRectangle(greyBrush, x, y, 8, 8);
					}
				}
			}

			// Draw the palette image over the top.
			if (this.previewBitmap != null) {
				e.Graphics.DrawImage(this.previewBitmap, clientRectangle);
			}
		}

		private void PaletteEditor_MouseLeave(object sender, EventArgs e) {
			this.Text = "Palette";
		}

		private void PaletteEditor_MouseMove(object sender, MouseEventArgs e) {
			// Show the palette index of what we're hovering our mouse over.
			var caption = "Palette";
			var index = -1;
			if (this.previewBitmap != null && this.ClientSize.Width > 0 && this.ClientSize.Height > 0 && e.X >= 0 && e.Y>=0 && e.X < this.ClientSize.Width && e.Y < this.ClientSize.Height) {
				int x = (e.X * this.previewBitmap.Width) / this.ClientSize.Width;
				int y = (e.Y * this.previewBitmap.Height) / this.ClientSize.Height;
				index = x + y * this.previewBitmap.Width;
			}
			if (index >= 0) {
				caption += string.Format(": {0}", index);
			}
			if (this.Text != caption) {
				this.Text = caption;
			}
		}
	}
}
