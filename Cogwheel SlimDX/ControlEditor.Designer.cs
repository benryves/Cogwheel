namespace CogwheelSlimDX {
	partial class ControlEditor {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ControlEditor));
			this.InputTabs = new System.Windows.Forms.TabControl();
			this.KeyboardTab = new System.Windows.Forms.TabPage();
			this.JoypadGroup = new System.Windows.Forms.GroupBox();
			this.JoypadTable = new System.Windows.Forms.TableLayoutPanel();
			this.LabelButton1 = new System.Windows.Forms.Label();
			this.LabelButton2 = new System.Windows.Forms.Label();
			this.LabelUp = new System.Windows.Forms.Label();
			this.LabelDown = new System.Windows.Forms.Label();
			this.LabelLeft = new System.Windows.Forms.Label();
			this.LabelRight = new System.Windows.Forms.Label();
			this.ConsoleGroup = new System.Windows.Forms.GroupBox();
			this.ConsoleTable = new System.Windows.Forms.TableLayoutPanel();
			this.LabelReset = new System.Windows.Forms.Label();
			this.LabelStart = new System.Windows.Forms.Label();
			this.LabelPause = new System.Windows.Forms.Label();
			this.PanelOKCancel = new System.Windows.Forms.Panel();
			this.ButtonOK = new System.Windows.Forms.Button();
			this.ButtonCancel = new System.Windows.Forms.Button();
			this.Player1TL = new CogwheelSlimDX.KeyButton();
			this.Player1TR = new CogwheelSlimDX.KeyButton();
			this.Player1Up = new CogwheelSlimDX.KeyButton();
			this.Player1Down = new CogwheelSlimDX.KeyButton();
			this.Player1Left = new CogwheelSlimDX.KeyButton();
			this.Player1Right = new CogwheelSlimDX.KeyButton();
			this.Player2TL = new CogwheelSlimDX.KeyButton();
			this.Player2TR = new CogwheelSlimDX.KeyButton();
			this.Player2Up = new CogwheelSlimDX.KeyButton();
			this.Player2Down = new CogwheelSlimDX.KeyButton();
			this.Player2Left = new CogwheelSlimDX.KeyButton();
			this.Player2Right = new CogwheelSlimDX.KeyButton();
			this.ConsolePause = new CogwheelSlimDX.KeyButton();
			this.ConsoleReset = new CogwheelSlimDX.KeyButton();
			this.ConsoleStart = new CogwheelSlimDX.KeyButton();
			this.TabIcons = new System.Windows.Forms.ImageList(this.components);
			this.InputTabs.SuspendLayout();
			this.KeyboardTab.SuspendLayout();
			this.JoypadGroup.SuspendLayout();
			this.JoypadTable.SuspendLayout();
			this.ConsoleGroup.SuspendLayout();
			this.ConsoleTable.SuspendLayout();
			this.PanelOKCancel.SuspendLayout();
			this.SuspendLayout();
			// 
			// InputTabs
			// 
			this.InputTabs.Controls.Add(this.KeyboardTab);
			this.InputTabs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InputTabs.ImageList = this.TabIcons;
			this.InputTabs.Location = new System.Drawing.Point(3, 3);
			this.InputTabs.Name = "InputTabs";
			this.InputTabs.SelectedIndex = 0;
			this.InputTabs.Size = new System.Drawing.Size(315, 329);
			this.InputTabs.TabIndex = 0;
			// 
			// KeyboardTab
			// 
			this.KeyboardTab.Controls.Add(this.JoypadGroup);
			this.KeyboardTab.Controls.Add(this.ConsoleGroup);
			this.KeyboardTab.ImageIndex = 0;
			this.KeyboardTab.Location = new System.Drawing.Point(4, 23);
			this.KeyboardTab.Name = "KeyboardTab";
			this.KeyboardTab.Padding = new System.Windows.Forms.Padding(3);
			this.KeyboardTab.Size = new System.Drawing.Size(307, 302);
			this.KeyboardTab.TabIndex = 0;
			this.KeyboardTab.Text = "Keyboard";
			this.KeyboardTab.UseVisualStyleBackColor = true;
			// 
			// JoypadGroup
			// 
			this.JoypadGroup.Controls.Add(this.JoypadTable);
			this.JoypadGroup.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JoypadGroup.Location = new System.Drawing.Point(3, 3);
			this.JoypadGroup.Name = "JoypadGroup";
			this.JoypadGroup.Size = new System.Drawing.Size(301, 187);
			this.JoypadGroup.TabIndex = 1;
			this.JoypadGroup.TabStop = false;
			this.JoypadGroup.Text = "Joypad";
			// 
			// JoypadTable
			// 
			this.JoypadTable.ColumnCount = 3;
			this.JoypadTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.JoypadTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.JoypadTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.JoypadTable.Controls.Add(this.LabelButton1, 0, 0);
			this.JoypadTable.Controls.Add(this.LabelButton2, 0, 1);
			this.JoypadTable.Controls.Add(this.LabelUp, 0, 2);
			this.JoypadTable.Controls.Add(this.LabelDown, 0, 3);
			this.JoypadTable.Controls.Add(this.LabelLeft, 0, 4);
			this.JoypadTable.Controls.Add(this.LabelRight, 0, 5);
			this.JoypadTable.Controls.Add(this.Player1TL, 1, 0);
			this.JoypadTable.Controls.Add(this.Player1TR, 1, 1);
			this.JoypadTable.Controls.Add(this.Player1Up, 1, 2);
			this.JoypadTable.Controls.Add(this.Player1Down, 1, 3);
			this.JoypadTable.Controls.Add(this.Player1Left, 1, 4);
			this.JoypadTable.Controls.Add(this.Player1Right, 1, 5);
			this.JoypadTable.Controls.Add(this.Player2TL, 2, 0);
			this.JoypadTable.Controls.Add(this.Player2TR, 2, 1);
			this.JoypadTable.Controls.Add(this.Player2Up, 2, 2);
			this.JoypadTable.Controls.Add(this.Player2Down, 2, 3);
			this.JoypadTable.Controls.Add(this.Player2Left, 2, 4);
			this.JoypadTable.Controls.Add(this.Player2Right, 2, 5);
			this.JoypadTable.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JoypadTable.Location = new System.Drawing.Point(3, 16);
			this.JoypadTable.Name = "JoypadTable";
			this.JoypadTable.RowCount = 6;
			this.JoypadTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
			this.JoypadTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
			this.JoypadTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
			this.JoypadTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
			this.JoypadTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
			this.JoypadTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
			this.JoypadTable.Size = new System.Drawing.Size(295, 168);
			this.JoypadTable.TabIndex = 0;
			// 
			// LabelButton1
			// 
			this.LabelButton1.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.LabelButton1.AutoSize = true;
			this.LabelButton1.Location = new System.Drawing.Point(42, 7);
			this.LabelButton1.Name = "LabelButton1";
			this.LabelButton1.Size = new System.Drawing.Size(13, 13);
			this.LabelButton1.TabIndex = 0;
			this.LabelButton1.Text = "1";
			// 
			// LabelButton2
			// 
			this.LabelButton2.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.LabelButton2.AutoSize = true;
			this.LabelButton2.Location = new System.Drawing.Point(42, 35);
			this.LabelButton2.Name = "LabelButton2";
			this.LabelButton2.Size = new System.Drawing.Size(13, 13);
			this.LabelButton2.TabIndex = 1;
			this.LabelButton2.Text = "2";
			// 
			// LabelUp
			// 
			this.LabelUp.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.LabelUp.AutoSize = true;
			this.LabelUp.Location = new System.Drawing.Point(38, 63);
			this.LabelUp.Name = "LabelUp";
			this.LabelUp.Size = new System.Drawing.Size(21, 13);
			this.LabelUp.TabIndex = 2;
			this.LabelUp.Text = "Up";
			// 
			// LabelDown
			// 
			this.LabelDown.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.LabelDown.AutoSize = true;
			this.LabelDown.Location = new System.Drawing.Point(31, 91);
			this.LabelDown.Name = "LabelDown";
			this.LabelDown.Size = new System.Drawing.Size(35, 13);
			this.LabelDown.TabIndex = 3;
			this.LabelDown.Text = "Down";
			// 
			// LabelLeft
			// 
			this.LabelLeft.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.LabelLeft.AutoSize = true;
			this.LabelLeft.Location = new System.Drawing.Point(36, 119);
			this.LabelLeft.Name = "LabelLeft";
			this.LabelLeft.Size = new System.Drawing.Size(25, 13);
			this.LabelLeft.TabIndex = 4;
			this.LabelLeft.Text = "Left";
			// 
			// LabelRight
			// 
			this.LabelRight.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.LabelRight.AutoSize = true;
			this.LabelRight.Location = new System.Drawing.Point(33, 147);
			this.LabelRight.Name = "LabelRight";
			this.LabelRight.Size = new System.Drawing.Size(32, 13);
			this.LabelRight.TabIndex = 5;
			this.LabelRight.Text = "Right";
			// 
			// ConsoleGroup
			// 
			this.ConsoleGroup.Controls.Add(this.ConsoleTable);
			this.ConsoleGroup.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ConsoleGroup.Location = new System.Drawing.Point(3, 190);
			this.ConsoleGroup.Name = "ConsoleGroup";
			this.ConsoleGroup.Size = new System.Drawing.Size(301, 109);
			this.ConsoleGroup.TabIndex = 2;
			this.ConsoleGroup.TabStop = false;
			this.ConsoleGroup.Text = "Console";
			// 
			// ConsoleTable
			// 
			this.ConsoleTable.ColumnCount = 2;
			this.ConsoleTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.ConsoleTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 66.66667F));
			this.ConsoleTable.Controls.Add(this.LabelPause, 0, 0);
			this.ConsoleTable.Controls.Add(this.LabelReset, 0, 1);
			this.ConsoleTable.Controls.Add(this.LabelStart, 0, 2);
			this.ConsoleTable.Controls.Add(this.ConsolePause, 1, 0);
			this.ConsoleTable.Controls.Add(this.ConsoleReset, 1, 1);
			this.ConsoleTable.Controls.Add(this.ConsoleStart, 1, 2);
			this.ConsoleTable.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsoleTable.Location = new System.Drawing.Point(3, 16);
			this.ConsoleTable.Name = "ConsoleTable";
			this.ConsoleTable.RowCount = 3;
			this.ConsoleTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.ConsoleTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.ConsoleTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.ConsoleTable.Size = new System.Drawing.Size(295, 90);
			this.ConsoleTable.TabIndex = 0;
			// 
			// LabelReset
			// 
			this.LabelReset.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.LabelReset.AutoSize = true;
			this.LabelReset.Location = new System.Drawing.Point(31, 37);
			this.LabelReset.Name = "LabelReset";
			this.LabelReset.Size = new System.Drawing.Size(35, 13);
			this.LabelReset.TabIndex = 21;
			this.LabelReset.Text = "Reset";
			// 
			// LabelStart
			// 
			this.LabelStart.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.LabelStart.AutoSize = true;
			this.LabelStart.Location = new System.Drawing.Point(34, 67);
			this.LabelStart.Name = "LabelStart";
			this.LabelStart.Size = new System.Drawing.Size(29, 13);
			this.LabelStart.TabIndex = 22;
			this.LabelStart.Text = "Start";
			// 
			// LabelPause
			// 
			this.LabelPause.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.LabelPause.AutoSize = true;
			this.LabelPause.Location = new System.Drawing.Point(30, 8);
			this.LabelPause.Name = "LabelPause";
			this.LabelPause.Size = new System.Drawing.Size(37, 13);
			this.LabelPause.TabIndex = 23;
			this.LabelPause.Text = "Pause";
			// 
			// PanelOKCancel
			// 
			this.PanelOKCancel.Controls.Add(this.ButtonOK);
			this.PanelOKCancel.Controls.Add(this.ButtonCancel);
			this.PanelOKCancel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.PanelOKCancel.Location = new System.Drawing.Point(3, 332);
			this.PanelOKCancel.Name = "PanelOKCancel";
			this.PanelOKCancel.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.PanelOKCancel.Size = new System.Drawing.Size(315, 29);
			this.PanelOKCancel.TabIndex = 1;
			// 
			// ButtonOK
			// 
			this.ButtonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ButtonOK.Dock = System.Windows.Forms.DockStyle.Right;
			this.ButtonOK.Location = new System.Drawing.Point(165, 3);
			this.ButtonOK.Name = "ButtonOK";
			this.ButtonOK.Size = new System.Drawing.Size(75, 26);
			this.ButtonOK.TabIndex = 0;
			this.ButtonOK.Text = "&OK";
			this.ButtonOK.UseVisualStyleBackColor = true;
			// 
			// ButtonCancel
			// 
			this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ButtonCancel.Dock = System.Windows.Forms.DockStyle.Right;
			this.ButtonCancel.Location = new System.Drawing.Point(240, 3);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.Size = new System.Drawing.Size(75, 26);
			this.ButtonCancel.TabIndex = 1;
			this.ButtonCancel.Text = "&Cancel";
			this.ButtonCancel.UseVisualStyleBackColor = true;
			// 
			// Player1TL
			// 
			this.Player1TL.Appearance = System.Windows.Forms.Appearance.Button;
			this.Player1TL.AutoCheck = false;
			this.Player1TL.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Player1TL.Key = System.Windows.Forms.Keys.None;
			this.Player1TL.Location = new System.Drawing.Point(101, 3);
			this.Player1TL.Name = "Player1TL";
			this.Player1TL.Size = new System.Drawing.Size(92, 22);
			this.Player1TL.TabIndex = 6;
			this.Player1TL.Text = "None";
			this.Player1TL.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.Player1TL.UseVisualStyleBackColor = true;
			// 
			// Player1TR
			// 
			this.Player1TR.Appearance = System.Windows.Forms.Appearance.Button;
			this.Player1TR.AutoCheck = false;
			this.Player1TR.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Player1TR.Key = System.Windows.Forms.Keys.None;
			this.Player1TR.Location = new System.Drawing.Point(101, 31);
			this.Player1TR.Name = "Player1TR";
			this.Player1TR.Size = new System.Drawing.Size(92, 22);
			this.Player1TR.TabIndex = 7;
			this.Player1TR.Text = "None";
			this.Player1TR.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.Player1TR.UseVisualStyleBackColor = true;
			// 
			// Player1Up
			// 
			this.Player1Up.Appearance = System.Windows.Forms.Appearance.Button;
			this.Player1Up.AutoCheck = false;
			this.Player1Up.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Player1Up.Key = System.Windows.Forms.Keys.None;
			this.Player1Up.Location = new System.Drawing.Point(101, 59);
			this.Player1Up.Name = "Player1Up";
			this.Player1Up.Size = new System.Drawing.Size(92, 22);
			this.Player1Up.TabIndex = 8;
			this.Player1Up.Text = "None";
			this.Player1Up.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.Player1Up.UseVisualStyleBackColor = true;
			// 
			// Player1Down
			// 
			this.Player1Down.Appearance = System.Windows.Forms.Appearance.Button;
			this.Player1Down.AutoCheck = false;
			this.Player1Down.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Player1Down.Key = System.Windows.Forms.Keys.None;
			this.Player1Down.Location = new System.Drawing.Point(101, 87);
			this.Player1Down.Name = "Player1Down";
			this.Player1Down.Size = new System.Drawing.Size(92, 22);
			this.Player1Down.TabIndex = 9;
			this.Player1Down.Text = "None";
			this.Player1Down.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.Player1Down.UseVisualStyleBackColor = true;
			// 
			// Player1Left
			// 
			this.Player1Left.Appearance = System.Windows.Forms.Appearance.Button;
			this.Player1Left.AutoCheck = false;
			this.Player1Left.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Player1Left.Key = System.Windows.Forms.Keys.None;
			this.Player1Left.Location = new System.Drawing.Point(101, 115);
			this.Player1Left.Name = "Player1Left";
			this.Player1Left.Size = new System.Drawing.Size(92, 22);
			this.Player1Left.TabIndex = 10;
			this.Player1Left.Text = "None";
			this.Player1Left.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.Player1Left.UseVisualStyleBackColor = true;
			// 
			// Player1Right
			// 
			this.Player1Right.Appearance = System.Windows.Forms.Appearance.Button;
			this.Player1Right.AutoCheck = false;
			this.Player1Right.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Player1Right.Key = System.Windows.Forms.Keys.None;
			this.Player1Right.Location = new System.Drawing.Point(101, 143);
			this.Player1Right.Name = "Player1Right";
			this.Player1Right.Size = new System.Drawing.Size(92, 22);
			this.Player1Right.TabIndex = 12;
			this.Player1Right.Text = "None";
			this.Player1Right.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.Player1Right.UseVisualStyleBackColor = true;
			// 
			// Player2TL
			// 
			this.Player2TL.Appearance = System.Windows.Forms.Appearance.Button;
			this.Player2TL.AutoCheck = false;
			this.Player2TL.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Player2TL.Key = System.Windows.Forms.Keys.None;
			this.Player2TL.Location = new System.Drawing.Point(199, 3);
			this.Player2TL.Name = "Player2TL";
			this.Player2TL.Size = new System.Drawing.Size(93, 22);
			this.Player2TL.TabIndex = 11;
			this.Player2TL.Text = "None";
			this.Player2TL.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.Player2TL.UseVisualStyleBackColor = true;
			// 
			// Player2TR
			// 
			this.Player2TR.Appearance = System.Windows.Forms.Appearance.Button;
			this.Player2TR.AutoCheck = false;
			this.Player2TR.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Player2TR.Key = System.Windows.Forms.Keys.None;
			this.Player2TR.Location = new System.Drawing.Point(199, 31);
			this.Player2TR.Name = "Player2TR";
			this.Player2TR.Size = new System.Drawing.Size(93, 22);
			this.Player2TR.TabIndex = 13;
			this.Player2TR.Text = "None";
			this.Player2TR.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.Player2TR.UseVisualStyleBackColor = true;
			// 
			// Player2Up
			// 
			this.Player2Up.Appearance = System.Windows.Forms.Appearance.Button;
			this.Player2Up.AutoCheck = false;
			this.Player2Up.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Player2Up.Key = System.Windows.Forms.Keys.None;
			this.Player2Up.Location = new System.Drawing.Point(199, 59);
			this.Player2Up.Name = "Player2Up";
			this.Player2Up.Size = new System.Drawing.Size(93, 22);
			this.Player2Up.TabIndex = 14;
			this.Player2Up.Text = "None";
			this.Player2Up.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.Player2Up.UseVisualStyleBackColor = true;
			// 
			// Player2Down
			// 
			this.Player2Down.Appearance = System.Windows.Forms.Appearance.Button;
			this.Player2Down.AutoCheck = false;
			this.Player2Down.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Player2Down.Key = System.Windows.Forms.Keys.None;
			this.Player2Down.Location = new System.Drawing.Point(199, 87);
			this.Player2Down.Name = "Player2Down";
			this.Player2Down.Size = new System.Drawing.Size(93, 22);
			this.Player2Down.TabIndex = 15;
			this.Player2Down.Text = "None";
			this.Player2Down.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.Player2Down.UseVisualStyleBackColor = true;
			// 
			// Player2Left
			// 
			this.Player2Left.Appearance = System.Windows.Forms.Appearance.Button;
			this.Player2Left.AutoCheck = false;
			this.Player2Left.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Player2Left.Key = System.Windows.Forms.Keys.None;
			this.Player2Left.Location = new System.Drawing.Point(199, 115);
			this.Player2Left.Name = "Player2Left";
			this.Player2Left.Size = new System.Drawing.Size(93, 22);
			this.Player2Left.TabIndex = 16;
			this.Player2Left.Text = "None";
			this.Player2Left.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.Player2Left.UseVisualStyleBackColor = true;
			// 
			// Player2Right
			// 
			this.Player2Right.Appearance = System.Windows.Forms.Appearance.Button;
			this.Player2Right.AutoCheck = false;
			this.Player2Right.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Player2Right.Key = System.Windows.Forms.Keys.None;
			this.Player2Right.Location = new System.Drawing.Point(199, 143);
			this.Player2Right.Name = "Player2Right";
			this.Player2Right.Size = new System.Drawing.Size(93, 22);
			this.Player2Right.TabIndex = 17;
			this.Player2Right.Text = "None";
			this.Player2Right.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.Player2Right.UseVisualStyleBackColor = true;
			// 
			// ConsolePause
			// 
			this.ConsolePause.Appearance = System.Windows.Forms.Appearance.Button;
			this.ConsolePause.AutoCheck = false;
			this.ConsolePause.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsolePause.Key = System.Windows.Forms.Keys.None;
			this.ConsolePause.Location = new System.Drawing.Point(101, 3);
			this.ConsolePause.Name = "ConsolePause";
			this.ConsolePause.Size = new System.Drawing.Size(191, 23);
			this.ConsolePause.TabIndex = 18;
			this.ConsolePause.Text = "None";
			this.ConsolePause.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.ConsolePause.UseVisualStyleBackColor = true;
			// 
			// ConsoleReset
			// 
			this.ConsoleReset.Appearance = System.Windows.Forms.Appearance.Button;
			this.ConsoleReset.AutoCheck = false;
			this.ConsoleReset.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsoleReset.Key = System.Windows.Forms.Keys.None;
			this.ConsoleReset.Location = new System.Drawing.Point(101, 32);
			this.ConsoleReset.Name = "ConsoleReset";
			this.ConsoleReset.Size = new System.Drawing.Size(191, 23);
			this.ConsoleReset.TabIndex = 20;
			this.ConsoleReset.Text = "None";
			this.ConsoleReset.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.ConsoleReset.UseVisualStyleBackColor = true;
			// 
			// ConsoleStart
			// 
			this.ConsoleStart.Appearance = System.Windows.Forms.Appearance.Button;
			this.ConsoleStart.AutoCheck = false;
			this.ConsoleStart.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsoleStart.Key = System.Windows.Forms.Keys.None;
			this.ConsoleStart.Location = new System.Drawing.Point(101, 61);
			this.ConsoleStart.Name = "ConsoleStart";
			this.ConsoleStart.Size = new System.Drawing.Size(191, 26);
			this.ConsoleStart.TabIndex = 19;
			this.ConsoleStart.Text = "None";
			this.ConsoleStart.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.ConsoleStart.UseVisualStyleBackColor = true;
			// 
			// TabIcons
			// 
			this.TabIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.TabIcons.ImageSize = new System.Drawing.Size(16, 16);
			this.TabIcons.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// ControlEditor
			// 
			this.AcceptButton = this.ButtonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.ButtonCancel;
			this.ClientSize = new System.Drawing.Size(321, 364);
			this.Controls.Add(this.InputTabs);
			this.Controls.Add(this.PanelOKCancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "ControlEditor";
			this.Padding = new System.Windows.Forms.Padding(3);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Customise Controls";
			this.InputTabs.ResumeLayout(false);
			this.KeyboardTab.ResumeLayout(false);
			this.JoypadGroup.ResumeLayout(false);
			this.JoypadTable.ResumeLayout(false);
			this.JoypadTable.PerformLayout();
			this.ConsoleGroup.ResumeLayout(false);
			this.ConsoleTable.ResumeLayout(false);
			this.ConsoleTable.PerformLayout();
			this.PanelOKCancel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl InputTabs;
		private System.Windows.Forms.TabPage KeyboardTab;
		private System.Windows.Forms.GroupBox JoypadGroup;
		private System.Windows.Forms.TableLayoutPanel JoypadTable;
		private System.Windows.Forms.Label LabelButton1;
		private System.Windows.Forms.Label LabelButton2;
		private System.Windows.Forms.Label LabelUp;
		private System.Windows.Forms.Label LabelDown;
		private System.Windows.Forms.Label LabelLeft;
		private System.Windows.Forms.Label LabelRight;
		private KeyButton Player1TL;
		private KeyButton Player1Left;
		private KeyButton Player1Down;
		private KeyButton Player1Up;
		private KeyButton Player1TR;
		private KeyButton Player2TL;
		private KeyButton Player1Right;
		private KeyButton Player2TR;
		private KeyButton Player2Up;
		private KeyButton Player2Down;
		private KeyButton Player2Left;
		private KeyButton Player2Right;
		private System.Windows.Forms.GroupBox ConsoleGroup;
		private System.Windows.Forms.TableLayoutPanel ConsoleTable;
		private KeyButton ConsoleReset;
		private KeyButton ConsoleStart;
		private KeyButton ConsolePause;
		private System.Windows.Forms.Label LabelPause;
		private System.Windows.Forms.Label LabelReset;
		private System.Windows.Forms.Label LabelStart;
		private System.Windows.Forms.Panel PanelOKCancel;
		private System.Windows.Forms.Button ButtonOK;
		private System.Windows.Forms.Button ButtonCancel;
		private System.Windows.Forms.ImageList TabIcons;
	}
}