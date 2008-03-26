using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using BeeDevelopment.Sega8Bit;
using BeeDevelopment.Sega8Bit.Mappers;
using BeeDevelopment.Sega8Bit.Utility;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.Xml.Serialization;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using BeeDevelopment.Zip;


namespace CogwheelSlimDX {
	public partial class MainForm : Form {

		#region Fields

		// Input.
		private InputManager Input;
		private KeyboardInputSource KeyboardInput;

		// Output.
		private PixelDumper Dumper;

		// Emulator stuff.
		private Emulator Emulator;
		private RomIdentifier Identifier;
		private RomInfo CurrentRomInfo;
		protected bool Paused = false;

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

		public MainForm() {

			InitializeComponent();

			// Set up input devices:
			this.Input = new InputManager();
			this.KeyboardInput = new KeyboardInputSource();
			this.Input.Sources.Add(this.KeyboardInput);
			foreach (var Stick in new JoystickInput.JoystickCollection().Joysticks) this.Input.Sources.Add(new JoystickInputSource(Stick));	
			this.Input.ReloadSettings();
			

			// Create a pixel dumper.
			this.Dumper = new PixelDumper(this.RenderPanel);
			this.Dumper.LinearInterpolation = Properties.Settings.Default.OptionLinearInterpolation;

			// Initialise sound.
			this.InitialiseSound();
			this.SoundMuted = !Properties.Settings.Default.OptionEnableSound;

			// Load the emulator.
			this.Emulator = new BeeDevelopment.Sega8Bit.Emulator();

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

			// Attach render loop handler.
			Application.Idle += new EventHandler(Application_Idle);
			this.Disposed += new EventHandler(MainForm_Disposed);

		}

		void MainForm_Disposed(object sender, EventArgs e) {
			this.DisposeSound();
			if (this.Dumper != null) this.Dumper.Dispose();
		}
		
		#endregion

		#region Render Loop

		[System.Security.SuppressUnmanagedCodeSecurity, DllImport("User32.dll", CharSet = CharSet.Auto)]
		public static extern bool PeekMessage(out Message msg, IntPtr hWnd, UInt32 msgFilterMin, UInt32 msgFilterMax, UInt32 flags);

		bool AppStillIdle {
			get {
				Message msg;
				return !PeekMessage(out msg, IntPtr.Zero, 0, 0, 0);
			}
		}

		// HACK: Refresh rate of the emulator (hard-coded to NTSC 60Hz at the moment.
		int TargetRefreshRate = 60;
		int SystemRefreshRate = PixelDumper.GetCurrentRefreshRate();
		int RefreshStepper = 0;

		void Application_Idle(object sender, EventArgs e) {

			while (AppStillIdle) {
				if (!this.Paused) {
					RefreshStepper -= TargetRefreshRate;
					while (RefreshStepper <= 0) {
						RefreshStepper += SystemRefreshRate;
						this.Emulator.RunFrame();
						short[] Buffer = new short[735 * 2];
						this.Emulator.Sound.CreateSamples(Buffer);
						this.GeneratedSoundSamples.Enqueue(Buffer);
						this.Input.Poll();
						this.Input.UpdateEmulatorState(this.Emulator);
					}
				}
				this.RepaintVideo();
			}
		}

		#endregion

		#region Video Output

		private void RepaintVideo() {
			this.Dumper.Render(this.Emulator.Video.LastCompleteFrame, this.Emulator.Video.LastCompleteFrameWidth, this.Emulator.Video.LastCompleteFrameHeight);
		}

		FormWindowState LastWindowState = FormWindowState.Normal;
		private void MainForm_Resize(object sender, EventArgs e) {
			if (this.WindowState == FormWindowState.Maximized || this.LastWindowState == FormWindowState.Maximized) {
				this.Dumper.ReinitialiseRenderer();
				this.LastWindowState = this.WindowState;
			}
			this.RepaintVideo();
		}

