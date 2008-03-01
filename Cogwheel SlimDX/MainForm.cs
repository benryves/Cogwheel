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

			// Load the emulator.
			this.Emulator = new BeeDevelopment.Sega8Bit.Emulator();
			this.Emulator.Cartridge = new BeeDevelopment.Sega8Bit.Mappers.Standard();

			// Load the ROM data (if required).
			string RomDataDir = Path.Combine(Application.StartupPath, "ROM Data");
			this.Identifier = new RomIdentifier();
			if (Directory.Exists(RomDataDir)) {
				try {
					this.Identifier = new RomIdentifier("ROM Data");
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

		void Application_Idle(object sender, EventArgs e) {

			while (AppStillIdle) {
				Thread.Sleep(10);
				this.Input.Poll();
				this.Input.UpdateEmulatorState(this.Emulator);
				this.Emulator.RunFrame();
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

		private void QuickLoadRomMenu_Click(object sender, EventArgs e) {
			if (this.OpenRomDialog.ShowDialog(this) == DialogResult.OK) {

				string Filename = this.OpenRomDialog.FileName;
				byte[] Data;

				HardwareModel Model = HardwareModel.MasterSystem2;

				try {

					Data = ZipLoader.FindRom(ref Filename);
					switch (Path.GetExtension(Filename)) {
						case ".gg":
							Model = HardwareModel.GameGear;
							break;
						case ".sc":
							Model = HardwareModel.SC3000;
							break;
						case ".mv":
						case ".sg":
							Model = HardwareModel.SG1000;
							break;
					}

				} catch (Exception ex) {
					MessageBox.Show(this, ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
					return;
				}


				this.CurrentRomInfo = this.CurrentRomInfo = this.Identifier.GetRomInfo(Data, Filename);

				if (this.CurrentRomInfo != null) {
					Model = this.CurrentRomInfo.Model;
					this.CurrentRomInfo.Fix(ref Data);
				}

				this.Text = (this.CurrentRomInfo != null ? this.CurrentRomInfo.Title : Path.GetFileNameWithoutExtension(Filename)) + " - " + Application.ProductName;

				this.Emulator.ResetAll();

				if (this.CurrentRomInfo != null) {

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

				this.Emulator.Region = Countries.CountryToRegion(this.CurrentRomInfo != null ? this.CurrentRomInfo.Country : Country.None);
				this.Emulator.Cartridge = this.Identifier.CreateCartridgeMapper(Data);

				this.Emulator.SetCapabilitiesByModel(
					(this.Emulator.Region == BeeDevelopment.Sega8Bit.Region.Japanese && Model == HardwareModel.MasterSystem2) ? HardwareModel.MasterSystem : Model
				);
			}
		}

		#endregion		

		#region Screenshot

		/// <summary>
		/// Takes a screenshot and copies it to the clipboard.
		/// </summary>
		/// <returns>True if the screenshot was taken succesfully, false otherwise.</returns>
		public bool TakeScreenshot() {

			// Quick sanity check that a frame has been generated.
			if (this.Emulator.Video.LastCompleteFrameHeight + this.Emulator.Video.LastCompleteFrameWidth <= 0) return false;

			try {
				// Use a temporary Bitmap to store the screenshot image.
				using (var ScreenshotImage = new Bitmap(this.Emulator.Video.LastCompleteFrameWidth, this.Emulator.Video.LastCompleteFrameHeight)) {

					// Lock, copy, unlock.
					var LockedData = ScreenshotImage.LockBits(new Rectangle(0, 0, ScreenshotImage.Width, ScreenshotImage.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
					try {
						Marshal.Copy(this.Emulator.Video.LastCompleteFrame, 0, LockedData.Scan0, ScreenshotImage.Width * ScreenshotImage.Height);
					} finally {
						ScreenshotImage.UnlockBits(LockedData);
					}

					// Store in clipboard.
					Clipboard.SetData(DataFormats.Bitmap, ScreenshotImage);
				}

				// Success!
				return true;
			} catch {

				// If anything goes wrong, return false.
				return false;
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

	}
}
