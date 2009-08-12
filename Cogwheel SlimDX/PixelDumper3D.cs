using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using SlimDX;
using SlimDX.Direct3D9;

namespace BeeDevelopment.Cogwheel {
	class PixelDumper3D : IDisposable {

		#region Enums

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

		#endregion

		#region Types

		/// <summary>
		/// Represents a vertex with a 3D position and 2D texture coordinate.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		private struct VertexPositionTextureTexture {

			/// <summary>
			/// The position in 3D space of the <see cref="VertexPositionTexture"/>.
			/// </summary>
			public Vector3 Position;

			/// <summary>
			/// The 2D texture coordinate of the <see cref="VertexPositionTexture"/>.
			/// </summary>
			public Vector2 TextureCoordinate;

			/// <summary>
			/// A 2D texture coordinate of the <see cref="VertexPositionTexture"/>, with (0,0) corresponding to the top-left of the shape and (1,1) the bottom-right (regardless of the position of the texture itself).
			/// </summary>
			public Vector2 NormalisedTextureCoordinate;

			/// <summary>
			/// Creates an instance of a <see cref="VertexPositionTexture"/>.
			/// </summary>
			/// <param name="position">The position of the vertex.</param>
			/// <param name="textureCoordinate">The texture coordinate of the vertex.</param>
			/// <param name="normalisedTextureCoordinate">The normalised texture coordinate of the vertex, with (0,0) corresponding to the top-left of the shape and (1,1) the bottom-right (regardless of the position of the texture itself).</param>
			public VertexPositionTextureTexture(Vector3 position, Vector2 textureCoordinate, Vector2 normalisedTextureCoordinate) {
				this.Position = position;
				this.TextureCoordinate = textureCoordinate;
				this.NormalisedTextureCoordinate = normalisedTextureCoordinate;
			}

			/// <summary>
			/// Gets the size in bytes of the <see cref="VertexPositionTexture"/> structure.
			/// </summary>
			public static int Size {
				get { return Marshal.SizeOf(typeof(VertexPositionTextureTexture)); }
			}

			/// <summary>
			/// Gets the <see cref="VertexFormat"/> of the <see cref="VertexPositionTexture"/> structure.
			/// </summary>
			public static VertexFormat Format {
				get { return VertexFormat.Position | VertexFormat.Texture0 | VertexFormat.Texture1; }
			}

			/// <summary>
			/// Gets an array of <see cref="VertexElement"/>s describing the <see cref="VertexPositionTexture"/> structure.
			/// </summary>
			public static VertexElement[] Elements {
				get {
					return new VertexElement[]{
						new VertexElement(0, 0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0),
						new VertexElement(0, 12, DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 0),
						new VertexElement(0, 20, DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 1),
						VertexElement.VertexDeclarationEnd,
					};
				}
			}
		}

		/// <summary>
		/// Represents a texture for a particular eye.
		/// </summary>
		class EyeTexture : IDisposable {

			#region Fields

			/// <summary>
			/// Stores a reference to the <see cref="GraphicsDevive"/> used to create the textures on.
			/// </summary>
			private Device GraphicsDevice;

			/// <summary>
			/// Stores a reference to the internal D3D texture.
			/// </summary>
			public Texture Texture;

			private int imageWidth;
			/// <summary>
			/// Represents the width of the image data.
			/// </summary>
			/// <remarks>This is not necessarily the same width as the internal D3D texture.</remarks>
			public int ImageWidth { get { return this.imageWidth; } }
			
			private int imageHeight;
			/// <summary>
			/// Represents the height of the image data.
			/// </summary>
			/// <remarks>This is not necessarily the same height as the internal D3D texture.</remarks>
			public int ImageHeight { get { return this.imageHeight; } }

			private int textureWidth;
			/// <summary>
			/// Represents the width of the texture.
			/// </summary>
			public int TextureWidth { get { return this.textureWidth; } }

