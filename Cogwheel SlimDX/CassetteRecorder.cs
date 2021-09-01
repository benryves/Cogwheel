using BeeDevelopment.Sega8Bit;
using BeeDevelopment.Sega8Bit.Hardware.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace BeeDevelopment.Cogwheel {
	public partial class CassetteRecorder : Form {
		public CassetteRecorder() {
			InitializeComponent();
		}

		private Sega8Bit.Emulator GetEmulator() {
			var EmulatorForm = this.Owner as MainForm;
			if (EmulatorForm == null) return null;
			return EmulatorForm.Emulator;
		}

		private void PlayButton_Click(object sender, EventArgs e) {
			var emulator = this.GetEmulator();
			if ((emulator != null) && emulator.HasCassetteRecorder) {
				emulator.CassetteRecorder.Play();
			}
		}

		private UnifiedEmulatorFormat previousTape = null;

		public void Refresh(Emulator emulator) {
			if (!emulator.HasCassetteRecorder) {
				this.Enabled = false;
				return;
			} else {
				this.Enabled = true;
				
				var cassetteRecorder = emulator.CassetteRecorder;

				if (cassetteRecorder.Tape == null) {

					this.BlockList.Items.Clear();

					this.TapeProgressControls.Enabled = false;
					this.TapeProgress.Value = 0;
					this.TapeProgress.Maximum = 600000;
					this.TapeCounterLength.Text = this.TapeCounterPosition.Text = "--:--";

					this.PlayButton.Checked = this.StopButton.Checked = this.RewindButton.Checked = this.FFwdButton.Checked = false;

				} else {
					this.TapeProgressControls.Enabled = true;

					// Populate the block list.
					var currentTape = cassetteRecorder.Tape;
					if (currentTape != previousTape) {

						var tapeStream = currentTape.ToTapeBitStream();

						var tapeBlocks = tapeStream.GetDataBlocks();

						this.BlockList.Groups.Clear();
						this.BlockList.Items.Clear();
						
						var bitstreamIndices = new int[currentTape.Chunks.Length];

						ListViewGroup currentGroup = null;
						List<byte> currentGroupData = null;

						foreach (var item in tapeBlocks) {

							var headerReader = new BinaryReader(new MemoryStream(item.Value));

							// Read up to sync byte.
							do {
								if (headerReader.BaseStream.Position == headerReader.BaseStream.Length) {
									break;
								}
							} while (headerReader.ReadByte() != 0x2A);

							// Is there any data left?
							if ((headerReader.BaseStream.Length - headerReader.BaseStream.Position) < 16) {
								continue;
							}


							List<byte> filename = new List<byte>();
							{
								byte b;
								while ((b = headerReader.ReadByte()) != 0 && filename.Count < 10) {
									filename.Add(b);
								}
							}


							var blockName = Encoding.ASCII.GetString(filename.ToArray());

							// Is there enough data left?
							if ((headerReader.BaseStream.Length - headerReader.BaseStream.Position) < 19) {
								continue;
							}

							headerReader.ReadUInt32(); // load address
							headerReader.ReadUInt32(); // execution address
							var blockNumber = headerReader.ReadUInt16();
							var dataLength = headerReader.ReadUInt16();
							headerReader.ReadByte(); // flag
							headerReader.ReadUInt32(); // address of next file
							headerReader.ReadUInt16(); // header CRC

							if (currentGroup == null || currentGroup.Header != blockName) {
								currentGroupData = new List<byte>();
								currentGroup = new ListViewGroup {
									Header = blockName,
									Tag = currentGroupData,
								};
								this.BlockList.Groups.Add(currentGroup);
							}

							// Is there enough data left?
							if ((headerReader.BaseStream.Length - headerReader.BaseStream.Position) < dataLength) {
								continue;
							}

							currentGroupData.AddRange(headerReader.ReadBytes(dataLength));

							var seekTime = Math.Max(0, item.Key - 2400);
							var time = TimeSpan.FromSeconds(seekTime / 4800d);

							var blockSkipItem = new ListViewItem {
								Text = string.Format("{0}:{1:00}", time.Minutes, time.Seconds),
								Tag = time,
								Group = currentGroup,
							};

							blockSkipItem.SubItems.Add(blockName);
							blockSkipItem.SubItems.Add(blockNumber.ToString("X2"));
							blockSkipItem.SubItems.Add(dataLength.ToString("X4"));

							this.BlockList.Items.Add(blockSkipItem);

						}

						this.previousTape = currentTape;
					}

					var tapePosition = cassetteRecorder.TapePosition;
					var tapeLength = cassetteRecorder.TapeLength;

					var value = (int)tapePosition.TotalMilliseconds;
					var maximum = (int)tapeLength.TotalMilliseconds;
					if (value > maximum) value = maximum; // <- just in case!

					if (value > this.TapeProgress.Maximum) {
						this.TapeProgress.Maximum = maximum;
						this.TapeProgress.Value = value;
					} else {
						this.TapeProgress.Value = value;
						this.TapeProgress.Maximum = maximum;
					}

					this.PlayButton.Checked = cassetteRecorder.PlayState == CassetteRecorderPlayState.Playing;
					this.StopButton.Checked = cassetteRecorder.PlayState == CassetteRecorderPlayState.Stopped;
					this.RewindButton.Checked = cassetteRecorder.PlayState == CassetteRecorderPlayState.Rewinding;
					this.FFwdButton.Checked = cassetteRecorder.PlayState == CassetteRecorderPlayState.FastForwarding;

					this.TapeCounterLength.Text = string.Format("{0}:{1:00}", tapeLength.Minutes, tapeLength.Seconds);
					this.TapeCounterPosition.Text = string.Format("{0}:{1:00}", tapePosition.Minutes, tapePosition.Seconds);

					try {
						this.changingBlockListCheckboxes = true;
						var foundActive = false;
						for (int i = this.BlockList.Items.Count - 1; i >= 0; i--) {
							ListViewItem blockListItem = this.BlockList.Items[i];
							var tag = blockListItem.Tag;
							if (tag == null) return;
							var seekTime = (TimeSpan)tag;
							var active = !foundActive && tapePosition >= seekTime;
							if (active != blockListItem.Checked) {
								blockListItem.Checked = active;
								if (active) blockListItem.EnsureVisible();
							}
							foundActive |= active;
						}
					} finally {
						this.changingBlockListCheckboxes = false;
					}
				}
			}
		}

		private void CassetteFileOpenMenu_Click(object sender, EventArgs e) {
			
			var emulator = this.GetEmulator();
			if (emulator == null) return;
			if (!emulator.HasCassetteRecorder) return;

			if (this.OpenCassetteDialog.ShowDialog(this) == DialogResult.OK) {
				try {
					emulator.CassetteRecorder.Stop();
					emulator.CassetteRecorder.Tape = UnifiedEmulatorFormat.FromFile(this.OpenCassetteDialog.FileName);
				} catch (Exception ex) {
					MessageBox.Show(this, "There was an error loading the UEF: " + ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private void EjectButton_Click(object sender, EventArgs e) {
			var emulator = this.GetEmulator();
			if (emulator == null) return;
			if (!emulator.HasCassetteRecorder) return;
			emulator.CassetteRecorder.Stop();
			emulator.CassetteRecorder.Tape = null;
		}

		private void StopButton_Click(object sender, EventArgs e) {
			var emulator = this.GetEmulator();
			if (emulator == null) return;
			if (!emulator.HasCassetteRecorder) return;
			emulator.CassetteRecorder.Stop();
		}

		private void RewindButton_CheckedChanged(object sender, EventArgs e) {
			var emulator = this.GetEmulator();
			if (emulator == null) return;
			if (!emulator.HasCassetteRecorder) return;
			emulator.CassetteRecorder.Rewind();
		}

		private void FFwdButton_CheckedChanged(object sender, EventArgs e) {
			var emulator = this.GetEmulator();
			if (emulator == null) return;
			if (!emulator.HasCassetteRecorder) return;
			emulator.CassetteRecorder.FastForward();
		}

		bool changingBlockListCheckboxes = false;

		private void BlockList_ItemChecked(object sender, ItemCheckedEventArgs e) {
			if (!this.changingBlockListCheckboxes && e.Item.Checked) {
				var tag = e.Item.Tag;
				if (tag == null) return;
				var seekTime = (TimeSpan)tag;
				e.Item.Checked = false;
				var emulator = this.GetEmulator();
				if (emulator == null) return;
				if (!emulator.HasCassetteRecorder) return;
				emulator.CassetteRecorder.TapePosition = seekTime;
			}
		}

		private void CassetteInvertPhaseMenuItem_Click(object sender, EventArgs e) {
			Emulator emulator = this.GetEmulator();
			if (emulator != null) {
				emulator.CassetteRecorder.InvertLevel ^= true;
			}
		}

		private void CassetteOptionsMenu_DropDownOpening(object sender, EventArgs e) {
			Emulator emulator = this.GetEmulator();
			if (emulator != null) {
				CassetteInvertPhaseMenuItem.Checked = emulator.CassetteRecorder.InvertLevel;
			}
		}

		private void CassetteFileMenu_DropDownOpening(object sender, EventArgs e) {
			this.CassetteFileExport.Enabled = this.BlockList.SelectedItems.Count > 0;
		}

		private void CassetteFileExport_Click(object sender, EventArgs e) {
			try {
				List<ListViewGroup> filesToExport = new List<ListViewGroup>();
				foreach (ListViewItem item in this.BlockList.SelectedItems) {
					if (!filesToExport.Contains(item.Group as ListViewGroup)) {
						filesToExport.Add(item.Group as ListViewGroup);
					}
				}
				if (filesToExport.Count == 1) {
					var file = filesToExport[0];
					this.ExportFileDialog.FileName = file.Header;
					if (this.ExportFileDialog.ShowDialog(this) == DialogResult.OK) {
						File.WriteAllBytes(this.ExportFileDialog.FileName, (file.Tag as List<byte>).ToArray());
					}
				} else if (filesToExport.Count > 1) {
					if (this.ExportFolderDialog.ShowDialog(this) == DialogResult.OK) {
						foreach (ListViewGroup file in filesToExport) {
							File.WriteAllBytes(Path.Combine(this.ExportFolderDialog.SelectedPath, file.Header), (file.Tag as List<byte>).ToArray());
						}
					}
				}
			} catch (Exception ex) {
				MessageBox.Show(this, "There was an error exporting the file: " + ex.Message, "Export", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}
}
