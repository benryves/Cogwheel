using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace BeeDevelopment.Sega8Bit.Hardware.Controllers {


	public class TapeBitStream {

		private List<uint> dataBits = new List<uint>();
		private int count = 0;

		public int Count {
			get {
				return this.count;
			}
		}

		public void Add(bool value) {
			if (dataBits.Count < (this.count / 32) + 1) {
				this.dataBits.Add(0);
			}
			this[this.count++] = value;
		}

		public void Add(TapeBitStream value) {
			for (int i = 0; i < value.count; ++i) {
				this.Add(value[i]);
			}
		}

		public bool this[int index] {
			get {
				if (index < 0 || index >= this.count) throw new IndexOutOfRangeException();
				var wholeValue = this.dataBits[index / 32];
				return (wholeValue & (1 << (index % 32))) != 0;
			}
			set {
				if (index < 0 || index >= this.count) throw new IndexOutOfRangeException();
				var wholeValue = this.dataBits[index / 32];
				if (value) {
					wholeValue |= (uint)(1 << (index % 32));
				} else {
					wholeValue &= (uint)(~(1 << (index % 32)));
				}
				this.dataBits[index / 32] = wholeValue;
			}
		}

		public void WriteDataBit(bool value) {
			if (value) {
				this.Add(true);
				this.Add(false);
				this.Add(true);
				this.Add(false);
			} else {
				this.Add(true);
				this.Add(true);
				this.Add(false);
				this.Add(false);
			}
		}

		public void WriteDataByte(byte value, int startBits = 1, int stopBits = 1) {
			for (int i = 0; i < startBits; ++i) {
				this.WriteDataBit(false);
			}
			for (int i = 0; i < 8; ++i) {
				this.WriteDataBit((value & 1) != 0);
				value >>= 1;
			}
			for (int i = 0; i < stopBits; ++i) {
				this.WriteDataBit(true);
			}
		}

	}

	public struct UnifiedEmulatorFormatChunk {
		public ushort ID;
		public byte[] Data;

		public TapeBitStream ToTapeBitStream() {

			// We can only convert tape chunks.
			if ((this.ID & 0xFF00) != 0x0100) throw new InvalidOperationException(string.Format("Chunk ID &{0:X4} is not a tape chunk.", this.ID));

			var tapeBitStream = new TapeBitStream();

			switch (this.ID) {
				case 0x0100: // Chunk &0100 - implicit start/stop bit tape data block 
					foreach (var item in this.Data) {
						tapeBitStream.WriteDataByte(item, 1, 1);
					}
					break;
				case 0x0110: // Chunk &0110 - carrier tone (previously referred to as 'high tone') 
					if (this.Data.Length != 2) throw new InvalidOperationException();
					for (int i = this.Data[0] + (this.Data[1] * 256); i > 0; --i) {
						tapeBitStream.WriteDataBit(true);
					}
					break;
				case 0x0112: // Chunk &0112 - integer gap
					for (int i = (this.Data[0] + (this.Data[1] * 256)) * 8; i > 0; --i) {
						tapeBitStream.Add(false);
					}
					break;
				default:
					throw new NotSupportedException(string.Format("Chunk ID &{0:X4} is not supported.", this.ID));
			}
			return tapeBitStream;
		}

	}

	public class UnifiedEmulatorFormat {


		public byte MinorVersion { get; private set; }
		public byte MajorVersion { get; private set; }

		public UnifiedEmulatorFormatChunk[] Chunks { get; private set; }

		public TapeBitStream ToTapeBitStream() {
			var result = new TapeBitStream();
			foreach (var chunk in this.Chunks) {
				if ((chunk.ID & 0xFF00) == 0x0100) {
					result.Add(chunk.ToTapeBitStream());
				}
			}
			return result;
		}

		public UnifiedEmulatorFormat() {
		}

		public static UnifiedEmulatorFormat FromStream(Stream stream) {

			// Use the BinaryReader to decode the file.
			var reader = new BinaryReader(stream);

			// Only check for GZip magic numbers in seekable streams.
			// If we can't seek, we can't backtrack after opening the GZipStream.
			if (stream.CanSeek) {
				// Check for GZip magic number.
				ushort magicNumber = reader.ReadUInt16();
				stream.Seek(-2, SeekOrigin.Current);
				if (magicNumber == 0x8B1F) {
					return FromStream(new GZipStream(stream, CompressionMode.Decompress, true));
				}
			}

			var result = new UnifiedEmulatorFormat();

			// Check for the magic header.
			var magicHeader = Encoding.ASCII.GetString(reader.ReadBytes(10));
			if (magicHeader != "UEF File!\0") {
				throw new InvalidDataException("File does not have 'UEF File!' header.");
			}

			// Version number.
			result.MinorVersion = reader.ReadByte();
			result.MajorVersion = reader.ReadByte();

			var chunks = new List<UnifiedEmulatorFormatChunk>();

			// Read all the chunks.
			for (; ; ) {
				try {
					var chunkID = reader.ReadUInt16();
					var chunkLength = reader.ReadInt32();
					chunks.Add(new UnifiedEmulatorFormatChunk {
						ID = chunkID,
						Data = reader.ReadBytes(chunkLength)
					});
				} catch (EndOfStreamException) {
					break;
				}
			}

			result.Chunks = chunks.ToArray();

			return result;
		}
		public static UnifiedEmulatorFormat FromFile(string filename) {
			using (var stream = File.OpenRead(filename)) {
				return FromStream(stream);
			}

		}
	}

	public enum CassetteRecorderPlayState {
		Stopped,
		Playing,
		Rewinding,
		FastForwarding,
	};

	public class CassetteRecorder {



		public CassetteRecorderPlayState PlayState { get; private set; }

		private Emulator emulator;

		private UnifiedEmulatorFormat tape;

		public UnifiedEmulatorFormat Tape {
			get { return this.tape; }
			set {
				this.tape = value;
				this.PlayState = CassetteRecorderPlayState.Stopped;
				this.tapePosition = 0;
				if (this.tape != null) {
					this.tapeBitstream = this.Tape.ToTapeBitStream();
				} else {
					this.tapeBitstream = null;
				}
			}
		}

		private int tapePosition = 0;

		TapeBitStream tapeBitstream;

		public void Play() {
			if (this.Tape != null) {
				this.lastCpuCycles = this.emulator.TotalExecutedCycles;
				this.PlayState = CassetteRecorderPlayState.Playing;
				this.ClampPlayPosition();
			}
		}

		public void Stop() {
			this.lastCpuCycles = this.emulator.TotalExecutedCycles;
			this.PlayState = CassetteRecorderPlayState.Stopped;
			this.ClampPlayPosition();
		}

		public void Eject() {
			this.Tape = null;
			this.PlayState = CassetteRecorderPlayState.Stopped;
			this.ClampPlayPosition();
		}

		public void FastForward() {
			if (this.Tape != null) {
				this.lastCpuCycles = this.emulator.TotalExecutedCycles;
				this.PlayState = CassetteRecorderPlayState.FastForwarding;
				this.ClampPlayPosition();
			}
		}

		public void Rewind() {
			if (this.Tape != null) {
				this.lastCpuCycles = this.emulator.TotalExecutedCycles;
				this.PlayState = CassetteRecorderPlayState.Rewinding;
				this.ClampPlayPosition();
			}
		}

		private void ClampPlayPosition() {
			if (this.tape != null && this.tapeBitstream != null) {
				this.tapePosition = Math.Max(0, Math.Min(this.tapeBitstream.Count - 1, this.tapePosition));
			}
		}

		public CassetteRecorder(Emulator emulator) {
			this.emulator = emulator;
		}

		private int lastCpuCycles = 0;

		public void UpdateState() {
			if (this.PlayState == CassetteRecorderPlayState.Stopped || this.tapeBitstream == null) {
				this.emulator.SegaPorts[1].TR.InputState = true;
			} else {

				var playSpeed = 0;
				switch (this.PlayState) {
					case CassetteRecorderPlayState.Playing:
						playSpeed = 1;
						break;
					case CassetteRecorderPlayState.FastForwarding:
						playSpeed = 4;
						break;
					case CassetteRecorderPlayState.Rewinding:
						playSpeed = -2;
						break;
				}

				// Calculate the new read position.
				int cyclesAdvanced = this.emulator.TotalExecutedCycles - this.lastCpuCycles;
				int bitsAdvanced = (cyclesAdvanced * 4800) / this.emulator.ClockSpeed;
				this.lastCpuCycles += bitsAdvanced * (this.emulator.ClockSpeed / 4800);
				this.tapePosition += bitsAdvanced * playSpeed;

				if (this.tapePosition < 0 || this.tapePosition >= this.tapeBitstream.Count) {
					this.emulator.SegaPorts[1].TR.InputState = true;
					this.Stop();
				} else if (this.PlayState == CassetteRecorderPlayState.Playing) {
					this.emulator.SegaPorts[1].TR.InputState = !this.tapeBitstream[this.tapePosition];
				} else {
					this.emulator.SegaPorts[1].TR.InputState = true;
				}

			}
		}

		public TimeSpan TapeLength {
			get {
				if (this.tapeBitstream != null) {
					return TimeSpan.FromSeconds(this.tapeBitstream.Count / 4800d);
				} else {
					return TimeSpan.Zero;
				}
			}
		}

		public TimeSpan TapePosition {
			get {
				if (this.tapeBitstream != null) {
					return TimeSpan.FromSeconds(Math.Max(0, this.tapePosition) / 4800d);
				} else {
					return TimeSpan.Zero;
				}
			}
		}
	}
}

