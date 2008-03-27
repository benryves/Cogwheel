namespace CogwheelSlimDX {
	partial class AdvancedRomLoadDialog {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AdvancedRomLoadDialog));
			this.GroupMemory = new System.Windows.Forms.GroupBox();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.BrowseCartridgePatchFileName = new BeeDevelopment.Sega8Bit.FileBrowseTextBox();
			this.BrowseCartridgePatchLabel = new System.Windows.Forms.Label();
			this.BrowseBiosFileName = new BeeDevelopment.Sega8Bit.FileBrowseTextBox();
			this.BrowsBiosLabel = new System.Windows.Forms.Label();
			this.BrowseCartridgeFileName = new BeeDevelopment.Sega8Bit.FileBrowseTextBox();
			this.BrowseCartridgeLabel = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.ButtonOK = new System.Windows.Forms.Button();
			this.ButtonCancel = new System.Windows.Forms.Button();
			this.GroupMemory.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// GroupMemory
			// 
			this.GroupMemory.Controls.Add(this.tableLayoutPanel1);
			this.GroupMemory.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GroupMemory.Location = new System.Drawing.Point(3, 3);
			this.GroupMemory.Name = "GroupMemory";
			this.GroupMemory.Size = new System.Drawing.Size(371, 104);
			this.GroupMemory.TabIndex = 1;
			this.GroupMemory.TabStop = false;
			this.GroupMemory.Text = "Memory";
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 24F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 76F));
			this.tableLayoutPanel1.Controls.Add(this.BrowseCartridgePatchFileName, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.BrowseCartridgePatchLabel, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.BrowseBiosFileName, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.BrowsBiosLabel, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.BrowseCartridgeFileName, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.BrowseCartridgeLabel, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 16);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(365, 85);
			this.tableLayoutPanel1.TabIndex = 2;
			// 
			// BrowseCartridgePatchFileName
			// 
			this.BrowseCartridgePatchFileName.Dock = System.Windows.Forms.DockStyle.Top;
			this.BrowseCartridgePatchFileName.FileName = "";
			this.BrowseCartridgePatchFileName.Filter = "";
			this.BrowseCartridgePatchFileName.InitialDirectory = "";
			this.BrowseCartridgePatchFileName.Location = new System.Drawing.Point(90, 31);
			this.BrowseCartridgePatchFileName.Name = "BrowseCartridgePatchFileName";
			this.BrowseCartridgePatchFileName.Size = new System.Drawing.Size(272, 20);
			this.BrowseCartridgePatchFileName.TabIndex = 5;
			// 
			// BrowseCartridgePatchLabel
			// 
			this.BrowseCartridgePatchLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.BrowseCartridgePatchLabel.Location = new System.Drawing.Point(3, 28);
			this.BrowseCartridgePatchLabel.Name = "BrowseCartridgePatchLabel";
			this.BrowseCartridgePatchLabel.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
			this.BrowseCartridgePatchLabel.Size = new System.Drawing.Size(81, 23);
			this.BrowseCartridgePatchLabel.TabIndex = 4;
			this.BrowseCartridgePatchLabel.Text = "Cartridge Patch";
			// 
			// BrowseBiosFileName
			// 
			this.BrowseBiosFileName.Dock = System.Windows.Forms.DockStyle.Top;
			this.BrowseBiosFileName.FileName = "";
			this.BrowseBiosFileName.Filter = "";
			this.BrowseBiosFileName.InitialDirectory = "";
			this.BrowseBiosFileName.Location = new System.Drawing.Point(90, 59);
			this.BrowseBiosFileName.Name = "BrowseBiosFileName";
			this.BrowseBiosFileName.Size = new System.Drawing.Size(272, 20);
			this.BrowseBiosFileName.TabIndex = 2;
			// 
			// BrowsBiosLabel
			// 
			this.BrowsBiosLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.BrowsBiosLabel.Location = new System.Drawing.Point(3, 56);
			this.BrowsBiosLabel.Name = "BrowsBiosLabel";
			this.BrowsBiosLabel.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
			this.BrowsBiosLabel.Size = new System.Drawing.Size(81, 23);
			this.BrowsBiosLabel.TabIndex = 3;
			this.BrowsBiosLabel.Text = "BIOS ROM";
			// 
			// BrowseCartridgeFileName
			// 
			this.BrowseCartridgeFileName.Dock = System.Windows.Forms.DockStyle.Top;
			this.BrowseCartridgeFileName.FileName = "";
			this.BrowseCartridgeFileName.Filter = "";
			this.BrowseCartridgeFileName.InitialDirectory = "";
			this.BrowseCartridgeFileName.Location = new System.Drawing.Point(90, 3);
			this.BrowseCartridgeFileName.Name = "BrowseCartridgeFileName";
			this.BrowseCartridgeFileName.Size = new System.Drawing.Size(272, 20);
			this.BrowseCartridgeFileName.TabIndex = 0;
			// 
			// BrowseCartridgeLabel
			// 
			this.BrowseCartridgeLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.BrowseCartridgeLabel.Location = new System.Drawing.Point(3, 0);
			this.BrowseCartridgeLabel.Name = "BrowseCartridgeLabel";
			this.BrowseCartridgeLabel.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
			this.BrowseCartridgeLabel.Size = new System.Drawing.Size(81, 23);
			this.BrowseCartridgeLabel.TabIndex = 1;
			this.BrowseCartridgeLabel.Text = "Cartridge ROM";
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.ButtonOK);
			this.panel1.Controls.Add(this.ButtonCancel);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(3, 107);
			this.panel1.Name = "panel1";
			this.panel1.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
			this.panel1.Size = new System.Drawing.Size(371, 31);
			this.panel1.TabIndex = 2;
			// 
			// ButtonOK
			// 
			this.ButtonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ButtonOK.Dock = System.Windows.Forms.DockStyle.Right;
			this.ButtonOK.Location = new System.Drawing.Point(221, 4);
			this.ButtonOK.Name = "ButtonOK";
			this.ButtonOK.Size = new System.Drawing.Size(75, 27);
			this.ButtonOK.TabIndex = 1;
			this.ButtonOK.Text = "&OK";
			this.ButtonOK.UseVisualStyleBackColor = true;
			this.ButtonOK.Click += new System.EventHandler(this.ButtonOK_Click);
			// 
			// ButtonCancel
			// 
			this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ButtonCancel.Dock = System.Windows.Forms.DockStyle.Right;
			this.ButtonCancel.Location = new System.Drawing.Point(296, 4);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.Size = new System.Drawing.Size(75, 27);
			this.ButtonCancel.TabIndex = 0;
			this.ButtonCancel.Text = "&Cancel";
			this.ButtonCancel.UseVisualStyleBackColor = true;
			// 
			// AdvancedRomLoadDialog
			// 
			this.AcceptButton = this.ButtonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.ButtonCancel;
			this.ClientSize = new System.Drawing.Size(377, 141);
			this.Controls.Add(this.GroupMemory);
			this.Controls.Add(this.panel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AdvancedRomLoadDialog";
			this.Padding = new System.Windows.Forms.Padding(3);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Load ROM";
			this.GroupMemory.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private BeeDevelopment.Sega8Bit.FileBrowseTextBox BrowseCartridgeFileName;
		private System.Windows.Forms.GroupBox GroupMemory;
		private System.Windows.Forms.Label BrowseCartridgeLabel;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label BrowsBiosLabel;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button ButtonOK;
		private System.Windows.Forms.Button ButtonCancel;
		private System.Windows.Forms.Label BrowseCartridgePatchLabel;
		private BeeDevelopment.Sega8Bit.FileBrowseTextBox BrowseBiosFileName;
		private BeeDevelopment.Sega8Bit.FileBrowseTextBox BrowseCartridgePatchFileName;
	}
}