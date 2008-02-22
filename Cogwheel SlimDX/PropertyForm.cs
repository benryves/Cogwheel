using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CogwheelSlimDX {
	public partial class PropertyForm : Form {
		public PropertyForm(string title, object o) {
			InitializeComponent();
			this.Text = title;
			this.Properties.SelectedObject = o;
		}
	}
}
