using System;
using e_uint8 = System.Byte;
using e_int16 = System.Int16;
using e_uint16 = System.Int16;
using e_int32 = System.Int32;
using e_uint32 = System.UInt32;

namespace BeeDevelopment.Sega8Bit.Hardware {

	public partial class Emu2413 {

		const int OPLL_TONE_NUM = 3;

		static byte[][] default_inst = new byte[3][] {
			new byte[] {
				0x49,0x4c,0x4c,0x32,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x61,0x61,0x1E,0x17,0xF0,0x7F,0x00,0x17,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x13,0x41,0x17,0x0E,0xFF,0xFF,0x23,0x13,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x23,0x01,0x9A,0x04,0xA3,0xf4,0xF0,0x23,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x11,0x61,0x0e,0x07,0xfa,0x64,0x70,0x17,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00, 
				0x22,0x21,0x1e,0x06,0xf0,0x76,0x00,0x28,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00, 
				0x21,0x22,0x16,0x05,0xf0,0x71,0x00,0x18,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00, 
				0x21,0x61,0x1d,0x07,0x82,0x80,0x10,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00, 
				0x23,0x21,0x2d,0x16,0x90,0x90,0x00,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00, 
				0x21,0x21,0x1b,0x06,0x64,0x65,0x10,0x17,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00, 
				0x21,0x21,0x0b,0x1a,0x85,0xa0,0x70,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00, 
				0x23,0x01,0x83,0x10,0xff,0xb0,0x10,0x04,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00, 
				0x97,0xc1,0x20,0x07,0xff,0xff,0x22,0x12,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00, 
				0x61,0x00,0x0c,0x05,0xd2,0xf6,0x40,0x43,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00, 
				0x01,0x01,0x56,0x03,0xf4,0xf0,0x03,0x02,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00, 
				0x21,0x41,0x89,0x03,0xf1,0xf4,0xf0,0x23,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00, 
				0x07,0x21,0x14,0x00,0xee,0xf8,0xff,0xf8,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x01,0x31,0x00,0x00,0xf8,0xf7,0xf8,0xf7,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x25,0x11,0x00,0x00,0xf8,0xfa,0xf8,0x55,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
			},
			new byte[] {
				/* VRC7 TONES by okazaki@angel.ne.jp */
				0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x33,0x01,0x09,0x0e,0x94,0x90,0x40,0x01,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x13,0x41,0x0f,0x0d,0xce,0xd3,0x43,0x13,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x01,0x12,0x1b,0x06,0xff,0xd2,0x00,0x32,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x61,0x61,0x1b,0x07,0xaf,0x63,0x20,0x28,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x22,0x21,0x1e,0x06,0xf0,0x76,0x08,0x28,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x66,0x21,0x15,0x00,0x93,0x94,0x20,0xf8,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x21,0x61,0x1c,0x07,0x82,0x81,0x10,0x17,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x23,0x21,0x20,0x1f,0xc0,0x71,0x07,0x47,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x25,0x31,0x26,0x05,0x64,0x41,0x18,0xf8,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x17,0x21,0x28,0x07,0xff,0x83,0x02,0xf8,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x97,0x81,0x25,0x07,0xcf,0xc8,0x02,0x14,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x21,0x21,0x54,0x0f,0x80,0x7f,0x07,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x01,0x01,0x56,0x03,0xd3,0xb2,0x43,0x58,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x31,0x21,0x0c,0x03,0x82,0xc0,0x40,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x21,0x01,0x0c,0x03,0xd4,0xd3,0x40,0x84,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x07,0x21,0x14,0x00,0xee,0xf8,0xff,0xf8,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x01,0x31,0x00,0x00,0xf8,0xf7,0xf8,0xf7,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x25,0x11,0x00,0x00,0xf8,0xfa,0xf8,0x55,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
			},
			new byte[] {
				/* YMF281B tone by Chabin */
				0x49,0x4c,0x4c,0x32,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x62,0x21,0x1a,0x07,0xf0,0x6f,0x00,0x16,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x00,0x10,0x44,0x02,0xf6,0xf4,0x54,0x23,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x03,0x01,0x97,0x04,0xf3,0xf3,0x13,0xf3,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x01,0x61,0x0a,0x0f,0xfa,0x64,0x70,0x17,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x22,0x21,0x1e,0x06,0xf0,0x76,0x00,0x28,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x00,0x61,0x8a,0x0e,0xc0,0x61,0x00,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x21,0x61,0x1b,0x07,0x84,0x80,0x17,0x17,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x37,0x32,0xc9,0x01,0x66,0x64,0x40,0x28,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x01,0x21,0x06,0x03,0xa5,0x71,0x51,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x06,0x11,0x5e,0x07,0xf3,0xf2,0xf6,0x11,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x00,0x20,0x18,0x06,0xf5,0xf3,0x20,0x26,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x97,0x41,0x20,0x07,0xff,0xf4,0x22,0x22,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x65,0x61,0x15,0x00,0xf7,0xf3,0x16,0xf4,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x01,0x31,0x0e,0x07,0xfa,0xf3,0xff,0xff,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x48,0x61,0x09,0x07,0xf1,0x94,0xf0,0xf5,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x07,0x21,0x14,0x00,0xee,0xf8,0xff,0xf8,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x01,0x31,0x00,0x00,0xf8,0xf7,0xf8,0xf7,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
				0x25,0x11,0x00,0x00,0xf8,0xfa,0xf8,0x55,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
			}
		};

		/* Size of Sintable ( 8 -- 18 can be used. 9 recommended.) */
		const int PG_BITS = 9;
		const int PG_WIDTH = (1 << PG_BITS);

		/* Phase increment counter */
		const int DP_BITS = 18;
		const int DP_WIDTH = (1 << DP_BITS);
		const int DP_BASE_BITS = (DP_BITS - PG_BITS);

		/* Dynamic range (Accuracy of sin table) */
		const int DB_BITS = 8;
		const double DB_STEP = (48.0 / (1 << DB_BITS));
		const int DB_MUTE = (1 << DB_BITS);

		/* Dynamic range of envelope */
		const double EG_STEP = 0.375;
		const int EG_BITS = 7;
		const int EG_MUTE = (1 << EG_BITS);

		/* Dynamic range of total level */
		const double TL_STEP = 0.75;
		const int TL_BITS = 6;
		const int TL_MUTE = (1 << TL_BITS);

		/* Dynamic range of sustine level */
		const double SL_STEP = 3.0;
		const int SL_BITS = 4;
		const int SL_MUTE = (1 << SL_BITS);

		static uint EG2DB(uint d) { return ((d) * unchecked((int)(EG_STEP / DB_STEP))); }
		static uint TL2EG(uint d) { return ((d) * unchecked((int)(TL_STEP / EG_STEP))); }
		static uint SL2EG(uint d) { return ((d) * unchecked((int)(SL_STEP / EG_STEP))); }

		static uint DB_POS(double x) { return (e_uint32)((x) / DB_STEP); }
		static uint DB_NEG(double x) { return (e_uint32)(DB_MUTE + DB_MUTE + (x) / DB_STEP); }

		/* Bits for liner value */
		const int DB2LIN_AMP_BITS = 8;
		const int SLOT_AMP_BITS = (DB2LIN_AMP_BITS);

		/* Bits for envelope phase incremental counter */
		const int EG_DP_BITS = 22;
		const int EG_DP_WIDTH = (1 << EG_DP_BITS);

		/* Bits for Pitch and Amp modulator */
		const int PM_PG_BITS = 8;
		const int PM_PG_WIDTH = (1 << PM_PG_BITS);
		const int PM_DP_BITS = 16;
		const int PM_DP_WIDTH = (1 << PM_DP_BITS);
		const int AM_PG_BITS = 8;
		const int AM_PG_WIDTH = (1 << AM_PG_BITS);
		const int AM_DP_BITS = 16;
		const int AM_DP_WIDTH = (1 << AM_DP_BITS);

