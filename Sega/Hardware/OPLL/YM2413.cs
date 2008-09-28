using System;
using System.Collections.Generic;
using System.Text;

namespace BeeDevelopment.Sega8Bit.Hardware {
	public partial class YM2413 {

		/// <summary>
		/// Gets or sets the value written to the detection port.
		/// </summary>
		public byte DetectionValue { get; set; }

		public YM2413() {
			this.Reset();
		}

		/// <summary>
		/// Gets or sets the clock rate of the YM2413.
		/// </summary>
		public double ClockRate { get; set; }
		/// <summary>
		/// Gets or sets the sample rate of the YM2413.
		/// </summary>
		public const double SampleRate = 44100.0d;
		/// <summary>
		/// Gets or sets the total running time of the YM2413.
		/// </summary>
		private double TotalRunningTime { get; set; }

		/// <summary>
		/// Gets or sets the latched register when writing.
		/// </summary>
		public byte LatchedRegister { get; set; }

		public void Reset() {

			// Unpack the instrument data from the ROM to each instrument def.
			for (int i = 0; i < 19; ++i) {
				Instruments[i] = new OpllPatch();
				// Should replace some of these with "Bit()"...
				Instruments[i].AM[0] = ((InstrumentRom[i * 16 + 0] >> 7) & 1) != 0;
				Instruments[i].AM[1] = ((InstrumentRom[i * 16 + 1] >> 7) & 1) != 0;
				Instruments[i].VIB[0] = ((InstrumentRom[i * 16 + 0] >> 6) & 1) != 0;
				Instruments[i].VIB[1] = ((InstrumentRom[i * 16 + 1] >> 6) & 1) != 0;
				Instruments[i].EG_TYP[0] = ((InstrumentRom[i * 16 + 0] >> 5) & 1) != 0;
				Instruments[i].EG_TYP[1] = ((InstrumentRom[i * 16 + 1] >> 5) & 1) != 0;
				Instruments[i].KSR[0] = ((InstrumentRom[i * 16 + 0] >> 4) & 1) != 0;
				Instruments[i].KSR[1] = ((InstrumentRom[i * 16 + 1] >> 4) & 1) != 0;
				Instruments[i].MUL[0] = (byte)(InstrumentRom[i * 16 + 0] & 0xF);
				Instruments[i].MUL[1] = (byte)(InstrumentRom[i * 16 + 1] & 0xF);
				Instruments[i].TL = (byte)(InstrumentRom[i * 16 + 2] & 0x1F);
			}

			this.Channels = new Channel[9]; // 9 different channels.
			for (int i = 0; i < 9; i++) Channels[i] = new Channel(this);

			// Wipe the registers to 0:
			for (byte i = 0; i < 0x39; ++i) this.Write(i, 0x00);

			this.TotalRunningTime = 0.0d;
		}

		#region Instrument ROM
		// Taken from Mitsutaka Okazaki's emu2413
		// At least, I *think* this is the instrument ROM.
		// For all I know, it could be anything, but sounds OK, even if it is entirely by accident.
		private static byte[] InstrumentRom =  {
            0x49,0x4c,0x4c,0x32,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x61,0x61,0x1e,0x17,0xf0,0x7f,0x00,0x17,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x13,0x41,0x16,0x0e,0xfd,0xf4,0x23,0x23,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x03,0x01,0x9a,0x04,0xf3,0xf3,0x13,0xf3,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x11,0x61,0x0e,0x07,0xfa,0x64,0x70,0x17,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x22,0x21,0x1e,0x06,0xf0,0x76,0x00,0x28,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x21,0x22,0x16,0x05,0xf0,0x71,0x00,0x18,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x21,0x61,0x1d,0x07,0x82,0x80,0x17,0x17,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x23,0x21,0x2d,0x16,0x90,0x90,0x00,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x21,0x21,0x1b,0x06,0x64,0x65,0x10,0x17,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x21,0x21,0x0b,0x1a,0x85,0xa0,0x70,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x23,0x01,0x83,0x10,0xff,0xb4,0x10,0xf4,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x97,0xc1,0x20,0x07,0xff,0xf4,0x22,0x22,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x61,0x00,0x0c,0x05,0xc2,0xf6,0x40,0x44,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x01,0x01,0x56,0x03,0x94,0xc2,0x03,0x12,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x21,0x01,0x89,0x03,0xf1,0xe4,0xf0,0x23,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x07,0x21,0x14,0x00,0xee,0xf8,0xff,0xf8,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x01,0x31,0x00,0x00,0xf8,0xf7,0xf8,0xf7,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x25,0x11,0x00,0x00,0xf8,0xfa,0xf8,0x55,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00 
        };
		#endregion


		private OpllPatch[] Instruments = new OpllPatch[19]; // 19 instruments
		private class OpllPatch {

			/// <summary>Amplitude Modulation.</summary>
			public bool[] AM = new bool[2];

			/// <summary>Vibrato</summary>
			public bool[] VIB = new bool[2];

			/// <summary>Multiplication factor.</summary>
			public int[] MUL = new int[2];

