using System;
using System.Windows.Forms;
using BeeDevelopment.Sega8Bit.Utility.RomData;

namespace BeeDevelopment.Cogwheel {
	public partial class MainForm : Form {

		private RomIdentifier Identifier;

		public MainForm() {
			InitializeComponent();
			this.Text = Application.ProductName;
			this.MainFormMenus.Renderer = new Asztal.Szótár.NativeToolStripRenderer(this.MainFormMenus);
		}

		private void MainForm_Load(object sender, EventArgs e) {

			this.Identifier = new RomIdentifier(Application.StartupPath);

			Application.Idle += new EventHandler(Application_Idle);
			this.EmulatorHost.LoadKeyMappings();
		}

		void Application_Idle(object sender, EventArgs e) {
			this.RunEmulatorTicks();
		}

		private void ExitMenu_Click(object sender, EventArgs e) {
			this.Close();
		}

		private void OpenMenu_Click(object sender, EventArgs e) {
			this.PromptOpenRom();
		}

		private void EmulatorHost_Load(object sender, EventArgs e) {

		}

		private void JapaneseMenu_Click(object sender, EventArgs e) {
			if (this.EmulatorHost.Emulator != null) this.EmulatorHost.Emulator.IsJapanese = true;
			Properties.Settings.Default.EmulatorIsJapanese = true;
		}

		private void ExportMenu_Click(object sender, EventArgs e) {
			if (this.EmulatorHost.Emulator != null) this.EmulatorHost.Emulator.IsJapanese = false;
			Properties.Settings.Default.EmulatorIsJapanese = false;
		}

		private void OptionsMenu_DropDownOpening(object sender, EventArgs e) {
			this.JapaneseMenu.Checked = Properties.Settings.Default.EmulatorIsJapanese;
			this.ExportMenu.Checked = !Properties.Settings.Default.EmulatorIsJapanese;

		}
	}
}