		/* PM table is calcurated by PM_AMP * pow(2,PM_DEPTH*sin(x)/1200) */
		const int PM_AMP_BITS = 8;
		const int PM_AMP = (1 << PM_AMP_BITS);

		/* PM speed(Hz) and depth(cent) */
		const double PM_SPEED = 6.068835788302951;
		const double PM_DEPTH = 13.75;

		/* AM speed(Hz) and depth(dB) */
		const double AM_SPEED = 3.6413;
		const double AM_DEPTH = 4.875;

		/* Cut the lower b bit(s) off. */
		static uint HIGHBITS(uint c, int b) { return ((c) >> (b)); }

		/* Leave the lower b bit(s). */
		static int LOWBITS(int c, int b) { return ((c) & ((1 << (b)) - 1)); }

		/* Expand x which is s bits to d bits. */
		static int EXPAND_BITS(int x, int s, int d) { return ((x) << ((d) - (s))); }

		/* Expand x which is s bits to d bits and fill expanded bits '1' */
		static int EXPAND_BITS_X(int x, int s, int d) { return (((x) << ((d) - (s))) | ((1 << ((d) - (s))) - 1)); }

		/* Adjust envelope speed which depends on sampling rate. */
		static uint RATE_ADJUST(double x) { return (rate == 49716 ? (uint)x : (e_uint32)((double)(x) * clk / 72 / rate + 0.5)); }        /* added 0.5 to round the value*/

		static OPLL_SLOT MOD(OPLL o, int x) { return ((o).slot[(x) << 1]); }
		static OPLL_SLOT CAR(OPLL o, int x) { return ((o).slot[((x) << 1) | 1]); }

		static bool BIT(uint s, int b) { return (((s) >> (b)) & 1) != 0; }
		static bool BIT(int s, int b) { return (((s) >> (b)) & 1) != 0; }

		/* Input clock */
		static e_uint32 clk = 844451141;
		/* Sampling rate */
		static e_uint32 rate = 3354932;

		/* WaveTable for each envelope amp */
		static e_uint16[] fullsintable = new e_int16[PG_WIDTH];
		static e_uint16[] halfsintable = new e_int16[PG_WIDTH];

		static e_uint16[][] waveform = new e_uint16[2][] { fullsintable, halfsintable };

		/* LFO Table */
		static e_int32[] pmtable = new e_int32[PM_PG_WIDTH];
		static e_int32[] amtable = new e_int32[AM_PG_WIDTH];

		/* Phase delta for LFO */
		static e_uint32 pm_dphase;
		static e_uint32 am_dphase;

		/* dB to Liner table */
		static e_int16[] DB2LIN_TABLE = new e_int16[(DB_MUTE + DB_MUTE) * 2];

		/* Liner to Log curve conversion table (for Attack rate). */
		static e_uint16[] AR_ADJUST_TABLE = new e_uint16[1 << EG_BITS];

		/* Empty voice data */
		//static OPLL_PATCH null_patch = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

		/* Basic voice Data */
		static OPLL_PATCH[,] default_patch = new OPLL_PATCH[OPLL_TONE_NUM, (16 + 3) * 2];

		/* Definition of envelope mode */
		public enum OPLL_EG_STATE { READY, ATTACK, DECAY, SUSHOLD, SUSTINE, RELEASE, SETTLE, FINISH };

		/* Phase incr table for Attack */
		static e_uint32[,] dphaseARTable = new e_uint32[16, 16];
		/* Phase incr table for Decay and Release */
		static e_uint32[,] dphaseDRTable = new e_uint32[16, 16];

		/* KSL + TL Table */
		static e_uint32[, , ,] tllTable = new e_uint32[16, 8, 1 << TL_BITS, 4];
		static e_int32[, ,] rksTable = new e_int32[2, 8, 2];

		/* Phase incr table for PG */
		static e_uint32[, ,] dphaseTable = new e_uint32[512, 8, 16];

		/***************************************************
 
                  Create tables
 
		****************************************************/
		static e_int32 Min(e_int32 i, e_int32 j) {
			if (i < j)
				return i;
			else
				return j;
		}

		/* Table for AR to LogCurve. */
		static void
		makeAdjustTable() {
			e_int32 i;

			AR_ADJUST_TABLE[0] = (1 << EG_BITS) - 1;
			for (i = 1; i < (1 << EG_BITS); i++)
				AR_ADJUST_TABLE[i] = (e_uint16)((double)(1 << EG_BITS) - 1 - ((1 << EG_BITS) - 1) * Math.Log(i) / Math.Log10(127));
		}


		/* Table for dB(0 -- (1<<DB_BITS)-1) to Liner(0 -- DB2LIN_AMP_WIDTH) */
		static void
		makeDB2LinTable() {
			e_int32 i;

			for (i = 0; i < DB_MUTE + DB_MUTE; i++) {
				DB2LIN_TABLE[i] = (e_int16)((double)((1 << DB2LIN_AMP_BITS) - 1) * Math.Pow(10, -(double)i * DB_STEP / 20));
				if (i >= DB_MUTE) DB2LIN_TABLE[i] = 0;
				DB2LIN_TABLE[i + DB_MUTE + DB_MUTE] = (e_int16)(-DB2LIN_TABLE[i]);
			}
		}

		/* Liner(+0.0 - +1.0) to dB((1<<DB_BITS) - 1 -- 0) */
		static e_int32
		lin2db(double d) {
			if (d == 0)
				return (DB_MUTE - 1);
			else
				return Min(-(e_int32)(20.0 * Math.Log10(d) / DB_STEP), DB_MUTE - 1);  /* 0 -- 127 */
		}


		/* Sin Table */
		static void
		makeSinTable() {
			e_int32 i;

			for (i = 0; i < PG_WIDTH / 4; i++) {
				fullsintable[i] = (e_uint16)lin2db(Math.Sin(2.0 * Math.PI * i / PG_WIDTH));
			}

			for (i = 0; i < PG_WIDTH / 4; i++) {
				fullsintable[PG_WIDTH / 2 - 1 - i] = fullsintable[i];
			}

			for (i = 0; i < PG_WIDTH / 2; i++) {
				fullsintable[PG_WIDTH / 2 + i] = (e_uint16)(DB_MUTE + DB_MUTE + fullsintable[i]);
			}

			for (i = 0; i < PG_WIDTH / 2; i++)
				halfsintable[i] = fullsintable[i];
			for (i = PG_WIDTH / 2; i < PG_WIDTH; i++)
				halfsintable[i] = fullsintable[0];
		}

		static double saw(double phase) {
			if (phase <= Math.PI / 2)
				return phase * 2 / Math.PI;
			else if (phase <= Math.PI * 3 / 2)
				return 2.0 - (phase * 2 / Math.PI);
			else
				return -4.0 + phase * 2 / Math.PI;
		}

		/* Table for Pitch Modulator */
		static void
		makePmTable() {
			e_int32 i;

			for (i = 0; i < PM_PG_WIDTH; i++)
				/* pmtable[i] = (e_int32) ((double) PM_AMP * pow (2, (double) PM_DEPTH * sin (2.0 * PI * i / PM_PG_WIDTH) / 1200)); */
				pmtable[i] = (e_int32)((double)PM_AMP * Math.Pow(2, (double)PM_DEPTH * saw(2.0 * Math.PI * i / PM_PG_WIDTH) / 1200));
		}

		/* Table for Amp Modulator */
		static void
		makeAmTable() {
			e_int32 i;

			for (i = 0; i < AM_PG_WIDTH; i++)
				/* amtable[i] = (e_int32) ((double) AM_DEPTH / 2 / DB_STEP * (1.0 + sin (2.0 * PI * i / PM_PG_WIDTH))); */
				amtable[i] = (e_int32)((double)AM_DEPTH / 2 / DB_STEP * (1.0 + saw(2.0 * Math.PI * i / PM_PG_WIDTH)));
		}

