using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using System.Text;
using System.Windows.Forms;

namespace BeeDevelopment.Cogwheel {
	public partial class SerialTerminal : Form {

		private SerialPort connectedPort;
		public SerialTerminal() {
			InitializeComponent();

			for (int baudRate = 38400; baudRate >= 300; baudRate /= 2) {
				var baudRateDropdownMenuItem = new ToolStripMenuItem {
					Text = baudRate.ToString(),
					Tag = baudRate
				};
				BaudRateMenuItem.DropDownItems.Add(baudRateDropdownMenuItem);
				baudRateDropdownMenuItem.Click += BaudRateDropdownMenuItem_Click;
			}

			BaudRateMenuItem.DropDownItems.Add(new ToolStripSeparator());
			for (int baudRate = 31250; baudRate != 0; baudRate = 0) {
				var baudRateDropdownMenuItem = new ToolStripMenuItem {
					Text = baudRate.ToString(),
					Tag = baudRate
				};
				BaudRateMenuItem.DropDownItems.Add(baudRateDropdownMenuItem);
				baudRateDropdownMenuItem.Click += BaudRateDropdownMenuItem_Click;
			}



			if (SerialPort.GetPortNames().Length > 0) {
				ConnectToMenuItem.DropDownItems.Add(new ToolStripSeparator());
				foreach (var serialPortName in SerialPort.GetPortNames()) {
					var connectToDropdownMenuItem = new ToolStripMenuItem {
						Text = serialPortName,
						Tag = serialPortName
					};
					ConnectToMenuItem.DropDownItems.Add(connectToDropdownMenuItem);
					connectToDropdownMenuItem.Click += ConnectToDropdownMenuItem_Click;
				}
			}

			this.Disposed += SerialTerminal_Disposed;

		}

		private void SerialTerminal_Disposed(object sender, EventArgs e) {
			if (this.connectedPort != null) {
				this.connectedPort.Close();
				this.connectedPort.Dispose();
				this.connectedPort = null;
			}
		}

