using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BeeDevelopment.Sega8Bit.Hardware.Controllers {
	public class SerialPort {

		private Emulator emulator;

		public bool ConnectedToEmulator {
			get; set;
		}

		private int baudRate = 9600;
		public int BaudRate {
			get {
				return this.baudRate;
			}
			set {
				this.baudRate = value;
			}
		}
		private int stopBits = 2;
		public int StopBits {
			get {
				return this.stopBits;
			}
			set {
				this.stopBits = value;
			}
		}


		public bool TxD { // Master System OUT
			get;
			private set;
		}

		public bool RTS { // Master System OUT
			get;
			private set;
		}

		public bool RxD { // Master System IN
			get;
			private set;
		}

		private bool cts = true;
		public bool CTS { // Master System IN
			get {
				return cts;
			}
			set {
				this.cts = value;
				this.UpdateState();
			}
		}

		private Queue<KeyValuePair<int, bool>> transmitBitQueue = new Queue<KeyValuePair<int, bool>>();


		private bool receivingData;
		private Queue<KeyValuePair<int, bool>> receiveBitQueue = new Queue<KeyValuePair<int, bool>>();
		private int receiveStartTime = 0;
		private int receiveEndTime = 0;

		public SerialPort(Emulator emulator) {
			this.emulator = emulator;
			this.TxD = true;
			this.RxD = true;
			this.CTS = true;
			this.RTS = true;
			this.receivingData = false;
		}

		int lastUpdatedState = 0;

		public void UpdateState() {

			lastUpdatedState = this.emulator.TotalExecutedCycles;

			if (!this.ConnectedToEmulator) {
				this.transmitBitQueue.Clear();
				this.receiveBitQueue.Clear();
				this.receivingData = false;
				return;
			}

			// Has the the RTS status changed?
			var oldRTS = this.RTS;
			this.RTS = this.emulator.SegaPorts[1].TR.Direction == PinDirection.Output ? this.emulator.SegaPorts[1].TR.State : true;
			if (RTS != oldRTS) {
				this.OnRtsChanged(new EventArgs());
			}

			// Copy the CTS state.
			this.emulator.SegaPorts[1].Up.State = !this.CTS;

			if (this.transmitBitQueue.Count > 0) {
				var headOfQueue = this.transmitBitQueue.Peek();
				if (headOfQueue.Key < this.lastUpdatedState) {
					this.TxD = headOfQueue.Value;
					this.transmitBitQueue.Dequeue();
				}
			} else if (!this.RTS && this.writeBuffer.Count > 0 && this.transmitBitQueue.Count == 0) {
				var value = this.writeBuffer.Dequeue();
				this.RawWrite(value);
				this.TxD = true;
			} else {
				this.TxD = true;
			}

			this.emulator.SegaPorts[1].Down.State = this.TxD;

			this.RxD = this.emulator.SegaPorts[1].TH.Direction == PinDirection.Output ? this.emulator.SegaPorts[1].TH.State : true;

			if (!receivingData) {
				if (!this.RxD) {
					this.receivingData = true;
					this.receiveBitQueue.Clear();
					this.receiveBitQueue.Enqueue(new KeyValuePair<int, bool>(this.emulator.TotalExecutedCycles, this.RxD));
					this.receiveStartTime = this.emulator.TotalExecutedCycles;
					this.receiveEndTime = this.receiveStartTime + 10 * this.emulator.ClockSpeed / baudRate; // 10 bits: start, 8 data bits, stop.
				}
			} else {

				this.receiveBitQueue.Enqueue(new KeyValuePair<int, bool>(emulator.TotalExecutedCycles, RxD));

				if (this.lastUpdatedState > this.receiveEndTime) {

					this.receivingData = false;

					// Sample the recorded data.

					bool currentRxD = true;  // Start bit should always be false.

					int data = 0;
					for (int i = 0, sampleTime = this.receiveStartTime + (this.emulator.ClockSpeed / baudRate) / 2; i < 10; ++i, sampleTime += (this.emulator.ClockSpeed / baudRate)) {

						while (this.receiveBitQueue.Count > 0) {
							var newRxD = this.receiveBitQueue.Peek();
							if (newRxD.Key > sampleTime) {
								break;
							} else {
								newRxD = this.receiveBitQueue.Dequeue();
								currentRxD = newRxD.Value;
							}
						}

						if (currentRxD) data |= (1 << i);

					}

					if ((data & 1) == 0 && ((data >> 9) & 1) == 1) {
						this.OnDataRecieved(new SerialDataReceivedEventArgs((byte)(data >> 1)));
					}
				}
			}

		}

		public event EventHandler<SerialDataReceivedEventArgs> DataRecieved;

		protected virtual void OnDataRecieved(SerialDataReceivedEventArgs e) {
			DataRecieved?.Invoke(this, e);
		}

		public event EventHandler<EventArgs> RtsChanged;

		protected virtual void OnRtsChanged(EventArgs e) {
			RtsChanged?.Invoke(this, e);
		}

		private Queue<byte> writeBuffer = new Queue<byte>();

		public void Write(byte value) {
			this.UpdateState();
			if (this.RTS || this.writeBuffer.Count > 0 || this.transmitBitQueue.Count > 0) {
				// Not clear to send right now.
				this.writeBuffer.Enqueue(value);
			} else {
				this.RawWrite(value);
			}
		}


		public void Write(IEnumerable<byte> values) {
			foreach (var item in values) {
				this.Write(item);
			}
		}


		int nextWriteTime = 0;
		private void RawWrite(byte value) {
			
			int minNextWriteTime = this.lastUpdatedState;

			if (nextWriteTime < minNextWriteTime) {
				nextWriteTime = minNextWriteTime;
			}

			this.Write(false); // Start bit
			for (int i = 0; i < 8; ++i) {
				this.Write((value & (1 << i)) != 0); // Data bit
			}
			for (int i = 0; i < stopBits; ++i) {
				this.Write(true); // Stop bit
			}
		}

		private void Write(bool value) {
			this.transmitBitQueue.Enqueue(new KeyValuePair<int, bool>(nextWriteTime += this.emulator.ClockSpeed / this.baudRate, value));
		}

	}

	public class SerialDataReceivedEventArgs : EventArgs {

		public byte Data { get; private set; }
		public SerialDataReceivedEventArgs(byte data) {
			this.Data = data;
		}
	}
}
