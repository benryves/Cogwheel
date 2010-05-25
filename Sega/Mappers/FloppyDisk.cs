using System;

namespace BeeDevelopment.Sega8Bit.Mappers {
	class FloppyDisk : IMemoryMapper {

		private byte[] data;

		public FloppyDisk() {
			this.Reset();
		}

		public byte ReadMemory(ushort address) {
			throw new InvalidOperationException("You cannot read floppy disks directly.");
		}

		public void WriteMemory(ushort address, byte value) {
			throw new InvalidOperationException("You cannot write to floppy disks directly.");
		}

		public void Reset() {
			this.data = new byte[160 * 1024];
		}

		public void Load(byte[] data) {
			Array.Copy(data, this.data, Math.Min(this.data.Length, data.Length));
		}

		public int Crc32 {
			get { return Zip.Crc32.Calculate(this.data); }
		}
	}
}
