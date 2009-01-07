using System;
using e_int32 = System.Int32;
using e_uint16 = System.UInt16;
using e_uint32 = System.UInt32;
using e_uint8 = System.Byte;

namespace BeeDevelopment.Sega8Bit.Hardware {

	public partial class Emu2413 {

		enum OPLL_TONE_ENUM { OPLL_2413_TONE = 0, OPLL_VRC7_TONE = 1, OPLL_281B_TONE = 2 } ;

		/* voice data */
		class OPLL_PATCH : ICloneable {
			public e_uint32 TL, FB, EG, ML, AR, DR, SL, RR, KR, KL, AM, PM, WF;

			public object Clone() {
				OPLL_PATCH o = new OPLL_PATCH();
				o.TL = this.TL; o.FB = this.FB; o.EG = this.EG; o.ML = this.ML; o.AR = this.AR; o.DR = this.DR; o.SL = this.SL; o.RR = this.RR; o.KR = this.KR; o.KL = this.KL; o.AM = this.AM; o.PM = this.PM; o.WF = this.WF;
				return o;
			}

		}

		/* slot */
		class OPLL_SLOT {

			public OPLL_PATCH patch;

			public e_int32 type;          /* 0 : modulator 1 : carrier */

			/* OUTPUT */
			public e_int32 feedback;
			public e_int32[] output = new e_int32[2];   /* Output value of slot */

			/* for Phase Generator (PG) */
			public e_uint16[] sintbl;   /* Wavetable */
			public e_uint32 phase;      /* Phase */
			public e_uint32 dphase;     /* Phase increment amount */
			public e_uint32 pgout;      /* output */

			/* for Envelope Generator (EG) */
			public e_int32 fnum;          /* F-Number */
			public e_int32 block;         /* Block */
			public e_int32 volume;        /* Current volume */
			public e_int32 sustine;       /* Sustine 1 = ON, 0 = OFF */
			public e_uint32 tll;	      /* Total Level + Key scale level*/
			public e_uint32 rks;        /* Key scale offset (Rks) */
			public OPLL_EG_STATE eg_mode;       /* Current state */
			public e_uint32 eg_phase;   /* Phase */
			public e_uint32 eg_dphase;  /* Phase increment amount */
			public e_uint32 egout;      /* output */

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
		class OPLL {

			public e_uint32 adr;
			public e_int32 output;

			public e_uint32 realstep;
			public e_uint32 oplltime;
			public e_uint32 opllstep;
			public e_int32 prev, next;
			public e_int32[] sprev = new e_int32[2], snext = new e_int32[2];
			public e_uint32[] pan = new e_uint32[16];

			/* Register */
			public e_uint8[] reg = new e_uint8[0x40];
			public e_int32[] slot_on_flag = new e_int32[18];

			/* Pitch Modulator */
			public e_uint32 pm_phase;
			public e_int32 lfo_pm;

			/* Amp Modulator */
			public e_int32 am_phase;
			public e_int32 lfo_am;

			public bool quality;

			/* Noise Generator */
			public e_uint32 noise_seed;

			/* Channel Data */
			public e_int32[] patch_number = new e_int32[9];
			public e_int32[] key_status = new e_int32[9];

			/* Slot */
			public OPLL_SLOT[] slot = new OPLL_SLOT[18];

			/* Voice Data */
			public OPLL_PATCH[] patch = new OPLL_PATCH[19 * 2];
			public e_int32[] patch_update = new e_int32[2]; /* flag for check patch update */

			public e_uint32 mask;

			public OPLL() {
				for (int i = 0; i < this.slot.Length; ++i) this.slot[i] = new OPLL_SLOT();
				for (int i = 0; i < this.patch.Length; i++) this.patch[i] = new OPLL_PATCH();
			}

		}

	}
}