			/// <summary>Switching between sustained tone and Percussive tone.</summary>
			public bool[] EG_TYP = new bool[2];

			/// <summary>Key scale of rate.</summary>
			public bool[] KSR = new bool[2];

			/// <summary>Key scaling</summary>
			public int[] KSL = new int[2];

			/// <summary>Total Level</summary>
			public int TL = 0x00;

			/// <summary>Modulated wave rectified to half-wave.</summary>
			public bool DM = false;

			/// <summary>Carrier wave rectified to half-wave.</summary>
			public bool DC = false;

			/// <summary>Feedback FM modulation.</summary>
			public int FB = 0x00;

			/// <summary>Attack rate</summary>
			public int[] AR = new int[2];
			/// <summary>Decay rate</summary>
			public int[] DR = new int[2];
			/// <summary>Sustain level</summary>
			public int[] SL = new int[2];
			/// <summary>Release rate</summary>
			public int[] RR = new int[2];

		}

		public class Channel {

			/// <summary>
			/// Gets the YM2413 that contains this channel.
			/// </summary>
			public YM2413 Owner { get; private set; }

			// Various properties of the channel...

			// The properties that require getters/setters are done that way so that in the event of
			// one of them changing, the phase increments are recalculated to reflect this new change.

			private int m_F_Number = 0;
			public int F_Number {
				get { return m_F_Number; }
				set {
					if (value != m_F_Number) {
						m_F_Number = value;
						RecalculatePhaseIncrement(0);
						RecalculatePhaseIncrement(1);
					} else {
						m_F_Number = value;
					}
				}
			}

			private int m_Block = 0;
			public int Block {
				get { return m_Block; }
				set {
					if (value != m_Block) {
						m_Block = value;
						RecalculatePhaseIncrement(0);
						RecalculatePhaseIncrement(1);
					} else {
						m_Block = value;
					}
				}
			}

			private int m_InstrumentIndex = 0;
			public int InstrumentIndex {
				get { return m_InstrumentIndex; }
				set {
					if (value != m_InstrumentIndex) {
						m_InstrumentIndex = value;
						RecalculatePhaseIncrement(0);
						RecalculatePhaseIncrement(1);
					} else {
						m_InstrumentIndex = value;
					}
				}
			}

			private OpllPatch CurrentInstrument {
				get { return this.Owner.Instruments[InstrumentIndex]; }
			}

			public int Volume = 0;
			public bool KeyOn = false;
			public bool SustainOn = false;

			// Envelope stores the LINEAR envelope generator levels.
			public double[] Envelope = new double[2];



			// C# get: "double x = OpllChannel.Level;" executes the code below.
			public double Level {
				get {
					if (KeyOn) {
						for (int i = 0; i < 2; i++) {
							if (CurrentInstrument.AM[i]) RecalculateEnvelope(i);
						}
						return DbToPcm(Volume * 3) * Envelope[1] * (ModulationIndices[CurrentInstrument.FB] + Sin(Phase[1] + Envelope[0] * Sin(Phase[0])));
					} else {
						return 0.0d;
					}
				}
			}

			// Phase stores the total phase, phase increment the amount phase is altered by each sample.
			public double[] Phase = new double[2];
			public double[] PhaseIncrement = new double[2];

			// Call this if ANY factor that affects a phase increment changes.
			public void RecalculatePhaseIncrement(int IncrementIndex) {
				PhaseIncrement[IncrementIndex] = ((m_F_Number / ((262144.0d / (this.Owner.ClockRate / 72.0d)) / Blocks[m_Block])) * Multipliers[this.Owner.Instruments[m_InstrumentIndex].MUL[IncrementIndex]]) / YM2413.SampleRate;
				RecalculateEnvelope(IncrementIndex); // If the F-number changes, so does the envelope (KSL)
			}

			public void RecalculateEnvelope(int EnvelopeIndex) {

				double NewLevel = 48.0d; // 48dB

				if (EnvelopeIndex == 0) { // Modulator
					NewLevel -= CurrentInstrument.TL * 0.75d;
				}

				if (CurrentInstrument.AM[EnvelopeIndex]) {
					// Drop off a little...
					NewLevel -= (Sin(3.7d * this.Owner.TotalRunningTime) / 2.0d);
				}

				if (NewLevel < 0.0d) NewLevel = 0.0d;

				Envelope[EnvelopeIndex] = DbToPcm(48.0d - NewLevel);
			}

			// Update the phases by one sample.
			public void Tick() {
				for (int i = 0; i < 2; ++i) {
					if (CurrentInstrument.VIB[i]) {
						Phase[i] += PhaseIncrement[i] * (1 + (Sin(6.4 * this.Owner.TotalRunningTime) / 128.0d));
					} else {
						Phase[i] += PhaseIncrement[i];
					}
				}
			}

			// Sine function based on a phase of whole waves [ Sin(1 phase) == Sin(360 degrees) == Sin(2pi radians) ]
			private double Sin(double Phase) { return Math.Sin(Phase * 2.0d * Math.PI); }

