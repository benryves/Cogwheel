using System;
using BeeDevelopment.Brazil;
using e_int16 = System.Int16;
using e_int32 = System.Int32;
using e_uint32 = System.UInt32;
using e_uint8 = System.Byte;

namespace BeeDevelopment.Sega8Bit.Hardware {

	public partial class Emu2413 {

		enum OPLL_TONE_ENUM { OPLL_2413_TONE = 0, OPLL_VRC7_TONE = 1, OPLL_281B_TONE = 2 } ;

		/* voice data */
		public class OPLL_PATCH : ICloneable {
			internal e_uint32 tl, fb, eg, ml, ar, dr, sl, rr, kr, kl, am, pm, wf;

			public e_uint32 TL { get { return this.tl; } set { this.tl = value; } }
			public e_uint32 FB { get { return this.fb; } set { this.fb = value; } }
			public e_uint32 EG { get { return this.eg; } set { this.eg = value; } }
			public e_uint32 ML { get { return this.ml; } set { this.ml = value; } }
			public e_uint32 AR { get { return this.ar; } set { this.ar = value; } }
			public e_uint32 DR { get { return this.dr; } set { this.dr = value; } }
			public e_uint32 SL { get { return this.sl; } set { this.sl = value; } }
			public e_uint32 RR { get { return this.rr; } set { this.rr = value; } }
			public e_uint32 KR { get { return this.kr; } set { this.kr = value; } }
			public e_uint32 KL { get { return this.kl; } set { this.kl = value; } }
			public e_uint32 AM { get { return this.am; } set { this.am = value; } }
			public e_uint32 PM { get { return this.pm; } set { this.pm = value; } }
			public e_uint32 WF { get { return this.wf; } set { this.wf = value; } }

			public object Clone() {
				OPLL_PATCH o = new OPLL_PATCH();
				o.tl = this.tl; o.fb = this.fb; o.eg = this.eg; o.ml = this.ml; o.ar = this.ar; o.dr = this.dr; o.sl = this.sl; o.rr = this.rr; o.kr = this.kr; o.kl = this.kl; o.am = this.am; o.pm = this.pm; o.wf = this.wf;
				return o;
			}

		}

		/* slot */
		public class OPLL_SLOT {

			internal OPLL_PATCH patch;
			[StateNotSaved()]
			public OPLL_PATCH Patch { get { return this.patch; } set { this.patch = value; } }

			internal e_int32 type;          /* 0 : modulator 1 : carrier */
			public e_int32 Type { get { return this.type; } set { this.type = value; } }

			/* OUTPUT */
			internal e_int32 feedback;
			public e_int32 Feedback { get { return this.feedback; } set { this.feedback = value; } }

			internal e_int32[] output = new e_int32[2];   /* Output value of slot */
			public e_int32[] Output { get { return this.output; } set { this.output = value; } }

			/* for Phase Generator (PG) */
			internal e_int16[] sintbl;   /* Wavetable */
			public e_int16[] SinTbl { get { return this.sintbl; } set { this.sintbl = value; } }
			internal e_uint32 phase;      /* Phase */
			public e_uint32 Phase { get { return this.phase; } set { this.phase = value; } }
			internal e_uint32 dphase;     /* Phase increment amount */
			public e_uint32 DPhase { get { return this.dphase; } set { this.dphase = value; } }
			internal e_uint32 pgout;      /* output */
			public e_uint32 PGOut { get { return this.pgout; } set { this.pgout = value; } }

			/* for Envelope Generator (EG) */
			internal e_int32 fnum;          /* F-Number */
			public e_int32 FNum { get { return this.fnum; } set { this.fnum = value; } }
			internal e_int32 block;         /* Block */
			public e_int32 Block { get { return this.block; } set { this.block = value; } }
			internal e_int32 volume;        /* Current volume */
			public e_int32 Volumne { get { return this.volume; } set { this.volume = value; } }
			internal e_int32 sustine;       /* Sustine 1 = ON, 0 = OFF */
			public e_int32 Sustine { get { return this.sustine; } set { this.sustine = value; } }
			internal e_uint32 tll;	      /* Total Level + Key scale level*/
			public e_uint32 TlL { get { return this.tll; } set { this.tll = value; } }
			internal e_uint32 rks;        /* Key scale offset (Rks) */
			public e_uint32 Rks { get { return this.rks; } set { this.rks = value; } }
			internal OPLL_EG_STATE eg_mode;       /* Current state */
			public OPLL_EG_STATE EGMode { get { return this.eg_mode; } set { this.eg_mode = value; } }
			internal e_uint32 eg_phase;   /* Phase */
			public e_uint32 EGPhase { get { return this.eg_phase; } set { this.eg_phase = value; } }
			internal e_uint32 eg_dphase;  /* Phase increment amount */
			public e_uint32 EGDPhase { get { return this.eg_dphase; } set { this.eg_dphase = value; } }
			internal e_uint32 egout;      /* output */
			public e_uint32 EGOutput { get { return this.egout; } set { this.egout = value; } }

		}

