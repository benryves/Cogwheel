using System;
using SlimDX;
using SlimDX.Direct3D9;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Management;
using System.Collections.Generic;

namespace BeeDevelopment.Cogwheel {
	class PixelDumper : IDisposable {

		#region Types

		/// <summary>
		/// Represents a vertex with a 3D position and 2D texture coordinate.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		private struct Vertex {

			/// <summary>
			/// The position in 3D space of the <see cref="Vertex"/>.
			/// </summary>
			public Vector3 Position;

			/// <summary>
			/// The 2D texture coordinate of the <see cref="Vertex"/>.
			/// </summary>
			public Vector2 TextureCoordinate;

			/// <summary>
			/// Gets the size in bytes of the <see cref="Vertex"/> structure.
			/// </summary>
			public static int Size {
				get { return Marshal.SizeOf(typeof(Vertex)); }
			}

			/// <summary>
			/// Gets the <see cref="VertexFormat"/> of the <see cref="Vertex"/> structure.
			/// </summary>
			public static VertexFormat Format {
				get { return VertexFormat.Position | VertexFormat.Texture1; }
			}

		}

		/// <summary>
		/// Defines the way that the image is stretched to fill the control.
		/// </summary>
		public enum ScaleModes {
			/// <summary>The image fills the entire control, ignoring it aspect ratio.</summary>
			Stretch,
			/// <summary>The image is zoomed, retaining its aspect ratio, so that it just fits inside the control (leaving borders).</summary>
			ZoomInside,
			/// <summary>The image is zoomed, retaining its aspect ratio, so that it entirely fills the control, cropping some of itself.</summary>
			ZoomOutside,
		}

		#endregion

		#region Private Fields

		/// <summary>
		/// D3D graphics device.
		/// </summary>
		private Device GraphicsDevice;

		/// <summary>
		/// Vertices used for the quad.
		/// </summary>
		private VertexBuffer Vertices;

		/// <summary>
		/// The video output texture.
		/// </summary>
		private Texture VideoOutput;

		/// <summary>
		/// Current video output width.
		/// </summary>
		private int VideoOutputWidth = 32;

		/// <summary>
		/// Current video output height.
		/// </summary>
		private int VideoOutputHeight = 32;

		/// <summary>
		/// The width of the area that was last cleared to the backdrop colour.
		/// </summary>
		private int LastClearedWidth;

		/// <summary>
		/// The height of the area that was last cleared to the backdrop colour.
		/// </summary>
		private int LastClearedHeight;

		/// <summary>
		/// The colour of the area that was last cleared.
		/// </summary>
		private Color LastClearedColour;


		#endregion

		#region Private Methods

