using System;
using System.Collections.Generic;
using System.Text;

namespace BeeDevelopment.Sega8Bit.Hardware {
	public class FloppyDiskController {

		public void Reset() {
			this.Int = true;
		}

		public bool Int { get; set; }

		public byte ReadStatus() {
			/*
			 * b0..3  DB  FDD0..3 Busy (seek/recalib active, until succesful sense intstat)
			 * b4     CB  FDC Busy (still in command-, execution- or result-phase)
			 * b5     EXM Execution Mode (still in execution-phase, non_DMA_only)
			 * b6     DIO Data Input/Output (0=CPU->FDC, 1=FDC->CPU) (see b7)
			 * b7     RQM Request For Master (1=ready for next byte) (see b6 for direction)
			 */
			return 0x80;
		}

		public byte ReadData() {
			return 0x00;
		}

		public void WriteData(byte data) {
		}

	}
}