			private double[] Multipliers =  // Multipliers (for the MUL factor)
				{ 0.5d, 1.0d, 2.0d, 3.0d, 4.0d, 5.0d, 6.0d, 7.0d, 8.0d, 9.0d, 10.0d, 10.0d, 12.0d, 12.0d, 15.0d, 15.0d };
			private double[] Blocks = // Block (octave multipliers)
				{ 0.5d, 1.0d, 2.0d, 4.0d, 8.0d, 16.0d, 32.0d, 64.0d };
			private double[] ModulationIndices = // "For the modulated wave of the first slot's feedback FM modulation." ???
				{ 0.0d, Math.PI / 16.0d, Math.PI / 8.0d, Math.PI / 4.0d, Math.PI / 2.0d, Math.PI, Math.PI * 2.0d, Math.PI * 4.0d };

			private double DbToPcm(double Decibels) {
				return 1.0d / Math.Pow(10.0d, Decibels / 20.0d);
			}

			public Channel(YM2413 owner) {
				this.Owner = owner;
			}
		}



		public Channel[] Channels { get; private set; }

		/// <summary>
		/// Runs the emulator for a single clock cycle.
		/// </summary>
		public double Tick() {
			
			foreach (Channel C in Channels) C.Tick();
			double Result = 0.0d;
			for (int i = 0; i < 6; ++i) Result += this.Channels[i].Level;
			TotalRunningTime += 1.0d / SampleRate;
			return Result;
		}

		/// <summary>
		/// Set the latched register.
		/// </summary>
		/// <param name="register">The register number to latch.</param>
		public void LatchRegister(byte register) {
			this.LatchedRegister = register;
		}

		/// <summary>
		/// Write a value to the last latched register.
		/// </summary>
		/// <param name="value">The value to write.</param>
		public void Write(byte value) {
			this.Write(this.LatchedRegister, value);
		}

		/// <summary>
		/// Write to a YM2413 register
		/// </summary>
		/// <param name="Register">Register number to write to</param>
		/// <param name="Value">Value to load into the register</param>
		public void Write(byte register, byte value) {
			int ChannelNumber = register & 0x0F; // In case we ned to use it.
			switch (register & 0xF0) {
				case 0x00:
					// Setting up some part of the user-defined instrument.
					switch (register) {
						case 0x00:
						case 0x01:
							// AM, VIB, EG-TYP, KSR and MUL.
							// The register is used as an index as there are two waves.
							Instruments[0].MUL[register] = value & 0x0F;
							Instruments[0].KSR[register] = Bit(value, 4);
							Instruments[0].EG_TYP[register] = Bit(value, 5);
							Instruments[0].VIB[register] = Bit(value, 6);
							Instruments[0].AM[register] = Bit(value, 7);
							foreach (Channel C in Channels) {
								if (C.InstrumentIndex == 0) {
									C.RecalculatePhaseIncrement(register);
								}
							}
							break;
						case 0x02:
							// KSL[0], TL.
							Instruments[0].TL = value & 0x3F;
							Instruments[0].KSL[0] = (value >> 6) & 0x3;
							break;
						case 0x03:
							// KSL[1], DC, DM, FB
							Instruments[0].FB = value & 0x7;
							Instruments[0].DM = Bit(value, 3);
							Instruments[0].DC = Bit(value, 4);
							Instruments[0].KSL[1] = (value >> 6) & 0x2;
							break;
						case 0x04:
						case 0x05:
							// AR, DR
							Instruments[0].DR[register - 0x4] = value & 0x0F;
							Instruments[0].AR[register - 0x4] = (value >> 4) & 0x0F;
							break;
						case 0x06:
						case 0x07:
							// SL, RR
							Instruments[0].RR[register - 0x6] = value & 0x0F;
							Instruments[0].SL[register - 0x6] = (value >> 4) & 0x0F;
							break;
					}
					break;

				case 0x10:
					// Setting the LSB of an F-number
					if (ChannelNumber < 9) {
						Channels[ChannelNumber].F_Number &= 0x0100; // Clear the lower 8 bits.
						Channels[ChannelNumber].F_Number |= value; // Set the lower 8 bits.
					}
					break;
				case 0x20:
					// Setting the MSB of an F-number, block, key and sustain.
					if (ChannelNumber < 9) {
						Channels[ChannelNumber].F_Number &= 0x00FF; // Clear the upper 9th bit.
						Channels[ChannelNumber].F_Number |= ((value & 1) << 8); // Set the 9th bit.
						Channels[ChannelNumber].Block = (value >> 1) & 0x7; // Set the block
						Channels[ChannelNumber].KeyOn = Bit(value, 4);
						Channels[ChannelNumber].SustainOn = Bit(value, 5);
					}
					break;
				case 0x30:
					// Setting the instrument and volume data.
					if (ChannelNumber < 9) {
						Channels[ChannelNumber].Volume = value & 0x0F;
						Channels[ChannelNumber].InstrumentIndex = (value >> 4) & 0x0F;
					}
					break;
			}
		}

		private static bool Bit(byte Value, int Index) {
			return (Value & (1 << Index)) != 0;
		}

	}
}
