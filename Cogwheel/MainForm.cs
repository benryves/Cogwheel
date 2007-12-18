using System;
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
			
		}

		private void ExitMenu_Click(object sender, EventArgs e) {
			this.Close();
		}

		private void OpenMenu_Click(object sender, EventArgs e) {
			this.PromptOpenRom();
		}
	}
}
