using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace CogwheelSlimDX {
	public partial class About : Form {
		public About() {
			InitializeComponent();
			this.ClientSize = new Size(500, this.ClientSize.Height);
			this.ProductNameLabel.Text = Application.ProductName;
			this.AboutText.Height = this.ClientSize.Height - 64;
			this.AboutText.DocumentText = string.Format(@"<style>* {{ font-family: ""{0}""; font-size: {1}pt; }}</style>", this.Font.FontFamily.Name, this.Font.SizeInPoints) + Properties.Resources.Html_About;
		}

		private void WebsiteLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
			try {
				Process.Start(Properties.Settings.Default.UrlWebsite);
			} catch (Exception ex) {
				MessageBox.Show(this, "Please visit " + Properties.Settings.Default.UrlWebsite + " in your browser." + Environment.NewLine + "(Error: " + ex.Message + ")", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		private void AboutText_Navigating(object sender, WebBrowserNavigatingEventArgs e) {
			if (e.Url.Scheme == "http") {
				e.Cancel = true;
				try {
					Process.Start(e.Url.AbsoluteUri);
				} catch { }
			}
		}
	}
}
