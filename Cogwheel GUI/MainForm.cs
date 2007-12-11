using System.Windows.Forms;
using System.Drawing;
using System;
using System.IO;
using System.Threading;

namespace CogwheelGui {
	public partial class MainForm : Form {


		private CogwheelHost Host;


		public MainForm() {
			InitializeComponent();
			this.Text = Application.ProductName;

			this.ClientSize = new Size(256 * 2, 192 * 2);

			this.Disposed += (sender, e) => {
				this.Host.Dispose();
			};

			
			
		}

		private void MenuExit_Click(object sender, System.EventArgs e) {
			this.Close();
		}

		private void MainForm_Load(object sender, EventArgs e) {
			this.Host = new CogwheelHost();
			this.Controls.Add(Host);
			this.Host.Dock = DockStyle.Fill;
			this.Host.BringToFront();
			using (var FS = new FileStream(@"D:\Documents\Desktop\VDPTEST.sms", FileMode.Open)) {
				this.Host.Emulator.LoadCartridge(FS);
			}
		}



	}
}
