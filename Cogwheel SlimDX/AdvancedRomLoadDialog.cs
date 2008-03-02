using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CogwheelSlimDX {
	public partial class AdvancedRomLoadDialog : Form {

		public string CartridgeFileName {
			get { return this.BrowseCartridgeFileName.FileName; }
			set { this.BrowseCartridgeFileName.FileName = value; }
		}

		public string BiosFileName {
			get { return this.BrowseBiosFileName.FileName; }
			set { this.BrowseBiosFileName.FileName = value; }
		}

		public AdvancedRomLoadDialog() {
			InitializeComponent();
		}
	}
}
