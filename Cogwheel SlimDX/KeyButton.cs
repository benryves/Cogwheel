using System.Windows.Forms;

namespace CogwheelSlimDX {
	class KeyButton : CheckBox {

		private Keys key;
		public Keys Key {
			get { return this.key; }
			set { this.key = value; base.Text = key.ToString(); }
		}

		public override string Text {
			get {
				return base.Text;
			}
			set { }
		}

		public KeyButton() {
			this.Key = Keys.None;
			this.Appearance = Appearance.Button;
			this.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.AutoCheck = false;
		}

		protected override void OnLostFocus(System.EventArgs e) {
			this.Checked = false;
			base.OnLostFocus(e);
		}

		protected override void OnMouseDown(MouseEventArgs mevent) {
			switch (mevent.Button) {
				case MouseButtons.Left:
					this.Checked = true;
					break;
				case MouseButtons.Right:
					this.Checked = false;
					this.Key = Keys.None;
					break;
			}
		}

		protected override bool IsInputKey(Keys keyData) {
			return this.Checked;
		}

		protected override void OnKeyDown(KeyEventArgs e) {
			if (this.Checked) {
				e.Handled = true;
				this.Key = e.KeyCode;
				this.Checked = false;
			}
			base.OnKeyDown(e);
		}


	}
}
