using System;
using System.Windows.Forms;
using SlimDX.Direct3D9;

namespace BeeDevelopment.Cogwheel {
	static class Program {

		internal static Direct3D D3D;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] arguments) {

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);



			// Try to initialise Direct3D:
			try {
				D3D = new Direct3D();
			} catch (Exception ex) {
				MessageBox.Show("Could not initialise Direct3D: " + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine + "Make sure you have Direct3D 9.0c and the latest updates installed.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// We're good to go (hopefully).
			Application.Run(new MainForm(arguments));
		}
	}
}
