namespace BeeDevelopment.Cogwheel {
	partial class About {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(About));
			this.ProductNameLabel = new System.Windows.Forms.Label();
			this.WebsiteLink = new System.Windows.Forms.LinkLabel();
			this.AboutText = new System.Windows.Forms.WebBrowser();
			this.SuspendLayout();
			// 
			// ProductNameLabel
			// 
			this.ProductNameLabel.AutoSize = true;
			this.ProductNameLabel.BackColor = System.Drawing.Color.Transparent;
			this.ProductNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ProductNameLabel.Location = new System.Drawing.Point(12, 9);
			this.ProductNameLabel.Name = "ProductNameLabel";
			this.ProductNameLabel.Size = new System.Drawing.Size(169, 15);
			this.ProductNameLabel.TabIndex = 0;
			this.ProductNameLabel.Text = "Application.ProductName";
			// 
			// WebsiteLink
			// 
			this.WebsiteLink.AutoSize = true;
			this.WebsiteLink.BackColor = System.Drawing.Color.Transparent;
			this.WebsiteLink.Location = new System.Drawing.Point(12, 36);
			this.WebsiteLink.Name = "WebsiteLink";
			this.WebsiteLink.Size = new System.Drawing.Size(65, 13);
			this.WebsiteLink.TabIndex = 1;
			this.WebsiteLink.TabStop = true;
			this.WebsiteLink.Text = "Visit website";
			this.WebsiteLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.WebsiteLink_LinkClicked);
			// 
			// AboutText
			// 
			this.AboutText.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.AboutText.Location = new System.Drawing.Point(0, 111);
			this.AboutText.MinimumSize = new System.Drawing.Size(20, 20);
			this.AboutText.Name = "AboutText";
			this.AboutText.Size = new System.Drawing.Size(501, 171);
			this.AboutText.TabIndex = 2;
			this.AboutText.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.AboutText_Navigating);
			// 
			// About
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.BackgroundImage = global::BeeDevelopment.Cogwheel.Properties.Resources.Image_Banner;
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.ClientSize = new System.Drawing.Size(501, 282);
			this.Controls.Add(this.AboutText);
			this.Controls.Add(this.WebsiteLink);
			this.Controls.Add(this.ProductNameLabel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "About";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "About";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label ProductNameLabel;
		private System.Windows.Forms.LinkLabel WebsiteLink;
		private System.Windows.Forms.WebBrowser AboutText;
	}
}