			private int textureHeight;
			/// <summary>
			/// Represents the height of the texture.
			/// </summary>
			public int TextureHeight { get { return this.textureHeight; } }

			#endregion

			#region Properties

			private bool disposed = false;
			/// <summary>
			/// Gets whether the <see cref="EyeTexture"/> has been disposed yet or not.
			/// </summary>
			public bool Disposed {
				get { return this.disposed; }
			}

			#endregion

			#region Constructor

			/// <summary>
			/// Creates an instance of an <see cref="EyeTexture"/>.
			/// </summary>
			/// <param name="graphicsDevice">The <see cref="Device"/> to create the <see cref="EyeTexture"/> on.</param>
			public EyeTexture(Device graphicsDevice) {
				this.GraphicsDevice = graphicsDevice;
			}

			#endregion

			#region Methods

			/// <summary>
			/// Update the internal image.
			/// </summary>
			/// <param name="data">The XRGB image data.</param>
			/// <param name="width">The width of the image in pixels.</param>
			/// <param name="height">The height of the image in pixels.</param>
			public void SetImage(int[] data, int width, int height) {
				var NeedsRecreating = false;
				if (this.Texture == null) {
					NeedsRecreating = true;
				} else {
					var CurrentTextureSize = Texture.GetLevelDescription(0);
					if (this.imageWidth != width || this.imageHeight != height) {
						NeedsRecreating = true;
					}
				}
				// If we need to recreate the texture, do so.
				if (NeedsRecreating) {
					if (Texture != null) {
						Texture.Dispose();
						Texture = null;
					}
					// Copy the width/height to member fields.
					this.imageWidth = width;
					this.imageHeight = height;
					// Round up the width/height to the nearest power of two.
					this.textureWidth = 1; this.textureHeight = 1;
					while (this.textureWidth < this.imageWidth) this.textureWidth <<= 1;
					while (this.textureHeight < this.imageWidth) this.textureHeight <<= 1;
					// Create a new texture instance.
					Texture = new Texture(this.GraphicsDevice, this.textureWidth, this.textureHeight, 1, Usage.Dynamic, Format.X8R8G8B8, Pool.Default);
				}
				// Copy the image data to the texture.
				using (var Data = this.Texture.LockRectangle(0, new Rectangle(0, 0, this.imageWidth, this.imageHeight), LockFlags.None).Data) {
					if (this.imageWidth == this.textureWidth) {
						// Widths are the same, just dump the data across (easy!)
						Data.WriteRange(data);
					} else {
						// Widths are different, need a bit of additional magic here to make them fit:
						long RowSeekOffset = 4 * (this.textureWidth - this.imageWidth);
						for (int r = 0, s = 0; r < this.imageHeight; ++r, s += this.imageWidth) {
							Data.WriteRange(data, s, this.imageWidth);
							Data.Seek(RowSeekOffset, SeekOrigin.Current);
						}
					}
					this.Texture.UnlockRectangle(0);
				}
			}

			/// <summary>
			/// Release the resources used by this <see cref="EyeTexture"/>.
			/// </summary>
			public void Dispose() {
				this.Dispose(true);
				GC.SuppressFinalize(this);
			}

			protected void Dispose(bool disposing) {
				if (!disposed) {
					if (disposing) {
						if (this.Texture != null) {
							this.Texture.Dispose();
						}
					}
					this.Texture = null;
					this.disposed = true;
				}
			}

			#endregion
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
		private EyeTexture[] Textures;

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
		/// Stores the factor used to correct the height of the textured quad and maintain the source aspect ratio.
		/// </summary>
		private float CorrectedHeightScale = 1.0f;

		/// <summary>
		/// Stores the factor used to correct the width of the textured quad and maintain the source aspect ratio.
		/// </summary>
		private float CorrectedWidthScale = 1.0f;

		/// <summary>
		/// Stores the coordinates of the top-left corner of the displayed texture.
		/// </summary>
		private Vector2 TextureTopLeft = Vector2.Zero;

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

