using System;
using System.Windows.Forms;
using SlimDX.Direct3D9;

namespace CogwheelSlimDX {
	static class Program {
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() {

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			//MessageBox.Show("This is a special build for Jacko; this is not meant to be out on general release!" + Environment.NewLine + "It is still full of bugs.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);

			// Try to initialise Direct3D:
			try {
				Direct3D.Initialize();
			} catch (Exception ex) {
				MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// We're good to go (hopefully).
			Application.Run(new MainForm());
		}
	}
}
