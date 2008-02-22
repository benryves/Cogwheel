using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using BeeDevelopment.Sega8Bit;
using BeeDevelopment.Sega8Bit.Mappers;
using BeeDevelopment.Sega8Bit.Utility;


namespace CogwheelSlimDX {
	public partial class MainForm : Form {

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
				this.Emulator.RunFrame();
				this.RepaintVideo();
			}
		}
		
		#endregion


		private PixelDumper Dumper;
		private Emulator Emulator;
		private RomIdentifier Identifier;

		private RomInfo CurrentRomInfo;

		public MainForm() {
			InitializeComponent();

			this.Text = Application.ProductName;
			this.Dumper = new PixelDumper(this.RenderPanel);

			this.Emulator = new BeeDevelopment.Sega8Bit.Emulator();
			this.Emulator.Cartridge = new BeeDevelopment.Sega8Bit.Mappers.Standard();

			Application.Idle += new EventHandler(Application_Idle);

			string RomDataDir = Path.Combine(Application.StartupPath, "ROM Data");
			if (Directory.Exists(RomDataDir)) {
				try {
					this.Identifier = new RomIdentifier("ROM Data");
				} catch (Exception ex) {
					MessageBox.Show(this, ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
			}
			
		}


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

		private void OnKeyChange(KeyEventArgs e, bool state) {
			
			if (e.KeyCode == Properties.Settings.Default.KeyUp) {
				this.Emulator.Ports[0].Up.State = state;
				e.Handled = true;
			}
			if (e.KeyCode == Properties.Settings.Default.KeyDown) {
				this.Emulator.Ports[0].Down.State = state;
				e.Handled = true;
			}
			if (e.KeyCode == Properties.Settings.Default.KeyLeft) {
				this.Emulator.Ports[0].Left.State = state;
				e.Handled = true;
			}
			if (e.KeyCode == Properties.Settings.Default.KeyRight) {
				this.Emulator.Ports[0].Right.State = state;
				e.Handled = true;
			}
			if (e.KeyCode == Properties.Settings.Default.KeyTL) {
				this.Emulator.Ports[0].TL.State = state;
				e.Handled = true;
			}
			if (e.KeyCode == Properties.Settings.Default.KeyTR) {
				this.Emulator.Ports[0].TR.State = state;
				e.Handled = true;
			}
			if (e.KeyCode == Properties.Settings.Default.KeyPause && this.Emulator.HasPauseButton) {
				this.Emulator.PauseButton = !state;
				e.Handled = true;
			}
			if (e.KeyCode == Properties.Settings.Default.KeyStart && this.Emulator.HasStartButton) {
				this.Emulator.StartButton = !state;
				e.Handled = true;
			}
			if (e.KeyCode == Properties.Settings.Default.KeyReset && this.Emulator.HasResetButton) {
				this.Emulator.ResetButton = !state;
				e.Handled = true;
			}

			base.OnKeyDown(e);
		}

		protected override void OnKeyDown(KeyEventArgs e) {
			this.OnKeyChange(e, false);
		}
		protected override void OnKeyUp(KeyEventArgs e) {
			this.OnKeyChange(e, true);
		}
		protected override bool IsInputKey(Keys keyData) {
			if (keyData == Properties.Settings.Default.KeyUp) return true;
			if (keyData == Properties.Settings.Default.KeyDown) return true;
			if (keyData == Properties.Settings.Default.KeyLeft) return true;
			if (keyData == Properties.Settings.Default.KeyRight) return true;
			if (keyData == Properties.Settings.Default.KeyTL) return true;
			if (keyData == Properties.Settings.Default.KeyTR) return true;
			if (keyData == Properties.Settings.Default.KeyPause) return true;
			if (keyData == Properties.Settings.Default.KeyStart) return true;
			if (keyData == Properties.Settings.Default.KeyReset) return true;
			return base.IsInputKey(keyData);
		}

		private void ExitMenu_Click(object sender, EventArgs e) {
			this.Close();
		}

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


				this.CurrentRomInfo = this.Identifier.GetRomInfo(Data, Filename);

				if (this.CurrentRomInfo != null) {
					if (this.CurrentRomInfo.RomData != null) {
						Model = this.CurrentRomInfo.RomData.Model;
					}
					this.CurrentRomInfo.Fix(ref Data);
				}

				this.Text = (this.CurrentRomInfo != null ? this.CurrentRomInfo.Name : Path.GetFileNameWithoutExtension(Filename)) + " - " + Application.ProductName;

				this.Emulator.ResetAll();

				this.Emulator.Region = BeeDevelopment.Sega8Bit.Region.Export;
				this.Emulator.Cartridge = this.Identifier.CreateCartridgeMapper(Data);
				this.Emulator.SetCapabilitiesByModel(Model);



			}
		}

		private void PropertiesMenu_Click(object sender, EventArgs e) {
			if (this.CurrentRomInfo != null) {
				new PropertyForm("ROM Properties", this.CurrentRomInfo).Show(this);
			}
		}
	}
}