		/// <summary>
		/// Gets or sets the magnification filter.
		/// </summary>
		public TextureFilter MagnificationFilter { get; set; }

		/// <summary>
		/// Gets the current refresh rate.
		/// </summary>
		public int CurrentRefreshRate { get; private set; }

		/// <summary>
		/// Gets or sets the first eye shown in interleaved modes.
		/// </summary>
		/// <remarks>This is the eye view shown in the first row, column or pixel when using row, column or chequerboard interleaving display modes respectively.</remarks>
		public Eye FirstInterleavedEye { get; set; }

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
			this.Textures = new EyeTexture[2];
			this.CurrentRefreshRate = PixelDumper3D.GetCurrentRefreshRate();
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
			this.VertexDeclaration = new VertexDeclaration(this.GraphicsDevice, VertexPositionTextureTexture.Elements);
			// Create the vertex buffer:
			this.Vertices = new VertexBuffer(this.GraphicsDevice, 6 * VertexPositionTextureTexture.Size, Usage.WriteOnly, VertexFormat.None, Pool.Managed);
			this.RewriteVertexBuffer();
			// Update the monitor refresh rate:
			this.CurrentRefreshRate = PixelDumper3D.GetCurrentRefreshRate();
		}

		private void RewriteVertexBuffer() {
			// Start at the full position size:
			float PL = -1.0f, PR = +1.0f, PT = +1.0f, PB = -1.0f;

			// Calculate the aspect ratio of the source image and the viewport.

			var MostRecentlyUpdatedTexture = this.Textures[(int)this.MostRecentlyUpdatedEye];
			float ImageAspectRatio = MostRecentlyUpdatedTexture == null ? 1.0f : (float)MostRecentlyUpdatedTexture.ImageWidth / (float)MostRecentlyUpdatedTexture.ImageHeight;
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

			this.TextureTopLeft.X = PL;
			this.TextureTopLeft.Y = PT;

			// Start with the full texture size:
			float TL = 0.0f, TR = 1.0f, TT = 0.0f, TB = 1.0f;

			// Adjust texture coordinates to clip the image if need be:
			if (MostRecentlyUpdatedTexture != null) {
				TR = (float)MostRecentlyUpdatedTexture.ImageWidth / (float)MostRecentlyUpdatedTexture.TextureWidth;
				TB = (float)MostRecentlyUpdatedTexture.ImageHeight / (float)MostRecentlyUpdatedTexture.TextureHeight;
			}

			// Create the vectors representing the corners of the texture:
			Vector2 TTL = new Vector2(TL, TT);
			Vector2 TTR = new Vector2(TR, TT);
			Vector2 TBL = new Vector2(TL, TB);
			Vector2 TBR = new Vector2(TR, TB);

			// Create normalised 2D vectors representing the corners of the shape:
			Vector2 NTL = new Vector2(0.0f, 0.0f);
			Vector2 NTR = new Vector2(1.0f, 0.0f);
			Vector2 NBL = new Vector2(0.0f, 1.0f);
			Vector2 NBR = new Vector2(1.0f, 1.0f);

			using (var VertexStream = Vertices.Lock(0, 0, LockFlags.None)) {
				VertexStream.WriteRange(
					new[] {
						new VertexPositionTextureTexture(PBL, TBL, NBL),
						new VertexPositionTextureTexture(PTR, TTR, NTR),
						new VertexPositionTextureTexture(PBR, TBR, NBR),
						new VertexPositionTextureTexture(PBL, TBL, NBL),
						new VertexPositionTextureTexture(PTL, TTL, NTL),
						new VertexPositionTextureTexture(PTR, TTR, NTR),
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
				this.GraphicsDevice.VertexFormat = VertexPositionTextureTexture.Format;

				this.GraphicsDevice.SetRenderState(RenderState.AlphaFunc, Compare.Greater);

				this.GraphicsDevice.SetSamplerState(0, SamplerState.MagFilter, this.MagnificationFilter);
				this.GraphicsDevice.SetSamplerState(1, SamplerState.MagFilter, this.MagnificationFilter);

				this.GraphicsDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, this.BackgroundColour, 0.0f, 0);

				// Set effect width/height parameters:
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

				// Set texture eye parameters (if we need to flip):
				{
					bool FlipLeftRightTextures = false;
					// If we're using an interleaved mode, ensure that the left eye 
					switch (this.displayMode) {
						case StereoscopicDisplayMode.RowInterleaved:
							int RowOffset = this.Control.PointToScreen(new Point(0, (int)Math.Round(0.5f * this.GraphicsDevice.Viewport.Height * (1.0f - this.TextureTopLeft.Y)))).Y;
							FlipLeftRightTextures = this.FirstInterleavedEye == Eye.Right ^ (RowOffset & 1) != 0;
							break;
						case StereoscopicDisplayMode.ColumnInterleaved:
							int ColumnOffset = this.Control.PointToScreen(new Point((int)Math.Round(0.5f * this.GraphicsDevice.Viewport.Width * (1.0f + this.TextureTopLeft.X)), 0)).X;
							FlipLeftRightTextures = this.FirstInterleavedEye == Eye.Right ^ (ColumnOffset & 1) != 0;
							break;
						case StereoscopicDisplayMode.ChequerboardInterleaved:
							Point PixelOffset = this.Control.PointToScreen(new Point((int)Math.Round(0.5f * this.GraphicsDevice.Viewport.Width * (1.0f + this.TextureTopLeft.X)), (int)Math.Round(0.5f * this.GraphicsDevice.Viewport.Height * (1.0f - this.TextureTopLeft.Y))));
							FlipLeftRightTextures = this.FirstInterleavedEye == Eye.Right ^ ((PixelOffset.X + PixelOffset.Y) & 1) != 0;
							break;
					}
					EyeTexture LeftTexture = this.Textures[FlipLeftRightTextures ? 1 : 0], RightTexture = this.Textures[FlipLeftRightTextures ? 0 : 1];
					if (LeftTexture != null) {
						using (var ParameterHandle = this.Effect.GetParameter(null, "LeftEye")) {
							this.Effect.SetTexture(ParameterHandle, LeftTexture.Texture);
						}
					}
					if (RightTexture != null) {
						using (var ParameterHandle = this.Effect.GetParameter(null, "RightEye")) {
							this.Effect.SetTexture(ParameterHandle, RightTexture.Texture);
						}
					}
				}

				this.Effect.Begin();
				{
					this.Effect.BeginPass(0);
					{
						this.GraphicsDevice.SetStreamSource(0, this.Vertices, 0, VertexPositionTextureTexture.Size);
						this.GraphicsDevice.VertexFormat = VertexPositionTextureTexture.Format;
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
				this.GraphicsDevice.Present(Present.DoNotWait);
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

			// Fetch the texture to update.
			var Texture = this.Textures[(int)eye];

			var VertexBufferNeedsRewriting = false;

			// Create the texture instance if need be.
			if (Texture == null) {
				Texture = new EyeTexture(this.GraphicsDevice);
				this.Textures[(int)eye] = Texture;
				VertexBufferNeedsRewriting = true;
			} else if (width != Texture.ImageWidth || height != Texture.ImageHeight) {
				VertexBufferNeedsRewriting = true;
			}
			// Set the image.
			Texture.SetImage(data, width, height);
			
			// Update the effect.
			using (var ParameterHandle = this.Effect.GetParameter(null, eye.ToString() + "Eye")) {
				this.Effect.SetTexture(ParameterHandle, Texture.Texture);
			}

			// Mark the most recently updated eye, width and height as such.
			this.MostRecentlyUpdatedEye = eye;
			if (VertexBufferNeedsRewriting) {
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
		private static int GetCurrentRefreshRate() {
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