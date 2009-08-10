using System;
using System.Collections.Generic;
using System.Drawing;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using SlimDX;
using SlimDX.Direct3D9;

namespace BeeDevelopment.Cogwheel {
	class PixelDumper3D : IDisposable {

		#region Types

		/// <summary>
		/// Represents an eye.
		/// </summary>
		public enum Eye {
			/// <summary>
			/// The left eye.
			/// </summary>
			Left = 0,
			/// <summary>
			/// The right eye.
			/// </summary>
			Right = 1,
		}

		/// <summary>
		/// Represents a stereoscopic display mode.
		/// </summary>
		public enum StereoscopicDisplayMode {
			/// <summary>
			/// Only the view for the most recently updated eye is displayed.
			/// </summary>
			MostRecentEyeOnly,
			/// <summary>
			/// Only the view for the left eye is displayed.
			/// </summary>
			LeftEyeOnly,
			/// <summary>
			/// Only the view for the right eye is displayed.
			/// </summary>
			RightEyeOnly,
			/// <summary>
			/// The left and right eye views are shown in alternate rows.
			/// </summary>
			RowInterleaved,
			/// <summary>
			/// The left and right eye views are shown in alternate columns.
			/// </summary>
			ColumnInterleaved,
			/// <summary>
			/// The left and right views are shown in alternate pixels in a chequerboard pattern.
			/// </summary>
			ChequerboardInterleaved,
			/// <summary>
			/// The left and right eye views are combined into a greyscale anaglyph.
			/// </summary>
			MonochromeAnaglyph,
			/// <summary>
			/// The left and right eye views are combined into a colour anaglyph.
			/// </summary>
			ColourAnaglyph,
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

		/// <summary>
		/// Represents a vertex with a 3D position and 2D texture coordinate.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		private struct VertexPositionTexture {

			/// <summary>
			/// The position in 3D space of the <see cref="VertexPositionTexture"/>.
			/// </summary>
			public Vector3 Position;

			/// <summary>
			/// The 2D texture coordinate of the <see cref="VertexPositionTexture"/>.
			/// </summary>
			public Vector2 TextureCoordinate;

			/// <summary>
			/// Creates an instance of a <see cref="VertexPositionTexture"/>.
			/// </summary>
			/// <param name="position">The position of the vertex.</param>
			/// <param name="textureCoordinate">The texture coordinate of the vertex.</param>
			public VertexPositionTexture(Vector3 position, Vector2 textureCoordinate) {
				this.Position = position;
				this.TextureCoordinate = textureCoordinate;
			}

			/// <summary>
			/// Gets the size in bytes of the <see cref="VertexPositionTexture"/> structure.
			/// </summary>
			public static int Size {
				get { return Marshal.SizeOf(typeof(VertexPositionTexture)); }
			}

			/// <summary>
			/// Gets the <see cref="VertexFormat"/> of the <see cref="VertexPositionTexture"/> structure.
			/// </summary>
			public static VertexFormat Format {
				get { return VertexFormat.Position | VertexFormat.Texture0; }
			}

			/// <summary>
			/// Gets an array of <see cref="VertexElement"/>s describing the <see cref="VertexPositionTexture"/> structure.
			/// </summary>
			public static VertexElement[] Elements {
				get {
					return new VertexElement[]{
						new VertexElement(0, 0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0),
						new VertexElement(0, 12, DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 0),
						VertexElement.VertexDeclarationEnd,
					};
				}
			}
		}

		#endregion

		#region Private Fields

		/// <summary>
		/// Stores a reference to the <see cref="Direct3D"/> instance in use.
		/// </summary>
		private Direct3D D3D;

		/// <summary>
		/// Stores a reference to the <see cref="Device"/> instance in use.
		/// </summary>
		private Device GraphicsDevice;

		/// <summary>
		/// Stores a reference to the control that we're rendering to.
		/// </summary>
		private Control Control;

		/// <summary>
		/// Stores references to the two textures (one for each eye).
		/// </summary>
		private Texture[] Textures;

		/// <summary>
		/// Stores the current vertex declaration.
		/// </summary>
		private VertexDeclaration VertexDeclaration;

