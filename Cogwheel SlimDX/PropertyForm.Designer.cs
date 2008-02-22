namespace CogwheelSlimDX {
	partial class PropertyForm {
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
			this.Properties = new System.Windows.Forms.PropertyGrid();
			this.SuspendLayout();
			// 
			// Properties
			// 
			this.Properties.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Properties.Location = new System.Drawing.Point(0, 0);
			this.Properties.Name = "Properties";
			this.Properties.Size = new System.Drawing.Size(214, 320);
			this.Properties.TabIndex = 0;
			// 
			// PropertyForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(214, 320);
			this.Controls.Add(this.Properties);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Name = "PropertyForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "PropertyForm";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.PropertyGrid Properties;
	}
}