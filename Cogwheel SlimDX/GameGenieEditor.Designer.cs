namespace CogwheelSlimDX {
	partial class GameGenieEditor {
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GameGenieEditor));
			this.CurrentCheatList = new System.Windows.Forms.ListView();
			this.CodeColumn = new System.Windows.Forms.ColumnHeader();
			this.MainContext = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.AddCodeContext = new System.Windows.Forms.ToolStripMenuItem();
			this.RemoveCodeContext = new System.Windows.Forms.ToolStripMenuItem();
			this.Sep0Context = new System.Windows.Forms.ToolStripSeparator();
			this.EditCodeContext = new System.Windows.Forms.ToolStripMenuItem();
			this.MainContext.SuspendLayout();
			this.SuspendLayout();
			// 
			// CurrentCheatList
			// 
			this.CurrentCheatList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.CodeColumn});
			this.CurrentCheatList.ContextMenuStrip = this.MainContext;
			this.CurrentCheatList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CurrentCheatList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.CurrentCheatList.LabelEdit = true;
			this.CurrentCheatList.Location = new System.Drawing.Point(0, 0);
			this.CurrentCheatList.Name = "CurrentCheatList";
			this.CurrentCheatList.Size = new System.Drawing.Size(252, 184);
			this.CurrentCheatList.TabIndex = 0;
			this.CurrentCheatList.UseCompatibleStateImageBehavior = false;
			this.CurrentCheatList.View = System.Windows.Forms.View.Details;
			this.CurrentCheatList.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.CurrentCheatList_AfterLabelEdit);
			// 
			// CodeColumn
			// 
			this.CodeColumn.Text = "Code";
			this.CodeColumn.Width = 215;
			// 
			// MainContext
			// 
			this.MainContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AddCodeContext,
            this.RemoveCodeContext,
            this.Sep0Context,
            this.EditCodeContext});
			this.MainContext.Name = "MainContext";
			this.MainContext.Size = new System.Drawing.Size(118, 76);
			this.MainContext.Opening += new System.ComponentModel.CancelEventHandler(this.MainContext_Opening);
			// 
			// AddCodeContext
			// 
			this.AddCodeContext.Name = "AddCodeContext";
			this.AddCodeContext.Size = new System.Drawing.Size(117, 22);
			this.AddCodeContext.Text = "&Add";
			this.AddCodeContext.Click += new System.EventHandler(this.AddCodeContext_Click);
			// 
			// RemoveCodeContext
			// 
			this.RemoveCodeContext.Name = "RemoveCodeContext";
			this.RemoveCodeContext.Size = new System.Drawing.Size(117, 22);
			this.RemoveCodeContext.Text = "&Remove";
			this.RemoveCodeContext.Click += new System.EventHandler(this.RemoveCodeContext_Click);
			// 
			// Sep0Context
			// 
			this.Sep0Context.Name = "Sep0Context";
			this.Sep0Context.Size = new System.Drawing.Size(114, 6);
			// 
			// EditCodeContext
			// 
			this.EditCodeContext.Name = "EditCodeContext";
			this.EditCodeContext.Size = new System.Drawing.Size(117, 22);
			this.EditCodeContext.Text = "E&dit";
			this.EditCodeContext.Click += new System.EventHandler(this.EditCodeContext_Click);
			// 
			// GameGenieEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(252, 184);
			this.Controls.Add(this.CurrentCheatList);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "GameGenieEditor";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Game Genie Codes";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GameGenieEditor_FormClosing);
			this.MainContext.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListView CurrentCheatList;
		private System.Windows.Forms.ColumnHeader CodeColumn;
		private System.Windows.Forms.ContextMenuStrip MainContext;
		private System.Windows.Forms.ToolStripMenuItem AddCodeContext;
		private System.Windows.Forms.ToolStripMenuItem RemoveCodeContext;
		private System.Windows.Forms.ToolStripSeparator Sep0Context;
		private System.Windows.Forms.ToolStripMenuItem EditCodeContext;

	}
}