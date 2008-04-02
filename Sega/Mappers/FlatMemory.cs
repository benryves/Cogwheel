using System;
using BeeDevelopment.Brazil;

namespace BeeDevelopment.Sega8Bit.Mappers {

	/// <summary>
	/// Represents a simple flat memory.
	/// </summary>
	public class FlatMemory : IMemoryMapper {

		private byte[] memory;

		/// <summary>
		/// Gets or sets the contents of memory as an array of bytes.
		/// </summary>
		public byte[] Memory {
			get { return this.memory; }
			set {
				if (value == null || value.Length != memory.Length) throw new InvalidOperationException();
				this.memory = value;
			}
		}

		/// <summary>
		/// Gets the CRC-32 checksum of the ROM contents.
		/// </summary>
		[StateNotSaved]
		public int Crc32 { get; private set; }

		/// <summary>
		/// Gets the size of the memory.
		/// </summary>
		protected virtual int Size { get { return 64 * 1024; } }

		/// <summary>
		/// Gets whether the memory is writable.
		/// </summary>
		protected virtual bool IsWritable { get { return true; } }

		/// <summary>
		/// Reads a byte from the memory.
		/// </summary>
		/// <param name="address">The address to read the data from.</param>
		/// <returns>The data read from the address.</returns>
		public byte ReadMemory(ushort address) {
			return this.memory[address % memory.Length];
		}

		/// <summary>
		/// Writes a byte to the memory.
		/// </summary>
		/// <param name="address">The address to write the data to.</param>
		/// <param name="value">The data to write to the memory.</param>
		public void WriteMemory(ushort address, byte value) {
			if (this.IsWritable) this.memory[address % memory.Length] = value;
		}

		/// <summary>
		/// Resets the <see cref="FlatMemory"/> to its default state.
		/// </summary>
		/// <remarks>This does not do anything, as the memory has no state worth saving.</remarks>
		public void Reset() { }

		/// <summary>
		/// Loads an array of bytes into the <see cref="FlatMemory"/>.
		/// </summary>
		/// <param name="data">The data to load.</param>
		public void Load(byte[] data) {
			Array.Copy(data, this.memory, Math.Min(data.Length, this.memory.Length));
			this.Crc32 = Zip.Crc32.Calculate(data);
		}

		/// <summary>
		/// Creates an instance of a <see cref="FlatMemory"/>.
		/// </summary>
		public FlatMemory() {
			// Count the number of bits in this.Size
			int BitCount = 0;
			for (int i = 0, j = this.Size; i < 32; ++i, j >>= 1) BitCount += j & 1;
			if (BitCount != 1) throw new InvalidOperationException();
			this.memory = new byte[this.Size];
			this.Load(new byte[0]);
		}
	}

	#region RAM

	/// <summary>1KB of RAM.</summary>
	[Serializable()]
	public class Ram1 : FlatMemory {
		protected override int Size { get { return 1 * 1024; } }
		public Ram1() : base() { }
	}

	/// <summary>2KB of RAM.</summary>
	[Serializable()]
	public class Ram2 : FlatMemory {
		protected override int Size { get { return 2 * 1024; } }
		public Ram2() : base() { }
	}

	/// <summary>4KB of RAM.</summary>
	[Serializable()]
	public class Ram4 : FlatMemory {
		protected override int Size { get { return 4 * 1024; } }
		public Ram4() : base() { }
	}

	/// <summary>8KB of RAM.</summary>
	[Serializable()]
	public class Ram8 : FlatMemory {
		protected override int Size { get { return 8 * 1024; } }
		public Ram8() : base() { }
	}

	/// <summary>16KB of RAM.</summary>
	[Serializable()]
	public class Ram16 : FlatMemory {
		protected override int Size { get { return 16 * 1024; } }
		public Ram16() : base() { }
	}

	/// <summary>32KB of RAM.</summary>
	[Serializable()]
	public class Ram32 : FlatMemory {
		protected override int Size { get { return 32 * 1024; } }
		public Ram32() : base() { }
	}

	/// <summary>64KB of RAM.</summary>
	[Serializable()]
	public class Ram64 : FlatMemory {
		protected override int Size { get { return 64 * 1024; } }
		public Ram64() : base() { }
	}

	#endregion

	#region ROM

	/// <summary>1KB of ROM.</summary>
	[Serializable()]
	public class Rom1 : FlatMemory {
		protected override bool IsWritable { get { return false; } }
		protected override int Size { get { return 1 * 1024; } }
		public Rom1() : base() { }
	}

	/// <summary>2KB of ROM.</summary>
	[Serializable()]
	public class Rom2 : FlatMemory {
		protected override bool IsWritable { get { return false; } }
		protected override int Size { get { return 2 * 1024; } }
		public Rom2() : base() { }
	}

	/// <summary>4KB of ROM.</summary>
	[Serializable()]
	public class Rom4 : FlatMemory {
		protected override bool IsWritable { get { return false; } }
		protected override int Size { get { return 4 * 1024; } }
		public Rom4() : base() { }
	}

	/// <summary>8KB of ROM.</summary>
	[Serializable()]
	public class Rom8 : FlatMemory {
		protected override bool IsWritable { get { return false; } }
		protected override int Size { get { return 8 * 1024; } }
		public Rom8() : base() { }
	}

	/// <summary>16KB of ROM.</summary>
	[Serializable()]
	public class Rom16 : FlatMemory {
		protected override bool IsWritable { get { return false; } }
		protected override int Size { get { return 16 * 1024; } }
		public Rom16() : base() { }
	}

	/// <summary>32KB of ROM.</summary>
	[Serializable()]
	public class Rom32 : FlatMemory {
		protected override bool IsWritable { get { return false; } }
		protected override int Size { get { return 32 * 1024; } }
		public Rom32() : base() { }
	}

	/// <summary>64KB of ROM.</summary>
	[Serializable()]
	public class Rom64 : FlatMemory {
		protected override bool IsWritable { get { return false; } }
		protected override int Size { get { return 64 * 1024; } }
		public Rom64() : base() { }
	}

	#endregion

}
