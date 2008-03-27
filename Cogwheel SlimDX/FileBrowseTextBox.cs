using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace BeeDevelopment.Sega8Bit {
	public partial class FileBrowseTextBox : UserControl {

		public string FileName {
			get { return this.FilePathTextBox.Text; }
			set { this.FilePathTextBox.Text = value; }
		}

		public string InitialDirectory {
			get { return this.FileDialog.InitialDirectory; }
			set { if (!string.IsNullOrEmpty(value) && Directory.Exists(value)) this.FileDialog.InitialDirectory = value; }
		}

		public string Filter {
			get { return this.FileDialog.Filter; }
			set { this.FileDialog.Filter = value; }
		}

		public FileBrowseTextBox() {
			InitializeComponent();
		}
		protected override void OnSizeChanged(EventArgs e) {
			this.ClientSize = new Size(this.ClientSize.Width, this.FilePathTextBox.Size.Height);
			base.OnSizeChanged(e);
		}

		private void OpenFileDialogButton_Click(object sender, EventArgs e) {
			this.FileDialog.FileName = this.FileName;
			if (this.FileDialog.ShowDialog(this) == DialogResult.OK) {
				this.FileName = this.FileDialog.FileName;
				this.FilePathTextBox.SelectAll();
			}
		}
	}
	
}