		/* Phase increment counter table */
		static void
		makeDphaseTable() {
			e_uint32 fnum, block, ML;
			e_uint32[] mltable = new e_uint32[] { 1, 1 * 2, 2 * 2, 3 * 2, 4 * 2, 5 * 2, 6 * 2, 7 * 2, 8 * 2, 9 * 2, 10 * 2, 10 * 2, 12 * 2, 12 * 2, 15 * 2, 15 * 2 };

			for (fnum = 0; fnum < 512; fnum++)
				for (block = 0; block < 8; block++)
					for (ML = 0; ML < 16; ML++)
						dphaseTable[fnum, block, ML] = RATE_ADJUST(((fnum * mltable[ML]) << (int)block) >> (20 - DP_BITS));
		}

		static double dB2(double x) { return ((x) * 2); }

		static void
		makeTllTable() {


			double[] kltable = new double[] {
				dB2 (0.000), dB2 (9.000), dB2 (12.000), dB2 (13.875), dB2 (15.000), dB2 (16.125), dB2 (16.875), dB2 (17.625),
				dB2 (18.000), dB2 (18.750), dB2 (19.125), dB2 (19.500), dB2 (19.875), dB2 (20.250), dB2 (20.625), dB2 (21.000)
			};

			e_int32 tmp;
			e_int32 fnum, block, TL, KL;

			for (fnum = 0; fnum < 16; fnum++)
				for (block = 0; block < 8; block++)
					for (TL = 0; TL < 64; TL++)
						for (KL = 0; KL < 4; KL++) {
							if (KL == 0) {
								tllTable[fnum, block, TL, KL] = TL2EG((uint)TL);
							} else {
								tmp = (e_int32)(kltable[fnum] - dB2(3.000) * (7 - block));
								if (tmp <= 0)
									tllTable[fnum, block, TL, KL] = TL2EG((uint)TL);
								else
									tllTable[fnum, block, TL, KL] = (e_uint32)((tmp >> (3 - KL)) / EG_STEP) + TL2EG((uint)TL);
							}
						}
		}

		/* Rate Table for Attack */
		static void
		makeDphaseARTable() {
			e_int32 AR, Rks, RM, RL;

			for (AR = 0; AR < 16; AR++)
				for (Rks = 0; Rks < 16; Rks++) {
					RM = AR + (Rks >> 2);
					RL = Rks & 3;
					if (RM > 15)
						RM = 15;
					switch (AR) {
						case 0:
							dphaseARTable[AR, Rks] = 0;
							break;
						case 15:
							dphaseARTable[AR, Rks] = 0;/*EG_DP_WIDTH;*/
							break;
						default:
							dphaseARTable[AR, Rks] = RATE_ADJUST((uint)((3 * (RL + 4) << (RM + 1))));
							break;
					}
				}
		}

		/* Rate Table for Decay and Release */
		static void
		makeDphaseDRTable() {
			e_int32 DR, Rks, RM, RL;

			for (DR = 0; DR < 16; DR++)
				for (Rks = 0; Rks < 16; Rks++) {
					RM = DR + (Rks >> 2);
					RL = Rks & 3;
					if (RM > 15)
						RM = 15;
					switch (DR) {
						case 0:
							dphaseDRTable[DR, Rks] = 0;
							break;
						default:
							dphaseDRTable[DR, Rks] = RATE_ADJUST((uint)((RL + 4) << (RM - 1)));
							break;
					}
				}
		}

		static void
		makeRksTable() {

			e_int32 fnum8, block, KR;

			for (fnum8 = 0; fnum8 < 2; fnum8++)
				for (block = 0; block < 8; block++)
					for (KR = 0; KR < 2; KR++) {
						if (KR != 0)
							rksTable[fnum8, block, KR] = (block << 1) + fnum8;
						else
							rksTable[fnum8, block, KR] = block >> 1;
					}
		}

		static void
		OPLL_dump2patch(e_uint8[] dump, OPLL_PATCH[] patch) {
			patch[0].am = (uint)(dump[0] >> 7) & 1;
			patch[1].am = (uint)(dump[1] >> 7) & 1;
			patch[0].pm = (uint)(dump[0] >> 6) & 1;
			patch[1].pm = (uint)(dump[1] >> 6) & 1;
			patch[0].eg = (uint)(dump[0] >> 5) & 1;
			patch[1].eg = (uint)(dump[1] >> 5) & 1;
			patch[0].kr = (uint)(dump[0] >> 4) & 1;
			patch[1].kr = (uint)(dump[1] >> 4) & 1;
			patch[0].ml = (uint)(dump[0]) & 15;
			patch[1].ml = (uint)(dump[1]) & 15;
			patch[0].kl = (uint)(dump[2] >> 6) & 3;
			patch[1].kl = (uint)(dump[3] >> 6) & 3;
			patch[0].tl = (uint)(dump[2]) & 63;
			patch[0].fb = (uint)(dump[3]) & 7;
			patch[0].wf = (uint)(dump[3] >> 3) & 1;
			patch[1].wf = (uint)(dump[3] >> 4) & 1;
			patch[0].ar = (uint)(dump[4] >> 4) & 15;
			patch[1].ar = (uint)(dump[5] >> 4) & 15;
			patch[0].dr = (uint)(dump[4]) & 15;
			patch[1].dr = (uint)(dump[5]) & 15;
			patch[0].sl = (uint)(dump[6] >> 4) & 15;
			patch[1].sl = (uint)(dump[7] >> 4) & 15;
			patch[0].rr = (uint)(dump[6]) & 15;
			patch[1].rr = (uint)(dump[7]) & 15;
		}

		static void
		OPLL_getDefaultPatch(e_int32 type, e_int32 num, OPLL_PATCH[] patch) {
			byte[] r = new byte[8];
			Array.Copy(default_inst[type], num * 16, r, 0, r.Length);
			OPLL_dump2patch(r, patch);
		}

		static void
		makeDefaultPatch() {
			e_int32 i, j;

			for (i = 0; i < OPLL_TONE_NUM; i++) {
				for (j = 0; j < 19; j++) {
					default_patch[i, j * 2 + 0] = new OPLL_PATCH();
					default_patch[i, j * 2 + 1] = new OPLL_PATCH();
					OPLL_getDefaultPatch(i, j, new[] { default_patch[i, j * 2 + 0], default_patch[i, j * 2 + 1] });
				}
			}

		}

		/************************************************************

							  Calc Parameters

		************************************************************/

		static e_uint32
		calc_eg_dphase(OPLL_SLOT slot) {

			switch (slot.eg_mode) {
				case OPLL_EG_STATE.ATTACK:
					return dphaseARTable[slot.patch.ar, slot.rks];

				case OPLL_EG_STATE.DECAY:
					return dphaseDRTable[slot.patch.dr, slot.rks];

				case OPLL_EG_STATE.SUSHOLD:
					return 0;

				case OPLL_EG_STATE.SUSTINE:
					return dphaseDRTable[slot.patch.rr, slot.rks];

				case OPLL_EG_STATE.RELEASE:
					if (slot.sustine != 0)
						return dphaseDRTable[5, slot.rks];
					else if (slot.patch.eg != 0)
						return dphaseDRTable[slot.patch.rr, slot.rks];
					else
						return dphaseDRTable[7, slot.rks];

				case OPLL_EG_STATE.SETTLE:
					return dphaseDRTable[15, 0];

				case OPLL_EG_STATE.FINISH:
					return 0;

				default:
					return 0;
			}
		}

		/*************************************************************

							OPLL internal interfaces

		*************************************************************/
		const int SLOT_BD1 = 12;
		const int SLOT_BD2 = 13;
		const int SLOT_HH = 14;
		const int SLOT_SD = 15;
		const int SLOT_TOM = 16;
		const int SLOT_CYM = 17;

