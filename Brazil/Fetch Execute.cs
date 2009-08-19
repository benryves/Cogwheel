using System.Collections.Generic;
namespace BeeDevelopment.Brazil {
	public partial class Z80A {
	
		[StateNotSaved()]
		private int totalExecutedCycles;
		/// <summary>
		/// Gets or sets the total number of clock cycles actually executed.
		/// </summary>
		public int TotalExecutedCycles { get { return this.totalExecutedCycles; } set { this.totalExecutedCycles = value; } }

		[StateNotSaved()]
		private int expectedExecutedCycles;
		/// <summary>
		/// Gets or sets the number of clock cycles that have been requested to be executed.
		/// </summary>		
		public int ExpectedExecutedCycles { get { return this.expectedExecutedCycles; } set { this.expectedExecutedCycles = value; } }

		[StateNotSaved()]
		private int pendingCycles;
		/// <summary>
		/// Gets or sets the number of clock cycles left to run.
		/// </summary>
		public int PendingCycles { get { return this.pendingCycles; } set { this.pendingCycles = value; } }
		
		/// <summary>
		/// Runs the CPU for a particular number of clock cycles.
		/// </summary>
		/// <param name="cycles">The number of cycles to run the CPU emulator for. Specify -1 to run for a single instruction.</param>
		public void FetchExecute(int cycles) {
			//*/
			
			this.expectedExecutedCycles += cycles;
			this.pendingCycles += cycles;
			
			sbyte Displacement;
			
			byte TB; byte TBH; byte TBL; byte TB1; byte TB2; sbyte TSB; ushort TUS; int TI1; int TI2; int TIR;

			bool Interruptable;

			while (this.pendingCycles > 0) {

				Interruptable = true;

				if (this.halted) {
					++RegR;
					this.totalExecutedCycles += 4; this.pendingCycles -= 4;
				} else {
					++RegR;
					switch (ReadMemory(RegPC.Value16++)) {
						case 0x00: // NOP
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x01: // LD BC, nn
							RegBC.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							break;
						case 0x02: // LD (BC), A
							WriteMemory(RegBC.Value16, RegAF.High8);
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0x03: // INC BC
							++RegBC.Value16;
							this.totalExecutedCycles += 6; this.pendingCycles -= 6;
							break;
						case 0x04: // INC B
							RegAF.Low8 = (byte)(TableInc[++RegBC.High8] | (RegAF.Low8 & 1));
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x05: // DEC B
							RegAF.Low8 = (byte)(TableDec[--RegBC.High8] | (RegAF.Low8 & 1));
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x06: // LD B, n
							RegBC.High8 = ReadMemory(RegPC.Value16++);
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0x07: // RLCA
							RegAF.Value16 = TableRotShift[0, 0, RegAF.Value16];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x08: // EX AF, AF'
							TUS = RegAF.Value16; RegAF.Value16 = RegAltAF.Value16; RegAltAF.Value16 = TUS;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x09: // ADD HL, BC
							TI1 = (short)RegHL.Value16; TI2 = (short)RegBC.Value16; TIR = TI1 + TI2;
							TUS = (ushort)TIR;
							RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
							RegFlagN = false;
							RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
							RegHL.Value16 = TUS;
							RegFlag3 = (TUS & 0x0800) != 0;
							RegFlag5 = (TUS & 0x2000) != 0;
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							break;
						case 0x0A: // LD A, (BC)
							RegAF.High8 = ReadMemory(RegBC.Value16);
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0x0B: // DEC BC
							--RegBC.Value16;
							this.totalExecutedCycles += 6; this.pendingCycles -= 6;
							break;
						case 0x0C: // INC C
							RegAF.Low8 = (byte)(TableInc[++RegBC.Low8] | (RegAF.Low8 & 1));
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x0D: // DEC C
							RegAF.Low8 = (byte)(TableDec[--RegBC.Low8] | (RegAF.Low8 & 1));
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x0E: // LD C, n
							RegBC.Low8 = ReadMemory(RegPC.Value16++);
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0x0F: // RRCA
							RegAF.Value16 = TableRotShift[0, 1, RegAF.Value16];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x10: // DJNZ d
							TSB = (sbyte)ReadMemory(RegPC.Value16++);
							if (--RegBC.High8 != 0) {
								RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
								this.totalExecutedCycles += 13; this.pendingCycles -= 13;
							} else {
								this.totalExecutedCycles += 8; this.pendingCycles -= 8;
							}
							break;
						case 0x11: // LD DE, nn
							RegDE.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							break;
						case 0x12: // LD (DE), A
							WriteMemory(RegDE.Value16, RegAF.High8);
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0x13: // INC DE
							++RegDE.Value16;
							this.totalExecutedCycles += 6; this.pendingCycles -= 6;
							break;
						case 0x14: // INC D
							RegAF.Low8 = (byte)(TableInc[++RegDE.High8] | (RegAF.Low8 & 1));
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x15: // DEC D
							RegAF.Low8 = (byte)(TableDec[--RegDE.High8] | (RegAF.Low8 & 1));
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x16: // LD D, n
							RegDE.High8 = ReadMemory(RegPC.Value16++);
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0x17: // RLA
							RegAF.Value16 = TableRotShift[0, 2, RegAF.Value16];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x18: // JR d
							TSB = (sbyte)ReadMemory(RegPC.Value16++);
							RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
								this.totalExecutedCycles += 12; this.pendingCycles -= 12;
							break;
						case 0x19: // ADD HL, DE
							TI1 = (short)RegHL.Value16; TI2 = (short)RegDE.Value16; TIR = TI1 + TI2;
							TUS = (ushort)TIR;
							RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
							RegFlagN = false;
							RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
							RegHL.Value16 = TUS;
							RegFlag3 = (TUS & 0x0800) != 0;
							RegFlag5 = (TUS & 0x2000) != 0;
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							break;
						case 0x1A: // LD A, (DE)
							RegAF.High8 = ReadMemory(RegDE.Value16);
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0x1B: // DEC DE
							--RegDE.Value16;
							this.totalExecutedCycles += 6; this.pendingCycles -= 6;
							break;
						case 0x1C: // INC E
							RegAF.Low8 = (byte)(TableInc[++RegDE.Low8] | (RegAF.Low8 & 1));
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x1D: // DEC E
							RegAF.Low8 = (byte)(TableDec[--RegDE.Low8] | (RegAF.Low8 & 1));
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x1E: // LD E, n
							RegDE.Low8 = ReadMemory(RegPC.Value16++);
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0x1F: // RRA
							RegAF.Value16 = TableRotShift[0, 3, RegAF.Value16];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x20: // JR NZ, d
							TSB = (sbyte)ReadMemory(RegPC.Value16++);
							if (!RegFlagZ) {
								RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
								this.totalExecutedCycles += 12; this.pendingCycles -= 12;
							} else {
								this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							}
							break;
						case 0x21: // LD HL, nn
							RegHL.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							break;
						case 0x22: // LD (nn), HL
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							WriteMemory(TUS++, RegHL.Low8);
							WriteMemory(TUS, RegHL.High8);
							this.totalExecutedCycles += 16; this.pendingCycles -= 16;
							break;
						case 0x23: // INC HL
							++RegHL.Value16;
							this.totalExecutedCycles += 6; this.pendingCycles -= 6;
							break;
						case 0x24: // INC H
							RegAF.Low8 = (byte)(TableInc[++RegHL.High8] | (RegAF.Low8 & 1));
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x25: // DEC H
							RegAF.Low8 = (byte)(TableDec[--RegHL.High8] | (RegAF.Low8 & 1));
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x26: // LD H, n
							RegHL.High8 = ReadMemory(RegPC.Value16++);
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0x27: // DAA
							RegAF.Value16 = TableDaa[RegAF.Value16];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x28: // JR Z, d
							TSB = (sbyte)ReadMemory(RegPC.Value16++);
							if (RegFlagZ) {
								RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
								this.totalExecutedCycles += 12; this.pendingCycles -= 12;
							} else {
								this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							}
							break;
						case 0x29: // ADD HL, HL
							TI1 = (short)RegHL.Value16; TI2 = (short)RegHL.Value16; TIR = TI1 + TI2;
							TUS = (ushort)TIR;
							RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
							RegFlagN = false;
							RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
							RegHL.Value16 = TUS;
							RegFlag3 = (TUS & 0x0800) != 0;
							RegFlag5 = (TUS & 0x2000) != 0;
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							break;
						case 0x2A: // LD HL, (nn)
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							RegHL.Low8 = ReadMemory(TUS++); RegHL.High8 = ReadMemory(TUS);
							this.totalExecutedCycles += 16; this.pendingCycles -= 16;
							break;
						case 0x2B: // DEC HL
							--RegHL.Value16;
							this.totalExecutedCycles += 6; this.pendingCycles -= 6;
							break;
						case 0x2C: // INC L
							RegAF.Low8 = (byte)(TableInc[++RegHL.Low8] | (RegAF.Low8 & 1));
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x2D: // DEC L
							RegAF.Low8 = (byte)(TableDec[--RegHL.Low8] | (RegAF.Low8 & 1));
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x2E: // LD L, n
							RegHL.Low8 = ReadMemory(RegPC.Value16++);
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0x2F: // CPL
							RegAF.High8 ^= 0xFF; RegFlagH = true; RegFlagN = true; RegFlag3 = (RegAF.High8 & 0x08) != 0; RegFlag5 = (RegAF.High8 & 0x20) != 0;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x30: // JR NC, d
							TSB = (sbyte)ReadMemory(RegPC.Value16++);
							if (!RegFlagC) {
								RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
								this.totalExecutedCycles += 12; this.pendingCycles -= 12;
							} else {
								this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							}
							break;
						case 0x31: // LD SP, nn
							RegSP.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							break;
						case 0x32: // LD (nn), A
							WriteMemory((ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256), RegAF.High8);
							this.totalExecutedCycles += 13; this.pendingCycles -= 13;
							break;
						case 0x33: // INC SP
							++RegSP.Value16;
							this.totalExecutedCycles += 6; this.pendingCycles -= 6;
							break;
						case 0x34: // INC (HL)
							TB = ReadMemory(RegHL.Value16); RegAF.Low8 = (byte)(TableInc[++TB] | (RegAF.Low8 & 1)); WriteMemory(RegHL.Value16, TB);
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							break;
						case 0x35: // DEC (HL)
							TB = ReadMemory(RegHL.Value16); RegAF.Low8 = (byte)(TableDec[--TB] | (RegAF.Low8 & 1)); WriteMemory(RegHL.Value16, TB);
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							break;
						case 0x36: // LD (HL), n
							WriteMemory(RegHL.Value16, ReadMemory(RegPC.Value16++));
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							break;
						case 0x37: // SCF
							RegFlagH = false; RegFlagN = false; RegFlagC = true; RegFlag3 = (RegAF.High8 & 0x08) != 0; RegFlag5 = (RegAF.High8 & 0x20) != 0;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x38: // JR C, d
							TSB = (sbyte)ReadMemory(RegPC.Value16++);
							if (RegFlagC) {
								RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
								this.totalExecutedCycles += 12; this.pendingCycles -= 12;
							} else {
								this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							}
							break;
						case 0x39: // ADD HL, SP
							TI1 = (short)RegHL.Value16; TI2 = (short)RegSP.Value16; TIR = TI1 + TI2;
							TUS = (ushort)TIR;
							RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
							RegFlagN = false;
							RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
							RegHL.Value16 = TUS;
							RegFlag3 = (TUS & 0x0800) != 0;
							RegFlag5 = (TUS & 0x2000) != 0;
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							break;
						case 0x3A: // LD A, (nn)
							RegAF.High8 = ReadMemory((ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256));
							this.totalExecutedCycles += 13; this.pendingCycles -= 13;
							break;
						case 0x3B: // DEC SP
							--RegSP.Value16;
							this.totalExecutedCycles += 6; this.pendingCycles -= 6;
							break;
						case 0x3C: // INC A
							RegAF.Low8 = (byte)(TableInc[++RegAF.High8] | (RegAF.Low8 & 1));
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x3D: // DEC A
							RegAF.Low8 = (byte)(TableDec[--RegAF.High8] | (RegAF.Low8 & 1));
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x3E: // LD A, n
							RegAF.High8 = ReadMemory(RegPC.Value16++);
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0x3F: // CCF
							RegFlagH = RegFlagC; RegFlagN = false; RegFlagC ^= true; RegFlag3 = (RegAF.High8 & 0x08) != 0; RegFlag5 = (RegAF.High8 & 0x20) != 0;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x40: // LD B, B
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x41: // LD B, C
							RegBC.High8 = RegBC.Low8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x42: // LD B, D
							RegBC.High8 = RegDE.High8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x43: // LD B, E
							RegBC.High8 = RegDE.Low8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x44: // LD B, H
							RegBC.High8 = RegHL.High8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x45: // LD B, L
							RegBC.High8 = RegHL.Low8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x46: // LD B, (HL)
							RegBC.High8 = ReadMemory(RegHL.Value16);
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0x47: // LD B, A
							RegBC.High8 = RegAF.High8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x48: // LD C, B
							RegBC.Low8 = RegBC.High8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x49: // LD C, C
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x4A: // LD C, D
							RegBC.Low8 = RegDE.High8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x4B: // LD C, E
							RegBC.Low8 = RegDE.Low8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x4C: // LD C, H
							RegBC.Low8 = RegHL.High8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x4D: // LD C, L
							RegBC.Low8 = RegHL.Low8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x4E: // LD C, (HL)
							RegBC.Low8 = ReadMemory(RegHL.Value16);
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0x4F: // LD C, A
							RegBC.Low8 = RegAF.High8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x50: // LD D, B
							RegDE.High8 = RegBC.High8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x51: // LD D, C
							RegDE.High8 = RegBC.Low8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x52: // LD D, D
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x53: // LD D, E
							RegDE.High8 = RegDE.Low8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x54: // LD D, H
							RegDE.High8 = RegHL.High8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x55: // LD D, L
							RegDE.High8 = RegHL.Low8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x56: // LD D, (HL)
							RegDE.High8 = ReadMemory(RegHL.Value16);
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0x57: // LD D, A
							RegDE.High8 = RegAF.High8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x58: // LD E, B
							RegDE.Low8 = RegBC.High8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x59: // LD E, C
							RegDE.Low8 = RegBC.Low8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x5A: // LD E, D
							RegDE.Low8 = RegDE.High8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x5B: // LD E, E
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x5C: // LD E, H
							RegDE.Low8 = RegHL.High8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x5D: // LD E, L
							RegDE.Low8 = RegHL.Low8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x5E: // LD E, (HL)
							RegDE.Low8 = ReadMemory(RegHL.Value16);
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0x5F: // LD E, A
							RegDE.Low8 = RegAF.High8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x60: // LD H, B
							RegHL.High8 = RegBC.High8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x61: // LD H, C
							RegHL.High8 = RegBC.Low8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x62: // LD H, D
							RegHL.High8 = RegDE.High8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x63: // LD H, E
							RegHL.High8 = RegDE.Low8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x64: // LD H, H
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x65: // LD H, L
							RegHL.High8 = RegHL.Low8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x66: // LD H, (HL)
							RegHL.High8 = ReadMemory(RegHL.Value16);
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0x67: // LD H, A
							RegHL.High8 = RegAF.High8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x68: // LD L, B
							RegHL.Low8 = RegBC.High8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x69: // LD L, C
							RegHL.Low8 = RegBC.Low8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x6A: // LD L, D
							RegHL.Low8 = RegDE.High8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x6B: // LD L, E
							RegHL.Low8 = RegDE.Low8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x6C: // LD L, H
							RegHL.Low8 = RegHL.High8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x6D: // LD L, L
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x6E: // LD L, (HL)
							RegHL.Low8 = ReadMemory(RegHL.Value16);
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0x6F: // LD L, A
							RegHL.Low8 = RegAF.High8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x70: // LD (HL), B
							WriteMemory(RegHL.Value16, RegBC.High8);
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0x71: // LD (HL), C
							WriteMemory(RegHL.Value16, RegBC.Low8);
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0x72: // LD (HL), D
							WriteMemory(RegHL.Value16, RegDE.High8);
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0x73: // LD (HL), E
							WriteMemory(RegHL.Value16, RegDE.Low8);
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0x74: // LD (HL), H
							WriteMemory(RegHL.Value16, RegHL.High8);
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0x75: // LD (HL), L
							WriteMemory(RegHL.Value16, RegHL.Low8);
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0x76: // HALT
							this.Halt();
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x77: // LD (HL), A
							WriteMemory(RegHL.Value16, RegAF.High8);
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0x78: // LD A, B
							RegAF.High8 = RegBC.High8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x79: // LD A, C
							RegAF.High8 = RegBC.Low8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x7A: // LD A, D
							RegAF.High8 = RegDE.High8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x7B: // LD A, E
							RegAF.High8 = RegDE.Low8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x7C: // LD A, H
							RegAF.High8 = RegHL.High8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x7D: // LD A, L
							RegAF.High8 = RegHL.Low8;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x7E: // LD A, (HL)
							RegAF.High8 = ReadMemory(RegHL.Value16);
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0x7F: // LD A, A
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x80: // ADD A, B
							RegAF.Value16 = TableALU[0, RegAF.High8, RegBC.High8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x81: // ADD A, C
							RegAF.Value16 = TableALU[0, RegAF.High8, RegBC.Low8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x82: // ADD A, D
							RegAF.Value16 = TableALU[0, RegAF.High8, RegDE.High8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x83: // ADD A, E
							RegAF.Value16 = TableALU[0, RegAF.High8, RegDE.Low8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x84: // ADD A, H
							RegAF.Value16 = TableALU[0, RegAF.High8, RegHL.High8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x85: // ADD A, L
							RegAF.Value16 = TableALU[0, RegAF.High8, RegHL.Low8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x86: // ADD A, (HL)
							RegAF.Value16 = TableALU[0, RegAF.High8, ReadMemory(RegHL.Value16), 0];
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0x87: // ADD A, A
							RegAF.Value16 = TableALU[0, RegAF.High8, RegAF.High8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x88: // ADC A, B
							RegAF.Value16 = TableALU[1, RegAF.High8, RegBC.High8, RegFlagC ? 1 : 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x89: // ADC A, C
							RegAF.Value16 = TableALU[1, RegAF.High8, RegBC.Low8, RegFlagC ? 1 : 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x8A: // ADC A, D
							RegAF.Value16 = TableALU[1, RegAF.High8, RegDE.High8, RegFlagC ? 1 : 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x8B: // ADC A, E
							RegAF.Value16 = TableALU[1, RegAF.High8, RegDE.Low8, RegFlagC ? 1 : 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x8C: // ADC A, H
							RegAF.Value16 = TableALU[1, RegAF.High8, RegHL.High8, RegFlagC ? 1 : 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x8D: // ADC A, L
							RegAF.Value16 = TableALU[1, RegAF.High8, RegHL.Low8, RegFlagC ? 1 : 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x8E: // ADC A, (HL)
							RegAF.Value16 = TableALU[1, RegAF.High8, ReadMemory(RegHL.Value16), RegFlagC ? 1 : 0];
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0x8F: // ADC A, A
							RegAF.Value16 = TableALU[1, RegAF.High8, RegAF.High8, RegFlagC ? 1 : 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x90: // SUB B
							RegAF.Value16 = TableALU[2, RegAF.High8, RegBC.High8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x91: // SUB C
							RegAF.Value16 = TableALU[2, RegAF.High8, RegBC.Low8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x92: // SUB D
							RegAF.Value16 = TableALU[2, RegAF.High8, RegDE.High8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x93: // SUB E
							RegAF.Value16 = TableALU[2, RegAF.High8, RegDE.Low8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x94: // SUB H
							RegAF.Value16 = TableALU[2, RegAF.High8, RegHL.High8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x95: // SUB L
							RegAF.Value16 = TableALU[2, RegAF.High8, RegHL.Low8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x96: // SUB (HL)
							RegAF.Value16 = TableALU[2, RegAF.High8, ReadMemory(RegHL.Value16), 0];
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0x97: // SUB A, A
							RegAF.Value16 = TableALU[2, RegAF.High8, RegAF.High8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x98: // SBC A, B
							RegAF.Value16 = TableALU[3, RegAF.High8, RegBC.High8, RegFlagC ? 1 : 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x99: // SBC A, C
							RegAF.Value16 = TableALU[3, RegAF.High8, RegBC.Low8, RegFlagC ? 1 : 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x9A: // SBC A, D
							RegAF.Value16 = TableALU[3, RegAF.High8, RegDE.High8, RegFlagC ? 1 : 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x9B: // SBC A, E
							RegAF.Value16 = TableALU[3, RegAF.High8, RegDE.Low8, RegFlagC ? 1 : 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x9C: // SBC A, H
							RegAF.Value16 = TableALU[3, RegAF.High8, RegHL.High8, RegFlagC ? 1 : 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x9D: // SBC A, L
							RegAF.Value16 = TableALU[3, RegAF.High8, RegHL.Low8, RegFlagC ? 1 : 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x9E: // SBC A, (HL)
							RegAF.Value16 = TableALU[3, RegAF.High8, ReadMemory(RegHL.Value16), RegFlagC ? 1 : 0];
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0x9F: // SBC A, A
							RegAF.Value16 = TableALU[3, RegAF.High8, RegAF.High8, RegFlagC ? 1 : 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0xA0: // AND B
							RegAF.Value16 = TableALU[4, RegAF.High8, RegBC.High8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0xA1: // AND C
							RegAF.Value16 = TableALU[4, RegAF.High8, RegBC.Low8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0xA2: // AND D
							RegAF.Value16 = TableALU[4, RegAF.High8, RegDE.High8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0xA3: // AND E
							RegAF.Value16 = TableALU[4, RegAF.High8, RegDE.Low8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0xA4: // AND H
							RegAF.Value16 = TableALU[4, RegAF.High8, RegHL.High8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0xA5: // AND L
							RegAF.Value16 = TableALU[4, RegAF.High8, RegHL.Low8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0xA6: // AND (HL)
							RegAF.Value16 = TableALU[4, RegAF.High8, ReadMemory(RegHL.Value16), 0];
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0xA7: // AND A
							RegAF.Value16 = TableALU[4, RegAF.High8, RegAF.High8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0xA8: // XOR B
							RegAF.Value16 = TableALU[5, RegAF.High8, RegBC.High8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0xA9: // XOR C
							RegAF.Value16 = TableALU[5, RegAF.High8, RegBC.Low8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0xAA: // XOR D
							RegAF.Value16 = TableALU[5, RegAF.High8, RegDE.High8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0xAB: // XOR E
							RegAF.Value16 = TableALU[5, RegAF.High8, RegDE.Low8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0xAC: // XOR H
							RegAF.Value16 = TableALU[5, RegAF.High8, RegHL.High8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0xAD: // XOR L
							RegAF.Value16 = TableALU[5, RegAF.High8, RegHL.Low8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0xAE: // XOR (HL)
							RegAF.Value16 = TableALU[5, RegAF.High8, ReadMemory(RegHL.Value16), 0];
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0xAF: // XOR A
							RegAF.Value16 = TableALU[5, RegAF.High8, RegAF.High8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0xB0: // OR B
							RegAF.Value16 = TableALU[6, RegAF.High8, RegBC.High8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0xB1: // OR C
							RegAF.Value16 = TableALU[6, RegAF.High8, RegBC.Low8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0xB2: // OR D
							RegAF.Value16 = TableALU[6, RegAF.High8, RegDE.High8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0xB3: // OR E
							RegAF.Value16 = TableALU[6, RegAF.High8, RegDE.Low8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0xB4: // OR H
							RegAF.Value16 = TableALU[6, RegAF.High8, RegHL.High8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0xB5: // OR L
							RegAF.Value16 = TableALU[6, RegAF.High8, RegHL.Low8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0xB6: // OR (HL)
							RegAF.Value16 = TableALU[6, RegAF.High8, ReadMemory(RegHL.Value16), 0];
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0xB7: // OR A
							RegAF.Value16 = TableALU[6, RegAF.High8, RegAF.High8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0xB8: // CP B
							RegAF.Value16 = TableALU[7, RegAF.High8, RegBC.High8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0xB9: // CP C
							RegAF.Value16 = TableALU[7, RegAF.High8, RegBC.Low8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0xBA: // CP D
							RegAF.Value16 = TableALU[7, RegAF.High8, RegDE.High8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0xBB: // CP E
							RegAF.Value16 = TableALU[7, RegAF.High8, RegDE.Low8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0xBC: // CP H
							RegAF.Value16 = TableALU[7, RegAF.High8, RegHL.High8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0xBD: // CP L
							RegAF.Value16 = TableALU[7, RegAF.High8, RegHL.Low8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0xBE: // CP (HL)
							RegAF.Value16 = TableALU[7, RegAF.High8, ReadMemory(RegHL.Value16), 0];
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0xBF: // CP A
							RegAF.Value16 = TableALU[7, RegAF.High8, RegAF.High8, 0];
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0xC0: // RET NZ
							if (!RegFlagZ) {
								RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
								this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							} else {
								this.totalExecutedCycles += 5; this.pendingCycles -= 5;
							}
							break;
						case 0xC1: // POP BC
							RegBC.Low8 = ReadMemory(RegSP.Value16++); RegBC.High8 = ReadMemory(RegSP.Value16++);
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							break;
						case 0xC2: // JP NZ, nn
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (!RegFlagZ) {
								RegPC.Value16 = TUS;
							}
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							break;
						case 0xC3: // JP nn
							RegPC.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							break;
						case 0xC4: // CALL NZ, nn
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (!RegFlagZ) {
								WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
								RegPC.Value16 = TUS;
								this.totalExecutedCycles += 17; this.pendingCycles -= 17;
							} else {
								this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							}
							break;
						case 0xC5: // PUSH BC
							WriteMemory(--RegSP.Value16, RegBC.High8); WriteMemory(--RegSP.Value16, RegBC.Low8);
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							break;
						case 0xC6: // ADD A, n
							RegAF.Value16 = TableALU[0, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0xC7: // RST $00
							WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
							RegPC.Value16 = 0x00;
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							break;
						case 0xC8: // RET Z
							if (RegFlagZ) {
								RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
								this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							} else {
								this.totalExecutedCycles += 5; this.pendingCycles -= 5;
							}
							break;
						case 0xC9: // RET
							RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							break;
						case 0xCA: // JP Z, nn
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (RegFlagZ) {
								RegPC.Value16 = TUS;
							}
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							break;
						case 0xCB: // (Prefix)
							++RegR;
							switch (ReadMemory(RegPC.Value16++)) {
								case 0x00: // RLC B
									TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * RegBC.High8];
									RegBC.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x01: // RLC C
									TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * RegBC.Low8];
									RegBC.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x02: // RLC D
									TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * RegDE.High8];
									RegDE.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x03: // RLC E
									TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * RegDE.Low8];
									RegDE.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x04: // RLC H
									TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * RegHL.High8];
									RegHL.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x05: // RLC L
									TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * RegHL.Low8];
									RegHL.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x06: // RLC (HL)
									TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory(RegHL.Value16)];
									WriteMemory(RegHL.Value16, (byte)(TUS >> 8));
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0x07: // RLC A
									TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * RegAF.High8];
									RegAF.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x08: // RRC B
									TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * RegBC.High8];
									RegBC.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x09: // RRC C
									TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * RegBC.Low8];
									RegBC.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x0A: // RRC D
									TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * RegDE.High8];
									RegDE.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x0B: // RRC E
									TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * RegDE.Low8];
									RegDE.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x0C: // RRC H
									TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * RegHL.High8];
									RegHL.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x0D: // RRC L
									TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * RegHL.Low8];
									RegHL.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x0E: // RRC (HL)
									TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory(RegHL.Value16)];
									WriteMemory(RegHL.Value16, (byte)(TUS >> 8));
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0x0F: // RRC A
									TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * RegAF.High8];
									RegAF.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x10: // RL B
									TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * RegBC.High8];
									RegBC.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x11: // RL C
									TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * RegBC.Low8];
									RegBC.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x12: // RL D
									TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * RegDE.High8];
									RegDE.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x13: // RL E
									TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * RegDE.Low8];
									RegDE.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x14: // RL H
									TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * RegHL.High8];
									RegHL.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x15: // RL L
									TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * RegHL.Low8];
									RegHL.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x16: // RL (HL)
									TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory(RegHL.Value16)];
									WriteMemory(RegHL.Value16, (byte)(TUS >> 8));
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0x17: // RL A
									TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * RegAF.High8];
									RegAF.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x18: // RR B
									TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * RegBC.High8];
									RegBC.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x19: // RR C
									TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * RegBC.Low8];
									RegBC.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x1A: // RR D
									TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * RegDE.High8];
									RegDE.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x1B: // RR E
									TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * RegDE.Low8];
									RegDE.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x1C: // RR H
									TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * RegHL.High8];
									RegHL.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x1D: // RR L
									TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * RegHL.Low8];
									RegHL.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x1E: // RR (HL)
									TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory(RegHL.Value16)];
									WriteMemory(RegHL.Value16, (byte)(TUS >> 8));
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0x1F: // RR A
									TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * RegAF.High8];
									RegAF.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x20: // SLA B
									TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * RegBC.High8];
									RegBC.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x21: // SLA C
									TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * RegBC.Low8];
									RegBC.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x22: // SLA D
									TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * RegDE.High8];
									RegDE.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x23: // SLA E
									TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * RegDE.Low8];
									RegDE.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x24: // SLA H
									TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * RegHL.High8];
									RegHL.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x25: // SLA L
									TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * RegHL.Low8];
									RegHL.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x26: // SLA (HL)
									TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory(RegHL.Value16)];
									WriteMemory(RegHL.Value16, (byte)(TUS >> 8));
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0x27: // SLA A
									TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * RegAF.High8];
									RegAF.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x28: // SRA B
									TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * RegBC.High8];
									RegBC.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x29: // SRA C
									TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * RegBC.Low8];
									RegBC.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x2A: // SRA D
									TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * RegDE.High8];
									RegDE.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x2B: // SRA E
									TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * RegDE.Low8];
									RegDE.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x2C: // SRA H
									TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * RegHL.High8];
									RegHL.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x2D: // SRA L
									TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * RegHL.Low8];
									RegHL.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x2E: // SRA (HL)
									TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory(RegHL.Value16)];
									WriteMemory(RegHL.Value16, (byte)(TUS >> 8));
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0x2F: // SRA A
									TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * RegAF.High8];
									RegAF.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x30: // SL1 B
									TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * RegBC.High8];
									RegBC.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x31: // SL1 C
									TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * RegBC.Low8];
									RegBC.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x32: // SL1 D
									TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * RegDE.High8];
									RegDE.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x33: // SL1 E
									TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * RegDE.Low8];
									RegDE.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x34: // SL1 H
									TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * RegHL.High8];
									RegHL.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x35: // SL1 L
									TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * RegHL.Low8];
									RegHL.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x36: // SL1 (HL)
									TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory(RegHL.Value16)];
									WriteMemory(RegHL.Value16, (byte)(TUS >> 8));
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0x37: // SL1 A
									TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * RegAF.High8];
									RegAF.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x38: // SRL B
									TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * RegBC.High8];
									RegBC.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x39: // SRL C
									TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * RegBC.Low8];
									RegBC.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x3A: // SRL D
									TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * RegDE.High8];
									RegDE.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x3B: // SRL E
									TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * RegDE.Low8];
									RegDE.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x3C: // SRL H
									TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * RegHL.High8];
									RegHL.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x3D: // SRL L
									TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * RegHL.Low8];
									RegHL.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x3E: // SRL (HL)
									TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory(RegHL.Value16)];
									WriteMemory(RegHL.Value16, (byte)(TUS >> 8));
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0x3F: // SRL A
									TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * RegAF.High8];
									RegAF.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x40: // BIT 0, B
									RegFlagZ = (RegBC.High8 & 0x01) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x41: // BIT 0, C
									RegFlagZ = (RegBC.Low8 & 0x01) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x42: // BIT 0, D
									RegFlagZ = (RegDE.High8 & 0x01) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x43: // BIT 0, E
									RegFlagZ = (RegDE.Low8 & 0x01) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x44: // BIT 0, H
									RegFlagZ = (RegHL.High8 & 0x01) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x45: // BIT 0, L
									RegFlagZ = (RegHL.Low8 & 0x01) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x46: // BIT 0, (HL)
									RegFlagZ = (ReadMemory(RegHL.Value16) & 0x01) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									break;
								case 0x47: // BIT 0, A
									RegFlagZ = (RegAF.High8 & 0x01) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x48: // BIT 1, B
									RegFlagZ = (RegBC.High8 & 0x02) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x49: // BIT 1, C
									RegFlagZ = (RegBC.Low8 & 0x02) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x4A: // BIT 1, D
									RegFlagZ = (RegDE.High8 & 0x02) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x4B: // BIT 1, E
									RegFlagZ = (RegDE.Low8 & 0x02) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x4C: // BIT 1, H
									RegFlagZ = (RegHL.High8 & 0x02) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x4D: // BIT 1, L
									RegFlagZ = (RegHL.Low8 & 0x02) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x4E: // BIT 1, (HL)
									RegFlagZ = (ReadMemory(RegHL.Value16) & 0x02) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									break;
								case 0x4F: // BIT 1, A
									RegFlagZ = (RegAF.High8 & 0x02) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x50: // BIT 2, B
									RegFlagZ = (RegBC.High8 & 0x04) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x51: // BIT 2, C
									RegFlagZ = (RegBC.Low8 & 0x04) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x52: // BIT 2, D
									RegFlagZ = (RegDE.High8 & 0x04) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x53: // BIT 2, E
									RegFlagZ = (RegDE.Low8 & 0x04) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x54: // BIT 2, H
									RegFlagZ = (RegHL.High8 & 0x04) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x55: // BIT 2, L
									RegFlagZ = (RegHL.Low8 & 0x04) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x56: // BIT 2, (HL)
									RegFlagZ = (ReadMemory(RegHL.Value16) & 0x04) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									break;
								case 0x57: // BIT 2, A
									RegFlagZ = (RegAF.High8 & 0x04) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x58: // BIT 3, B
									RegFlagZ = (RegBC.High8 & 0x08) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = !RegFlagZ;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x59: // BIT 3, C
									RegFlagZ = (RegBC.Low8 & 0x08) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = !RegFlagZ;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x5A: // BIT 3, D
									RegFlagZ = (RegDE.High8 & 0x08) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = !RegFlagZ;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x5B: // BIT 3, E
									RegFlagZ = (RegDE.Low8 & 0x08) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = !RegFlagZ;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x5C: // BIT 3, H
									RegFlagZ = (RegHL.High8 & 0x08) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = !RegFlagZ;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x5D: // BIT 3, L
									RegFlagZ = (RegHL.Low8 & 0x08) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = !RegFlagZ;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x5E: // BIT 3, (HL)
									RegFlagZ = (ReadMemory(RegHL.Value16) & 0x08) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = !RegFlagZ;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									break;
								case 0x5F: // BIT 3, A
									RegFlagZ = (RegAF.High8 & 0x08) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = !RegFlagZ;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x60: // BIT 4, B
									RegFlagZ = (RegBC.High8 & 0x10) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x61: // BIT 4, C
									RegFlagZ = (RegBC.Low8 & 0x10) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x62: // BIT 4, D
									RegFlagZ = (RegDE.High8 & 0x10) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x63: // BIT 4, E
									RegFlagZ = (RegDE.Low8 & 0x10) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x64: // BIT 4, H
									RegFlagZ = (RegHL.High8 & 0x10) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x65: // BIT 4, L
									RegFlagZ = (RegHL.Low8 & 0x10) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x66: // BIT 4, (HL)
									RegFlagZ = (ReadMemory(RegHL.Value16) & 0x10) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									break;
								case 0x67: // BIT 4, A
									RegFlagZ = (RegAF.High8 & 0x10) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x68: // BIT 5, B
									RegFlagZ = (RegBC.High8 & 0x20) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = !RegFlagZ;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x69: // BIT 5, C
									RegFlagZ = (RegBC.Low8 & 0x20) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = !RegFlagZ;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x6A: // BIT 5, D
									RegFlagZ = (RegDE.High8 & 0x20) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = !RegFlagZ;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x6B: // BIT 5, E
									RegFlagZ = (RegDE.Low8 & 0x20) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = !RegFlagZ;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x6C: // BIT 5, H
									RegFlagZ = (RegHL.High8 & 0x20) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = !RegFlagZ;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x6D: // BIT 5, L
									RegFlagZ = (RegHL.Low8 & 0x20) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = !RegFlagZ;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x6E: // BIT 5, (HL)
									RegFlagZ = (ReadMemory(RegHL.Value16) & 0x20) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = !RegFlagZ;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									break;
								case 0x6F: // BIT 5, A
									RegFlagZ = (RegAF.High8 & 0x20) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = !RegFlagZ;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x70: // BIT 6, B
									RegFlagZ = (RegBC.High8 & 0x40) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x71: // BIT 6, C
									RegFlagZ = (RegBC.Low8 & 0x40) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x72: // BIT 6, D
									RegFlagZ = (RegDE.High8 & 0x40) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x73: // BIT 6, E
									RegFlagZ = (RegDE.Low8 & 0x40) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x74: // BIT 6, H
									RegFlagZ = (RegHL.High8 & 0x40) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x75: // BIT 6, L
									RegFlagZ = (RegHL.Low8 & 0x40) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x76: // BIT 6, (HL)
									RegFlagZ = (ReadMemory(RegHL.Value16) & 0x40) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									break;
								case 0x77: // BIT 6, A
									RegFlagZ = (RegAF.High8 & 0x40) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x78: // BIT 7, B
									RegFlagZ = (RegBC.High8 & 0x80) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = !RegFlagZ;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x79: // BIT 7, C
									RegFlagZ = (RegBC.Low8 & 0x80) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = !RegFlagZ;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x7A: // BIT 7, D
									RegFlagZ = (RegDE.High8 & 0x80) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = !RegFlagZ;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x7B: // BIT 7, E
									RegFlagZ = (RegDE.Low8 & 0x80) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = !RegFlagZ;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x7C: // BIT 7, H
									RegFlagZ = (RegHL.High8 & 0x80) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = !RegFlagZ;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x7D: // BIT 7, L
									RegFlagZ = (RegHL.Low8 & 0x80) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = !RegFlagZ;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x7E: // BIT 7, (HL)
									RegFlagZ = (ReadMemory(RegHL.Value16) & 0x80) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = !RegFlagZ;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									break;
								case 0x7F: // BIT 7, A
									RegFlagZ = (RegAF.High8 & 0x80) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = !RegFlagZ;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x80: // RES 0, B
									RegBC.High8 &= unchecked((byte)~0x01);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x81: // RES 0, C
									RegBC.Low8 &= unchecked((byte)~0x01);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x82: // RES 0, D
									RegDE.High8 &= unchecked((byte)~0x01);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x83: // RES 0, E
									RegDE.Low8 &= unchecked((byte)~0x01);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x84: // RES 0, H
									RegHL.High8 &= unchecked((byte)~0x01);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x85: // RES 0, L
									RegHL.Low8 &= unchecked((byte)~0x01);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x86: // RES 0, (HL)
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) & unchecked((byte)~0x01)));
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0x87: // RES 0, A
									RegAF.High8 &= unchecked((byte)~0x01);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x88: // RES 1, B
									RegBC.High8 &= unchecked((byte)~0x02);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x89: // RES 1, C
									RegBC.Low8 &= unchecked((byte)~0x02);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x8A: // RES 1, D
									RegDE.High8 &= unchecked((byte)~0x02);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x8B: // RES 1, E
									RegDE.Low8 &= unchecked((byte)~0x02);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x8C: // RES 1, H
									RegHL.High8 &= unchecked((byte)~0x02);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x8D: // RES 1, L
									RegHL.Low8 &= unchecked((byte)~0x02);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x8E: // RES 1, (HL)
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) & unchecked((byte)~0x02)));
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0x8F: // RES 1, A
									RegAF.High8 &= unchecked((byte)~0x02);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x90: // RES 2, B
									RegBC.High8 &= unchecked((byte)~0x04);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x91: // RES 2, C
									RegBC.Low8 &= unchecked((byte)~0x04);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x92: // RES 2, D
									RegDE.High8 &= unchecked((byte)~0x04);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x93: // RES 2, E
									RegDE.Low8 &= unchecked((byte)~0x04);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x94: // RES 2, H
									RegHL.High8 &= unchecked((byte)~0x04);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x95: // RES 2, L
									RegHL.Low8 &= unchecked((byte)~0x04);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x96: // RES 2, (HL)
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) & unchecked((byte)~0x04)));
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0x97: // RES 2, A
									RegAF.High8 &= unchecked((byte)~0x04);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x98: // RES 3, B
									RegBC.High8 &= unchecked((byte)~0x08);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x99: // RES 3, C
									RegBC.Low8 &= unchecked((byte)~0x08);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x9A: // RES 3, D
									RegDE.High8 &= unchecked((byte)~0x08);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x9B: // RES 3, E
									RegDE.Low8 &= unchecked((byte)~0x08);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x9C: // RES 3, H
									RegHL.High8 &= unchecked((byte)~0x08);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x9D: // RES 3, L
									RegHL.Low8 &= unchecked((byte)~0x08);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x9E: // RES 3, (HL)
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) & unchecked((byte)~0x08)));
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0x9F: // RES 3, A
									RegAF.High8 &= unchecked((byte)~0x08);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xA0: // RES 4, B
									RegBC.High8 &= unchecked((byte)~0x10);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xA1: // RES 4, C
									RegBC.Low8 &= unchecked((byte)~0x10);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xA2: // RES 4, D
									RegDE.High8 &= unchecked((byte)~0x10);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xA3: // RES 4, E
									RegDE.Low8 &= unchecked((byte)~0x10);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xA4: // RES 4, H
									RegHL.High8 &= unchecked((byte)~0x10);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xA5: // RES 4, L
									RegHL.Low8 &= unchecked((byte)~0x10);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xA6: // RES 4, (HL)
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) & unchecked((byte)~0x10)));
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0xA7: // RES 4, A
									RegAF.High8 &= unchecked((byte)~0x10);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xA8: // RES 5, B
									RegBC.High8 &= unchecked((byte)~0x20);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xA9: // RES 5, C
									RegBC.Low8 &= unchecked((byte)~0x20);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xAA: // RES 5, D
									RegDE.High8 &= unchecked((byte)~0x20);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xAB: // RES 5, E
									RegDE.Low8 &= unchecked((byte)~0x20);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xAC: // RES 5, H
									RegHL.High8 &= unchecked((byte)~0x20);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xAD: // RES 5, L
									RegHL.Low8 &= unchecked((byte)~0x20);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xAE: // RES 5, (HL)
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) & unchecked((byte)~0x20)));
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0xAF: // RES 5, A
									RegAF.High8 &= unchecked((byte)~0x20);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xB0: // RES 6, B
									RegBC.High8 &= unchecked((byte)~0x40);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xB1: // RES 6, C
									RegBC.Low8 &= unchecked((byte)~0x40);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xB2: // RES 6, D
									RegDE.High8 &= unchecked((byte)~0x40);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xB3: // RES 6, E
									RegDE.Low8 &= unchecked((byte)~0x40);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xB4: // RES 6, H
									RegHL.High8 &= unchecked((byte)~0x40);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xB5: // RES 6, L
									RegHL.Low8 &= unchecked((byte)~0x40);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xB6: // RES 6, (HL)
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) & unchecked((byte)~0x40)));
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0xB7: // RES 6, A
									RegAF.High8 &= unchecked((byte)~0x40);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xB8: // RES 7, B
									RegBC.High8 &= unchecked((byte)~0x80);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xB9: // RES 7, C
									RegBC.Low8 &= unchecked((byte)~0x80);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xBA: // RES 7, D
									RegDE.High8 &= unchecked((byte)~0x80);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xBB: // RES 7, E
									RegDE.Low8 &= unchecked((byte)~0x80);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xBC: // RES 7, H
									RegHL.High8 &= unchecked((byte)~0x80);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xBD: // RES 7, L
									RegHL.Low8 &= unchecked((byte)~0x80);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xBE: // RES 7, (HL)
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) & unchecked((byte)~0x80)));
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0xBF: // RES 7, A
									RegAF.High8 &= unchecked((byte)~0x80);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xC0: // SET 0, B
									RegBC.High8 |= unchecked((byte)0x01);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xC1: // SET 0, C
									RegBC.Low8 |= unchecked((byte)0x01);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xC2: // SET 0, D
									RegDE.High8 |= unchecked((byte)0x01);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xC3: // SET 0, E
									RegDE.Low8 |= unchecked((byte)0x01);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xC4: // SET 0, H
									RegHL.High8 |= unchecked((byte)0x01);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xC5: // SET 0, L
									RegHL.Low8 |= unchecked((byte)0x01);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xC6: // SET 0, (HL)
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) | unchecked((byte)0x01)));
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0xC7: // SET 0, A
									RegAF.High8 |= unchecked((byte)0x01);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xC8: // SET 1, B
									RegBC.High8 |= unchecked((byte)0x02);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xC9: // SET 1, C
									RegBC.Low8 |= unchecked((byte)0x02);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xCA: // SET 1, D
									RegDE.High8 |= unchecked((byte)0x02);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xCB: // SET 1, E
									RegDE.Low8 |= unchecked((byte)0x02);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xCC: // SET 1, H
									RegHL.High8 |= unchecked((byte)0x02);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xCD: // SET 1, L
									RegHL.Low8 |= unchecked((byte)0x02);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xCE: // SET 1, (HL)
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) | unchecked((byte)0x02)));
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0xCF: // SET 1, A
									RegAF.High8 |= unchecked((byte)0x02);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xD0: // SET 2, B
									RegBC.High8 |= unchecked((byte)0x04);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xD1: // SET 2, C
									RegBC.Low8 |= unchecked((byte)0x04);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xD2: // SET 2, D
									RegDE.High8 |= unchecked((byte)0x04);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xD3: // SET 2, E
									RegDE.Low8 |= unchecked((byte)0x04);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xD4: // SET 2, H
									RegHL.High8 |= unchecked((byte)0x04);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xD5: // SET 2, L
									RegHL.Low8 |= unchecked((byte)0x04);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xD6: // SET 2, (HL)
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) | unchecked((byte)0x04)));
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0xD7: // SET 2, A
									RegAF.High8 |= unchecked((byte)0x04);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xD8: // SET 3, B
									RegBC.High8 |= unchecked((byte)0x08);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xD9: // SET 3, C
									RegBC.Low8 |= unchecked((byte)0x08);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xDA: // SET 3, D
									RegDE.High8 |= unchecked((byte)0x08);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xDB: // SET 3, E
									RegDE.Low8 |= unchecked((byte)0x08);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xDC: // SET 3, H
									RegHL.High8 |= unchecked((byte)0x08);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xDD: // SET 3, L
									RegHL.Low8 |= unchecked((byte)0x08);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xDE: // SET 3, (HL)
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) | unchecked((byte)0x08)));
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0xDF: // SET 3, A
									RegAF.High8 |= unchecked((byte)0x08);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xE0: // SET 4, B
									RegBC.High8 |= unchecked((byte)0x10);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xE1: // SET 4, C
									RegBC.Low8 |= unchecked((byte)0x10);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xE2: // SET 4, D
									RegDE.High8 |= unchecked((byte)0x10);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xE3: // SET 4, E
									RegDE.Low8 |= unchecked((byte)0x10);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xE4: // SET 4, H
									RegHL.High8 |= unchecked((byte)0x10);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xE5: // SET 4, L
									RegHL.Low8 |= unchecked((byte)0x10);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xE6: // SET 4, (HL)
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) | unchecked((byte)0x10)));
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0xE7: // SET 4, A
									RegAF.High8 |= unchecked((byte)0x10);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xE8: // SET 5, B
									RegBC.High8 |= unchecked((byte)0x20);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xE9: // SET 5, C
									RegBC.Low8 |= unchecked((byte)0x20);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xEA: // SET 5, D
									RegDE.High8 |= unchecked((byte)0x20);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xEB: // SET 5, E
									RegDE.Low8 |= unchecked((byte)0x20);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xEC: // SET 5, H
									RegHL.High8 |= unchecked((byte)0x20);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xED: // SET 5, L
									RegHL.Low8 |= unchecked((byte)0x20);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xEE: // SET 5, (HL)
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) | unchecked((byte)0x20)));
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0xEF: // SET 5, A
									RegAF.High8 |= unchecked((byte)0x20);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xF0: // SET 6, B
									RegBC.High8 |= unchecked((byte)0x40);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xF1: // SET 6, C
									RegBC.Low8 |= unchecked((byte)0x40);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xF2: // SET 6, D
									RegDE.High8 |= unchecked((byte)0x40);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xF3: // SET 6, E
									RegDE.Low8 |= unchecked((byte)0x40);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xF4: // SET 6, H
									RegHL.High8 |= unchecked((byte)0x40);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xF5: // SET 6, L
									RegHL.Low8 |= unchecked((byte)0x40);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xF6: // SET 6, (HL)
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) | unchecked((byte)0x40)));
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0xF7: // SET 6, A
									RegAF.High8 |= unchecked((byte)0x40);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xF8: // SET 7, B
									RegBC.High8 |= unchecked((byte)0x80);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xF9: // SET 7, C
									RegBC.Low8 |= unchecked((byte)0x80);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xFA: // SET 7, D
									RegDE.High8 |= unchecked((byte)0x80);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xFB: // SET 7, E
									RegDE.Low8 |= unchecked((byte)0x80);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xFC: // SET 7, H
									RegHL.High8 |= unchecked((byte)0x80);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xFD: // SET 7, L
									RegHL.Low8 |= unchecked((byte)0x80);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xFE: // SET 7, (HL)
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) | unchecked((byte)0x80)));
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0xFF: // SET 7, A
									RegAF.High8 |= unchecked((byte)0x80);
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
							}
							break;
						case 0xCC: // CALL Z, nn
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (RegFlagZ) {
								WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
								RegPC.Value16 = TUS;
								this.totalExecutedCycles += 17; this.pendingCycles -= 17;
							} else {
								this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							}
							break;
						case 0xCD: // CALL nn
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
							RegPC.Value16 = TUS;
							this.totalExecutedCycles += 17; this.pendingCycles -= 17;
							break;
						case 0xCE: // ADC A, n
							RegAF.Value16 = TableALU[1, RegAF.High8, ReadMemory(RegPC.Value16++), RegFlagC ? 1 : 0];
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0xCF: // RST $08
							WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
							RegPC.Value16 = 0x08;
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							break;
						case 0xD0: // RET NC
							if (!RegFlagC) {
								RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
								this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							} else {
								this.totalExecutedCycles += 5; this.pendingCycles -= 5;
							}
							break;
						case 0xD1: // POP DE
							RegDE.Low8 = ReadMemory(RegSP.Value16++); RegDE.High8 = ReadMemory(RegSP.Value16++);
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							break;
						case 0xD2: // JP NC, nn
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (!RegFlagC) {
								RegPC.Value16 = TUS;
							}
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							break;
						case 0xD3: // OUT n, A
							WriteHardware(ReadMemory(RegPC.Value16++), RegAF.High8);
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							break;
						case 0xD4: // CALL NC, nn
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (!RegFlagC) {
								WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
								RegPC.Value16 = TUS;
								this.totalExecutedCycles += 17; this.pendingCycles -= 17;
							} else {
								this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							}
							break;
						case 0xD5: // PUSH DE
							WriteMemory(--RegSP.Value16, RegDE.High8); WriteMemory(--RegSP.Value16, RegDE.Low8);
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							break;
						case 0xD6: // SUB n
							RegAF.Value16 = TableALU[2, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0xD7: // RST $10
							WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
							RegPC.Value16 = 0x10;
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							break;
						case 0xD8: // RET C
							if (RegFlagC) {
								RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
								this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							} else {
								this.totalExecutedCycles += 5; this.pendingCycles -= 5;
							}
							break;
						case 0xD9: // EXX
							TUS = RegBC.Value16; RegBC.Value16 = RegAltBC.Value16; RegAltBC.Value16 = TUS;
							TUS = RegDE.Value16; RegDE.Value16 = RegAltDE.Value16; RegAltDE.Value16 = TUS;
							TUS = RegHL.Value16; RegHL.Value16 = RegAltHL.Value16; RegAltHL.Value16 = TUS;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0xDA: // JP C, nn
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (RegFlagC) {
								RegPC.Value16 = TUS;
							}
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							break;
						case 0xDB: // IN A, n
							RegAF.High8 = ReadHardware((ushort)ReadMemory(RegPC.Value16++));
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							break;
						case 0xDC: // CALL C, nn
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (RegFlagC) {
								WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
								RegPC.Value16 = TUS;
								this.totalExecutedCycles += 17; this.pendingCycles -= 17;
							} else {
								this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							}
							break;
						case 0xDD: // (Prefix)
							++RegR;
							switch (ReadMemory(RegPC.Value16++)) {
								case 0x00: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x01: // LD BC, nn
									RegBC.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0x02: // LD (BC), A
									WriteMemory(RegBC.Value16, RegAF.High8);
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									break;
								case 0x03: // INC BC
									++RegBC.Value16;
									this.totalExecutedCycles += 6; this.pendingCycles -= 6;
									break;
								case 0x04: // INC B
									RegAF.Low8 = (byte)(TableInc[++RegBC.High8] | (RegAF.Low8 & 1));
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x05: // DEC B
									RegAF.Low8 = (byte)(TableDec[--RegBC.High8] | (RegAF.Low8 & 1));
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x06: // LD B, n
									RegBC.High8 = ReadMemory(RegPC.Value16++);
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									break;
								case 0x07: // RLCA
									RegAF.Value16 = TableRotShift[0, 0, RegAF.Value16];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x08: // EX AF, AF'
									TUS = RegAF.Value16; RegAF.Value16 = RegAltAF.Value16; RegAltAF.Value16 = TUS;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x09: // ADD IX, BC
									TI1 = (short)RegIX.Value16; TI2 = (short)RegBC.Value16; TIR = TI1 + TI2;
									TUS = (ushort)TIR;
									RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
									RegFlagN = false;
									RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
									RegIX.Value16 = TUS;
									RegFlag3 = (TUS & 0x0800) != 0;
									RegFlag5 = (TUS & 0x2000) != 0;
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0x0A: // LD A, (BC)
									RegAF.High8 = ReadMemory(RegBC.Value16);
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									break;
								case 0x0B: // DEC BC
									--RegBC.Value16;
									this.totalExecutedCycles += 6; this.pendingCycles -= 6;
									break;
								case 0x0C: // INC C
									RegAF.Low8 = (byte)(TableInc[++RegBC.Low8] | (RegAF.Low8 & 1));
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x0D: // DEC C
									RegAF.Low8 = (byte)(TableDec[--RegBC.Low8] | (RegAF.Low8 & 1));
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x0E: // LD C, n
									RegBC.Low8 = ReadMemory(RegPC.Value16++);
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									break;
								case 0x0F: // RRCA
									RegAF.Value16 = TableRotShift[0, 1, RegAF.Value16];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x10: // DJNZ d
									TSB = (sbyte)ReadMemory(RegPC.Value16++);
									if (--RegBC.High8 != 0) {
										RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
										this.totalExecutedCycles += 13; this.pendingCycles -= 13;
									} else {
										this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									}
									break;
								case 0x11: // LD DE, nn
									RegDE.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0x12: // LD (DE), A
									WriteMemory(RegDE.Value16, RegAF.High8);
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									break;
								case 0x13: // INC DE
									++RegDE.Value16;
									this.totalExecutedCycles += 6; this.pendingCycles -= 6;
									break;
								case 0x14: // INC D
									RegAF.Low8 = (byte)(TableInc[++RegDE.High8] | (RegAF.Low8 & 1));
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x15: // DEC D
									RegAF.Low8 = (byte)(TableDec[--RegDE.High8] | (RegAF.Low8 & 1));
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x16: // LD D, n
									RegDE.High8 = ReadMemory(RegPC.Value16++);
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									break;
								case 0x17: // RLA
									RegAF.Value16 = TableRotShift[0, 2, RegAF.Value16];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x18: // JR d
									TSB = (sbyte)ReadMemory(RegPC.Value16++);
									RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
										this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									break;
								case 0x19: // ADD IX, DE
									TI1 = (short)RegIX.Value16; TI2 = (short)RegDE.Value16; TIR = TI1 + TI2;
									TUS = (ushort)TIR;
									RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
									RegFlagN = false;
									RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
									RegIX.Value16 = TUS;
									RegFlag3 = (TUS & 0x0800) != 0;
									RegFlag5 = (TUS & 0x2000) != 0;
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0x1A: // LD A, (DE)
									RegAF.High8 = ReadMemory(RegDE.Value16);
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									break;
								case 0x1B: // DEC DE
									--RegDE.Value16;
									this.totalExecutedCycles += 6; this.pendingCycles -= 6;
									break;
								case 0x1C: // INC E
									RegAF.Low8 = (byte)(TableInc[++RegDE.Low8] | (RegAF.Low8 & 1));
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x1D: // DEC E
									RegAF.Low8 = (byte)(TableDec[--RegDE.Low8] | (RegAF.Low8 & 1));
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x1E: // LD E, n
									RegDE.Low8 = ReadMemory(RegPC.Value16++);
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									break;
								case 0x1F: // RRA
									RegAF.Value16 = TableRotShift[0, 3, RegAF.Value16];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x20: // JR NZ, d
									TSB = (sbyte)ReadMemory(RegPC.Value16++);
									if (!RegFlagZ) {
										RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
										this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									} else {
										this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									}
									break;
								case 0x21: // LD IX, nn
									RegIX.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									this.totalExecutedCycles += 14; this.pendingCycles -= 14;
									break;
								case 0x22: // LD (nn), IX
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									WriteMemory(TUS++, RegIX.Low8);
									WriteMemory(TUS, RegIX.High8);
									this.totalExecutedCycles += 20; this.pendingCycles -= 20;
									break;
								case 0x23: // INC IX
									++RegIX.Value16;
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0x24: // INC IXH
									RegAF.Low8 = (byte)(TableInc[++RegIX.High8] | (RegAF.Low8 & 1));
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x25: // DEC IXH
									RegAF.Low8 = (byte)(TableDec[--RegIX.High8] | (RegAF.Low8 & 1));
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x26: // LD IXH, n
									RegIX.High8 = ReadMemory(RegPC.Value16++);
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x27: // DAA
									RegAF.Value16 = TableDaa[RegAF.Value16];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x28: // JR Z, d
									TSB = (sbyte)ReadMemory(RegPC.Value16++);
									if (RegFlagZ) {
										RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
										this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									} else {
										this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									}
									break;
								case 0x29: // ADD IX, IX
									TI1 = (short)RegIX.Value16; TI2 = (short)RegIX.Value16; TIR = TI1 + TI2;
									TUS = (ushort)TIR;
									RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
									RegFlagN = false;
									RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
									RegIX.Value16 = TUS;
									RegFlag3 = (TUS & 0x0800) != 0;
									RegFlag5 = (TUS & 0x2000) != 0;
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0x2A: // LD IX, (nn)
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									RegIX.Low8 = ReadMemory(TUS++); RegIX.High8 = ReadMemory(TUS);
									this.totalExecutedCycles += 20; this.pendingCycles -= 20;
									break;
								case 0x2B: // DEC IX
									--RegIX.Value16;
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0x2C: // INC IXL
									RegAF.Low8 = (byte)(TableInc[++RegIX.Low8] | (RegAF.Low8 & 1));
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x2D: // DEC IXL
									RegAF.Low8 = (byte)(TableDec[--RegIX.Low8] | (RegAF.Low8 & 1));
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x2E: // LD IXL, n
									RegIX.Low8 = ReadMemory(RegPC.Value16++);
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x2F: // CPL
									RegAF.High8 ^= 0xFF; RegFlagH = true; RegFlagN = true; RegFlag3 = (RegAF.High8 & 0x08) != 0; RegFlag5 = (RegAF.High8 & 0x20) != 0;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x30: // JR NC, d
									TSB = (sbyte)ReadMemory(RegPC.Value16++);
									if (!RegFlagC) {
										RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
										this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									} else {
										this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									}
									break;
								case 0x31: // LD SP, nn
									RegSP.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0x32: // LD (nn), A
									WriteMemory((ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256), RegAF.High8);
									this.totalExecutedCycles += 13; this.pendingCycles -= 13;
									break;
								case 0x33: // INC SP
									++RegSP.Value16;
									this.totalExecutedCycles += 6; this.pendingCycles -= 6;
									break;
								case 0x34: // INC (IX+d)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									TB = ReadMemory((ushort)(RegIX.Value16 + Displacement)); RegAF.Low8 = (byte)(TableInc[++TB] | (RegAF.Low8 & 1)); WriteMemory((ushort)(RegIX.Value16 + Displacement), TB);
									this.totalExecutedCycles += 23; this.pendingCycles -= 23;
									break;
								case 0x35: // DEC (IX+d)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									TB = ReadMemory((ushort)(RegIX.Value16 + Displacement)); RegAF.Low8 = (byte)(TableDec[--TB] | (RegAF.Low8 & 1)); WriteMemory((ushort)(RegIX.Value16 + Displacement), TB);
									this.totalExecutedCycles += 23; this.pendingCycles -= 23;
									break;
								case 0x36: // LD (IX+d), n
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIX.Value16 + Displacement), ReadMemory(RegPC.Value16++));
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0x37: // SCF
									RegFlagH = false; RegFlagN = false; RegFlagC = true; RegFlag3 = (RegAF.High8 & 0x08) != 0; RegFlag5 = (RegAF.High8 & 0x20) != 0;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x38: // JR C, d
									TSB = (sbyte)ReadMemory(RegPC.Value16++);
									if (RegFlagC) {
										RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
										this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									} else {
										this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									}
									break;
								case 0x39: // ADD IX, SP
									TI1 = (short)RegIX.Value16; TI2 = (short)RegSP.Value16; TIR = TI1 + TI2;
									TUS = (ushort)TIR;
									RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
									RegFlagN = false;
									RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
									RegIX.Value16 = TUS;
									RegFlag3 = (TUS & 0x0800) != 0;
									RegFlag5 = (TUS & 0x2000) != 0;
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0x3A: // LD A, (nn)
									RegAF.High8 = ReadMemory((ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256));
									this.totalExecutedCycles += 13; this.pendingCycles -= 13;
									break;
								case 0x3B: // DEC SP
									--RegSP.Value16;
									this.totalExecutedCycles += 6; this.pendingCycles -= 6;
									break;
								case 0x3C: // INC A
									RegAF.Low8 = (byte)(TableInc[++RegAF.High8] | (RegAF.Low8 & 1));
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x3D: // DEC A
									RegAF.Low8 = (byte)(TableDec[--RegAF.High8] | (RegAF.Low8 & 1));
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x3E: // LD A, n
									RegAF.High8 = ReadMemory(RegPC.Value16++);
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									break;
								case 0x3F: // CCF
									RegFlagH = RegFlagC; RegFlagN = false; RegFlagC ^= true; RegFlag3 = (RegAF.High8 & 0x08) != 0; RegFlag5 = (RegAF.High8 & 0x20) != 0;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x40: // LD B, B
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x41: // LD B, C
									RegBC.High8 = RegBC.Low8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x42: // LD B, D
									RegBC.High8 = RegDE.High8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x43: // LD B, E
									RegBC.High8 = RegDE.Low8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x44: // LD B, IXH
									RegBC.High8 = RegIX.High8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x45: // LD B, IXL
									RegBC.High8 = RegIX.Low8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x46: // LD B, (IX+d)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegBC.High8 = ReadMemory((ushort)(RegIX.Value16 + Displacement));
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0x47: // LD B, A
									RegBC.High8 = RegAF.High8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x48: // LD C, B
									RegBC.Low8 = RegBC.High8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x49: // LD C, C
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x4A: // LD C, D
									RegBC.Low8 = RegDE.High8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x4B: // LD C, E
									RegBC.Low8 = RegDE.Low8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x4C: // LD C, IXH
									RegBC.Low8 = RegIX.High8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x4D: // LD C, IXL
									RegBC.Low8 = RegIX.Low8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x4E: // LD C, (IX+d)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegBC.Low8 = ReadMemory((ushort)(RegIX.Value16 + Displacement));
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0x4F: // LD C, A
									RegBC.Low8 = RegAF.High8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x50: // LD D, B
									RegDE.High8 = RegBC.High8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x51: // LD D, C
									RegDE.High8 = RegBC.Low8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x52: // LD D, D
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x53: // LD D, E
									RegDE.High8 = RegDE.Low8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x54: // LD D, IXH
									RegDE.High8 = RegIX.High8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x55: // LD D, IXL
									RegDE.High8 = RegIX.Low8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x56: // LD D, (IX+d)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegDE.High8 = ReadMemory((ushort)(RegIX.Value16 + Displacement));
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0x57: // LD D, A
									RegDE.High8 = RegAF.High8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x58: // LD E, B
									RegDE.Low8 = RegBC.High8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x59: // LD E, C
									RegDE.Low8 = RegBC.Low8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x5A: // LD E, D
									RegDE.Low8 = RegDE.High8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x5B: // LD E, E
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x5C: // LD E, IXH
									RegDE.Low8 = RegIX.High8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x5D: // LD E, IXL
									RegDE.Low8 = RegIX.Low8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x5E: // LD E, (IX+d)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegDE.Low8 = ReadMemory((ushort)(RegIX.Value16 + Displacement));
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0x5F: // LD E, A
									RegDE.Low8 = RegAF.High8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x60: // LD IXH, B
									RegIX.High8 = RegBC.High8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x61: // LD IXH, C
									RegIX.High8 = RegBC.Low8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x62: // LD IXH, D
									RegIX.High8 = RegDE.High8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x63: // LD IXH, E
									RegIX.High8 = RegDE.Low8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x64: // LD IXH, IXH
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x65: // LD IXH, IXL
									RegIX.High8 = RegIX.Low8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x66: // LD H, (IX+d)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegHL.High8 = ReadMemory((ushort)(RegIX.Value16 + Displacement));
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0x67: // LD IXH, A
									RegIX.High8 = RegAF.High8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x68: // LD IXL, B
									RegIX.Low8 = RegBC.High8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x69: // LD IXL, C
									RegIX.Low8 = RegBC.Low8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x6A: // LD IXL, D
									RegIX.Low8 = RegDE.High8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x6B: // LD IXL, E
									RegIX.Low8 = RegDE.Low8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x6C: // LD IXL, IXH
									RegIX.Low8 = RegIX.High8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x6D: // LD IXL, IXL
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x6E: // LD L, (IX+d)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegHL.Low8 = ReadMemory((ushort)(RegIX.Value16 + Displacement));
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0x6F: // LD IXL, A
									RegIX.Low8 = RegAF.High8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x70: // LD (IX+d), B
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0x71: // LD (IX+d), C
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0x72: // LD (IX+d), D
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0x73: // LD (IX+d), E
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0x74: // LD (IX+d), H
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0x75: // LD (IX+d), L
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0x76: // HALT
									this.Halt();
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x77: // LD (IX+d), A
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0x78: // LD A, B
									RegAF.High8 = RegBC.High8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x79: // LD A, C
									RegAF.High8 = RegBC.Low8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x7A: // LD A, D
									RegAF.High8 = RegDE.High8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x7B: // LD A, E
									RegAF.High8 = RegDE.Low8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x7C: // LD A, IXH
									RegAF.High8 = RegIX.High8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x7D: // LD A, IXL
									RegAF.High8 = RegIX.Low8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x7E: // LD A, (IX+d)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.High8 = ReadMemory((ushort)(RegIX.Value16 + Displacement));
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0x7F: // LD A, A
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x80: // ADD A, B
									RegAF.Value16 = TableALU[0, RegAF.High8, RegBC.High8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x81: // ADD A, C
									RegAF.Value16 = TableALU[0, RegAF.High8, RegBC.Low8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x82: // ADD A, D
									RegAF.Value16 = TableALU[0, RegAF.High8, RegDE.High8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x83: // ADD A, E
									RegAF.Value16 = TableALU[0, RegAF.High8, RegDE.Low8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x84: // ADD A, IXH
									RegAF.Value16 = TableALU[0, RegAF.High8, RegIX.High8, 0];
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x85: // ADD A, IXL
									RegAF.Value16 = TableALU[0, RegAF.High8, RegIX.Low8, 0];
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x86: // ADD A, (IX+d)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[0, RegAF.High8, ReadMemory((ushort)(RegIX.Value16 + Displacement)), 0];
									this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									break;
								case 0x87: // ADD A, A
									RegAF.Value16 = TableALU[0, RegAF.High8, RegAF.High8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x88: // ADC A, B
									RegAF.Value16 = TableALU[1, RegAF.High8, RegBC.High8, RegFlagC ? 1 : 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x89: // ADC A, C
									RegAF.Value16 = TableALU[1, RegAF.High8, RegBC.Low8, RegFlagC ? 1 : 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x8A: // ADC A, D
									RegAF.Value16 = TableALU[1, RegAF.High8, RegDE.High8, RegFlagC ? 1 : 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x8B: // ADC A, E
									RegAF.Value16 = TableALU[1, RegAF.High8, RegDE.Low8, RegFlagC ? 1 : 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x8C: // ADC A, IXH
									RegAF.Value16 = TableALU[1, RegAF.High8, RegIX.High8, RegFlagC ? 1 : 0];
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x8D: // ADC A, IXL
									RegAF.Value16 = TableALU[1, RegAF.High8, RegIX.Low8, RegFlagC ? 1 : 0];
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x8E: // ADC A, (IX+d)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[1, RegAF.High8, ReadMemory((ushort)(RegIX.Value16 + Displacement)), RegFlagC ? 1 : 0];
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0x8F: // ADC A, A
									RegAF.Value16 = TableALU[1, RegAF.High8, RegAF.High8, RegFlagC ? 1 : 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x90: // SUB B
									RegAF.Value16 = TableALU[2, RegAF.High8, RegBC.High8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x91: // SUB C
									RegAF.Value16 = TableALU[2, RegAF.High8, RegBC.Low8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x92: // SUB D
									RegAF.Value16 = TableALU[2, RegAF.High8, RegDE.High8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x93: // SUB E
									RegAF.Value16 = TableALU[2, RegAF.High8, RegDE.Low8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x94: // SUB IXH
									RegAF.Value16 = TableALU[2, RegAF.High8, RegIX.High8, 0];
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x95: // SUB IXL
									RegAF.Value16 = TableALU[2, RegAF.High8, RegIX.Low8, 0];
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x96: // SUB (IX+d)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[2, RegAF.High8, ReadMemory((ushort)(RegIX.Value16 + Displacement)), 0];
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0x97: // SUB A, A
									RegAF.Value16 = TableALU[2, RegAF.High8, RegAF.High8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x98: // SBC A, B
									RegAF.Value16 = TableALU[3, RegAF.High8, RegBC.High8, RegFlagC ? 1 : 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x99: // SBC A, C
									RegAF.Value16 = TableALU[3, RegAF.High8, RegBC.Low8, RegFlagC ? 1 : 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x9A: // SBC A, D
									RegAF.Value16 = TableALU[3, RegAF.High8, RegDE.High8, RegFlagC ? 1 : 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x9B: // SBC A, E
									RegAF.Value16 = TableALU[3, RegAF.High8, RegDE.Low8, RegFlagC ? 1 : 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x9C: // SBC A, IXH
									RegAF.Value16 = TableALU[3, RegAF.High8, RegIX.High8, RegFlagC ? 1 : 0];
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x9D: // SBC A, IXL
									RegAF.Value16 = TableALU[3, RegAF.High8, RegIX.Low8, RegFlagC ? 1 : 0];
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x9E: // SBC A, (IX+d)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[3, RegAF.High8, ReadMemory((ushort)(RegIX.Value16 + Displacement)), RegFlagC ? 1 : 0];
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0x9F: // SBC A, A
									RegAF.Value16 = TableALU[3, RegAF.High8, RegAF.High8, RegFlagC ? 1 : 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xA0: // AND B
									RegAF.Value16 = TableALU[4, RegAF.High8, RegBC.High8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xA1: // AND C
									RegAF.Value16 = TableALU[4, RegAF.High8, RegBC.Low8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xA2: // AND D
									RegAF.Value16 = TableALU[4, RegAF.High8, RegDE.High8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xA3: // AND E
									RegAF.Value16 = TableALU[4, RegAF.High8, RegDE.Low8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xA4: // AND IXH
									RegAF.Value16 = TableALU[4, RegAF.High8, RegIX.High8, 0];
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0xA5: // AND IXL
									RegAF.Value16 = TableALU[4, RegAF.High8, RegIX.Low8, 0];
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0xA6: // AND (IX+d)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[4, RegAF.High8, ReadMemory((ushort)(RegIX.Value16 + Displacement)), 0];
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0xA7: // AND A
									RegAF.Value16 = TableALU[4, RegAF.High8, RegAF.High8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xA8: // XOR B
									RegAF.Value16 = TableALU[5, RegAF.High8, RegBC.High8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xA9: // XOR C
									RegAF.Value16 = TableALU[5, RegAF.High8, RegBC.Low8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xAA: // XOR D
									RegAF.Value16 = TableALU[5, RegAF.High8, RegDE.High8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xAB: // XOR E
									RegAF.Value16 = TableALU[5, RegAF.High8, RegDE.Low8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xAC: // XOR IXH
									RegAF.Value16 = TableALU[5, RegAF.High8, RegIX.High8, 0];
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0xAD: // XOR IXL
									RegAF.Value16 = TableALU[5, RegAF.High8, RegIX.Low8, 0];
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0xAE: // XOR (IX+d)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[5, RegAF.High8, ReadMemory((ushort)(RegIX.Value16 + Displacement)), 0];
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0xAF: // XOR A
									RegAF.Value16 = TableALU[5, RegAF.High8, RegAF.High8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xB0: // OR B
									RegAF.Value16 = TableALU[6, RegAF.High8, RegBC.High8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xB1: // OR C
									RegAF.Value16 = TableALU[6, RegAF.High8, RegBC.Low8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xB2: // OR D
									RegAF.Value16 = TableALU[6, RegAF.High8, RegDE.High8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xB3: // OR E
									RegAF.Value16 = TableALU[6, RegAF.High8, RegDE.Low8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xB4: // OR IXH
									RegAF.Value16 = TableALU[6, RegAF.High8, RegIX.High8, 0];
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0xB5: // OR IXL
									RegAF.Value16 = TableALU[6, RegAF.High8, RegIX.Low8, 0];
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0xB6: // OR (IX+d)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[6, RegAF.High8, ReadMemory((ushort)(RegIX.Value16 + Displacement)), 0];
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0xB7: // OR A
									RegAF.Value16 = TableALU[6, RegAF.High8, RegAF.High8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xB8: // CP B
									RegAF.Value16 = TableALU[7, RegAF.High8, RegBC.High8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xB9: // CP C
									RegAF.Value16 = TableALU[7, RegAF.High8, RegBC.Low8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xBA: // CP D
									RegAF.Value16 = TableALU[7, RegAF.High8, RegDE.High8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xBB: // CP E
									RegAF.Value16 = TableALU[7, RegAF.High8, RegDE.Low8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xBC: // CP IXH
									RegAF.Value16 = TableALU[7, RegAF.High8, RegIX.High8, 0];
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0xBD: // CP IXL
									RegAF.Value16 = TableALU[7, RegAF.High8, RegIX.Low8, 0];
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0xBE: // CP (IX+d)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[7, RegAF.High8, ReadMemory((ushort)(RegIX.Value16 + Displacement)), 0];
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0xBF: // CP A
									RegAF.Value16 = TableALU[7, RegAF.High8, RegAF.High8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xC0: // RET NZ
									if (!RegFlagZ) {
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
										this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									} else {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
									}
									break;
								case 0xC1: // POP BC
									RegBC.Low8 = ReadMemory(RegSP.Value16++); RegBC.High8 = ReadMemory(RegSP.Value16++);
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0xC2: // JP NZ, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagZ) {
										RegPC.Value16 = TUS;
									}
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0xC3: // JP nn
									RegPC.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0xC4: // CALL NZ, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagZ) {
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
										this.totalExecutedCycles += 17; this.pendingCycles -= 17;
									} else {
										this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									}
									break;
								case 0xC5: // PUSH BC
									WriteMemory(--RegSP.Value16, RegBC.High8); WriteMemory(--RegSP.Value16, RegBC.Low8);
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									break;
								case 0xC6: // ADD A, n
									RegAF.Value16 = TableALU[0, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									break;
								case 0xC7: // RST $00
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x00;
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									break;
								case 0xC8: // RET Z
									if (RegFlagZ) {
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
										this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									} else {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
									}
									break;
								case 0xC9: // RET
									RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0xCA: // JP Z, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagZ) {
										RegPC.Value16 = TUS;
									}
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0xCB: // (Prefix)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									++RegR;
									switch (ReadMemory(RegPC.Value16++)) {
										case 0x00: // RLC (IX+d)→B
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.High8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x01: // RLC (IX+d)→C
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x02: // RLC (IX+d)→D
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.High8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x03: // RLC (IX+d)→E
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x04: // RLC (IX+d)→H
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.High8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x05: // RLC (IX+d)→L
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x06: // RLC (IX+d)
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x07: // RLC (IX+d)→A
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegAF.High8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x08: // RRC (IX+d)→B
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.High8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x09: // RRC (IX+d)→C
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x0A: // RRC (IX+d)→D
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.High8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x0B: // RRC (IX+d)→E
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x0C: // RRC (IX+d)→H
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.High8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x0D: // RRC (IX+d)→L
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x0E: // RRC (IX+d)
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x0F: // RRC (IX+d)→A
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegAF.High8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x10: // RL (IX+d)→B
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.High8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x11: // RL (IX+d)→C
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x12: // RL (IX+d)→D
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.High8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x13: // RL (IX+d)→E
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x14: // RL (IX+d)→H
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.High8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x15: // RL (IX+d)→L
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x16: // RL (IX+d)
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x17: // RL (IX+d)→A
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegAF.High8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x18: // RR (IX+d)→B
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.High8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x19: // RR (IX+d)→C
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x1A: // RR (IX+d)→D
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.High8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x1B: // RR (IX+d)→E
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x1C: // RR (IX+d)→H
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.High8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x1D: // RR (IX+d)→L
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x1E: // RR (IX+d)
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x1F: // RR (IX+d)→A
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegAF.High8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x20: // SLA (IX+d)→B
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.High8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x21: // SLA (IX+d)→C
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x22: // SLA (IX+d)→D
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.High8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x23: // SLA (IX+d)→E
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x24: // SLA (IX+d)→H
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.High8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x25: // SLA (IX+d)→L
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x26: // SLA (IX+d)
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x27: // SLA (IX+d)→A
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegAF.High8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x28: // SRA (IX+d)→B
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.High8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x29: // SRA (IX+d)→C
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x2A: // SRA (IX+d)→D
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.High8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x2B: // SRA (IX+d)→E
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x2C: // SRA (IX+d)→H
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.High8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x2D: // SRA (IX+d)→L
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x2E: // SRA (IX+d)
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x2F: // SRA (IX+d)→A
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegAF.High8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x30: // SL1 (IX+d)→B
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.High8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x31: // SL1 (IX+d)→C
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x32: // SL1 (IX+d)→D
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.High8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x33: // SL1 (IX+d)→E
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x34: // SL1 (IX+d)→H
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.High8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x35: // SL1 (IX+d)→L
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x36: // SL1 (IX+d)
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x37: // SL1 (IX+d)→A
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegAF.High8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x38: // SRL (IX+d)→B
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.High8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x39: // SRL (IX+d)→C
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x3A: // SRL (IX+d)→D
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.High8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x3B: // SRL (IX+d)→E
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x3C: // SRL (IX+d)→H
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.High8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x3D: // SRL (IX+d)→L
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x3E: // SRL (IX+d)
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x3F: // SRL (IX+d)→A
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegAF.High8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x40: // BIT 0, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x41: // BIT 0, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x42: // BIT 0, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x43: // BIT 0, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x44: // BIT 0, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x45: // BIT 0, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x46: // BIT 0, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x47: // BIT 0, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x48: // BIT 1, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x49: // BIT 1, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x4A: // BIT 1, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x4B: // BIT 1, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x4C: // BIT 1, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x4D: // BIT 1, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x4E: // BIT 1, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x4F: // BIT 1, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x50: // BIT 2, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x51: // BIT 2, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x52: // BIT 2, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x53: // BIT 2, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x54: // BIT 2, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x55: // BIT 2, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x56: // BIT 2, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x57: // BIT 2, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x58: // BIT 3, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x59: // BIT 3, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x5A: // BIT 3, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x5B: // BIT 3, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x5C: // BIT 3, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x5D: // BIT 3, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x5E: // BIT 3, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x5F: // BIT 3, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x60: // BIT 4, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x61: // BIT 4, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x62: // BIT 4, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x63: // BIT 4, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x64: // BIT 4, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x65: // BIT 4, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x66: // BIT 4, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x67: // BIT 4, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x68: // BIT 5, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x69: // BIT 5, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x6A: // BIT 5, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x6B: // BIT 5, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x6C: // BIT 5, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x6D: // BIT 5, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x6E: // BIT 5, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x6F: // BIT 5, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x70: // BIT 6, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x71: // BIT 6, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x72: // BIT 6, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x73: // BIT 6, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x74: // BIT 6, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x75: // BIT 6, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x76: // BIT 6, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x77: // BIT 6, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x78: // BIT 7, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x79: // BIT 7, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x7A: // BIT 7, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x7B: // BIT 7, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x7C: // BIT 7, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x7D: // BIT 7, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x7E: // BIT 7, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x7F: // BIT 7, (IX+d)
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x80: // RES 0, (IX+d)→B
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x81: // RES 0, (IX+d)→C
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x82: // RES 0, (IX+d)→D
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x83: // RES 0, (IX+d)→E
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x84: // RES 0, (IX+d)→H
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x85: // RES 0, (IX+d)→L
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x86: // RES 0, (IX+d)
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x01)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x87: // RES 0, (IX+d)→A
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x88: // RES 1, (IX+d)→B
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x89: // RES 1, (IX+d)→C
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x8A: // RES 1, (IX+d)→D
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x8B: // RES 1, (IX+d)→E
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x8C: // RES 1, (IX+d)→H
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x8D: // RES 1, (IX+d)→L
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x8E: // RES 1, (IX+d)
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x02)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x8F: // RES 1, (IX+d)→A
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x90: // RES 2, (IX+d)→B
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x91: // RES 2, (IX+d)→C
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x92: // RES 2, (IX+d)→D
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x93: // RES 2, (IX+d)→E
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x94: // RES 2, (IX+d)→H
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x95: // RES 2, (IX+d)→L
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x96: // RES 2, (IX+d)
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x04)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x97: // RES 2, (IX+d)→A
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x98: // RES 3, (IX+d)→B
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x99: // RES 3, (IX+d)→C
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x9A: // RES 3, (IX+d)→D
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x9B: // RES 3, (IX+d)→E
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x9C: // RES 3, (IX+d)→H
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x9D: // RES 3, (IX+d)→L
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x9E: // RES 3, (IX+d)
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x08)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x9F: // RES 3, (IX+d)→A
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xA0: // RES 4, (IX+d)→B
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xA1: // RES 4, (IX+d)→C
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xA2: // RES 4, (IX+d)→D
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xA3: // RES 4, (IX+d)→E
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xA4: // RES 4, (IX+d)→H
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xA5: // RES 4, (IX+d)→L
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xA6: // RES 4, (IX+d)
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x10)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xA7: // RES 4, (IX+d)→A
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xA8: // RES 5, (IX+d)→B
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xA9: // RES 5, (IX+d)→C
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xAA: // RES 5, (IX+d)→D
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xAB: // RES 5, (IX+d)→E
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xAC: // RES 5, (IX+d)→H
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xAD: // RES 5, (IX+d)→L
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xAE: // RES 5, (IX+d)
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x20)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xAF: // RES 5, (IX+d)→A
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xB0: // RES 6, (IX+d)→B
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xB1: // RES 6, (IX+d)→C
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xB2: // RES 6, (IX+d)→D
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xB3: // RES 6, (IX+d)→E
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xB4: // RES 6, (IX+d)→H
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xB5: // RES 6, (IX+d)→L
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xB6: // RES 6, (IX+d)
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x40)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xB7: // RES 6, (IX+d)→A
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xB8: // RES 7, (IX+d)→B
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xB9: // RES 7, (IX+d)→C
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xBA: // RES 7, (IX+d)→D
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xBB: // RES 7, (IX+d)→E
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xBC: // RES 7, (IX+d)→H
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xBD: // RES 7, (IX+d)→L
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xBE: // RES 7, (IX+d)
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x80)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xBF: // RES 7, (IX+d)→A
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xC0: // SET 0, (IX+d)→B
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xC1: // SET 0, (IX+d)→C
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xC2: // SET 0, (IX+d)→D
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xC3: // SET 0, (IX+d)→E
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xC4: // SET 0, (IX+d)→H
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xC5: // SET 0, (IX+d)→L
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xC6: // SET 0, (IX+d)
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x01)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xC7: // SET 0, (IX+d)→A
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xC8: // SET 1, (IX+d)→B
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xC9: // SET 1, (IX+d)→C
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xCA: // SET 1, (IX+d)→D
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xCB: // SET 1, (IX+d)→E
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xCC: // SET 1, (IX+d)→H
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xCD: // SET 1, (IX+d)→L
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xCE: // SET 1, (IX+d)
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x02)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xCF: // SET 1, (IX+d)→A
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xD0: // SET 2, (IX+d)→B
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xD1: // SET 2, (IX+d)→C
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xD2: // SET 2, (IX+d)→D
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xD3: // SET 2, (IX+d)→E
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xD4: // SET 2, (IX+d)→H
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xD5: // SET 2, (IX+d)→L
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xD6: // SET 2, (IX+d)
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x04)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xD7: // SET 2, (IX+d)→A
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xD8: // SET 3, (IX+d)→B
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xD9: // SET 3, (IX+d)→C
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xDA: // SET 3, (IX+d)→D
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xDB: // SET 3, (IX+d)→E
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xDC: // SET 3, (IX+d)→H
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xDD: // SET 3, (IX+d)→L
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xDE: // SET 3, (IX+d)
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x08)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xDF: // SET 3, (IX+d)→A
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xE0: // SET 4, (IX+d)→B
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xE1: // SET 4, (IX+d)→C
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xE2: // SET 4, (IX+d)→D
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xE3: // SET 4, (IX+d)→E
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xE4: // SET 4, (IX+d)→H
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xE5: // SET 4, (IX+d)→L
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xE6: // SET 4, (IX+d)
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x10)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xE7: // SET 4, (IX+d)→A
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xE8: // SET 5, (IX+d)→B
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xE9: // SET 5, (IX+d)→C
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xEA: // SET 5, (IX+d)→D
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xEB: // SET 5, (IX+d)→E
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xEC: // SET 5, (IX+d)→H
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xED: // SET 5, (IX+d)→L
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xEE: // SET 5, (IX+d)
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x20)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xEF: // SET 5, (IX+d)→A
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xF0: // SET 6, (IX+d)→B
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xF1: // SET 6, (IX+d)→C
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xF2: // SET 6, (IX+d)→D
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xF3: // SET 6, (IX+d)→E
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xF4: // SET 6, (IX+d)→H
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xF5: // SET 6, (IX+d)→L
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xF6: // SET 6, (IX+d)
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x40)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xF7: // SET 6, (IX+d)→A
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xF8: // SET 7, (IX+d)→B
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xF9: // SET 7, (IX+d)→C
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xFA: // SET 7, (IX+d)→D
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xFB: // SET 7, (IX+d)→E
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xFC: // SET 7, (IX+d)→H
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xFD: // SET 7, (IX+d)→L
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xFE: // SET 7, (IX+d)
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x80)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xFF: // SET 7, (IX+d)→A
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
									}
									break;
								case 0xCC: // CALL Z, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagZ) {
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
										this.totalExecutedCycles += 17; this.pendingCycles -= 17;
									} else {
										this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									}
									break;
								case 0xCD: // CALL nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = TUS;
									this.totalExecutedCycles += 17; this.pendingCycles -= 17;
									break;
								case 0xCE: // ADC A, n
									RegAF.Value16 = TableALU[1, RegAF.High8, ReadMemory(RegPC.Value16++), RegFlagC ? 1 : 0];
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									break;
								case 0xCF: // RST $08
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x08;
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									break;
								case 0xD0: // RET NC
									if (!RegFlagC) {
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
										this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									} else {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
									}
									break;
								case 0xD1: // POP DE
									RegDE.Low8 = ReadMemory(RegSP.Value16++); RegDE.High8 = ReadMemory(RegSP.Value16++);
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0xD2: // JP NC, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagC) {
										RegPC.Value16 = TUS;
									}
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0xD3: // OUT n, A
									WriteHardware(ReadMemory(RegPC.Value16++), RegAF.High8);
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									break;
								case 0xD4: // CALL NC, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagC) {
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
										this.totalExecutedCycles += 17; this.pendingCycles -= 17;
									} else {
										this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									}
									break;
								case 0xD5: // PUSH DE
									WriteMemory(--RegSP.Value16, RegDE.High8); WriteMemory(--RegSP.Value16, RegDE.Low8);
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									break;
								case 0xD6: // SUB n
									RegAF.Value16 = TableALU[2, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									break;
								case 0xD7: // RST $10
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x10;
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									break;
								case 0xD8: // RET C
									if (RegFlagC) {
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
										this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									} else {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
									}
									break;
								case 0xD9: // EXX
									TUS = RegBC.Value16; RegBC.Value16 = RegAltBC.Value16; RegAltBC.Value16 = TUS;
									TUS = RegDE.Value16; RegDE.Value16 = RegAltDE.Value16; RegAltDE.Value16 = TUS;
									TUS = RegHL.Value16; RegHL.Value16 = RegAltHL.Value16; RegAltHL.Value16 = TUS;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xDA: // JP C, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagC) {
										RegPC.Value16 = TUS;
									}
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0xDB: // IN A, n
									RegAF.High8 = ReadHardware((ushort)ReadMemory(RegPC.Value16++));
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									break;
								case 0xDC: // CALL C, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagC) {
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
										this.totalExecutedCycles += 17; this.pendingCycles -= 17;
									} else {
										this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									}
									break;
								case 0xDD: // <-
									// Invalid sequence.
									this.totalExecutedCycles += 1337; this.pendingCycles -= 1337;
									break;
								case 0xDE: // SBC A, n
									RegAF.Value16 = TableALU[3, RegAF.High8, ReadMemory(RegPC.Value16++), RegFlagC ? 1 : 0];
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									break;
								case 0xDF: // RST $18
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x18;
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									break;
								case 0xE0: // RET PO
									if (!RegFlagP) {
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
										this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									} else {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
									}
									break;
								case 0xE1: // POP IX
									RegIX.Low8 = ReadMemory(RegSP.Value16++); RegIX.High8 = ReadMemory(RegSP.Value16++);
									this.totalExecutedCycles += 14; this.pendingCycles -= 14;
									break;
								case 0xE2: // JP PO, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagP) {
										RegPC.Value16 = TUS;
									}
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0xE3: // EX (SP), IX
									TUS = RegSP.Value16; TBL = ReadMemory(TUS++); TBH = ReadMemory(TUS--);
									WriteMemory(TUS++, RegIX.Low8); WriteMemory(TUS, RegIX.High8);
									RegIX.Low8 = TBL; RegIX.High8 = TBH;
									this.totalExecutedCycles += 23; this.pendingCycles -= 23;
									break;
								case 0xE4: // CALL C, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagC) {
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
										this.totalExecutedCycles += 17; this.pendingCycles -= 17;
									} else {
										this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									}
									break;
								case 0xE5: // PUSH IX
									WriteMemory(--RegSP.Value16, RegIX.High8); WriteMemory(--RegSP.Value16, RegIX.Low8);
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0xE6: // AND n
									RegAF.Value16 = TableALU[4, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									break;
								case 0xE7: // RST $20
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x20;
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									break;
								case 0xE8: // RET PE
									if (RegFlagP) {
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
										this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									} else {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
									}
									break;
								case 0xE9: // JP IX
									RegPC.Value16 = RegIX.Value16;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xEA: // JP PE, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagP) {
										RegPC.Value16 = TUS;
									}
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0xEB: // EX DE, HL
									TUS = RegDE.Value16; RegDE.Value16 = RegHL.Value16; RegHL.Value16 = TUS;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xEC: // CALL PE, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagP) {
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
										this.totalExecutedCycles += 17; this.pendingCycles -= 17;
									} else {
										this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									}
									break;
								case 0xED: // (Prefix)
									++RegR;
									switch (ReadMemory(RegPC.Value16++)) {
										case 0x00: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x01: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x02: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x03: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x04: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x05: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x06: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x07: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x08: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x09: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x0A: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x0B: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x0C: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x0D: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x0E: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x0F: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x10: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x11: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x12: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x13: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x14: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x15: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x16: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x17: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x18: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x19: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x1A: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x1B: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x1C: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x1D: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x1E: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x1F: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x20: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x21: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x22: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x23: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x24: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x25: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x26: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x27: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x28: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x29: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x2A: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x2B: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x2C: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x2D: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x2E: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x2F: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x30: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x31: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x32: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x33: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x34: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x35: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x36: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x37: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x38: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x39: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x3A: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x3B: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x3C: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x3D: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x3E: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x3F: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x40: // IN B, C
											RegBC.High8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegBC.High8 > 127;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegBC.High8];
											RegFlagN = false;
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											break;
										case 0x41: // OUT C, B
											WriteHardware(RegBC.Low8, RegBC.High8);
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											break;
										case 0x42: // SBC HL, BC
											TI1 = (short)RegHL.Value16; TI2 = (short)RegBC.Value16; TIR = TI1 - TI2;
											if (RegFlagC) { --TIR; ++TI2; }
											TUS = (ushort)TIR;
											RegFlagH = ((RegHL.Value16 ^ RegBC.Value16 ^ TUS) & 0x1000) != 0;
											RegFlagN = true;
											RegFlagC = (((int)RegHL.Value16 - (int)RegBC.Value16 - (this.RegFlagC ? 1 : 0)) & 0x10000) != 0;
											RegFlagP = TIR > 32767 || TIR < -32768;
											RegFlagS = TUS > 32767;
											RegFlagZ = TUS == 0;
											RegHL.Value16 = TUS;
											RegFlag3 = (TUS & 0x0800) != 0;
											RegFlag5 = (TUS & 0x2000) != 0;
											this.totalExecutedCycles += 15; this.pendingCycles -= 15;
											break;
										case 0x43: // LD (nn), BC
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											WriteMemory(TUS++, RegBC.Low8);
											WriteMemory(TUS, RegBC.High8);
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x44: // NEG
											RegAF.Value16 = TableNeg[RegAF.Value16];
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											break;
										case 0x45: // RETN
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											this.IFF1 = this.IFF2;
											this.totalExecutedCycles += 14; this.pendingCycles -= 14;
											break;
										case 0x46: // IM $0
											interruptMode = 0;
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											break;
										case 0x47: // LD I, A
											RegI = RegAF.High8;
											this.totalExecutedCycles += 9; this.pendingCycles -= 9;
											break;
										case 0x48: // IN C, C
											RegBC.Low8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegBC.Low8 > 127;
											RegFlagZ = RegBC.Low8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegBC.Low8];
											RegFlagN = false;
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											break;
										case 0x49: // OUT C, C
											WriteHardware(RegBC.Low8, RegBC.Low8);
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											break;
										case 0x4A: // ADC HL, BC
											TI1 = (short)RegHL.Value16; TI2 = (short)RegBC.Value16; TIR = TI1 + TI2;
											if (RegFlagC) { ++TIR; ++TI2; }
											TUS = (ushort)TIR;
											RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
											RegFlagN = false;
											RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
											RegFlagP = TIR > 32767 || TIR < -32768;
											RegFlagS = TUS > 32767;
											RegFlagZ = TUS == 0;
											RegHL.Value16 = TUS;
											RegFlag3 = (TUS & 0x0800) != 0;
											RegFlag5 = (TUS & 0x2000) != 0;
											this.totalExecutedCycles += 15; this.pendingCycles -= 15;
											break;
										case 0x4B: // LD BC, (nn)
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											RegBC.Low8 = ReadMemory(TUS++); RegBC.High8 = ReadMemory(TUS);
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x4C: // NEG
											RegAF.Value16 = TableNeg[RegAF.Value16];
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											break;
										case 0x4D: // RETI
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											this.totalExecutedCycles += 14; this.pendingCycles -= 14;
											break;
										case 0x4E: // IM $0
											interruptMode = 0;
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											break;
										case 0x4F: // LD R, A
											RegR = RegAF.High8;
											this.totalExecutedCycles += 9; this.pendingCycles -= 9;
											break;
										case 0x50: // IN D, C
											RegDE.High8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegDE.High8 > 127;
											RegFlagZ = RegDE.High8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegDE.High8];
											RegFlagN = false;
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											break;
										case 0x51: // OUT C, D
											WriteHardware(RegBC.Low8, RegDE.High8);
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											break;
										case 0x52: // SBC HL, DE
											TI1 = (short)RegHL.Value16; TI2 = (short)RegDE.Value16; TIR = TI1 - TI2;
											if (RegFlagC) { --TIR; ++TI2; }
											TUS = (ushort)TIR;
											RegFlagH = ((RegHL.Value16 ^ RegDE.Value16 ^ TUS) & 0x1000) != 0;
											RegFlagN = true;
											RegFlagC = (((int)RegHL.Value16 - (int)RegDE.Value16 - (this.RegFlagC ? 1 : 0)) & 0x10000) != 0;
											RegFlagP = TIR > 32767 || TIR < -32768;
											RegFlagS = TUS > 32767;
											RegFlagZ = TUS == 0;
											RegHL.Value16 = TUS;
											RegFlag3 = (TUS & 0x0800) != 0;
											RegFlag5 = (TUS & 0x2000) != 0;
											this.totalExecutedCycles += 15; this.pendingCycles -= 15;
											break;
										case 0x53: // LD (nn), DE
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											WriteMemory(TUS++, RegDE.Low8);
											WriteMemory(TUS, RegDE.High8);
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x54: // NEG
											RegAF.Value16 = TableNeg[RegAF.Value16];
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											break;
										case 0x55: // RETN
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											this.IFF1 = this.IFF2;
											this.totalExecutedCycles += 14; this.pendingCycles -= 14;
											break;
										case 0x56: // IM $1
											interruptMode = 1;
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											break;
										case 0x57: // LD A, I
											RegAF.High8 = RegI;
											RegFlagS = RegI > 127;
											RegFlagZ = RegI == 0;
											RegFlagH = false;
											RegFlagN = false;
											RegFlagP = this.IFF2;
											this.totalExecutedCycles += 9; this.pendingCycles -= 9;
											break;
										case 0x58: // IN E, C
											RegDE.Low8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegDE.Low8 > 127;
											RegFlagZ = RegDE.Low8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegDE.Low8];
											RegFlagN = false;
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											break;
										case 0x59: // OUT C, E
											WriteHardware(RegBC.Low8, RegDE.Low8);
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											break;
										case 0x5A: // ADC HL, DE
											TI1 = (short)RegHL.Value16; TI2 = (short)RegDE.Value16; TIR = TI1 + TI2;
											if (RegFlagC) { ++TIR; ++TI2; }
											TUS = (ushort)TIR;
											RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
											RegFlagN = false;
											RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
											RegFlagP = TIR > 32767 || TIR < -32768;
											RegFlagS = TUS > 32767;
											RegFlagZ = TUS == 0;
											RegHL.Value16 = TUS;
											RegFlag3 = (TUS & 0x0800) != 0;
											RegFlag5 = (TUS & 0x2000) != 0;
											this.totalExecutedCycles += 15; this.pendingCycles -= 15;
											break;
										case 0x5B: // LD DE, (nn)
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											RegDE.Low8 = ReadMemory(TUS++); RegDE.High8 = ReadMemory(TUS);
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x5C: // NEG
											RegAF.Value16 = TableNeg[RegAF.Value16];
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											break;
										case 0x5D: // RETI
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											this.totalExecutedCycles += 14; this.pendingCycles -= 14;
											break;
										case 0x5E: // IM $2
											interruptMode = 2;
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											break;
										case 0x5F: // LD A, R
											RegAF.High8 = (byte)(RegR & 0x7F);
											RegFlagS = (byte)(RegR & 0x7F) > 127;
											RegFlagZ = (byte)(RegR & 0x7F) == 0;
											RegFlagH = false;
											RegFlagN = false;
											RegFlagP = this.IFF2;
											this.totalExecutedCycles += 9; this.pendingCycles -= 9;
											break;
										case 0x60: // IN H, C
											RegHL.High8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegHL.High8 > 127;
											RegFlagZ = RegHL.High8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegHL.High8];
											RegFlagN = false;
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											break;
										case 0x61: // OUT C, H
											WriteHardware(RegBC.Low8, RegHL.High8);
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											break;
										case 0x62: // SBC HL, HL
											TI1 = (short)RegHL.Value16; TI2 = (short)RegHL.Value16; TIR = TI1 - TI2;
											if (RegFlagC) { --TIR; ++TI2; }
											TUS = (ushort)TIR;
											RegFlagH = ((RegHL.Value16 ^ RegHL.Value16 ^ TUS) & 0x1000) != 0;
											RegFlagN = true;
											RegFlagC = (((int)RegHL.Value16 - (int)RegHL.Value16 - (this.RegFlagC ? 1 : 0)) & 0x10000) != 0;
											RegFlagP = TIR > 32767 || TIR < -32768;
											RegFlagS = TUS > 32767;
											RegFlagZ = TUS == 0;
											RegHL.Value16 = TUS;
											RegFlag3 = (TUS & 0x0800) != 0;
											RegFlag5 = (TUS & 0x2000) != 0;
											this.totalExecutedCycles += 15; this.pendingCycles -= 15;
											break;
										case 0x63: // LD (nn), HL
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											WriteMemory(TUS++, RegHL.Low8);
											WriteMemory(TUS, RegHL.High8);
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											break;
										case 0x64: // NEG
											RegAF.Value16 = TableNeg[RegAF.Value16];
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											break;
										case 0x65: // RETN
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											this.IFF1 = this.IFF2;
											this.totalExecutedCycles += 14; this.pendingCycles -= 14;
											break;
										case 0x66: // IM $0
											interruptMode = 0;
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											break;
										case 0x67: // RRD
											TB1 = RegAF.High8; TB2 = ReadMemory(RegHL.Value16);
											WriteMemory(RegHL.Value16, (byte)((TB2 >> 4) + (TB1 << 4)));
											RegAF.High8 = (byte)((TB1 & 0xF0) + (TB2 & 0x0F));
											RegFlagS = RegAF.High8 > 127;
											RegFlagZ = RegAF.High8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegAF.High8];
											RegFlagN = false;
											RegFlag3 = (RegAF.High8 & 0x08) != 0;
											RegFlag5 = (RegAF.High8 & 0x20) != 0;
											this.totalExecutedCycles += 18; this.pendingCycles -= 18;
											break;
										case 0x68: // IN L, C
											RegHL.Low8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegHL.Low8 > 127;
											RegFlagZ = RegHL.Low8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegHL.Low8];
											RegFlagN = false;
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											break;
										case 0x69: // OUT C, L
											WriteHardware(RegBC.Low8, RegHL.Low8);
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											break;
										case 0x6A: // ADC HL, HL
											TI1 = (short)RegHL.Value16; TI2 = (short)RegHL.Value16; TIR = TI1 + TI2;
											if (RegFlagC) { ++TIR; ++TI2; }
											TUS = (ushort)TIR;
											RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
											RegFlagN = false;
											RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
											RegFlagP = TIR > 32767 || TIR < -32768;
											RegFlagS = TUS > 32767;
											RegFlagZ = TUS == 0;
											RegHL.Value16 = TUS;
											RegFlag3 = (TUS & 0x0800) != 0;
											RegFlag5 = (TUS & 0x2000) != 0;
											this.totalExecutedCycles += 15; this.pendingCycles -= 15;
											break;
										case 0x6B: // LD HL, (nn)
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											RegHL.Low8 = ReadMemory(TUS++); RegHL.High8 = ReadMemory(TUS);
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											break;
										case 0x6C: // NEG
											RegAF.Value16 = TableNeg[RegAF.Value16];
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											break;
										case 0x6D: // RETI
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											this.totalExecutedCycles += 14; this.pendingCycles -= 14;
											break;
										case 0x6E: // IM $0
											interruptMode = 0;
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											break;
										case 0x6F: // RLD
											TB1 = RegAF.High8; TB2 = ReadMemory(RegHL.Value16);
											WriteMemory(RegHL.Value16, (byte)((TB1 & 0x0F) + (TB2 << 4)));
											RegAF.High8 = (byte)((TB1 & 0xF0) + (TB2 >> 4));
											RegFlagS = RegAF.High8 > 127;
											RegFlagZ = RegAF.High8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegAF.High8];
											RegFlagN = false;
											RegFlag3 = (RegAF.High8 & 0x08) != 0;
											RegFlag5 = (RegAF.High8 & 0x20) != 0;
											this.totalExecutedCycles += 18; this.pendingCycles -= 18;
											break;
										case 0x70: // IN 0, C
											TB = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = TB > 127;
											RegFlagZ = TB == 0;
											RegFlagH = false;
											RegFlagP = TableParity[TB];
											RegFlagN = false;
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											break;
										case 0x71: // OUT C, 0
											WriteHardware(RegBC.Low8, 0);
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											break;
										case 0x72: // SBC HL, SP
											TI1 = (short)RegHL.Value16; TI2 = (short)RegSP.Value16; TIR = TI1 - TI2;
											if (RegFlagC) { --TIR; ++TI2; }
											TUS = (ushort)TIR;
											RegFlagH = ((RegHL.Value16 ^ RegSP.Value16 ^ TUS) & 0x1000) != 0;
											RegFlagN = true;
											RegFlagC = (((int)RegHL.Value16 - (int)RegSP.Value16 - (this.RegFlagC ? 1 : 0)) & 0x10000) != 0;
											RegFlagP = TIR > 32767 || TIR < -32768;
											RegFlagS = TUS > 32767;
											RegFlagZ = TUS == 0;
											RegHL.Value16 = TUS;
											RegFlag3 = (TUS & 0x0800) != 0;
											RegFlag5 = (TUS & 0x2000) != 0;
											this.totalExecutedCycles += 15; this.pendingCycles -= 15;
											break;
										case 0x73: // LD (nn), SP
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											WriteMemory(TUS++, RegSP.Low8);
											WriteMemory(TUS, RegSP.High8);
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x74: // NEG
											RegAF.Value16 = TableNeg[RegAF.Value16];
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											break;
										case 0x75: // RETN
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											this.IFF1 = this.IFF2;
											this.totalExecutedCycles += 14; this.pendingCycles -= 14;
											break;
										case 0x76: // IM $1
											interruptMode = 1;
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											break;
										case 0x77: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x78: // IN A, C
											RegAF.High8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegAF.High8 > 127;
											RegFlagZ = RegAF.High8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegAF.High8];
											RegFlagN = false;
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											break;
										case 0x79: // OUT C, A
											WriteHardware(RegBC.Low8, RegAF.High8);
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											break;
										case 0x7A: // ADC HL, SP
											TI1 = (short)RegHL.Value16; TI2 = (short)RegSP.Value16; TIR = TI1 + TI2;
											if (RegFlagC) { ++TIR; ++TI2; }
											TUS = (ushort)TIR;
											RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
											RegFlagN = false;
											RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
											RegFlagP = TIR > 32767 || TIR < -32768;
											RegFlagS = TUS > 32767;
											RegFlagZ = TUS == 0;
											RegHL.Value16 = TUS;
											RegFlag3 = (TUS & 0x0800) != 0;
											RegFlag5 = (TUS & 0x2000) != 0;
											this.totalExecutedCycles += 15; this.pendingCycles -= 15;
											break;
										case 0x7B: // LD SP, (nn)
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											RegSP.Low8 = ReadMemory(TUS++); RegSP.High8 = ReadMemory(TUS);
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x7C: // NEG
											RegAF.Value16 = TableNeg[RegAF.Value16];
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											break;
										case 0x7D: // RETI
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											this.totalExecutedCycles += 14; this.pendingCycles -= 14;
											break;
										case 0x7E: // IM $2
											interruptMode = 2;
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											break;
										case 0x7F: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x80: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x81: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x82: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x83: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x84: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x85: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x86: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x87: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x88: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x89: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x8A: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x8B: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x8C: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x8D: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x8E: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x8F: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x90: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x91: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x92: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x93: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x94: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x95: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x96: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x97: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x98: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x99: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x9A: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x9B: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x9C: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x9D: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x9E: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x9F: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xA0: // LDI
											WriteMemory(RegDE.Value16++, TB1 = ReadMemory(RegHL.Value16++));
											TB1 += RegAF.High8; RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											RegFlagH = false;
											RegFlagN = false;
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											break;
										case 0xA1: // CPI
											TB1 = ReadMemory(RegHL.Value16++); TB2 = (byte)(RegAF.High8 - TB1);
											RegFlagN = true;
											RegFlagH = TableHalfBorrow[RegAF.High8, TB1];
											RegFlagZ = TB2 == 0;
											RegFlagS = TB2 > 127;
											TB1 = (byte)(RegAF.High8 - TB1 - (RegFlagH ? 1 : 0)); RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											break;
										case 0xA2: // INI
											WriteMemory(RegHL.Value16++, ReadHardware(RegBC.Value16));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											break;
										case 0xA3: // OUTI
											WriteHardware(RegBC.Value16, ReadMemory(RegHL.Value16++));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											break;
										case 0xA4: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xA5: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xA6: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xA7: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xA8: // LDD
											WriteMemory(RegDE.Value16--, TB1 = ReadMemory(RegHL.Value16--));
											TB1 += RegAF.High8; RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											RegFlagH = false;
											RegFlagN = false;
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											break;
										case 0xA9: // CPD
											TB1 = ReadMemory(RegHL.Value16--); TB2 = (byte)(RegAF.High8 - TB1);
											RegFlagN = true;
											RegFlagH = TableHalfBorrow[RegAF.High8, TB1];
											RegFlagZ = TB2 == 0;
											RegFlagS = TB2 > 127;
											TB1 = (byte)(RegAF.High8 - TB1 - (RegFlagH ? 1 : 0)); RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											break;
										case 0xAA: // IND
											WriteMemory(RegHL.Value16--, ReadHardware(RegBC.Value16));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											break;
										case 0xAB: // OUTD
											WriteHardware(RegBC.Value16, ReadMemory(RegHL.Value16--));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											break;
										case 0xAC: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xAD: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xAE: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xAF: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xB0: // LDIR
											WriteMemory(RegDE.Value16++, TB1 = ReadMemory(RegHL.Value16++));
											TB1 += RegAF.High8; RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											RegFlagH = false;
											RegFlagN = false;
											if (RegBC.Value16 != 0) {
												RegPC.Value16 -= 2;
												this.totalExecutedCycles += 21; this.pendingCycles -= 21;
											} else {
												this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											}
											break;
										case 0xB1: // CPIR
											TB1 = ReadMemory(RegHL.Value16++); TB2 = (byte)(RegAF.High8 - TB1);
											RegFlagN = true;
											RegFlagH = TableHalfBorrow[RegAF.High8, TB1];
											RegFlagZ = TB2 == 0;
											RegFlagS = TB2 > 127;
											TB1 = (byte)(RegAF.High8 - TB1 - (RegFlagH ? 1 : 0)); RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											if (RegBC.Value16 != 0 && !RegFlagZ) {
												RegPC.Value16 -= 2;
												this.totalExecutedCycles += 21; this.pendingCycles -= 21;
											} else {
												this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											}
											break;
										case 0xB2: // INIR
											WriteMemory(RegHL.Value16++, ReadHardware(RegBC.Value16));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											if (RegBC.High8 != 0) {
												RegPC.Value16 -= 2;
												this.totalExecutedCycles += 21; this.pendingCycles -= 21;
											} else {
												this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											}
											break;
										case 0xB3: // OTIR
											WriteHardware(RegBC.Value16, ReadMemory(RegHL.Value16++));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											if (RegBC.High8 != 0) {
												RegPC.Value16 -= 2;
												this.totalExecutedCycles += 21; this.pendingCycles -= 21;
											} else {
												this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											}
											break;
										case 0xB4: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xB5: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xB6: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xB7: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xB8: // LDDR
											WriteMemory(RegDE.Value16--, TB1 = ReadMemory(RegHL.Value16--));
											TB1 += RegAF.High8; RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											RegFlagH = false;
											RegFlagN = false;
											if (RegBC.Value16 != 0) {
												RegPC.Value16 -= 2;
												this.totalExecutedCycles += 21; this.pendingCycles -= 21;
											} else {
												this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											}
											break;
										case 0xB9: // CPDR
											TB1 = ReadMemory(RegHL.Value16--); TB2 = (byte)(RegAF.High8 - TB1);
											RegFlagN = true;
											RegFlagH = TableHalfBorrow[RegAF.High8, TB1];
											RegFlagZ = TB2 == 0;
											RegFlagS = TB2 > 127;
											TB1 = (byte)(RegAF.High8 - TB1 - (RegFlagH ? 1 : 0)); RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											if (RegBC.Value16 != 0 && !RegFlagZ) {
												RegPC.Value16 -= 2;
												this.totalExecutedCycles += 21; this.pendingCycles -= 21;
											} else {
												this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											}
											break;
										case 0xBA: // INDR
											WriteMemory(RegHL.Value16--, ReadHardware(RegBC.Value16));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											if (RegBC.High8 != 0) {
												RegPC.Value16 -= 2;
												this.totalExecutedCycles += 21; this.pendingCycles -= 21;
											} else {
												this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											}
											break;
										case 0xBB: // OTDR
											WriteHardware(RegBC.Value16, ReadMemory(RegHL.Value16--));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											if (RegBC.High8 != 0) {
												RegPC.Value16 -= 2;
												this.totalExecutedCycles += 21; this.pendingCycles -= 21;
											} else {
												this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											}
											break;
										case 0xBC: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xBD: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xBE: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xBF: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xC0: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xC1: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xC2: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xC3: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xC4: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xC5: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xC6: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xC7: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xC8: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xC9: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xCA: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xCB: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xCC: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xCD: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xCE: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xCF: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xD0: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xD1: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xD2: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xD3: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xD4: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xD5: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xD6: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xD7: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xD8: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xD9: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xDA: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xDB: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xDC: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xDD: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xDE: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xDF: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xE0: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xE1: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xE2: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xE3: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xE4: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xE5: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xE6: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xE7: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xE8: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xE9: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xEA: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xEB: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xEC: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xED: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xEE: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xEF: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xF0: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xF1: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xF2: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xF3: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xF4: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xF5: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xF6: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xF7: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xF8: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xF9: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xFA: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xFB: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xFC: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xFD: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xFE: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xFF: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
									}
									break;
								case 0xEE: // XOR n
									RegAF.Value16 = TableALU[5, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									break;
								case 0xEF: // RST $28
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x28;
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									break;
								case 0xF0: // RET P
									if (!RegFlagS) {
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
										this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									} else {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
									}
									break;
								case 0xF1: // POP AF
									RegAF.Low8 = ReadMemory(RegSP.Value16++); RegAF.High8 = ReadMemory(RegSP.Value16++);
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0xF2: // JP P, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagS) {
										RegPC.Value16 = TUS;
									}
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0xF3: // DI
									this.IFF1 = this.IFF2 = false;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xF4: // CALL P, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagS) {
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
										this.totalExecutedCycles += 17; this.pendingCycles -= 17;
									} else {
										this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									}
									break;
								case 0xF5: // PUSH AF
									WriteMemory(--RegSP.Value16, RegAF.High8); WriteMemory(--RegSP.Value16, RegAF.Low8);
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									break;
								case 0xF6: // OR n
									RegAF.Value16 = TableALU[6, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									break;
								case 0xF7: // RST $30
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x30;
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									break;
								case 0xF8: // RET M
									if (RegFlagS) {
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
										this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									} else {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
									}
									break;
								case 0xF9: // LD SP, IX
									RegSP.Value16 = RegIX.Value16;
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0xFA: // JP M, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagS) {
										RegPC.Value16 = TUS;
									}
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0xFB: // EI
									this.IFF1 = this.IFF2 = true;
									Interruptable = false;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xFC: // CALL M, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagS) {
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
										this.totalExecutedCycles += 17; this.pendingCycles -= 17;
									} else {
										this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									}
									break;
								case 0xFD: // <-
									// Invalid sequence.
									this.totalExecutedCycles += 1337; this.pendingCycles -= 1337;
									break;
								case 0xFE: // CP n
									RegAF.Value16 = TableALU[7, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									break;
								case 0xFF: // RST $38
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x38;
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									break;
							}
							break;
						case 0xDE: // SBC A, n
							RegAF.Value16 = TableALU[3, RegAF.High8, ReadMemory(RegPC.Value16++), RegFlagC ? 1 : 0];
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0xDF: // RST $18
							WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
							RegPC.Value16 = 0x18;
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							break;
						case 0xE0: // RET PO
							if (!RegFlagP) {
								RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
								this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							} else {
								this.totalExecutedCycles += 5; this.pendingCycles -= 5;
							}
							break;
						case 0xE1: // POP HL
							RegHL.Low8 = ReadMemory(RegSP.Value16++); RegHL.High8 = ReadMemory(RegSP.Value16++);
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							break;
						case 0xE2: // JP PO, nn
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (!RegFlagP) {
								RegPC.Value16 = TUS;
							}
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							break;
						case 0xE3: // EX (SP), HL
							TUS = RegSP.Value16; TBL = ReadMemory(TUS++); TBH = ReadMemory(TUS--);
							WriteMemory(TUS++, RegHL.Low8); WriteMemory(TUS, RegHL.High8);
							RegHL.Low8 = TBL; RegHL.High8 = TBH;
							this.totalExecutedCycles += 19; this.pendingCycles -= 19;
							break;
						case 0xE4: // CALL C, nn
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (RegFlagC) {
								WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
								RegPC.Value16 = TUS;
								this.totalExecutedCycles += 17; this.pendingCycles -= 17;
							} else {
								this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							}
							break;
						case 0xE5: // PUSH HL
							WriteMemory(--RegSP.Value16, RegHL.High8); WriteMemory(--RegSP.Value16, RegHL.Low8);
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							break;
						case 0xE6: // AND n
							RegAF.Value16 = TableALU[4, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0xE7: // RST $20
							WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
							RegPC.Value16 = 0x20;
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							break;
						case 0xE8: // RET PE
							if (RegFlagP) {
								RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
								this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							} else {
								this.totalExecutedCycles += 5; this.pendingCycles -= 5;
							}
							break;
						case 0xE9: // JP HL
							RegPC.Value16 = RegHL.Value16;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0xEA: // JP PE, nn
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (RegFlagP) {
								RegPC.Value16 = TUS;
							}
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							break;
						case 0xEB: // EX DE, HL
							TUS = RegDE.Value16; RegDE.Value16 = RegHL.Value16; RegHL.Value16 = TUS;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0xEC: // CALL PE, nn
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (RegFlagP) {
								WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
								RegPC.Value16 = TUS;
								this.totalExecutedCycles += 17; this.pendingCycles -= 17;
							} else {
								this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							}
							break;
						case 0xED: // (Prefix)
							++RegR;
							switch (ReadMemory(RegPC.Value16++)) {
								case 0x00: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x01: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x02: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x03: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x04: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x05: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x06: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x07: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x08: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x09: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x0A: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x0B: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x0C: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x0D: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x0E: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x0F: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x10: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x11: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x12: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x13: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x14: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x15: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x16: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x17: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x18: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x19: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x1A: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x1B: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x1C: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x1D: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x1E: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x1F: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x20: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x21: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x22: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x23: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x24: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x25: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x26: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x27: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x28: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x29: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x2A: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x2B: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x2C: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x2D: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x2E: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x2F: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x30: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x31: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x32: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x33: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x34: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x35: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x36: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x37: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x38: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x39: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x3A: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x3B: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x3C: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x3D: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x3E: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x3F: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x40: // IN B, C
									RegBC.High8 = ReadHardware((ushort)RegBC.Low8);
									RegFlagS = RegBC.High8 > 127;
									RegFlagZ = RegBC.High8 == 0;
									RegFlagH = false;
									RegFlagP = TableParity[RegBC.High8];
									RegFlagN = false;
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									break;
								case 0x41: // OUT C, B
									WriteHardware(RegBC.Low8, RegBC.High8);
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									break;
								case 0x42: // SBC HL, BC
									TI1 = (short)RegHL.Value16; TI2 = (short)RegBC.Value16; TIR = TI1 - TI2;
									if (RegFlagC) { --TIR; ++TI2; }
									TUS = (ushort)TIR;
									RegFlagH = ((RegHL.Value16 ^ RegBC.Value16 ^ TUS) & 0x1000) != 0;
									RegFlagN = true;
									RegFlagC = (((int)RegHL.Value16 - (int)RegBC.Value16 - (this.RegFlagC ? 1 : 0)) & 0x10000) != 0;
									RegFlagP = TIR > 32767 || TIR < -32768;
									RegFlagS = TUS > 32767;
									RegFlagZ = TUS == 0;
									RegHL.Value16 = TUS;
									RegFlag3 = (TUS & 0x0800) != 0;
									RegFlag5 = (TUS & 0x2000) != 0;
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0x43: // LD (nn), BC
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									WriteMemory(TUS++, RegBC.Low8);
									WriteMemory(TUS, RegBC.High8);
									this.totalExecutedCycles += 20; this.pendingCycles -= 20;
									break;
								case 0x44: // NEG
									RegAF.Value16 = TableNeg[RegAF.Value16];
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x45: // RETN
									RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									this.IFF1 = this.IFF2;
									this.totalExecutedCycles += 14; this.pendingCycles -= 14;
									break;
								case 0x46: // IM $0
									interruptMode = 0;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x47: // LD I, A
									RegI = RegAF.High8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x48: // IN C, C
									RegBC.Low8 = ReadHardware((ushort)RegBC.Low8);
									RegFlagS = RegBC.Low8 > 127;
									RegFlagZ = RegBC.Low8 == 0;
									RegFlagH = false;
									RegFlagP = TableParity[RegBC.Low8];
									RegFlagN = false;
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									break;
								case 0x49: // OUT C, C
									WriteHardware(RegBC.Low8, RegBC.Low8);
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									break;
								case 0x4A: // ADC HL, BC
									TI1 = (short)RegHL.Value16; TI2 = (short)RegBC.Value16; TIR = TI1 + TI2;
									if (RegFlagC) { ++TIR; ++TI2; }
									TUS = (ushort)TIR;
									RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
									RegFlagN = false;
									RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
									RegFlagP = TIR > 32767 || TIR < -32768;
									RegFlagS = TUS > 32767;
									RegFlagZ = TUS == 0;
									RegHL.Value16 = TUS;
									RegFlag3 = (TUS & 0x0800) != 0;
									RegFlag5 = (TUS & 0x2000) != 0;
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0x4B: // LD BC, (nn)
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									RegBC.Low8 = ReadMemory(TUS++); RegBC.High8 = ReadMemory(TUS);
									this.totalExecutedCycles += 20; this.pendingCycles -= 20;
									break;
								case 0x4C: // NEG
									RegAF.Value16 = TableNeg[RegAF.Value16];
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x4D: // RETI
									RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									this.totalExecutedCycles += 14; this.pendingCycles -= 14;
									break;
								case 0x4E: // IM $0
									interruptMode = 0;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x4F: // LD R, A
									RegR = RegAF.High8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x50: // IN D, C
									RegDE.High8 = ReadHardware((ushort)RegBC.Low8);
									RegFlagS = RegDE.High8 > 127;
									RegFlagZ = RegDE.High8 == 0;
									RegFlagH = false;
									RegFlagP = TableParity[RegDE.High8];
									RegFlagN = false;
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									break;
								case 0x51: // OUT C, D
									WriteHardware(RegBC.Low8, RegDE.High8);
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									break;
								case 0x52: // SBC HL, DE
									TI1 = (short)RegHL.Value16; TI2 = (short)RegDE.Value16; TIR = TI1 - TI2;
									if (RegFlagC) { --TIR; ++TI2; }
									TUS = (ushort)TIR;
									RegFlagH = ((RegHL.Value16 ^ RegDE.Value16 ^ TUS) & 0x1000) != 0;
									RegFlagN = true;
									RegFlagC = (((int)RegHL.Value16 - (int)RegDE.Value16 - (this.RegFlagC ? 1 : 0)) & 0x10000) != 0;
									RegFlagP = TIR > 32767 || TIR < -32768;
									RegFlagS = TUS > 32767;
									RegFlagZ = TUS == 0;
									RegHL.Value16 = TUS;
									RegFlag3 = (TUS & 0x0800) != 0;
									RegFlag5 = (TUS & 0x2000) != 0;
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0x53: // LD (nn), DE
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									WriteMemory(TUS++, RegDE.Low8);
									WriteMemory(TUS, RegDE.High8);
									this.totalExecutedCycles += 20; this.pendingCycles -= 20;
									break;
								case 0x54: // NEG
									RegAF.Value16 = TableNeg[RegAF.Value16];
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x55: // RETN
									RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									this.IFF1 = this.IFF2;
									this.totalExecutedCycles += 14; this.pendingCycles -= 14;
									break;
								case 0x56: // IM $1
									interruptMode = 1;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x57: // LD A, I
									RegAF.High8 = RegI;
									RegFlagS = RegI > 127;
									RegFlagZ = RegI == 0;
									RegFlagH = false;
									RegFlagN = false;
									RegFlagP = this.IFF2;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x58: // IN E, C
									RegDE.Low8 = ReadHardware((ushort)RegBC.Low8);
									RegFlagS = RegDE.Low8 > 127;
									RegFlagZ = RegDE.Low8 == 0;
									RegFlagH = false;
									RegFlagP = TableParity[RegDE.Low8];
									RegFlagN = false;
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									break;
								case 0x59: // OUT C, E
									WriteHardware(RegBC.Low8, RegDE.Low8);
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									break;
								case 0x5A: // ADC HL, DE
									TI1 = (short)RegHL.Value16; TI2 = (short)RegDE.Value16; TIR = TI1 + TI2;
									if (RegFlagC) { ++TIR; ++TI2; }
									TUS = (ushort)TIR;
									RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
									RegFlagN = false;
									RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
									RegFlagP = TIR > 32767 || TIR < -32768;
									RegFlagS = TUS > 32767;
									RegFlagZ = TUS == 0;
									RegHL.Value16 = TUS;
									RegFlag3 = (TUS & 0x0800) != 0;
									RegFlag5 = (TUS & 0x2000) != 0;
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0x5B: // LD DE, (nn)
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									RegDE.Low8 = ReadMemory(TUS++); RegDE.High8 = ReadMemory(TUS);
									this.totalExecutedCycles += 20; this.pendingCycles -= 20;
									break;
								case 0x5C: // NEG
									RegAF.Value16 = TableNeg[RegAF.Value16];
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x5D: // RETI
									RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									this.totalExecutedCycles += 14; this.pendingCycles -= 14;
									break;
								case 0x5E: // IM $2
									interruptMode = 2;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x5F: // LD A, R
									RegAF.High8 = (byte)(RegR & 0x7F);
									RegFlagS = (byte)(RegR & 0x7F) > 127;
									RegFlagZ = (byte)(RegR & 0x7F) == 0;
									RegFlagH = false;
									RegFlagN = false;
									RegFlagP = this.IFF2;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x60: // IN H, C
									RegHL.High8 = ReadHardware((ushort)RegBC.Low8);
									RegFlagS = RegHL.High8 > 127;
									RegFlagZ = RegHL.High8 == 0;
									RegFlagH = false;
									RegFlagP = TableParity[RegHL.High8];
									RegFlagN = false;
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									break;
								case 0x61: // OUT C, H
									WriteHardware(RegBC.Low8, RegHL.High8);
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									break;
								case 0x62: // SBC HL, HL
									TI1 = (short)RegHL.Value16; TI2 = (short)RegHL.Value16; TIR = TI1 - TI2;
									if (RegFlagC) { --TIR; ++TI2; }
									TUS = (ushort)TIR;
									RegFlagH = ((RegHL.Value16 ^ RegHL.Value16 ^ TUS) & 0x1000) != 0;
									RegFlagN = true;
									RegFlagC = (((int)RegHL.Value16 - (int)RegHL.Value16 - (this.RegFlagC ? 1 : 0)) & 0x10000) != 0;
									RegFlagP = TIR > 32767 || TIR < -32768;
									RegFlagS = TUS > 32767;
									RegFlagZ = TUS == 0;
									RegHL.Value16 = TUS;
									RegFlag3 = (TUS & 0x0800) != 0;
									RegFlag5 = (TUS & 0x2000) != 0;
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0x63: // LD (nn), HL
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									WriteMemory(TUS++, RegHL.Low8);
									WriteMemory(TUS, RegHL.High8);
									this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									break;
								case 0x64: // NEG
									RegAF.Value16 = TableNeg[RegAF.Value16];
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x65: // RETN
									RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									this.IFF1 = this.IFF2;
									this.totalExecutedCycles += 14; this.pendingCycles -= 14;
									break;
								case 0x66: // IM $0
									interruptMode = 0;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x67: // RRD
									TB1 = RegAF.High8; TB2 = ReadMemory(RegHL.Value16);
									WriteMemory(RegHL.Value16, (byte)((TB2 >> 4) + (TB1 << 4)));
									RegAF.High8 = (byte)((TB1 & 0xF0) + (TB2 & 0x0F));
									RegFlagS = RegAF.High8 > 127;
									RegFlagZ = RegAF.High8 == 0;
									RegFlagH = false;
									RegFlagP = TableParity[RegAF.High8];
									RegFlagN = false;
									RegFlag3 = (RegAF.High8 & 0x08) != 0;
									RegFlag5 = (RegAF.High8 & 0x20) != 0;
									this.totalExecutedCycles += 18; this.pendingCycles -= 18;
									break;
								case 0x68: // IN L, C
									RegHL.Low8 = ReadHardware((ushort)RegBC.Low8);
									RegFlagS = RegHL.Low8 > 127;
									RegFlagZ = RegHL.Low8 == 0;
									RegFlagH = false;
									RegFlagP = TableParity[RegHL.Low8];
									RegFlagN = false;
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									break;
								case 0x69: // OUT C, L
									WriteHardware(RegBC.Low8, RegHL.Low8);
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									break;
								case 0x6A: // ADC HL, HL
									TI1 = (short)RegHL.Value16; TI2 = (short)RegHL.Value16; TIR = TI1 + TI2;
									if (RegFlagC) { ++TIR; ++TI2; }
									TUS = (ushort)TIR;
									RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
									RegFlagN = false;
									RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
									RegFlagP = TIR > 32767 || TIR < -32768;
									RegFlagS = TUS > 32767;
									RegFlagZ = TUS == 0;
									RegHL.Value16 = TUS;
									RegFlag3 = (TUS & 0x0800) != 0;
									RegFlag5 = (TUS & 0x2000) != 0;
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0x6B: // LD HL, (nn)
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									RegHL.Low8 = ReadMemory(TUS++); RegHL.High8 = ReadMemory(TUS);
									this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									break;
								case 0x6C: // NEG
									RegAF.Value16 = TableNeg[RegAF.Value16];
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x6D: // RETI
									RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									this.totalExecutedCycles += 14; this.pendingCycles -= 14;
									break;
								case 0x6E: // IM $0
									interruptMode = 0;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x6F: // RLD
									TB1 = RegAF.High8; TB2 = ReadMemory(RegHL.Value16);
									WriteMemory(RegHL.Value16, (byte)((TB1 & 0x0F) + (TB2 << 4)));
									RegAF.High8 = (byte)((TB1 & 0xF0) + (TB2 >> 4));
									RegFlagS = RegAF.High8 > 127;
									RegFlagZ = RegAF.High8 == 0;
									RegFlagH = false;
									RegFlagP = TableParity[RegAF.High8];
									RegFlagN = false;
									RegFlag3 = (RegAF.High8 & 0x08) != 0;
									RegFlag5 = (RegAF.High8 & 0x20) != 0;
									this.totalExecutedCycles += 18; this.pendingCycles -= 18;
									break;
								case 0x70: // IN 0, C
									TB = ReadHardware((ushort)RegBC.Low8);
									RegFlagS = TB > 127;
									RegFlagZ = TB == 0;
									RegFlagH = false;
									RegFlagP = TableParity[TB];
									RegFlagN = false;
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									break;
								case 0x71: // OUT C, 0
									WriteHardware(RegBC.Low8, 0);
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									break;
								case 0x72: // SBC HL, SP
									TI1 = (short)RegHL.Value16; TI2 = (short)RegSP.Value16; TIR = TI1 - TI2;
									if (RegFlagC) { --TIR; ++TI2; }
									TUS = (ushort)TIR;
									RegFlagH = ((RegHL.Value16 ^ RegSP.Value16 ^ TUS) & 0x1000) != 0;
									RegFlagN = true;
									RegFlagC = (((int)RegHL.Value16 - (int)RegSP.Value16 - (this.RegFlagC ? 1 : 0)) & 0x10000) != 0;
									RegFlagP = TIR > 32767 || TIR < -32768;
									RegFlagS = TUS > 32767;
									RegFlagZ = TUS == 0;
									RegHL.Value16 = TUS;
									RegFlag3 = (TUS & 0x0800) != 0;
									RegFlag5 = (TUS & 0x2000) != 0;
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0x73: // LD (nn), SP
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									WriteMemory(TUS++, RegSP.Low8);
									WriteMemory(TUS, RegSP.High8);
									this.totalExecutedCycles += 20; this.pendingCycles -= 20;
									break;
								case 0x74: // NEG
									RegAF.Value16 = TableNeg[RegAF.Value16];
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x75: // RETN
									RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									this.IFF1 = this.IFF2;
									this.totalExecutedCycles += 14; this.pendingCycles -= 14;
									break;
								case 0x76: // IM $1
									interruptMode = 1;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x77: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x78: // IN A, C
									RegAF.High8 = ReadHardware((ushort)RegBC.Low8);
									RegFlagS = RegAF.High8 > 127;
									RegFlagZ = RegAF.High8 == 0;
									RegFlagH = false;
									RegFlagP = TableParity[RegAF.High8];
									RegFlagN = false;
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									break;
								case 0x79: // OUT C, A
									WriteHardware(RegBC.Low8, RegAF.High8);
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									break;
								case 0x7A: // ADC HL, SP
									TI1 = (short)RegHL.Value16; TI2 = (short)RegSP.Value16; TIR = TI1 + TI2;
									if (RegFlagC) { ++TIR; ++TI2; }
									TUS = (ushort)TIR;
									RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
									RegFlagN = false;
									RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
									RegFlagP = TIR > 32767 || TIR < -32768;
									RegFlagS = TUS > 32767;
									RegFlagZ = TUS == 0;
									RegHL.Value16 = TUS;
									RegFlag3 = (TUS & 0x0800) != 0;
									RegFlag5 = (TUS & 0x2000) != 0;
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0x7B: // LD SP, (nn)
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									RegSP.Low8 = ReadMemory(TUS++); RegSP.High8 = ReadMemory(TUS);
									this.totalExecutedCycles += 20; this.pendingCycles -= 20;
									break;
								case 0x7C: // NEG
									RegAF.Value16 = TableNeg[RegAF.Value16];
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x7D: // RETI
									RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									this.totalExecutedCycles += 14; this.pendingCycles -= 14;
									break;
								case 0x7E: // IM $2
									interruptMode = 2;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0x7F: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x80: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x81: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x82: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x83: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x84: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x85: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x86: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x87: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x88: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x89: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x8A: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x8B: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x8C: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x8D: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x8E: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x8F: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x90: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x91: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x92: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x93: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x94: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x95: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x96: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x97: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x98: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x99: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x9A: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x9B: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x9C: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x9D: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x9E: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x9F: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xA0: // LDI
									WriteMemory(RegDE.Value16++, TB1 = ReadMemory(RegHL.Value16++));
									TB1 += RegAF.High8; RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
									--RegBC.Value16;
									RegFlagP = RegBC.Value16 != 0;
									RegFlagH = false;
									RegFlagN = false;
									this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									break;
								case 0xA1: // CPI
									TB1 = ReadMemory(RegHL.Value16++); TB2 = (byte)(RegAF.High8 - TB1);
									RegFlagN = true;
									RegFlagH = TableHalfBorrow[RegAF.High8, TB1];
									RegFlagZ = TB2 == 0;
									RegFlagS = TB2 > 127;
									TB1 = (byte)(RegAF.High8 - TB1 - (RegFlagH ? 1 : 0)); RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
									--RegBC.Value16;
									RegFlagP = RegBC.Value16 != 0;
									this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									break;
								case 0xA2: // INI
									WriteMemory(RegHL.Value16++, ReadHardware(RegBC.Value16));
									--RegBC.High8;
									RegFlagZ = RegBC.High8 == 0;
									RegFlagN = true;
									this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									break;
								case 0xA3: // OUTI
									WriteHardware(RegBC.Value16, ReadMemory(RegHL.Value16++));
									--RegBC.High8;
									RegFlagZ = RegBC.High8 == 0;
									RegFlagN = true;
									this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									break;
								case 0xA4: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xA5: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xA6: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xA7: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xA8: // LDD
									WriteMemory(RegDE.Value16--, TB1 = ReadMemory(RegHL.Value16--));
									TB1 += RegAF.High8; RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
									--RegBC.Value16;
									RegFlagP = RegBC.Value16 != 0;
									RegFlagH = false;
									RegFlagN = false;
									this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									break;
								case 0xA9: // CPD
									TB1 = ReadMemory(RegHL.Value16--); TB2 = (byte)(RegAF.High8 - TB1);
									RegFlagN = true;
									RegFlagH = TableHalfBorrow[RegAF.High8, TB1];
									RegFlagZ = TB2 == 0;
									RegFlagS = TB2 > 127;
									TB1 = (byte)(RegAF.High8 - TB1 - (RegFlagH ? 1 : 0)); RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
									--RegBC.Value16;
									RegFlagP = RegBC.Value16 != 0;
									this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									break;
								case 0xAA: // IND
									WriteMemory(RegHL.Value16--, ReadHardware(RegBC.Value16));
									--RegBC.High8;
									RegFlagZ = RegBC.High8 == 0;
									RegFlagN = true;
									this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									break;
								case 0xAB: // OUTD
									WriteHardware(RegBC.Value16, ReadMemory(RegHL.Value16--));
									--RegBC.High8;
									RegFlagZ = RegBC.High8 == 0;
									RegFlagN = true;
									this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									break;
								case 0xAC: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xAD: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xAE: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xAF: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xB0: // LDIR
									WriteMemory(RegDE.Value16++, TB1 = ReadMemory(RegHL.Value16++));
									TB1 += RegAF.High8; RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
									--RegBC.Value16;
									RegFlagP = RegBC.Value16 != 0;
									RegFlagH = false;
									RegFlagN = false;
									if (RegBC.Value16 != 0) {
										RegPC.Value16 -= 2;
										this.totalExecutedCycles += 21; this.pendingCycles -= 21;
									} else {
										this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									}
									break;
								case 0xB1: // CPIR
									TB1 = ReadMemory(RegHL.Value16++); TB2 = (byte)(RegAF.High8 - TB1);
									RegFlagN = true;
									RegFlagH = TableHalfBorrow[RegAF.High8, TB1];
									RegFlagZ = TB2 == 0;
									RegFlagS = TB2 > 127;
									TB1 = (byte)(RegAF.High8 - TB1 - (RegFlagH ? 1 : 0)); RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
									--RegBC.Value16;
									RegFlagP = RegBC.Value16 != 0;
									if (RegBC.Value16 != 0 && !RegFlagZ) {
										RegPC.Value16 -= 2;
										this.totalExecutedCycles += 21; this.pendingCycles -= 21;
									} else {
										this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									}
									break;
								case 0xB2: // INIR
									WriteMemory(RegHL.Value16++, ReadHardware(RegBC.Value16));
									--RegBC.High8;
									RegFlagZ = RegBC.High8 == 0;
									RegFlagN = true;
									if (RegBC.High8 != 0) {
										RegPC.Value16 -= 2;
										this.totalExecutedCycles += 21; this.pendingCycles -= 21;
									} else {
										this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									}
									break;
								case 0xB3: // OTIR
									WriteHardware(RegBC.Value16, ReadMemory(RegHL.Value16++));
									--RegBC.High8;
									RegFlagZ = RegBC.High8 == 0;
									RegFlagN = true;
									if (RegBC.High8 != 0) {
										RegPC.Value16 -= 2;
										this.totalExecutedCycles += 21; this.pendingCycles -= 21;
									} else {
										this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									}
									break;
								case 0xB4: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xB5: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xB6: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xB7: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xB8: // LDDR
									WriteMemory(RegDE.Value16--, TB1 = ReadMemory(RegHL.Value16--));
									TB1 += RegAF.High8; RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
									--RegBC.Value16;
									RegFlagP = RegBC.Value16 != 0;
									RegFlagH = false;
									RegFlagN = false;
									if (RegBC.Value16 != 0) {
										RegPC.Value16 -= 2;
										this.totalExecutedCycles += 21; this.pendingCycles -= 21;
									} else {
										this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									}
									break;
								case 0xB9: // CPDR
									TB1 = ReadMemory(RegHL.Value16--); TB2 = (byte)(RegAF.High8 - TB1);
									RegFlagN = true;
									RegFlagH = TableHalfBorrow[RegAF.High8, TB1];
									RegFlagZ = TB2 == 0;
									RegFlagS = TB2 > 127;
									TB1 = (byte)(RegAF.High8 - TB1 - (RegFlagH ? 1 : 0)); RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
									--RegBC.Value16;
									RegFlagP = RegBC.Value16 != 0;
									if (RegBC.Value16 != 0 && !RegFlagZ) {
										RegPC.Value16 -= 2;
										this.totalExecutedCycles += 21; this.pendingCycles -= 21;
									} else {
										this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									}
									break;
								case 0xBA: // INDR
									WriteMemory(RegHL.Value16--, ReadHardware(RegBC.Value16));
									--RegBC.High8;
									RegFlagZ = RegBC.High8 == 0;
									RegFlagN = true;
									if (RegBC.High8 != 0) {
										RegPC.Value16 -= 2;
										this.totalExecutedCycles += 21; this.pendingCycles -= 21;
									} else {
										this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									}
									break;
								case 0xBB: // OTDR
									WriteHardware(RegBC.Value16, ReadMemory(RegHL.Value16--));
									--RegBC.High8;
									RegFlagZ = RegBC.High8 == 0;
									RegFlagN = true;
									if (RegBC.High8 != 0) {
										RegPC.Value16 -= 2;
										this.totalExecutedCycles += 21; this.pendingCycles -= 21;
									} else {
										this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									}
									break;
								case 0xBC: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xBD: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xBE: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xBF: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xC0: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xC1: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xC2: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xC3: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xC4: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xC5: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xC6: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xC7: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xC8: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xC9: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xCA: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xCB: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xCC: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xCD: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xCE: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xCF: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xD0: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xD1: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xD2: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xD3: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xD4: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xD5: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xD6: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xD7: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xD8: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xD9: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xDA: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xDB: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xDC: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xDD: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xDE: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xDF: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xE0: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xE1: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xE2: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xE3: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xE4: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xE5: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xE6: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xE7: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xE8: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xE9: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xEA: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xEB: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xEC: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xED: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xEE: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xEF: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xF0: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xF1: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xF2: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xF3: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xF4: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xF5: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xF6: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xF7: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xF8: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xF9: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xFA: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xFB: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xFC: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xFD: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xFE: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xFF: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
							}
							break;
						case 0xEE: // XOR n
							RegAF.Value16 = TableALU[5, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0xEF: // RST $28
							WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
							RegPC.Value16 = 0x28;
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							break;
						case 0xF0: // RET P
							if (!RegFlagS) {
								RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
								this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							} else {
								this.totalExecutedCycles += 5; this.pendingCycles -= 5;
							}
							break;
						case 0xF1: // POP AF
							RegAF.Low8 = ReadMemory(RegSP.Value16++); RegAF.High8 = ReadMemory(RegSP.Value16++);
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							break;
						case 0xF2: // JP P, nn
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (!RegFlagS) {
								RegPC.Value16 = TUS;
							}
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							break;
						case 0xF3: // DI
							this.IFF1 = this.IFF2 = false;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0xF4: // CALL P, nn
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (!RegFlagS) {
								WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
								RegPC.Value16 = TUS;
								this.totalExecutedCycles += 17; this.pendingCycles -= 17;
							} else {
								this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							}
							break;
						case 0xF5: // PUSH AF
							WriteMemory(--RegSP.Value16, RegAF.High8); WriteMemory(--RegSP.Value16, RegAF.Low8);
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							break;
						case 0xF6: // OR n
							RegAF.Value16 = TableALU[6, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0xF7: // RST $30
							WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
							RegPC.Value16 = 0x30;
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							break;
						case 0xF8: // RET M
							if (RegFlagS) {
								RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
								this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							} else {
								this.totalExecutedCycles += 5; this.pendingCycles -= 5;
							}
							break;
						case 0xF9: // LD SP, HL
							RegSP.Value16 = RegHL.Value16;
							this.totalExecutedCycles += 6; this.pendingCycles -= 6;
							break;
						case 0xFA: // JP M, nn
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (RegFlagS) {
								RegPC.Value16 = TUS;
							}
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							break;
						case 0xFB: // EI
							this.IFF1 = this.IFF2 = true;
							Interruptable = false;
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0xFC: // CALL M, nn
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (RegFlagS) {
								WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
								RegPC.Value16 = TUS;
								this.totalExecutedCycles += 17; this.pendingCycles -= 17;
							} else {
								this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							}
							break;
						case 0xFD: // (Prefix)
							++RegR;
							switch (ReadMemory(RegPC.Value16++)) {
								case 0x00: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x01: // LD BC, nn
									RegBC.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0x02: // LD (BC), A
									WriteMemory(RegBC.Value16, RegAF.High8);
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									break;
								case 0x03: // INC BC
									++RegBC.Value16;
									this.totalExecutedCycles += 6; this.pendingCycles -= 6;
									break;
								case 0x04: // INC B
									RegAF.Low8 = (byte)(TableInc[++RegBC.High8] | (RegAF.Low8 & 1));
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x05: // DEC B
									RegAF.Low8 = (byte)(TableDec[--RegBC.High8] | (RegAF.Low8 & 1));
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x06: // LD B, n
									RegBC.High8 = ReadMemory(RegPC.Value16++);
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									break;
								case 0x07: // RLCA
									RegAF.Value16 = TableRotShift[0, 0, RegAF.Value16];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x08: // EX AF, AF'
									TUS = RegAF.Value16; RegAF.Value16 = RegAltAF.Value16; RegAltAF.Value16 = TUS;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x09: // ADD IY, BC
									TI1 = (short)RegIY.Value16; TI2 = (short)RegBC.Value16; TIR = TI1 + TI2;
									TUS = (ushort)TIR;
									RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
									RegFlagN = false;
									RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
									RegIY.Value16 = TUS;
									RegFlag3 = (TUS & 0x0800) != 0;
									RegFlag5 = (TUS & 0x2000) != 0;
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0x0A: // LD A, (BC)
									RegAF.High8 = ReadMemory(RegBC.Value16);
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									break;
								case 0x0B: // DEC BC
									--RegBC.Value16;
									this.totalExecutedCycles += 6; this.pendingCycles -= 6;
									break;
								case 0x0C: // INC C
									RegAF.Low8 = (byte)(TableInc[++RegBC.Low8] | (RegAF.Low8 & 1));
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x0D: // DEC C
									RegAF.Low8 = (byte)(TableDec[--RegBC.Low8] | (RegAF.Low8 & 1));
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x0E: // LD C, n
									RegBC.Low8 = ReadMemory(RegPC.Value16++);
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									break;
								case 0x0F: // RRCA
									RegAF.Value16 = TableRotShift[0, 1, RegAF.Value16];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x10: // DJNZ d
									TSB = (sbyte)ReadMemory(RegPC.Value16++);
									if (--RegBC.High8 != 0) {
										RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
										this.totalExecutedCycles += 13; this.pendingCycles -= 13;
									} else {
										this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									}
									break;
								case 0x11: // LD DE, nn
									RegDE.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0x12: // LD (DE), A
									WriteMemory(RegDE.Value16, RegAF.High8);
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									break;
								case 0x13: // INC DE
									++RegDE.Value16;
									this.totalExecutedCycles += 6; this.pendingCycles -= 6;
									break;
								case 0x14: // INC D
									RegAF.Low8 = (byte)(TableInc[++RegDE.High8] | (RegAF.Low8 & 1));
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x15: // DEC D
									RegAF.Low8 = (byte)(TableDec[--RegDE.High8] | (RegAF.Low8 & 1));
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x16: // LD D, n
									RegDE.High8 = ReadMemory(RegPC.Value16++);
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									break;
								case 0x17: // RLA
									RegAF.Value16 = TableRotShift[0, 2, RegAF.Value16];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x18: // JR d
									TSB = (sbyte)ReadMemory(RegPC.Value16++);
									RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
										this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									break;
								case 0x19: // ADD IY, DE
									TI1 = (short)RegIY.Value16; TI2 = (short)RegDE.Value16; TIR = TI1 + TI2;
									TUS = (ushort)TIR;
									RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
									RegFlagN = false;
									RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
									RegIY.Value16 = TUS;
									RegFlag3 = (TUS & 0x0800) != 0;
									RegFlag5 = (TUS & 0x2000) != 0;
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0x1A: // LD A, (DE)
									RegAF.High8 = ReadMemory(RegDE.Value16);
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									break;
								case 0x1B: // DEC DE
									--RegDE.Value16;
									this.totalExecutedCycles += 6; this.pendingCycles -= 6;
									break;
								case 0x1C: // INC E
									RegAF.Low8 = (byte)(TableInc[++RegDE.Low8] | (RegAF.Low8 & 1));
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x1D: // DEC E
									RegAF.Low8 = (byte)(TableDec[--RegDE.Low8] | (RegAF.Low8 & 1));
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x1E: // LD E, n
									RegDE.Low8 = ReadMemory(RegPC.Value16++);
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									break;
								case 0x1F: // RRA
									RegAF.Value16 = TableRotShift[0, 3, RegAF.Value16];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x20: // JR NZ, d
									TSB = (sbyte)ReadMemory(RegPC.Value16++);
									if (!RegFlagZ) {
										RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
										this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									} else {
										this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									}
									break;
								case 0x21: // LD IY, nn
									RegIY.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									this.totalExecutedCycles += 14; this.pendingCycles -= 14;
									break;
								case 0x22: // LD (nn), IY
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									WriteMemory(TUS++, RegIY.Low8);
									WriteMemory(TUS, RegIY.High8);
									this.totalExecutedCycles += 20; this.pendingCycles -= 20;
									break;
								case 0x23: // INC IY
									++RegIY.Value16;
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0x24: // INC IYH
									RegAF.Low8 = (byte)(TableInc[++RegIY.High8] | (RegAF.Low8 & 1));
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x25: // DEC IYH
									RegAF.Low8 = (byte)(TableDec[--RegIY.High8] | (RegAF.Low8 & 1));
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x26: // LD IYH, n
									RegIY.High8 = ReadMemory(RegPC.Value16++);
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x27: // DAA
									RegAF.Value16 = TableDaa[RegAF.Value16];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x28: // JR Z, d
									TSB = (sbyte)ReadMemory(RegPC.Value16++);
									if (RegFlagZ) {
										RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
										this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									} else {
										this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									}
									break;
								case 0x29: // ADD IY, IY
									TI1 = (short)RegIY.Value16; TI2 = (short)RegIY.Value16; TIR = TI1 + TI2;
									TUS = (ushort)TIR;
									RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
									RegFlagN = false;
									RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
									RegIY.Value16 = TUS;
									RegFlag3 = (TUS & 0x0800) != 0;
									RegFlag5 = (TUS & 0x2000) != 0;
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0x2A: // LD IY, (nn)
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									RegIY.Low8 = ReadMemory(TUS++); RegIY.High8 = ReadMemory(TUS);
									this.totalExecutedCycles += 20; this.pendingCycles -= 20;
									break;
								case 0x2B: // DEC IY
									--RegIY.Value16;
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0x2C: // INC IYL
									RegAF.Low8 = (byte)(TableInc[++RegIY.Low8] | (RegAF.Low8 & 1));
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x2D: // DEC IYL
									RegAF.Low8 = (byte)(TableDec[--RegIY.Low8] | (RegAF.Low8 & 1));
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x2E: // LD IYL, n
									RegIY.Low8 = ReadMemory(RegPC.Value16++);
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x2F: // CPL
									RegAF.High8 ^= 0xFF; RegFlagH = true; RegFlagN = true; RegFlag3 = (RegAF.High8 & 0x08) != 0; RegFlag5 = (RegAF.High8 & 0x20) != 0;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x30: // JR NC, d
									TSB = (sbyte)ReadMemory(RegPC.Value16++);
									if (!RegFlagC) {
										RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
										this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									} else {
										this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									}
									break;
								case 0x31: // LD SP, nn
									RegSP.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0x32: // LD (nn), A
									WriteMemory((ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256), RegAF.High8);
									this.totalExecutedCycles += 13; this.pendingCycles -= 13;
									break;
								case 0x33: // INC SP
									++RegSP.Value16;
									this.totalExecutedCycles += 6; this.pendingCycles -= 6;
									break;
								case 0x34: // INC (IY+d)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									TB = ReadMemory((ushort)(RegIY.Value16 + Displacement)); RegAF.Low8 = (byte)(TableInc[++TB] | (RegAF.Low8 & 1)); WriteMemory((ushort)(RegIY.Value16 + Displacement), TB);
									this.totalExecutedCycles += 23; this.pendingCycles -= 23;
									break;
								case 0x35: // DEC (IY+d)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									TB = ReadMemory((ushort)(RegIY.Value16 + Displacement)); RegAF.Low8 = (byte)(TableDec[--TB] | (RegAF.Low8 & 1)); WriteMemory((ushort)(RegIY.Value16 + Displacement), TB);
									this.totalExecutedCycles += 23; this.pendingCycles -= 23;
									break;
								case 0x36: // LD (IY+d), n
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIY.Value16 + Displacement), ReadMemory(RegPC.Value16++));
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0x37: // SCF
									RegFlagH = false; RegFlagN = false; RegFlagC = true; RegFlag3 = (RegAF.High8 & 0x08) != 0; RegFlag5 = (RegAF.High8 & 0x20) != 0;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x38: // JR C, d
									TSB = (sbyte)ReadMemory(RegPC.Value16++);
									if (RegFlagC) {
										RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
										this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									} else {
										this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									}
									break;
								case 0x39: // ADD IY, SP
									TI1 = (short)RegIY.Value16; TI2 = (short)RegSP.Value16; TIR = TI1 + TI2;
									TUS = (ushort)TIR;
									RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
									RegFlagN = false;
									RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
									RegIY.Value16 = TUS;
									RegFlag3 = (TUS & 0x0800) != 0;
									RegFlag5 = (TUS & 0x2000) != 0;
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0x3A: // LD A, (nn)
									RegAF.High8 = ReadMemory((ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256));
									this.totalExecutedCycles += 13; this.pendingCycles -= 13;
									break;
								case 0x3B: // DEC SP
									--RegSP.Value16;
									this.totalExecutedCycles += 6; this.pendingCycles -= 6;
									break;
								case 0x3C: // INC A
									RegAF.Low8 = (byte)(TableInc[++RegAF.High8] | (RegAF.Low8 & 1));
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x3D: // DEC A
									RegAF.Low8 = (byte)(TableDec[--RegAF.High8] | (RegAF.Low8 & 1));
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x3E: // LD A, n
									RegAF.High8 = ReadMemory(RegPC.Value16++);
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									break;
								case 0x3F: // CCF
									RegFlagH = RegFlagC; RegFlagN = false; RegFlagC ^= true; RegFlag3 = (RegAF.High8 & 0x08) != 0; RegFlag5 = (RegAF.High8 & 0x20) != 0;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x40: // LD B, B
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x41: // LD B, C
									RegBC.High8 = RegBC.Low8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x42: // LD B, D
									RegBC.High8 = RegDE.High8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x43: // LD B, E
									RegBC.High8 = RegDE.Low8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x44: // LD B, IYH
									RegBC.High8 = RegIY.High8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x45: // LD B, IYL
									RegBC.High8 = RegIY.Low8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x46: // LD B, (IY+d)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegBC.High8 = ReadMemory((ushort)(RegIY.Value16 + Displacement));
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0x47: // LD B, A
									RegBC.High8 = RegAF.High8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x48: // LD C, B
									RegBC.Low8 = RegBC.High8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x49: // LD C, C
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x4A: // LD C, D
									RegBC.Low8 = RegDE.High8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x4B: // LD C, E
									RegBC.Low8 = RegDE.Low8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x4C: // LD C, IYH
									RegBC.Low8 = RegIY.High8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x4D: // LD C, IYL
									RegBC.Low8 = RegIY.Low8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x4E: // LD C, (IY+d)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegBC.Low8 = ReadMemory((ushort)(RegIY.Value16 + Displacement));
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0x4F: // LD C, A
									RegBC.Low8 = RegAF.High8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x50: // LD D, B
									RegDE.High8 = RegBC.High8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x51: // LD D, C
									RegDE.High8 = RegBC.Low8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x52: // LD D, D
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x53: // LD D, E
									RegDE.High8 = RegDE.Low8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x54: // LD D, IYH
									RegDE.High8 = RegIY.High8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x55: // LD D, IYL
									RegDE.High8 = RegIY.Low8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x56: // LD D, (IY+d)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegDE.High8 = ReadMemory((ushort)(RegIY.Value16 + Displacement));
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0x57: // LD D, A
									RegDE.High8 = RegAF.High8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x58: // LD E, B
									RegDE.Low8 = RegBC.High8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x59: // LD E, C
									RegDE.Low8 = RegBC.Low8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x5A: // LD E, D
									RegDE.Low8 = RegDE.High8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x5B: // LD E, E
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x5C: // LD E, IYH
									RegDE.Low8 = RegIY.High8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x5D: // LD E, IYL
									RegDE.Low8 = RegIY.Low8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x5E: // LD E, (IY+d)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegDE.Low8 = ReadMemory((ushort)(RegIY.Value16 + Displacement));
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0x5F: // LD E, A
									RegDE.Low8 = RegAF.High8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x60: // LD IYH, B
									RegIY.High8 = RegBC.High8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x61: // LD IYH, C
									RegIY.High8 = RegBC.Low8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x62: // LD IYH, D
									RegIY.High8 = RegDE.High8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x63: // LD IYH, E
									RegIY.High8 = RegDE.Low8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x64: // LD IYH, IYH
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x65: // LD IYH, IYL
									RegIY.High8 = RegIY.Low8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x66: // LD H, (IY+d)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegHL.High8 = ReadMemory((ushort)(RegIY.Value16 + Displacement));
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0x67: // LD IYH, A
									RegIY.High8 = RegAF.High8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x68: // LD IYL, B
									RegIY.Low8 = RegBC.High8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x69: // LD IYL, C
									RegIY.Low8 = RegBC.Low8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x6A: // LD IYL, D
									RegIY.Low8 = RegDE.High8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x6B: // LD IYL, E
									RegIY.Low8 = RegDE.Low8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x6C: // LD IYL, IYH
									RegIY.Low8 = RegIY.High8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x6D: // LD IYL, IYL
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x6E: // LD L, (IY+d)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegHL.Low8 = ReadMemory((ushort)(RegIY.Value16 + Displacement));
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0x6F: // LD IYL, A
									RegIY.Low8 = RegAF.High8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x70: // LD (IY+d), B
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIY.Value16 + Displacement), RegBC.High8);
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0x71: // LD (IY+d), C
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIY.Value16 + Displacement), RegBC.Low8);
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0x72: // LD (IY+d), D
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIY.Value16 + Displacement), RegDE.High8);
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0x73: // LD (IY+d), E
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIY.Value16 + Displacement), RegDE.Low8);
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0x74: // LD (IY+d), H
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIY.Value16 + Displacement), RegHL.High8);
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0x75: // LD (IY+d), L
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIY.Value16 + Displacement), RegHL.Low8);
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0x76: // HALT
									this.Halt();
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x77: // LD (IY+d), A
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIY.Value16 + Displacement), RegAF.High8);
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0x78: // LD A, B
									RegAF.High8 = RegBC.High8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x79: // LD A, C
									RegAF.High8 = RegBC.Low8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x7A: // LD A, D
									RegAF.High8 = RegDE.High8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x7B: // LD A, E
									RegAF.High8 = RegDE.Low8;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x7C: // LD A, IYH
									RegAF.High8 = RegIY.High8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x7D: // LD A, IYL
									RegAF.High8 = RegIY.Low8;
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x7E: // LD A, (IY+d)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.High8 = ReadMemory((ushort)(RegIY.Value16 + Displacement));
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0x7F: // LD A, A
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x80: // ADD A, B
									RegAF.Value16 = TableALU[0, RegAF.High8, RegBC.High8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x81: // ADD A, C
									RegAF.Value16 = TableALU[0, RegAF.High8, RegBC.Low8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x82: // ADD A, D
									RegAF.Value16 = TableALU[0, RegAF.High8, RegDE.High8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x83: // ADD A, E
									RegAF.Value16 = TableALU[0, RegAF.High8, RegDE.Low8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x84: // ADD A, IYH
									RegAF.Value16 = TableALU[0, RegAF.High8, RegIY.High8, 0];
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x85: // ADD A, IYL
									RegAF.Value16 = TableALU[0, RegAF.High8, RegIY.Low8, 0];
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x86: // ADD A, (IY+d)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[0, RegAF.High8, ReadMemory((ushort)(RegIY.Value16 + Displacement)), 0];
									this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									break;
								case 0x87: // ADD A, A
									RegAF.Value16 = TableALU[0, RegAF.High8, RegAF.High8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x88: // ADC A, B
									RegAF.Value16 = TableALU[1, RegAF.High8, RegBC.High8, RegFlagC ? 1 : 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x89: // ADC A, C
									RegAF.Value16 = TableALU[1, RegAF.High8, RegBC.Low8, RegFlagC ? 1 : 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x8A: // ADC A, D
									RegAF.Value16 = TableALU[1, RegAF.High8, RegDE.High8, RegFlagC ? 1 : 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x8B: // ADC A, E
									RegAF.Value16 = TableALU[1, RegAF.High8, RegDE.Low8, RegFlagC ? 1 : 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x8C: // ADC A, IYH
									RegAF.Value16 = TableALU[1, RegAF.High8, RegIY.High8, RegFlagC ? 1 : 0];
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x8D: // ADC A, IYL
									RegAF.Value16 = TableALU[1, RegAF.High8, RegIY.Low8, RegFlagC ? 1 : 0];
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x8E: // ADC A, (IY+d)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[1, RegAF.High8, ReadMemory((ushort)(RegIY.Value16 + Displacement)), RegFlagC ? 1 : 0];
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0x8F: // ADC A, A
									RegAF.Value16 = TableALU[1, RegAF.High8, RegAF.High8, RegFlagC ? 1 : 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x90: // SUB B
									RegAF.Value16 = TableALU[2, RegAF.High8, RegBC.High8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x91: // SUB C
									RegAF.Value16 = TableALU[2, RegAF.High8, RegBC.Low8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x92: // SUB D
									RegAF.Value16 = TableALU[2, RegAF.High8, RegDE.High8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x93: // SUB E
									RegAF.Value16 = TableALU[2, RegAF.High8, RegDE.Low8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x94: // SUB IYH
									RegAF.Value16 = TableALU[2, RegAF.High8, RegIY.High8, 0];
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x95: // SUB IYL
									RegAF.Value16 = TableALU[2, RegAF.High8, RegIY.Low8, 0];
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x96: // SUB (IY+d)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[2, RegAF.High8, ReadMemory((ushort)(RegIY.Value16 + Displacement)), 0];
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0x97: // SUB A, A
									RegAF.Value16 = TableALU[2, RegAF.High8, RegAF.High8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x98: // SBC A, B
									RegAF.Value16 = TableALU[3, RegAF.High8, RegBC.High8, RegFlagC ? 1 : 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x99: // SBC A, C
									RegAF.Value16 = TableALU[3, RegAF.High8, RegBC.Low8, RegFlagC ? 1 : 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x9A: // SBC A, D
									RegAF.Value16 = TableALU[3, RegAF.High8, RegDE.High8, RegFlagC ? 1 : 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x9B: // SBC A, E
									RegAF.Value16 = TableALU[3, RegAF.High8, RegDE.Low8, RegFlagC ? 1 : 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x9C: // SBC A, IYH
									RegAF.Value16 = TableALU[3, RegAF.High8, RegIY.High8, RegFlagC ? 1 : 0];
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x9D: // SBC A, IYL
									RegAF.Value16 = TableALU[3, RegAF.High8, RegIY.Low8, RegFlagC ? 1 : 0];
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x9E: // SBC A, (IY+d)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[3, RegAF.High8, ReadMemory((ushort)(RegIY.Value16 + Displacement)), RegFlagC ? 1 : 0];
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0x9F: // SBC A, A
									RegAF.Value16 = TableALU[3, RegAF.High8, RegAF.High8, RegFlagC ? 1 : 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xA0: // AND B
									RegAF.Value16 = TableALU[4, RegAF.High8, RegBC.High8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xA1: // AND C
									RegAF.Value16 = TableALU[4, RegAF.High8, RegBC.Low8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xA2: // AND D
									RegAF.Value16 = TableALU[4, RegAF.High8, RegDE.High8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xA3: // AND E
									RegAF.Value16 = TableALU[4, RegAF.High8, RegDE.Low8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xA4: // AND IYH
									RegAF.Value16 = TableALU[4, RegAF.High8, RegIY.High8, 0];
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0xA5: // AND IYL
									RegAF.Value16 = TableALU[4, RegAF.High8, RegIY.Low8, 0];
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0xA6: // AND (IY+d)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[4, RegAF.High8, ReadMemory((ushort)(RegIY.Value16 + Displacement)), 0];
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0xA7: // AND A
									RegAF.Value16 = TableALU[4, RegAF.High8, RegAF.High8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xA8: // XOR B
									RegAF.Value16 = TableALU[5, RegAF.High8, RegBC.High8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xA9: // XOR C
									RegAF.Value16 = TableALU[5, RegAF.High8, RegBC.Low8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xAA: // XOR D
									RegAF.Value16 = TableALU[5, RegAF.High8, RegDE.High8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xAB: // XOR E
									RegAF.Value16 = TableALU[5, RegAF.High8, RegDE.Low8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xAC: // XOR IYH
									RegAF.Value16 = TableALU[5, RegAF.High8, RegIY.High8, 0];
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0xAD: // XOR IYL
									RegAF.Value16 = TableALU[5, RegAF.High8, RegIY.Low8, 0];
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0xAE: // XOR (IY+d)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[5, RegAF.High8, ReadMemory((ushort)(RegIY.Value16 + Displacement)), 0];
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0xAF: // XOR A
									RegAF.Value16 = TableALU[5, RegAF.High8, RegAF.High8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xB0: // OR B
									RegAF.Value16 = TableALU[6, RegAF.High8, RegBC.High8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xB1: // OR C
									RegAF.Value16 = TableALU[6, RegAF.High8, RegBC.Low8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xB2: // OR D
									RegAF.Value16 = TableALU[6, RegAF.High8, RegDE.High8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xB3: // OR E
									RegAF.Value16 = TableALU[6, RegAF.High8, RegDE.Low8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xB4: // OR IYH
									RegAF.Value16 = TableALU[6, RegAF.High8, RegIY.High8, 0];
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0xB5: // OR IYL
									RegAF.Value16 = TableALU[6, RegAF.High8, RegIY.Low8, 0];
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0xB6: // OR (IY+d)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[6, RegAF.High8, ReadMemory((ushort)(RegIY.Value16 + Displacement)), 0];
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0xB7: // OR A
									RegAF.Value16 = TableALU[6, RegAF.High8, RegAF.High8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xB8: // CP B
									RegAF.Value16 = TableALU[7, RegAF.High8, RegBC.High8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xB9: // CP C
									RegAF.Value16 = TableALU[7, RegAF.High8, RegBC.Low8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xBA: // CP D
									RegAF.Value16 = TableALU[7, RegAF.High8, RegDE.High8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xBB: // CP E
									RegAF.Value16 = TableALU[7, RegAF.High8, RegDE.Low8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xBC: // CP IYH
									RegAF.Value16 = TableALU[7, RegAF.High8, RegIY.High8, 0];
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0xBD: // CP IYL
									RegAF.Value16 = TableALU[7, RegAF.High8, RegIY.Low8, 0];
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0xBE: // CP (IY+d)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[7, RegAF.High8, ReadMemory((ushort)(RegIY.Value16 + Displacement)), 0];
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									break;
								case 0xBF: // CP A
									RegAF.Value16 = TableALU[7, RegAF.High8, RegAF.High8, 0];
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xC0: // RET NZ
									if (!RegFlagZ) {
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
										this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									} else {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
									}
									break;
								case 0xC1: // POP BC
									RegBC.Low8 = ReadMemory(RegSP.Value16++); RegBC.High8 = ReadMemory(RegSP.Value16++);
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0xC2: // JP NZ, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagZ) {
										RegPC.Value16 = TUS;
									}
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0xC3: // JP nn
									RegPC.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0xC4: // CALL NZ, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagZ) {
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
										this.totalExecutedCycles += 17; this.pendingCycles -= 17;
									} else {
										this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									}
									break;
								case 0xC5: // PUSH BC
									WriteMemory(--RegSP.Value16, RegBC.High8); WriteMemory(--RegSP.Value16, RegBC.Low8);
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									break;
								case 0xC6: // ADD A, n
									RegAF.Value16 = TableALU[0, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									break;
								case 0xC7: // RST $00
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x00;
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									break;
								case 0xC8: // RET Z
									if (RegFlagZ) {
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
										this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									} else {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
									}
									break;
								case 0xC9: // RET
									RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0xCA: // JP Z, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagZ) {
										RegPC.Value16 = TUS;
									}
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0xCB: // (Prefix)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									++RegR;
									switch (ReadMemory(RegPC.Value16++)) {
										case 0x00: // RLC (IY+d)
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x01: // RLC (IY+d)
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x02: // RLC (IY+d)
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x03: // RLC (IY+d)
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x04: // RLC (IY+d)
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x05: // RLC (IY+d)
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x06: // RLC (IY+d)
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x07: // RLC (IY+d)
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x08: // RRC (IY+d)
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x09: // RRC (IY+d)
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x0A: // RRC (IY+d)
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x0B: // RRC (IY+d)
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x0C: // RRC (IY+d)
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x0D: // RRC (IY+d)
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x0E: // RRC (IY+d)
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x0F: // RRC (IY+d)
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x10: // RL (IY+d)
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x11: // RL (IY+d)
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x12: // RL (IY+d)
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x13: // RL (IY+d)
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x14: // RL (IY+d)
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x15: // RL (IY+d)
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x16: // RL (IY+d)
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x17: // RL (IY+d)
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x18: // RR (IY+d)
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x19: // RR (IY+d)
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x1A: // RR (IY+d)
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x1B: // RR (IY+d)
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x1C: // RR (IY+d)
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x1D: // RR (IY+d)
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x1E: // RR (IY+d)
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x1F: // RR (IY+d)
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x20: // SLA (IY+d)
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x21: // SLA (IY+d)
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x22: // SLA (IY+d)
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x23: // SLA (IY+d)
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x24: // SLA (IY+d)
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x25: // SLA (IY+d)
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x26: // SLA (IY+d)
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x27: // SLA (IY+d)
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x28: // SRA (IY+d)
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x29: // SRA (IY+d)
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x2A: // SRA (IY+d)
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x2B: // SRA (IY+d)
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x2C: // SRA (IY+d)
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x2D: // SRA (IY+d)
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x2E: // SRA (IY+d)
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x2F: // SRA (IY+d)
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x30: // SL1 (IY+d)
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x31: // SL1 (IY+d)
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x32: // SL1 (IY+d)
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x33: // SL1 (IY+d)
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x34: // SL1 (IY+d)
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x35: // SL1 (IY+d)
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x36: // SL1 (IY+d)
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x37: // SL1 (IY+d)
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x38: // SRL (IY+d)
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x39: // SRL (IY+d)
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x3A: // SRL (IY+d)
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x3B: // SRL (IY+d)
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x3C: // SRL (IY+d)
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x3D: // SRL (IY+d)
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x3E: // SRL (IY+d)
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x3F: // SRL (IY+d)
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x40: // BIT 0, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x41: // BIT 0, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x42: // BIT 0, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x43: // BIT 0, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x44: // BIT 0, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x45: // BIT 0, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x46: // BIT 0, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x47: // BIT 0, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x48: // BIT 1, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x49: // BIT 1, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x4A: // BIT 1, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x4B: // BIT 1, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x4C: // BIT 1, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x4D: // BIT 1, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x4E: // BIT 1, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x4F: // BIT 1, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x50: // BIT 2, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x51: // BIT 2, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x52: // BIT 2, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x53: // BIT 2, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x54: // BIT 2, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x55: // BIT 2, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x56: // BIT 2, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x57: // BIT 2, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x58: // BIT 3, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x59: // BIT 3, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x5A: // BIT 3, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x5B: // BIT 3, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x5C: // BIT 3, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x5D: // BIT 3, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x5E: // BIT 3, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x5F: // BIT 3, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x60: // BIT 4, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x61: // BIT 4, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x62: // BIT 4, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x63: // BIT 4, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x64: // BIT 4, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x65: // BIT 4, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x66: // BIT 4, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x67: // BIT 4, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x68: // BIT 5, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x69: // BIT 5, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x6A: // BIT 5, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x6B: // BIT 5, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x6C: // BIT 5, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x6D: // BIT 5, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x6E: // BIT 5, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x6F: // BIT 5, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x70: // BIT 6, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x71: // BIT 6, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x72: // BIT 6, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x73: // BIT 6, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x74: // BIT 6, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x75: // BIT 6, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x76: // BIT 6, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x77: // BIT 6, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x78: // BIT 7, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x79: // BIT 7, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x7A: // BIT 7, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x7B: // BIT 7, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x7C: // BIT 7, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x7D: // BIT 7, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x7E: // BIT 7, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x7F: // BIT 7, (IY+d)
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x80: // RES 0, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x01)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x81: // RES 0, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x01)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x82: // RES 0, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x01)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x83: // RES 0, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x01)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x84: // RES 0, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x01)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x85: // RES 0, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x01)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x86: // RES 0, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x01)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x87: // RES 0, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x01)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x88: // RES 1, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x02)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x89: // RES 1, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x02)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x8A: // RES 1, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x02)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x8B: // RES 1, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x02)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x8C: // RES 1, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x02)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x8D: // RES 1, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x02)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x8E: // RES 1, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x02)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x8F: // RES 1, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x02)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x90: // RES 2, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x04)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x91: // RES 2, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x04)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x92: // RES 2, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x04)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x93: // RES 2, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x04)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x94: // RES 2, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x04)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x95: // RES 2, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x04)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x96: // RES 2, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x04)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x97: // RES 2, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x04)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x98: // RES 3, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x08)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x99: // RES 3, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x08)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x9A: // RES 3, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x08)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x9B: // RES 3, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x08)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x9C: // RES 3, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x08)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x9D: // RES 3, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x08)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x9E: // RES 3, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x08)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0x9F: // RES 3, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x08)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xA0: // RES 4, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x10)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xA1: // RES 4, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x10)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xA2: // RES 4, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x10)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xA3: // RES 4, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x10)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xA4: // RES 4, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x10)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xA5: // RES 4, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x10)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xA6: // RES 4, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x10)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xA7: // RES 4, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x10)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xA8: // RES 5, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x20)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xA9: // RES 5, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x20)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xAA: // RES 5, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x20)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xAB: // RES 5, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x20)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xAC: // RES 5, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x20)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xAD: // RES 5, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x20)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xAE: // RES 5, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x20)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xAF: // RES 5, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x20)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xB0: // RES 6, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x40)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xB1: // RES 6, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x40)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xB2: // RES 6, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x40)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xB3: // RES 6, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x40)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xB4: // RES 6, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x40)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xB5: // RES 6, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x40)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xB6: // RES 6, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x40)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xB7: // RES 6, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x40)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xB8: // RES 7, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x80)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xB9: // RES 7, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x80)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xBA: // RES 7, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x80)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xBB: // RES 7, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x80)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xBC: // RES 7, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x80)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xBD: // RES 7, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x80)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xBE: // RES 7, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x80)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xBF: // RES 7, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x80)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xC0: // SET 0, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x01)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xC1: // SET 0, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x01)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xC2: // SET 0, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x01)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xC3: // SET 0, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x01)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xC4: // SET 0, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x01)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xC5: // SET 0, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x01)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xC6: // SET 0, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x01)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xC7: // SET 0, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x01)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xC8: // SET 1, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x02)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xC9: // SET 1, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x02)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xCA: // SET 1, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x02)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xCB: // SET 1, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x02)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xCC: // SET 1, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x02)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xCD: // SET 1, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x02)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xCE: // SET 1, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x02)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xCF: // SET 1, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x02)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xD0: // SET 2, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x04)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xD1: // SET 2, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x04)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xD2: // SET 2, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x04)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xD3: // SET 2, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x04)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xD4: // SET 2, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x04)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xD5: // SET 2, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x04)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xD6: // SET 2, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x04)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xD7: // SET 2, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x04)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xD8: // SET 3, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x08)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xD9: // SET 3, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x08)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xDA: // SET 3, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x08)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xDB: // SET 3, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x08)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xDC: // SET 3, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x08)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xDD: // SET 3, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x08)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xDE: // SET 3, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x08)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xDF: // SET 3, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x08)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xE0: // SET 4, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x10)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xE1: // SET 4, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x10)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xE2: // SET 4, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x10)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xE3: // SET 4, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x10)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xE4: // SET 4, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x10)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xE5: // SET 4, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x10)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xE6: // SET 4, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x10)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xE7: // SET 4, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x10)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xE8: // SET 5, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x20)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xE9: // SET 5, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x20)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xEA: // SET 5, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x20)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xEB: // SET 5, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x20)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xEC: // SET 5, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x20)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xED: // SET 5, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x20)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xEE: // SET 5, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x20)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xEF: // SET 5, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x20)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xF0: // SET 6, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x40)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xF1: // SET 6, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x40)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xF2: // SET 6, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x40)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xF3: // SET 6, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x40)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xF4: // SET 6, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x40)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xF5: // SET 6, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x40)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xF6: // SET 6, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x40)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xF7: // SET 6, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x40)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xF8: // SET 7, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x80)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xF9: // SET 7, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x80)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xFA: // SET 7, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x80)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xFB: // SET 7, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x80)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xFC: // SET 7, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x80)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xFD: // SET 7, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x80)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xFE: // SET 7, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x80)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
										case 0xFF: // SET 7, (IY+d)
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x80)));
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											break;
									}
									break;
								case 0xCC: // CALL Z, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagZ) {
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
										this.totalExecutedCycles += 17; this.pendingCycles -= 17;
									} else {
										this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									}
									break;
								case 0xCD: // CALL nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = TUS;
									this.totalExecutedCycles += 17; this.pendingCycles -= 17;
									break;
								case 0xCE: // ADC A, n
									RegAF.Value16 = TableALU[1, RegAF.High8, ReadMemory(RegPC.Value16++), RegFlagC ? 1 : 0];
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									break;
								case 0xCF: // RST $08
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x08;
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									break;
								case 0xD0: // RET NC
									if (!RegFlagC) {
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
										this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									} else {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
									}
									break;
								case 0xD1: // POP DE
									RegDE.Low8 = ReadMemory(RegSP.Value16++); RegDE.High8 = ReadMemory(RegSP.Value16++);
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0xD2: // JP NC, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagC) {
										RegPC.Value16 = TUS;
									}
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0xD3: // OUT n, A
									WriteHardware(ReadMemory(RegPC.Value16++), RegAF.High8);
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									break;
								case 0xD4: // CALL NC, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagC) {
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
										this.totalExecutedCycles += 17; this.pendingCycles -= 17;
									} else {
										this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									}
									break;
								case 0xD5: // PUSH DE
									WriteMemory(--RegSP.Value16, RegDE.High8); WriteMemory(--RegSP.Value16, RegDE.Low8);
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									break;
								case 0xD6: // SUB n
									RegAF.Value16 = TableALU[2, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									break;
								case 0xD7: // RST $10
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x10;
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									break;
								case 0xD8: // RET C
									if (RegFlagC) {
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
										this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									} else {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
									}
									break;
								case 0xD9: // EXX
									TUS = RegBC.Value16; RegBC.Value16 = RegAltBC.Value16; RegAltBC.Value16 = TUS;
									TUS = RegDE.Value16; RegDE.Value16 = RegAltDE.Value16; RegAltDE.Value16 = TUS;
									TUS = RegHL.Value16; RegHL.Value16 = RegAltHL.Value16; RegAltHL.Value16 = TUS;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xDA: // JP C, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagC) {
										RegPC.Value16 = TUS;
									}
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0xDB: // IN A, n
									RegAF.High8 = ReadHardware((ushort)ReadMemory(RegPC.Value16++));
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									break;
								case 0xDC: // CALL C, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagC) {
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
										this.totalExecutedCycles += 17; this.pendingCycles -= 17;
									} else {
										this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									}
									break;
								case 0xDD: // <-
									// Invalid sequence.
									this.totalExecutedCycles += 1337; this.pendingCycles -= 1337;
									break;
								case 0xDE: // SBC A, n
									RegAF.Value16 = TableALU[3, RegAF.High8, ReadMemory(RegPC.Value16++), RegFlagC ? 1 : 0];
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									break;
								case 0xDF: // RST $18
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x18;
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									break;
								case 0xE0: // RET PO
									if (!RegFlagP) {
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
										this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									} else {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
									}
									break;
								case 0xE1: // POP IY
									RegIY.Low8 = ReadMemory(RegSP.Value16++); RegIY.High8 = ReadMemory(RegSP.Value16++);
									this.totalExecutedCycles += 14; this.pendingCycles -= 14;
									break;
								case 0xE2: // JP PO, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagP) {
										RegPC.Value16 = TUS;
									}
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0xE3: // EX (SP), IY
									TUS = RegSP.Value16; TBL = ReadMemory(TUS++); TBH = ReadMemory(TUS--);
									WriteMemory(TUS++, RegIY.Low8); WriteMemory(TUS, RegIY.High8);
									RegIY.Low8 = TBL; RegIY.High8 = TBH;
									this.totalExecutedCycles += 23; this.pendingCycles -= 23;
									break;
								case 0xE4: // CALL C, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagC) {
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
										this.totalExecutedCycles += 17; this.pendingCycles -= 17;
									} else {
										this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									}
									break;
								case 0xE5: // PUSH IY
									WriteMemory(--RegSP.Value16, RegIY.High8); WriteMemory(--RegSP.Value16, RegIY.Low8);
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									break;
								case 0xE6: // AND n
									RegAF.Value16 = TableALU[4, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									break;
								case 0xE7: // RST $20
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x20;
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									break;
								case 0xE8: // RET PE
									if (RegFlagP) {
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
										this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									} else {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
									}
									break;
								case 0xE9: // JP IY
									RegPC.Value16 = RegIY.Value16;
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									break;
								case 0xEA: // JP PE, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagP) {
										RegPC.Value16 = TUS;
									}
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0xEB: // EX DE, HL
									TUS = RegDE.Value16; RegDE.Value16 = RegHL.Value16; RegHL.Value16 = TUS;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xEC: // CALL PE, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagP) {
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
										this.totalExecutedCycles += 17; this.pendingCycles -= 17;
									} else {
										this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									}
									break;
								case 0xED: // (Prefix)
									++RegR;
									switch (ReadMemory(RegPC.Value16++)) {
										case 0x00: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x01: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x02: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x03: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x04: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x05: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x06: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x07: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x08: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x09: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x0A: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x0B: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x0C: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x0D: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x0E: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x0F: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x10: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x11: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x12: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x13: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x14: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x15: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x16: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x17: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x18: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x19: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x1A: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x1B: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x1C: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x1D: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x1E: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x1F: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x20: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x21: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x22: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x23: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x24: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x25: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x26: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x27: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x28: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x29: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x2A: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x2B: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x2C: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x2D: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x2E: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x2F: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x30: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x31: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x32: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x33: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x34: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x35: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x36: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x37: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x38: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x39: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x3A: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x3B: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x3C: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x3D: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x3E: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x3F: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x40: // IN B, C
											RegBC.High8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegBC.High8 > 127;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegBC.High8];
											RegFlagN = false;
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											break;
										case 0x41: // OUT C, B
											WriteHardware(RegBC.Low8, RegBC.High8);
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											break;
										case 0x42: // SBC HL, BC
											TI1 = (short)RegHL.Value16; TI2 = (short)RegBC.Value16; TIR = TI1 - TI2;
											if (RegFlagC) { --TIR; ++TI2; }
											TUS = (ushort)TIR;
											RegFlagH = ((RegHL.Value16 ^ RegBC.Value16 ^ TUS) & 0x1000) != 0;
											RegFlagN = true;
											RegFlagC = (((int)RegHL.Value16 - (int)RegBC.Value16 - (this.RegFlagC ? 1 : 0)) & 0x10000) != 0;
											RegFlagP = TIR > 32767 || TIR < -32768;
											RegFlagS = TUS > 32767;
											RegFlagZ = TUS == 0;
											RegHL.Value16 = TUS;
											RegFlag3 = (TUS & 0x0800) != 0;
											RegFlag5 = (TUS & 0x2000) != 0;
											this.totalExecutedCycles += 15; this.pendingCycles -= 15;
											break;
										case 0x43: // LD (nn), BC
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											WriteMemory(TUS++, RegBC.Low8);
											WriteMemory(TUS, RegBC.High8);
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x44: // NEG
											RegAF.Value16 = TableNeg[RegAF.Value16];
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											break;
										case 0x45: // RETN
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											this.IFF1 = this.IFF2;
											this.totalExecutedCycles += 14; this.pendingCycles -= 14;
											break;
										case 0x46: // IM $0
											interruptMode = 0;
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											break;
										case 0x47: // LD I, A
											RegI = RegAF.High8;
											this.totalExecutedCycles += 9; this.pendingCycles -= 9;
											break;
										case 0x48: // IN C, C
											RegBC.Low8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegBC.Low8 > 127;
											RegFlagZ = RegBC.Low8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegBC.Low8];
											RegFlagN = false;
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											break;
										case 0x49: // OUT C, C
											WriteHardware(RegBC.Low8, RegBC.Low8);
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											break;
										case 0x4A: // ADC HL, BC
											TI1 = (short)RegHL.Value16; TI2 = (short)RegBC.Value16; TIR = TI1 + TI2;
											if (RegFlagC) { ++TIR; ++TI2; }
											TUS = (ushort)TIR;
											RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
											RegFlagN = false;
											RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
											RegFlagP = TIR > 32767 || TIR < -32768;
											RegFlagS = TUS > 32767;
											RegFlagZ = TUS == 0;
											RegHL.Value16 = TUS;
											RegFlag3 = (TUS & 0x0800) != 0;
											RegFlag5 = (TUS & 0x2000) != 0;
											this.totalExecutedCycles += 15; this.pendingCycles -= 15;
											break;
										case 0x4B: // LD BC, (nn)
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											RegBC.Low8 = ReadMemory(TUS++); RegBC.High8 = ReadMemory(TUS);
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x4C: // NEG
											RegAF.Value16 = TableNeg[RegAF.Value16];
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											break;
										case 0x4D: // RETI
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											this.totalExecutedCycles += 14; this.pendingCycles -= 14;
											break;
										case 0x4E: // IM $0
											interruptMode = 0;
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											break;
										case 0x4F: // LD R, A
											RegR = RegAF.High8;
											this.totalExecutedCycles += 9; this.pendingCycles -= 9;
											break;
										case 0x50: // IN D, C
											RegDE.High8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegDE.High8 > 127;
											RegFlagZ = RegDE.High8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegDE.High8];
											RegFlagN = false;
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											break;
										case 0x51: // OUT C, D
											WriteHardware(RegBC.Low8, RegDE.High8);
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											break;
										case 0x52: // SBC HL, DE
											TI1 = (short)RegHL.Value16; TI2 = (short)RegDE.Value16; TIR = TI1 - TI2;
											if (RegFlagC) { --TIR; ++TI2; }
											TUS = (ushort)TIR;
											RegFlagH = ((RegHL.Value16 ^ RegDE.Value16 ^ TUS) & 0x1000) != 0;
											RegFlagN = true;
											RegFlagC = (((int)RegHL.Value16 - (int)RegDE.Value16 - (this.RegFlagC ? 1 : 0)) & 0x10000) != 0;
											RegFlagP = TIR > 32767 || TIR < -32768;
											RegFlagS = TUS > 32767;
											RegFlagZ = TUS == 0;
											RegHL.Value16 = TUS;
											RegFlag3 = (TUS & 0x0800) != 0;
											RegFlag5 = (TUS & 0x2000) != 0;
											this.totalExecutedCycles += 15; this.pendingCycles -= 15;
											break;
										case 0x53: // LD (nn), DE
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											WriteMemory(TUS++, RegDE.Low8);
											WriteMemory(TUS, RegDE.High8);
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x54: // NEG
											RegAF.Value16 = TableNeg[RegAF.Value16];
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											break;
										case 0x55: // RETN
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											this.IFF1 = this.IFF2;
											this.totalExecutedCycles += 14; this.pendingCycles -= 14;
											break;
										case 0x56: // IM $1
											interruptMode = 1;
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											break;
										case 0x57: // LD A, I
											RegAF.High8 = RegI;
											RegFlagS = RegI > 127;
											RegFlagZ = RegI == 0;
											RegFlagH = false;
											RegFlagN = false;
											RegFlagP = this.IFF2;
											this.totalExecutedCycles += 9; this.pendingCycles -= 9;
											break;
										case 0x58: // IN E, C
											RegDE.Low8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegDE.Low8 > 127;
											RegFlagZ = RegDE.Low8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegDE.Low8];
											RegFlagN = false;
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											break;
										case 0x59: // OUT C, E
											WriteHardware(RegBC.Low8, RegDE.Low8);
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											break;
										case 0x5A: // ADC HL, DE
											TI1 = (short)RegHL.Value16; TI2 = (short)RegDE.Value16; TIR = TI1 + TI2;
											if (RegFlagC) { ++TIR; ++TI2; }
											TUS = (ushort)TIR;
											RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
											RegFlagN = false;
											RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
											RegFlagP = TIR > 32767 || TIR < -32768;
											RegFlagS = TUS > 32767;
											RegFlagZ = TUS == 0;
											RegHL.Value16 = TUS;
											RegFlag3 = (TUS & 0x0800) != 0;
											RegFlag5 = (TUS & 0x2000) != 0;
											this.totalExecutedCycles += 15; this.pendingCycles -= 15;
											break;
										case 0x5B: // LD DE, (nn)
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											RegDE.Low8 = ReadMemory(TUS++); RegDE.High8 = ReadMemory(TUS);
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x5C: // NEG
											RegAF.Value16 = TableNeg[RegAF.Value16];
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											break;
										case 0x5D: // RETI
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											this.totalExecutedCycles += 14; this.pendingCycles -= 14;
											break;
										case 0x5E: // IM $2
											interruptMode = 2;
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											break;
										case 0x5F: // LD A, R
											RegAF.High8 = (byte)(RegR & 0x7F);
											RegFlagS = (byte)(RegR & 0x7F) > 127;
											RegFlagZ = (byte)(RegR & 0x7F) == 0;
											RegFlagH = false;
											RegFlagN = false;
											RegFlagP = this.IFF2;
											this.totalExecutedCycles += 9; this.pendingCycles -= 9;
											break;
										case 0x60: // IN H, C
											RegHL.High8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegHL.High8 > 127;
											RegFlagZ = RegHL.High8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegHL.High8];
											RegFlagN = false;
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											break;
										case 0x61: // OUT C, H
											WriteHardware(RegBC.Low8, RegHL.High8);
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											break;
										case 0x62: // SBC HL, HL
											TI1 = (short)RegHL.Value16; TI2 = (short)RegHL.Value16; TIR = TI1 - TI2;
											if (RegFlagC) { --TIR; ++TI2; }
											TUS = (ushort)TIR;
											RegFlagH = ((RegHL.Value16 ^ RegHL.Value16 ^ TUS) & 0x1000) != 0;
											RegFlagN = true;
											RegFlagC = (((int)RegHL.Value16 - (int)RegHL.Value16 - (this.RegFlagC ? 1 : 0)) & 0x10000) != 0;
											RegFlagP = TIR > 32767 || TIR < -32768;
											RegFlagS = TUS > 32767;
											RegFlagZ = TUS == 0;
											RegHL.Value16 = TUS;
											RegFlag3 = (TUS & 0x0800) != 0;
											RegFlag5 = (TUS & 0x2000) != 0;
											this.totalExecutedCycles += 15; this.pendingCycles -= 15;
											break;
										case 0x63: // LD (nn), HL
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											WriteMemory(TUS++, RegHL.Low8);
											WriteMemory(TUS, RegHL.High8);
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											break;
										case 0x64: // NEG
											RegAF.Value16 = TableNeg[RegAF.Value16];
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											break;
										case 0x65: // RETN
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											this.IFF1 = this.IFF2;
											this.totalExecutedCycles += 14; this.pendingCycles -= 14;
											break;
										case 0x66: // IM $0
											interruptMode = 0;
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											break;
										case 0x67: // RRD
											TB1 = RegAF.High8; TB2 = ReadMemory(RegHL.Value16);
											WriteMemory(RegHL.Value16, (byte)((TB2 >> 4) + (TB1 << 4)));
											RegAF.High8 = (byte)((TB1 & 0xF0) + (TB2 & 0x0F));
											RegFlagS = RegAF.High8 > 127;
											RegFlagZ = RegAF.High8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegAF.High8];
											RegFlagN = false;
											RegFlag3 = (RegAF.High8 & 0x08) != 0;
											RegFlag5 = (RegAF.High8 & 0x20) != 0;
											this.totalExecutedCycles += 18; this.pendingCycles -= 18;
											break;
										case 0x68: // IN L, C
											RegHL.Low8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegHL.Low8 > 127;
											RegFlagZ = RegHL.Low8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegHL.Low8];
											RegFlagN = false;
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											break;
										case 0x69: // OUT C, L
											WriteHardware(RegBC.Low8, RegHL.Low8);
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											break;
										case 0x6A: // ADC HL, HL
											TI1 = (short)RegHL.Value16; TI2 = (short)RegHL.Value16; TIR = TI1 + TI2;
											if (RegFlagC) { ++TIR; ++TI2; }
											TUS = (ushort)TIR;
											RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
											RegFlagN = false;
											RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
											RegFlagP = TIR > 32767 || TIR < -32768;
											RegFlagS = TUS > 32767;
											RegFlagZ = TUS == 0;
											RegHL.Value16 = TUS;
											RegFlag3 = (TUS & 0x0800) != 0;
											RegFlag5 = (TUS & 0x2000) != 0;
											this.totalExecutedCycles += 15; this.pendingCycles -= 15;
											break;
										case 0x6B: // LD HL, (nn)
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											RegHL.Low8 = ReadMemory(TUS++); RegHL.High8 = ReadMemory(TUS);
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											break;
										case 0x6C: // NEG
											RegAF.Value16 = TableNeg[RegAF.Value16];
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											break;
										case 0x6D: // RETI
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											this.totalExecutedCycles += 14; this.pendingCycles -= 14;
											break;
										case 0x6E: // IM $0
											interruptMode = 0;
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											break;
										case 0x6F: // RLD
											TB1 = RegAF.High8; TB2 = ReadMemory(RegHL.Value16);
											WriteMemory(RegHL.Value16, (byte)((TB1 & 0x0F) + (TB2 << 4)));
											RegAF.High8 = (byte)((TB1 & 0xF0) + (TB2 >> 4));
											RegFlagS = RegAF.High8 > 127;
											RegFlagZ = RegAF.High8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegAF.High8];
											RegFlagN = false;
											RegFlag3 = (RegAF.High8 & 0x08) != 0;
											RegFlag5 = (RegAF.High8 & 0x20) != 0;
											this.totalExecutedCycles += 18; this.pendingCycles -= 18;
											break;
										case 0x70: // IN 0, C
											TB = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = TB > 127;
											RegFlagZ = TB == 0;
											RegFlagH = false;
											RegFlagP = TableParity[TB];
											RegFlagN = false;
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											break;
										case 0x71: // OUT C, 0
											WriteHardware(RegBC.Low8, 0);
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											break;
										case 0x72: // SBC HL, SP
											TI1 = (short)RegHL.Value16; TI2 = (short)RegSP.Value16; TIR = TI1 - TI2;
											if (RegFlagC) { --TIR; ++TI2; }
											TUS = (ushort)TIR;
											RegFlagH = ((RegHL.Value16 ^ RegSP.Value16 ^ TUS) & 0x1000) != 0;
											RegFlagN = true;
											RegFlagC = (((int)RegHL.Value16 - (int)RegSP.Value16 - (this.RegFlagC ? 1 : 0)) & 0x10000) != 0;
											RegFlagP = TIR > 32767 || TIR < -32768;
											RegFlagS = TUS > 32767;
											RegFlagZ = TUS == 0;
											RegHL.Value16 = TUS;
											RegFlag3 = (TUS & 0x0800) != 0;
											RegFlag5 = (TUS & 0x2000) != 0;
											this.totalExecutedCycles += 15; this.pendingCycles -= 15;
											break;
										case 0x73: // LD (nn), SP
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											WriteMemory(TUS++, RegSP.Low8);
											WriteMemory(TUS, RegSP.High8);
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x74: // NEG
											RegAF.Value16 = TableNeg[RegAF.Value16];
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											break;
										case 0x75: // RETN
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											this.IFF1 = this.IFF2;
											this.totalExecutedCycles += 14; this.pendingCycles -= 14;
											break;
										case 0x76: // IM $1
											interruptMode = 1;
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											break;
										case 0x77: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x78: // IN A, C
											RegAF.High8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegAF.High8 > 127;
											RegFlagZ = RegAF.High8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegAF.High8];
											RegFlagN = false;
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											break;
										case 0x79: // OUT C, A
											WriteHardware(RegBC.Low8, RegAF.High8);
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											break;
										case 0x7A: // ADC HL, SP
											TI1 = (short)RegHL.Value16; TI2 = (short)RegSP.Value16; TIR = TI1 + TI2;
											if (RegFlagC) { ++TIR; ++TI2; }
											TUS = (ushort)TIR;
											RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
											RegFlagN = false;
											RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
											RegFlagP = TIR > 32767 || TIR < -32768;
											RegFlagS = TUS > 32767;
											RegFlagZ = TUS == 0;
											RegHL.Value16 = TUS;
											RegFlag3 = (TUS & 0x0800) != 0;
											RegFlag5 = (TUS & 0x2000) != 0;
											this.totalExecutedCycles += 15; this.pendingCycles -= 15;
											break;
										case 0x7B: // LD SP, (nn)
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											RegSP.Low8 = ReadMemory(TUS++); RegSP.High8 = ReadMemory(TUS);
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											break;
										case 0x7C: // NEG
											RegAF.Value16 = TableNeg[RegAF.Value16];
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											break;
										case 0x7D: // RETI
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											this.totalExecutedCycles += 14; this.pendingCycles -= 14;
											break;
										case 0x7E: // IM $2
											interruptMode = 2;
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											break;
										case 0x7F: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x80: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x81: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x82: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x83: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x84: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x85: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x86: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x87: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x88: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x89: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x8A: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x8B: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x8C: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x8D: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x8E: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x8F: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x90: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x91: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x92: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x93: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x94: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x95: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x96: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x97: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x98: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x99: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x9A: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x9B: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x9C: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x9D: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x9E: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x9F: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xA0: // LDI
											WriteMemory(RegDE.Value16++, TB1 = ReadMemory(RegHL.Value16++));
											TB1 += RegAF.High8; RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											RegFlagH = false;
											RegFlagN = false;
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											break;
										case 0xA1: // CPI
											TB1 = ReadMemory(RegHL.Value16++); TB2 = (byte)(RegAF.High8 - TB1);
											RegFlagN = true;
											RegFlagH = TableHalfBorrow[RegAF.High8, TB1];
											RegFlagZ = TB2 == 0;
											RegFlagS = TB2 > 127;
											TB1 = (byte)(RegAF.High8 - TB1 - (RegFlagH ? 1 : 0)); RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											break;
										case 0xA2: // INI
											WriteMemory(RegHL.Value16++, ReadHardware(RegBC.Value16));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											break;
										case 0xA3: // OUTI
											WriteHardware(RegBC.Value16, ReadMemory(RegHL.Value16++));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											break;
										case 0xA4: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xA5: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xA6: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xA7: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xA8: // LDD
											WriteMemory(RegDE.Value16--, TB1 = ReadMemory(RegHL.Value16--));
											TB1 += RegAF.High8; RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											RegFlagH = false;
											RegFlagN = false;
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											break;
										case 0xA9: // CPD
											TB1 = ReadMemory(RegHL.Value16--); TB2 = (byte)(RegAF.High8 - TB1);
											RegFlagN = true;
											RegFlagH = TableHalfBorrow[RegAF.High8, TB1];
											RegFlagZ = TB2 == 0;
											RegFlagS = TB2 > 127;
											TB1 = (byte)(RegAF.High8 - TB1 - (RegFlagH ? 1 : 0)); RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											break;
										case 0xAA: // IND
											WriteMemory(RegHL.Value16--, ReadHardware(RegBC.Value16));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											break;
										case 0xAB: // OUTD
											WriteHardware(RegBC.Value16, ReadMemory(RegHL.Value16--));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											break;
										case 0xAC: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xAD: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xAE: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xAF: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xB0: // LDIR
											WriteMemory(RegDE.Value16++, TB1 = ReadMemory(RegHL.Value16++));
											TB1 += RegAF.High8; RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											RegFlagH = false;
											RegFlagN = false;
											if (RegBC.Value16 != 0) {
												RegPC.Value16 -= 2;
												this.totalExecutedCycles += 21; this.pendingCycles -= 21;
											} else {
												this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											}
											break;
										case 0xB1: // CPIR
											TB1 = ReadMemory(RegHL.Value16++); TB2 = (byte)(RegAF.High8 - TB1);
											RegFlagN = true;
											RegFlagH = TableHalfBorrow[RegAF.High8, TB1];
											RegFlagZ = TB2 == 0;
											RegFlagS = TB2 > 127;
											TB1 = (byte)(RegAF.High8 - TB1 - (RegFlagH ? 1 : 0)); RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											if (RegBC.Value16 != 0 && !RegFlagZ) {
												RegPC.Value16 -= 2;
												this.totalExecutedCycles += 21; this.pendingCycles -= 21;
											} else {
												this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											}
											break;
										case 0xB2: // INIR
											WriteMemory(RegHL.Value16++, ReadHardware(RegBC.Value16));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											if (RegBC.High8 != 0) {
												RegPC.Value16 -= 2;
												this.totalExecutedCycles += 21; this.pendingCycles -= 21;
											} else {
												this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											}
											break;
										case 0xB3: // OTIR
											WriteHardware(RegBC.Value16, ReadMemory(RegHL.Value16++));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											if (RegBC.High8 != 0) {
												RegPC.Value16 -= 2;
												this.totalExecutedCycles += 21; this.pendingCycles -= 21;
											} else {
												this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											}
											break;
										case 0xB4: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xB5: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xB6: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xB7: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xB8: // LDDR
											WriteMemory(RegDE.Value16--, TB1 = ReadMemory(RegHL.Value16--));
											TB1 += RegAF.High8; RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											RegFlagH = false;
											RegFlagN = false;
											if (RegBC.Value16 != 0) {
												RegPC.Value16 -= 2;
												this.totalExecutedCycles += 21; this.pendingCycles -= 21;
											} else {
												this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											}
											break;
										case 0xB9: // CPDR
											TB1 = ReadMemory(RegHL.Value16--); TB2 = (byte)(RegAF.High8 - TB1);
											RegFlagN = true;
											RegFlagH = TableHalfBorrow[RegAF.High8, TB1];
											RegFlagZ = TB2 == 0;
											RegFlagS = TB2 > 127;
											TB1 = (byte)(RegAF.High8 - TB1 - (RegFlagH ? 1 : 0)); RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											if (RegBC.Value16 != 0 && !RegFlagZ) {
												RegPC.Value16 -= 2;
												this.totalExecutedCycles += 21; this.pendingCycles -= 21;
											} else {
												this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											}
											break;
										case 0xBA: // INDR
											WriteMemory(RegHL.Value16--, ReadHardware(RegBC.Value16));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											if (RegBC.High8 != 0) {
												RegPC.Value16 -= 2;
												this.totalExecutedCycles += 21; this.pendingCycles -= 21;
											} else {
												this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											}
											break;
										case 0xBB: // OTDR
											WriteHardware(RegBC.Value16, ReadMemory(RegHL.Value16--));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											if (RegBC.High8 != 0) {
												RegPC.Value16 -= 2;
												this.totalExecutedCycles += 21; this.pendingCycles -= 21;
											} else {
												this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											}
											break;
										case 0xBC: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xBD: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xBE: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xBF: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xC0: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xC1: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xC2: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xC3: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xC4: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xC5: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xC6: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xC7: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xC8: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xC9: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xCA: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xCB: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xCC: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xCD: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xCE: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xCF: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xD0: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xD1: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xD2: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xD3: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xD4: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xD5: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xD6: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xD7: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xD8: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xD9: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xDA: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xDB: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xDC: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xDD: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xDE: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xDF: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xE0: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xE1: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xE2: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xE3: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xE4: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xE5: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xE6: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xE7: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xE8: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xE9: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xEA: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xEB: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xEC: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xED: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xEE: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xEF: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xF0: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xF1: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xF2: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xF3: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xF4: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xF5: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xF6: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xF7: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xF8: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xF9: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xFA: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xFB: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xFC: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xFD: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xFE: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0xFF: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
									}
									break;
								case 0xEE: // XOR n
									RegAF.Value16 = TableALU[5, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									break;
								case 0xEF: // RST $28
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x28;
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									break;
								case 0xF0: // RET P
									if (!RegFlagS) {
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
										this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									} else {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
									}
									break;
								case 0xF1: // POP AF
									RegAF.Low8 = ReadMemory(RegSP.Value16++); RegAF.High8 = ReadMemory(RegSP.Value16++);
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0xF2: // JP P, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagS) {
										RegPC.Value16 = TUS;
									}
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0xF3: // DI
									this.IFF1 = this.IFF2 = false;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xF4: // CALL P, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagS) {
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
										this.totalExecutedCycles += 17; this.pendingCycles -= 17;
									} else {
										this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									}
									break;
								case 0xF5: // PUSH AF
									WriteMemory(--RegSP.Value16, RegAF.High8); WriteMemory(--RegSP.Value16, RegAF.Low8);
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									break;
								case 0xF6: // OR n
									RegAF.Value16 = TableALU[6, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									break;
								case 0xF7: // RST $30
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x30;
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									break;
								case 0xF8: // RET M
									if (RegFlagS) {
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
										this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									} else {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
									}
									break;
								case 0xF9: // LD SP, IY
									RegSP.Value16 = RegIY.Value16;
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0xFA: // JP M, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagS) {
										RegPC.Value16 = TUS;
									}
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									break;
								case 0xFB: // EI
									this.IFF1 = this.IFF2 = true;
									Interruptable = false;
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0xFC: // CALL M, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagS) {
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
										this.totalExecutedCycles += 17; this.pendingCycles -= 17;
									} else {
										this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									}
									break;
								case 0xFD: // <-
									// Invalid sequence.
									this.totalExecutedCycles += 1337; this.pendingCycles -= 1337;
									break;
								case 0xFE: // CP n
									RegAF.Value16 = TableALU[7, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									break;
								case 0xFF: // RST $38
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x38;
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									break;
							}
							break;
						case 0xFE: // CP n
							RegAF.Value16 = TableALU[7, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							break;
						case 0xFF: // RST $38
							WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
							RegPC.Value16 = 0x38;
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							break;
					}

				}

				// Process interrupt requests.
				if (this.nonMaskableInterruptPending) {

					this.halted = false;

					this.totalExecutedCycles += 11; this.pendingCycles -= 11;
					this.nonMaskableInterruptPending = false;

					this.iff2 = this.iff1;
					this.iff1 = false;

					WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
					RegPC.Value16 = 0x66;
				
				} else if (this.iff1 && this.interrupt && Interruptable) {
				
					this.Halted = false;

					this.iff1 = this.iff2 = false;

					switch (interruptMode) {
						case 0:
							this.totalExecutedCycles += 13; this.pendingCycles -= 13;
							break;
						case 1:
							WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
							RegPC.Value16 = 0x38;
							this.totalExecutedCycles += 13; this.pendingCycles -= 13;
							break;
						case 2:
							TUS = (ushort)(RegI * 256 + 0);
							WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
							RegPC.Low8 = ReadMemory(TUS++); RegPC.High8 = ReadMemory(TUS);
							this.totalExecutedCycles += 19; this.pendingCycles -= 19;
							break;
					}
				}
			}
		}
	}
}