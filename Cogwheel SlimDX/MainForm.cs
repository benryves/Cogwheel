using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using BeeDevelopment.Sega8Bit;
using BeeDevelopment.Sega8Bit.Mappers;
using BeeDevelopment.Sega8Bit.Utility;
using BeeDevelopment.Zip;
using SlimDX;
using SlimDX.DirectSound;
using SlimDX.Multimedia;

namespace BeeDevelopment.Cogwheel {
	public partial class MainForm : Form {

		#region Fields

		// Input.
		private InputManager Input;
		private KeyboardInputSource KeyboardInput;

		// Output.
		private PixelDumper3D Dumper;

		// Emulator stuff.
		internal Emulator Emulator;
		private RomIdentifier Identifier;
		protected bool Paused = false;

		private Emulator.GlassesShutter LastEye;
		private int FramesSinceEyeWasUpdated = 100;
		private PixelDumper3D.StereoscopicDisplayMode ThreeDeeDisplayMode = PixelDumper3D.StereoscopicDisplayMode.MostRecentEyeOnly;


		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the size of the form's RenderPanel.
		/// </summary>
		/// <remarks>Setting this property sets the form size to better accomodate the RenderPanel.</remarks>
		public Size RenderPanelSize {
			get { return this.RenderPanel.Size; }
			set {
				this.ClientSize = new Size(
					value.Width + this.ClientSize.Width - this.RenderPanel.Width,
					value.Height + this.ClientSize.Height - this.RenderPanel.Height
				);
			}
		}

		#endregion

		#region Constructor/Initialiser

