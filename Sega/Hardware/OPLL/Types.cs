using System;
using System.Runtime.InteropServices;
using BeeDevelopment.Brazil;
using e_int32 = System.Int32;
using e_uint32 = System.UInt32;
using e_uint8 = System.Byte;

namespace BeeDevelopment.Sega8Bit.Hardware {

	public partial class Emu2413 {

		[StructLayout(LayoutKind.Sequential)]
		public struct OPLL_STATE {

			private e_uint32 adr;
			/// <summary>
			/// Gets or sets the latched register address.
			/// </summary>
			public e_uint32 LatchedRegister {
				get { return this.adr; }
				set { this.adr = value; }
			}

			private e_int32 output;
			/// <summary>
			/// Gets or sets the current output level of the YM2413.
			/// </summary>
			public e_int32 Output {
				get { return this.output; }
				set { this.output = value; }
			}

			#if !EMU2413_COMPACTION

			private e_uint32 realstep;
			public e_uint32 RealStep {
				get { return this.realstep; }
				set { this.realstep = value; }
			}

			private e_uint32 oplltime;
			public e_uint32 OpllTime {
				get { return this.oplltime; }
				set { this.oplltime = value; }
			}

			private e_uint32 opllstep;
			public e_uint32 OpllStep {
				get { return this.opllstep; }
				set { this.opllstep = value; }
			}

			private e_int32 prev;
			public e_int32 PreviousLevel {
				get { return this.prev; }
				set { this.prev = value; }
			}

			private e_int32 next;
			public e_int32 NextLevel {
				get { return this.next; }
				set { this.next = value; }
			}

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
			private e_int32[] sprev;
			public e_int32[] StereoPreviousLevel {
				get { return this.sprev; }
				set { Array.Copy(value, this.sprev, this.sprev.Length); }
			}
			
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
			private e_int32[] snext;
			public e_int32[] StereoNextLevel {
				get { return this.snext; }
				set { Array.Copy(value, this.snext, this.snext.Length); }
			}
			
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
			private e_uint32[] pan;
			/// <summary>
			/// Gets or sets the panning flags for each YM2413 channel.
			/// </summary>
			public e_uint32[] Pan {
				get { return this.pan; }
				set { Array.Copy(value, this.pan, this.pan.Length); }
			}

			#endif

