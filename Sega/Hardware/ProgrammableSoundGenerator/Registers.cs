using System;
namespace BeeDevelopment.Sega8Bit.Hardware {
	public partial class ProgrammableSoundGenerator {

		private ushort[] toneRegisters;
		private byte[] volumeRegisters;

		/// <summary>Gets or sets the value of tone register 0.</summary>
		public ushort ToneRegister0 {
			get { return this.toneRegisters[0]; }
			set {
				if ((value & 0xFC00) != 0) throw new OverflowException();
				this.toneRegisters[0] = value; 
			}
		}

		/// <summary>Gets or sets the value of tone register 1.</summary>
		public ushort ToneRegister1 {
			get { return this.toneRegisters[1]; }
			set {
				if ((value & 0xFC00) != 0) throw new OverflowException(); 
				this.toneRegisters[1] = value;
			}
		}

		/// <summary>Gets or sets the value of tone register 2.</summary>
		public ushort ToneRegister2 {
			get { return this.toneRegisters[2]; }
			set {
				this.toneRegisters[2] = value;
				if ((value & 0xFC00) != 0) throw new OverflowException();
			}
		}

		/// <summary>Gets or sets the value of tone register 3.</summary>
		public ushort ToneRegister3 {
			get { return this.toneRegisters[3]; }
			set {
				if ((value & 0xFC00) != 0) throw new OverflowException();
				this.toneRegisters[3] = value;
			}
		}

		/// <summary>Gets or sets the value of volume register 0.</summary>
		public byte VolumeRegister0 {
			get { return this.volumeRegisters[0]; }
			set {
				if ((value & 0xF0) != 0) throw new OverflowException();
				this.volumeRegisters[0] = value; 
			}
		}

		/// <summary>Gets or sets the value of volume register 1.</summary>
		public byte VolumeRegister1 {
			get { return this.volumeRegisters[1]; }
			set {
				if ((value & 0xF0) != 0) throw new OverflowException();
				this.volumeRegisters[1] = value;
			}
		}

		/// <summary>Gets or sets the value of volume register 2.</summary>
		public byte VolumeRegister2 {
			get { return this.volumeRegisters[2]; }
			set {
				if ((value & 0xF0) != 0) throw new OverflowException();
				this.volumeRegisters[2] = value;
			}
		}

		/// <summary>Gets or sets the value of volume register 3.</summary>
		public byte VolumeRegister3 {
			get { return this.volumeRegisters[3]; }
			set {
				if ((value & 0xF0) != 0) throw new OverflowException();
				this.volumeRegisters[3] = value;
			}
		}

	}
}
