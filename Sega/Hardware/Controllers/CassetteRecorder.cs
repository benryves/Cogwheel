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
				this.Add(false);
				this.Add(true);
				this.Add(false);
				this.Add(true);
			} else {
				this.Add(false);
				this.Add(false);
				this.Add(true);
				this.Add(true);
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

		/// <summary>
		/// Decode the bit stream into a stream of data bits.
		/// </summary>
		public KeyValuePair<int, bool>[] GetDataBits() {
			List<KeyValuePair<int, bool>> result = new List<KeyValuePair<int, bool>>();

			int lastStateChanged = 0;
			bool waitingSecondHalfOfOne = false;
			for (int i = 1; i < this.count; ++i) {
				if (this[i] != this[i - 1]) {
					if (!this[i]) {
						var waveLength = i - lastStateChanged;
						if (waveLength > 1 && waveLength < 6) {
							if (waveLength < 3 && waitingSecondHalfOfOne) {
								waitingSecondHalfOfOne = false;
							} else {
								if (waveLength < 3) {
									waitingSecondHalfOfOne = true;
									result.Add(new KeyValuePair<int, bool>(lastStateChanged, true));
								} else {
									result.Add(new KeyValuePair<int, bool>(lastStateChanged, false));
								}
							}
						} else {
							waitingSecondHalfOfOne = false;
						}
						lastStateChanged = i;
					}
				}
			}
			return result.ToArray();
		}

		/// <summary>
		/// Decode the bit stream into a stream of data bytes.
		/// </summary>
		public KeyValuePair<int, byte>[] GetDataBytes() {
			List<KeyValuePair<int, byte>> result = new List<KeyValuePair<int, byte>>();
			int lastBitReceived = 0;
			byte value = 0;
			int bitsReceived = -1;
			int startBitTime = 0;
			foreach (var item in this.GetDataBits()) {
				if (item.Key - lastBitReceived > 8) {
					bitsReceived = -1;
				}
				if (bitsReceived < 0) {
					if (!item.Value) {
						bitsReceived = 0;
						startBitTime = item.Key;
					}
				} else if (bitsReceived < 8) {
					value >>= 1;
					if (item.Value) value |= 0x80;
					++bitsReceived;
				} else {
					bitsReceived = -1;
					if (item.Value) {
						result.Add(new KeyValuePair<int, byte>(startBitTime, value));
					}
				}
				lastBitReceived = item.Key;
			}
			return result.ToArray();
		}

		/// <summary>
		/// Decode the bit stream into a stream of data blocks.
		/// </summary>
		public KeyValuePair<int, byte[]>[] GetDataBlocks() {
			List<KeyValuePair<int, List<byte>>> result = new List<KeyValuePair<int, List<byte>>>();
			int lastByteReceived = 0;
			foreach (var item in this.GetDataBytes()) {
				if (result.Count == 0 || (item.Key - lastByteReceived) > 80) {
					result.Add(new KeyValuePair<int, List<byte>>(item.Key, new List<byte>()));
				}
				result[result.Count - 1].Value.Add(item.Value);
				lastByteReceived = item.Key;
			}
			return Array.ConvertAll(result.ToArray(), item => new KeyValuePair<int, byte[]>(item.Key, item.Value.ToArray()));
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
						tapeBitStream.Add(true);
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
			if (this.Chunks != null) {
				foreach (var chunk in this.Chunks) {
					if ((chunk.ID & 0xFF00) == 0x0100) {
						result.Add(chunk.ToTapeBitStream());
					}
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
		Recording,
		Rewinding,
		FastForwarding,
	};

	public class CassetteRecorder {



		public CassetteRecorderPlayState PlayState { get; private set; }
		public bool MotorOn { get; set; }

		private Emulator emulator;

		public UnifiedEmulatorFormat Tape {
			set {
				this.PlayState = CassetteRecorderPlayState.Stopped;
				this.tapePosition = 0;
				if (value != null) {
					this.tapeBitstream = value.ToTapeBitStream();
				} else {
					this.tapeBitstream = null;
				}
			}
		}

		public TapeBitStream TapeBitStream {
			get {
				return this.tapeBitstream;
			}
		}

		public bool InvertLevel {
			get; set;
		}


		private int tapePosition = 0;

		TapeBitStream tapeBitstream;

		public void Play() {
			if (this.tapeBitstream != null) {
				this.lastCpuCycles = this.emulator.TotalExecutedCycles;
				this.PlayState = CassetteRecorderPlayState.Playing;
				this.ClampPlayPosition();
			}
		}

		public void Record() {
			if (this.tapeBitstream != null) {
				this.lastCpuCycles = this.emulator.TotalExecutedCycles;
				this.PlayState = CassetteRecorderPlayState.Recording;
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
			if (this.tapeBitstream != null) {
				this.lastCpuCycles = this.emulator.TotalExecutedCycles;
				this.PlayState = CassetteRecorderPlayState.FastForwarding;
				this.ClampPlayPosition();
			}
		}

		public void Rewind() {
			if (this.tapeBitstream != null) {
				this.lastCpuCycles = this.emulator.TotalExecutedCycles;
				this.PlayState = CassetteRecorderPlayState.Rewinding;
				this.ClampPlayPosition();
			}
		}

		private void ClampPlayPosition() {
			if (this.tapeBitstream != null) {
				this.tapePosition = Math.Max(0, Math.Min(this.tapeBitstream.Count - 1, this.tapePosition));
			}
		}

		public CassetteRecorder(Emulator emulator) {
			this.emulator = emulator;
			this.MotorOn = true;
		}

		private int lastCpuCycles = 0;

		public void UpdateState() {

			this.MotorOn = this.emulator.SegaPorts[1].TR.Direction == PinDirection.Output && this.emulator.SegaPorts[1].TR.OutputState;

			if (this.PlayState == CassetteRecorderPlayState.Stopped || this.tapeBitstream == null) {
				this.emulator.SegaPorts[1].Down.State = true;
			} else {

				var playSpeed = 0;
				switch (this.PlayState) {
					case CassetteRecorderPlayState.Playing:
					case CassetteRecorderPlayState.Recording:
						playSpeed = 1;
						break;
					case CassetteRecorderPlayState.FastForwarding:
						playSpeed = 4;
						break;
					case CassetteRecorderPlayState.Rewinding:
						playSpeed = -2;
						break;
				}

				if (!this.MotorOn) {
					playSpeed = 0;
				}

				// Calculate the new read position.
				int cyclesAdvanced = this.emulator.TotalExecutedCycles - this.lastCpuCycles;
				int bitsAdvanced = (cyclesAdvanced * 4800) / this.emulator.ClockSpeed;
				this.lastCpuCycles += bitsAdvanced * (this.emulator.ClockSpeed / 4800);
				this.tapePosition += bitsAdvanced * playSpeed;

				if (this.tapePosition < 0 || (this.tapePosition >= this.tapeBitstream.Count && this.PlayState != CassetteRecorderPlayState.Recording)) {
					this.emulator.SegaPorts[1].Down.State = true;
					this.Stop();
				} else if (this.PlayState == CassetteRecorderPlayState.Playing) {
					this.emulator.SegaPorts[1].Down.State = this.tapeBitstream[this.tapePosition] ^ this.InvertLevel;
				} else if (this.PlayState == CassetteRecorderPlayState.Recording) {
					var level = this.emulator.SegaPorts[1].TH.State ^ this.InvertLevel;
					while (this.tapeBitstream.Count <= this.tapePosition) {
						this.tapeBitstream.Add(false);
					}
					this.tapeBitstream[this.tapePosition] = level;
				} else {
					this.emulator.SegaPorts[1].Down.State = true;
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
			set {
				if (this.tapeBitstream != null) {
					this.tapePosition = (int)Math.Floor(value.TotalSeconds * 4800d);
					this.ClampPlayPosition();
				}
			}
		}
	}
}

