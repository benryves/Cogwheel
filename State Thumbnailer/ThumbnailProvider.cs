using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace BeeDevelopment.Cogwheel.State.Thumbnailer {
	
	[ComVisible(true), ClassInterface(ClassInterfaceType.None)]
	[ProgId("Cogwheel.StateThumbnailProvider"), Guid("f5d20abb-95b3-4a4c-8b60-b3df9d872a63")]
	public class ThumbnailProvider : IThumbnailProvider, IInitializeWithStream {

		#region IInitializeWithStream

		protected IStream Stream { get; set; }

		public void Initialize(IStream stream, int grfMode) {
			this.Stream = stream;
		}

		protected byte[] GetStreamContents() {

			if (this.Stream == null) return null;

			System.Runtime.InteropServices.ComTypes.STATSTG statData;
			this.Stream.Stat(out statData, 1);

			byte[] Result = new byte[statData.cbSize];

			IntPtr P = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(UInt64)));
			try {
				this.Stream.Read(Result, Result.Length, P);
			} finally {
				Marshal.FreeCoTaskMem(P);
			}
			return Result;
		}

		#endregion

		public void GetThumbnail(int squareLength, out IntPtr hBitmap, out WTS_ALPHATYPE bitmapType) {

			hBitmap = IntPtr.Zero;
			bitmapType = WTS_ALPHATYPE.WTSAT_UNKNOWN;

			try {

				Zip.ZipFile StateFile = Zip.ZipFile.FromStream(new MemoryStream(this.GetStreamContents()));

				foreach (var ZipEntry in StateFile) {

					if (ZipEntry.Name.ToLowerInvariant() == "screenshot.png") {
						using (Bitmap ScreenshotSource = new Bitmap(new MemoryStream(ZipEntry.Data))) {
							if (ScreenshotSource.Width <= squareLength && ScreenshotSource.Height <= squareLength) {
								hBitmap = ScreenshotSource.GetHbitmap();
								return;
							} else {
								int ThumbWidth = squareLength, ThumbHeight = squareLength;
								if (ScreenshotSource.Width > ScreenshotSource.Height) {
									ThumbHeight = squareLength * ScreenshotSource.Height / ScreenshotSource.Width;
								} else if (ScreenshotSource.Width < ScreenshotSource.Height) {
									ThumbWidth = squareLength * ScreenshotSource.Width / ScreenshotSource.Height;
								}
								using (var ReducedThumbnail = new Bitmap(ThumbWidth, ThumbHeight)) {
									using (var G = Graphics.FromImage(ReducedThumbnail)) {
										G.PixelOffsetMode = PixelOffsetMode.Half;
										G.DrawImage(ScreenshotSource, 0, 0, ThumbWidth, ThumbHeight);
									}
									hBitmap = ReducedThumbnail.GetHbitmap();
									return;
								}
							}
						}
					}
				}
			} catch { } // Cop-out!
		}
	}
}