		private void MainForm_ResizeEnd(object sender, EventArgs e) {
			this.Dumper.ReinitialiseRenderer();
		}

		#endregion

		#region Sound Output

		private WaveLib.WaveOutPlayer WaveOutput;

		private int SoundBufferSizeInFrames = 4;

		private Queue<short[]> GeneratedSoundSamples;

		private void InitialiseSound() {
			this.GeneratedSoundSamples = new Queue<short[]>();
			var Format = new WaveLib.WaveFormat (44100, 16, 2);
			this.WaveOutput = new WaveLib.WaveOutPlayer(-1, Format, SoundBufferSizeInFrames * 735 * 4, 3, new WaveLib.BufferFillEventHandler(SoundBufferFiller));
		}

		private void DisposeSound() {
			if (this.WaveOutput != null) {
				this.WaveOutput.Dispose();
				this.WaveOutput = null;
			}
		}

		private bool SoundMuted = false;

		private void SoundBufferFiller(IntPtr data, int size) {
			lock (this.GeneratedSoundSamples) {

				short[] Generated = new short[size / 2];

				if (SoundMuted || Paused) {

					this.GeneratedSoundSamples.Clear();

				} else {

					for (int i = 0; i < Generated.Length; i += 735 * 2) {
						if (this.GeneratedSoundSamples.Count > 0) {
							Array.Copy(this.GeneratedSoundSamples.Dequeue(), 0, Generated, i, 735 * 2);
						} else {
							short[] Temp = new short[735 * 2];
							if (this.Emulator != null) this.Emulator.Sound.CreateSamples(Temp);
							Array.Copy(Temp, 0, Generated, i, 735 * 2);
						}
					}

				}

				Marshal.Copy(Generated, 0, data, size / 2);
				while (this.GeneratedSoundSamples.Count > this.SoundBufferSizeInFrames) this.GeneratedSoundSamples.Dequeue();

			}
		}

		#endregion

		#region Keyboard Input

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

		private void UpdateFormTitle(string filename) {
			string Name = "";
			if (this.CurrentRomInfo == null) {
				if (filename == null) {
					Name = "";
				} else {
					Name = Path.GetFileNameWithoutExtension(filename);
				}
			} else {
				Name = this.CurrentRomInfo.Title;
			}
			if (!string.IsNullOrEmpty(Name)) Name += " - ";
			this.Text = Name + Application.ProductName;
		}