		static void UPDATE_PG(OPLL_SLOT S) { (S).dphase = dphaseTable[(S).fnum, (S).block, (S).patch.ml]; }
		static void UPDATE_TLL(OPLL_SLOT S) {
			if (S.type == 0) {
				(S).tll = tllTable[((S).fnum) >> 5, (S).block, (S).patch.tl, (S).patch.kl];
			} else {
				(S).tll = tllTable[((S).fnum) >> 5, (S).block, (S).volume, (S).patch.kl];
			}
		}
		static void UPDATE_RKS(OPLL_SLOT S) { (S).rks = (uint)rksTable[((S).fnum) >> 8, (S).block, (S).patch.kr]; }
		static void UPDATE_WF(OPLL_SLOT S) { (S).sintbl = waveform[(S).patch.wf]; }
		static void UPDATE_EG(OPLL_SLOT S) { (S).eg_dphase = calc_eg_dphase(S); }
		static void UPDATE_ALL(OPLL_SLOT S) {
			UPDATE_PG(S);
			UPDATE_TLL(S);
			UPDATE_RKS(S);
			UPDATE_WF(S);
			UPDATE_EG(S);                  /* EG should be updated last. */
		}


		/* Slot key on  */
		static void
		slotOn(OPLL_SLOT slot) {
			slot.eg_mode = OPLL_EG_STATE.ATTACK;
			slot.eg_phase = 0;
			slot.phase = 0;
			UPDATE_EG(slot);
		}

		/* Slot key on without reseting the phase */
		static void
		slotOn2(OPLL_SLOT slot) {
			slot.eg_mode = OPLL_EG_STATE.ATTACK;
			slot.eg_phase = 0;
			UPDATE_EG(slot);
		}

		/* Slot key off */
		static void
		slotOff(OPLL_SLOT slot) {
			if (slot.eg_mode == OPLL_EG_STATE.ATTACK)
				slot.eg_phase = (uint)EXPAND_BITS(AR_ADJUST_TABLE[HIGHBITS(slot.eg_phase, EG_DP_BITS - EG_BITS)], EG_BITS, EG_DP_BITS);
			slot.eg_mode = OPLL_EG_STATE.RELEASE;
			UPDATE_EG(slot);
		}

		/* Channel key on */
		static void
		keyOn(OPLL opll, e_int32 i) {
			if (opll.slot_on_flag[i * 2] == 0)
				slotOn(MOD(opll, i));
			if (opll.slot_on_flag[i * 2 + 1] == 0)
				slotOn(CAR(opll, i));
			opll.key_status[i] = 1;
		}

		/* Channel key off */
		static void
		keyOff(OPLL opll, e_int32 i) {
			if (opll.slot_on_flag[i * 2 + 1] != 0)
				slotOff(CAR(opll, i));
			opll.key_status[i] = 0;
		}

		static void
		keyOn_BD(OPLL opll) {
			keyOn(opll, 6);
		}
		static void
		keyOn_SD(OPLL opll) {
			if (opll.slot_on_flag[SLOT_SD] == 0)
				slotOn(CAR(opll, 7));
		}
		static void
		keyOn_TOM(OPLL opll) {
			if (opll.slot_on_flag[SLOT_TOM] == 0)
				slotOn(MOD(opll, 8));
		}
		static void
		keyOn_HH(OPLL opll) {
			if (opll.slot_on_flag[SLOT_HH] == 0)
				slotOn2(MOD(opll, 7));
		}
		static void
		keyOn_CYM(OPLL opll) {
			if (opll.slot_on_flag[SLOT_CYM] == 0)
				slotOn2(CAR(opll, 8));
		}

		/* Drum key off */
		static void
		keyOff_BD(OPLL opll) {
			keyOff(opll, 6);
		}
		static void
		keyOff_SD(OPLL opll) {
			if (opll.slot_on_flag[SLOT_SD] == 0)
				slotOff(CAR(opll, 7));
		}
		static void
		keyOff_TOM(OPLL opll) {
			if (opll.slot_on_flag[SLOT_TOM] != 0)
				slotOff(MOD(opll, 8));
		}
		static void
		keyOff_HH(OPLL opll) {
			if (opll.slot_on_flag[SLOT_HH] != 0)
				slotOff(MOD(opll, 7));
		}
		static void
		keyOff_CYM(OPLL opll) {
			if (opll.slot_on_flag[SLOT_CYM] != 0)
				slotOff(CAR(opll, 8));
		}

		/* Change a voice */
		static void
		setPatch(OPLL opll, e_int32 i, e_int32 num) {
			opll.patch_number[i] = num;
			MOD(opll, i).patch = opll.patch[num * 2 + 0];
			CAR(opll, i).patch = opll.patch[num * 2 + 1];
		}

		/* Change a rhythm voice */
		static void
		setSlotPatch(OPLL_SLOT slot, OPLL_PATCH patch) {
			slot.patch = patch;
		}

		/* Set sustine parameter */
		static void
		setSustine(OPLL opll, e_int32 c, e_int32 sustine) {
			CAR(opll, c).sustine = sustine;
			if (MOD(opll, c).type != 0)
				MOD(opll, c).sustine = sustine;
		}

		/* Volume : 6bit ( Volume register << 2 ) */
		static void
		setVolume(OPLL opll, e_int32 c, e_int32 volume) {
			CAR(opll, c).volume = volume;
		}

		static void
		setSlotVolume(OPLL_SLOT slot, e_int32 volume) {
			slot.volume = volume;
		}

		/* Set F-Number ( fnum : 9bit ) */
		static void
		setFnumber(OPLL opll, e_int32 c, e_int32 fnum) {
			CAR(opll, c).fnum = fnum;
			MOD(opll, c).fnum = fnum;
		}

		/* Set Block data (block : 3bit ) */
		static void
		setBlock(OPLL opll, e_int32 c, e_int32 block) {
			CAR(opll, c).block = block;
			MOD(opll, c).block = block;
		}

		/* Change Rhythm Mode */
		static void
		update_rhythm_mode(OPLL opll) {
			if ((opll.patch_number[6] & 0x10) != 0) {
				if ((opll.slot_on_flag[SLOT_BD2] | (opll.reg[0x0e] & 32)) == 0) {
					opll.slot[SLOT_BD1].eg_mode = OPLL_EG_STATE.FINISH;
					opll.slot[SLOT_BD2].eg_mode = OPLL_EG_STATE.FINISH;
					setPatch(opll, 6, opll.reg[0x36] >> 4);
				}
			} else if ((opll.reg[0x0e] & 32) != 0) {
				opll.patch_number[6] = 16;
				opll.slot[SLOT_BD1].eg_mode = OPLL_EG_STATE.FINISH;
				opll.slot[SLOT_BD2].eg_mode = OPLL_EG_STATE.FINISH;
				setSlotPatch(opll.slot[SLOT_BD1], opll.patch[16 * 2 + 0]);
				setSlotPatch(opll.slot[SLOT_BD2], opll.patch[16 * 2 + 1]);
			}

			if ((opll.patch_number[7] & 0x10) != 0) {
				if (!((opll.slot_on_flag[SLOT_HH] != 0 && opll.slot_on_flag[SLOT_SD] != 0) | ((opll.reg[0x0e] & 32) != 0))) {
					opll.slot[SLOT_HH].type = 0;
					opll.slot[SLOT_HH].eg_mode = OPLL_EG_STATE.FINISH;
					opll.slot[SLOT_SD].eg_mode = OPLL_EG_STATE.FINISH;
					setPatch(opll, 7, opll.reg[0x37] >> 4);
				}
			} else if ((opll.reg[0x0e] & 32) != 0) {
				opll.patch_number[7] = 17;
				opll.slot[SLOT_HH].type = 1;
				opll.slot[SLOT_HH].eg_mode = OPLL_EG_STATE.FINISH;
				opll.slot[SLOT_SD].eg_mode = OPLL_EG_STATE.FINISH;
				setSlotPatch(opll.slot[SLOT_HH], opll.patch[17 * 2 + 0]);
				setSlotPatch(opll.slot[SLOT_SD], opll.patch[17 * 2 + 1]);
			}

			if ((opll.patch_number[8] & 0x10) != 0) {
				if (!((opll.slot_on_flag[SLOT_CYM] != 0 && opll.slot_on_flag[SLOT_TOM] != 0) | ((opll.reg[0x0e] & 32) != 0))) {
					opll.slot[SLOT_TOM].type = 0;
					opll.slot[SLOT_TOM].eg_mode = OPLL_EG_STATE.FINISH;
					opll.slot[SLOT_CYM].eg_mode = OPLL_EG_STATE.FINISH;
					setPatch(opll, 8, opll.reg[0x38] >> 4);
				}
			} else if ((opll.reg[0x0e] & 32) != 0) {
				opll.patch_number[8] = 18;
				opll.slot[SLOT_TOM].type = 1;
				opll.slot[SLOT_TOM].eg_mode = OPLL_EG_STATE.FINISH;
				opll.slot[SLOT_CYM].eg_mode = OPLL_EG_STATE.FINISH;
				setSlotPatch(opll.slot[SLOT_TOM], opll.patch[18 * 2 + 0]);
				setSlotPatch(opll.slot[SLOT_CYM], opll.patch[18 * 2 + 1]);
			}
		}

