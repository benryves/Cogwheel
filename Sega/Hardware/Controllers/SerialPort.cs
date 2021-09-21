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


		public void UpdateState() {

			if (!this.ConnectedToEmulator) {
				this.transmitBitQueue.Clear();
				this.receiveBitQueue.Clear();
				this.receivingData = false;
				return;
			}

			while (this.transmitBitQueue.Count > 0) {
				var headOfQueue = this.transmitBitQueue.Peek();
				if (headOfQueue.Key < this.emulator.TotalExecutedCycles) {
					this.TxD = headOfQueue.Value;
					this.transmitBitQueue.Dequeue();
					this.emulator.SegaPorts[1].Down.State = this.TxD;
				} else {
					break;
				}
			}

			this.RxD = this.emulator.SegaPorts[1].TH.Direction == PinDirection.Output ? this.emulator.SegaPorts[1].TH.State : true;

			if (!receivingData) {
				if (!this.RxD) {
					this.receivingData = true;
					this.receiveBitQueue.Clear();
					this.receiveBitQueue.Enqueue(new KeyValuePair<int, bool>(this.emulator.TotalExecutedCycles, this.RxD));
					this.receiveStartTime = this.emulator.TotalExecutedCycles;
					this.receiveEndTime = this.receiveStartTime + 10 * emulator.ClockSpeed / baudRate; // 10 bits: start, 8 data bits, stop.
				}
			} else {

				this.receiveBitQueue.Enqueue(new KeyValuePair<int, bool>(emulator.TotalExecutedCycles, RxD));

				if (this.emulator.TotalExecutedCycles > this.receiveEndTime) {

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

			// Has the the RTS status changed?
			var oldRTS = this.RTS;
			this.RTS = this.emulator.SegaPorts[1].TR.Direction == PinDirection.Output ? this.emulator.SegaPorts[1].TR.State : true;
			if (RTS != oldRTS) {
				this.OnRtsChanged(new EventArgs());
			}

			// Copy the CTS state.
			this.emulator.SegaPorts[1].Up.State = !this.CTS;

			if (!this.RTS && this.writeBuffer.Count > 0 && this.transmitBitQueue.Count == 0) {
				var value = this.writeBuffer.Dequeue();
				this.RawWrite(value);
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

			Debug.WriteLine(string.Format("Data out -> {0:X2}", value));

			this.UpdateState();

			if (this.RTS || this.writeBuffer.Count > 0) {
				// Not clear to send right now.
				this.writeBuffer.Enqueue(value);
				Debug.WriteLine(string.Format("Write buffer++: value={0:X2}, length={1}", value, writeBuffer.Count));
			} else {
				this.RawWrite(value);
			}
		}


		public void Write(IEnumerable<byte> values) {
			foreach (var item in values) {
				this.Write(item);
			}
		}

		private void RawWrite(byte value) {
			this.Write(false); // Start bit
			for (int i = 0; i < 8; ++i) {
				this.Write((value & (1 << i)) != 0); // Data bit
			}
			for (int i = 0; i < stopBits; ++i) {
				this.Write(true); // Stop bit
			}
		}

		int nextWriteTime = 0;
		private void Write(bool value) {

			int minNextWriteTime = this.emulator.ExpectedExecutedCycles + 1024;


			if (nextWriteTime < minNextWriteTime) {
				nextWriteTime = minNextWriteTime;
			}

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
