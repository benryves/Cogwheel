using System;
using System.Windows.Forms;

namespace BeeDevelopment.SgcPlayer {
	static class Program {
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args) {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainInterface(args.Length == 1 ? args[0] : null));
		}
	}
}