		static void
		update_key_status(OPLL opll) {
			int ch;

			for (ch = 0; ch < 9; ch++)
				opll.slot_on_flag[ch * 2] = opll.slot_on_flag[ch * 2 + 1] = (opll.reg[0x20 + ch]) & 0x10;

			if ((opll.reg[0x0e] & 32) != 0) {
				opll.slot_on_flag[SLOT_BD1] |= (opll.reg[0x0e] & 0x10);
				opll.slot_on_flag[SLOT_BD2] |= (opll.reg[0x0e] & 0x10);
				opll.slot_on_flag[SLOT_SD] |= (opll.reg[0x0e] & 0x08);
				opll.slot_on_flag[SLOT_HH] |= (opll.reg[0x0e] & 0x01);
				opll.slot_on_flag[SLOT_TOM] |= (opll.reg[0x0e] & 0x04);
				opll.slot_on_flag[SLOT_CYM] |= (opll.reg[0x0e] & 0x02);
			}
		}

		void
		OPLL_copyPatch(OPLL opll, e_int32 num, OPLL_PATCH patch) {
			opll.patch[num] = (OPLL_PATCH)patch.Clone();
		}

		/***********************************************************

                      Initializing

		***********************************************************/

		static void
		OPLL_SLOT_reset(OPLL_SLOT slot, int type) {
			slot.type = type;
			slot.sintbl = waveform[0];
			slot.phase = 0;
			slot.dphase = 0;
			slot.output[0] = 0;
			slot.output[1] = 0;
			slot.feedback = 0;
			slot.eg_mode = OPLL_EG_STATE.FINISH;
			slot.eg_phase = EG_DP_WIDTH;
			slot.eg_dphase = 0;
			slot.rks = 0;
			slot.tll = 0;
			slot.sustine = 0;
			slot.fnum = 0;
			slot.block = 0;
			slot.volume = 0;
			slot.pgout = 0;
			slot.egout = 0;
			slot.patch = new OPLL_PATCH();
		}

		static void
		internal_refresh() {
			makeDphaseTable();
			makeDphaseARTable();
			makeDphaseDRTable();
			pm_dphase = (e_uint32)RATE_ADJUST(PM_SPEED * PM_DP_WIDTH / (clk / 72));
			am_dphase = (e_uint32)RATE_ADJUST(AM_SPEED * AM_DP_WIDTH / (clk / 72));
		}

		static void
		maketables(e_uint32 c, e_uint32 r) {
			if (c != clk) {
				clk = c;
				makePmTable();
				makeAmTable();
				makeDB2LinTable();
				makeAdjustTable();
				makeTllTable();
				makeRksTable();
				makeSinTable();
				makeDefaultPatch();
			}

			if (r != rate) {
				rate = r;
				internal_refresh();
			}
		}

		OPLL
		OPLL_new(e_uint32 clk, e_uint32 rate) {
			OPLL opll = new OPLL();
			e_int32 i;

			maketables(clk, rate);

			for (i = 0; i < 19 * 2; i++)
				opll.patch[i] = new OPLL_PATCH();

			opll.mask = 0;

			OPLL_reset(opll);
			OPLL_reset_patch(opll, 0);

			return opll;
		}


		void
		OPLL_delete(OPLL opll) {
			/* ... */
		}


		/* Reset patch datas by system default. */
		void
		OPLL_reset_patch(OPLL opll, e_int32 type) {
			e_int32 i;

			for (i = 0; i < 19 * 2; i++)
				OPLL_copyPatch(opll, i, default_patch[type % OPLL_TONE_NUM, i]);
		}

		/* Reset whole of OPLL except patch datas. */
		void
		OPLL_reset(OPLL opll) {
			e_int32 i;

			opll.adr = 0;
			opll.output = 0;

			opll.pm_phase = 0;
			opll.am_phase = 0;

			opll.noise_seed = 0xffff;
			opll.mask = 0;

			for (i = 0; i < 18; i++)
				OPLL_SLOT_reset(opll.slot[i], i % 2);

			for (i = 0; i < 9; i++) {
				opll.key_status[i] = 0;
				setPatch(opll, i, 0);
			}

			for (i = 0; i < 0x40; i++)
				OPLL_writeReg(opll, (uint)i, 0);

			opll.realstep = (e_uint32)((1 << 31) / rate);
			opll.opllstep = (e_uint32)((1 << 31) / (clk / 72));
			opll.oplltime = 0;
			for (i = 0; i < 14; i++)
				opll.pan[i] = 3;
			opll.sprev[0] = opll.sprev[1] = 0;
			opll.snext[0] = opll.snext[1] = 0;

		}

		/* Force Refresh (When external program changes some parameters). */
		void
		OPLL_forceRefresh(OPLL opll) {
			e_int32 i;

			if (opll == null)
				return;

			for (i = 0; i < 9; i++)
				setPatch(opll, i, opll.patch_number[i]);

			for (i = 0; i < 18; i++) {
				UPDATE_PG(opll.slot[i]);
				UPDATE_RKS(opll.slot[i]);
				UPDATE_TLL(opll.slot[i]);
				UPDATE_WF(opll.slot[i]);
				UPDATE_EG(opll.slot[i]);
			}
		}

		void
		OPLL_set_rate(OPLL opll, e_uint32 r) {
			if (opll.quality)
				rate = 49716;
			else
				rate = r;
			internal_refresh();
			rate = r;
		}

		void
		OPLL_set_quality(OPLL opll, bool q) {
			opll.quality = q;
			OPLL_set_rate(opll, rate);
		}

		/*********************************************************

                 Generate wave data

		*********************************************************/
		/* Convert Amp(0 to EG_HEIGHT) to Phase(0 to 2PI). */
		static int wave2_2pi(int e) {
			return ((SLOT_AMP_BITS - PG_BITS) > 0) ?
				((e) >> (SLOT_AMP_BITS - PG_BITS)) :
				((e) << (PG_BITS - SLOT_AMP_BITS));
		}


		/* Convert Amp(0 to EG_HEIGHT) to Phase(0 to 4PI). */
		static int wave2_4pi(int e) {
			if ((SLOT_AMP_BITS - PG_BITS - 1) == 0) {
				return (e);
			} else if ((SLOT_AMP_BITS - PG_BITS - 1) > 0) {
				return ((e) >> (SLOT_AMP_BITS - PG_BITS - 1));
			} else {
				return ((e) << (1 + PG_BITS - SLOT_AMP_BITS));
			}
		}
		/* Convert Amp(0 to EG_HEIGHT) to Phase(0 to 8PI). */
		static int wave2_8pi(int e) {
			if ((SLOT_AMP_BITS - PG_BITS - 2) == 0) {
				return (e);
			} else if ((SLOT_AMP_BITS - PG_BITS - 2) > 0) {
				return ((e) >> (SLOT_AMP_BITS - PG_BITS - 2));
			} else {
				return ((e) << (2 + PG_BITS - SLOT_AMP_BITS));
			}
		}
		/* Update AM, PM unit */
		static void
		update_ampm(OPLL opll) {
			opll.pm_phase = (uint)((opll.pm_phase + pm_dphase) & (PM_DP_WIDTH - 1));
			opll.am_phase = (int)((opll.am_phase + am_dphase) & (AM_DP_WIDTH - 1));
			opll.lfo_am = amtable[HIGHBITS((uint)opll.am_phase, AM_DP_BITS - AM_PG_BITS)];
			opll.lfo_pm = pmtable[HIGHBITS(opll.pm_phase, PM_DP_BITS - PM_PG_BITS)];
		}