		public MainForm(string[] arguments) {

			InitializeComponent();
			this.Menus.Renderer = this.Status.Renderer = new Szotar.WindowsForms.ToolStripAeroRenderer(Szotar.WindowsForms.ToolbarTheme.MediaToolbar);

			// Set up input devices:
			this.ReinitialiseInput(null);

			// Create a pixel dumper.
			this.Dumper = new PixelDumper3D(Program.D3D, this.RenderPanel);
			try {
				this.ThreeDeeDisplayMode = (PixelDumper3D.StereoscopicDisplayMode)Enum.Parse(typeof(PixelDumper3D.StereoscopicDisplayMode), Properties.Settings.Default.Option3DGlasses);
			} catch { }
			this.Dumper.MagnificationFilter = Properties.Settings.Default.OptionLinearInterpolation ? SlimDX.Direct3D9.TextureFilter.Linear : SlimDX.Direct3D9.TextureFilter.Point;
			this.Dumper.ScaleMode = Properties.Settings.Default.OptionMaintainAspectRatio ? PixelDumper3D.ScaleModes.ZoomInside : PixelDumper3D.ScaleModes.Stretch;
			this.Dumper.FirstInterleavedEye = Properties.Settings.Default.Option3DGlassesLeftEyeFirst ? PixelDumper3D.Eye.Left : PixelDumper3D.Eye.Right;
			this.Dumper.LeftEyeColour = Color.FromArgb(0xFF, Color.FromArgb(Properties.Settings.Default.Option3DGlassesLeftFilterColour));
			this.Dumper.RightEyeColour = Color.FromArgb(0xFF, Color.FromArgb(Properties.Settings.Default.Option3DGlassesRightFilterColour));

			// Initialise sound.
			this.InitialiseSound();
			this.SoundMuted = !Properties.Settings.Default.OptionEnableSound;

			// Load the emulator.
			this.Emulator = new BeeDevelopment.Sega8Bit.Emulator() {
#if EMU2413
				FmSoundEnabled = Properties.Settings.Default.OptionEnableFMSound,
#endif
			};
			this.OverrideAutomaticSettings(null);

			// Load the ROM data (if required).
			string RomDataDir = Path.Combine(Application.StartupPath, "ROM Data");
			this.Identifier = new RomIdentifier();
			if (Directory.Exists(RomDataDir)) {
				try {
					this.Identifier = new RomIdentifier(RomDataDir);
				} catch (Exception ex) {
					MessageBox.Show(this, ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
			}

			// Set window text and position.
			this.Text = Application.ProductName;
			this.RenderPanelSize = new Size(256 * 2, 192 * 2);
			this.CenterToScreen();

			// Load MRU list.
			this.LoadRecentItemsFromSettings();

			// Attach render loop handler.
			Application.Idle += new EventHandler(Application_Idle);
			this.Disposed += new EventHandler(MainForm_Disposed);

			// Reset quick-load state index:
			this.QuickSaveSlot = 0;

			// Parse command-line arguments.
			if (arguments.Length == 1 && File.Exists(arguments[0])) {
				try {
					this.QuickLoad(arguments[0]);
				} catch { }
			}
		}

		void MainForm_Disposed(object sender, EventArgs e) {
			this.DisposeSound();
			if (this.Dumper != null) this.Dumper.Dispose();
		}
		
		#endregion

		#region Render Loop

		[System.Security.SuppressUnmanagedCodeSecurity, DllImport("User32.dll", CharSet = CharSet.Auto)]
		public static extern bool PeekMessage(out Message msg, IntPtr hWnd, UInt32 msgFilterMin, UInt32 msgFilterMax, UInt32 flags);

		int RefreshStepper = 0;
		bool IsLiveFrame = false;

		Stopwatch FrameTime = new Stopwatch();

		void Application_Idle(object sender, EventArgs e) {
			int SystemRefreshRate = this.Dumper.CurrentRefreshRate;
			Message msg;
			while (!PeekMessage(out msg, IntPtr.Zero, 0, 0, 0)) {
				if (this.WindowState == FormWindowState.Minimized) {
					Thread.Sleep(100);
				} else {
					int FramesToRender= (int)(Math.Round(this.FrameTime.Elapsed.TotalSeconds * SystemRefreshRate));
					this.FrameTime.Reset();
					this.FrameTime.Start();
					if (FramesToRender < 1) {
						FramesToRender = 1;
					} else if (FramesToRender > 5) {
						FramesToRender = 5;
					}

					if (!this.Paused) {
						RefreshStepper -= this.Emulator.Video.FrameRate * FramesToRender;
						this.Input.Poll();
						this.Input.UpdateEmulatorState(this.Emulator);
						while (RefreshStepper <= 0) {
							RefreshStepper += SystemRefreshRate;
							this.Emulator.RunFrame();
							this.IsLiveFrame = true;
							if (!this.SoundMuted) {

								// How many frames of sound should we generate? Usually, one.
								int FramesOfSoundToGenerate = 1;

								// How many samples do we need to calculate per frame?
								int SamplesPerFrame = 44100 / this.Emulator.Video.FrameRate;

								// Offset (backwards) to align within the sound buffer when changing video refresh rates.
								this.SoundBufferPosition -= (this.SoundBufferPosition % (SamplesPerFrame * 2 * 2));

								// Determine how far the "write" buffer pointer is ahead of the "read" buffer pointer.
								int WriteAheadOfPlay = 0;
								lock (this.SoundBuffer) {
									WriteAheadOfPlay = this.SoundBufferPosition - this.SoundBuffer.CurrentPlayPosition;
								}
								while (WriteAheadOfPlay < 0) WriteAheadOfPlay += SoundBufferSize;

								if (SoundBufferCorrectionDirection == 0) {
									// If the write pointer is less than third of a buffer away, we're reading faster than writing.
									if ((WriteAheadOfPlay / 4) < (SoundBufferSampleCount / 3)) {
										Debug.WriteLine("Sound glitch (too slow) at " + DateTime.Now.TimeOfDay);
										SoundBufferCorrectionDirection = +1;
									}
									// If the write pointer is over two thirds of a buffer away, we're writing faster than reading.
									if ((WriteAheadOfPlay / 4) > ((SoundBufferSampleCount * 2) / 3)) {
										Debug.WriteLine("Sound glitch (too fast) at " + DateTime.Now.TimeOfDay);
										SoundBufferCorrectionDirection = -1;
									}
								} else if (SoundBufferCorrectionDirection > 0) {
									// We're speeding up:
									if ((WriteAheadOfPlay / 4) > (SoundBufferSampleCount / 2) - SamplesPerFrame) {
										SoundBufferCorrectionDirection = 0;
									}
								} else if (SoundBufferCorrectionDirection < 0) {
									// We're slowing down:
									if ((WriteAheadOfPlay / 4) < (SoundBufferSampleCount / 2) + SamplesPerFrame) {
										SoundBufferCorrectionDirection = 0;
									}
								}
								FramesOfSoundToGenerate += SoundBufferCorrectionDirection;

							

								// Generate samples as appropriate.
								if (FramesOfSoundToGenerate > 0) {
									short[] Buffer = new short[SamplesPerFrame * 2 * FramesOfSoundToGenerate];
									this.Emulator.Sound.CreateSamples(Buffer);
#if EMU2413
									if (this.Emulator.FmSoundEnabled) {
										var FmBuffer = new short[Buffer.Length];
										this.Emulator.FmSound.GenerateSamples(FmBuffer);
										for (int i = 0; i < FmBuffer.Length; ++i) {
											Buffer[i] = (short)(Buffer[i] + FmBuffer[i] * 2);
										}
									}
#endif
									// Convert 16-bit samples to 8-bit bytes of data.
									for (int i = 0; i < Buffer.Length; i += SamplesPerFrame * 2) {
										for (int j = 0; j < SamplesPerFrame * 2; ++j) {
											InternalSoundBuffer[this.SoundBufferPosition++] = (byte)Buffer[i + j];
											InternalSoundBuffer[this.SoundBufferPosition++] = (byte)(Buffer[i + j] >> 8);
										}
										if (this.SoundBufferPosition >= SoundBufferSize) this.SoundBufferPosition = 0;
									}
									lock (this.SoundBuffer) {
										this.SoundBuffer.Write(this.InternalSoundBuffer, 0, LockFlags.EntireBuffer);
									}
								}
							}
							this.RepaintVideo(false);
						}
						this.Dumper.Render();
					} else {
						this.RepaintVideo(true);
					}
				}
			}
		}

		#endregion

		#region Video Output / Window State

		private void RepaintVideo(bool present) {
			var BackdropColour = Color.FromArgb(unchecked((int)0xFF000000 | Emulator.Video.LastBackdropColour));

			// Send the current frame to the renderer.
			var Video = this.Emulator.Video;
			this.Dumper.BackgroundColour = Color.FromArgb(Video.LastBackdropColour);
			if (Video.LastCompleteFrameWidth > 0 && Video.LastCompleteFrameHeight > 0) {
				var Eye = Video.LastOpenGlassesShutter == Emulator.GlassesShutter.Left ? PixelDumper3D.Eye.Left : PixelDumper3D.Eye.Right;
				this.Dumper.SetImage(Eye, Video.LastCompleteFrame, Video.LastCompleteFrameWidth, Video.LastCompleteFrameHeight);
				this.Dumper.SetBackdrop(Eye, Video.LastCompleteBackdrop, Video.LastCompleteFrameHeight);
			}

			// Work out how recently the eye changed.
			if (Video.LastOpenGlassesShutter != this.LastEye) {
				this.LastEye = Video.LastOpenGlassesShutter;
				this.FramesSinceEyeWasUpdated = 0;
			} else {
				if (this.IsLiveFrame) {
					++this.FramesSinceEyeWasUpdated;
					if (this.FramesSinceEyeWasUpdated > 100) this.FramesSinceEyeWasUpdated = 100;
					this.IsLiveFrame = false;
				}
			}

			if (this.FramesSinceEyeWasUpdated < 4 && this.Emulator.Family == HardwareFamily.MasterSystem && this.Emulator.Video.ResizingMode == BeeDevelopment.Sega8Bit.Hardware.VideoDisplayProcessor.ResizingModes.Normal) {
				// If we have received a change in eye recently, enable a 3D mode.
				if (this.Dumper.DisplayMode != this.ThreeDeeDisplayMode) this.Dumper.DisplayMode = this.ThreeDeeDisplayMode;
			} else {
				// If we haven't received a change in eye recently, disable the 3D mode.
				if (this.Dumper.DisplayMode != PixelDumper3D.StereoscopicDisplayMode.MostRecentEyeOnly) this.Dumper.DisplayMode = PixelDumper3D.StereoscopicDisplayMode.MostRecentEyeOnly;
			}

			// Repaint.
			if (present) {
				this.Dumper.Render();
				this.RenderPanel.BackColor = BackdropColour;
			}
		}

		FormWindowState LastWindowState = FormWindowState.Normal;
		Size LastClientSize;


		private bool TogglingFullScreen = false;
		private void MainForm_Resize(object sender, EventArgs e) {

			
			if (this.WindowState == FormWindowState.Maximized || this.LastWindowState == FormWindowState.Maximized) {
				this.Dumper.RecreateDevice();
				this.LastWindowState = this.WindowState;
			}

			if (!this.TogglingFullScreen) {

				if (this.WindowState == FormWindowState.Normal && this.FormBorderStyle == FormBorderStyle.Sizable) {
					this.LastClientSize = this.ClientSize;
				}
	
				if (this.WindowState == FormWindowState.Maximized && this.FormBorderStyle == FormBorderStyle.Sizable) {
					this.TogglingFullScreen = true;
					this.FormBorderStyle = FormBorderStyle.None;
					this.Menus.Visible = false;
					this.Status.Visible = false;
					this.Visible = false;
					this.WindowState = FormWindowState.Normal;
					this.WindowState = FormWindowState.Maximized;
					this.Visible = true;
					this.RenderPanel.SendToBack();
					this.BringToFront();
					this.CursorHider.Start();
					this.TogglingFullScreen = false;
				}

				if (this.WindowState == FormWindowState.Normal && this.FormBorderStyle == FormBorderStyle.None) {
					this.TogglingFullScreen = true;
					this.Menus.Visible = true;
					this.Status.Visible = true;
					this.FormBorderStyle = FormBorderStyle.Sizable;
					this.ShowIcon = false;
					this.ShowIcon = true;
					this.RenderPanel.BringToFront();
					this.ShowCursor();
					this.CursorHider.Stop();
					this.ClientSize = this.LastClientSize;
					this.MainForm_ResizeEnd(sender, e);
					this.TogglingFullScreen = false;
				}
			}

			this.RepaintVideo(true);
		}

		private void MainForm_Move(object sender, EventArgs e) {
			if (this.Dumper != null) {
				switch (this.Dumper.DisplayMode) {
					case PixelDumper3D.StereoscopicDisplayMode.RowInterleaved:
					case PixelDumper3D.StereoscopicDisplayMode.ColumnInterleaved:
					case PixelDumper3D.StereoscopicDisplayMode.ChequerboardInterleaved:
						this.Dumper.Render();
						break;
				}
			}
		}

		private void MainForm_ResizeBegin(object sender, EventArgs e) {
			if (!this.SoundMuted) this.SoundBuffer.Stop();
		}

		private void MainForm_ResizeEnd(object sender, EventArgs e) {
			this.Dumper.RecreateDevice();
			if (!this.SoundMuted) this.StartPlayingSound();
		}

		private void ToggleFullScreenMenu_Click(object sender, EventArgs e) {
			if (this.WindowState == FormWindowState.Normal) {
				this.WindowState = FormWindowState.Maximized;
			} else if (this.WindowState == FormWindowState.Maximized) {
				this.WindowState = FormWindowState.Normal;
			}
		}

		private Point LastMouseLocation;

		private void RenderPanel_MouseMove(object sender, MouseEventArgs e) {

			// Check the amount the mouse has moved to prevent accidental 1 pixel "jitters" from showing the cursor.
			double DistanceMovedSquared = Math.Pow(e.X - LastMouseLocation.X, 2.0d) + Math.Pow(e.Y - LastMouseLocation.Y, 2.0d);
			LastMouseLocation = e.Location;

			if (this.WindowState == FormWindowState.Maximized && DistanceMovedSquared > (this.CursorVisible ? 0 : 64)) {
				this.Menus.Visible = e.Y < this.Menus.ClientSize.Height;
				this.Status.Visible = e.Y >= this.RenderPanel.ClientSize.Height - this.Status.ClientSize.Height;
				this.ShowCursor();
				if (!(this.Menus.Visible || this.Status.Visible)) {
					this.CursorHiderTicksUntilHide = 10;
					this.CursorHider.Start();
				} else {
					this.CursorHider.Stop();
				}
			}
		}

		private void RenderPanel_DoubleClick(object sender, EventArgs e) {
			this.ToggleFullScreenMenu.PerformClick();
		}

		#endregion

		#region Cursor Hiding/Showing

		bool CursorVisible = true;

		/// <summary>
		/// Shows the mouse cursor.
		/// </summary>
		private void ShowCursor() {
			if (this.CursorVisible) return;
			this.CursorVisible = true;
			Cursor.Show();
		}

		/// <summary>
		/// Hides the mouse cursor.
		/// </summary>
		private void HideCursor() {
			if (!this.CursorVisible) return;
			this.CursorVisible = false;
			Cursor.Hide();
		}

		/// <summary>
		/// Hides the mouse after a delay.
		/// </summary>
		private void CursorHider_Tick(object sender, EventArgs e) {
			if (--this.CursorHiderTicksUntilHide < 0) {
				this.HideCursor();
				this.CursorHider.Stop();
			}
		}
		private int CursorHiderTicksUntilHide = 1;

		#endregion

		#region Sound Output

		private SecondarySoundBuffer SoundBuffer;

		private const int SoundBufferSampleCount = 4410 * 2;
		private const int SoundBufferSize = SoundBufferSampleCount * 2 * 2;
		private int SoundBufferPosition = (SoundBufferSampleCount / 2) * 2 * 2;
		private int SoundBufferCorrectionDirection = 0;

		private byte[] InternalSoundBuffer;

		private void InitialiseSound() {

			Program.DS.SetCooperativeLevel(this.Handle, CooperativeLevel.Priority);

			var Format = new WaveFormat() {
				Channels = 2,
				BitsPerSample = 16,
				FormatTag = WaveFormatTag.Pcm,
				SamplesPerSecond = 44100,
				BlockAlignment = 4,
				AverageBytesPerSecond = 44100 * 2 * 2
			};

			var Description = new SoundBufferDescription() {
				Format = Format,
				Flags = BufferFlags.GlobalFocus | BufferFlags.Software,
				SizeInBytes = SoundBufferSize,
			};

			this.SoundBuffer = new SecondarySoundBuffer(Program.DS, Description);
			this.InternalSoundBuffer = new byte[SoundBufferSize];

			if (!this.SoundMuted) this.StartPlayingSound();

		}

		private void StartPlayingSound() {
			this.InternalSoundBuffer = new byte[this.InternalSoundBuffer.Length];
			this.SoundBufferPosition = 0;
			this.SoundBuffer.CurrentPlayPosition = 0;
			this.SoundBuffer.Play(0, PlayFlags.Looping);
		}

		private void DisposeSound() {
			if (this.SoundBuffer != null && !this.SoundBuffer.Disposed) {
				this.SoundBuffer.Dispose();
				this.SoundBuffer = null;
			}
		}

		private bool SoundMuted = false;

		#endregion

		#region Input

		private void ReinitialiseInput(string profileDirectory) {
			this.Input = new InputManager(profileDirectory);
			this.KeyboardInput = new KeyboardInputSource(Input);
			this.Input.Sources.Add(this.KeyboardInput);
			
			for (int i = 0; i < 4; i++) {
				var C = new SlimDX.XInput.Controller((SlimDX.XInput.UserIndex)i);
				if (C.IsConnected) this.Input.Sources.Add(new XInputSource(this.Input, (SlimDX.XInput.UserIndex)i));
			}
	
			this.Input.Sources.AddRange(Array.ConvertAll(new JoystickInput.JoystickCollection(Properties.Settings.Default.InputSkipDuplicatedXInputJoysticks).Joysticks, J => new JoystickInputSource(Input, J)));			
			this.Input.ReloadSettings();
		}

		protected override void OnKeyDown(KeyEventArgs e) {
			this.KeyboardInput.KeyChange(e, true);
		}

		protected override void OnKeyUp(KeyEventArgs e) {
			this.KeyboardInput.KeyChange(e, false);
		}

		protected override bool IsInputKey(Keys keyData) {
			return this.KeyboardInput.IsInputKey(keyData);
		}

		// Process window messages to check for Alt+Space (ie, window menu) problems.
		protected override void WndProc(ref Message m) {
			switch (m.Msg) {
				case 0x112: // WM_SYSCOMMAND
					switch ((int)m.WParam & 0xFFF0) {
						case 0xF100: // SC_KEYMENU
							m.Result = IntPtr.Zero;
							break;
						default:
							base.WndProc(ref m);
							break;
					}
					break;
				default:
					base.WndProc(ref m);
					break;
			}			
		}

		#endregion

		#region Misc. Menus

		private void ExitMenu_Click(object sender, EventArgs e) {
			this.Close();
		}

		#endregion

		#region ROM Loading

		private void FileMenu_DropDownOpening(object sender, EventArgs e) {
			this.StartStopRecordingVgmMenu.Text = this.Recorder == null ? "Start recording &VGM..." : "Stop recording &VGM";
		}

		private void UpdateFormTitle(string filename) {
			string Name = "";
			
			RomInfo Info = null;

			if (this.Emulator.Bios.Memory != null) Info = this.Identifier.GetRomInfo(this.Emulator.Bios.Memory.Crc32);
			if (this.Emulator.ExpansionSlot.Memory != null) Info = this.Identifier.GetRomInfo(this.Emulator.ExpansionSlot.Memory.Crc32) ?? Info;
			if (this.Emulator.CardSlot.Memory != null) Info = this.Identifier.GetRomInfo(this.Emulator.CardSlot.Memory.Crc32) ?? Info;
			if (this.Emulator.CartridgeSlot.Memory != null) Info = this.Identifier.GetRomInfo(this.Emulator.CartridgeSlot.Memory.Crc32) ?? Info;

			if (Info != null) Name = Info.Title;

			if (!string.IsNullOrEmpty(Name)) Name += " - ";
			this.Text = Name + Application.ProductName;
		}

		/// <summary>
		/// Override any automatic settings (eg region) with user-defined ones.
		/// </summary>
		private void OverrideAutomaticSettings(RomInfo romInfo) {
			if (!Properties.Settings.Default.OptionRegionAutomatic) {
				this.Emulator.Region = Properties.Settings.Default.OptionRegionJapanese ? BeeDevelopment.Sega8Bit.Region.Japanese : BeeDevelopment.Sega8Bit.Region.Export;
			}
			if (!Properties.Settings.Default.OptionVideoStandardAutomatic) {
				this.Emulator.Video.System = Properties.Settings.Default.OptionVideoStandardNtsc ? BeeDevelopment.Sega8Bit.Hardware.VideoDisplayProcessor.VideoSystem.Ntsc : BeeDevelopment.Sega8Bit.Hardware.VideoDisplayProcessor.VideoSystem.Pal;
			}
			if (romInfo != null) {
				if (romInfo.Model == HardwareModel.GameGearMasterSystem && !Properties.Settings.Default.OptionSimulateGameGearLcdScaling) {
					this.Emulator.Video.SetCapabilitiesByModelAndVideoSystem(HardwareModel.MasterSystem2, this.Emulator.Video.System);
				}
			}
		}

		/// <summary>
		/// Quick-load "something" (ROM, VGM, save state etc).
		/// </summary>
		/// <param name="filename"></param>
		private void QuickLoad(string filename) {
			switch (Path.GetExtension(filename).ToLowerInvariant()) {
				case ".cogstate":
					this.LoadState(filename);
					break;
				case ".vgm":
				case ".vgz":
					this.LoadVgm(filename);
					break;
				default:
					this.QuickLoadRom(filename);
					break;
			}
		}

		/// <summary>
		/// Quick-load a ROM.
		/// </summary>
		/// <param name="filename">The name of the ROM file to quick-load.</param>
		private void QuickLoadRom(string filename) {
			this.QuickLoad(filename, true);
		}

		/// <summary>
		/// Quick-load a ROM.
		/// </summary>
		/// <param name="filename">The name of the ROM file to quick-load.</param>
		/// <param name="addToRecentFileList">True (default) to add to the MRU, false to skip it.</param>
		private void QuickLoad(string filename, bool addToRecentFileList) {
			
			string Filename = filename;

			RomInfo LoadingRomInfo = null;
			try {
				LoadingRomInfo = this.Identifier.QuickLoadEmulator(ref Filename, this.Emulator);
			} catch (Exception ex) {
				MessageBox.Show(this, ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			this.Dumper.RecreateDevice();

			this.UpdateFormTitle(Filename);

			if (addToRecentFileList) {
				this.AddRecentFile(filename);
			}

			if (LoadingRomInfo != null) {

				this.AddMessage(Properties.Resources.Icon_Information, LoadingRomInfo.Title);
				if (!string.IsNullOrEmpty(LoadingRomInfo.Author)) this.AddMessage(Properties.Resources.Icon_User, LoadingRomInfo.Author);

				switch (LoadingRomInfo.Type) {
					case RomInfo.RomType.HeaderedFootered:
						if (LoadingRomInfo.FooterSize > 0) this.AddMessage(Properties.Resources.Icon_Exclamation, "Footered");
						if (LoadingRomInfo.HeaderSize > 0) this.AddMessage(Properties.Resources.Icon_Exclamation, "Headered");
						break;
					case RomInfo.RomType.Overdumped:
						this.AddMessage(Properties.Resources.Icon_Exclamation, "Overdumped");
						break;
					case RomInfo.RomType.Translation:
						this.AddMessage(Properties.Resources.Icon_CommentEdit, "Translation");
						break;
					case RomInfo.RomType.Bios:
						this.AddMessage(Properties.Resources.Icon_Lightning, "BIOS");
						break;
					case RomInfo.RomType.Bad:
						this.AddMessage(Properties.Resources.Icon_Exclamation, "Fixable errors in dump");
						break;
					case RomInfo.RomType.VeryBad:
						this.AddMessage(Properties.Resources.Icon_Error, "Unusable dump");
						break;
					case RomInfo.RomType.Demo:
						this.AddMessage(Properties.Resources.Icon_House, "Homebrew");
						break;
					case RomInfo.RomType.Hack:
						this.AddMessage(Properties.Resources.Icon_Wrench, "Hack");
						break;
				}

				switch (LoadingRomInfo.Country) {
					case Country.Japan: this.AddMessage(Properties.Resources.Flag_JP, "Japan"); break;
					case Country.Brazil: this.AddMessage(Properties.Resources.Flag_BR, "Brazil"); break;
					case Country.UnitedStates: this.AddMessage(Properties.Resources.Flag_US, "United States"); break;
					case Country.Korea: this.AddMessage(Properties.Resources.Flag_KR, "Korea"); break;
					case Country.France: this.AddMessage(Properties.Resources.Flag_FR, "France"); break;
					case Country.Spain: this.AddMessage(Properties.Resources.Flag_ES, "Spain"); break;
					case Country.Germany: this.AddMessage(Properties.Resources.Flag_DE, "Germany"); break;
					case Country.Italy: this.AddMessage(Properties.Resources.Flag_IT, "Italy"); break;
					case Country.England: this.AddMessage(Properties.Resources.Flag_EN, "England"); break;
					case Country.NewZealand: this.AddMessage(Properties.Resources.Flag_NZ, "New Zealand"); break;
				}
			}

			this.OverrideAutomaticSettings(LoadingRomInfo);

		}

		private void QuickLoadRomMenu_Click(object sender, EventArgs e) {
			if (!string.IsNullOrEmpty(Properties.Settings.Default.StoredPathQuickLoad) && Directory.Exists(Properties.Settings.Default.StoredPathQuickLoad)) {
				this.OpenRomDialog.InitialDirectory = Properties.Settings.Default.StoredPathQuickLoad;
			}
			if (this.OpenRomDialog.ShowDialog(this) == DialogResult.OK) {
				Properties.Settings.Default.StoredPathQuickLoad = Path.GetDirectoryName(this.OpenRomDialog.FileName);
				this.QuickLoadRom(this.OpenRomDialog.FileName);
			}
		}

		private void AdvancedLoadMenu_Click(object sender, EventArgs e) {
			var RomLoadDialog = new AdvancedRomLoadDialog();
			if (RomLoadDialog.ShowDialog(this) == DialogResult.OK) {


				this.Emulator.RemoveAllMedia();
				this.Emulator.ResetAll();

				RomInfo LoadingRomInfo = null;

				if (File.Exists(RomLoadDialog.CartridgeFileName)) {
					string CartridgeName = RomLoadDialog.CartridgeFileName;
					LoadingRomInfo = this.Identifier.QuickLoadEmulator(ref CartridgeName, this.Emulator);
					
					if (File.Exists(RomLoadDialog.CartridgePatchFileName)) {
						try {
							string s = RomLoadDialog.CartridgeFileName;
							var SourceStream = new MemoryStream(ZipLoader.FindRom(ref s));
							s = RomLoadDialog.CartridgePatchFileName;
							var PatchStream = new MemoryStream(ZipLoader.FindRom(ref s));
							Patch.ApplyPatch(SourceStream, PatchStream);
							this.Emulator.CartridgeSlot.Memory = this.Identifier.CreateMapper(SourceStream.ToArray());
						} catch (Exception ex) {
							MessageBox.Show(this, ex.Message, "Cartridge ROM Patch", MessageBoxButtons.OK, MessageBoxIcon.Error);
						}
					}

				}

				if (File.Exists(RomLoadDialog.BiosFileName)) {
					string BiosName = RomLoadDialog.BiosFileName;
					this.Emulator.Bios.Memory = this.Identifier.CreateMapper(this.Identifier.LoadAndFixRomData(ref BiosName));
					if (this.Emulator.Bios.Memory is Shared1KBios) {
						((Shared1KBios)this.Emulator.Bios.Memory).SharedMapper = this.Emulator.CartridgeSlot.Memory;
						if (Properties.Settings.Default.OptionSimulateGameGearLcdScaling) this.Emulator.Video.SetCapabilitiesByModelAndVideoSystem(HardwareModel.GameGearMasterSystem, this.Emulator.Video.System);
						this.Emulator.HasGameGearPorts = true;
						this.Emulator.RespondsToGameGearPorts = LoadingRomInfo != null && LoadingRomInfo.Model == HardwareModel.GameGear;
					}
					this.Emulator.Bios.Enabled = true;
					this.Emulator.CartridgeSlot.Enabled = false;
				}

				this.OverrideAutomaticSettings(LoadingRomInfo);

				this.Dumper.RecreateDevice();

				this.UpdateFormTitle(null);
			}
		}

		#endregion		

		#region Screenshot

		/// <summary>
		/// Takes a screenshot and copies it to the clipboard.
		/// </summary>
		/// <returns>True if the screenshot was taken succesfully, false otherwise.</returns>
		public bool TakeScreenshot() {

			var ScreenshotBitmap = this.GetScreenshotBitmap();
			if (ScreenshotBitmap != null) {
				try {
					// Store in clipboard.
					Clipboard.SetData(DataFormats.Bitmap, ScreenshotBitmap);
					return true;
				} catch {
					return false;
				} finally {
					ScreenshotBitmap.Dispose();
				}
			} else {
				return false;
			}
		}

		/// <summary>
		/// Gets a screenshot of the current display as a <see cref="Bitmap"/>.
		/// </summary>
		/// <returns>A <see cref="Bitmap"/> if one was recorded, false otherwise.</returns>
		private Bitmap GetScreenshotBitmap() {

			// Quick sanity check that a frame has been generated.
			if (this.Emulator.Video.LastCompleteFrameHeight + this.Emulator.Video.LastCompleteFrameWidth <= 0) return null;

			try {
				// Use a temporary Bitmap to store the screenshot image.
				var ScreenshotImage = new Bitmap(this.Emulator.Video.LastCompleteFrameWidth, this.Emulator.Video.LastCompleteFrameHeight);

				// Lock, copy, unlock.
				var LockedData = ScreenshotImage.LockBits(new Rectangle(0, 0, ScreenshotImage.Width, ScreenshotImage.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
				try {
					Marshal.Copy(this.Emulator.Video.LastCompleteFrame, 0, LockedData.Scan0, ScreenshotImage.Width * ScreenshotImage.Height);
				} finally {
					ScreenshotImage.UnlockBits(LockedData);
				}


				// Success!
				return ScreenshotImage;
			} catch {

				// If anything goes wrong, return false.
				return null;
			}
		}

		private void CopyScreenshotMenu_Click(object sender, EventArgs e) {
			this.TakeScreenshot();
		}

		#endregion

		#region Messages

		/// <summary>
		/// Storage for the messages.
		/// </summary>
		private Queue<KeyValuePair<Image, string>> Messages = new Queue<KeyValuePair<Image, string>>();

		/// <summary>
		/// Add a popup message.
		/// </summary>
		/// <param name="icon">The icon of the message.</param>
		/// <param name="message">The text of the message.</param>
		private void AddMessage(Image icon, string message) {
			this.Messages.Enqueue(new KeyValuePair<Image, string>(icon, message));
			if (!this.MessageTicker.Enabled) {
				this.MessageTicker.Enabled = true;
				this.MessageTicker_Tick(null, null);
			}
		}

		private void MessageTicker_Tick(object sender, EventArgs e) {
			lock (this.Messages) {

				if (this.MessageStatus.Image != null) {
					this.MessageStatus.Image.Dispose();
					this.MessageStatus.Image = null;
				}
				this.MessageStatus.Text = "";

				if (this.Messages.Count == 0) {
					this.MessageTicker.Enabled = false;
				} else {
					var Message = this.Messages.Dequeue();
					this.MessageStatus.Image = Message.Key;
					this.MessageStatus.Text = Message.Value.Replace("&", "&&");
				}
			}
		}

		#endregion

		#region Game Genie

		private void GameGenieMenu_DropDownOpening(object sender, EventArgs e) {
			this.GameGenieEnabledMenu.Checked = this.Emulator.Cheats.Enabled;
		}

		private void GameGenieEnabledMenu_Click(object sender, EventArgs e) {
			this.Emulator.Cheats.Enabled ^= true;
		}

		private void GameGenieEditMenu_Click(object sender, EventArgs e) {
			new GameGenieEditor(this.Emulator.Cheats).ShowDialog(this);
		}

		#endregion

		#region Settings

		private void CustomiseControlsToolStripMenuItem_Click(object sender, EventArgs e) {
			new ControlEditor(this.Input).ShowDialog(this);
			this.Input.ReloadSettings();
		}

		private void SetControllerProfile_Click(object sender, EventArgs e) {
			this.ReinitialiseInput((string)((ToolStripMenuItem)sender).Tag);
		}

		private void ControllerProfileMenu_DropDownOpening(object sender, EventArgs e) {
			foreach (ToolStripMenuItem SubItem in this.ControllerProfileMenu.DropDownItems) {
				SubItem.Image = (this.Input.ProfileDirectory == (string)SubItem.Tag) ? Properties.Resources.Icon_Bullet_Black : null;
			}
		}

		#region 3D Glasses

		private void ThreeDeeGlassesAdvancedOptions_DropDownOpening(object sender, EventArgs e) {
			this.ThreeDeeGlassesToggleFirstEye.Text = (Properties.Settings.Default.Option3DGlassesLeftEyeFirst ? "Left" : "Right") + " &eye first (interleaved)";
			// Left eye filter preview colour:
			if (this.ThreeDeeGlassesLeftFilterColour.Image != null) this.ThreeDeeGlassesLeftFilterColour.Image.Dispose();
			this.ThreeDeeGlassesLeftFilterColour.Image = CreateColouredPreviewIcon(Properties.Settings.Default.Option3DGlassesLeftFilterColour);
			// Right eye filter preview colour:
			if (this.ThreeDeeGlassesRightFilterColour.Image != null) this.ThreeDeeGlassesRightFilterColour.Image.Dispose();
			this.ThreeDeeGlassesRightFilterColour.Image = CreateColouredPreviewIcon(Properties.Settings.Default.Option3DGlassesRightFilterColour);
		}

		private void ThreeDeeGlassesToggleFirstEye_Click(object sender, EventArgs e) {
			this.Dumper.FirstInterleavedEye = (Properties.Settings.Default.Option3DGlassesLeftEyeFirst ^= true) ? PixelDumper3D.Eye.Left : PixelDumper3D.Eye.Right;
		}

		private Bitmap CreateColouredPreviewIcon(int colour) {
			return CreateColouredPreviewIcon(Color.FromArgb(0xFF, Color.FromArgb(colour)));
		}
		private Bitmap CreateColouredPreviewIcon(Color colour) {
			var Result = new Bitmap(16, 16, PixelFormat.Format32bppArgb);
			using (var G = Graphics.FromImage(Result)) {
				G.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
				G.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
				var DrawingRegion = new Rectangle(2, 2, 12, 12);
				G.FillRectangle(Brushes.Black, DrawingRegion);
				DrawingRegion.Inflate(-1, -1);
				using (var B = new SolidBrush(colour)) {
					G.FillRectangle(B, DrawingRegion);
				}
			}
			return Result;
		}

		private void ThreeDeeGlassesFilterColour_Click(object sender, EventArgs e) {
			bool IsLeftEye = sender == this.ThreeDeeGlassesLeftFilterColour;
			this.ColourDialog.Color = Color.FromArgb(0xFF, Color.FromArgb(IsLeftEye ? Properties.Settings.Default.Option3DGlassesLeftFilterColour : Properties.Settings.Default.Option3DGlassesRightFilterColour));
			if (this.ColourDialog.ShowDialog(this) == DialogResult.OK) {
				if (IsLeftEye) {
					Properties.Settings.Default.Option3DGlassesLeftFilterColour = (this.Dumper.LeftEyeColour = this.ColourDialog.Color).ToArgb();
				} else {
					Properties.Settings.Default.Option3DGlassesRightFilterColour = (this.Dumper.RightEyeColour = this.ColourDialog.Color).ToArgb();
				}
			}
		}

		#endregion

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e) {
			if (this.Recorder != null) {
				this.StartStopRecordingVgmMenu.PerformClick();
			}
			try {
				Properties.Settings.Default.Save();
			} catch (Exception ex) {
				MessageBox.Show(this, "Error saving settings - " + ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
			base.OnClosing(e);
		}

		#endregion

		#region Focus

		protected override void OnLostFocus(EventArgs e) {
			foreach (Form F in Application.OpenForms) {
				if (F is DebugConsole && F.Focused) return;
			}
			this.Paused = true;
			if (!this.SoundMuted) this.SoundBuffer.Stop();
			this.Input.ReleaseAll();
			this.ShowCursor();
			this.CursorHider.Stop();
			base.OnLostFocus(e);
		}

		protected override void OnGotFocus(EventArgs e) {
			this.Paused = false;
			if (!this.SoundMuted) this.StartPlayingSound();
			this.OnMouseMove(new MouseEventArgs(MouseButtons.None, 0, -10, +10, 0));
			this.OnMouseMove(new MouseEventArgs(MouseButtons.None, 0, +10, -10, 0));
			base.OnGotFocus(e);
		}

		#endregion

		#region Help

		private void AboutMenu_Click(object sender, EventArgs e) {
			new About().ShowDialog(this);
		}

		private void GoToUrl(string url) {
			try {
				Process.Start(url);
			} catch (Exception ex) {
				MessageBox.Show(this, "Please visit " + url + " in your browser." + Environment.NewLine + "(Error: " + ex.Message + ")", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		private void BugReportMenu_Click(object sender, EventArgs e) {
			GoToUrl(Properties.Settings.Default.UrlBugReport);
		}

		#endregion

		#region Options

		private void OptionsMenu_DropDownOpening(object sender, EventArgs e) {
			this.SimulateGameGearLcdMenu.Checked = Properties.Settings.Default.OptionSimulateGameGearLcdScaling;
			this.LinearInterpolationMenu.Checked = Properties.Settings.Default.OptionLinearInterpolation;
			this.MaintainAspectRatioMenu.Checked = Properties.Settings.Default.OptionMaintainAspectRatio;
			if (this.EnableSoundMenu.Image != null) this.EnableSoundMenu.Image.Dispose();
			this.EnableSoundMenu.Image = Properties.Settings.Default.OptionEnableSound ? Properties.Resources.Icon_Sound : Properties.Resources.Icon_SoundMute;
#if EMU2413
			this.EnableFMSoundMenu.Checked = Properties.Settings.Default.OptionEnableFMSound;
#endif
		}

		private void SimulateGameGearLcdMenu_Click(object sender, EventArgs e) {
			Properties.Settings.Default.OptionSimulateGameGearLcdScaling ^= true;
		}


		private void LinearInterpolationMenu_Click(object sender, EventArgs e) {
			Properties.Settings.Default.OptionLinearInterpolation ^= true;
			this.Dumper.MagnificationFilter = Properties.Settings.Default.OptionLinearInterpolation ? SlimDX.Direct3D9.TextureFilter.Linear : SlimDX.Direct3D9.TextureFilter.Point;
		}


		private void EnableSoundMenu_Click(object sender, EventArgs e) {
			Properties.Settings.Default.OptionEnableSound ^= true;
			this.SoundMuted = !Properties.Settings.Default.OptionEnableSound;
			if (this.SoundMuted) {
				this.SoundBuffer.Stop();
			} else {
				this.StartPlayingSound();
			}
		}


		private void MaintainAspectRatioMenu_Click(object sender, EventArgs e) {
			Properties.Settings.Default.OptionMaintainAspectRatio ^= true;
			this.Dumper.ScaleMode = Properties.Settings.Default.OptionMaintainAspectRatio ? PixelDumper3D.ScaleModes.ZoomInside : PixelDumper3D.ScaleModes.Stretch;
		}

#if EMU2413
		private void EnableFMSoundMenu_Click(object sender, EventArgs e) {
			this.Emulator.FmSoundEnabled = Properties.Settings.Default.OptionEnableFMSound ^= true;
		}
#endif

		// 3D glasses drop-down population:
		private void ThreeDeeGlassesMenu_DropDownOpening(object sender, EventArgs e) {
			foreach (object item in ThreeDeeGlassesMenu.DropDownItems) {
				if (item is ToolStripMenuItem) {
					var Tag = ((ToolStripMenuItem)item).Tag;
					if (Tag != null) {
						((ToolStripMenuItem)item).Image = (Tag.ToString() == this.ThreeDeeDisplayMode.ToString()) ? Properties.Resources.Icon_Bullet_Black : null;
					}
				}
			}
		}

		// 3D glasses option selection:
		private void ThreeDeeGlassesOption_Click(object sender, EventArgs e) {
			this.ThreeDeeDisplayMode = (PixelDumper3D.StereoscopicDisplayMode)Enum.Parse(typeof(PixelDumper3D.StereoscopicDisplayMode), ((ToolStripMenuItem)sender).Tag.ToString());
			Properties.Settings.Default.Option3DGlasses = this.ThreeDeeDisplayMode.ToString();
		}

		#endregion

		#region State Saving

		/// <summary>Gets or sets the current quick-save slot.</summary>
		private int QuickSaveSlot { get; set; }

		private string GetQuickSaveFilename(int slot) {
			var BaseSavePath = Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Application.CompanyName), "Cogwheel"), "QuickSaves");
			var RomDirectory = "Default";
			if (this.Emulator.CartridgeSlot.Memory != null) {
				RomDirectory = string.Format("Cartridge.{0:X8}", this.Emulator.CartridgeSlot.Memory.Crc32);
			} else if (this.Emulator.CardSlot.Memory != null) {
				RomDirectory = string.Format("Card.{0:X8}", this.Emulator.CardSlot.Memory.Crc32);
			} else if (this.Emulator.Bios.Memory != null) {
				RomDirectory = string.Format("Bios.{0:X8}", this.Emulator.Bios.Memory.Crc32);
			}
			return Path.Combine(Path.Combine(BaseSavePath, RomDirectory), string.Format("{0}.cogstate", slot));
		}

		private void QuickStateSlotMenu_DropDownOpening(object sender, EventArgs e) {
			this.QuickStateSlotMenu.DropDownItems.Clear();
			for (int i = 0; i < 10; ++i) {
				this.QuickStateSlotMenu.DropDownItems.Add(new ToolStripMenuItem() {
					Text = string.Format("Slot &{0}", i),
					Checked = i == this.QuickSaveSlot,
					Tag = i,
					ForeColor = Color.FromKnownColor(File.Exists(this.GetQuickSaveFilename(i)) ? KnownColor.ControlText : KnownColor.GrayText),
				});
				this.QuickStateSlotMenu.DropDownItems[this.QuickStateSlotMenu.DropDownItems.Count - 1].Click += (SlotSender, SlotEvent) => this.QuickSaveSlot = (int)(((ToolStripMenuItem)SlotSender).Tag);
			}
		}

		private void QuickSaveState() {
			string Filename = this.GetQuickSaveFilename(this.QuickSaveSlot);
			try {
				if (!Directory.Exists(Path.GetDirectoryName(Filename))) Directory.CreateDirectory(Path.GetDirectoryName(Filename));
				this.SaveState(Filename);
				this.AddMessage(Properties.Resources.Icon_Disk, string.Format("Saved state to slot {0}", this.QuickSaveSlot));
			} catch (Exception ex) {
				MessageBox.Show(this, "Could not save state: " + ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void QuickLoadState() {
			if (!File.Exists(this.GetQuickSaveFilename(this.QuickSaveSlot))) {
				System.Media.SystemSounds.Beep.Play();
				return;
			}
			try {
				this.LoadState(this.GetQuickSaveFilename(this.QuickSaveSlot));
				this.AddMessage(Properties.Resources.Icon_TimeGo, string.Format("Loaded state from slot {0}", this.QuickSaveSlot));
			} catch (Exception ex) {
				MessageBox.Show(this, "Could not load state: " + ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}


		private void QuickLoadStateMenu_Click(object sender, EventArgs e) {
			this.QuickLoadState();
		}

		private void QuickSaveStateMenu_Click(object sender, EventArgs e) {
			this.QuickSaveState();
		}

		private void SaveStateMenu_Click(object sender, EventArgs e) {

			if (!string.IsNullOrEmpty(Properties.Settings.Default.StoredPathState) && Directory.Exists(Properties.Settings.Default.StoredPathState)) {
				this.SaveStateDialog.InitialDirectory = Properties.Settings.Default.StoredPathState;
			}

			if (this.SaveStateDialog.ShowDialog(this) == DialogResult.OK) {
				try {
					Properties.Settings.Default.StoredPathState = Path.GetDirectoryName(this.SaveStateDialog.FileName);
					this.SaveState(this.SaveStateDialog.FileName);
				} catch (Exception ex) {
					MessageBox.Show(this, "Could not save state: " + ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private void LoadStateMenu_Click(object sender, EventArgs e) {
			
			if (!string.IsNullOrEmpty(Properties.Settings.Default.StoredPathState) && Directory.Exists(Properties.Settings.Default.StoredPathState)) {
				this.OpenStateDialog.InitialDirectory = Properties.Settings.Default.StoredPathState;
			}

			if (this.OpenStateDialog.ShowDialog(this) == DialogResult.OK) {
				try {
					Properties.Settings.Default.StoredPathState = Path.GetDirectoryName(this.OpenStateDialog.FileName);
					this.LoadState(this.OpenStateDialog.FileName);
				} catch (Exception ex) {
					MessageBox.Show(this, "Could not load state: " + ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private void SaveState(string filename) {
			
			var SavedState = new ZipFile();

			BeeDevelopment.Sega8Bit.Utility.SaveState.Save(this.Emulator, SavedState);

			var ScreenshotBitmap = this.GetScreenshotBitmap();
			if (ScreenshotBitmap != null) {
				try {
					using (var ScreenshotStream = new MemoryStream(2048)) {
						ScreenshotBitmap.Save(ScreenshotStream, ImageFormat.Png);
						SavedState.Add(new ZipFileEntry() {
							Name = "Screenshot.png",
							Comment = "A screenshot of the last video frame in PNG format.",
							Data = ScreenshotStream.ToArray(),
							LastWriteTime = DateTime.Now,
						});
					}
				} finally {
					ScreenshotBitmap.Dispose();
				}
			}

			SavedState.Save(filename);
		}

		private void LoadState(string filename) {
			try {
				var StateFile = ZipFile.FromFile(filename);
				Emulator NewEmulator;
				BeeDevelopment.Sega8Bit.Utility.SaveState.Load(out NewEmulator, StateFile);
				this.Emulator = NewEmulator;
			} finally {
				this.UpdateFormTitle(null);
			}
		}

		private bool LoadVgm(string filename) {
			// Grab the VGM player stub:
			var StubName = Path.Combine(Application.StartupPath, "vgmplayer.stub");
			if (!File.Exists(StubName)) {
				if (MessageBox.Show(this, "To play VGM files you will need to extract vgmplayer.stub from Maxim's VGM Player into Cogwheel's installation directory." + Environment.NewLine + "Would you like to visit Maxim's VGM Player page to download the software?", "Play VGM", MessageBoxButtons.YesNo) == DialogResult.Yes) {
					this.GoToUrl(Properties.Settings.Default.UrlVgmPlayerStub);
				}
				return false;
			}
			byte[] VgmPlayerStub = File.ReadAllBytes(StubName);

			// Load (and decompress) the VGM.
			byte[] SourceVgm = new byte[0];
			try {
				using (var SourceVgmStream = new GZipStream(File.OpenRead(filename), CompressionMode.Decompress)) {
					using (var Reader = new BinaryReader(SourceVgmStream)) {
						var DecompressedData = new List<byte>(1024);
						byte[] Chunk = null;
						do {
							Chunk = Reader.ReadBytes(1024);
							DecompressedData.AddRange(Chunk);
						} while (Chunk.Length > 0);
						SourceVgm = DecompressedData.ToArray();
					}
				}
			} catch (Exception ex) {
				MessageBox.Show(this, "Could not open VGM: " + ex.Message, "Play VGM");
				return false;
			}

			// Check it's a valid VGM file.
			if (SourceVgm.Length < 64 || Encoding.ASCII.GetString(SourceVgm, 0, 4) != "Vgm ") {
				MessageBox.Show(this, Path.GetFileName(this.OpenVgmDialog.FileName) + " is not a valid VGM file.", "Play VGM", MessageBoxButtons.OK);
				return false;
			}

			// Create a temporary file made from the VGM file appended to the VGM player stub:
			string TempFileName = null;
			while (TempFileName == null || File.Exists(TempFileName)) {
				TempFileName = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), "sms"));
			}
			try {
				using (var TempVgmPlayer = new BinaryWriter(File.Create(TempFileName))) {
					TempVgmPlayer.Write(VgmPlayerStub);
					TempVgmPlayer.Write(SourceVgm);
				}
				// Load the ROM:
				this.QuickLoad(TempFileName, false);
				int VgmVersion = BitConverter.ToInt32(SourceVgm, 0x08);
				// Do we have a rate setting?
				if (VgmVersion >= 0x101) {
					this.Emulator.Video.System = BitConverter.ToInt32(SourceVgm, 0x24) == 50 ? BeeDevelopment.Sega8Bit.Hardware.VideoDisplayProcessor.VideoSystem.Pal : BeeDevelopment.Sega8Bit.Hardware.VideoDisplayProcessor.VideoSystem.Ntsc;
				}
				// Do we have periodic noise settings?
				if (VgmVersion >= 0x110) {
					short TappedBits = BitConverter.ToInt16(SourceVgm, 0x28);
					if (TappedBits != 0) this.Emulator.Sound.TappedBits = TappedBits;
					byte ShiftRegisterWidth = SourceVgm[0x2A];
					if (ShiftRegisterWidth != 0) this.Emulator.Sound.ShiftRegisterWidth = ShiftRegisterWidth;
				}
				this.OverrideAutomaticSettings(null);
				return true;
			} finally {
				File.Delete(TempFileName);
			}
		}

		#endregion

		#region Recent Items

		private LinkedList<string> RecentFiles = new LinkedList<string>();

		private void AddRecentFile(string file) {
			var ToRemove = new List<string>();
			foreach (var RecentFile in RecentFiles) {
				if (RecentFile.ToLowerInvariant() == file.ToLowerInvariant()) {
					ToRemove.Add(RecentFile);
				}
			}
			foreach (var RecentFile in ToRemove) {
				RecentFiles.Remove(RecentFile);
			}
			this.RecentFiles.AddFirst(file);

			while (this.RecentFiles.Count > Properties.Settings.Default.OptionMaxMRUEntries && this.RecentFiles.Count > 0) this.RecentFiles.RemoveLast();

			this.SaveRecentItemsToSettings();
		}

		private void LoadRecentItemsFromSettings() {
			this.RecentFiles.Clear();
			if (!string.IsNullOrEmpty(Properties.Settings.Default.StoredPathMRU)) {
				this.RecentFiles = new LinkedList<string>(Properties.Settings.Default.StoredPathMRU.Split('|'));
			}
		}

		private void SaveRecentItemsToSettings() {
			var MRUEntries = new string[this.RecentFiles.Count];
			this.RecentFiles.CopyTo(MRUEntries,0);
			Properties.Settings.Default.StoredPathMRU = string.Join("|", MRUEntries);
		}

		private void RecentRomsMenu_DropDownOpening(object sender, EventArgs e) {
			var ToDispose = new List<IDisposable>();
			foreach (IDisposable RecentItem in this.RecentRomsMenu.DropDownItems) ToDispose.Add(RecentItem);
			this.RecentRomsMenu.DropDownItems.Clear();
			foreach (IDisposable RecentItem in ToDispose) {
				RecentItem.Dispose();
			}

			if (RecentFiles.Count == 0) {
				this.RecentRomsMenu.DropDownItems.Add(new ToolStripMenuItem("(None)") { Enabled = false });
			} else {
				int i = 1;
				foreach (var RecentFile in RecentFiles) {
					this.RecentRomsMenu.DropDownItems.Add(new ToolStripMenuItem("&" + (i++) + " " + Path.GetFileNameWithoutExtension(RecentFile), null, (RecentFileMenu, e2) => this.QuickLoadRom(((ToolStripMenuItem)RecentFileMenu).Tag.ToString())) { Tag = RecentFile });
				}
			}

			
		}

		#endregion

		#region Emulation

		#region Region Settings

		private void EmulationRegionMenu_DropDownOpening(object sender, EventArgs e) {
			this.RegionAutomaticMenu.Checked = Properties.Settings.Default.OptionRegionAutomatic;
			this.RegionJapaneseMenu.Enabled = !this.RegionAutomaticMenu.Checked;
			this.RegionExportMenu.Enabled = !this.RegionAutomaticMenu.Checked;
			this.RegionJapaneseMenu.Image = this.Emulator.Region == BeeDevelopment.Sega8Bit.Region.Japanese ? Properties.Resources.Icon_Bullet_Black : null;
			this.RegionExportMenu.Image = this.Emulator.Region == BeeDevelopment.Sega8Bit.Region.Export ? Properties.Resources.Icon_Bullet_Black : null;
		}

		private void RegionAutomaticMenu_Click(object sender, EventArgs e) {
			Properties.Settings.Default.OptionRegionAutomatic ^= true;
		}

		private void RegionCountry_Click(object sender, EventArgs e) {
			var IsJapanese = sender == this.RegionJapaneseMenu;
			Properties.Settings.Default.OptionRegionJapanese = IsJapanese;
			this.Emulator.Region = IsJapanese ? BeeDevelopment.Sega8Bit.Region.Japanese : BeeDevelopment.Sega8Bit.Region.Export;
		}

		#endregion

		#region Video Settings

		private void DebugVideoMenu_DropDownOpening(object sender, EventArgs e) {
			this.VideoStandardAutomaticMenu.Checked = Properties.Settings.Default.OptionVideoStandardAutomatic;
			this.VideoStandardNtscMenu.Enabled = !this.VideoStandardAutomaticMenu.Checked;
			this.VideoStandardPalMenu.Enabled = !this.VideoStandardAutomaticMenu.Checked;
			this.VideoStandardNtscMenu.Image = this.Emulator.Video.System == BeeDevelopment.Sega8Bit.Hardware.VideoDisplayProcessor.VideoSystem.Ntsc ? Properties.Resources.Icon_Bullet_Black : null;
			this.VideoStandardPalMenu.Image = this.Emulator.Video.System == BeeDevelopment.Sega8Bit.Hardware.VideoDisplayProcessor.VideoSystem.Pal ? Properties.Resources.Icon_Bullet_Black : null;
		}

		private void VideoStandardAutomaticMenu_Click(object sender, EventArgs e) {
			Properties.Settings.Default.OptionVideoStandardAutomatic ^= true;
		}

		private void VideoStandardTypeMenu_Click(object sender, EventArgs e) {
			bool IsNtsc = sender == this.VideoStandardNtscMenu;
			Properties.Settings.Default.OptionVideoStandardNtsc = IsNtsc;
			this.Emulator.Video.System = IsNtsc ? BeeDevelopment.Sega8Bit.Hardware.VideoDisplayProcessor.VideoSystem.Ntsc : BeeDevelopment.Sega8Bit.Hardware.VideoDisplayProcessor.VideoSystem.Pal;
		}

		#endregion

		#endregion

		#region Debug
		
		private void DebugMenu_DropDownOpening(object sender, EventArgs e) {
			this.EmulationVideoBackgroundEnabledMenu.Checked = this.Emulator.Video.BackgroundLayerEnabled;
			this.EmulationVideoSpritesEnabledMenu.Checked = this.Emulator.Video.SpriteLayerEnabled;
		}

		private void SdscDebugConsoleMenu_Click(object sender, EventArgs e) {
			foreach (Form F in Application.OpenForms) {
				if (F is DebugConsole) {
					F.BringToFront();
					return;
				}
			}
			new DebugConsole().Show(this);
		}

		private void BackgroundEnabledMenu_Click(object sender, EventArgs e) {
			this.Emulator.Video.BackgroundLayerEnabled ^= true;
		}

		private void SpritesEnabledMenu_Click(object sender, EventArgs e) {
			this.Emulator.Video.SpriteLayerEnabled ^= true;
		}

		#endregion

		#region VGM Recording/Playback

		private VgmRecorder Recorder = null;
		private bool CompressVgm = false;

		private void StartStopRecordingVgmMenu_Click(object sender, EventArgs e) {
			if (this.Recorder == null) {
				if (!string.IsNullOrEmpty(Properties.Settings.Default.StoredPathVgm) && Directory.Exists(Properties.Settings.Default.StoredPathVgm)) {
					this.SaveVgmDialog.InitialDirectory = Properties.Settings.Default.StoredPathVgm;
				} else {
					this.SaveVgmDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
				}
				if (this.SaveVgmDialog.ShowDialog(this) == DialogResult.OK) {
					try {
						this.CompressVgm = this.SaveVgmDialog.FileName.ToLowerInvariant().EndsWith("gz");
						this.Recorder = new VgmRecorder(File.Create(this.SaveVgmDialog.FileName), this.Emulator);
						this.Recorder.Start();
						Properties.Settings.Default.StoredPathVgm = Path.GetDirectoryName(this.SaveVgmDialog.FileName);
					} catch (Exception ex) {
						MessageBox.Show(this, "Could not start recording: " + ex.Message, "Start recording VGM");
					}
				}
			} else {
				// Stop recording.
				this.Recorder.Stop();
				// Do we need to compress the file?
				if (CompressVgm) {
					this.Recorder.Stream.Seek(0, SeekOrigin.Begin);
					byte[] UncompressedData = new byte[this.Recorder.Stream.Length];
					this.Recorder.Stream.Read(UncompressedData, 0, UncompressedData.Length);
					this.Recorder.Stream.SetLength(0);
					using (var gz = new GZipStream(this.Recorder.Stream, CompressionMode.Compress)) {
						gz.Write(UncompressedData, 0, UncompressedData.Length);
					}
				}
				this.Recorder.Stream.Dispose();
				this.Recorder = null;
			}
		}


		private void PlayVgmMenu_Click(object sender, EventArgs e) {
			

			if (!string.IsNullOrEmpty(Properties.Settings.Default.StoredPathVgm) && Directory.Exists(Properties.Settings.Default.StoredPathVgm)) {
				this.OpenVgmDialog.InitialDirectory = Properties.Settings.Default.StoredPathVgm;
			} else {
				this.OpenVgmDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
			}
			if (this.OpenVgmDialog.ShowDialog(this) == DialogResult.OK) {
				if (this.LoadVgm(this.OpenVgmDialog.FileName)) {
					Properties.Settings.Default.StoredPathVgm = Path.GetDirectoryName(this.OpenVgmDialog.FileName);
				}
			}
		}

		#endregion

		#region Drag-and-Drop

		private void RenderPanel_DragDrop(object sender, DragEventArgs e) {
			string[] DropData;
			if (e.Data.GetDataPresent(DataFormats.FileDrop) && (DropData = (e.Data.GetData(DataFormats.FileDrop) as string[])) != null && DropData.Length == 1) {
				this.QuickLoad(DropData[0]);
				this.Focus();
				this.WindowState = FormWindowState.Normal;
				this.BringToFront();
			}
		}

		private void RenderPanel_DragOver(object sender, DragEventArgs e) {
			string[] DropData;
			if (e.Data.GetDataPresent(DataFormats.FileDrop) && (DropData = (e.Data.GetData(DataFormats.FileDrop) as string[])) != null && DropData.Length == 1) {
				e.Effect = DragDropEffects.Copy;
			}
		}

		#endregion

	}
}