		private void QuickLoadRomMenu_Click(object sender, EventArgs e) {
			if (this.OpenRomDialog.ShowDialog(this) == DialogResult.OK) {

				string Filename = this.OpenRomDialog.FileName;

				try {
					this.CurrentRomInfo = this.Identifier.QuickLoadEmulator(ref Filename, this.Emulator);
				} catch (Exception ex) {
					MessageBox.Show(this, ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				this.UpdateFormTitle(Filename);

				if (this.CurrentRomInfo != null) {

					if (this.CurrentRomInfo.Model == HardwareModel.GameGearMasterSystem && !Properties.Settings.Default.OptionSimulateGameGearLcdScaling) {
						this.Emulator.Video.SetCapabilitiesByModel(HardwareModel.MasterSystem2);
					}

					this.AddMessage(Properties.Resources.Icon_Information, this.CurrentRomInfo.Title);
					if (!string.IsNullOrEmpty(this.CurrentRomInfo.Author)) this.AddMessage(Properties.Resources.Icon_User, this.CurrentRomInfo.Author);

					switch (this.CurrentRomInfo.Type) {
						case RomInfo.RomType.HeaderedFootered:
							if (this.CurrentRomInfo.FooterSize > 0) this.AddMessage(Properties.Resources.Icon_Exclamation, "Footered");
							if (this.CurrentRomInfo.HeaderSize > 0) this.AddMessage(Properties.Resources.Icon_Exclamation, "Headered");
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

					switch (this.CurrentRomInfo.Country) {
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

				
			}
		}

		private void AdvancedLoadMenu_Click(object sender, EventArgs e) {
			var RomLoadDialog = new AdvancedRomLoadDialog();
			if (RomLoadDialog.ShowDialog(this) == DialogResult.OK) {

				this.CurrentRomInfo = null;

				this.Emulator.RemoveAllMedia();
				this.Emulator.ResetAll();

				if (File.Exists(RomLoadDialog.CartridgeFileName)) {
					string CartridgeName = RomLoadDialog.CartridgeFileName;
					this.CurrentRomInfo = this.Identifier.QuickLoadEmulator(ref CartridgeName, this.Emulator);

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
						if (Properties.Settings.Default.OptionSimulateGameGearLcdScaling) this.Emulator.Video.SetCapabilitiesByModel(HardwareModel.GameGearMasterSystem);
						this.Emulator.HasGameGearPorts = true;
						this.Emulator.RespondsToGameGearPorts = this.CurrentRomInfo != null && this.CurrentRomInfo.Model == HardwareModel.GameGear;
					}
					this.Emulator.Bios.Enabled = true;
					this.Emulator.CartridgeSlot.Enabled = false;
				}

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
					this.MessageStatus.Text = Message.Value;
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

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e) {
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
			this.Paused = true;
			base.OnLostFocus(e);
		}

		protected override void OnGotFocus(EventArgs e) {
			this.Paused = false;
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
			if (this.EnableSoundMenu.Image != null) this.EnableSoundMenu.Image.Dispose();
			this.EnableSoundMenu.Image = Properties.Settings.Default.OptionEnableSound ? Properties.Resources.Icon_Sound : Properties.Resources.Icon_SoundMute;
		}

		private void SimulateGameGearLcdMenu_Click(object sender, EventArgs e) {
			Properties.Settings.Default.OptionSimulateGameGearLcdScaling ^= true;
		}


		private void LinearInterpolationMenu_Click(object sender, EventArgs e) {
			Properties.Settings.Default.OptionLinearInterpolation ^= true;
			this.Dumper.LinearInterpolation = Properties.Settings.Default.OptionLinearInterpolation;
		}


		private void EnableSoundMenu_Click(object sender, EventArgs e) {
			Properties.Settings.Default.OptionEnableSound ^= true;
			this.SoundMuted = !Properties.Settings.Default.OptionEnableSound;
		}

		#endregion

		#region State Saving

		private void SaveStateMenu_Click(object sender, EventArgs e) {
			if (this.SaveStateDialog.ShowDialog(this) == DialogResult.OK) {
				try {
					this.SaveState(this.SaveStateDialog.FileName);
				} catch (Exception ex) {
					MessageBox.Show(this, ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private void LoadStateMenu_Click(object sender, EventArgs e) {
			if (this.OpenStateDialog.ShowDialog(this) == DialogResult.OK) {
				try {
					this.LoadState(this.OpenStateDialog.FileName);
				} catch (Exception ex) {
					MessageBox.Show(this, ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private void SaveState(string filename) {
			var SavedState = new ZipFile();
			var Serialiser = new BinaryFormatter();
			using (var StateStream = new MemoryStream(2048)) {
				Serialiser.Serialize(StateStream, this.Emulator);
				SavedState.Add(new ZipFileEntry() {
					Name = "State.bin",
					Comment = "Raw saved state data (as created by BinaryFormatter).",
					Data = StateStream.ToArray(),
					LastWriteTime = DateTime.Now,
				});
			}

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
			var StateFile = ZipFile.FromFile(filename);
			this.CurrentRomInfo = null;
			this.UpdateFormTitle(null);
			lock (this.Emulator) {
				foreach (var Entry in StateFile) {
					if (Entry.Name.ToLowerInvariant() == "state.bin") {
						var Deserialiser = new BinaryFormatter();
						using (var StateStream = new MemoryStream(Entry.Data)) {
							this.Emulator = (Emulator)Deserialiser.Deserialize(StateStream);
						}
					}
				}
			}
		}

		#endregion


	}
}