		/* PG */
		static void
		calc_phase(OPLL_SLOT slot, e_int32 lfo) {
			if (slot.patch.pm != 0)
				slot.phase += (uint)((slot.dphase * lfo) >> PM_AMP_BITS);
			else
				slot.phase += slot.dphase;

			slot.phase &= unchecked((uint)(DP_WIDTH - 1));

			slot.pgout = HIGHBITS(slot.phase, DP_BASE_BITS);
		}

		/* Update Noise unit */
		static void
		update_noise(OPLL opll) {
			if ((opll.noise_seed & 1) != 0) opll.noise_seed ^= 0x8003020;
			opll.noise_seed >>= 1;
		}

		/* EG */
		static e_uint32 S2E(double x) { return (SL2EG((e_uint32)(x / SL_STEP)) << (EG_DP_BITS - EG_BITS)); }
		static e_uint32[] SL = new[] {
			S2E (0.0), S2E (3.0), S2E (6.0), S2E (9.0), S2E (12.0), S2E (15.0), S2E (18.0), S2E (21.0),
			S2E (24.0), S2E (27.0), S2E (30.0), S2E (33.0), S2E (36.0), S2E (39.0), S2E (42.0), S2E (48.0)
		};
		static void
		calc_envelope(OPLL_SLOT slot, e_int32 lfo) {

			e_uint32 egout;

			switch (slot.eg_mode) {
				case OPLL_EG_STATE.ATTACK:
					egout = (uint)AR_ADJUST_TABLE[HIGHBITS(slot.eg_phase, EG_DP_BITS - EG_BITS)];
					slot.eg_phase += slot.eg_dphase;
					if ((EG_DP_WIDTH & slot.eg_phase) != 0 || (slot.patch.ar == 15)) {
						egout = 0;
						slot.eg_phase = 0;
						slot.eg_mode = OPLL_EG_STATE.DECAY;
						UPDATE_EG(slot);
					}
					break;

				case OPLL_EG_STATE.DECAY:
					egout = HIGHBITS(slot.eg_phase, EG_DP_BITS - EG_BITS);
					slot.eg_phase += slot.eg_dphase;
					if (slot.eg_phase >= SL[slot.patch.sl]) {
						if (slot.patch.eg != 0) {
							slot.eg_phase = SL[slot.patch.sl];
							slot.eg_mode = OPLL_EG_STATE.SUSHOLD;
							UPDATE_EG(slot);
						} else {
							slot.eg_phase = SL[slot.patch.sl];
							slot.eg_mode = OPLL_EG_STATE.SUSTINE;
							UPDATE_EG(slot);
						}
					}
					break;

				case OPLL_EG_STATE.SUSHOLD:
					egout = HIGHBITS(slot.eg_phase, EG_DP_BITS - EG_BITS);
					if (slot.patch.eg == 0) {
						slot.eg_mode = OPLL_EG_STATE.SUSTINE;
						UPDATE_EG(slot);
					}
					break;

				case OPLL_EG_STATE.SUSTINE:
				case OPLL_EG_STATE.RELEASE:
					egout = HIGHBITS(slot.eg_phase, EG_DP_BITS - EG_BITS);
					slot.eg_phase += slot.eg_dphase;
					if (egout >= (1 << EG_BITS)) {
						slot.eg_mode = OPLL_EG_STATE.FINISH;
						egout = (1 << EG_BITS) - 1;
					}
					break;

				case OPLL_EG_STATE.SETTLE:
					egout = HIGHBITS(slot.eg_phase, EG_DP_BITS - EG_BITS);
					slot.eg_phase += slot.eg_dphase;
					if (egout >= (1 << EG_BITS)) {
						slot.eg_mode = OPLL_EG_STATE.ATTACK;
						egout = (1 << EG_BITS) - 1;
						UPDATE_EG(slot);
					}
					break;

				case OPLL_EG_STATE.FINISH:
					egout = (1 << EG_BITS) - 1;
					break;

				default:
					egout = (1 << EG_BITS) - 1;
					break;
			}

			if (slot.patch.am != 0)
				egout = (uint)(EG2DB(egout + slot.tll) + lfo);
			else
				egout = EG2DB(egout + slot.tll);

			if (egout >= DB_MUTE)
				egout = unchecked((uint)DB_MUTE - 1);

			slot.egout = egout | 3;
		}

		/* CARRIOR */
		static e_int32
		calc_slot_car(OPLL_SLOT slot, e_int32 fm) {
			if (slot.egout >= (DB_MUTE - 1)) {
				slot.output[0] = 0;
			} else {
				slot.output[0] = DB2LIN_TABLE[slot.sintbl[(slot.pgout + wave2_8pi(fm)) & (PG_WIDTH - 1)] + slot.egout];
			}

			slot.output[1] = (slot.output[1] + slot.output[0]) >> 1;
			return slot.output[1];
		}

		/* MODULATOR */
		static e_int32
		calc_slot_mod(OPLL_SLOT slot) {
			e_int32 fm;

			slot.output[1] = slot.output[0];

			if (slot.egout >= (DB_MUTE - 1)) {
				slot.output[0] = 0;
			} else if (slot.patch.fb != 0) {
				fm = wave2_4pi(slot.feedback) >> (int)(7 - slot.patch.fb);
				slot.output[0] = DB2LIN_TABLE[slot.sintbl[(slot.pgout + fm) & (PG_WIDTH - 1)] + slot.egout];
			} else {
				slot.output[0] = DB2LIN_TABLE[slot.sintbl[slot.pgout] + slot.egout];
			}

			slot.feedback = (slot.output[1] + slot.output[0]) >> 1;

			return slot.feedback;

		}

		/* TOM */
		static e_int32
		calc_slot_tom(OPLL_SLOT slot) {
			if (slot.egout >= (DB_MUTE - 1))
				return 0;

			return DB2LIN_TABLE[slot.sintbl[slot.pgout] + slot.egout];

		}

		/* SNARE */
		static e_int32
		calc_slot_snare(OPLL_SLOT slot, e_uint32 noise) {
			if (slot.egout >= (DB_MUTE - 1))
				return 0;

			if (BIT(slot.pgout, 7))
				return DB2LIN_TABLE[(noise != 0 ? DB_POS(0.0) : DB_POS(15.0)) + slot.egout];
			else
				return DB2LIN_TABLE[(noise != 0 ? DB_NEG(0.0) : DB_NEG(15.0)) + slot.egout];
		}

		/* 
		  TOP-CYM 
		 */
		static e_int32
		calc_slot_cym(OPLL_SLOT slot, e_uint32 pgout_hh) {
			e_uint32 dbout;

			if (slot.egout >= (DB_MUTE - 1))
				return 0;
			else if (
				/* the same as fmopl.c */
				((BIT(pgout_hh, PG_BITS - 8) ^ BIT(pgout_hh, PG_BITS - 1)) | BIT(pgout_hh, PG_BITS - 7)) ^
				/* different from fmopl.c */
			   (BIT(slot.pgout, PG_BITS - 7) & !BIT(slot.pgout, PG_BITS - 5))
			  )
				dbout = DB_NEG(3.0);
			else
				dbout = DB_POS(3.0);

			return DB2LIN_TABLE[dbout + slot.egout];
		}

		/* 
		  HI-HAT 
		*/
		static e_int32
		calc_slot_hat(OPLL_SLOT slot, e_int32 pgout_cym, e_uint32 noise) {
			e_uint32 dbout;

			if (slot.egout >= (DB_MUTE - 1))
				return 0;
			else if (
				/* the same as fmopl.c */
				((BIT(slot.pgout, PG_BITS - 8) ^ BIT(slot.pgout, PG_BITS - 1)) | BIT(slot.pgout, PG_BITS - 7)) ^
				/* different from fmopl.c */
				(BIT(pgout_cym, PG_BITS - 7) & !BIT(pgout_cym, PG_BITS - 5))
			  ) {
				if (noise != 0)
					dbout = DB_NEG(12.0);
				else
					dbout = DB_NEG(24.0);
			} else {
				if (noise != 0)
					dbout = DB_POS(12.0);
				else
					dbout = DB_POS(24.0);
			}

			return DB2LIN_TABLE[dbout + slot.egout];
		}

