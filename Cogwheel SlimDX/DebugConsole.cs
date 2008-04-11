using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using BeeDevelopment.Sega8Bit;

namespace CogwheelSlimDX {
	public partial class DebugConsole : Form {

		private Bitmap RenderedConsole;
		private int[] RenderedConsoleData;
		private bool[,] FontData;

		private const int BufferWidth = 80;
		private const int BufferHeight = 25;
		private const int FontCharWidth = 6;
		private const int FontCharHeight = 8;
		private const int BitmapWidth = BufferWidth * FontCharWidth;
		private const int BitmapHeight = BufferHeight * FontCharHeight;

		private int[] ColourTranslationTable;

		public DebugConsole() {
			
			InitializeComponent();

			this.ClientSize = new Size(BitmapWidth, BitmapHeight);
			this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
			
			this.RenderedConsole = new Bitmap(BitmapWidth, BitmapHeight);
			this.Disposed += (sender, e) => this.RenderedConsole.Dispose();

			this.RenderedConsoleData = new int[BitmapWidth * BitmapHeight];
			this.FontData = new bool[256 * FontCharHeight, FontCharWidth];

			using (var FontImage = Properties.Resources.Image_Font) {
				var LockedFont = FontImage.LockBits(new Rectangle(0, 0, FontImage.Width, FontImage.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
				int[] TempFont = new int[96 * 128];
				Marshal.Copy(LockedFont.Scan0, TempFont, 0, TempFont.Length);
				FontImage.UnlockBits(LockedFont);
				for (int c = 0; c < 16; ++c) {
					for (int r = 0; r < 16; ++r) {
						for (int y = 0; y < FontCharHeight; ++y) {
							for (int x = 0; x < FontCharWidth; ++x) {
								FontData[FontCharHeight * (r * 16 + c) + y, x] = (TempFont[(c * 6 + x) + (r * 8 + y) * 96] & 0xFFFFFF) == 0;
							}
						}
					}
				}
			}
			
			this.ColourTranslationTable = new int[16];
			for (int i = 0; i < 16; ++i) {
				int b = (i & 1);
				int g = (i & 2) >> 1;
				int r = (i & 4) >> 2;
				for (int j = 0; j < 4; ++j) {
					b |= (b << 1);
					g |= (g << 1);
					r |= (r << 1);
				}
				if ((i & 8) != 0) {
					b |= (b << 4);
					g |= (g << 4);
					r |= (r << 4);
				}
				ColourTranslationTable[i] = (r << 16) | (g << 8) | (b);
			}

			this.UpdateRenderedConsole();
		}

		private void UpdateRenderedConsole() {
			if (this.Owner as MainForm != null) {
				Emulator E = (this.Owner as MainForm).Emulator;
				for (int r = 0; r < BufferHeight; ++r) {
					for (int c = 0; c < BufferWidth; ++c) {
						
						var Character = E.DebugConsole.OutputBuffer[c, r];
						for (int y = 0; y < FontCharHeight; ++y) {
							int Destination = (c * FontCharWidth) + (((r * FontCharHeight) + y) * BitmapWidth);
							for (int x = 0; x < 6; ++x) {
								int Colour = this.FontData[(Character.Character & 0xFF) * 8 + y, x] ? ColourTranslationTable[(int)Character.Colour.Foreground] : ColourTranslationTable[(int)Character.Colour.Background];
								this.RenderedConsoleData[Destination + x] = Colour;
							}
						}
					}
				}
				var Locked = this.RenderedConsole.LockBits(new Rectangle(0, 0, BitmapWidth, BitmapHeight), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
				Marshal.Copy(this.RenderedConsoleData, 0, Locked.Scan0, this.RenderedConsoleData.Length);
				this.RenderedConsole.UnlockBits(Locked);
			}
		}

		protected override void OnPaint(PaintEventArgs e) {
			e.Graphics.Clear(Color.Black);
			e.Graphics.DrawImageUnscaled(this.RenderedConsole, 0, 0);
		}

		private void Refresher_Tick(object sender, EventArgs e) {
			this.UpdateRenderedConsole();
			this.Invalidate();
		}
	}
}