		/// <summary>
		/// Stores the <see cref="VertexBuffer"/> that in turn stores the vertices for the full-screen quad to render.
		/// </summary>
		private VertexBuffer Vertices;

		/// <summary>
		/// Stores the <see cref="Effect"/> that contains the various blending techniques.
		/// </summary>
		private Effect Effect;

		/// <summary>
		/// Stores the most recently updated eye.
		/// </summary>
		private Eye MostRecentlyUpdatedEye;

		/// <summary>
		/// Stores the most recently written width.
		/// </summary>
		private int MostRecentWidth = 1;

		/// <summary>
		/// Stores the most recently written height.
		/// </summary>
		private int MostRecentHeight = 1;

		/// <summary>
		/// Stores the factor used to correct the height of the textured quad and maintain the source aspect ratio.
		/// </summary>
		private float CorrectedHeightScale = 1.0f;

		/// <summary>
		/// Stores the factor used to correct the width of the textured quad and maintain the source aspect ratio.
		/// </summary>
		private float CorrectedWidthScale = 1.0f;

		#endregion

		#region Properties

		private Color leftEyeColour = Color.Red;
		/// <summary>
		/// Gets or sets the colour used for the left eye filter when using an anaglyph display mode.
		/// </summary>
		public Color LeftEyeColour {
			get { return this.leftEyeColour; }
			set {
				this.leftEyeColour = value;
				if (this.Effect != null) {
					using (var p = this.Effect.GetParameter(null, "LeftEyeColour")) {
						this.Effect.SetValue(p, new Color4(value));
					}
				}
			}
		}

		private Color rightEyeColour = Color.Cyan;
		/// <summary>
		/// Gets or sets the colour used for the right eye filter when using an anaglyph display mode.
		/// </summary>
		public Color RightEyeColour {
			get { return this.rightEyeColour; }
			set {
				this.rightEyeColour = value;
				if (this.Effect != null) {
					using (var p = this.Effect.GetParameter(null, "RightEyeColour")) {
						this.Effect.SetValue(p, new Color4(value));
					}
				}
			}
		}

		private StereoscopicDisplayMode displayMode = StereoscopicDisplayMode.MostRecentEyeOnly;
		/// <summary>
		/// Gets or sets the current <see cref="StereoscopicDisplayMode"/>.
		/// </summary>
		public StereoscopicDisplayMode DisplayMode {
			get { return this.displayMode; }
			set {
				if (!Enum.IsDefined(typeof(StereoscopicDisplayMode), value)) throw new ArgumentException("Unsupported display mode.");
				this.displayMode = value;
				var EffectTechnique = value;
				if (this.displayMode == StereoscopicDisplayMode.MostRecentEyeOnly) {
					EffectTechnique = this.MostRecentlyUpdatedEye == Eye.Left ? StereoscopicDisplayMode.LeftEyeOnly : StereoscopicDisplayMode.RightEyeOnly;
				}
				if (this.Effect != null) {
					if (this.Effect.Technique != null) {
						this.Effect.Technique.Dispose();
					}
					this.Effect.Technique = this.Effect.GetTechnique(EffectTechnique.ToString());
				}
				// Interleaving display modes need to fix the vertex buffer so that it's displayed as an even number of pixels on the backbuffer.
				if (this.Vertices != null) {
					switch (this.displayMode) {
						case StereoscopicDisplayMode.RowInterleaved:
						case StereoscopicDisplayMode.ColumnInterleaved:
						case StereoscopicDisplayMode.ChequerboardInterleaved:
							this.RewriteVertexBuffer();
							break;
					}
				}
			}
		}

		private ScaleModes scaleMode = ScaleModes.ZoomInside;
		/// <summary>
		/// Gets the <see cref="ScaleModes"/> used to scale the image.
		/// </summary>
		public ScaleModes ScaleMode {
			get {return this.scaleMode;}
			set {
				this.scaleMode = value;
				if (this.GraphicsDevice != null) {
					this.RewriteVertexBuffer();
				}
			}
		}

