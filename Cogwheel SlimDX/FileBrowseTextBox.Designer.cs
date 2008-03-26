namespace BeeDevelopment.Sega8Bit {
	partial class FileBrowseTextBox {
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.FileDialog = new System.Windows.Forms.OpenFileDialog();
			this.FilePathTextBox = new System.Windows.Forms.TextBox();
			this.OpenFileDialogButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// FilePathTextBox
			// 
			this.FilePathTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
			this.FilePathTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
			this.FilePathTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FilePathTextBox.Location = new System.Drawing.Point(0, 0);
			this.FilePathTextBox.Name = "FilePathTextBox";
			this.FilePathTextBox.Size = new System.Drawing.Size(370, 20);
			this.FilePathTextBox.TabIndex = 0;
			// 
			// OpenFileDialogButton
			// 
			this.OpenFileDialogButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.OpenFileDialogButton.Location = new System.Drawing.Point(370, 0);
			this.OpenFileDialogButton.Name = "OpenFileDialogButton";
			this.OpenFileDialogButton.Size = new System.Drawing.Size(29, 73);
			this.OpenFileDialogButton.TabIndex = 1;
			this.OpenFileDialogButton.Text = "...";
			this.OpenFileDialogButton.UseVisualStyleBackColor = true;
			this.OpenFileDialogButton.Click += new System.EventHandler(this.OpenFileDialogButton_Click);
			// 
			// FileBrowseTextBox
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.FilePathTextBox);
			this.Controls.Add(this.OpenFileDialogButton);
			this.Name = "FileBrowseTextBox";
			this.Size = new System.Drawing.Size(399, 73);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.OpenFileDialog FileDialog;
		private System.Windows.Forms.TextBox FilePathTextBox;
		private System.Windows.Forms.Button OpenFileDialogButton;
	}
}
