﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BeeDevelopment.Cogwheel {
	public partial class MainForm : Form {

		public MainForm() {
			InitializeComponent();
			this.Text = Application.ProductName;
			this.MainFormMenus.Renderer = new Asztal.Szótár.NativeToolStripRenderer(this.MainFormMenus);
		}

		private void MainForm_Load(object sender, EventArgs e) {
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