		static e_int16
		calc(OPLL opll) {
			e_int32 inst = 0, perc = 0, output = 0;
			e_int32 i;

			update_ampm(opll);
			update_noise(opll);

			for (i = 0; i < 18; i++) {
				calc_phase(opll.slot[i], opll.lfo_pm);
				calc_envelope(opll.slot[i], opll.lfo_am);
			}

			for (i = 0; i < 6; i++)
				if ((opll.mask & OPLL_MASK_CH(i)) == 0 && (CAR(opll, i).eg_mode != OPLL_EG_STATE.FINISH))
					inst += calc_slot_car(CAR(opll, i), calc_slot_mod(MOD(opll, i)));

			/* CH6 */
			if (opll.patch_number[6] <= 15) {
				if ((opll.mask & OPLL_MASK_CH(6)) == 0 && (CAR(opll, 6).eg_mode != OPLL_EG_STATE.FINISH))
					inst += calc_slot_car(CAR(opll, 6), calc_slot_mod(MOD(opll, 6)));
			} else {
				if ((opll.mask & OPLL_MASK_BD) == 0 && (CAR(opll, 6).eg_mode != OPLL_EG_STATE.FINISH))
					perc += calc_slot_car(CAR(opll, 6), calc_slot_mod(MOD(opll, 6)));
			}

			/* CH7 */
			if (opll.patch_number[7] <= 15) {
				if ((opll.mask & OPLL_MASK_CH(7)) == 0 && (CAR(opll, 7).eg_mode != OPLL_EG_STATE.FINISH))
					inst += calc_slot_car(CAR(opll, 7), calc_slot_mod(MOD(opll, 7)));
			} else {
				if ((opll.mask & OPLL_MASK_HH) == 0 && (MOD(opll, 7).eg_mode != OPLL_EG_STATE.FINISH))
					perc += calc_slot_hat(MOD(opll, 7), (int)CAR(opll, 8).pgout, opll.noise_seed & 1);
				if ((opll.mask & OPLL_MASK_SD) == 0 && (CAR(opll, 7).eg_mode != OPLL_EG_STATE.FINISH))
					perc -= calc_slot_snare(CAR(opll, 7), opll.noise_seed & 1);
			}

			/* CH8 */
			if (opll.patch_number[8] <= 15) {
				if ((opll.mask & OPLL_MASK_CH(8)) == 0 && (CAR(opll, 8).eg_mode != OPLL_EG_STATE.FINISH))
					inst += calc_slot_car(CAR(opll, 8), calc_slot_mod(MOD(opll, 8)));
			} else {
				if ((opll.mask & OPLL_MASK_TOM) == 0 && (MOD(opll, 8).eg_mode != OPLL_EG_STATE.FINISH))
					perc += calc_slot_tom(MOD(opll, 8));
				if ((opll.mask & OPLL_MASK_CYM) == 0 && (CAR(opll, 8).eg_mode != OPLL_EG_STATE.FINISH))
					perc -= calc_slot_cym(CAR(opll, 8), MOD(opll, 7).pgout);
			}

			output = inst + (perc << 1);
			return (e_int16)(output << 3);
		}

		e_int16
		OPLL_calc(OPLL opll) {
			if (!opll.quality)
				return calc(opll);

			while (opll.realstep > opll.oplltime) {
				opll.oplltime += opll.opllstep;
				opll.prev = opll.next;
				opll.next = calc(opll);
			}

			opll.oplltime -= opll.realstep;
			opll.output = (e_int16)(((double)opll.next * (opll.opllstep - opll.oplltime)
									+ (double)opll.prev * opll.oplltime) / opll.opllstep);
			return (e_int16)opll.output;
		}

		e_uint32
		OPLL_setMask(OPLL opll, e_uint32 mask) {
			e_uint32 ret;

			if (opll != null) {
				ret = opll.mask;
				opll.mask = mask;
				return ret;
			} else
				return 0;
		}

		e_uint32
		OPLL_toggleMask(OPLL opll, e_uint32 mask) {
			e_uint32 ret;

			if (opll != null) {
				ret = opll.mask;
				opll.mask ^= mask;
				return ret;
			} else
				return 0;
		}

		/****************************************************

							   I/O Ctrl

		*****************************************************/

		void
		OPLL_writeReg(OPLL opll, e_uint32 reg, e_uint32 data) {
			e_int32 i, v, ch;

			data = data & 0xff;
			reg = reg & 0x3f;
			opll.reg[reg] = (e_uint8)data;

			switch (reg) {
				case 0x00:
					opll.patch[0].am = (data >> 7) & 1;
					opll.patch[0].pm = (data >> 6) & 1;
					opll.patch[0].eg = (data >> 5) & 1;
					opll.patch[0].kr = (data >> 4) & 1;
					opll.patch[0].ml = (data) & 15;
					for (i = 0; i < 9; i++) {
						if (opll.patch_number[i] == 0) {
							UPDATE_PG(MOD(opll, i));
							UPDATE_RKS(MOD(opll, i));
							UPDATE_EG(MOD(opll, i));
						}
					}
					break;

				case 0x01:
					opll.patch[1].am = (data >> 7) & 1;
					opll.patch[1].pm = (data >> 6) & 1;
					opll.patch[1].eg = (data >> 5) & 1;
					opll.patch[1].kr = (data >> 4) & 1;
					opll.patch[1].ml = (data) & 15;
					for (i = 0; i < 9; i++) {
						if (opll.patch_number[i] == 0) {
							UPDATE_PG(CAR(opll, i));
							UPDATE_RKS(CAR(opll, i));
							UPDATE_EG(CAR(opll, i));
						}
					}
					break;

				case 0x02:
					opll.patch[0].kl = (data >> 6) & 3;
					opll.patch[0].tl = (data) & 63;
					for (i = 0; i < 9; i++) {
						if (opll.patch_number[i] == 0) {
							UPDATE_TLL(MOD(opll, i));
						}
					}
					break;

				case 0x03:
					opll.patch[1].kl = (data >> 6) & 3;
					opll.patch[1].wf = (data >> 4) & 1;
					opll.patch[0].wf = (data >> 3) & 1;
					opll.patch[0].fb = (data) & 7;
					for (i = 0; i < 9; i++) {
						if (opll.patch_number[i] == 0) {
							UPDATE_WF(MOD(opll, i));
							UPDATE_WF(CAR(opll, i));
						}
					}
					break;

				case 0x04:
					opll.patch[0].ar = (data >> 4) & 15;
					opll.patch[0].dr = (data) & 15;
					for (i = 0; i < 9; i++) {
						if (opll.patch_number[i] == 0) {
							UPDATE_EG(MOD(opll, i));
						}
					}
					break;

				case 0x05:
					opll.patch[1].ar = (data >> 4) & 15;
					opll.patch[1].dr = (data) & 15;
					for (i = 0; i < 9; i++) {
						if (opll.patch_number[i] == 0) {
							UPDATE_EG(CAR(opll, i));
						}
					}
					break;

				case 0x06:
					opll.patch[0].sl = (data >> 4) & 15;
					opll.patch[0].rr = (data) & 15;
					for (i = 0; i < 9; i++) {
						if (opll.patch_number[i] == 0) {
							UPDATE_EG(MOD(opll, i));
						}
					}
					break;

				case 0x07:
					opll.patch[1].sl = (data >> 4) & 15;
					opll.patch[1].rr = (data) & 15;
					for (i = 0; i < 9; i++) {
						if (opll.patch_number[i] == 0) {
							UPDATE_EG(CAR(opll, i));
						}
					}
					break;

				case 0x0e:
					update_rhythm_mode(opll);
					if ((data & 32) != 0) {
						if ((data & 0x10) != 0)
							keyOn_BD(opll);
						else
							keyOff_BD(opll);
						if ((data & 0x8) != 0)
							keyOn_SD(opll);
						else
							keyOff_SD(opll);
						if ((data & 0x4) != 0)
							keyOn_TOM(opll);
						else
							keyOff_TOM(opll);
						if ((data & 0x2) != 0)
							keyOn_CYM(opll);
						else
							keyOff_CYM(opll);
						if ((data & 0x1) != 0)
							keyOn_HH(opll);
						else
							keyOff_HH(opll);
					}
					update_key_status(opll);

					UPDATE_ALL(MOD(opll, 6));
					UPDATE_ALL(CAR(opll, 6));
					UPDATE_ALL(MOD(opll, 7));
					UPDATE_ALL(CAR(opll, 7));
					UPDATE_ALL(MOD(opll, 8));
					UPDATE_ALL(CAR(opll, 8));

					break;

				case 0x0f:
					break;

				case 0x10:
				case 0x11:
				case 0x12:
				case 0x13:
				case 0x14:
				case 0x15:
				case 0x16:
				case 0x17:
				case 0x18:
					ch = (int)(reg - 0x10);
					setFnumber(opll, ch, (int)data + ((opll.reg[0x20 + ch] & 1) << 8));
					UPDATE_ALL(MOD(opll, ch));
					UPDATE_ALL(CAR(opll, ch));
					break;

				case 0x20:
				case 0x21:
				case 0x22:
				case 0x23:
				case 0x24:
				case 0x25:
				case 0x26:
				case 0x27:
				case 0x28:
					ch = (int)(reg - 0x20);
					setFnumber(opll, ch, (int)((data & 1) << 8) + opll.reg[0x10 + ch]);
					setBlock(opll, ch, (int)(data >> 1) & 7);
					setSustine(opll, ch, (int)(data >> 5) & 1);
					if ((data & 0x10) != 0)
						keyOn(opll, ch);
					else
						keyOff(opll, ch);
					UPDATE_ALL(MOD(opll, ch));
					UPDATE_ALL(CAR(opll, ch));
					update_key_status(opll);
					update_rhythm_mode(opll);
					break;

				case 0x30:
				case 0x31:
				case 0x32:
				case 0x33:
				case 0x34:
				case 0x35:
				case 0x36:
				case 0x37:
				case 0x38:
					i = (int)((data >> 4) & 15);
					v = (int)(data & 15);
					if ((opll.reg[0x0e] & 32) != 0 && (reg >= 0x36)) {
						switch (reg) {
							case 0x37:
								setSlotVolume(MOD(opll, 7), i << 2);
								break;
							case 0x38:
								setSlotVolume(MOD(opll, 8), i << 2);
								break;
							default:
								break;
						}
					} else {
						setPatch(opll, (int)(reg - 0x30), i);
					}
					setVolume(opll, (int)(reg - 0x30), v << 2);
					UPDATE_ALL(MOD(opll, (int)(reg - 0x30)));
					UPDATE_ALL(CAR(opll, (int)(reg - 0x30)));
					break;

				default:
					break;

			}
		}

