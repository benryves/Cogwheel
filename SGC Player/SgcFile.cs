using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using BeeDevelopment.Sega8Bit.Hardware;
using BeeDevelopment.Sega8Bit;
using BeeDevelopment.Sega8Bit.Mappers;
using System.Windows.Forms;

namespace BeeDevelopment.SgcPlayer {
	class SgcFile {

		#region Types

		/// <summary>
		/// Represents a system type.
		/// </summary>
		public enum SystemType {
			/// <summary>Sega Master System.</summary>
			SegaMasterSystem = 0x00,
			/// <summary>Sega Game Gear.</summary>
			SegaGameGear = 0x01,
			/// <summary>ColecoVision.</summary>
			ColecoVision = 0x02,
		}

		#endregion

		#region Header Properties

		/// <summary>
		/// Gets or sets the version of the SGC file format used by the file.
		/// </summary>
		public byte Version { get; set; }

		/// <summary>
		/// Gets or sets the video system used by the file.
		/// </summary>
		public VideoDisplayProcessor.VideoSystem VideoSystem { get; set; }

		/// <summary>
		/// Gets or sets the number of scanlines between interrupts.
		/// </summary>
		public byte ScanlinesBetweenInterrupts { get; set; }

		/// <summary>
		/// Gets or sets the address of data in ROM space.
		/// </summary>
		public ushort StartRomAddress { get; set; }

		public ushort InitialisationAddress { get; set; }

		public ushort PlayAddress { get; set; }

		public ushort StackPointer { get; set; }

		public ushort[] RestartVectors { get; set; }

		public byte[] MapperInitialisation { get; set; }

		public byte FirstSongIndex { get; set; }

		public byte SongCount { get; set; }

		public byte FirstSoundEffectIndex { get; set; }

		public byte LastSoundEffectIndex { get; set; }

		public SystemType System { get; set; }

		public string SongName { get; set; }
		public string Author { get; set; }
		public string Copyright { get; set; }

		public byte[] SongData { get; set; }

		#endregion		

		#region Constructors

		protected SgcFile() { }

		/// <summary>
		/// Creates an instance of a <see cref="SgcFile"/> from a file.
		/// </summary>
		/// <param name="filename">The name of the file to load from.</param>
		/// <returns>An instance of a <see cref="SgcFile"/>.</returns>
		public static SgcFile FromFile(string filename) {
			var Result = new SgcFile();
			Result.Load(filename);
			return Result;
		}

		/// <summary>
		/// Creates an instance of a <see cref="SgcFile"/> from a <see cref="Stream"/>.
		/// </summary>
		/// <param name="stream">The <see cref="Stream"/> to load from.</param>
		/// <returns>An instance of a <see cref="SgcFile"/>.</returns>
		public static SgcFile FromStream(Stream stream) {
			var Result = new SgcFile();
			Result.Load(stream);
			return Result;
		}

		#endregion		

		#region Loaders

		// Load from a file.
		void Load(string filename) {
			using (var stream = File.OpenRead(filename)) {
				Load(stream);
			}
		}
		// Load from a stream.
		void Load(Stream stream) {
			Load(new BinaryReader(stream));
		}
		// Load from a BinaryReader.
		void Load(BinaryReader reader) {
		
			// Identification string.
			if (Encoding.ASCII.GetString(reader.ReadBytes(4)) != "SGC\x1A") throw new InvalidDataException("File does not have SGC identifier.");

			this.Version = reader.ReadByte();
			this.VideoSystem = (VideoDisplayProcessor.VideoSystem)reader.ReadByte();
			this.ScanlinesBetweenInterrupts = reader.ReadByte();
			reader.ReadByte();

			this.StartRomAddress = reader.ReadUInt16();
			this.InitialisationAddress = reader.ReadUInt16();
			this.PlayAddress = reader.ReadUInt16();
			this.StackPointer = reader.ReadUInt16();
			reader.ReadUInt16();

			this.RestartVectors = new ushort[7];
			for (int i = 0; i < 7; ++i) {
				this.RestartVectors[i] = reader.ReadUInt16();
			}

			this.MapperInitialisation = reader.ReadBytes(4);

			this.FirstSongIndex = reader.ReadByte();
			this.SongCount = reader.ReadByte();
			this.FirstSoundEffectIndex = reader.ReadByte();
			this.LastSoundEffectIndex = reader.ReadByte();

			this.System = (SystemType)reader.ReadByte();

			reader.BaseStream.Seek(0x40 - 0x29, SeekOrigin.Current);

			this.SongName = Encoding.ASCII.GetString(reader.ReadBytes(32)).TrimEnd('\0');
			this.Author = Encoding.ASCII.GetString(reader.ReadBytes(32)).TrimEnd('\0');
			this.Copyright = Encoding.ASCII.GetString(reader.ReadBytes(32)).TrimEnd('\0');

			this.SongData = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
		}