		/* Mask */
		static int OPLL_MASK_CH(int x) { return (1 << (x)); }
		const int OPLL_MASK_HH = (1 << (9));
		const int OPLL_MASK_CYM = (1 << (10));
		const int OPLL_MASK_TOM = (1 << (11));
		const int OPLL_MASK_SD = (1 << (12));
		const int OPLL_MASK_BD = (1 << (13));
		const int OPLL_MASK_RHYTHM = (OPLL_MASK_HH | OPLL_MASK_CYM | OPLL_MASK_TOM | OPLL_MASK_SD | OPLL_MASK_BD);

		/* opll */
		public class OPLL {

			internal e_uint32 adr;
			public e_uint32 Adr { get { return this.adr; } set { this.adr = value; } }
			internal e_int32 output;
			public e_int32 Output { get { return this.output; } set { this.output = value; } }

			internal e_uint32 realstep;
			public e_uint32 RealStep { get { return this.realstep; } set { this.realstep = value; } }
			internal e_uint32 oplltime;
			public e_uint32 OpllTime { get { return this.oplltime; } set { this.oplltime = value; } }
			internal e_uint32 opllstep;
			public e_uint32 OpllStep { get { return this.opllstep; } set { this.opllstep = value; } }
			internal e_int32 prev, next;
			public e_int32 Prev { get { return this.prev; } set { this.prev = value; } }
			public e_int32 Next { get { return this.next; } set { this.next = value; } }
			internal e_int32[] sprev = new e_int32[2], snext = new e_int32[2];
			public e_int32[] SPrev { get { return this.sprev; } set { this.sprev = value; } }
			public e_int32[] SNext { get { return this.snext; } set { this.snext = value; } }
			internal e_uint32[] pan = new e_uint32[16];
			public e_uint32[] Pan { get { return this.pan; } set { this.pan = value; } }

			/* Register */
			internal e_uint8[] reg = new e_uint8[0x40];
			public e_uint8[] Reg { get { return this.reg; } set { this.reg = value; } }
			internal e_int32[] slot_on_flag = new e_int32[18];
			public e_int32[] SlotOnFlag { get { return this.slot_on_flag; } set { this.slot_on_flag = value; } }

			/* Pitch Modulator */
			internal e_uint32 pm_phase;
			public e_uint32 PMPhase { get { return this.pm_phase; } set { this.pm_phase = value; } }
			internal e_int32 lfo_pm;
			public e_int32 LfoPM { get { return this.lfo_pm; } set { this.lfo_pm = value; } }


			/* Amp Modulator */
			internal e_int32 am_phase;
			public e_int32 AMPhase { get { return this.am_phase; } set { this.am_phase = value; } }
			internal e_int32 lfo_am;
			public e_int32 LfoAM { get { return this.lfo_am; } set { this.lfo_am = value; } }

			internal bool quality;
			public bool Quality { get { return this.quality; } set { this.quality = value; } }

			/* Noise Generator */
			internal e_uint32 noise_seed;
			public e_uint32 NoiseSeed { get { return this.noise_seed; } set { this.noise_seed = value; } }

			/* Channel Data */
			internal e_int32[] patch_number = new e_int32[9];
			public e_int32[] PatchNumber { get { return this.patch_number; } set { this.patch_number = value; } }
			internal e_int32[] key_status = new e_int32[9];
			public e_int32[] KeyStatus { get { return this.key_status; } set { this.key_status = value; } }

			/* Slot */
			internal OPLL_SLOT[] slot = new OPLL_SLOT[18];
			[StateNotSaved()]
			public OPLL_SLOT[] Slot { get { return this.slot; } set { this.slot = value; } }

			/* Voice Data */
			internal OPLL_PATCH[] patch = new OPLL_PATCH[19 * 2];
			[StateNotSaved()]
			public OPLL_PATCH[] Patch { get { return this.patch; } set { this.patch = value; } }
			internal e_int32[] patch_update = new e_int32[2]; /* flag for check patch update */
			public e_int32[] PatchUpdate { get { return this.patch_update; } set { this.patch_update = value; } }

			internal e_uint32 mask;
			public e_uint32 Mask { get { return this.mask; } set { this.mask = value; } }

			public OPLL() {
				for (int i = 0; i < this.slot.Length; ++i) this.slot[i] = new OPLL_SLOT();
				for (int i = 0; i < this.patch.Length; i++) this.patch[i] = new OPLL_PATCH();
			}

		}

	}
}