		private void ConnectToDropdownMenuItem_Click(object sender, EventArgs e) {
			var connectToDropdownMenuItem = sender as ToolStripMenuItem;
			if (connectToDropdownMenuItem != null) {

				var emulator = this.GetEmulator();
				if (emulator == null) return;

				var portName = connectToDropdownMenuItem.Tag as string;

				if (this.connectedPort != null) {
					this.connectedPort.Close();
					this.connectedPort.Dispose();
					this.connectedPort = null;
				}

				if (!string.IsNullOrEmpty(portName)) {
					try {
						this.connectedPort = new SerialPort(portName, emulator.SerialPort.BaudRate, Parity.None, 8, StopBits.One);
						//this.connectedPort.Handshake = Handshake.RequestToSend; // <- no handshaking, please, as 
						this.connectedPort.DtrEnable = true;
						this.connectedPort.RtsEnable = emulator.SerialPort.RTS;
						this.connectedPort.Open();
					} catch (Exception ex) {
						MessageBox.Show(this, "Could not open serial port: " + ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
						this.connectedPort = null;
						return;
					}

					this.connectedPort.DataReceived += ConnectedPort_DataReceived;
					this.connectedPort.PinChanged += ConnectedPort_PinChanged;
				}
			}
		}

		

		private void SerialPort_DataRecieved(object sender, Sega8Bit.Hardware.Controllers.SerialDataReceivedEventArgs e) {
			Debug.WriteLine(string.Format("{0:X2} -> From emulator", e.Data));
			this.ConsoleOutput.Text += (char)e.Data;
			
			this.ConsoleOutput.SelectionLength = 0;
			this.ConsoleOutput.SelectionStart = this.ConsoleOutput.Text.Length;
			this.ConsoleOutput.ScrollToCaret();

			if (this.connectedPort != null && this.connectedPort.IsOpen) {
				if (this.connectedPort.CtsHolding) {
					this.connectedPort.Write(new[] { e.Data }, 0, 1);
				}
			}
		}
		private void SerialPort_RtsChanged(object sender, EventArgs e) {
			var emulator = this.GetEmulator();
			if (emulator != null) {
				Debug.WriteLine(string.Format("RTS={0} -> From emulator", emulator.SerialPort.RTS));
				if (this.connectedPort != null && this.connectedPort.IsOpen) {
					//this.connectedPort.RtsEnable = emulator.SerialPort.RTS; // <- causes problems?
				}
			}
		}

		private void ConnectedPort_DataReceived(object sender, SerialDataReceivedEventArgs e) {
			Debug.WriteLine("RECEIVED DATA FROM REAL SERIAL PORT!");
			var emulator = this.GetEmulator();
			if (this.connectedPort != null && this.connectedPort.IsOpen) {
				while (this.connectedPort.BytesToRead > 0) {
					var data = (byte)connectedPort.ReadByte();
					Debug.WriteLine(string.Format("{0:X2} <- To emulator", data));
					emulator.SerialPort.Write(data);
				}
			}
		}

		private void ConnectedPort_PinChanged(object sender, SerialPinChangedEventArgs e) {
			var emulator = this.GetEmulator();
			if (this.connectedPort != null && this.connectedPort.IsOpen) {
				switch (e.EventType) {
					case SerialPinChange.CtsChanged:
						Debug.WriteLine(string.Format("CTS={0} <- To emulator", this.connectedPort.CtsHolding));
						emulator.SerialPort.CTS = this.connectedPort.CtsHolding;
						break;
				}
			}
		}

		private Sega8Bit.Emulator previousEmulator = null;
		private Sega8Bit.Emulator GetEmulator() {
			var EmulatorForm = this.Owner as MainForm;
			if (EmulatorForm == null) {
				previousEmulator = null;
			} else {
				if (EmulatorForm.Emulator != previousEmulator) {
					previousEmulator = EmulatorForm.Emulator;
					if (previousEmulator != null) {
						previousEmulator.SerialPort.DataRecieved += SerialPort_DataRecieved;
						previousEmulator.SerialPort.RtsChanged += SerialPort_RtsChanged;
					}
				}
			}
			return previousEmulator;
		}

		private void BaudRateDropdownMenuItem_Click(object sender, EventArgs e) {
			var baudRateDropdownMenuItem = sender as ToolStripMenuItem;
			if (baudRateDropdownMenuItem != null) {
				var emulator = this.GetEmulator();
				if (emulator != null) {
					emulator.SerialPort.BaudRate = (int)baudRateDropdownMenuItem.Tag;
				}
				if (this.connectedPort != null) {
					this.connectedPort.Close();
					this.connectedPort.BaudRate = (int)baudRateDropdownMenuItem.Tag;
					this.connectedPort.Open();
				}
			}
		}

		private void ConsoleInput_KeyPress(object sender, KeyPressEventArgs e) {
			if (e.KeyChar == '\r') {
				var emulator = this.GetEmulator();
				if (emulator != null) {
					emulator.SerialPort.Write(Encoding.ASCII.GetBytes(this.ConsoleInput.Text.Trim() + "\r"));
					this.ConsoleInput.Text = "";
				}
			}
		}

		private void BaudRateMenuItem_DropDownOpening(object sender, EventArgs e) {
			var emulator = this.GetEmulator();
			foreach (ToolStripItem item in BaudRateMenuItem.DropDownItems) {
				var menuItem = item as ToolStripMenuItem;
				if (menuItem != null) {
					menuItem.Checked = (emulator != null && emulator.SerialPort.BaudRate == (int)menuItem.Tag);
				}
			}
		}

		private void ConnectToMenuItem_DropDownOpening(object sender, EventArgs e) {
			this.ConnectToNoneMenuItem.Checked = this.connectedPort == null;
			foreach (ToolStripItem item in ConnectToMenuItem.DropDownItems) {
				var menuItem = item as ToolStripMenuItem;
				if (menuItem != null) {
					var portName = menuItem.Tag as string;
					if (portName != null) {
						menuItem.Checked = this.connectedPort != null && this.connectedPort.PortName == portName;
					}
				}
			}
		}

		private void SerialTerminal_Load(object sender, EventArgs e) {
			var emulator = this.GetEmulator();
			if (!emulator.HasSerialPort) {
				this.Close();
			}
			if (!emulator.SerialPort.ConnectedToEmulator) {
				switch (MessageBox.Show(this, "The emulator doesn't have a serial cable plugged in at the moment." + Environment.NewLine + "Would you like to plug one in?", this.Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1)) {
					case DialogResult.Yes:
						emulator.SerialPort.ConnectedToEmulator = true;
						break;
					case DialogResult.No:
						emulator.SerialPort.ConnectedToEmulator = false;
						break;
					case DialogResult.Cancel:
						this.Close();
						break;
				}
			}
			if (emulator.SerialPort.ConnectedToEmulator && emulator.HasCassetteRecorder) {
				emulator.CassetteRecorder.ConnectedToEmulator = false;
			}
		}
	}
}