		/// <summary>
		/// Gets or sets the background clear colour.
		/// </summary>
		public Color BackgroundColour { get; set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Creates an instance of a <see cref="PixelDumper3D"/>.
		/// </summary>
		/// <param name="direct3D">The current <see cref="Direct3D"/> instance for the application.</param>
		/// <param name="control">The <see cref="Control"/> that is being rendered to.</param>
		public PixelDumper3D(Direct3D direct3D, Control control) {
			this.D3D = direct3D;
			this.Control = control;
			this.Textures = new Texture[2];
		}

		#endregion

		#region Device Creation/Destruction

		/// <summary>
		/// Destroys the current <see cref="Device"/>.
		/// </summary>
		private void DestroyDevice() {
			if (this.Vertices != null) {
				this.Vertices.Dispose();
				this.Vertices = null;
			}
			if (this.VertexDeclaration != null) {
				this.VertexDeclaration.Dispose();
				this.VertexDeclaration = null;
			}
			for (int i = 0; i < this.Textures.Length; ++i) {
				if (this.Textures[i] != null) {
					this.Textures[i].Dispose();
					this.Textures[i] = null;
				}
			}
			if (this.Effect != null) {
				this.Effect.Dispose();
				this.Effect = null;
			}
			if (this.GraphicsDevice != null) {
				this.GraphicsDevice.Dispose();
				this.GraphicsDevice = null;
			}
		}

		/// <summary>
		/// Recreates the <see cref="Device"/> used to render to.
		/// </summary>
		public void RecreateDevice() {
			this.DestroyDevice();
			// Create the device:
			this.GraphicsDevice = new Device(
				this.D3D,
				0,
				DeviceType.Hardware,
				this.Control.Handle,
				CreateFlags.HardwareVertexProcessing,
				new PresentParameters() {
					BackBufferWidth = Math.Max(1, this.Control.ClientSize.Width),
					BackBufferHeight = Math.Max(1, this.Control.ClientSize.Height),
					DeviceWindowHandle = this.Control.Handle,
					PresentationInterval = PresentInterval.One,
				}
			);
			// Create the effect:
			this.Effect = Effect.FromString(this.GraphicsDevice, Properties.Resources.PixelDumper3DShader, ShaderFlags.OptimizationLevel3);
			this.LeftEyeColour = this.LeftEyeColour;
			this.RightEyeColour = this.RightEyeColour;
			this.DisplayMode = this.DisplayMode;

			// Create the vertex declaration:
			this.VertexDeclaration = new VertexDeclaration(this.GraphicsDevice, VertexPositionTexture.Elements);
			// Create the vertex buffer:
			this.Vertices = new VertexBuffer(this.GraphicsDevice, 6 * VertexPositionTexture.Size, Usage.WriteOnly, VertexFormat.None, Pool.Managed);
			this.RewriteVertexBuffer();	
		}

		private void RewriteVertexBuffer() {
			// Start at the full position size:
			float PL = -1.0f, PR = +1.0f, PT = +1.0f, PB = -1.0f;

			// Calculate the aspect ratio of the source image and the viewport.
			float ImageAspectRatio = (float)this.MostRecentWidth / (float)this.MostRecentHeight;
			float ViewportAspectRatio = (float)this.GraphicsDevice.Viewport.Width / (float)this.GraphicsDevice.Viewport.Height;

			// Calculate the aspect ratio correction scale factors:
			this.CorrectedWidthScale = this.CorrectedHeightScale = 1.0f;
			switch (this.scaleMode) {
				case ScaleModes.ZoomInside:
					if (ImageAspectRatio > ViewportAspectRatio) {
						this.CorrectedHeightScale = (ViewportAspectRatio / ImageAspectRatio);
					} else {
						this.CorrectedWidthScale = (ImageAspectRatio / ViewportAspectRatio);
					}
					break;
				case ScaleModes.ZoomOutside:
					if (ImageAspectRatio < ViewportAspectRatio) {
						this.CorrectedHeightScale = (ViewportAspectRatio / ImageAspectRatio);
					} else {
						this.CorrectedWidthScale = (ImageAspectRatio / ViewportAspectRatio);
					}
					break;
			}

			// Apply the aspect ratio correction scale factors:
			PL *= CorrectedWidthScale; PR *= CorrectedWidthScale;
			PT *= CorrectedHeightScale; PB *= CorrectedHeightScale;

			// If we're using a row-interlaced mode, make sure the top edge of the screen is aligned to a device pixel.
			if (this.displayMode == StereoscopicDisplayMode.RowInterleaved || this.displayMode == StereoscopicDisplayMode.ChequerboardInterleaved) {
				float StartingPosition = this.GraphicsDevice.Viewport.Height * 0.5f - PT * this.GraphicsDevice.Viewport.Height * 0.5f;
				float StartingPositionFix = 2.0f * (StartingPosition - (int)StartingPosition);
				PT += StartingPositionFix / this.GraphicsDevice.Viewport.Height;
				PB += StartingPositionFix / this.GraphicsDevice.Viewport.Height;
			}

			// If we're using a column-interlaced mode, make sure the left edge of the screen is aligned to a device pixel.
			if (this.displayMode == StereoscopicDisplayMode.ColumnInterleaved || this.displayMode == StereoscopicDisplayMode.ChequerboardInterleaved) {
				float StartingPosition = this.GraphicsDevice.Viewport.Width * 0.5f + PL * this.GraphicsDevice.Viewport.Width * 0.5f;
				float StartingPositionFix = 2.0f * (StartingPosition - (int)StartingPosition);
				PL += StartingPositionFix / this.GraphicsDevice.Viewport.Width;
				PR += StartingPositionFix / this.GraphicsDevice.Viewport.Width;
			}

			// Create the vectors representing the corners of the screen:
			Vector3 PTL = new Vector3(PL, PT, 0.0f);
			Vector3 PTR = new Vector3(PR, PT, 0.0f);
			Vector3 PBL = new Vector3(PL, PB, 0.0f);
			Vector3 PBR = new Vector3(PR, PB, 0.0f);

			// Start with the full texture size:
			float TL = 0.0f, TR = 1.0f, TT = 0.0f, TB = 1.0f;

			// Create the vectors representing the corners of the texture:
			Vector2 TTL = new Vector2(TL, TT);
			Vector2 TTR = new Vector2(TR, TT);
			Vector2 TBL = new Vector2(TL, TB);
			Vector2 TBR = new Vector2(TR, TB);

			using (var VertexStream = Vertices.Lock(0, 0, LockFlags.None)) {
				VertexStream.WriteRange(
					new[] {
						new VertexPositionTexture(PBL, TBL),
						new VertexPositionTexture(PTR, TTR),
						new VertexPositionTexture(PBR, TBR),
						new VertexPositionTexture(PBL, TBL),
						new VertexPositionTexture(PTL, TTL),
						new VertexPositionTexture(PTR, TTR),
					}
				);
				this.Vertices.Unlock();
			}
		}

		#endregion

		#region Renderer

		public void Render() {
			// Recreate the graphics device if required:
			if (this.GraphicsDevice == null) {
				this.RecreateDevice();
			}
			if (this.GraphicsDevice == null) {
				Thread.Sleep(100);
				return;
			}

			// If we're using MostRecentEyeOnly, update the technique appropriately.
			if (this.displayMode == StereoscopicDisplayMode.MostRecentEyeOnly) {
				this.Effect.Technique.Dispose();
				this.Effect.Technique = this.Effect.GetTechnique(this.MostRecentlyUpdatedEye.ToString() + "EyeOnly");
			}

			// Render:
			this.GraphicsDevice.BeginScene();
			{
				this.GraphicsDevice.VertexFormat = VertexPositionTexture.Format;
				this.GraphicsDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, this.BackgroundColour, 0.0f, 0);

				// Set effect parameters:
				switch (this.displayMode) { 
					case StereoscopicDisplayMode.RowInterleaved:
						using (var ParameterHandle = this.Effect.GetParameter(null, "ViewportHeight")) {
							this.Effect.SetValue(ParameterHandle, this.CorrectedHeightScale * this.GraphicsDevice.Viewport.Height);
						}
						break;
					case StereoscopicDisplayMode.ColumnInterleaved:
						using (var ParameterHandle = this.Effect.GetParameter(null, "ViewportWidth")) {
							this.Effect.SetValue(ParameterHandle, this.CorrectedWidthScale * this.GraphicsDevice.Viewport.Width);
						}
						break;
					case StereoscopicDisplayMode.ChequerboardInterleaved:
						using (var ParameterHandle = this.Effect.GetParameter(null, "ViewportHeight")) {
							this.Effect.SetValue(ParameterHandle, this.CorrectedHeightScale * this.GraphicsDevice.Viewport.Height);
						}
						using (var ParameterHandle = this.Effect.GetParameter(null, "ViewportWidth")) {
							this.Effect.SetValue(ParameterHandle, this.CorrectedWidthScale * this.GraphicsDevice.Viewport.Width);
						}
						break;
				}

				this.Effect.Begin();
				{
					this.Effect.BeginPass(0);
					{
						this.GraphicsDevice.SetStreamSource(0, this.Vertices, 0, VertexPositionTexture.Size);
						this.GraphicsDevice.VertexFormat = VertexPositionTexture.Format;
						this.GraphicsDevice.VertexDeclaration = this.VertexDeclaration;
						this.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
					}
					this.Effect.EndPass();
				}
				this.Effect.End();
			}
			this.GraphicsDevice.EndScene();
			
			// Present:
			try {
				this.GraphicsDevice.Present();
			} catch (Direct3D9Exception) {
				// Did we lose the device? If so, try again.
				if (Result.Last == ResultCode.DeviceLost) {
					this.RecreateDevice();
					this.Render();
				}
			}
		}

