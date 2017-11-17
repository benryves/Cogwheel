using System;
using System.Collections.Generic;
using System.Text;

namespace BeeDevelopment.Sega8Bit.Hardware {

	/// <summary>
	/// 
	/// </summary>
	public class FloppyDiskController {

		#region Types

		/// <summary>
		/// Status flags returned by reading the status register.
		/// </summary>
		[Flags]
		public enum Status : byte {
			/// <summary>No status bits are set.</summary>
			None = 0,
			/// <summary>RQM = Request for Master = ready to send/receive data.</summary>
			RequestForMaster = 1 << 7,
			/// <summary>DIO = Data Input/Output  = 1 when CPU should read, 0 when it should write</summary>
			DataInputOutput = 1 << 6,
			/// <summary>EXM = Execution Mode     = 1 during "execution phase", 0 during "results phase"</summary>
			ExecutionMode = 1 << 5,
			/// <summary>CB  = Controller Busy    = set when controller is busy and can't accept anything</summary>
			ControllerBusy = 1 << 4,
			/// <summary>D3B = Drive 3 Busy       = set when a particular drive is busy</summary>
			Drive3Busy = 1 << 3,
			/// <summary>D2B = Drive 2 Busy       = set when a particular drive is busy</summary>
			Drive2Busy = 1 << 2,
			/// <summary>D1B = Drive 1 Busy       = set when a particular drive is busy</summary>
			Drive1Busy = 1 << 1,
			/// <summary>D0B = Drive 0 Busy       = set when a particular drive is busy</summary>
			Drive0Busy = 1 << 0,
		}

		/// <summary>
		/// Status flags returned by status register 0 (ST0).
		/// </summary>
		[Flags]
		public enum Status0 : byte {
			/// <summary>Interrupt code was OK.</summary>
			InterruptCodeOK = 0 << 6,
			/// <summary>Interrupt code was abnormal termination.</summary>
			InterruptCodeAbnormalTermination = 1 << 6,
			/// <summary>Interrupt code was invalid command.</summary>
			InterruptCodeInvalidCommand = 2 << 6,
			/// <summary>Interrupt code was FDD changed state and invalidated command.</summary>
			InterruptCodeFddChangedState = 3 << 6,
			SeekEnd = 1 << 5,
			/// <summary>FDD failure signal or recalibration failed.</summary>
			EquipmentCheck = 1 << 4,
			/// <summary>FDD not ready, or side unavailable.</summary>
			NotReady = 1 << 3,
			/// <summary>Side of disk when interrupt happened.</summary>
			HeadAddress = 1 << 2,
			/// <summary>Drive number when interrupt happened (0).</summary>
			UnitSelect0 = 0 << 0,
			/// <summary>Drive number when interrupt happened (1).</summary>
			UnitSelect1 = 1 << 0,
			/// <summary>Drive number when interrupt happened (2).</summary>
			UnitSelect2 = 2 << 0,
			/// <summary>Drive number when interrupt happened (3).</summary>
			UnitSelect3 = 3 << 0,
		}

		#endregion

		

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

		public Status ReadStatus() {
			Status result = Status.RequestForMaster;

			if (this.responseQueue.Count > 0) {
				result |= Status.DataInputOutput;
			}

			return result;
		}

		public Status0 ReadStatus0() {

			Status0 result = Status0.UnitSelect0;

			return result;

		}

		public byte ReadData() {
			return responseQueue.Dequeue();
		}

		public void WriteData(byte data) {
			if (data == 0x08) {
				// Sense interrupt state
				responseQueue.Enqueue((byte)this.ReadStatus0());
				responseQueue.Enqueue(this.TrackNumber);
			} else {
				throw new InvalidOperationException();
			}
		}

	}
}
