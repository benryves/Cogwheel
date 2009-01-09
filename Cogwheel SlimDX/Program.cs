using System;
using System.Windows.Forms;
using SlimDX.Direct3D9;
using SlimDX.DirectSound;

namespace BeeDevelopment.Cogwheel {
	static class Program {

		internal static Direct3D D3D;
		internal static DirectSound DS;

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

			// Try to initialise DirectSound:
			try {
				DS = new DirectSound();
			} catch (Exception ex) {
				MessageBox.Show("Could not initialise DirectSound: " + Environment.NewLine + ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			try {

				// We're good to go (hopefully).
				Application.Run(new MainForm(arguments));

			} finally {
				try {
					if (DS != null && !DS.Disposed) DS.Dispose();
				} catch { }
				try {
					if (D3D != null && !D3D.Disposed) D3D.Dispose();
				} catch { }
			}			
		}
	}
}
