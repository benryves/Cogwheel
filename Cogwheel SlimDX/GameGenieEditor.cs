using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using BeeDevelopment.Sega8Bit;

namespace CogwheelSlimDX {
	public partial class GameGenieEditor : Form {

		public MemoryCheatCollection Collection { get; private set; }

		public GameGenieEditor(MemoryCheatCollection collection) {
			InitializeComponent();
			this.Collection = collection;
			foreach (var Cheat in collection) {
				this.CurrentCheatList.Items.Add(new ListViewItem() { 
					Text = Cheat.ToString(),
				});
			}
		}

		private void AddNewCheat() {
			var NewCheat = new ListViewItem();
			this.CurrentCheatList.Items.Add(NewCheat);
			NewCheat.BeginEdit();
		}

		private void AddCodeContext_Click(object sender, EventArgs e) {
			this.AddNewCheat();
		}

		private void CurrentCheatList_AfterLabelEdit(object sender, LabelEditEventArgs e) {
			if (e.Label == null) {
				e.CancelEdit = true;
				if (this.CurrentCheatList.Items[e.Item].Tag == null) {
					this.CurrentCheatList.Items.RemoveAt(e.Item);
				}
			} else {
				MemoryCheat Cheat;
				if (MemoryCheat.TryParse(e.Label, out Cheat)) {
					this.CurrentCheatList.Items[e.Item].Tag = Cheat;
					e.CancelEdit = true;
					this.CurrentCheatList.Items[e.Item].Text = Cheat.ToString();
				} else {
					MessageBox.Show(this, "The entered code is not a valid Game Genie code.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					e.CancelEdit = false;
					this.CurrentCheatList.Items[e.Item].Text = e.Label;
					this.CurrentCheatList.Items[e.Item].BeginEdit();
				}
			}

		}

		private void MainContext_Opening(object sender, CancelEventArgs e) {
			this.EditCodeContext.Enabled = this.CurrentCheatList.SelectedItems.Count == 1;
			this.RemoveCodeContext.Enabled = this.CurrentCheatList.SelectedItems.Count == 1;
		}

		private void RemoveCodeContext_Click(object sender, EventArgs e) {
			if (this.CurrentCheatList.SelectedItems.Count == 1) this.CurrentCheatList.Items.Remove(this.CurrentCheatList.SelectedItems[0]);
		}

		private void EditCodeContext_Click(object sender, EventArgs e) {
			if (this.CurrentCheatList.SelectedItems.Count == 1) this.CurrentCheatList.SelectedItems[0].BeginEdit();
		}

		private void GameGenieEditor_FormClosing(object sender, FormClosingEventArgs e) {
			this.Collection.Clear();
			foreach (ListViewItem Cheat in this.CurrentCheatList.Items) {
				this.Collection.Add(MemoryCheat.Parse(Cheat.Text));
			}
		}
	}
}
