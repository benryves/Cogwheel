
namespace BeeDevelopment.Cogwheel {
	partial class SerialTerminal {
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
			this.ConsoleInput = new System.Windows.Forms.TextBox();
			this.ConsoleMenuStrip = new System.Windows.Forms.MenuStrip();
			this.BaudRateMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ConnectToMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ConnectToNoneMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ConsoleOutput = new System.Windows.Forms.TextBox();
			this.ConsoleMenuStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// ConsoleInput
			// 
			this.ConsoleInput.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ConsoleInput.Location = new System.Drawing.Point(0, 248);
			this.ConsoleInput.Name = "ConsoleInput";
			this.ConsoleInput.Size = new System.Drawing.Size(424, 20);
			this.ConsoleInput.TabIndex = 0;
			this.ConsoleInput.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ConsoleInput_KeyPress);
			// 
			// ConsoleMenuStrip
			// 
			this.ConsoleMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.BaudRateMenuItem,
            this.ConnectToMenuItem});
			this.ConsoleMenuStrip.Location = new System.Drawing.Point(0, 0);
			this.ConsoleMenuStrip.Name = "ConsoleMenuStrip";
			this.ConsoleMenuStrip.Size = new System.Drawing.Size(424, 24);
			this.ConsoleMenuStrip.TabIndex = 1;
			// 
			// BaudRateMenuItem
			// 
			this.BaudRateMenuItem.Name = "BaudRateMenuItem";
			this.BaudRateMenuItem.Size = new System.Drawing.Size(72, 20);
			this.BaudRateMenuItem.Text = "Baud &Rate";
			this.BaudRateMenuItem.DropDownOpening += new System.EventHandler(this.BaudRateMenuItem_DropDownOpening);
			// 
			// ConnectToMenuItem
			// 
			this.ConnectToMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ConnectToNoneMenuItem});
			this.ConnectToMenuItem.Name = "ConnectToMenuItem";
			this.ConnectToMenuItem.Size = new System.Drawing.Size(79, 20);
			this.ConnectToMenuItem.Text = "&Connect To";
			this.ConnectToMenuItem.DropDownOpening += new System.EventHandler(this.ConnectToMenuItem_DropDownOpening);
			// 
			// ConnectToNoneMenuItem
			// 
			this.ConnectToNoneMenuItem.Name = "ConnectToNoneMenuItem";
			this.ConnectToNoneMenuItem.Size = new System.Drawing.Size(180, 22);
			this.ConnectToNoneMenuItem.Text = "None";
			this.ConnectToNoneMenuItem.Click += new System.EventHandler(this.ConnectToDropdownMenuItem_Click);
			// 
			// ConsoleOutput
			// 
			this.ConsoleOutput.BackColor = System.Drawing.SystemColors.WindowText;
			this.ConsoleOutput.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsoleOutput.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ConsoleOutput.ForeColor = System.Drawing.SystemColors.Window;
			this.ConsoleOutput.Location = new System.Drawing.Point(0, 24);
			this.ConsoleOutput.Multiline = true;
			this.ConsoleOutput.Name = "ConsoleOutput";
			this.ConsoleOutput.ReadOnly = true;
			this.ConsoleOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ConsoleOutput.Size = new System.Drawing.Size(424, 224);
			this.ConsoleOutput.TabIndex = 2;
			// 
			// SerialTerminal
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(424, 268);
			this.Controls.Add(this.ConsoleOutput);
			this.Controls.Add(this.ConsoleInput);
			this.Controls.Add(this.ConsoleMenuStrip);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MainMenuStrip = this.ConsoleMenuStrip;
			this.Name = "SerialTerminal";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Serial Terminal";
			this.Load += new System.EventHandler(this.SerialTerminal_Load);
			this.ConsoleMenuStrip.ResumeLayout(false);
			this.ConsoleMenuStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox ConsoleInput;
		private System.Windows.Forms.MenuStrip ConsoleMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem BaudRateMenuItem;
		private System.Windows.Forms.ToolStripMenuItem ConnectToMenuItem;
		private System.Windows.Forms.ToolStripMenuItem ConnectToNoneMenuItem;
		private System.Windows.Forms.TextBox ConsoleOutput;
	}
}