		#endregion

		#region Texture Setter

		/// <summary>
		/// Update the internal left or right eye image.
		/// </summary>
		/// <param name="eye">The <see cref="Eye"/> that the image corresponds to.</param>
		/// <param name="data">The XRGB image data.</param>
		/// <param name="width">The width of the image in pixels.</param>
		/// <param name="height">The height of the image in pixels.</param>
		public void SetImage(Eye eye, int[] data, int width, int height) {
			if (this.GraphicsDevice == null) this.RecreateDevice();
			var Texture = this.Textures[(int)eye];
			// Do we need to recreate the texture?
			var NeedsRecreating = false;
			if (Texture == null) {
				NeedsRecreating = true;
			} else {
				var CurrentTextureSize = Texture.GetLevelDescription(0);
				if (CurrentTextureSize.Width != width || CurrentTextureSize.Height != height) {
					NeedsRecreating = true;
				}
			}
			// If we need to recreate the texture, do so.
			if (NeedsRecreating) {
				if (Texture != null) {
					Texture.Dispose();
					Texture = null;
				}
				Texture = new Texture(this.GraphicsDevice, width, height, 1, Usage.Dynamic, Format.X8R8G8B8, Pool.Default);
				// Update the texture array.
				this.Textures[(int)eye] = Texture;
				// Update the effect.
				using (var ParameterHandle = this.Effect.GetParameter(null, eye.ToString() + "Eye")) {
					this.Effect.SetTexture(ParameterHandle, Texture);
				}
			}
			// Copy the image data to the texture.
			using (var Data = Texture.LockRectangle(0, LockFlags.Discard).Data) {
				Data.WriteRange(data);
				Texture.UnlockRectangle(0);
			}


			// Mark the most recently updated eye, width and height as such.
			this.MostRecentlyUpdatedEye = eye;
			if (width != this.MostRecentWidth || height != this.MostRecentHeight) {
				this.MostRecentWidth = width;
				this.MostRecentHeight = height;
				this.RewriteVertexBuffer();
			}
		}

		#endregion

		#region IDisposable Members

		private bool disposed = false;
		/// <summary>
		/// Gets a value indicating whether the <see cref="PixelDumper"/> has been disposed of.
		/// </summary>
		public bool Disposed { get { return this.disposed; } }

		/// <summary>
		/// Releases all resources used by the <see cref="PixelDumper3D"/>.
		/// </summary>
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			if (!this.disposed) {
				if (disposing) {
					// Dispose fields.
					this.DestroyDevice();
				}
				// Mark fields as null.
				for (int i = 0; i < this.Textures.Length; ++i) this.Textures[i] = null;
				this.Vertices = null;
				this.Effect = null;
				this.GraphicsDevice = null;
				this.D3D = null;
				this.disposed = true;
			}
		}

		#endregion

		#region Utility Methods

		/// <summary>
		/// Gets the current refresh rate.
		/// </summary>
		/// <returns>The current refresh rate in Hertz.</returns>
		public static int GetCurrentRefreshRate() {

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
	}
}