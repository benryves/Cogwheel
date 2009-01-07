using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace BeeDevelopment.Cogwheel {
	public partial class AdvancedRomLoadDialog : Form {

		public string CartridgeFileName {
			get { return this.BrowseCartridgeFileName.FileName; }
			set { this.BrowseCartridgeFileName.FileName = value; }
		}

		public string CartridgePatchFileName {
			get { return this.BrowseCartridgePatchFileName.FileName; }
			set { this.BrowseCartridgePatchFileName.FileName = value; }
		}

		public string BiosFileName {
			get { return this.BrowseBiosFileName.FileName; }
			set { this.BrowseBiosFileName.FileName = value; }
		}

		public AdvancedRomLoadDialog() {
			InitializeComponent();
			this.BrowseCartridgeFileName.InitialDirectory = Properties.Settings.Default.StoredPathAdvancedCartridge;
			this.BrowseCartridgePatchFileName.InitialDirectory = Properties.Settings.Default.StoredPathAdvancedCartridgePatch;
			this.BrowseBiosFileName.InitialDirectory = Properties.Settings.Default.StoredPathAdvancedBios;
		}

		private void ButtonOK_Click(object sender, EventArgs e) {
			
			if (!string.IsNullOrEmpty(this.BrowseCartridgeFileName.FileName) && File.Exists(this.BrowseCartridgeFileName.FileName))
				Properties.Settings.Default.StoredPathAdvancedCartridge = Path.GetDirectoryName(this.BrowseCartridgeFileName.FileName);

			if (!string.IsNullOrEmpty(this.BrowseCartridgePatchFileName.FileName) && File.Exists(this.BrowseCartridgePatchFileName.FileName))
				Properties.Settings.Default.StoredPathAdvancedCartridgePatch = Path.GetDirectoryName(this.BrowseCartridgePatchFileName.FileName);

			if (!string.IsNullOrEmpty(this.BrowseBiosFileName.FileName) && File.Exists(this.BrowseBiosFileName.FileName))
				Properties.Settings.Default.StoredPathAdvancedBios = Path.GetDirectoryName(this.BrowseBiosFileName.FileName);
		}


	}
}