		void
		OPLL_writeIO(OPLL opll, e_uint32 adr, e_uint32 val) {
			if ((adr & 1) != 0)
				OPLL_writeReg(opll, opll.adr, val);
			else
				opll.adr = val;
		}

		/* STEREO MODE (OPT) */
		void
		OPLL_set_pan(OPLL opll, e_uint32 ch, e_uint32 pan) {
			opll.pan[ch & 15] = pan & 3;
		}

		static void
		calc_stereo(OPLL opll, e_int32[] output) {
			e_int32[] b = new[] { 0, 0, 0, 0 };        /* Ignore, Right, Left, Center */
			e_int32[] r = new[] { 0, 0, 0, 0 };        /* Ignore, Right, Left, Center */
			e_int32 i;

			update_ampm(opll);
			update_noise(opll);

			for (i = 0; i < 18; i++) {
				calc_phase(opll.slot[i], opll.lfo_pm);
				calc_envelope(opll.slot[i], opll.lfo_am);
			}

			for (i = 0; i < 6; i++)
				if ((opll.mask & OPLL_MASK_CH(i)) == 0 && (CAR(opll, i).eg_mode != OPLL_EG_STATE.FINISH))
					b[opll.pan[i]] += calc_slot_car(CAR(opll, i), calc_slot_mod(MOD(opll, i)));


			if (opll.patch_number[6] <= 15) {
				if ((opll.mask & OPLL_MASK_CH(6)) == 0 && (CAR(opll, 6).eg_mode != OPLL_EG_STATE.FINISH))
					b[opll.pan[6]] += calc_slot_car(CAR(opll, 6), calc_slot_mod(MOD(opll, 6)));
			} else {
				if ((opll.mask & OPLL_MASK_BD) == 0 && (CAR(opll, 6).eg_mode != OPLL_EG_STATE.FINISH))
					r[opll.pan[9]] += calc_slot_car(CAR(opll, 6), calc_slot_mod(MOD(opll, 6)));
			}

			if (opll.patch_number[7] <= 15) {
				if ((opll.mask & OPLL_MASK_CH(7)) == 0 && (CAR(opll, 7).eg_mode != OPLL_EG_STATE.FINISH))
					b[opll.pan[7]] += calc_slot_car(CAR(opll, 7), calc_slot_mod(MOD(opll, 7)));
			} else {
				if ((opll.mask & OPLL_MASK_HH) == 0 && (MOD(opll, 7).eg_mode != OPLL_EG_STATE.FINISH))
					r[opll.pan[10]] += calc_slot_hat(MOD(opll, 7), (int)CAR(opll, 8).pgout, opll.noise_seed & 1);
				if ((opll.mask & OPLL_MASK_SD) == 0 && (CAR(opll, 7).eg_mode != OPLL_EG_STATE.FINISH))
					r[opll.pan[11]] -= calc_slot_snare(CAR(opll, 7), opll.noise_seed & 1);
			}

			if (opll.patch_number[8] <= 15) {
				if ((opll.mask & OPLL_MASK_CH(8)) == 0 && (CAR(opll, 8).eg_mode != OPLL_EG_STATE.FINISH))
					b[opll.pan[8]] += calc_slot_car(CAR(opll, 8), calc_slot_mod(MOD(opll, 8)));
			} else {
				if ((opll.mask & OPLL_MASK_TOM) == 0 && (MOD(opll, 8).eg_mode != OPLL_EG_STATE.FINISH))
					r[opll.pan[12]] += calc_slot_tom(MOD(opll, 8));
				if ((opll.mask & OPLL_MASK_CYM) == 0 && (CAR(opll, 8).eg_mode != OPLL_EG_STATE.FINISH))
					r[opll.pan[13]] -= calc_slot_cym(CAR(opll, 8), MOD(opll, 7).pgout);
			}

			output[1] = (b[1] + b[3] + ((r[1] + r[3]) << 1)) << 3;
			output[0] = (b[2] + b[3] + ((r[2] + r[3]) << 1)) << 3;
		}

		void
		OPLL_calc_stereo(OPLL opll, e_int32[] output) {
			if (!opll.quality) {
				calc_stereo(opll, output);
				return;
			}

			while (opll.realstep > opll.oplltime) {
				opll.oplltime += opll.opllstep;
				opll.sprev[0] = opll.snext[0];
				opll.sprev[1] = opll.snext[1];
				calc_stereo(opll, opll.snext);
			}

			opll.oplltime -= opll.realstep;
			output[0] = (e_int16)(((double)opll.snext[0] * (opll.opllstep - opll.oplltime)
								 + (double)opll.sprev[0] * opll.oplltime) / opll.opllstep);
			output[1] = (e_int16)(((double)opll.snext[1] * (opll.opllstep - opll.oplltime)
								 + (double)opll.sprev[1] * opll.oplltime) / opll.opllstep);
		}

	}
}