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
			this.BrowseBiosFileName = new BeeDevelopment.Sega8Bit.FileBrowseTextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.BrowseCartridgeFileName = new BeeDevelopment.Sega8Bit.FileBrowseTextBox();
			this.label1 = new System.Windows.Forms.Label();
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
			this.GroupMemory.Size = new System.Drawing.Size(371, 68);
			this.GroupMemory.TabIndex = 1;
			this.GroupMemory.TabStop = false;
			this.GroupMemory.Text = "Memory";
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 24F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 76F));
			this.tableLayoutPanel1.Controls.Add(this.BrowseBiosFileName, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.label2, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.BrowseCartridgeFileName, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 16);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(365, 49);
			this.tableLayoutPanel1.TabIndex = 2;
			// 
			// BrowseBiosFileName
			// 
			this.BrowseBiosFileName.Dock = System.Windows.Forms.DockStyle.Top;
			this.BrowseBiosFileName.FileName = "";
			this.BrowseBiosFileName.Filter = "";
			this.BrowseBiosFileName.Location = new System.Drawing.Point(90, 27);
			this.BrowseBiosFileName.Name = "BrowseBiosFileName";
			this.BrowseBiosFileName.Size = new System.Drawing.Size(272, 20);
			this.BrowseBiosFileName.TabIndex = 2;
			// 
			// label2
			// 
			this.label2.Dock = System.Windows.Forms.DockStyle.Top;
			this.label2.Location = new System.Drawing.Point(3, 24);
			this.label2.Name = "label2";
			this.label2.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
			this.label2.Size = new System.Drawing.Size(81, 23);
			this.label2.TabIndex = 3;
			this.label2.Text = "BIOS ROM";
			// 
			// BrowseCartridgeFileName
			// 
			this.BrowseCartridgeFileName.Dock = System.Windows.Forms.DockStyle.Top;
			this.BrowseCartridgeFileName.FileName = "";
			this.BrowseCartridgeFileName.Filter = "";
			this.BrowseCartridgeFileName.Location = new System.Drawing.Point(90, 3);
			this.BrowseCartridgeFileName.Name = "BrowseCartridgeFileName";
			this.BrowseCartridgeFileName.Size = new System.Drawing.Size(272, 20);
			this.BrowseCartridgeFileName.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.Dock = System.Windows.Forms.DockStyle.Top;
			this.label1.Location = new System.Drawing.Point(3, 0);
			this.label1.Name = "label1";
			this.label1.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
			this.label1.Size = new System.Drawing.Size(81, 23);
			this.label1.TabIndex = 1;
			this.label1.Text = "Cartridge ROM";
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.ButtonOK);
			this.panel1.Controls.Add(this.ButtonCancel);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(3, 71);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(371, 31);
			this.panel1.TabIndex = 2;
			// 
			// ButtonOK
			// 
			this.ButtonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ButtonOK.Dock = System.Windows.Forms.DockStyle.Right;
			this.ButtonOK.Location = new System.Drawing.Point(221, 0);
			this.ButtonOK.Name = "ButtonOK";
			this.ButtonOK.Size = new System.Drawing.Size(75, 31);
			this.ButtonOK.TabIndex = 1;
			this.ButtonOK.Text = "&OK";
			this.ButtonOK.UseVisualStyleBackColor = true;
			// 
			// ButtonCancel
			// 
			this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ButtonCancel.Dock = System.Windows.Forms.DockStyle.Right;
			this.ButtonCancel.Location = new System.Drawing.Point(296, 0);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.Size = new System.Drawing.Size(75, 31);
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
			this.ClientSize = new System.Drawing.Size(377, 105);
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
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private BeeDevelopment.Sega8Bit.FileBrowseTextBox BrowseBiosFileName;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button ButtonOK;
		private System.Windows.Forms.Button ButtonCancel;
	}
}