		#endregion

		#region Emulator Handling

		/// <summary>
		/// Creates an <see cref="Emulator"/> instance that can be used to play the file.
		/// </summary>
		/// <returns>The newly created <see cref="Emulator"/> instance with the song file loaded.</returns>
		public Emulator CreateEmulator() {
			var emulator = new Emulator();

			// Find the hardware model.
			var hardwareModel = HardwareModel.Default;
			switch (this.System) {
				case SystemType.SegaMasterSystem:
					hardwareModel = HardwareModel.MasterSystem;
					break;
				case SystemType.SegaGameGear:
					hardwareModel = HardwareModel.GameGear;
					break;
				case SystemType.ColecoVision:
					hardwareModel = HardwareModel.ColecoVision;
					break;
			}

			// Set the emulator capabilities.
			emulator.SetCapabilitiesByModelAndCountry(hardwareModel, this.VideoSystem == VideoDisplayProcessor.VideoSystem.Ntsc ? Country.Japan : Country.England);

			// Create a new cartridge.
			IMemoryMapper cartridge = null;
			switch (this.System) {
				case SystemType.SegaMasterSystem:
				case SystemType.SegaGameGear:
					cartridge = new Standard();
					emulator.FmSoundEnabled = true;
					break;
				case SystemType.ColecoVision:
					// Load ColecoVision ROM.
					var ColecoRomName = Path.Combine(Application.StartupPath, "coleco.bin");
					if (!File.Exists(ColecoRomName)) ColecoRomName = Path.Combine(Application.StartupPath, "coleco.rom");
					if (File.Exists(ColecoRomName)) {
						emulator.Bios.Memory = new Rom8();
						emulator.Bios.Memory.Load(File.ReadAllBytes(ColecoRomName));
						emulator.Bios.Enabled = true;
					}
					emulator.ExpansionSlot.Memory = new Rom16();
					emulator.ExpansionSlot.Memory.Load(new byte[] { 0x18, 0xFE }); // JR -2
					emulator.ExpansionSlot.Enabled = true;
					cartridge = new Rom32();
					emulator.FmSoundEnabled = false;
					break;
			}

			// Load the raw cartridge data.
			var rawCartridgeData = new byte[4 * 1024 * 1024];
			Array.Copy(this.SongData, 0, rawCartridgeData, this.StartRomAddress, this.SongData.Length);

			// Dummy loop at address 0x00.
			rawCartridgeData[0x0000] = 0x18; // JR
			rawCartridgeData[0x0001] = 0xFE; // -2

			// Add the restart vectors.
			for (int i = 0; i < 7; ++i) {
				rawCartridgeData[(i + 1) * 8 + 0] = 0xC3; // JP
				rawCartridgeData[(i + 1) * 8 + 1] = (byte)this.RestartVectors[i];
				rawCartridgeData[(i + 1) * 8 + 2] = (byte)(this.RestartVectors[i] >> 8);
			}

			// Load the cartridge.
			cartridge.Load(rawCartridgeData);
			emulator.CartridgeSlot.Memory = cartridge;
			emulator.WorkRam.Reset();


			// Do we need to perform bank switching to initialise?
			for (int i = 0; i < 4; ++i) {
				emulator.WriteMemory((ushort)(0xFFFC + i), this.MapperInitialisation[i]);
			}

			emulator.CartridgeSlot.Enabled = true;
			emulator.CartridgeSlot.Accessibility = MemoryDevice.AccessibilityMode.Always;

			emulator.RegisterSP = this.StackPointer;

			this.LoadSong(emulator, this.FirstSongIndex);

			return emulator;
		}

		private void Call(Emulator emulator, ushort address) {
			ushort infiniteLoopStubAddress = emulator.Family == HardwareFamily.ColecoVision ? (ushort)0x2000 : (ushort)0x0000;
			//emulator.RegisterSP = this.StackPointer;
			emulator.WriteMemory(--emulator.RegisterSP, (byte)(infiniteLoopStubAddress >> 8));
			emulator.WriteMemory(--emulator.RegisterSP, (byte)infiniteLoopStubAddress);
			emulator.RegisterPC = address;
			while (emulator.RegisterPC < infiniteLoopStubAddress || emulator.RegisterPC > infiniteLoopStubAddress + 1) {
				emulator.RunScanline();
			}
		}

		public void LoadSong(Emulator emulator, byte songIndex) {
			emulator.RegisterA = songIndex;
			Call(emulator, this.InitialisationAddress);
		}

		public void RunFrame(Emulator emulator) {
			Call(emulator, this.PlayAddress);
		}

		#endregion
	}
}
