using System;
using System.Collections.Generic;
using System.Text;

namespace BeeDevelopment.Sega8Bit.Hardware {
	public class FloppyDiskController {

		private Queue<byte> responseQueue = new Queue<byte>();

		public void Reset() {
			this.InUse = false;
			this.MotorOn = false;
		}

		public bool Int {
			get {
				return this.responseQueue.Count > 0;
			}
		}
		
		public bool InUse { get; set; }
		public bool MotorOn { get; set; }

		public byte TrackNumber { get; set; }

		public bool Index {
			get {
				return false;
			}
		}

		public byte ReadStatus() {
			/*
			 * b0..3  DB  FDD0..3 Busy (seek/recalib active, until succesful sense intstat)
			 * b4     CB  FDC Busy (still in command-, execution- or result-phase)
			 * b5     EXM Execution Mode (still in execution-phase, non_DMA_only)
			 * b6     DIO Data Input/Output (0=CPU->FDC, 1=FDC->CPU) (see b7)
			 * b7     RQM Request For Master (1=ready for next byte) (see b6 for direction)
			 */
			if (this.responseQueue.Count > 0) {
				return 0xC0;
			} else {
				return 0x80;
			}
		}

		public byte ReadData() {
			return responseQueue.Dequeue();
		}

		public void WriteData(byte data) {
			if (data == 0x08) {
				// Sense interrupt state
				responseQueue.Enqueue(this.ReadStatus());
				responseQueue.Enqueue(this.TrackNumber);
			} else {
				throw new InvalidOperationException();
			}
		}

	}
}