		/// <summary>
		/// Clean up Direct3D resources.
		/// </summary>
		private void DisposeRenderer() {

			if (this.VideoOutput != null && !this.VideoOutput.Disposed) {
				this.VideoOutput.Dispose();
			}

			if (this.Vertices != null && !this.Vertices.Disposed) {
				this.Vertices.Dispose();
			}

			if (this.GraphicsDevice != null && !this.GraphicsDevice.Disposed) {
				this.GraphicsDevice.Dispose();
			}

		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Reinitialises the <see cref="PixelDumper"/>.
		/// </summary>
		public void ReinitialiseRenderer() {

			// Clean up first!
			this.DisposeRenderer();

			try {

				// Set up initial device pararmeters.
				PresentParameters Params = new PresentParameters() {
					BackBufferWidth = Math.Max(1, this.Control.ClientSize.Width),
					BackBufferHeight = Math.Max(1, this.Control.ClientSize.Height),
					DeviceWindowHandle = this.Control.Handle,
					PresentationInterval =  PresentInterval.One,
				};

				// Try and create the device.
				this.GraphicsDevice = new Device(Program.D3D, 0, DeviceType.Hardware, this.Control.Handle, CreateFlags.HardwareVertexProcessing, Params);

				// Create the vertex buffer.
				this.Vertices = new VertexBuffer(this.GraphicsDevice, 6 * Vertex.Size, Usage.WriteOnly, VertexFormat.None, Pool.Managed);
				var VertexStream = Vertices.Lock(0, 0, LockFlags.None);
				VertexStream.WriteRange(
					new[] {
					new Vertex() { Position = new Vector3(-0.5f, -0.5f, 0f), TextureCoordinate = new Vector2(0f, 1f) },
					new Vertex() { Position = new Vector3(+0.5f, -0.5f, 0f), TextureCoordinate = new Vector2(1f, 1f) },
					new Vertex() { Position = new Vector3(-0.5f, +0.5f, 0f), TextureCoordinate = new Vector2(0f, 0f) },
					new Vertex() { Position = new Vector3(-0.5f, +0.5f, 0f), TextureCoordinate = new Vector2(0f, 0f) },
					new Vertex() { Position = new Vector3(+0.5f, -0.5f, 0f), TextureCoordinate = new Vector2(1f, 1f) },
					new Vertex() { Position = new Vector3(+0.5f, +0.5f, 0f), TextureCoordinate = new Vector2(1f, 0f) }
				}
				);
				this.Vertices.Unlock();

				// Create a texture to write to to create video ouptut.
				this.VideoOutput = new Texture(this.GraphicsDevice, this.VideoOutputWidth, this.VideoOutputHeight, 0, Usage.None, Format.A8R8G8B8, Pool.Managed);

				this.LastClearedHeight = 0;
				this.LastClearedWidth = 0;

			} catch {
				Thread.Sleep(1000);
			}
		}

		/// <summary>
		/// Renders the <see cref="PixelDumper"/> output to the control.
		/// </summary>
		/// <param name="data">The colour data to render in 32-bit ARGB format.</param>
		/// <param name="width">The width of the image to render in pixels.</param>
		/// <param name="height">The height of the image to render in pixels.</param>
		/// <param name="backgroundColour">The background colour to fill the unused area of the screen width.</param>
		public void Render(int[] data, int width, int height, Color backgroundColour) {

			if (this.GraphicsDevice == null) {
				this.ReinitialiseRenderer();
				if (this.GraphicsDevice == null) return;
			}

			try {

				if (this.VideoOutput != null) {

					if (width > this.VideoOutputWidth || height > this.VideoOutputHeight) {
						this.VideoOutputWidth = 1;
						while (this.VideoOutputWidth < width) this.VideoOutputWidth <<= 1;
						this.VideoOutputHeight = 1;
						while (this.VideoOutputHeight < height) this.VideoOutputHeight <<= 1;
						this.ReinitialiseRenderer();
					}

					if (this.LastClearedWidth < this.VideoOutputWidth || this.LastClearedHeight < this.VideoOutputHeight || this.LastClearedColour != backgroundColour) {
						var OutputStream = this.VideoOutput.LockRectangle(0, new Rectangle(0, 0, this.VideoOutputWidth, this.VideoOutputHeight), LockFlags.Discard);
						var ClearedBuffer = new int[this.VideoOutputWidth];
						for (int x = 0; x < ClearedBuffer.Length; ++x) ClearedBuffer[x] = backgroundColour.ToArgb();
						int PitchOverflow = OutputStream.Pitch - this.VideoOutputWidth * 4;
						for (int y = 0; y < this.VideoOutputHeight; ++y) {
							OutputStream.Data.WriteRange(ClearedBuffer, 0, ClearedBuffer.Length);
						}
						this.VideoOutput.UnlockRectangle(0);
						this.LastClearedColour = backgroundColour;
						this.LastClearedWidth = this.VideoOutputWidth;
						this.LastClearedHeight = this.VideoOutputHeight;
					}
					
					{

						if (data != null) {
							var OutputStream = this.VideoOutput.LockRectangle(0, new Rectangle(0, 0, width, height), LockFlags.Discard);
							int PitchOverflow = OutputStream.Pitch - width * 4;
							for (int y = 0; y < height; ++y) {
								OutputStream.Data.WriteRange(data, y * width, width);
								OutputStream.Data.Seek(PitchOverflow, SeekOrigin.Current);
							}
						}
						this.VideoOutput.UnlockRectangle(0);
					}
				}

				this.GraphicsDevice.SetRenderState(RenderState.CullMode, Cull.None);
				this.GraphicsDevice.SetRenderState(RenderState.Lighting, false);

				this.GraphicsDevice.BeginScene();

				this.GraphicsDevice.SetSamplerState(0, SamplerState.MagFilter, this.LinearInterpolation ? TextureFilter.Linear : TextureFilter.Point);

				this.GraphicsDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, backgroundColour, 1.0f, 0);

				float OffsetX = -.5f / (float)this.VideoOutputWidth;
				float OffsetY = +.5f / (float)this.VideoOutputHeight;

				OffsetX = 0f; OffsetY = 0f;

				float ScaleX = 1f, ScaleY = 1f;
				switch (this.ScaleMode) {
					case ScaleModes.ZoomInside:
					case ScaleModes.ZoomOutside:
						float DestinationAspectRatio = (float)this.Control.ClientSize.Width / (float)this.Control.ClientSize.Height;
						float SourceAspectRatio = (float)width / (float)height;
						switch (this.ScaleMode) {
							case ScaleModes.ZoomInside:
								if (SourceAspectRatio > DestinationAspectRatio) {
									ScaleY = SourceAspectRatio / DestinationAspectRatio;
								} else {
									ScaleX = DestinationAspectRatio / SourceAspectRatio;
								}
								break;
							case ScaleModes.ZoomOutside:
								if (SourceAspectRatio < DestinationAspectRatio) {
									ScaleY = SourceAspectRatio / DestinationAspectRatio;
								} else {
									ScaleX = DestinationAspectRatio / SourceAspectRatio;
								}
								break;
						}
						
						break;
				}

				this.GraphicsDevice.SetTransform(TransformState.World, Matrix.Translation(
					OffsetX + (((float)(this.VideoOutputWidth - width)) / (float)this.VideoOutputWidth) * 0.5f,
					OffsetY + (((float)(this.VideoOutputHeight - height)) / (float)this.VideoOutputHeight) * -0.5f,
					0f));


				this.GraphicsDevice.SetTransform(TransformState.View, Matrix.LookAtLH(new Vector3(0f, 0f, -5f), Vector3.Zero, Vector3.UnitY));
				
				this.GraphicsDevice.SetTransform(
					TransformState.Projection,
					Matrix.OrthoLH(((float)width / (float)this.VideoOutputWidth * ScaleX), ((float)height / (float)this.VideoOutputHeight * ScaleY), 0f, 10f)
				);


				this.GraphicsDevice.SetTexture(0, this.VideoOutput);

				this.GraphicsDevice.SetStreamSource(0, this.Vertices, 0, Vertex.Size);
				this.GraphicsDevice.VertexFormat = Vertex.Format;
				this.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);

				this.GraphicsDevice.EndScene();				

				try {
					this.GraphicsDevice.Present();
				} catch (Direct3D9Exception) {
					if (Result.Last == SlimDX.Direct3D9.ResultCode.DeviceLost) {
						this.ReinitialiseRenderer();
						this.Render(data, width, height, backgroundColour);
					}
				}

			} catch {
				this.DisposeRenderer();
			}

		}

		/// <summary>
		/// Gets the current refresh rate.
		/// </summary>
		/// <returns>The current refresh rate in Hertz.</returns>
		public static int GetCurrentRefreshRate() {

			if (EnTech.PowerStrip.IsAvailable) {
				try {
					return EnTech.PowerStrip.GetRefreshRate(0);
				} catch { }
			}

			var RefreshRates = new List<int>();
			using (var RefreshSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController")) {
				foreach (var VideoCard in RefreshSearcher.Get()) RefreshRates.Add(Convert.ToInt32(VideoCard["CurrentRefreshRate"]));
			}
			var ReportedRate = RefreshRates[0];
			// This 'orrible 'ack is to handle the problem that some refresh rates are returned incorrectly (1Hz too small).
			var CommonRefreshRates = new[] { 43, 56, 60, 65, 70, 72, 75, 80, 85, 90, 95, 100, 120 };
			foreach (var CommonRate in CommonRefreshRates) if (CommonRate == ReportedRate) return ReportedRate;
			foreach (var CommonRate in CommonRefreshRates) if (CommonRate == ReportedRate + 1) return CommonRate;
			return ReportedRate;
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets the <see cref="Control"/> that this dumps pixels to.
		/// </summary>
		public Control Control { get; private set; }

		/// <summary>
		/// Gets or sets the whether to use bilinear interpolation.
		/// </summary>
		public bool LinearInterpolation { get; set; }

		/// <summary>
		/// Gets the <see cref="ScaleModes"/> used to scale the image.
		/// </summary>
		public ScaleModes ScaleMode { get; set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Creates an instance of the <see cref="PixelDumper"/>.
		/// </summary>
		/// <param name="handle"><see cref="Control"/> to render to.</param>
		public PixelDumper(Control control) {
			this.Control = control;
			this.ScaleMode = ScaleModes.ZoomInside;
			this.ReinitialiseRenderer();
		}

		#endregion

		#region Waste Management

		/// <summary>
		/// Disposes resources used by this control.
		/// </summary>
		public void Dispose() {
			this.DisposeRenderer();
		}

		~PixelDumper() {
			this.Dispose();
		}

		#endregion
	}
}