			/* Register */
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x40)]
			private e_uint8[] reg;
			/// <summary>
			/// Gets or sets the value of all of the YM2413's registers.
			/// </summary>
			public e_uint8[] Registers {
				get { return this.reg; }
				set { Array.Copy(value, this.reg, this.reg.Length); }
			}

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)]
			private e_int32[] slot_on_flag;
			/// <summary>
			///  Gets or sets an array of flags indicating whether a slot is on or not.
			/// </summary>
			public e_int32[] SlotOnFlag {
				get { return this.slot_on_flag; }
				set { Array.Copy(value, this.slot_on_flag, this.slot_on_flag.Length); }
			}

			/* Pitch Modulator */
			private e_uint32 pm_phase;
			/// <summary>
			/// Gets or sets the pitch modulator's phase.
			/// </summary>
			public e_uint32 PitchModulatorPhase {
				get { return this.pm_phase; }
				set { this.pm_phase = value; }
			}

			private e_int32 lfo_pm;
			public e_int32 LowFrequencyOscillatorPitchModulator {
				get { return this.lfo_pm; }
				set { this.lfo_pm = value; }
			}

			/* Amp Modulator */
			private e_int32 am_phase;
			/// <summary>
			/// Gets or sets the amplitude modulator's phase.
			/// </summary>
			public e_int32 AmplitudeModulatorPhase {
				get { return this.am_phase; }
				set { this.am_phase = value; }
			}

			private e_int32 lfo_am;
			public e_int32 LowFrequencyOscillatorAmplitudeModulator {
				get { return this.lfo_am; }
				set { this.lfo_am = value; }
			}

			private bool quality;
			/// <summary>
			/// Gets or sets whether the YM2413 emulator is in high quality mode.
			/// </summary>
			public bool HighQuality {
				get { return this.quality; }
				set { this.quality = value; }
			}

			/* Noise Generator */
			private e_uint32 noise_seed;
			/// <summary>
			/// Gets or sets the noise generator's seed.
			/// </summary>
			public e_uint32 NoiseSeed {
				get { return this.noise_seed; }
				set { this.noise_seed = value; }
			}

			/* Channel Data */
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
			private e_int32[] patch_number;
			/// <summary>
			/// Gets or sets the patch number for the tone channels.
			/// </summary>
			public e_int32[] PatchNumber {
				get { return this.patch_number; }
				set { Array.Copy(value, this.patch_number, this.patch_number.Length); }
			}

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
			private e_int32[] key_status;
			/// <summary>
			/// Gets or sets an array of flags indicating whether a key is pressed or not for each tone channel.
			/// </summary>
			public e_int32[] KeyStatus {
				get { return this.key_status; }
				set { Array.Copy(value, this.key_status, this.key_status.Length); }
			}

			/* Slot */
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)]
			private OPLL_SLOT[] slot;
			/// <summary>
			/// Gets or sets the array of 18 YM2413 slots.
			/// </summary>
			[StateNotSaved()]
			public OPLL_SLOT[] Slots {
				get { return this.slot; }
				set { Array.Copy(value, this.slot, this.slot.Length); }
			}

			/* Voice Data */
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 19 * 2)]
			private OPLL_PATCH[] patch;
			/// <summary>
			/// Gets or sets an array of 38 YM2413 patches.
			/// </summary>
			[StateNotSaved()]
			public OPLL_PATCH[] Patches {
				get { return this.patch; }
				set { Array.Copy(value, this.patch, this.patch.Length); }
			}

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
			private e_int32[] patch_update; /* flag for check patch update */
			
			// (Is this used anywhere?)
			public e_int32[] PatchUpdate {
				get { return this.patch_update; }
				set { Array.Copy(value, this.patch_update, this.patch_update.Length); }
			}

			private e_uint32 mask;
			/// <summary>
			/// Gets or sets a bit array specifying masked-out channels.
			/// </summary>
			public e_uint32 Mask {
				get { return this.mask; }
				set { this.mask = value; }
			}

		}

		[StructLayout(LayoutKind.Sequential)]
		public struct OPLL_SLOT {

			private OPLL_PATCH patch;

			private e_int32 type;          /* 0 : modulator 1 : carrier */
			/// <summary>
			/// Gets or sets the type of the slot; 0 for modulator, 1 for carrier.
			/// </summary>
			public e_int32 Type {
				get { return this.type; }
				set { this.type = value; }
			}

			/* OUTPUT */
			private e_int32 feedback;
			public e_int32 Feedback {
				get { return this.feedback; }
				set { this.feedback = value; }
			}


			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
			private e_int32[] output;   /* Output value of slot */
			/// <summary>
			/// Gets or sets the output value of the slot.
			/// </summary>
			public e_int32[] Output {
				get { return this.output; }
				set { Array.Copy(value, this.output, this.output.Length); }
			}


			/* for Phase Generator (PG) */
			private IntPtr sintbl;    /* Wavetable */

			private e_uint32 phase;      /* Phase */
			public e_uint32 Phase { get { return this.phase; } set { this.phase = value; } }
			private e_uint32 dphase;     /* Phase increment amount */
			public e_uint32 PhaseIncrement { get { return this.dphase; } set { this.dphase = value; } }
			private e_uint32 pgout;      /* output */
			public e_uint32 PhaseGeneratorOutput { get { return this.pgout; } set { this.pgout = value; } }

			/* for Envelope Generator (EG) */
			private e_int32 fnum;          /* F-Number */
			public e_int32 FNumber { get { return this.fnum; } set { this.fnum = value; } }
			private e_int32 block;         /* Block */
			public e_int32 Block { get { return this.block; } set { this.block = value; } }
			private e_int32 volume;        /* Current volume */
			public e_int32 Volume { get { return this.volume; } set { this.volume = value; } }
			private bool sustine;       /* Sustine 1 = ON, 0 = OFF */
			public bool Sustain { get { return this.sustine; } set { this.sustine = value; } }
			private e_uint32 tll;	      /* Total Level + Key scale level*/
			public e_uint32 TotalLevel { get { return this.tll; } set { this.tll = value; } }
			private e_uint32 rks;        /* Key scale offset (Rks) */
			public e_uint32 KeyScaleOffset { get { return this.rks; } set { this.rks = value; } }
			private e_int32 eg_mode;       /* Current state */
			public e_int32 EnvelopeGeneratorMode { get { return this.eg_mode; } set { this.eg_mode = value; } }
			private e_uint32 eg_phase;   /* Phase */
			public e_uint32 EnvelopeGeneratorPhase { get { return this.eg_phase; } set { this.eg_phase = value; } }
			private e_uint32 eg_dphase;  /* Phase increment amount */
			public e_uint32 EnvelopeGeneratorPhaseIncrement { get { return this.eg_dphase; } set { this.eg_dphase = value; } }
			private e_uint32 egout;      /* output */
			public e_uint32 EnvelopeGeneratorOutput { get { return this.egout; } set { this.egout = value; } }

		}

		[StructLayout(LayoutKind.Sequential)]
		public struct OPLL_PATCH {
			private e_uint32 tl, fb, eg, ml, ar, dr, sl, rr, kr, kl, am, pm, wf;
			public e_uint32 FB { get { return fb; } set { fb = value; } }
			public e_uint32 EG { get { return eg; } set { eg = value; } }
			public e_uint32 ML { get { return ml; } set { ml = value; } }
			public e_uint32 AR { get { return ar; } set { ar = value; } }
			public e_uint32 DR { get { return dr; } set { dr = value; } }
			public e_uint32 SL { get { return sl; } set { sl = value; } }
			public e_uint32 RR { get { return rr; } set { rr = value; } }
			public e_uint32 KR { get { return kr; } set { kr = value; } }
			public e_uint32 KL { get { return kl; } set { kl = value; } }
			public e_uint32 AM { get { return am; } set { am = value; } }
			public e_uint32 PM { get { return pm; } set { pm = value; } }
			public e_uint32 WF { get { return wf; } set { wf = value; } }
		}

	}
}
