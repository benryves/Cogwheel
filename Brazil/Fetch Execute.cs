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
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							RegBC.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							break;
						case 0x02: // LD (BC), A
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							WriteMemory(RegBC.Value16, RegAF.High8);
							break;
						case 0x03: // INC BC
							this.totalExecutedCycles += 6; this.pendingCycles -= 6;
							++RegBC.Value16;
							break;
						case 0x04: // INC B
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Low8 = (byte)(TableInc[++RegBC.High8] | (RegAF.Low8 & 1));
							break;
						case 0x05: // DEC B
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Low8 = (byte)(TableDec[--RegBC.High8] | (RegAF.Low8 & 1));
							break;
						case 0x06: // LD B, n
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							RegBC.High8 = ReadMemory(RegPC.Value16++);
							break;
						case 0x07: // RLCA
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableRotShift[0, 0, RegAF.Value16];
							break;
						case 0x08: // EX AF, AF'
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							TUS = RegAF.Value16; RegAF.Value16 = RegAltAF.Value16; RegAltAF.Value16 = TUS;
							break;
						case 0x09: // ADD HL, BC
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							TI1 = (short)RegHL.Value16; TI2 = (short)RegBC.Value16; TIR = TI1 + TI2;
							TUS = (ushort)TIR;
							RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
							RegFlagN = false;
							RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
							RegHL.Value16 = TUS;
							RegFlag3 = (TUS & 0x0800) != 0;
							RegFlag5 = (TUS & 0x2000) != 0;
							break;
						case 0x0A: // LD A, (BC)
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							RegAF.High8 = ReadMemory(RegBC.Value16);
							break;
						case 0x0B: // DEC BC
							this.totalExecutedCycles += 6; this.pendingCycles -= 6;
							--RegBC.Value16;
							break;
						case 0x0C: // INC C
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Low8 = (byte)(TableInc[++RegBC.Low8] | (RegAF.Low8 & 1));
							break;
						case 0x0D: // DEC C
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Low8 = (byte)(TableDec[--RegBC.Low8] | (RegAF.Low8 & 1));
							break;
						case 0x0E: // LD C, n
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							RegBC.Low8 = ReadMemory(RegPC.Value16++);
							break;
						case 0x0F: // RRCA
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableRotShift[0, 1, RegAF.Value16];
							break;
						case 0x10: // DJNZ d
							TSB = (sbyte)ReadMemory(RegPC.Value16++);
							if (--RegBC.High8 != 0) {
								this.totalExecutedCycles += 13; this.pendingCycles -= 13;
								RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
							} else {
								this.totalExecutedCycles += 8; this.pendingCycles -= 8;
							}
							break;
						case 0x11: // LD DE, nn
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							RegDE.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							break;
						case 0x12: // LD (DE), A
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							WriteMemory(RegDE.Value16, RegAF.High8);
							break;
						case 0x13: // INC DE
							this.totalExecutedCycles += 6; this.pendingCycles -= 6;
							++RegDE.Value16;
							break;
						case 0x14: // INC D
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Low8 = (byte)(TableInc[++RegDE.High8] | (RegAF.Low8 & 1));
							break;
						case 0x15: // DEC D
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Low8 = (byte)(TableDec[--RegDE.High8] | (RegAF.Low8 & 1));
							break;
						case 0x16: // LD D, n
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							RegDE.High8 = ReadMemory(RegPC.Value16++);
							break;
						case 0x17: // RLA
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableRotShift[0, 2, RegAF.Value16];
							break;
						case 0x18: // JR d
							TSB = (sbyte)ReadMemory(RegPC.Value16++);
								this.totalExecutedCycles += 12; this.pendingCycles -= 12;
							RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
							break;
						case 0x19: // ADD HL, DE
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							TI1 = (short)RegHL.Value16; TI2 = (short)RegDE.Value16; TIR = TI1 + TI2;
							TUS = (ushort)TIR;
							RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
							RegFlagN = false;
							RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
							RegHL.Value16 = TUS;
							RegFlag3 = (TUS & 0x0800) != 0;
							RegFlag5 = (TUS & 0x2000) != 0;
							break;
						case 0x1A: // LD A, (DE)
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							RegAF.High8 = ReadMemory(RegDE.Value16);
							break;
						case 0x1B: // DEC DE
							this.totalExecutedCycles += 6; this.pendingCycles -= 6;
							--RegDE.Value16;
							break;
						case 0x1C: // INC E
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Low8 = (byte)(TableInc[++RegDE.Low8] | (RegAF.Low8 & 1));
							break;
						case 0x1D: // DEC E
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Low8 = (byte)(TableDec[--RegDE.Low8] | (RegAF.Low8 & 1));
							break;
						case 0x1E: // LD E, n
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							RegDE.Low8 = ReadMemory(RegPC.Value16++);
							break;
						case 0x1F: // RRA
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableRotShift[0, 3, RegAF.Value16];
							break;
						case 0x20: // JR NZ, d
							TSB = (sbyte)ReadMemory(RegPC.Value16++);
							if (!RegFlagZ) {
								this.totalExecutedCycles += 12; this.pendingCycles -= 12;
								RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
							} else {
								this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							}
							break;
						case 0x21: // LD HL, nn
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							RegHL.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							break;
						case 0x22: // LD (nn), HL
							this.totalExecutedCycles += 16; this.pendingCycles -= 16;
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							WriteMemory(TUS++, RegHL.Low8);
							WriteMemory(TUS, RegHL.High8);
							break;
						case 0x23: // INC HL
							this.totalExecutedCycles += 6; this.pendingCycles -= 6;
							++RegHL.Value16;
							break;
						case 0x24: // INC H
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Low8 = (byte)(TableInc[++RegHL.High8] | (RegAF.Low8 & 1));
							break;
						case 0x25: // DEC H
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Low8 = (byte)(TableDec[--RegHL.High8] | (RegAF.Low8 & 1));
							break;
						case 0x26: // LD H, n
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							RegHL.High8 = ReadMemory(RegPC.Value16++);
							break;
						case 0x27: // DAA
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableDaa[RegAF.Value16];
							break;
						case 0x28: // JR Z, d
							TSB = (sbyte)ReadMemory(RegPC.Value16++);
							if (RegFlagZ) {
								this.totalExecutedCycles += 12; this.pendingCycles -= 12;
								RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
							} else {
								this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							}
							break;
						case 0x29: // ADD HL, HL
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							TI1 = (short)RegHL.Value16; TI2 = (short)RegHL.Value16; TIR = TI1 + TI2;
							TUS = (ushort)TIR;
							RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
							RegFlagN = false;
							RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
							RegHL.Value16 = TUS;
							RegFlag3 = (TUS & 0x0800) != 0;
							RegFlag5 = (TUS & 0x2000) != 0;
							break;
						case 0x2A: // LD HL, (nn)
							this.totalExecutedCycles += 16; this.pendingCycles -= 16;
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							RegHL.Low8 = ReadMemory(TUS++); RegHL.High8 = ReadMemory(TUS);
							break;
						case 0x2B: // DEC HL
							this.totalExecutedCycles += 6; this.pendingCycles -= 6;
							--RegHL.Value16;
							break;
						case 0x2C: // INC L
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Low8 = (byte)(TableInc[++RegHL.Low8] | (RegAF.Low8 & 1));
							break;
						case 0x2D: // DEC L
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Low8 = (byte)(TableDec[--RegHL.Low8] | (RegAF.Low8 & 1));
							break;
						case 0x2E: // LD L, n
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							RegHL.Low8 = ReadMemory(RegPC.Value16++);
							break;
						case 0x2F: // CPL
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.High8 ^= 0xFF; RegFlagH = true; RegFlagN = true; RegFlag3 = (RegAF.High8 & 0x08) != 0; RegFlag5 = (RegAF.High8 & 0x20) != 0;
							break;
						case 0x30: // JR NC, d
							TSB = (sbyte)ReadMemory(RegPC.Value16++);
							if (!RegFlagC) {
								this.totalExecutedCycles += 12; this.pendingCycles -= 12;
								RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
							} else {
								this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							}
							break;
						case 0x31: // LD SP, nn
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							RegSP.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							break;
						case 0x32: // LD (nn), A
							this.totalExecutedCycles += 13; this.pendingCycles -= 13;
							WriteMemory((ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256), RegAF.High8);
							break;
						case 0x33: // INC SP
							this.totalExecutedCycles += 6; this.pendingCycles -= 6;
							++RegSP.Value16;
							break;
						case 0x34: // INC (HL)
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							TB = ReadMemory(RegHL.Value16); RegAF.Low8 = (byte)(TableInc[++TB] | (RegAF.Low8 & 1)); WriteMemory(RegHL.Value16, TB);
							break;
						case 0x35: // DEC (HL)
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							TB = ReadMemory(RegHL.Value16); RegAF.Low8 = (byte)(TableDec[--TB] | (RegAF.Low8 & 1)); WriteMemory(RegHL.Value16, TB);
							break;
						case 0x36: // LD (HL), n
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							WriteMemory(RegHL.Value16, ReadMemory(RegPC.Value16++));
							break;
						case 0x37: // SCF
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegFlagH = false; RegFlagN = false; RegFlagC = true; RegFlag3 = (RegAF.High8 & 0x08) != 0; RegFlag5 = (RegAF.High8 & 0x20) != 0;
							break;
						case 0x38: // JR C, d
							TSB = (sbyte)ReadMemory(RegPC.Value16++);
							if (RegFlagC) {
								this.totalExecutedCycles += 12; this.pendingCycles -= 12;
								RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
							} else {
								this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							}
							break;
						case 0x39: // ADD HL, SP
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							TI1 = (short)RegHL.Value16; TI2 = (short)RegSP.Value16; TIR = TI1 + TI2;
							TUS = (ushort)TIR;
							RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
							RegFlagN = false;
							RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
							RegHL.Value16 = TUS;
							RegFlag3 = (TUS & 0x0800) != 0;
							RegFlag5 = (TUS & 0x2000) != 0;
							break;
						case 0x3A: // LD A, (nn)
							this.totalExecutedCycles += 13; this.pendingCycles -= 13;
							RegAF.High8 = ReadMemory((ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256));
							break;
						case 0x3B: // DEC SP
							this.totalExecutedCycles += 6; this.pendingCycles -= 6;
							--RegSP.Value16;
							break;
						case 0x3C: // INC A
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Low8 = (byte)(TableInc[++RegAF.High8] | (RegAF.Low8 & 1));
							break;
						case 0x3D: // DEC A
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Low8 = (byte)(TableDec[--RegAF.High8] | (RegAF.Low8 & 1));
							break;
						case 0x3E: // LD A, n
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							RegAF.High8 = ReadMemory(RegPC.Value16++);
							break;
						case 0x3F: // CCF
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegFlagH = RegFlagC; RegFlagN = false; RegFlagC ^= true; RegFlag3 = (RegAF.High8 & 0x08) != 0; RegFlag5 = (RegAF.High8 & 0x20) != 0;
							break;
						case 0x40: // LD B, B
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x41: // LD B, C
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegBC.High8 = RegBC.Low8;
							break;
						case 0x42: // LD B, D
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegBC.High8 = RegDE.High8;
							break;
						case 0x43: // LD B, E
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegBC.High8 = RegDE.Low8;
							break;
						case 0x44: // LD B, H
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegBC.High8 = RegHL.High8;
							break;
						case 0x45: // LD B, L
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegBC.High8 = RegHL.Low8;
							break;
						case 0x46: // LD B, (HL)
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							RegBC.High8 = ReadMemory(RegHL.Value16);
							break;
						case 0x47: // LD B, A
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegBC.High8 = RegAF.High8;
							break;
						case 0x48: // LD C, B
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegBC.Low8 = RegBC.High8;
							break;
						case 0x49: // LD C, C
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x4A: // LD C, D
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegBC.Low8 = RegDE.High8;
							break;
						case 0x4B: // LD C, E
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegBC.Low8 = RegDE.Low8;
							break;
						case 0x4C: // LD C, H
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegBC.Low8 = RegHL.High8;
							break;
						case 0x4D: // LD C, L
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegBC.Low8 = RegHL.Low8;
							break;
						case 0x4E: // LD C, (HL)
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							RegBC.Low8 = ReadMemory(RegHL.Value16);
							break;
						case 0x4F: // LD C, A
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegBC.Low8 = RegAF.High8;
							break;
						case 0x50: // LD D, B
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegDE.High8 = RegBC.High8;
							break;
						case 0x51: // LD D, C
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegDE.High8 = RegBC.Low8;
							break;
						case 0x52: // LD D, D
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x53: // LD D, E
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegDE.High8 = RegDE.Low8;
							break;
						case 0x54: // LD D, H
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegDE.High8 = RegHL.High8;
							break;
						case 0x55: // LD D, L
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegDE.High8 = RegHL.Low8;
							break;
						case 0x56: // LD D, (HL)
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							RegDE.High8 = ReadMemory(RegHL.Value16);
							break;
						case 0x57: // LD D, A
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegDE.High8 = RegAF.High8;
							break;
						case 0x58: // LD E, B
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegDE.Low8 = RegBC.High8;
							break;
						case 0x59: // LD E, C
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegDE.Low8 = RegBC.Low8;
							break;
						case 0x5A: // LD E, D
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegDE.Low8 = RegDE.High8;
							break;
						case 0x5B: // LD E, E
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x5C: // LD E, H
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegDE.Low8 = RegHL.High8;
							break;
						case 0x5D: // LD E, L
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegDE.Low8 = RegHL.Low8;
							break;
						case 0x5E: // LD E, (HL)
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							RegDE.Low8 = ReadMemory(RegHL.Value16);
							break;
						case 0x5F: // LD E, A
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegDE.Low8 = RegAF.High8;
							break;
						case 0x60: // LD H, B
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegHL.High8 = RegBC.High8;
							break;
						case 0x61: // LD H, C
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegHL.High8 = RegBC.Low8;
							break;
						case 0x62: // LD H, D
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegHL.High8 = RegDE.High8;
							break;
						case 0x63: // LD H, E
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegHL.High8 = RegDE.Low8;
							break;
						case 0x64: // LD H, H
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x65: // LD H, L
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegHL.High8 = RegHL.Low8;
							break;
						case 0x66: // LD H, (HL)
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							RegHL.High8 = ReadMemory(RegHL.Value16);
							break;
						case 0x67: // LD H, A
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegHL.High8 = RegAF.High8;
							break;
						case 0x68: // LD L, B
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegHL.Low8 = RegBC.High8;
							break;
						case 0x69: // LD L, C
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegHL.Low8 = RegBC.Low8;
							break;
						case 0x6A: // LD L, D
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegHL.Low8 = RegDE.High8;
							break;
						case 0x6B: // LD L, E
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegHL.Low8 = RegDE.Low8;
							break;
						case 0x6C: // LD L, H
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegHL.Low8 = RegHL.High8;
							break;
						case 0x6D: // LD L, L
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x6E: // LD L, (HL)
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							RegHL.Low8 = ReadMemory(RegHL.Value16);
							break;
						case 0x6F: // LD L, A
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegHL.Low8 = RegAF.High8;
							break;
						case 0x70: // LD (HL), B
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							WriteMemory(RegHL.Value16, RegBC.High8);
							break;
						case 0x71: // LD (HL), C
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							WriteMemory(RegHL.Value16, RegBC.Low8);
							break;
						case 0x72: // LD (HL), D
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							WriteMemory(RegHL.Value16, RegDE.High8);
							break;
						case 0x73: // LD (HL), E
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							WriteMemory(RegHL.Value16, RegDE.Low8);
							break;
						case 0x74: // LD (HL), H
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							WriteMemory(RegHL.Value16, RegHL.High8);
							break;
						case 0x75: // LD (HL), L
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							WriteMemory(RegHL.Value16, RegHL.Low8);
							break;
						case 0x76: // HALT
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							this.Halt();
							break;
						case 0x77: // LD (HL), A
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							WriteMemory(RegHL.Value16, RegAF.High8);
							break;
						case 0x78: // LD A, B
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.High8 = RegBC.High8;
							break;
						case 0x79: // LD A, C
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.High8 = RegBC.Low8;
							break;
						case 0x7A: // LD A, D
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.High8 = RegDE.High8;
							break;
						case 0x7B: // LD A, E
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.High8 = RegDE.Low8;
							break;
						case 0x7C: // LD A, H
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.High8 = RegHL.High8;
							break;
						case 0x7D: // LD A, L
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.High8 = RegHL.Low8;
							break;
						case 0x7E: // LD A, (HL)
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							RegAF.High8 = ReadMemory(RegHL.Value16);
							break;
						case 0x7F: // LD A, A
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							break;
						case 0x80: // ADD A, B
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[0, RegAF.High8, RegBC.High8, 0];
							break;
						case 0x81: // ADD A, C
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[0, RegAF.High8, RegBC.Low8, 0];
							break;
						case 0x82: // ADD A, D
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[0, RegAF.High8, RegDE.High8, 0];
							break;
						case 0x83: // ADD A, E
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[0, RegAF.High8, RegDE.Low8, 0];
							break;
						case 0x84: // ADD A, H
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[0, RegAF.High8, RegHL.High8, 0];
							break;
						case 0x85: // ADD A, L
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[0, RegAF.High8, RegHL.Low8, 0];
							break;
						case 0x86: // ADD A, (HL)
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							RegAF.Value16 = TableALU[0, RegAF.High8, ReadMemory(RegHL.Value16), 0];
							break;
						case 0x87: // ADD A, A
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[0, RegAF.High8, RegAF.High8, 0];
							break;
						case 0x88: // ADC A, B
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[1, RegAF.High8, RegBC.High8, RegFlagC ? 1 : 0];
							break;
						case 0x89: // ADC A, C
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[1, RegAF.High8, RegBC.Low8, RegFlagC ? 1 : 0];
							break;
						case 0x8A: // ADC A, D
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[1, RegAF.High8, RegDE.High8, RegFlagC ? 1 : 0];
							break;
						case 0x8B: // ADC A, E
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[1, RegAF.High8, RegDE.Low8, RegFlagC ? 1 : 0];
							break;
						case 0x8C: // ADC A, H
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[1, RegAF.High8, RegHL.High8, RegFlagC ? 1 : 0];
							break;
						case 0x8D: // ADC A, L
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[1, RegAF.High8, RegHL.Low8, RegFlagC ? 1 : 0];
							break;
						case 0x8E: // ADC A, (HL)
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							RegAF.Value16 = TableALU[1, RegAF.High8, ReadMemory(RegHL.Value16), RegFlagC ? 1 : 0];
							break;
						case 0x8F: // ADC A, A
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[1, RegAF.High8, RegAF.High8, RegFlagC ? 1 : 0];
							break;
						case 0x90: // SUB B
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[2, RegAF.High8, RegBC.High8, 0];
							break;
						case 0x91: // SUB C
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[2, RegAF.High8, RegBC.Low8, 0];
							break;
						case 0x92: // SUB D
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[2, RegAF.High8, RegDE.High8, 0];
							break;
						case 0x93: // SUB E
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[2, RegAF.High8, RegDE.Low8, 0];
							break;
						case 0x94: // SUB H
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[2, RegAF.High8, RegHL.High8, 0];
							break;
						case 0x95: // SUB L
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[2, RegAF.High8, RegHL.Low8, 0];
							break;
						case 0x96: // SUB (HL)
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							RegAF.Value16 = TableALU[2, RegAF.High8, ReadMemory(RegHL.Value16), 0];
							break;
						case 0x97: // SUB A, A
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[2, RegAF.High8, RegAF.High8, 0];
							break;
						case 0x98: // SBC A, B
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[3, RegAF.High8, RegBC.High8, RegFlagC ? 1 : 0];
							break;
						case 0x99: // SBC A, C
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[3, RegAF.High8, RegBC.Low8, RegFlagC ? 1 : 0];
							break;
						case 0x9A: // SBC A, D
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[3, RegAF.High8, RegDE.High8, RegFlagC ? 1 : 0];
							break;
						case 0x9B: // SBC A, E
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[3, RegAF.High8, RegDE.Low8, RegFlagC ? 1 : 0];
							break;
						case 0x9C: // SBC A, H
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[3, RegAF.High8, RegHL.High8, RegFlagC ? 1 : 0];
							break;
						case 0x9D: // SBC A, L
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[3, RegAF.High8, RegHL.Low8, RegFlagC ? 1 : 0];
							break;
						case 0x9E: // SBC A, (HL)
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							RegAF.Value16 = TableALU[3, RegAF.High8, ReadMemory(RegHL.Value16), RegFlagC ? 1 : 0];
							break;
						case 0x9F: // SBC A, A
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[3, RegAF.High8, RegAF.High8, RegFlagC ? 1 : 0];
							break;
						case 0xA0: // AND B
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[4, RegAF.High8, RegBC.High8, 0];
							break;
						case 0xA1: // AND C
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[4, RegAF.High8, RegBC.Low8, 0];
							break;
						case 0xA2: // AND D
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[4, RegAF.High8, RegDE.High8, 0];
							break;
						case 0xA3: // AND E
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[4, RegAF.High8, RegDE.Low8, 0];
							break;
						case 0xA4: // AND H
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[4, RegAF.High8, RegHL.High8, 0];
							break;
						case 0xA5: // AND L
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[4, RegAF.High8, RegHL.Low8, 0];
							break;
						case 0xA6: // AND (HL)
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							RegAF.Value16 = TableALU[4, RegAF.High8, ReadMemory(RegHL.Value16), 0];
							break;
						case 0xA7: // AND A
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[4, RegAF.High8, RegAF.High8, 0];
							break;
						case 0xA8: // XOR B
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[5, RegAF.High8, RegBC.High8, 0];
							break;
						case 0xA9: // XOR C
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[5, RegAF.High8, RegBC.Low8, 0];
							break;
						case 0xAA: // XOR D
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[5, RegAF.High8, RegDE.High8, 0];
							break;
						case 0xAB: // XOR E
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[5, RegAF.High8, RegDE.Low8, 0];
							break;
						case 0xAC: // XOR H
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[5, RegAF.High8, RegHL.High8, 0];
							break;
						case 0xAD: // XOR L
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[5, RegAF.High8, RegHL.Low8, 0];
							break;
						case 0xAE: // XOR (HL)
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							RegAF.Value16 = TableALU[5, RegAF.High8, ReadMemory(RegHL.Value16), 0];
							break;
						case 0xAF: // XOR A
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[5, RegAF.High8, RegAF.High8, 0];
							break;
						case 0xB0: // OR B
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[6, RegAF.High8, RegBC.High8, 0];
							break;
						case 0xB1: // OR C
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[6, RegAF.High8, RegBC.Low8, 0];
							break;
						case 0xB2: // OR D
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[6, RegAF.High8, RegDE.High8, 0];
							break;
						case 0xB3: // OR E
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[6, RegAF.High8, RegDE.Low8, 0];
							break;
						case 0xB4: // OR H
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[6, RegAF.High8, RegHL.High8, 0];
							break;
						case 0xB5: // OR L
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[6, RegAF.High8, RegHL.Low8, 0];
							break;
						case 0xB6: // OR (HL)
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							RegAF.Value16 = TableALU[6, RegAF.High8, ReadMemory(RegHL.Value16), 0];
							break;
						case 0xB7: // OR A
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[6, RegAF.High8, RegAF.High8, 0];
							break;
						case 0xB8: // CP B
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[7, RegAF.High8, RegBC.High8, 0];
							break;
						case 0xB9: // CP C
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[7, RegAF.High8, RegBC.Low8, 0];
							break;
						case 0xBA: // CP D
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[7, RegAF.High8, RegDE.High8, 0];
							break;
						case 0xBB: // CP E
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[7, RegAF.High8, RegDE.Low8, 0];
							break;
						case 0xBC: // CP H
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[7, RegAF.High8, RegHL.High8, 0];
							break;
						case 0xBD: // CP L
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[7, RegAF.High8, RegHL.Low8, 0];
							break;
						case 0xBE: // CP (HL)
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							RegAF.Value16 = TableALU[7, RegAF.High8, ReadMemory(RegHL.Value16), 0];
							break;
						case 0xBF: // CP A
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegAF.Value16 = TableALU[7, RegAF.High8, RegAF.High8, 0];
							break;
						case 0xC0: // RET NZ
							if (!RegFlagZ) {
								this.totalExecutedCycles += 11; this.pendingCycles -= 11;
								RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
							} else {
								this.totalExecutedCycles += 5; this.pendingCycles -= 5;
							}
							break;
						case 0xC1: // POP BC
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							RegBC.Low8 = ReadMemory(RegSP.Value16++); RegBC.High8 = ReadMemory(RegSP.Value16++);
							break;
						case 0xC2: // JP NZ, nn
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (!RegFlagZ) {
								RegPC.Value16 = TUS;
							}
							break;
						case 0xC3: // JP nn
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							RegPC.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							break;
						case 0xC4: // CALL NZ, nn
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (!RegFlagZ) {
								this.totalExecutedCycles += 17; this.pendingCycles -= 17;
								WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
								RegPC.Value16 = TUS;
							} else {
								this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							}
							break;
						case 0xC5: // PUSH BC
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							WriteMemory(--RegSP.Value16, RegBC.High8); WriteMemory(--RegSP.Value16, RegBC.Low8);
							break;
						case 0xC6: // ADD A, n
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							RegAF.Value16 = TableALU[0, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
							break;
						case 0xC7: // RST $00
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
							RegPC.Value16 = 0x00;
							break;
						case 0xC8: // RET Z
							if (RegFlagZ) {
								this.totalExecutedCycles += 11; this.pendingCycles -= 11;
								RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
							} else {
								this.totalExecutedCycles += 5; this.pendingCycles -= 5;
							}
							break;
						case 0xC9: // RET
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
							break;
						case 0xCA: // JP Z, nn
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (RegFlagZ) {
								RegPC.Value16 = TUS;
							}
							break;
						case 0xCB: // (Prefix)
							++RegR;
							switch (ReadMemory(RegPC.Value16++)) {
								case 0x00: // RLC B
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * RegBC.High8];
									RegBC.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x01: // RLC C
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * RegBC.Low8];
									RegBC.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x02: // RLC D
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * RegDE.High8];
									RegDE.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x03: // RLC E
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * RegDE.Low8];
									RegDE.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x04: // RLC H
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * RegHL.High8];
									RegHL.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x05: // RLC L
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * RegHL.Low8];
									RegHL.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x06: // RLC (HL)
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory(RegHL.Value16)];
									WriteMemory(RegHL.Value16, (byte)(TUS >> 8));
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x07: // RLC A
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * RegAF.High8];
									RegAF.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x08: // RRC B
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * RegBC.High8];
									RegBC.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x09: // RRC C
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * RegBC.Low8];
									RegBC.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x0A: // RRC D
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * RegDE.High8];
									RegDE.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x0B: // RRC E
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * RegDE.Low8];
									RegDE.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x0C: // RRC H
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * RegHL.High8];
									RegHL.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x0D: // RRC L
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * RegHL.Low8];
									RegHL.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x0E: // RRC (HL)
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory(RegHL.Value16)];
									WriteMemory(RegHL.Value16, (byte)(TUS >> 8));
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x0F: // RRC A
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * RegAF.High8];
									RegAF.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x10: // RL B
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * RegBC.High8];
									RegBC.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x11: // RL C
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * RegBC.Low8];
									RegBC.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x12: // RL D
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * RegDE.High8];
									RegDE.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x13: // RL E
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * RegDE.Low8];
									RegDE.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x14: // RL H
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * RegHL.High8];
									RegHL.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x15: // RL L
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * RegHL.Low8];
									RegHL.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x16: // RL (HL)
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory(RegHL.Value16)];
									WriteMemory(RegHL.Value16, (byte)(TUS >> 8));
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x17: // RL A
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * RegAF.High8];
									RegAF.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x18: // RR B
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * RegBC.High8];
									RegBC.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x19: // RR C
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * RegBC.Low8];
									RegBC.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x1A: // RR D
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * RegDE.High8];
									RegDE.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x1B: // RR E
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * RegDE.Low8];
									RegDE.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x1C: // RR H
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * RegHL.High8];
									RegHL.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x1D: // RR L
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * RegHL.Low8];
									RegHL.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x1E: // RR (HL)
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory(RegHL.Value16)];
									WriteMemory(RegHL.Value16, (byte)(TUS >> 8));
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x1F: // RR A
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * RegAF.High8];
									RegAF.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x20: // SLA B
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * RegBC.High8];
									RegBC.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x21: // SLA C
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * RegBC.Low8];
									RegBC.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x22: // SLA D
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * RegDE.High8];
									RegDE.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x23: // SLA E
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * RegDE.Low8];
									RegDE.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x24: // SLA H
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * RegHL.High8];
									RegHL.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x25: // SLA L
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * RegHL.Low8];
									RegHL.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x26: // SLA (HL)
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory(RegHL.Value16)];
									WriteMemory(RegHL.Value16, (byte)(TUS >> 8));
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x27: // SLA A
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * RegAF.High8];
									RegAF.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x28: // SRA B
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * RegBC.High8];
									RegBC.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x29: // SRA C
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * RegBC.Low8];
									RegBC.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x2A: // SRA D
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * RegDE.High8];
									RegDE.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x2B: // SRA E
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * RegDE.Low8];
									RegDE.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x2C: // SRA H
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * RegHL.High8];
									RegHL.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x2D: // SRA L
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * RegHL.Low8];
									RegHL.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x2E: // SRA (HL)
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory(RegHL.Value16)];
									WriteMemory(RegHL.Value16, (byte)(TUS >> 8));
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x2F: // SRA A
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * RegAF.High8];
									RegAF.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x30: // SL1 B
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * RegBC.High8];
									RegBC.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x31: // SL1 C
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * RegBC.Low8];
									RegBC.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x32: // SL1 D
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * RegDE.High8];
									RegDE.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x33: // SL1 E
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * RegDE.Low8];
									RegDE.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x34: // SL1 H
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * RegHL.High8];
									RegHL.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x35: // SL1 L
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * RegHL.Low8];
									RegHL.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x36: // SL1 (HL)
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory(RegHL.Value16)];
									WriteMemory(RegHL.Value16, (byte)(TUS >> 8));
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x37: // SL1 A
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * RegAF.High8];
									RegAF.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x38: // SRL B
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * RegBC.High8];
									RegBC.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x39: // SRL C
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * RegBC.Low8];
									RegBC.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x3A: // SRL D
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * RegDE.High8];
									RegDE.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x3B: // SRL E
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * RegDE.Low8];
									RegDE.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x3C: // SRL H
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * RegHL.High8];
									RegHL.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x3D: // SRL L
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * RegHL.Low8];
									RegHL.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x3E: // SRL (HL)
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory(RegHL.Value16)];
									WriteMemory(RegHL.Value16, (byte)(TUS >> 8));
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x3F: // SRL A
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * RegAF.High8];
									RegAF.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x40: // BIT 0, B
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegBC.High8 & 0x01) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x41: // BIT 0, C
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegBC.Low8 & 0x01) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x42: // BIT 0, D
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegDE.High8 & 0x01) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x43: // BIT 0, E
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegDE.Low8 & 0x01) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x44: // BIT 0, H
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegHL.High8 & 0x01) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x45: // BIT 0, L
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegHL.Low8 & 0x01) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x46: // BIT 0, (HL)
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									RegFlagZ = (ReadMemory(RegHL.Value16) & 0x01) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x47: // BIT 0, A
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegAF.High8 & 0x01) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x48: // BIT 1, B
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegBC.High8 & 0x02) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x49: // BIT 1, C
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegBC.Low8 & 0x02) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x4A: // BIT 1, D
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegDE.High8 & 0x02) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x4B: // BIT 1, E
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegDE.Low8 & 0x02) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x4C: // BIT 1, H
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegHL.High8 & 0x02) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x4D: // BIT 1, L
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegHL.Low8 & 0x02) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x4E: // BIT 1, (HL)
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									RegFlagZ = (ReadMemory(RegHL.Value16) & 0x02) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x4F: // BIT 1, A
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegAF.High8 & 0x02) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x50: // BIT 2, B
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegBC.High8 & 0x04) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x51: // BIT 2, C
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegBC.Low8 & 0x04) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x52: // BIT 2, D
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegDE.High8 & 0x04) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x53: // BIT 2, E
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegDE.Low8 & 0x04) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x54: // BIT 2, H
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegHL.High8 & 0x04) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x55: // BIT 2, L
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegHL.Low8 & 0x04) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x56: // BIT 2, (HL)
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									RegFlagZ = (ReadMemory(RegHL.Value16) & 0x04) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x57: // BIT 2, A
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegAF.High8 & 0x04) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x58: // BIT 3, B
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegBC.High8 & 0x08) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = !RegFlagZ;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x59: // BIT 3, C
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegBC.Low8 & 0x08) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = !RegFlagZ;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x5A: // BIT 3, D
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegDE.High8 & 0x08) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = !RegFlagZ;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x5B: // BIT 3, E
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegDE.Low8 & 0x08) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = !RegFlagZ;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x5C: // BIT 3, H
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegHL.High8 & 0x08) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = !RegFlagZ;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x5D: // BIT 3, L
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegHL.Low8 & 0x08) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = !RegFlagZ;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x5E: // BIT 3, (HL)
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									RegFlagZ = (ReadMemory(RegHL.Value16) & 0x08) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = !RegFlagZ;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x5F: // BIT 3, A
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegAF.High8 & 0x08) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = !RegFlagZ;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x60: // BIT 4, B
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegBC.High8 & 0x10) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x61: // BIT 4, C
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegBC.Low8 & 0x10) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x62: // BIT 4, D
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegDE.High8 & 0x10) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x63: // BIT 4, E
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegDE.Low8 & 0x10) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x64: // BIT 4, H
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegHL.High8 & 0x10) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x65: // BIT 4, L
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegHL.Low8 & 0x10) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x66: // BIT 4, (HL)
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									RegFlagZ = (ReadMemory(RegHL.Value16) & 0x10) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x67: // BIT 4, A
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegAF.High8 & 0x10) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x68: // BIT 5, B
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegBC.High8 & 0x20) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = !RegFlagZ;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x69: // BIT 5, C
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegBC.Low8 & 0x20) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = !RegFlagZ;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x6A: // BIT 5, D
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegDE.High8 & 0x20) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = !RegFlagZ;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x6B: // BIT 5, E
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegDE.Low8 & 0x20) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = !RegFlagZ;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x6C: // BIT 5, H
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegHL.High8 & 0x20) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = !RegFlagZ;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x6D: // BIT 5, L
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegHL.Low8 & 0x20) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = !RegFlagZ;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x6E: // BIT 5, (HL)
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									RegFlagZ = (ReadMemory(RegHL.Value16) & 0x20) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = !RegFlagZ;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x6F: // BIT 5, A
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegAF.High8 & 0x20) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = !RegFlagZ;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x70: // BIT 6, B
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegBC.High8 & 0x40) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x71: // BIT 6, C
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegBC.Low8 & 0x40) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x72: // BIT 6, D
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegDE.High8 & 0x40) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x73: // BIT 6, E
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegDE.Low8 & 0x40) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x74: // BIT 6, H
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegHL.High8 & 0x40) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x75: // BIT 6, L
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegHL.Low8 & 0x40) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x76: // BIT 6, (HL)
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									RegFlagZ = (ReadMemory(RegHL.Value16) & 0x40) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x77: // BIT 6, A
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegAF.High8 & 0x40) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x78: // BIT 7, B
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegBC.High8 & 0x80) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = !RegFlagZ;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x79: // BIT 7, C
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegBC.Low8 & 0x80) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = !RegFlagZ;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x7A: // BIT 7, D
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegDE.High8 & 0x80) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = !RegFlagZ;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x7B: // BIT 7, E
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegDE.Low8 & 0x80) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = !RegFlagZ;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x7C: // BIT 7, H
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegHL.High8 & 0x80) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = !RegFlagZ;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x7D: // BIT 7, L
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegHL.Low8 & 0x80) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = !RegFlagZ;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x7E: // BIT 7, (HL)
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									RegFlagZ = (ReadMemory(RegHL.Value16) & 0x80) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = !RegFlagZ;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x7F: // BIT 7, A
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegFlagZ = (RegAF.High8 & 0x80) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = !RegFlagZ;
									RegFlag3 = false;
									RegFlag5 = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x80: // RES 0, B
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegBC.High8 &= unchecked((byte)~0x01);
									break;
								case 0x81: // RES 0, C
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegBC.Low8 &= unchecked((byte)~0x01);
									break;
								case 0x82: // RES 0, D
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegDE.High8 &= unchecked((byte)~0x01);
									break;
								case 0x83: // RES 0, E
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegDE.Low8 &= unchecked((byte)~0x01);
									break;
								case 0x84: // RES 0, H
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegHL.High8 &= unchecked((byte)~0x01);
									break;
								case 0x85: // RES 0, L
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegHL.Low8 &= unchecked((byte)~0x01);
									break;
								case 0x86: // RES 0, (HL)
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) & unchecked((byte)~0x01)));
									break;
								case 0x87: // RES 0, A
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegAF.High8 &= unchecked((byte)~0x01);
									break;
								case 0x88: // RES 1, B
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegBC.High8 &= unchecked((byte)~0x02);
									break;
								case 0x89: // RES 1, C
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegBC.Low8 &= unchecked((byte)~0x02);
									break;
								case 0x8A: // RES 1, D
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegDE.High8 &= unchecked((byte)~0x02);
									break;
								case 0x8B: // RES 1, E
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegDE.Low8 &= unchecked((byte)~0x02);
									break;
								case 0x8C: // RES 1, H
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegHL.High8 &= unchecked((byte)~0x02);
									break;
								case 0x8D: // RES 1, L
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegHL.Low8 &= unchecked((byte)~0x02);
									break;
								case 0x8E: // RES 1, (HL)
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) & unchecked((byte)~0x02)));
									break;
								case 0x8F: // RES 1, A
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegAF.High8 &= unchecked((byte)~0x02);
									break;
								case 0x90: // RES 2, B
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegBC.High8 &= unchecked((byte)~0x04);
									break;
								case 0x91: // RES 2, C
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegBC.Low8 &= unchecked((byte)~0x04);
									break;
								case 0x92: // RES 2, D
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegDE.High8 &= unchecked((byte)~0x04);
									break;
								case 0x93: // RES 2, E
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegDE.Low8 &= unchecked((byte)~0x04);
									break;
								case 0x94: // RES 2, H
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegHL.High8 &= unchecked((byte)~0x04);
									break;
								case 0x95: // RES 2, L
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegHL.Low8 &= unchecked((byte)~0x04);
									break;
								case 0x96: // RES 2, (HL)
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) & unchecked((byte)~0x04)));
									break;
								case 0x97: // RES 2, A
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegAF.High8 &= unchecked((byte)~0x04);
									break;
								case 0x98: // RES 3, B
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegBC.High8 &= unchecked((byte)~0x08);
									break;
								case 0x99: // RES 3, C
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegBC.Low8 &= unchecked((byte)~0x08);
									break;
								case 0x9A: // RES 3, D
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegDE.High8 &= unchecked((byte)~0x08);
									break;
								case 0x9B: // RES 3, E
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegDE.Low8 &= unchecked((byte)~0x08);
									break;
								case 0x9C: // RES 3, H
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegHL.High8 &= unchecked((byte)~0x08);
									break;
								case 0x9D: // RES 3, L
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegHL.Low8 &= unchecked((byte)~0x08);
									break;
								case 0x9E: // RES 3, (HL)
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) & unchecked((byte)~0x08)));
									break;
								case 0x9F: // RES 3, A
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegAF.High8 &= unchecked((byte)~0x08);
									break;
								case 0xA0: // RES 4, B
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegBC.High8 &= unchecked((byte)~0x10);
									break;
								case 0xA1: // RES 4, C
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegBC.Low8 &= unchecked((byte)~0x10);
									break;
								case 0xA2: // RES 4, D
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegDE.High8 &= unchecked((byte)~0x10);
									break;
								case 0xA3: // RES 4, E
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegDE.Low8 &= unchecked((byte)~0x10);
									break;
								case 0xA4: // RES 4, H
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegHL.High8 &= unchecked((byte)~0x10);
									break;
								case 0xA5: // RES 4, L
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegHL.Low8 &= unchecked((byte)~0x10);
									break;
								case 0xA6: // RES 4, (HL)
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) & unchecked((byte)~0x10)));
									break;
								case 0xA7: // RES 4, A
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegAF.High8 &= unchecked((byte)~0x10);
									break;
								case 0xA8: // RES 5, B
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegBC.High8 &= unchecked((byte)~0x20);
									break;
								case 0xA9: // RES 5, C
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegBC.Low8 &= unchecked((byte)~0x20);
									break;
								case 0xAA: // RES 5, D
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegDE.High8 &= unchecked((byte)~0x20);
									break;
								case 0xAB: // RES 5, E
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegDE.Low8 &= unchecked((byte)~0x20);
									break;
								case 0xAC: // RES 5, H
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegHL.High8 &= unchecked((byte)~0x20);
									break;
								case 0xAD: // RES 5, L
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegHL.Low8 &= unchecked((byte)~0x20);
									break;
								case 0xAE: // RES 5, (HL)
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) & unchecked((byte)~0x20)));
									break;
								case 0xAF: // RES 5, A
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegAF.High8 &= unchecked((byte)~0x20);
									break;
								case 0xB0: // RES 6, B
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegBC.High8 &= unchecked((byte)~0x40);
									break;
								case 0xB1: // RES 6, C
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegBC.Low8 &= unchecked((byte)~0x40);
									break;
								case 0xB2: // RES 6, D
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegDE.High8 &= unchecked((byte)~0x40);
									break;
								case 0xB3: // RES 6, E
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegDE.Low8 &= unchecked((byte)~0x40);
									break;
								case 0xB4: // RES 6, H
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegHL.High8 &= unchecked((byte)~0x40);
									break;
								case 0xB5: // RES 6, L
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegHL.Low8 &= unchecked((byte)~0x40);
									break;
								case 0xB6: // RES 6, (HL)
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) & unchecked((byte)~0x40)));
									break;
								case 0xB7: // RES 6, A
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegAF.High8 &= unchecked((byte)~0x40);
									break;
								case 0xB8: // RES 7, B
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegBC.High8 &= unchecked((byte)~0x80);
									break;
								case 0xB9: // RES 7, C
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegBC.Low8 &= unchecked((byte)~0x80);
									break;
								case 0xBA: // RES 7, D
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegDE.High8 &= unchecked((byte)~0x80);
									break;
								case 0xBB: // RES 7, E
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegDE.Low8 &= unchecked((byte)~0x80);
									break;
								case 0xBC: // RES 7, H
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegHL.High8 &= unchecked((byte)~0x80);
									break;
								case 0xBD: // RES 7, L
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegHL.Low8 &= unchecked((byte)~0x80);
									break;
								case 0xBE: // RES 7, (HL)
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) & unchecked((byte)~0x80)));
									break;
								case 0xBF: // RES 7, A
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegAF.High8 &= unchecked((byte)~0x80);
									break;
								case 0xC0: // SET 0, B
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegBC.High8 |= unchecked((byte)0x01);
									break;
								case 0xC1: // SET 0, C
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegBC.Low8 |= unchecked((byte)0x01);
									break;
								case 0xC2: // SET 0, D
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegDE.High8 |= unchecked((byte)0x01);
									break;
								case 0xC3: // SET 0, E
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegDE.Low8 |= unchecked((byte)0x01);
									break;
								case 0xC4: // SET 0, H
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegHL.High8 |= unchecked((byte)0x01);
									break;
								case 0xC5: // SET 0, L
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegHL.Low8 |= unchecked((byte)0x01);
									break;
								case 0xC6: // SET 0, (HL)
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) | unchecked((byte)0x01)));
									break;
								case 0xC7: // SET 0, A
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegAF.High8 |= unchecked((byte)0x01);
									break;
								case 0xC8: // SET 1, B
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegBC.High8 |= unchecked((byte)0x02);
									break;
								case 0xC9: // SET 1, C
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegBC.Low8 |= unchecked((byte)0x02);
									break;
								case 0xCA: // SET 1, D
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegDE.High8 |= unchecked((byte)0x02);
									break;
								case 0xCB: // SET 1, E
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegDE.Low8 |= unchecked((byte)0x02);
									break;
								case 0xCC: // SET 1, H
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegHL.High8 |= unchecked((byte)0x02);
									break;
								case 0xCD: // SET 1, L
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegHL.Low8 |= unchecked((byte)0x02);
									break;
								case 0xCE: // SET 1, (HL)
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) | unchecked((byte)0x02)));
									break;
								case 0xCF: // SET 1, A
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegAF.High8 |= unchecked((byte)0x02);
									break;
								case 0xD0: // SET 2, B
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegBC.High8 |= unchecked((byte)0x04);
									break;
								case 0xD1: // SET 2, C
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegBC.Low8 |= unchecked((byte)0x04);
									break;
								case 0xD2: // SET 2, D
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegDE.High8 |= unchecked((byte)0x04);
									break;
								case 0xD3: // SET 2, E
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegDE.Low8 |= unchecked((byte)0x04);
									break;
								case 0xD4: // SET 2, H
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegHL.High8 |= unchecked((byte)0x04);
									break;
								case 0xD5: // SET 2, L
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegHL.Low8 |= unchecked((byte)0x04);
									break;
								case 0xD6: // SET 2, (HL)
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) | unchecked((byte)0x04)));
									break;
								case 0xD7: // SET 2, A
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegAF.High8 |= unchecked((byte)0x04);
									break;
								case 0xD8: // SET 3, B
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegBC.High8 |= unchecked((byte)0x08);
									break;
								case 0xD9: // SET 3, C
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegBC.Low8 |= unchecked((byte)0x08);
									break;
								case 0xDA: // SET 3, D
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegDE.High8 |= unchecked((byte)0x08);
									break;
								case 0xDB: // SET 3, E
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegDE.Low8 |= unchecked((byte)0x08);
									break;
								case 0xDC: // SET 3, H
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegHL.High8 |= unchecked((byte)0x08);
									break;
								case 0xDD: // SET 3, L
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegHL.Low8 |= unchecked((byte)0x08);
									break;
								case 0xDE: // SET 3, (HL)
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) | unchecked((byte)0x08)));
									break;
								case 0xDF: // SET 3, A
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegAF.High8 |= unchecked((byte)0x08);
									break;
								case 0xE0: // SET 4, B
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegBC.High8 |= unchecked((byte)0x10);
									break;
								case 0xE1: // SET 4, C
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegBC.Low8 |= unchecked((byte)0x10);
									break;
								case 0xE2: // SET 4, D
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegDE.High8 |= unchecked((byte)0x10);
									break;
								case 0xE3: // SET 4, E
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegDE.Low8 |= unchecked((byte)0x10);
									break;
								case 0xE4: // SET 4, H
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegHL.High8 |= unchecked((byte)0x10);
									break;
								case 0xE5: // SET 4, L
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegHL.Low8 |= unchecked((byte)0x10);
									break;
								case 0xE6: // SET 4, (HL)
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) | unchecked((byte)0x10)));
									break;
								case 0xE7: // SET 4, A
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegAF.High8 |= unchecked((byte)0x10);
									break;
								case 0xE8: // SET 5, B
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegBC.High8 |= unchecked((byte)0x20);
									break;
								case 0xE9: // SET 5, C
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegBC.Low8 |= unchecked((byte)0x20);
									break;
								case 0xEA: // SET 5, D
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegDE.High8 |= unchecked((byte)0x20);
									break;
								case 0xEB: // SET 5, E
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegDE.Low8 |= unchecked((byte)0x20);
									break;
								case 0xEC: // SET 5, H
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegHL.High8 |= unchecked((byte)0x20);
									break;
								case 0xED: // SET 5, L
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegHL.Low8 |= unchecked((byte)0x20);
									break;
								case 0xEE: // SET 5, (HL)
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) | unchecked((byte)0x20)));
									break;
								case 0xEF: // SET 5, A
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegAF.High8 |= unchecked((byte)0x20);
									break;
								case 0xF0: // SET 6, B
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegBC.High8 |= unchecked((byte)0x40);
									break;
								case 0xF1: // SET 6, C
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegBC.Low8 |= unchecked((byte)0x40);
									break;
								case 0xF2: // SET 6, D
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegDE.High8 |= unchecked((byte)0x40);
									break;
								case 0xF3: // SET 6, E
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegDE.Low8 |= unchecked((byte)0x40);
									break;
								case 0xF4: // SET 6, H
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegHL.High8 |= unchecked((byte)0x40);
									break;
								case 0xF5: // SET 6, L
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegHL.Low8 |= unchecked((byte)0x40);
									break;
								case 0xF6: // SET 6, (HL)
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) | unchecked((byte)0x40)));
									break;
								case 0xF7: // SET 6, A
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegAF.High8 |= unchecked((byte)0x40);
									break;
								case 0xF8: // SET 7, B
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegBC.High8 |= unchecked((byte)0x80);
									break;
								case 0xF9: // SET 7, C
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegBC.Low8 |= unchecked((byte)0x80);
									break;
								case 0xFA: // SET 7, D
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegDE.High8 |= unchecked((byte)0x80);
									break;
								case 0xFB: // SET 7, E
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegDE.Low8 |= unchecked((byte)0x80);
									break;
								case 0xFC: // SET 7, H
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegHL.High8 |= unchecked((byte)0x80);
									break;
								case 0xFD: // SET 7, L
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegHL.Low8 |= unchecked((byte)0x80);
									break;
								case 0xFE: // SET 7, (HL)
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) | unchecked((byte)0x80)));
									break;
								case 0xFF: // SET 7, A
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegAF.High8 |= unchecked((byte)0x80);
									break;
							}
							break;
						case 0xCC: // CALL Z, nn
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (RegFlagZ) {
								this.totalExecutedCycles += 17; this.pendingCycles -= 17;
								WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
								RegPC.Value16 = TUS;
							} else {
								this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							}
							break;
						case 0xCD: // CALL nn
							this.totalExecutedCycles += 17; this.pendingCycles -= 17;
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
							RegPC.Value16 = TUS;
							break;
						case 0xCE: // ADC A, n
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							RegAF.Value16 = TableALU[1, RegAF.High8, ReadMemory(RegPC.Value16++), RegFlagC ? 1 : 0];
							break;
						case 0xCF: // RST $08
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
							RegPC.Value16 = 0x08;
							break;
						case 0xD0: // RET NC
							if (!RegFlagC) {
								this.totalExecutedCycles += 11; this.pendingCycles -= 11;
								RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
							} else {
								this.totalExecutedCycles += 5; this.pendingCycles -= 5;
							}
							break;
						case 0xD1: // POP DE
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							RegDE.Low8 = ReadMemory(RegSP.Value16++); RegDE.High8 = ReadMemory(RegSP.Value16++);
							break;
						case 0xD2: // JP NC, nn
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (!RegFlagC) {
								RegPC.Value16 = TUS;
							}
							break;
						case 0xD3: // OUT n, A
							this.totalExecutedCycles += 8; this.pendingCycles -= 8;
							WriteHardware(ReadMemory(RegPC.Value16++), RegAF.High8);
							this.totalExecutedCycles += 3; this.pendingCycles -= 3;
							break;
						case 0xD4: // CALL NC, nn
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (!RegFlagC) {
								this.totalExecutedCycles += 17; this.pendingCycles -= 17;
								WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
								RegPC.Value16 = TUS;
							} else {
								this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							}
							break;
						case 0xD5: // PUSH DE
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							WriteMemory(--RegSP.Value16, RegDE.High8); WriteMemory(--RegSP.Value16, RegDE.Low8);
							break;
						case 0xD6: // SUB n
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							RegAF.Value16 = TableALU[2, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
							break;
						case 0xD7: // RST $10
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
							RegPC.Value16 = 0x10;
							break;
						case 0xD8: // RET C
							if (RegFlagC) {
								this.totalExecutedCycles += 11; this.pendingCycles -= 11;
								RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
							} else {
								this.totalExecutedCycles += 5; this.pendingCycles -= 5;
							}
							break;
						case 0xD9: // EXX
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							TUS = RegBC.Value16; RegBC.Value16 = RegAltBC.Value16; RegAltBC.Value16 = TUS;
							TUS = RegDE.Value16; RegDE.Value16 = RegAltDE.Value16; RegAltDE.Value16 = TUS;
							TUS = RegHL.Value16; RegHL.Value16 = RegAltHL.Value16; RegAltHL.Value16 = TUS;
							break;
						case 0xDA: // JP C, nn
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (RegFlagC) {
								RegPC.Value16 = TUS;
							}
							break;
						case 0xDB: // IN A, n
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							RegAF.High8 = ReadHardware((ushort)ReadMemory(RegPC.Value16++));
							break;
						case 0xDC: // CALL C, nn
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (RegFlagC) {
								this.totalExecutedCycles += 17; this.pendingCycles -= 17;
								WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
								RegPC.Value16 = TUS;
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
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									RegBC.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									break;
								case 0x02: // LD (BC), A
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									WriteMemory(RegBC.Value16, RegAF.High8);
									break;
								case 0x03: // INC BC
									this.totalExecutedCycles += 6; this.pendingCycles -= 6;
									++RegBC.Value16;
									break;
								case 0x04: // INC B
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Low8 = (byte)(TableInc[++RegBC.High8] | (RegAF.Low8 & 1));
									break;
								case 0x05: // DEC B
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Low8 = (byte)(TableDec[--RegBC.High8] | (RegAF.Low8 & 1));
									break;
								case 0x06: // LD B, n
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									RegBC.High8 = ReadMemory(RegPC.Value16++);
									break;
								case 0x07: // RLCA
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableRotShift[0, 0, RegAF.Value16];
									break;
								case 0x08: // EX AF, AF'
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									TUS = RegAF.Value16; RegAF.Value16 = RegAltAF.Value16; RegAltAF.Value16 = TUS;
									break;
								case 0x09: // ADD IX, BC
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									TI1 = (short)RegIX.Value16; TI2 = (short)RegBC.Value16; TIR = TI1 + TI2;
									TUS = (ushort)TIR;
									RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
									RegFlagN = false;
									RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
									RegIX.Value16 = TUS;
									RegFlag3 = (TUS & 0x0800) != 0;
									RegFlag5 = (TUS & 0x2000) != 0;
									break;
								case 0x0A: // LD A, (BC)
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									RegAF.High8 = ReadMemory(RegBC.Value16);
									break;
								case 0x0B: // DEC BC
									this.totalExecutedCycles += 6; this.pendingCycles -= 6;
									--RegBC.Value16;
									break;
								case 0x0C: // INC C
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Low8 = (byte)(TableInc[++RegBC.Low8] | (RegAF.Low8 & 1));
									break;
								case 0x0D: // DEC C
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Low8 = (byte)(TableDec[--RegBC.Low8] | (RegAF.Low8 & 1));
									break;
								case 0x0E: // LD C, n
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									RegBC.Low8 = ReadMemory(RegPC.Value16++);
									break;
								case 0x0F: // RRCA
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableRotShift[0, 1, RegAF.Value16];
									break;
								case 0x10: // DJNZ d
									TSB = (sbyte)ReadMemory(RegPC.Value16++);
									if (--RegBC.High8 != 0) {
										this.totalExecutedCycles += 13; this.pendingCycles -= 13;
										RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
									} else {
										this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									}
									break;
								case 0x11: // LD DE, nn
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									RegDE.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									break;
								case 0x12: // LD (DE), A
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									WriteMemory(RegDE.Value16, RegAF.High8);
									break;
								case 0x13: // INC DE
									this.totalExecutedCycles += 6; this.pendingCycles -= 6;
									++RegDE.Value16;
									break;
								case 0x14: // INC D
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Low8 = (byte)(TableInc[++RegDE.High8] | (RegAF.Low8 & 1));
									break;
								case 0x15: // DEC D
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Low8 = (byte)(TableDec[--RegDE.High8] | (RegAF.Low8 & 1));
									break;
								case 0x16: // LD D, n
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									RegDE.High8 = ReadMemory(RegPC.Value16++);
									break;
								case 0x17: // RLA
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableRotShift[0, 2, RegAF.Value16];
									break;
								case 0x18: // JR d
									TSB = (sbyte)ReadMemory(RegPC.Value16++);
										this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
									break;
								case 0x19: // ADD IX, DE
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									TI1 = (short)RegIX.Value16; TI2 = (short)RegDE.Value16; TIR = TI1 + TI2;
									TUS = (ushort)TIR;
									RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
									RegFlagN = false;
									RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
									RegIX.Value16 = TUS;
									RegFlag3 = (TUS & 0x0800) != 0;
									RegFlag5 = (TUS & 0x2000) != 0;
									break;
								case 0x1A: // LD A, (DE)
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									RegAF.High8 = ReadMemory(RegDE.Value16);
									break;
								case 0x1B: // DEC DE
									this.totalExecutedCycles += 6; this.pendingCycles -= 6;
									--RegDE.Value16;
									break;
								case 0x1C: // INC E
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Low8 = (byte)(TableInc[++RegDE.Low8] | (RegAF.Low8 & 1));
									break;
								case 0x1D: // DEC E
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Low8 = (byte)(TableDec[--RegDE.Low8] | (RegAF.Low8 & 1));
									break;
								case 0x1E: // LD E, n
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									RegDE.Low8 = ReadMemory(RegPC.Value16++);
									break;
								case 0x1F: // RRA
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableRotShift[0, 3, RegAF.Value16];
									break;
								case 0x20: // JR NZ, d
									TSB = (sbyte)ReadMemory(RegPC.Value16++);
									if (!RegFlagZ) {
										this.totalExecutedCycles += 12; this.pendingCycles -= 12;
										RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
									} else {
										this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									}
									break;
								case 0x21: // LD IX, nn
									this.totalExecutedCycles += 14; this.pendingCycles -= 14;
									RegIX.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									break;
								case 0x22: // LD (nn), IX
									this.totalExecutedCycles += 20; this.pendingCycles -= 20;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									WriteMemory(TUS++, RegIX.Low8);
									WriteMemory(TUS, RegIX.High8);
									break;
								case 0x23: // INC IX
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									++RegIX.Value16;
									break;
								case 0x24: // INC IXH
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Low8 = (byte)(TableInc[++RegIX.High8] | (RegAF.Low8 & 1));
									break;
								case 0x25: // DEC IXH
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Low8 = (byte)(TableDec[--RegIX.High8] | (RegAF.Low8 & 1));
									break;
								case 0x26: // LD IXH, n
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegIX.High8 = ReadMemory(RegPC.Value16++);
									break;
								case 0x27: // DAA
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableDaa[RegAF.Value16];
									break;
								case 0x28: // JR Z, d
									TSB = (sbyte)ReadMemory(RegPC.Value16++);
									if (RegFlagZ) {
										this.totalExecutedCycles += 12; this.pendingCycles -= 12;
										RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
									} else {
										this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									}
									break;
								case 0x29: // ADD IX, IX
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									TI1 = (short)RegIX.Value16; TI2 = (short)RegIX.Value16; TIR = TI1 + TI2;
									TUS = (ushort)TIR;
									RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
									RegFlagN = false;
									RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
									RegIX.Value16 = TUS;
									RegFlag3 = (TUS & 0x0800) != 0;
									RegFlag5 = (TUS & 0x2000) != 0;
									break;
								case 0x2A: // LD IX, (nn)
									this.totalExecutedCycles += 20; this.pendingCycles -= 20;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									RegIX.Low8 = ReadMemory(TUS++); RegIX.High8 = ReadMemory(TUS);
									break;
								case 0x2B: // DEC IX
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									--RegIX.Value16;
									break;
								case 0x2C: // INC IXL
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Low8 = (byte)(TableInc[++RegIX.Low8] | (RegAF.Low8 & 1));
									break;
								case 0x2D: // DEC IXL
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Low8 = (byte)(TableDec[--RegIX.Low8] | (RegAF.Low8 & 1));
									break;
								case 0x2E: // LD IXL, n
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegIX.Low8 = ReadMemory(RegPC.Value16++);
									break;
								case 0x2F: // CPL
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.High8 ^= 0xFF; RegFlagH = true; RegFlagN = true; RegFlag3 = (RegAF.High8 & 0x08) != 0; RegFlag5 = (RegAF.High8 & 0x20) != 0;
									break;
								case 0x30: // JR NC, d
									TSB = (sbyte)ReadMemory(RegPC.Value16++);
									if (!RegFlagC) {
										this.totalExecutedCycles += 12; this.pendingCycles -= 12;
										RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
									} else {
										this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									}
									break;
								case 0x31: // LD SP, nn
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									RegSP.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									break;
								case 0x32: // LD (nn), A
									this.totalExecutedCycles += 13; this.pendingCycles -= 13;
									WriteMemory((ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256), RegAF.High8);
									break;
								case 0x33: // INC SP
									this.totalExecutedCycles += 6; this.pendingCycles -= 6;
									++RegSP.Value16;
									break;
								case 0x34: // INC (IX+d)
									this.totalExecutedCycles += 23; this.pendingCycles -= 23;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									TB = ReadMemory((ushort)(RegIX.Value16 + Displacement)); RegAF.Low8 = (byte)(TableInc[++TB] | (RegAF.Low8 & 1)); WriteMemory((ushort)(RegIX.Value16 + Displacement), TB);
									break;
								case 0x35: // DEC (IX+d)
									this.totalExecutedCycles += 23; this.pendingCycles -= 23;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									TB = ReadMemory((ushort)(RegIX.Value16 + Displacement)); RegAF.Low8 = (byte)(TableDec[--TB] | (RegAF.Low8 & 1)); WriteMemory((ushort)(RegIX.Value16 + Displacement), TB);
									break;
								case 0x36: // LD (IX+d), n
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIX.Value16 + Displacement), ReadMemory(RegPC.Value16++));
									break;
								case 0x37: // SCF
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegFlagH = false; RegFlagN = false; RegFlagC = true; RegFlag3 = (RegAF.High8 & 0x08) != 0; RegFlag5 = (RegAF.High8 & 0x20) != 0;
									break;
								case 0x38: // JR C, d
									TSB = (sbyte)ReadMemory(RegPC.Value16++);
									if (RegFlagC) {
										this.totalExecutedCycles += 12; this.pendingCycles -= 12;
										RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
									} else {
										this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									}
									break;
								case 0x39: // ADD IX, SP
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									TI1 = (short)RegIX.Value16; TI2 = (short)RegSP.Value16; TIR = TI1 + TI2;
									TUS = (ushort)TIR;
									RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
									RegFlagN = false;
									RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
									RegIX.Value16 = TUS;
									RegFlag3 = (TUS & 0x0800) != 0;
									RegFlag5 = (TUS & 0x2000) != 0;
									break;
								case 0x3A: // LD A, (nn)
									this.totalExecutedCycles += 13; this.pendingCycles -= 13;
									RegAF.High8 = ReadMemory((ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256));
									break;
								case 0x3B: // DEC SP
									this.totalExecutedCycles += 6; this.pendingCycles -= 6;
									--RegSP.Value16;
									break;
								case 0x3C: // INC A
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Low8 = (byte)(TableInc[++RegAF.High8] | (RegAF.Low8 & 1));
									break;
								case 0x3D: // DEC A
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Low8 = (byte)(TableDec[--RegAF.High8] | (RegAF.Low8 & 1));
									break;
								case 0x3E: // LD A, n
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									RegAF.High8 = ReadMemory(RegPC.Value16++);
									break;
								case 0x3F: // CCF
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegFlagH = RegFlagC; RegFlagN = false; RegFlagC ^= true; RegFlag3 = (RegAF.High8 & 0x08) != 0; RegFlag5 = (RegAF.High8 & 0x20) != 0;
									break;
								case 0x40: // LD B, B
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x41: // LD B, C
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegBC.High8 = RegBC.Low8;
									break;
								case 0x42: // LD B, D
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegBC.High8 = RegDE.High8;
									break;
								case 0x43: // LD B, E
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegBC.High8 = RegDE.Low8;
									break;
								case 0x44: // LD B, IXH
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegBC.High8 = RegIX.High8;
									break;
								case 0x45: // LD B, IXL
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegBC.High8 = RegIX.Low8;
									break;
								case 0x46: // LD B, (IX+d)
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegBC.High8 = ReadMemory((ushort)(RegIX.Value16 + Displacement));
									break;
								case 0x47: // LD B, A
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegBC.High8 = RegAF.High8;
									break;
								case 0x48: // LD C, B
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegBC.Low8 = RegBC.High8;
									break;
								case 0x49: // LD C, C
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x4A: // LD C, D
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegBC.Low8 = RegDE.High8;
									break;
								case 0x4B: // LD C, E
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegBC.Low8 = RegDE.Low8;
									break;
								case 0x4C: // LD C, IXH
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegBC.Low8 = RegIX.High8;
									break;
								case 0x4D: // LD C, IXL
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegBC.Low8 = RegIX.Low8;
									break;
								case 0x4E: // LD C, (IX+d)
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegBC.Low8 = ReadMemory((ushort)(RegIX.Value16 + Displacement));
									break;
								case 0x4F: // LD C, A
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegBC.Low8 = RegAF.High8;
									break;
								case 0x50: // LD D, B
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegDE.High8 = RegBC.High8;
									break;
								case 0x51: // LD D, C
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegDE.High8 = RegBC.Low8;
									break;
								case 0x52: // LD D, D
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x53: // LD D, E
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegDE.High8 = RegDE.Low8;
									break;
								case 0x54: // LD D, IXH
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegDE.High8 = RegIX.High8;
									break;
								case 0x55: // LD D, IXL
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegDE.High8 = RegIX.Low8;
									break;
								case 0x56: // LD D, (IX+d)
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegDE.High8 = ReadMemory((ushort)(RegIX.Value16 + Displacement));
									break;
								case 0x57: // LD D, A
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegDE.High8 = RegAF.High8;
									break;
								case 0x58: // LD E, B
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegDE.Low8 = RegBC.High8;
									break;
								case 0x59: // LD E, C
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegDE.Low8 = RegBC.Low8;
									break;
								case 0x5A: // LD E, D
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegDE.Low8 = RegDE.High8;
									break;
								case 0x5B: // LD E, E
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x5C: // LD E, IXH
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegDE.Low8 = RegIX.High8;
									break;
								case 0x5D: // LD E, IXL
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegDE.Low8 = RegIX.Low8;
									break;
								case 0x5E: // LD E, (IX+d)
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegDE.Low8 = ReadMemory((ushort)(RegIX.Value16 + Displacement));
									break;
								case 0x5F: // LD E, A
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegDE.Low8 = RegAF.High8;
									break;
								case 0x60: // LD IXH, B
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegIX.High8 = RegBC.High8;
									break;
								case 0x61: // LD IXH, C
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegIX.High8 = RegBC.Low8;
									break;
								case 0x62: // LD IXH, D
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegIX.High8 = RegDE.High8;
									break;
								case 0x63: // LD IXH, E
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegIX.High8 = RegDE.Low8;
									break;
								case 0x64: // LD IXH, IXH
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x65: // LD IXH, IXL
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegIX.High8 = RegIX.Low8;
									break;
								case 0x66: // LD H, (IX+d)
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegHL.High8 = ReadMemory((ushort)(RegIX.Value16 + Displacement));
									break;
								case 0x67: // LD IXH, A
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegIX.High8 = RegAF.High8;
									break;
								case 0x68: // LD IXL, B
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegIX.Low8 = RegBC.High8;
									break;
								case 0x69: // LD IXL, C
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegIX.Low8 = RegBC.Low8;
									break;
								case 0x6A: // LD IXL, D
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegIX.Low8 = RegDE.High8;
									break;
								case 0x6B: // LD IXL, E
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegIX.Low8 = RegDE.Low8;
									break;
								case 0x6C: // LD IXL, IXH
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegIX.Low8 = RegIX.High8;
									break;
								case 0x6D: // LD IXL, IXL
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x6E: // LD L, (IX+d)
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegHL.Low8 = ReadMemory((ushort)(RegIX.Value16 + Displacement));
									break;
								case 0x6F: // LD IXL, A
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegIX.Low8 = RegAF.High8;
									break;
								case 0x70: // LD (IX+d), B
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
									break;
								case 0x71: // LD (IX+d), C
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
									break;
								case 0x72: // LD (IX+d), D
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
									break;
								case 0x73: // LD (IX+d), E
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
									break;
								case 0x74: // LD (IX+d), H
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
									break;
								case 0x75: // LD (IX+d), L
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
									break;
								case 0x76: // HALT
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									this.Halt();
									break;
								case 0x77: // LD (IX+d), A
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
									break;
								case 0x78: // LD A, B
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.High8 = RegBC.High8;
									break;
								case 0x79: // LD A, C
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.High8 = RegBC.Low8;
									break;
								case 0x7A: // LD A, D
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.High8 = RegDE.High8;
									break;
								case 0x7B: // LD A, E
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.High8 = RegDE.Low8;
									break;
								case 0x7C: // LD A, IXH
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.High8 = RegIX.High8;
									break;
								case 0x7D: // LD A, IXL
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.High8 = RegIX.Low8;
									break;
								case 0x7E: // LD A, (IX+d)
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.High8 = ReadMemory((ushort)(RegIX.Value16 + Displacement));
									break;
								case 0x7F: // LD A, A
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x80: // ADD A, B
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[0, RegAF.High8, RegBC.High8, 0];
									break;
								case 0x81: // ADD A, C
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[0, RegAF.High8, RegBC.Low8, 0];
									break;
								case 0x82: // ADD A, D
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[0, RegAF.High8, RegDE.High8, 0];
									break;
								case 0x83: // ADD A, E
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[0, RegAF.High8, RegDE.Low8, 0];
									break;
								case 0x84: // ADD A, IXH
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Value16 = TableALU[0, RegAF.High8, RegIX.High8, 0];
									break;
								case 0x85: // ADD A, IXL
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Value16 = TableALU[0, RegAF.High8, RegIX.Low8, 0];
									break;
								case 0x86: // ADD A, (IX+d)
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[0, RegAF.High8, ReadMemory((ushort)(RegIX.Value16 + Displacement)), 0];
									break;
								case 0x87: // ADD A, A
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[0, RegAF.High8, RegAF.High8, 0];
									break;
								case 0x88: // ADC A, B
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[1, RegAF.High8, RegBC.High8, RegFlagC ? 1 : 0];
									break;
								case 0x89: // ADC A, C
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[1, RegAF.High8, RegBC.Low8, RegFlagC ? 1 : 0];
									break;
								case 0x8A: // ADC A, D
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[1, RegAF.High8, RegDE.High8, RegFlagC ? 1 : 0];
									break;
								case 0x8B: // ADC A, E
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[1, RegAF.High8, RegDE.Low8, RegFlagC ? 1 : 0];
									break;
								case 0x8C: // ADC A, IXH
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Value16 = TableALU[1, RegAF.High8, RegIX.High8, RegFlagC ? 1 : 0];
									break;
								case 0x8D: // ADC A, IXL
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Value16 = TableALU[1, RegAF.High8, RegIX.Low8, RegFlagC ? 1 : 0];
									break;
								case 0x8E: // ADC A, (IX+d)
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[1, RegAF.High8, ReadMemory((ushort)(RegIX.Value16 + Displacement)), RegFlagC ? 1 : 0];
									break;
								case 0x8F: // ADC A, A
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[1, RegAF.High8, RegAF.High8, RegFlagC ? 1 : 0];
									break;
								case 0x90: // SUB B
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[2, RegAF.High8, RegBC.High8, 0];
									break;
								case 0x91: // SUB C
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[2, RegAF.High8, RegBC.Low8, 0];
									break;
								case 0x92: // SUB D
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[2, RegAF.High8, RegDE.High8, 0];
									break;
								case 0x93: // SUB E
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[2, RegAF.High8, RegDE.Low8, 0];
									break;
								case 0x94: // SUB IXH
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Value16 = TableALU[2, RegAF.High8, RegIX.High8, 0];
									break;
								case 0x95: // SUB IXL
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Value16 = TableALU[2, RegAF.High8, RegIX.Low8, 0];
									break;
								case 0x96: // SUB (IX+d)
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[2, RegAF.High8, ReadMemory((ushort)(RegIX.Value16 + Displacement)), 0];
									break;
								case 0x97: // SUB A, A
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[2, RegAF.High8, RegAF.High8, 0];
									break;
								case 0x98: // SBC A, B
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[3, RegAF.High8, RegBC.High8, RegFlagC ? 1 : 0];
									break;
								case 0x99: // SBC A, C
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[3, RegAF.High8, RegBC.Low8, RegFlagC ? 1 : 0];
									break;
								case 0x9A: // SBC A, D
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[3, RegAF.High8, RegDE.High8, RegFlagC ? 1 : 0];
									break;
								case 0x9B: // SBC A, E
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[3, RegAF.High8, RegDE.Low8, RegFlagC ? 1 : 0];
									break;
								case 0x9C: // SBC A, IXH
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Value16 = TableALU[3, RegAF.High8, RegIX.High8, RegFlagC ? 1 : 0];
									break;
								case 0x9D: // SBC A, IXL
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Value16 = TableALU[3, RegAF.High8, RegIX.Low8, RegFlagC ? 1 : 0];
									break;
								case 0x9E: // SBC A, (IX+d)
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[3, RegAF.High8, ReadMemory((ushort)(RegIX.Value16 + Displacement)), RegFlagC ? 1 : 0];
									break;
								case 0x9F: // SBC A, A
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[3, RegAF.High8, RegAF.High8, RegFlagC ? 1 : 0];
									break;
								case 0xA0: // AND B
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[4, RegAF.High8, RegBC.High8, 0];
									break;
								case 0xA1: // AND C
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[4, RegAF.High8, RegBC.Low8, 0];
									break;
								case 0xA2: // AND D
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[4, RegAF.High8, RegDE.High8, 0];
									break;
								case 0xA3: // AND E
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[4, RegAF.High8, RegDE.Low8, 0];
									break;
								case 0xA4: // AND IXH
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Value16 = TableALU[4, RegAF.High8, RegIX.High8, 0];
									break;
								case 0xA5: // AND IXL
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Value16 = TableALU[4, RegAF.High8, RegIX.Low8, 0];
									break;
								case 0xA6: // AND (IX+d)
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[4, RegAF.High8, ReadMemory((ushort)(RegIX.Value16 + Displacement)), 0];
									break;
								case 0xA7: // AND A
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[4, RegAF.High8, RegAF.High8, 0];
									break;
								case 0xA8: // XOR B
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[5, RegAF.High8, RegBC.High8, 0];
									break;
								case 0xA9: // XOR C
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[5, RegAF.High8, RegBC.Low8, 0];
									break;
								case 0xAA: // XOR D
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[5, RegAF.High8, RegDE.High8, 0];
									break;
								case 0xAB: // XOR E
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[5, RegAF.High8, RegDE.Low8, 0];
									break;
								case 0xAC: // XOR IXH
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Value16 = TableALU[5, RegAF.High8, RegIX.High8, 0];
									break;
								case 0xAD: // XOR IXL
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Value16 = TableALU[5, RegAF.High8, RegIX.Low8, 0];
									break;
								case 0xAE: // XOR (IX+d)
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[5, RegAF.High8, ReadMemory((ushort)(RegIX.Value16 + Displacement)), 0];
									break;
								case 0xAF: // XOR A
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[5, RegAF.High8, RegAF.High8, 0];
									break;
								case 0xB0: // OR B
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[6, RegAF.High8, RegBC.High8, 0];
									break;
								case 0xB1: // OR C
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[6, RegAF.High8, RegBC.Low8, 0];
									break;
								case 0xB2: // OR D
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[6, RegAF.High8, RegDE.High8, 0];
									break;
								case 0xB3: // OR E
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[6, RegAF.High8, RegDE.Low8, 0];
									break;
								case 0xB4: // OR IXH
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Value16 = TableALU[6, RegAF.High8, RegIX.High8, 0];
									break;
								case 0xB5: // OR IXL
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Value16 = TableALU[6, RegAF.High8, RegIX.Low8, 0];
									break;
								case 0xB6: // OR (IX+d)
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[6, RegAF.High8, ReadMemory((ushort)(RegIX.Value16 + Displacement)), 0];
									break;
								case 0xB7: // OR A
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[6, RegAF.High8, RegAF.High8, 0];
									break;
								case 0xB8: // CP B
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[7, RegAF.High8, RegBC.High8, 0];
									break;
								case 0xB9: // CP C
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[7, RegAF.High8, RegBC.Low8, 0];
									break;
								case 0xBA: // CP D
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[7, RegAF.High8, RegDE.High8, 0];
									break;
								case 0xBB: // CP E
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[7, RegAF.High8, RegDE.Low8, 0];
									break;
								case 0xBC: // CP IXH
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Value16 = TableALU[7, RegAF.High8, RegIX.High8, 0];
									break;
								case 0xBD: // CP IXL
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Value16 = TableALU[7, RegAF.High8, RegIX.Low8, 0];
									break;
								case 0xBE: // CP (IX+d)
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[7, RegAF.High8, ReadMemory((ushort)(RegIX.Value16 + Displacement)), 0];
									break;
								case 0xBF: // CP A
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[7, RegAF.High8, RegAF.High8, 0];
									break;
								case 0xC0: // RET NZ
									if (!RegFlagZ) {
										this.totalExecutedCycles += 11; this.pendingCycles -= 11;
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									} else {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
									}
									break;
								case 0xC1: // POP BC
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									RegBC.Low8 = ReadMemory(RegSP.Value16++); RegBC.High8 = ReadMemory(RegSP.Value16++);
									break;
								case 0xC2: // JP NZ, nn
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagZ) {
										RegPC.Value16 = TUS;
									}
									break;
								case 0xC3: // JP nn
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									RegPC.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									break;
								case 0xC4: // CALL NZ, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagZ) {
										this.totalExecutedCycles += 17; this.pendingCycles -= 17;
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
									} else {
										this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									}
									break;
								case 0xC5: // PUSH BC
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									WriteMemory(--RegSP.Value16, RegBC.High8); WriteMemory(--RegSP.Value16, RegBC.Low8);
									break;
								case 0xC6: // ADD A, n
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									RegAF.Value16 = TableALU[0, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
									break;
								case 0xC7: // RST $00
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x00;
									break;
								case 0xC8: // RET Z
									if (RegFlagZ) {
										this.totalExecutedCycles += 11; this.pendingCycles -= 11;
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									} else {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
									}
									break;
								case 0xC9: // RET
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									break;
								case 0xCA: // JP Z, nn
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagZ) {
										RegPC.Value16 = TUS;
									}
									break;
								case 0xCB: // (Prefix)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									++RegR;
									switch (ReadMemory(RegPC.Value16++)) {
										case 0x00: // RLC (IX+d)→B
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.High8 = (byte)TUS;
											break;
										case 0x01: // RLC (IX+d)→C
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.Low8 = (byte)TUS;
											break;
										case 0x02: // RLC (IX+d)→D
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.High8 = (byte)TUS;
											break;
										case 0x03: // RLC (IX+d)→E
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.Low8 = (byte)TUS;
											break;
										case 0x04: // RLC (IX+d)→H
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.High8 = (byte)TUS;
											break;
										case 0x05: // RLC (IX+d)→L
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.Low8 = (byte)TUS;
											break;
										case 0x06: // RLC (IX+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x07: // RLC (IX+d)→A
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegAF.High8 = (byte)TUS;
											break;
										case 0x08: // RRC (IX+d)→B
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.High8 = (byte)TUS;
											break;
										case 0x09: // RRC (IX+d)→C
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.Low8 = (byte)TUS;
											break;
										case 0x0A: // RRC (IX+d)→D
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.High8 = (byte)TUS;
											break;
										case 0x0B: // RRC (IX+d)→E
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.Low8 = (byte)TUS;
											break;
										case 0x0C: // RRC (IX+d)→H
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.High8 = (byte)TUS;
											break;
										case 0x0D: // RRC (IX+d)→L
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.Low8 = (byte)TUS;
											break;
										case 0x0E: // RRC (IX+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x0F: // RRC (IX+d)→A
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegAF.High8 = (byte)TUS;
											break;
										case 0x10: // RL (IX+d)→B
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.High8 = (byte)TUS;
											break;
										case 0x11: // RL (IX+d)→C
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.Low8 = (byte)TUS;
											break;
										case 0x12: // RL (IX+d)→D
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.High8 = (byte)TUS;
											break;
										case 0x13: // RL (IX+d)→E
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.Low8 = (byte)TUS;
											break;
										case 0x14: // RL (IX+d)→H
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.High8 = (byte)TUS;
											break;
										case 0x15: // RL (IX+d)→L
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.Low8 = (byte)TUS;
											break;
										case 0x16: // RL (IX+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x17: // RL (IX+d)→A
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegAF.High8 = (byte)TUS;
											break;
										case 0x18: // RR (IX+d)→B
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.High8 = (byte)TUS;
											break;
										case 0x19: // RR (IX+d)→C
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.Low8 = (byte)TUS;
											break;
										case 0x1A: // RR (IX+d)→D
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.High8 = (byte)TUS;
											break;
										case 0x1B: // RR (IX+d)→E
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.Low8 = (byte)TUS;
											break;
										case 0x1C: // RR (IX+d)→H
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.High8 = (byte)TUS;
											break;
										case 0x1D: // RR (IX+d)→L
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.Low8 = (byte)TUS;
											break;
										case 0x1E: // RR (IX+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x1F: // RR (IX+d)→A
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegAF.High8 = (byte)TUS;
											break;
										case 0x20: // SLA (IX+d)→B
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.High8 = (byte)TUS;
											break;
										case 0x21: // SLA (IX+d)→C
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.Low8 = (byte)TUS;
											break;
										case 0x22: // SLA (IX+d)→D
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.High8 = (byte)TUS;
											break;
										case 0x23: // SLA (IX+d)→E
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.Low8 = (byte)TUS;
											break;
										case 0x24: // SLA (IX+d)→H
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.High8 = (byte)TUS;
											break;
										case 0x25: // SLA (IX+d)→L
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.Low8 = (byte)TUS;
											break;
										case 0x26: // SLA (IX+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x27: // SLA (IX+d)→A
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegAF.High8 = (byte)TUS;
											break;
										case 0x28: // SRA (IX+d)→B
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.High8 = (byte)TUS;
											break;
										case 0x29: // SRA (IX+d)→C
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.Low8 = (byte)TUS;
											break;
										case 0x2A: // SRA (IX+d)→D
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.High8 = (byte)TUS;
											break;
										case 0x2B: // SRA (IX+d)→E
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.Low8 = (byte)TUS;
											break;
										case 0x2C: // SRA (IX+d)→H
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.High8 = (byte)TUS;
											break;
										case 0x2D: // SRA (IX+d)→L
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.Low8 = (byte)TUS;
											break;
										case 0x2E: // SRA (IX+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x2F: // SRA (IX+d)→A
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegAF.High8 = (byte)TUS;
											break;
										case 0x30: // SL1 (IX+d)→B
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.High8 = (byte)TUS;
											break;
										case 0x31: // SL1 (IX+d)→C
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.Low8 = (byte)TUS;
											break;
										case 0x32: // SL1 (IX+d)→D
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.High8 = (byte)TUS;
											break;
										case 0x33: // SL1 (IX+d)→E
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.Low8 = (byte)TUS;
											break;
										case 0x34: // SL1 (IX+d)→H
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.High8 = (byte)TUS;
											break;
										case 0x35: // SL1 (IX+d)→L
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.Low8 = (byte)TUS;
											break;
										case 0x36: // SL1 (IX+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x37: // SL1 (IX+d)→A
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegAF.High8 = (byte)TUS;
											break;
										case 0x38: // SRL (IX+d)→B
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.High8 = (byte)TUS;
											break;
										case 0x39: // SRL (IX+d)→C
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.Low8 = (byte)TUS;
											break;
										case 0x3A: // SRL (IX+d)→D
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.High8 = (byte)TUS;
											break;
										case 0x3B: // SRL (IX+d)→E
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.Low8 = (byte)TUS;
											break;
										case 0x3C: // SRL (IX+d)→H
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.High8 = (byte)TUS;
											break;
										case 0x3D: // SRL (IX+d)→L
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.Low8 = (byte)TUS;
											break;
										case 0x3E: // SRL (IX+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x3F: // SRL (IX+d)→A
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegAF.High8 = (byte)TUS;
											break;
										case 0x40: // BIT 0, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x41: // BIT 0, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x42: // BIT 0, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x43: // BIT 0, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x44: // BIT 0, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x45: // BIT 0, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x46: // BIT 0, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x47: // BIT 0, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x48: // BIT 1, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x49: // BIT 1, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x4A: // BIT 1, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x4B: // BIT 1, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x4C: // BIT 1, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x4D: // BIT 1, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x4E: // BIT 1, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x4F: // BIT 1, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x50: // BIT 2, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x51: // BIT 2, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x52: // BIT 2, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x53: // BIT 2, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x54: // BIT 2, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x55: // BIT 2, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x56: // BIT 2, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x57: // BIT 2, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x58: // BIT 3, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x59: // BIT 3, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x5A: // BIT 3, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x5B: // BIT 3, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x5C: // BIT 3, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x5D: // BIT 3, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x5E: // BIT 3, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x5F: // BIT 3, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x60: // BIT 4, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x61: // BIT 4, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x62: // BIT 4, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x63: // BIT 4, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x64: // BIT 4, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x65: // BIT 4, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x66: // BIT 4, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x67: // BIT 4, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x68: // BIT 5, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x69: // BIT 5, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x6A: // BIT 5, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x6B: // BIT 5, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x6C: // BIT 5, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x6D: // BIT 5, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x6E: // BIT 5, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x6F: // BIT 5, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x70: // BIT 6, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x71: // BIT 6, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x72: // BIT 6, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x73: // BIT 6, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x74: // BIT 6, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x75: // BIT 6, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x76: // BIT 6, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x77: // BIT 6, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x78: // BIT 7, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x79: // BIT 7, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x7A: // BIT 7, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x7B: // BIT 7, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x7C: // BIT 7, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x7D: // BIT 7, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x7E: // BIT 7, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x7F: // BIT 7, (IX+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x80: // RES 0, (IX+d)→B
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											break;
										case 0x81: // RES 0, (IX+d)→C
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											break;
										case 0x82: // RES 0, (IX+d)→D
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											break;
										case 0x83: // RES 0, (IX+d)→E
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											break;
										case 0x84: // RES 0, (IX+d)→H
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											break;
										case 0x85: // RES 0, (IX+d)→L
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											break;
										case 0x86: // RES 0, (IX+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x01)));
											break;
										case 0x87: // RES 0, (IX+d)→A
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											break;
										case 0x88: // RES 1, (IX+d)→B
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											break;
										case 0x89: // RES 1, (IX+d)→C
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											break;
										case 0x8A: // RES 1, (IX+d)→D
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											break;
										case 0x8B: // RES 1, (IX+d)→E
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											break;
										case 0x8C: // RES 1, (IX+d)→H
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											break;
										case 0x8D: // RES 1, (IX+d)→L
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											break;
										case 0x8E: // RES 1, (IX+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x02)));
											break;
										case 0x8F: // RES 1, (IX+d)→A
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											break;
										case 0x90: // RES 2, (IX+d)→B
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											break;
										case 0x91: // RES 2, (IX+d)→C
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											break;
										case 0x92: // RES 2, (IX+d)→D
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											break;
										case 0x93: // RES 2, (IX+d)→E
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											break;
										case 0x94: // RES 2, (IX+d)→H
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											break;
										case 0x95: // RES 2, (IX+d)→L
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											break;
										case 0x96: // RES 2, (IX+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x04)));
											break;
										case 0x97: // RES 2, (IX+d)→A
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											break;
										case 0x98: // RES 3, (IX+d)→B
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											break;
										case 0x99: // RES 3, (IX+d)→C
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											break;
										case 0x9A: // RES 3, (IX+d)→D
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											break;
										case 0x9B: // RES 3, (IX+d)→E
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											break;
										case 0x9C: // RES 3, (IX+d)→H
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											break;
										case 0x9D: // RES 3, (IX+d)→L
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											break;
										case 0x9E: // RES 3, (IX+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x08)));
											break;
										case 0x9F: // RES 3, (IX+d)→A
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											break;
										case 0xA0: // RES 4, (IX+d)→B
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											break;
										case 0xA1: // RES 4, (IX+d)→C
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											break;
										case 0xA2: // RES 4, (IX+d)→D
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											break;
										case 0xA3: // RES 4, (IX+d)→E
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											break;
										case 0xA4: // RES 4, (IX+d)→H
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											break;
										case 0xA5: // RES 4, (IX+d)→L
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											break;
										case 0xA6: // RES 4, (IX+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x10)));
											break;
										case 0xA7: // RES 4, (IX+d)→A
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											break;
										case 0xA8: // RES 5, (IX+d)→B
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											break;
										case 0xA9: // RES 5, (IX+d)→C
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											break;
										case 0xAA: // RES 5, (IX+d)→D
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											break;
										case 0xAB: // RES 5, (IX+d)→E
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											break;
										case 0xAC: // RES 5, (IX+d)→H
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											break;
										case 0xAD: // RES 5, (IX+d)→L
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											break;
										case 0xAE: // RES 5, (IX+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x20)));
											break;
										case 0xAF: // RES 5, (IX+d)→A
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											break;
										case 0xB0: // RES 6, (IX+d)→B
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											break;
										case 0xB1: // RES 6, (IX+d)→C
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											break;
										case 0xB2: // RES 6, (IX+d)→D
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											break;
										case 0xB3: // RES 6, (IX+d)→E
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											break;
										case 0xB4: // RES 6, (IX+d)→H
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											break;
										case 0xB5: // RES 6, (IX+d)→L
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											break;
										case 0xB6: // RES 6, (IX+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x40)));
											break;
										case 0xB7: // RES 6, (IX+d)→A
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											break;
										case 0xB8: // RES 7, (IX+d)→B
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											break;
										case 0xB9: // RES 7, (IX+d)→C
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											break;
										case 0xBA: // RES 7, (IX+d)→D
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											break;
										case 0xBB: // RES 7, (IX+d)→E
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											break;
										case 0xBC: // RES 7, (IX+d)→H
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											break;
										case 0xBD: // RES 7, (IX+d)→L
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											break;
										case 0xBE: // RES 7, (IX+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x80)));
											break;
										case 0xBF: // RES 7, (IX+d)→A
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											break;
										case 0xC0: // SET 0, (IX+d)→B
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											break;
										case 0xC1: // SET 0, (IX+d)→C
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											break;
										case 0xC2: // SET 0, (IX+d)→D
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											break;
										case 0xC3: // SET 0, (IX+d)→E
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											break;
										case 0xC4: // SET 0, (IX+d)→H
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											break;
										case 0xC5: // SET 0, (IX+d)→L
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											break;
										case 0xC6: // SET 0, (IX+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x01)));
											break;
										case 0xC7: // SET 0, (IX+d)→A
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											break;
										case 0xC8: // SET 1, (IX+d)→B
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											break;
										case 0xC9: // SET 1, (IX+d)→C
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											break;
										case 0xCA: // SET 1, (IX+d)→D
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											break;
										case 0xCB: // SET 1, (IX+d)→E
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											break;
										case 0xCC: // SET 1, (IX+d)→H
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											break;
										case 0xCD: // SET 1, (IX+d)→L
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											break;
										case 0xCE: // SET 1, (IX+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x02)));
											break;
										case 0xCF: // SET 1, (IX+d)→A
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											break;
										case 0xD0: // SET 2, (IX+d)→B
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											break;
										case 0xD1: // SET 2, (IX+d)→C
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											break;
										case 0xD2: // SET 2, (IX+d)→D
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											break;
										case 0xD3: // SET 2, (IX+d)→E
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											break;
										case 0xD4: // SET 2, (IX+d)→H
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											break;
										case 0xD5: // SET 2, (IX+d)→L
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											break;
										case 0xD6: // SET 2, (IX+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x04)));
											break;
										case 0xD7: // SET 2, (IX+d)→A
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											break;
										case 0xD8: // SET 3, (IX+d)→B
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											break;
										case 0xD9: // SET 3, (IX+d)→C
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											break;
										case 0xDA: // SET 3, (IX+d)→D
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											break;
										case 0xDB: // SET 3, (IX+d)→E
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											break;
										case 0xDC: // SET 3, (IX+d)→H
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											break;
										case 0xDD: // SET 3, (IX+d)→L
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											break;
										case 0xDE: // SET 3, (IX+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x08)));
											break;
										case 0xDF: // SET 3, (IX+d)→A
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											break;
										case 0xE0: // SET 4, (IX+d)→B
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											break;
										case 0xE1: // SET 4, (IX+d)→C
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											break;
										case 0xE2: // SET 4, (IX+d)→D
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											break;
										case 0xE3: // SET 4, (IX+d)→E
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											break;
										case 0xE4: // SET 4, (IX+d)→H
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											break;
										case 0xE5: // SET 4, (IX+d)→L
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											break;
										case 0xE6: // SET 4, (IX+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x10)));
											break;
										case 0xE7: // SET 4, (IX+d)→A
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											break;
										case 0xE8: // SET 5, (IX+d)→B
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											break;
										case 0xE9: // SET 5, (IX+d)→C
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											break;
										case 0xEA: // SET 5, (IX+d)→D
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											break;
										case 0xEB: // SET 5, (IX+d)→E
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											break;
										case 0xEC: // SET 5, (IX+d)→H
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											break;
										case 0xED: // SET 5, (IX+d)→L
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											break;
										case 0xEE: // SET 5, (IX+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x20)));
											break;
										case 0xEF: // SET 5, (IX+d)→A
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											break;
										case 0xF0: // SET 6, (IX+d)→B
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											break;
										case 0xF1: // SET 6, (IX+d)→C
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											break;
										case 0xF2: // SET 6, (IX+d)→D
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											break;
										case 0xF3: // SET 6, (IX+d)→E
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											break;
										case 0xF4: // SET 6, (IX+d)→H
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											break;
										case 0xF5: // SET 6, (IX+d)→L
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											break;
										case 0xF6: // SET 6, (IX+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x40)));
											break;
										case 0xF7: // SET 6, (IX+d)→A
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											break;
										case 0xF8: // SET 7, (IX+d)→B
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											break;
										case 0xF9: // SET 7, (IX+d)→C
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											break;
										case 0xFA: // SET 7, (IX+d)→D
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											break;
										case 0xFB: // SET 7, (IX+d)→E
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											break;
										case 0xFC: // SET 7, (IX+d)→H
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											break;
										case 0xFD: // SET 7, (IX+d)→L
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											break;
										case 0xFE: // SET 7, (IX+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x80)));
											break;
										case 0xFF: // SET 7, (IX+d)→A
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											break;
									}
									break;
								case 0xCC: // CALL Z, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagZ) {
										this.totalExecutedCycles += 17; this.pendingCycles -= 17;
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
									} else {
										this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									}
									break;
								case 0xCD: // CALL nn
									this.totalExecutedCycles += 17; this.pendingCycles -= 17;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = TUS;
									break;
								case 0xCE: // ADC A, n
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									RegAF.Value16 = TableALU[1, RegAF.High8, ReadMemory(RegPC.Value16++), RegFlagC ? 1 : 0];
									break;
								case 0xCF: // RST $08
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x08;
									break;
								case 0xD0: // RET NC
									if (!RegFlagC) {
										this.totalExecutedCycles += 11; this.pendingCycles -= 11;
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									} else {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
									}
									break;
								case 0xD1: // POP DE
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									RegDE.Low8 = ReadMemory(RegSP.Value16++); RegDE.High8 = ReadMemory(RegSP.Value16++);
									break;
								case 0xD2: // JP NC, nn
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagC) {
										RegPC.Value16 = TUS;
									}
									break;
								case 0xD3: // OUT n, A
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									WriteHardware(ReadMemory(RegPC.Value16++), RegAF.High8);
									this.totalExecutedCycles += 3; this.pendingCycles -= 3;
									break;
								case 0xD4: // CALL NC, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagC) {
										this.totalExecutedCycles += 17; this.pendingCycles -= 17;
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
									} else {
										this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									}
									break;
								case 0xD5: // PUSH DE
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									WriteMemory(--RegSP.Value16, RegDE.High8); WriteMemory(--RegSP.Value16, RegDE.Low8);
									break;
								case 0xD6: // SUB n
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									RegAF.Value16 = TableALU[2, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
									break;
								case 0xD7: // RST $10
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x10;
									break;
								case 0xD8: // RET C
									if (RegFlagC) {
										this.totalExecutedCycles += 11; this.pendingCycles -= 11;
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									} else {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
									}
									break;
								case 0xD9: // EXX
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									TUS = RegBC.Value16; RegBC.Value16 = RegAltBC.Value16; RegAltBC.Value16 = TUS;
									TUS = RegDE.Value16; RegDE.Value16 = RegAltDE.Value16; RegAltDE.Value16 = TUS;
									TUS = RegHL.Value16; RegHL.Value16 = RegAltHL.Value16; RegAltHL.Value16 = TUS;
									break;
								case 0xDA: // JP C, nn
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagC) {
										RegPC.Value16 = TUS;
									}
									break;
								case 0xDB: // IN A, n
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									RegAF.High8 = ReadHardware((ushort)ReadMemory(RegPC.Value16++));
									break;
								case 0xDC: // CALL C, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagC) {
										this.totalExecutedCycles += 17; this.pendingCycles -= 17;
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
									} else {
										this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									}
									break;
								case 0xDD: // <-
									this.totalExecutedCycles += 1337; this.pendingCycles -= 1337;
									// Invalid sequence.
									break;
								case 0xDE: // SBC A, n
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									RegAF.Value16 = TableALU[3, RegAF.High8, ReadMemory(RegPC.Value16++), RegFlagC ? 1 : 0];
									break;
								case 0xDF: // RST $18
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x18;
									break;
								case 0xE0: // RET PO
									if (!RegFlagP) {
										this.totalExecutedCycles += 11; this.pendingCycles -= 11;
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									} else {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
									}
									break;
								case 0xE1: // POP IX
									this.totalExecutedCycles += 14; this.pendingCycles -= 14;
									RegIX.Low8 = ReadMemory(RegSP.Value16++); RegIX.High8 = ReadMemory(RegSP.Value16++);
									break;
								case 0xE2: // JP PO, nn
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagP) {
										RegPC.Value16 = TUS;
									}
									break;
								case 0xE3: // EX (SP), IX
									this.totalExecutedCycles += 23; this.pendingCycles -= 23;
									TUS = RegSP.Value16; TBL = ReadMemory(TUS++); TBH = ReadMemory(TUS--);
									WriteMemory(TUS++, RegIX.Low8); WriteMemory(TUS, RegIX.High8);
									RegIX.Low8 = TBL; RegIX.High8 = TBH;
									break;
								case 0xE4: // CALL C, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagC) {
										this.totalExecutedCycles += 17; this.pendingCycles -= 17;
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
									} else {
										this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									}
									break;
								case 0xE5: // PUSH IX
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									WriteMemory(--RegSP.Value16, RegIX.High8); WriteMemory(--RegSP.Value16, RegIX.Low8);
									break;
								case 0xE6: // AND n
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									RegAF.Value16 = TableALU[4, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
									break;
								case 0xE7: // RST $20
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x20;
									break;
								case 0xE8: // RET PE
									if (RegFlagP) {
										this.totalExecutedCycles += 11; this.pendingCycles -= 11;
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									} else {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
									}
									break;
								case 0xE9: // JP IX
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegPC.Value16 = RegIX.Value16;
									break;
								case 0xEA: // JP PE, nn
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagP) {
										RegPC.Value16 = TUS;
									}
									break;
								case 0xEB: // EX DE, HL
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									TUS = RegDE.Value16; RegDE.Value16 = RegHL.Value16; RegHL.Value16 = TUS;
									break;
								case 0xEC: // CALL PE, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagP) {
										this.totalExecutedCycles += 17; this.pendingCycles -= 17;
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
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
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											RegBC.High8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegBC.High8 > 127;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegBC.High8];
											RegFlagN = false;
											break;
										case 0x41: // OUT C, B
											this.totalExecutedCycles += 9; this.pendingCycles -= 9;
											WriteHardware(RegBC.Low8, RegBC.High8);
											this.totalExecutedCycles += 3; this.pendingCycles -= 3;
											break;
										case 0x42: // SBC HL, BC
											this.totalExecutedCycles += 15; this.pendingCycles -= 15;
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
											break;
										case 0x43: // LD (nn), BC
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											WriteMemory(TUS++, RegBC.Low8);
											WriteMemory(TUS, RegBC.High8);
											break;
										case 0x44: // NEG
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											RegAF.Value16 = TableNeg[RegAF.Value16];
											break;
										case 0x45: // RETN
											this.totalExecutedCycles += 14; this.pendingCycles -= 14;
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											this.IFF1 = this.IFF2;
											break;
										case 0x46: // IM $0
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											interruptMode = 0;
											break;
										case 0x47: // LD I, A
											this.totalExecutedCycles += 9; this.pendingCycles -= 9;
											RegI = RegAF.High8;
											break;
										case 0x48: // IN C, C
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											RegBC.Low8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegBC.Low8 > 127;
											RegFlagZ = RegBC.Low8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegBC.Low8];
											RegFlagN = false;
											break;
										case 0x49: // OUT C, C
											this.totalExecutedCycles += 9; this.pendingCycles -= 9;
											WriteHardware(RegBC.Low8, RegBC.Low8);
											this.totalExecutedCycles += 3; this.pendingCycles -= 3;
											break;
										case 0x4A: // ADC HL, BC
											this.totalExecutedCycles += 15; this.pendingCycles -= 15;
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
											break;
										case 0x4B: // LD BC, (nn)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											RegBC.Low8 = ReadMemory(TUS++); RegBC.High8 = ReadMemory(TUS);
											break;
										case 0x4C: // NEG
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											RegAF.Value16 = TableNeg[RegAF.Value16];
											break;
										case 0x4D: // RETI
											this.totalExecutedCycles += 14; this.pendingCycles -= 14;
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											break;
										case 0x4E: // IM $0
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											interruptMode = 0;
											break;
										case 0x4F: // LD R, A
											this.totalExecutedCycles += 9; this.pendingCycles -= 9;
											RegR = RegAF.High8;
											break;
										case 0x50: // IN D, C
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											RegDE.High8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegDE.High8 > 127;
											RegFlagZ = RegDE.High8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegDE.High8];
											RegFlagN = false;
											break;
										case 0x51: // OUT C, D
											this.totalExecutedCycles += 9; this.pendingCycles -= 9;
											WriteHardware(RegBC.Low8, RegDE.High8);
											this.totalExecutedCycles += 3; this.pendingCycles -= 3;
											break;
										case 0x52: // SBC HL, DE
											this.totalExecutedCycles += 15; this.pendingCycles -= 15;
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
											break;
										case 0x53: // LD (nn), DE
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											WriteMemory(TUS++, RegDE.Low8);
											WriteMemory(TUS, RegDE.High8);
											break;
										case 0x54: // NEG
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											RegAF.Value16 = TableNeg[RegAF.Value16];
											break;
										case 0x55: // RETN
											this.totalExecutedCycles += 14; this.pendingCycles -= 14;
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											this.IFF1 = this.IFF2;
											break;
										case 0x56: // IM $1
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											interruptMode = 1;
											break;
										case 0x57: // LD A, I
											this.totalExecutedCycles += 9; this.pendingCycles -= 9;
											RegAF.High8 = RegI;
											RegFlagS = RegI > 127;
											RegFlagZ = RegI == 0;
											RegFlagH = false;
											RegFlagN = false;
											RegFlagP = this.IFF2;
											break;
										case 0x58: // IN E, C
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											RegDE.Low8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegDE.Low8 > 127;
											RegFlagZ = RegDE.Low8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegDE.Low8];
											RegFlagN = false;
											break;
										case 0x59: // OUT C, E
											this.totalExecutedCycles += 9; this.pendingCycles -= 9;
											WriteHardware(RegBC.Low8, RegDE.Low8);
											this.totalExecutedCycles += 3; this.pendingCycles -= 3;
											break;
										case 0x5A: // ADC HL, DE
											this.totalExecutedCycles += 15; this.pendingCycles -= 15;
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
											break;
										case 0x5B: // LD DE, (nn)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											RegDE.Low8 = ReadMemory(TUS++); RegDE.High8 = ReadMemory(TUS);
											break;
										case 0x5C: // NEG
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											RegAF.Value16 = TableNeg[RegAF.Value16];
											break;
										case 0x5D: // RETI
											this.totalExecutedCycles += 14; this.pendingCycles -= 14;
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											break;
										case 0x5E: // IM $2
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											interruptMode = 2;
											break;
										case 0x5F: // LD A, R
											this.totalExecutedCycles += 9; this.pendingCycles -= 9;
											RegAF.High8 = (byte)(RegR & 0x7F);
											RegFlagS = (byte)(RegR & 0x7F) > 127;
											RegFlagZ = (byte)(RegR & 0x7F) == 0;
											RegFlagH = false;
											RegFlagN = false;
											RegFlagP = this.IFF2;
											break;
										case 0x60: // IN H, C
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											RegHL.High8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegHL.High8 > 127;
											RegFlagZ = RegHL.High8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegHL.High8];
											RegFlagN = false;
											break;
										case 0x61: // OUT C, H
											this.totalExecutedCycles += 9; this.pendingCycles -= 9;
											WriteHardware(RegBC.Low8, RegHL.High8);
											this.totalExecutedCycles += 3; this.pendingCycles -= 3;
											break;
										case 0x62: // SBC HL, HL
											this.totalExecutedCycles += 15; this.pendingCycles -= 15;
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
											break;
										case 0x63: // LD (nn), HL
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											WriteMemory(TUS++, RegHL.Low8);
											WriteMemory(TUS, RegHL.High8);
											break;
										case 0x64: // NEG
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											RegAF.Value16 = TableNeg[RegAF.Value16];
											break;
										case 0x65: // RETN
											this.totalExecutedCycles += 14; this.pendingCycles -= 14;
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											this.IFF1 = this.IFF2;
											break;
										case 0x66: // IM $0
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											interruptMode = 0;
											break;
										case 0x67: // RRD
											this.totalExecutedCycles += 18; this.pendingCycles -= 18;
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
											break;
										case 0x68: // IN L, C
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											RegHL.Low8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegHL.Low8 > 127;
											RegFlagZ = RegHL.Low8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegHL.Low8];
											RegFlagN = false;
											break;
										case 0x69: // OUT C, L
											this.totalExecutedCycles += 9; this.pendingCycles -= 9;
											WriteHardware(RegBC.Low8, RegHL.Low8);
											this.totalExecutedCycles += 3; this.pendingCycles -= 3;
											break;
										case 0x6A: // ADC HL, HL
											this.totalExecutedCycles += 15; this.pendingCycles -= 15;
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
											break;
										case 0x6B: // LD HL, (nn)
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											RegHL.Low8 = ReadMemory(TUS++); RegHL.High8 = ReadMemory(TUS);
											break;
										case 0x6C: // NEG
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											RegAF.Value16 = TableNeg[RegAF.Value16];
											break;
										case 0x6D: // RETI
											this.totalExecutedCycles += 14; this.pendingCycles -= 14;
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											break;
										case 0x6E: // IM $0
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											interruptMode = 0;
											break;
										case 0x6F: // RLD
											this.totalExecutedCycles += 18; this.pendingCycles -= 18;
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
											break;
										case 0x70: // IN 0, C
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											TB = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = TB > 127;
											RegFlagZ = TB == 0;
											RegFlagH = false;
											RegFlagP = TableParity[TB];
											RegFlagN = false;
											break;
										case 0x71: // OUT C, 0
											this.totalExecutedCycles += 9; this.pendingCycles -= 9;
											WriteHardware(RegBC.Low8, 0);
											this.totalExecutedCycles += 3; this.pendingCycles -= 3;
											break;
										case 0x72: // SBC HL, SP
											this.totalExecutedCycles += 15; this.pendingCycles -= 15;
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
											break;
										case 0x73: // LD (nn), SP
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											WriteMemory(TUS++, RegSP.Low8);
											WriteMemory(TUS, RegSP.High8);
											break;
										case 0x74: // NEG
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											RegAF.Value16 = TableNeg[RegAF.Value16];
											break;
										case 0x75: // RETN
											this.totalExecutedCycles += 14; this.pendingCycles -= 14;
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											this.IFF1 = this.IFF2;
											break;
										case 0x76: // IM $1
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											interruptMode = 1;
											break;
										case 0x77: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x78: // IN A, C
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											RegAF.High8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegAF.High8 > 127;
											RegFlagZ = RegAF.High8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegAF.High8];
											RegFlagN = false;
											break;
										case 0x79: // OUT C, A
											this.totalExecutedCycles += 9; this.pendingCycles -= 9;
											WriteHardware(RegBC.Low8, RegAF.High8);
											this.totalExecutedCycles += 3; this.pendingCycles -= 3;
											break;
										case 0x7A: // ADC HL, SP
											this.totalExecutedCycles += 15; this.pendingCycles -= 15;
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
											break;
										case 0x7B: // LD SP, (nn)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											RegSP.Low8 = ReadMemory(TUS++); RegSP.High8 = ReadMemory(TUS);
											break;
										case 0x7C: // NEG
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											RegAF.Value16 = TableNeg[RegAF.Value16];
											break;
										case 0x7D: // RETI
											this.totalExecutedCycles += 14; this.pendingCycles -= 14;
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											break;
										case 0x7E: // IM $2
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											interruptMode = 2;
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
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											WriteMemory(RegDE.Value16++, TB1 = ReadMemory(RegHL.Value16++));
											TB1 += RegAF.High8; RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											RegFlagH = false;
											RegFlagN = false;
											break;
										case 0xA1: // CPI
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											TB1 = ReadMemory(RegHL.Value16++); TB2 = (byte)(RegAF.High8 - TB1);
											RegFlagN = true;
											RegFlagH = TableHalfBorrow[RegAF.High8, TB1];
											RegFlagZ = TB2 == 0;
											RegFlagS = TB2 > 127;
											TB1 = (byte)(RegAF.High8 - TB1 - (RegFlagH ? 1 : 0)); RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											break;
										case 0xA2: // INI
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											WriteMemory(RegHL.Value16++, ReadHardware(RegBC.Value16));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											break;
										case 0xA3: // OUTI
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											WriteHardware(RegBC.Value16, ReadMemory(RegHL.Value16++));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
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
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											WriteMemory(RegDE.Value16--, TB1 = ReadMemory(RegHL.Value16--));
											TB1 += RegAF.High8; RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											RegFlagH = false;
											RegFlagN = false;
											break;
										case 0xA9: // CPD
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											TB1 = ReadMemory(RegHL.Value16--); TB2 = (byte)(RegAF.High8 - TB1);
											RegFlagN = true;
											RegFlagH = TableHalfBorrow[RegAF.High8, TB1];
											RegFlagZ = TB2 == 0;
											RegFlagS = TB2 > 127;
											TB1 = (byte)(RegAF.High8 - TB1 - (RegFlagH ? 1 : 0)); RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											break;
										case 0xAA: // IND
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											WriteMemory(RegHL.Value16--, ReadHardware(RegBC.Value16));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											break;
										case 0xAB: // OUTD
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											WriteHardware(RegBC.Value16, ReadMemory(RegHL.Value16--));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
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
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											WriteMemory(RegDE.Value16++, TB1 = ReadMemory(RegHL.Value16++));
											TB1 += RegAF.High8; RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											RegFlagH = false;
											RegFlagN = false;
											if (RegBC.Value16 != 0) {
												this.totalExecutedCycles += 5; this.pendingCycles -= 5;
												RegPC.Value16 -= 2;
											}
											break;
										case 0xB1: // CPIR
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											TB1 = ReadMemory(RegHL.Value16++); TB2 = (byte)(RegAF.High8 - TB1);
											RegFlagN = true;
											RegFlagH = TableHalfBorrow[RegAF.High8, TB1];
											RegFlagZ = TB2 == 0;
											RegFlagS = TB2 > 127;
											TB1 = (byte)(RegAF.High8 - TB1 - (RegFlagH ? 1 : 0)); RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											if (RegBC.Value16 != 0 && !RegFlagZ) {
												this.totalExecutedCycles += 5; this.pendingCycles -= 5;
												RegPC.Value16 -= 2;
											}
											break;
										case 0xB2: // INIR
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											WriteMemory(RegHL.Value16++, ReadHardware(RegBC.Value16));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											if (RegBC.High8 != 0) {
												this.totalExecutedCycles += 5; this.pendingCycles -= 5;
												RegPC.Value16 -= 2;
											}
											break;
										case 0xB3: // OTIR
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											WriteHardware(RegBC.Value16, ReadMemory(RegHL.Value16++));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											if (RegBC.High8 != 0) {
												this.totalExecutedCycles += 5; this.pendingCycles -= 5;
												RegPC.Value16 -= 2;
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
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											WriteMemory(RegDE.Value16--, TB1 = ReadMemory(RegHL.Value16--));
											TB1 += RegAF.High8; RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											RegFlagH = false;
											RegFlagN = false;
											if (RegBC.Value16 != 0) {
												this.totalExecutedCycles += 5; this.pendingCycles -= 5;
												RegPC.Value16 -= 2;
											}
											break;
										case 0xB9: // CPDR
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											TB1 = ReadMemory(RegHL.Value16--); TB2 = (byte)(RegAF.High8 - TB1);
											RegFlagN = true;
											RegFlagH = TableHalfBorrow[RegAF.High8, TB1];
											RegFlagZ = TB2 == 0;
											RegFlagS = TB2 > 127;
											TB1 = (byte)(RegAF.High8 - TB1 - (RegFlagH ? 1 : 0)); RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											if (RegBC.Value16 != 0 && !RegFlagZ) {
												this.totalExecutedCycles += 5; this.pendingCycles -= 5;
												RegPC.Value16 -= 2;
											}
											break;
										case 0xBA: // INDR
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											WriteMemory(RegHL.Value16--, ReadHardware(RegBC.Value16));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											if (RegBC.High8 != 0) {
												this.totalExecutedCycles += 5; this.pendingCycles -= 5;
												RegPC.Value16 -= 2;
											}
											break;
										case 0xBB: // OTDR
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											WriteHardware(RegBC.Value16, ReadMemory(RegHL.Value16--));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											if (RegBC.High8 != 0) {
												this.totalExecutedCycles += 5; this.pendingCycles -= 5;
												RegPC.Value16 -= 2;
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
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									RegAF.Value16 = TableALU[5, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
									break;
								case 0xEF: // RST $28
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x28;
									break;
								case 0xF0: // RET P
									if (!RegFlagS) {
										this.totalExecutedCycles += 11; this.pendingCycles -= 11;
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									} else {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
									}
									break;
								case 0xF1: // POP AF
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									RegAF.Low8 = ReadMemory(RegSP.Value16++); RegAF.High8 = ReadMemory(RegSP.Value16++);
									break;
								case 0xF2: // JP P, nn
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagS) {
										RegPC.Value16 = TUS;
									}
									break;
								case 0xF3: // DI
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									this.IFF1 = this.IFF2 = false;
									break;
								case 0xF4: // CALL P, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagS) {
										this.totalExecutedCycles += 17; this.pendingCycles -= 17;
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
									} else {
										this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									}
									break;
								case 0xF5: // PUSH AF
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									WriteMemory(--RegSP.Value16, RegAF.High8); WriteMemory(--RegSP.Value16, RegAF.Low8);
									break;
								case 0xF6: // OR n
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									RegAF.Value16 = TableALU[6, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
									break;
								case 0xF7: // RST $30
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x30;
									break;
								case 0xF8: // RET M
									if (RegFlagS) {
										this.totalExecutedCycles += 11; this.pendingCycles -= 11;
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									} else {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
									}
									break;
								case 0xF9: // LD SP, IX
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									RegSP.Value16 = RegIX.Value16;
									break;
								case 0xFA: // JP M, nn
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagS) {
										RegPC.Value16 = TUS;
									}
									break;
								case 0xFB: // EI
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									this.IFF1 = this.IFF2 = true;
									Interruptable = false;
									break;
								case 0xFC: // CALL M, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagS) {
										this.totalExecutedCycles += 17; this.pendingCycles -= 17;
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
									} else {
										this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									}
									break;
								case 0xFD: // <-
									this.totalExecutedCycles += 1337; this.pendingCycles -= 1337;
									// Invalid sequence.
									break;
								case 0xFE: // CP n
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									RegAF.Value16 = TableALU[7, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
									break;
								case 0xFF: // RST $38
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x38;
									break;
							}
							break;
						case 0xDE: // SBC A, n
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							RegAF.Value16 = TableALU[3, RegAF.High8, ReadMemory(RegPC.Value16++), RegFlagC ? 1 : 0];
							break;
						case 0xDF: // RST $18
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
							RegPC.Value16 = 0x18;
							break;
						case 0xE0: // RET PO
							if (!RegFlagP) {
								this.totalExecutedCycles += 11; this.pendingCycles -= 11;
								RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
							} else {
								this.totalExecutedCycles += 5; this.pendingCycles -= 5;
							}
							break;
						case 0xE1: // POP HL
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							RegHL.Low8 = ReadMemory(RegSP.Value16++); RegHL.High8 = ReadMemory(RegSP.Value16++);
							break;
						case 0xE2: // JP PO, nn
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (!RegFlagP) {
								RegPC.Value16 = TUS;
							}
							break;
						case 0xE3: // EX (SP), HL
							this.totalExecutedCycles += 19; this.pendingCycles -= 19;
							TUS = RegSP.Value16; TBL = ReadMemory(TUS++); TBH = ReadMemory(TUS--);
							WriteMemory(TUS++, RegHL.Low8); WriteMemory(TUS, RegHL.High8);
							RegHL.Low8 = TBL; RegHL.High8 = TBH;
							break;
						case 0xE4: // CALL C, nn
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (RegFlagC) {
								this.totalExecutedCycles += 17; this.pendingCycles -= 17;
								WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
								RegPC.Value16 = TUS;
							} else {
								this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							}
							break;
						case 0xE5: // PUSH HL
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							WriteMemory(--RegSP.Value16, RegHL.High8); WriteMemory(--RegSP.Value16, RegHL.Low8);
							break;
						case 0xE6: // AND n
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							RegAF.Value16 = TableALU[4, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
							break;
						case 0xE7: // RST $20
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
							RegPC.Value16 = 0x20;
							break;
						case 0xE8: // RET PE
							if (RegFlagP) {
								this.totalExecutedCycles += 11; this.pendingCycles -= 11;
								RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
							} else {
								this.totalExecutedCycles += 5; this.pendingCycles -= 5;
							}
							break;
						case 0xE9: // JP HL
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							RegPC.Value16 = RegHL.Value16;
							break;
						case 0xEA: // JP PE, nn
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (RegFlagP) {
								RegPC.Value16 = TUS;
							}
							break;
						case 0xEB: // EX DE, HL
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							TUS = RegDE.Value16; RegDE.Value16 = RegHL.Value16; RegHL.Value16 = TUS;
							break;
						case 0xEC: // CALL PE, nn
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (RegFlagP) {
								this.totalExecutedCycles += 17; this.pendingCycles -= 17;
								WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
								RegPC.Value16 = TUS;
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
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									RegBC.High8 = ReadHardware((ushort)RegBC.Low8);
									RegFlagS = RegBC.High8 > 127;
									RegFlagZ = RegBC.High8 == 0;
									RegFlagH = false;
									RegFlagP = TableParity[RegBC.High8];
									RegFlagN = false;
									break;
								case 0x41: // OUT C, B
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									WriteHardware(RegBC.Low8, RegBC.High8);
									this.totalExecutedCycles += 3; this.pendingCycles -= 3;
									break;
								case 0x42: // SBC HL, BC
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
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
									break;
								case 0x43: // LD (nn), BC
									this.totalExecutedCycles += 20; this.pendingCycles -= 20;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									WriteMemory(TUS++, RegBC.Low8);
									WriteMemory(TUS, RegBC.High8);
									break;
								case 0x44: // NEG
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegAF.Value16 = TableNeg[RegAF.Value16];
									break;
								case 0x45: // RETN
									this.totalExecutedCycles += 14; this.pendingCycles -= 14;
									RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									this.IFF1 = this.IFF2;
									break;
								case 0x46: // IM $0
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									interruptMode = 0;
									break;
								case 0x47: // LD I, A
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegI = RegAF.High8;
									break;
								case 0x48: // IN C, C
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									RegBC.Low8 = ReadHardware((ushort)RegBC.Low8);
									RegFlagS = RegBC.Low8 > 127;
									RegFlagZ = RegBC.Low8 == 0;
									RegFlagH = false;
									RegFlagP = TableParity[RegBC.Low8];
									RegFlagN = false;
									break;
								case 0x49: // OUT C, C
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									WriteHardware(RegBC.Low8, RegBC.Low8);
									this.totalExecutedCycles += 3; this.pendingCycles -= 3;
									break;
								case 0x4A: // ADC HL, BC
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
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
									break;
								case 0x4B: // LD BC, (nn)
									this.totalExecutedCycles += 20; this.pendingCycles -= 20;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									RegBC.Low8 = ReadMemory(TUS++); RegBC.High8 = ReadMemory(TUS);
									break;
								case 0x4C: // NEG
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegAF.Value16 = TableNeg[RegAF.Value16];
									break;
								case 0x4D: // RETI
									this.totalExecutedCycles += 14; this.pendingCycles -= 14;
									RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									break;
								case 0x4E: // IM $0
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									interruptMode = 0;
									break;
								case 0x4F: // LD R, A
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegR = RegAF.High8;
									break;
								case 0x50: // IN D, C
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									RegDE.High8 = ReadHardware((ushort)RegBC.Low8);
									RegFlagS = RegDE.High8 > 127;
									RegFlagZ = RegDE.High8 == 0;
									RegFlagH = false;
									RegFlagP = TableParity[RegDE.High8];
									RegFlagN = false;
									break;
								case 0x51: // OUT C, D
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									WriteHardware(RegBC.Low8, RegDE.High8);
									this.totalExecutedCycles += 3; this.pendingCycles -= 3;
									break;
								case 0x52: // SBC HL, DE
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
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
									break;
								case 0x53: // LD (nn), DE
									this.totalExecutedCycles += 20; this.pendingCycles -= 20;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									WriteMemory(TUS++, RegDE.Low8);
									WriteMemory(TUS, RegDE.High8);
									break;
								case 0x54: // NEG
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegAF.Value16 = TableNeg[RegAF.Value16];
									break;
								case 0x55: // RETN
									this.totalExecutedCycles += 14; this.pendingCycles -= 14;
									RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									this.IFF1 = this.IFF2;
									break;
								case 0x56: // IM $1
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									interruptMode = 1;
									break;
								case 0x57: // LD A, I
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.High8 = RegI;
									RegFlagS = RegI > 127;
									RegFlagZ = RegI == 0;
									RegFlagH = false;
									RegFlagN = false;
									RegFlagP = this.IFF2;
									break;
								case 0x58: // IN E, C
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									RegDE.Low8 = ReadHardware((ushort)RegBC.Low8);
									RegFlagS = RegDE.Low8 > 127;
									RegFlagZ = RegDE.Low8 == 0;
									RegFlagH = false;
									RegFlagP = TableParity[RegDE.Low8];
									RegFlagN = false;
									break;
								case 0x59: // OUT C, E
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									WriteHardware(RegBC.Low8, RegDE.Low8);
									this.totalExecutedCycles += 3; this.pendingCycles -= 3;
									break;
								case 0x5A: // ADC HL, DE
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
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
									break;
								case 0x5B: // LD DE, (nn)
									this.totalExecutedCycles += 20; this.pendingCycles -= 20;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									RegDE.Low8 = ReadMemory(TUS++); RegDE.High8 = ReadMemory(TUS);
									break;
								case 0x5C: // NEG
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegAF.Value16 = TableNeg[RegAF.Value16];
									break;
								case 0x5D: // RETI
									this.totalExecutedCycles += 14; this.pendingCycles -= 14;
									RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									break;
								case 0x5E: // IM $2
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									interruptMode = 2;
									break;
								case 0x5F: // LD A, R
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.High8 = (byte)(RegR & 0x7F);
									RegFlagS = (byte)(RegR & 0x7F) > 127;
									RegFlagZ = (byte)(RegR & 0x7F) == 0;
									RegFlagH = false;
									RegFlagN = false;
									RegFlagP = this.IFF2;
									break;
								case 0x60: // IN H, C
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									RegHL.High8 = ReadHardware((ushort)RegBC.Low8);
									RegFlagS = RegHL.High8 > 127;
									RegFlagZ = RegHL.High8 == 0;
									RegFlagH = false;
									RegFlagP = TableParity[RegHL.High8];
									RegFlagN = false;
									break;
								case 0x61: // OUT C, H
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									WriteHardware(RegBC.Low8, RegHL.High8);
									this.totalExecutedCycles += 3; this.pendingCycles -= 3;
									break;
								case 0x62: // SBC HL, HL
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
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
									break;
								case 0x63: // LD (nn), HL
									this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									WriteMemory(TUS++, RegHL.Low8);
									WriteMemory(TUS, RegHL.High8);
									break;
								case 0x64: // NEG
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegAF.Value16 = TableNeg[RegAF.Value16];
									break;
								case 0x65: // RETN
									this.totalExecutedCycles += 14; this.pendingCycles -= 14;
									RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									this.IFF1 = this.IFF2;
									break;
								case 0x66: // IM $0
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									interruptMode = 0;
									break;
								case 0x67: // RRD
									this.totalExecutedCycles += 18; this.pendingCycles -= 18;
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
									break;
								case 0x68: // IN L, C
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									RegHL.Low8 = ReadHardware((ushort)RegBC.Low8);
									RegFlagS = RegHL.Low8 > 127;
									RegFlagZ = RegHL.Low8 == 0;
									RegFlagH = false;
									RegFlagP = TableParity[RegHL.Low8];
									RegFlagN = false;
									break;
								case 0x69: // OUT C, L
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									WriteHardware(RegBC.Low8, RegHL.Low8);
									this.totalExecutedCycles += 3; this.pendingCycles -= 3;
									break;
								case 0x6A: // ADC HL, HL
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
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
									break;
								case 0x6B: // LD HL, (nn)
									this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									RegHL.Low8 = ReadMemory(TUS++); RegHL.High8 = ReadMemory(TUS);
									break;
								case 0x6C: // NEG
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegAF.Value16 = TableNeg[RegAF.Value16];
									break;
								case 0x6D: // RETI
									this.totalExecutedCycles += 14; this.pendingCycles -= 14;
									RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									break;
								case 0x6E: // IM $0
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									interruptMode = 0;
									break;
								case 0x6F: // RLD
									this.totalExecutedCycles += 18; this.pendingCycles -= 18;
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
									break;
								case 0x70: // IN 0, C
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									TB = ReadHardware((ushort)RegBC.Low8);
									RegFlagS = TB > 127;
									RegFlagZ = TB == 0;
									RegFlagH = false;
									RegFlagP = TableParity[TB];
									RegFlagN = false;
									break;
								case 0x71: // OUT C, 0
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									WriteHardware(RegBC.Low8, 0);
									this.totalExecutedCycles += 3; this.pendingCycles -= 3;
									break;
								case 0x72: // SBC HL, SP
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
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
									break;
								case 0x73: // LD (nn), SP
									this.totalExecutedCycles += 20; this.pendingCycles -= 20;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									WriteMemory(TUS++, RegSP.Low8);
									WriteMemory(TUS, RegSP.High8);
									break;
								case 0x74: // NEG
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegAF.Value16 = TableNeg[RegAF.Value16];
									break;
								case 0x75: // RETN
									this.totalExecutedCycles += 14; this.pendingCycles -= 14;
									RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									this.IFF1 = this.IFF2;
									break;
								case 0x76: // IM $1
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									interruptMode = 1;
									break;
								case 0x77: // NOP
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x78: // IN A, C
									this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									RegAF.High8 = ReadHardware((ushort)RegBC.Low8);
									RegFlagS = RegAF.High8 > 127;
									RegFlagZ = RegAF.High8 == 0;
									RegFlagH = false;
									RegFlagP = TableParity[RegAF.High8];
									RegFlagN = false;
									break;
								case 0x79: // OUT C, A
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									WriteHardware(RegBC.Low8, RegAF.High8);
									this.totalExecutedCycles += 3; this.pendingCycles -= 3;
									break;
								case 0x7A: // ADC HL, SP
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
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
									break;
								case 0x7B: // LD SP, (nn)
									this.totalExecutedCycles += 20; this.pendingCycles -= 20;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									RegSP.Low8 = ReadMemory(TUS++); RegSP.High8 = ReadMemory(TUS);
									break;
								case 0x7C: // NEG
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegAF.Value16 = TableNeg[RegAF.Value16];
									break;
								case 0x7D: // RETI
									this.totalExecutedCycles += 14; this.pendingCycles -= 14;
									RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									break;
								case 0x7E: // IM $2
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									interruptMode = 2;
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
									this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									WriteMemory(RegDE.Value16++, TB1 = ReadMemory(RegHL.Value16++));
									TB1 += RegAF.High8; RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
									--RegBC.Value16;
									RegFlagP = RegBC.Value16 != 0;
									RegFlagH = false;
									RegFlagN = false;
									break;
								case 0xA1: // CPI
									this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									TB1 = ReadMemory(RegHL.Value16++); TB2 = (byte)(RegAF.High8 - TB1);
									RegFlagN = true;
									RegFlagH = TableHalfBorrow[RegAF.High8, TB1];
									RegFlagZ = TB2 == 0;
									RegFlagS = TB2 > 127;
									TB1 = (byte)(RegAF.High8 - TB1 - (RegFlagH ? 1 : 0)); RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
									--RegBC.Value16;
									RegFlagP = RegBC.Value16 != 0;
									break;
								case 0xA2: // INI
									this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									WriteMemory(RegHL.Value16++, ReadHardware(RegBC.Value16));
									--RegBC.High8;
									RegFlagZ = RegBC.High8 == 0;
									RegFlagN = true;
									break;
								case 0xA3: // OUTI
									this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									WriteHardware(RegBC.Value16, ReadMemory(RegHL.Value16++));
									--RegBC.High8;
									RegFlagZ = RegBC.High8 == 0;
									RegFlagN = true;
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
									this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									WriteMemory(RegDE.Value16--, TB1 = ReadMemory(RegHL.Value16--));
									TB1 += RegAF.High8; RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
									--RegBC.Value16;
									RegFlagP = RegBC.Value16 != 0;
									RegFlagH = false;
									RegFlagN = false;
									break;
								case 0xA9: // CPD
									this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									TB1 = ReadMemory(RegHL.Value16--); TB2 = (byte)(RegAF.High8 - TB1);
									RegFlagN = true;
									RegFlagH = TableHalfBorrow[RegAF.High8, TB1];
									RegFlagZ = TB2 == 0;
									RegFlagS = TB2 > 127;
									TB1 = (byte)(RegAF.High8 - TB1 - (RegFlagH ? 1 : 0)); RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
									--RegBC.Value16;
									RegFlagP = RegBC.Value16 != 0;
									break;
								case 0xAA: // IND
									this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									WriteMemory(RegHL.Value16--, ReadHardware(RegBC.Value16));
									--RegBC.High8;
									RegFlagZ = RegBC.High8 == 0;
									RegFlagN = true;
									break;
								case 0xAB: // OUTD
									this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									WriteHardware(RegBC.Value16, ReadMemory(RegHL.Value16--));
									--RegBC.High8;
									RegFlagZ = RegBC.High8 == 0;
									RegFlagN = true;
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
									this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									WriteMemory(RegDE.Value16++, TB1 = ReadMemory(RegHL.Value16++));
									TB1 += RegAF.High8; RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
									--RegBC.Value16;
									RegFlagP = RegBC.Value16 != 0;
									RegFlagH = false;
									RegFlagN = false;
									if (RegBC.Value16 != 0) {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
										RegPC.Value16 -= 2;
									}
									break;
								case 0xB1: // CPIR
									this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									TB1 = ReadMemory(RegHL.Value16++); TB2 = (byte)(RegAF.High8 - TB1);
									RegFlagN = true;
									RegFlagH = TableHalfBorrow[RegAF.High8, TB1];
									RegFlagZ = TB2 == 0;
									RegFlagS = TB2 > 127;
									TB1 = (byte)(RegAF.High8 - TB1 - (RegFlagH ? 1 : 0)); RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
									--RegBC.Value16;
									RegFlagP = RegBC.Value16 != 0;
									if (RegBC.Value16 != 0 && !RegFlagZ) {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
										RegPC.Value16 -= 2;
									}
									break;
								case 0xB2: // INIR
									this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									WriteMemory(RegHL.Value16++, ReadHardware(RegBC.Value16));
									--RegBC.High8;
									RegFlagZ = RegBC.High8 == 0;
									RegFlagN = true;
									if (RegBC.High8 != 0) {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
										RegPC.Value16 -= 2;
									}
									break;
								case 0xB3: // OTIR
									this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									WriteHardware(RegBC.Value16, ReadMemory(RegHL.Value16++));
									--RegBC.High8;
									RegFlagZ = RegBC.High8 == 0;
									RegFlagN = true;
									if (RegBC.High8 != 0) {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
										RegPC.Value16 -= 2;
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
									this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									WriteMemory(RegDE.Value16--, TB1 = ReadMemory(RegHL.Value16--));
									TB1 += RegAF.High8; RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
									--RegBC.Value16;
									RegFlagP = RegBC.Value16 != 0;
									RegFlagH = false;
									RegFlagN = false;
									if (RegBC.Value16 != 0) {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
										RegPC.Value16 -= 2;
									}
									break;
								case 0xB9: // CPDR
									this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									TB1 = ReadMemory(RegHL.Value16--); TB2 = (byte)(RegAF.High8 - TB1);
									RegFlagN = true;
									RegFlagH = TableHalfBorrow[RegAF.High8, TB1];
									RegFlagZ = TB2 == 0;
									RegFlagS = TB2 > 127;
									TB1 = (byte)(RegAF.High8 - TB1 - (RegFlagH ? 1 : 0)); RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
									--RegBC.Value16;
									RegFlagP = RegBC.Value16 != 0;
									if (RegBC.Value16 != 0 && !RegFlagZ) {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
										RegPC.Value16 -= 2;
									}
									break;
								case 0xBA: // INDR
									this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									WriteMemory(RegHL.Value16--, ReadHardware(RegBC.Value16));
									--RegBC.High8;
									RegFlagZ = RegBC.High8 == 0;
									RegFlagN = true;
									if (RegBC.High8 != 0) {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
										RegPC.Value16 -= 2;
									}
									break;
								case 0xBB: // OTDR
									this.totalExecutedCycles += 16; this.pendingCycles -= 16;
									WriteHardware(RegBC.Value16, ReadMemory(RegHL.Value16--));
									--RegBC.High8;
									RegFlagZ = RegBC.High8 == 0;
									RegFlagN = true;
									if (RegBC.High8 != 0) {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
										RegPC.Value16 -= 2;
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
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							RegAF.Value16 = TableALU[5, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
							break;
						case 0xEF: // RST $28
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
							RegPC.Value16 = 0x28;
							break;
						case 0xF0: // RET P
							if (!RegFlagS) {
								this.totalExecutedCycles += 11; this.pendingCycles -= 11;
								RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
							} else {
								this.totalExecutedCycles += 5; this.pendingCycles -= 5;
							}
							break;
						case 0xF1: // POP AF
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							RegAF.Low8 = ReadMemory(RegSP.Value16++); RegAF.High8 = ReadMemory(RegSP.Value16++);
							break;
						case 0xF2: // JP P, nn
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (!RegFlagS) {
								RegPC.Value16 = TUS;
							}
							break;
						case 0xF3: // DI
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							this.IFF1 = this.IFF2 = false;
							break;
						case 0xF4: // CALL P, nn
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (!RegFlagS) {
								this.totalExecutedCycles += 17; this.pendingCycles -= 17;
								WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
								RegPC.Value16 = TUS;
							} else {
								this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							}
							break;
						case 0xF5: // PUSH AF
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							WriteMemory(--RegSP.Value16, RegAF.High8); WriteMemory(--RegSP.Value16, RegAF.Low8);
							break;
						case 0xF6: // OR n
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							RegAF.Value16 = TableALU[6, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
							break;
						case 0xF7: // RST $30
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
							RegPC.Value16 = 0x30;
							break;
						case 0xF8: // RET M
							if (RegFlagS) {
								this.totalExecutedCycles += 11; this.pendingCycles -= 11;
								RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
							} else {
								this.totalExecutedCycles += 5; this.pendingCycles -= 5;
							}
							break;
						case 0xF9: // LD SP, HL
							this.totalExecutedCycles += 6; this.pendingCycles -= 6;
							RegSP.Value16 = RegHL.Value16;
							break;
						case 0xFA: // JP M, nn
							this.totalExecutedCycles += 10; this.pendingCycles -= 10;
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (RegFlagS) {
								RegPC.Value16 = TUS;
							}
							break;
						case 0xFB: // EI
							this.totalExecutedCycles += 4; this.pendingCycles -= 4;
							this.IFF1 = this.IFF2 = true;
							Interruptable = false;
							break;
						case 0xFC: // CALL M, nn
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (RegFlagS) {
								this.totalExecutedCycles += 17; this.pendingCycles -= 17;
								WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
								RegPC.Value16 = TUS;
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
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									RegBC.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									break;
								case 0x02: // LD (BC), A
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									WriteMemory(RegBC.Value16, RegAF.High8);
									break;
								case 0x03: // INC BC
									this.totalExecutedCycles += 6; this.pendingCycles -= 6;
									++RegBC.Value16;
									break;
								case 0x04: // INC B
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Low8 = (byte)(TableInc[++RegBC.High8] | (RegAF.Low8 & 1));
									break;
								case 0x05: // DEC B
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Low8 = (byte)(TableDec[--RegBC.High8] | (RegAF.Low8 & 1));
									break;
								case 0x06: // LD B, n
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									RegBC.High8 = ReadMemory(RegPC.Value16++);
									break;
								case 0x07: // RLCA
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableRotShift[0, 0, RegAF.Value16];
									break;
								case 0x08: // EX AF, AF'
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									TUS = RegAF.Value16; RegAF.Value16 = RegAltAF.Value16; RegAltAF.Value16 = TUS;
									break;
								case 0x09: // ADD IY, BC
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									TI1 = (short)RegIY.Value16; TI2 = (short)RegBC.Value16; TIR = TI1 + TI2;
									TUS = (ushort)TIR;
									RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
									RegFlagN = false;
									RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
									RegIY.Value16 = TUS;
									RegFlag3 = (TUS & 0x0800) != 0;
									RegFlag5 = (TUS & 0x2000) != 0;
									break;
								case 0x0A: // LD A, (BC)
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									RegAF.High8 = ReadMemory(RegBC.Value16);
									break;
								case 0x0B: // DEC BC
									this.totalExecutedCycles += 6; this.pendingCycles -= 6;
									--RegBC.Value16;
									break;
								case 0x0C: // INC C
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Low8 = (byte)(TableInc[++RegBC.Low8] | (RegAF.Low8 & 1));
									break;
								case 0x0D: // DEC C
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Low8 = (byte)(TableDec[--RegBC.Low8] | (RegAF.Low8 & 1));
									break;
								case 0x0E: // LD C, n
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									RegBC.Low8 = ReadMemory(RegPC.Value16++);
									break;
								case 0x0F: // RRCA
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableRotShift[0, 1, RegAF.Value16];
									break;
								case 0x10: // DJNZ d
									TSB = (sbyte)ReadMemory(RegPC.Value16++);
									if (--RegBC.High8 != 0) {
										this.totalExecutedCycles += 13; this.pendingCycles -= 13;
										RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
									} else {
										this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									}
									break;
								case 0x11: // LD DE, nn
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									RegDE.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									break;
								case 0x12: // LD (DE), A
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									WriteMemory(RegDE.Value16, RegAF.High8);
									break;
								case 0x13: // INC DE
									this.totalExecutedCycles += 6; this.pendingCycles -= 6;
									++RegDE.Value16;
									break;
								case 0x14: // INC D
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Low8 = (byte)(TableInc[++RegDE.High8] | (RegAF.Low8 & 1));
									break;
								case 0x15: // DEC D
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Low8 = (byte)(TableDec[--RegDE.High8] | (RegAF.Low8 & 1));
									break;
								case 0x16: // LD D, n
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									RegDE.High8 = ReadMemory(RegPC.Value16++);
									break;
								case 0x17: // RLA
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableRotShift[0, 2, RegAF.Value16];
									break;
								case 0x18: // JR d
									TSB = (sbyte)ReadMemory(RegPC.Value16++);
										this.totalExecutedCycles += 12; this.pendingCycles -= 12;
									RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
									break;
								case 0x19: // ADD IY, DE
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									TI1 = (short)RegIY.Value16; TI2 = (short)RegDE.Value16; TIR = TI1 + TI2;
									TUS = (ushort)TIR;
									RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
									RegFlagN = false;
									RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
									RegIY.Value16 = TUS;
									RegFlag3 = (TUS & 0x0800) != 0;
									RegFlag5 = (TUS & 0x2000) != 0;
									break;
								case 0x1A: // LD A, (DE)
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									RegAF.High8 = ReadMemory(RegDE.Value16);
									break;
								case 0x1B: // DEC DE
									this.totalExecutedCycles += 6; this.pendingCycles -= 6;
									--RegDE.Value16;
									break;
								case 0x1C: // INC E
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Low8 = (byte)(TableInc[++RegDE.Low8] | (RegAF.Low8 & 1));
									break;
								case 0x1D: // DEC E
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Low8 = (byte)(TableDec[--RegDE.Low8] | (RegAF.Low8 & 1));
									break;
								case 0x1E: // LD E, n
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									RegDE.Low8 = ReadMemory(RegPC.Value16++);
									break;
								case 0x1F: // RRA
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableRotShift[0, 3, RegAF.Value16];
									break;
								case 0x20: // JR NZ, d
									TSB = (sbyte)ReadMemory(RegPC.Value16++);
									if (!RegFlagZ) {
										this.totalExecutedCycles += 12; this.pendingCycles -= 12;
										RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
									} else {
										this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									}
									break;
								case 0x21: // LD IY, nn
									this.totalExecutedCycles += 14; this.pendingCycles -= 14;
									RegIY.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									break;
								case 0x22: // LD (nn), IY
									this.totalExecutedCycles += 20; this.pendingCycles -= 20;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									WriteMemory(TUS++, RegIY.Low8);
									WriteMemory(TUS, RegIY.High8);
									break;
								case 0x23: // INC IY
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									++RegIY.Value16;
									break;
								case 0x24: // INC IYH
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Low8 = (byte)(TableInc[++RegIY.High8] | (RegAF.Low8 & 1));
									break;
								case 0x25: // DEC IYH
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Low8 = (byte)(TableDec[--RegIY.High8] | (RegAF.Low8 & 1));
									break;
								case 0x26: // LD IYH, n
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegIY.High8 = ReadMemory(RegPC.Value16++);
									break;
								case 0x27: // DAA
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableDaa[RegAF.Value16];
									break;
								case 0x28: // JR Z, d
									TSB = (sbyte)ReadMemory(RegPC.Value16++);
									if (RegFlagZ) {
										this.totalExecutedCycles += 12; this.pendingCycles -= 12;
										RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
									} else {
										this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									}
									break;
								case 0x29: // ADD IY, IY
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									TI1 = (short)RegIY.Value16; TI2 = (short)RegIY.Value16; TIR = TI1 + TI2;
									TUS = (ushort)TIR;
									RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
									RegFlagN = false;
									RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
									RegIY.Value16 = TUS;
									RegFlag3 = (TUS & 0x0800) != 0;
									RegFlag5 = (TUS & 0x2000) != 0;
									break;
								case 0x2A: // LD IY, (nn)
									this.totalExecutedCycles += 20; this.pendingCycles -= 20;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									RegIY.Low8 = ReadMemory(TUS++); RegIY.High8 = ReadMemory(TUS);
									break;
								case 0x2B: // DEC IY
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									--RegIY.Value16;
									break;
								case 0x2C: // INC IYL
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Low8 = (byte)(TableInc[++RegIY.Low8] | (RegAF.Low8 & 1));
									break;
								case 0x2D: // DEC IYL
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Low8 = (byte)(TableDec[--RegIY.Low8] | (RegAF.Low8 & 1));
									break;
								case 0x2E: // LD IYL, n
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegIY.Low8 = ReadMemory(RegPC.Value16++);
									break;
								case 0x2F: // CPL
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.High8 ^= 0xFF; RegFlagH = true; RegFlagN = true; RegFlag3 = (RegAF.High8 & 0x08) != 0; RegFlag5 = (RegAF.High8 & 0x20) != 0;
									break;
								case 0x30: // JR NC, d
									TSB = (sbyte)ReadMemory(RegPC.Value16++);
									if (!RegFlagC) {
										this.totalExecutedCycles += 12; this.pendingCycles -= 12;
										RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
									} else {
										this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									}
									break;
								case 0x31: // LD SP, nn
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									RegSP.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									break;
								case 0x32: // LD (nn), A
									this.totalExecutedCycles += 13; this.pendingCycles -= 13;
									WriteMemory((ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256), RegAF.High8);
									break;
								case 0x33: // INC SP
									this.totalExecutedCycles += 6; this.pendingCycles -= 6;
									++RegSP.Value16;
									break;
								case 0x34: // INC (IY+d)
									this.totalExecutedCycles += 23; this.pendingCycles -= 23;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									TB = ReadMemory((ushort)(RegIY.Value16 + Displacement)); RegAF.Low8 = (byte)(TableInc[++TB] | (RegAF.Low8 & 1)); WriteMemory((ushort)(RegIY.Value16 + Displacement), TB);
									break;
								case 0x35: // DEC (IY+d)
									this.totalExecutedCycles += 23; this.pendingCycles -= 23;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									TB = ReadMemory((ushort)(RegIY.Value16 + Displacement)); RegAF.Low8 = (byte)(TableDec[--TB] | (RegAF.Low8 & 1)); WriteMemory((ushort)(RegIY.Value16 + Displacement), TB);
									break;
								case 0x36: // LD (IY+d), n
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIY.Value16 + Displacement), ReadMemory(RegPC.Value16++));
									break;
								case 0x37: // SCF
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegFlagH = false; RegFlagN = false; RegFlagC = true; RegFlag3 = (RegAF.High8 & 0x08) != 0; RegFlag5 = (RegAF.High8 & 0x20) != 0;
									break;
								case 0x38: // JR C, d
									TSB = (sbyte)ReadMemory(RegPC.Value16++);
									if (RegFlagC) {
										this.totalExecutedCycles += 12; this.pendingCycles -= 12;
										RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
									} else {
										this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									}
									break;
								case 0x39: // ADD IY, SP
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									TI1 = (short)RegIY.Value16; TI2 = (short)RegSP.Value16; TIR = TI1 + TI2;
									TUS = (ushort)TIR;
									RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
									RegFlagN = false;
									RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
									RegIY.Value16 = TUS;
									RegFlag3 = (TUS & 0x0800) != 0;
									RegFlag5 = (TUS & 0x2000) != 0;
									break;
								case 0x3A: // LD A, (nn)
									this.totalExecutedCycles += 13; this.pendingCycles -= 13;
									RegAF.High8 = ReadMemory((ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256));
									break;
								case 0x3B: // DEC SP
									this.totalExecutedCycles += 6; this.pendingCycles -= 6;
									--RegSP.Value16;
									break;
								case 0x3C: // INC A
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Low8 = (byte)(TableInc[++RegAF.High8] | (RegAF.Low8 & 1));
									break;
								case 0x3D: // DEC A
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Low8 = (byte)(TableDec[--RegAF.High8] | (RegAF.Low8 & 1));
									break;
								case 0x3E: // LD A, n
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									RegAF.High8 = ReadMemory(RegPC.Value16++);
									break;
								case 0x3F: // CCF
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegFlagH = RegFlagC; RegFlagN = false; RegFlagC ^= true; RegFlag3 = (RegAF.High8 & 0x08) != 0; RegFlag5 = (RegAF.High8 & 0x20) != 0;
									break;
								case 0x40: // LD B, B
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x41: // LD B, C
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegBC.High8 = RegBC.Low8;
									break;
								case 0x42: // LD B, D
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegBC.High8 = RegDE.High8;
									break;
								case 0x43: // LD B, E
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegBC.High8 = RegDE.Low8;
									break;
								case 0x44: // LD B, IYH
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegBC.High8 = RegIY.High8;
									break;
								case 0x45: // LD B, IYL
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegBC.High8 = RegIY.Low8;
									break;
								case 0x46: // LD B, (IY+d)
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegBC.High8 = ReadMemory((ushort)(RegIY.Value16 + Displacement));
									break;
								case 0x47: // LD B, A
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegBC.High8 = RegAF.High8;
									break;
								case 0x48: // LD C, B
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegBC.Low8 = RegBC.High8;
									break;
								case 0x49: // LD C, C
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x4A: // LD C, D
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegBC.Low8 = RegDE.High8;
									break;
								case 0x4B: // LD C, E
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegBC.Low8 = RegDE.Low8;
									break;
								case 0x4C: // LD C, IYH
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegBC.Low8 = RegIY.High8;
									break;
								case 0x4D: // LD C, IYL
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegBC.Low8 = RegIY.Low8;
									break;
								case 0x4E: // LD C, (IY+d)
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegBC.Low8 = ReadMemory((ushort)(RegIY.Value16 + Displacement));
									break;
								case 0x4F: // LD C, A
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegBC.Low8 = RegAF.High8;
									break;
								case 0x50: // LD D, B
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegDE.High8 = RegBC.High8;
									break;
								case 0x51: // LD D, C
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegDE.High8 = RegBC.Low8;
									break;
								case 0x52: // LD D, D
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x53: // LD D, E
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegDE.High8 = RegDE.Low8;
									break;
								case 0x54: // LD D, IYH
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegDE.High8 = RegIY.High8;
									break;
								case 0x55: // LD D, IYL
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegDE.High8 = RegIY.Low8;
									break;
								case 0x56: // LD D, (IY+d)
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegDE.High8 = ReadMemory((ushort)(RegIY.Value16 + Displacement));
									break;
								case 0x57: // LD D, A
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegDE.High8 = RegAF.High8;
									break;
								case 0x58: // LD E, B
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegDE.Low8 = RegBC.High8;
									break;
								case 0x59: // LD E, C
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegDE.Low8 = RegBC.Low8;
									break;
								case 0x5A: // LD E, D
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegDE.Low8 = RegDE.High8;
									break;
								case 0x5B: // LD E, E
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x5C: // LD E, IYH
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegDE.Low8 = RegIY.High8;
									break;
								case 0x5D: // LD E, IYL
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegDE.Low8 = RegIY.Low8;
									break;
								case 0x5E: // LD E, (IY+d)
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegDE.Low8 = ReadMemory((ushort)(RegIY.Value16 + Displacement));
									break;
								case 0x5F: // LD E, A
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegDE.Low8 = RegAF.High8;
									break;
								case 0x60: // LD IYH, B
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegIY.High8 = RegBC.High8;
									break;
								case 0x61: // LD IYH, C
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegIY.High8 = RegBC.Low8;
									break;
								case 0x62: // LD IYH, D
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegIY.High8 = RegDE.High8;
									break;
								case 0x63: // LD IYH, E
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegIY.High8 = RegDE.Low8;
									break;
								case 0x64: // LD IYH, IYH
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x65: // LD IYH, IYL
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegIY.High8 = RegIY.Low8;
									break;
								case 0x66: // LD H, (IY+d)
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegHL.High8 = ReadMemory((ushort)(RegIY.Value16 + Displacement));
									break;
								case 0x67: // LD IYH, A
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegIY.High8 = RegAF.High8;
									break;
								case 0x68: // LD IYL, B
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegIY.Low8 = RegBC.High8;
									break;
								case 0x69: // LD IYL, C
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegIY.Low8 = RegBC.Low8;
									break;
								case 0x6A: // LD IYL, D
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegIY.Low8 = RegDE.High8;
									break;
								case 0x6B: // LD IYL, E
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegIY.Low8 = RegDE.Low8;
									break;
								case 0x6C: // LD IYL, IYH
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegIY.Low8 = RegIY.High8;
									break;
								case 0x6D: // LD IYL, IYL
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									break;
								case 0x6E: // LD L, (IY+d)
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegHL.Low8 = ReadMemory((ushort)(RegIY.Value16 + Displacement));
									break;
								case 0x6F: // LD IYL, A
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegIY.Low8 = RegAF.High8;
									break;
								case 0x70: // LD (IY+d), B
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIY.Value16 + Displacement), RegBC.High8);
									break;
								case 0x71: // LD (IY+d), C
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIY.Value16 + Displacement), RegBC.Low8);
									break;
								case 0x72: // LD (IY+d), D
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIY.Value16 + Displacement), RegDE.High8);
									break;
								case 0x73: // LD (IY+d), E
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIY.Value16 + Displacement), RegDE.Low8);
									break;
								case 0x74: // LD (IY+d), H
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIY.Value16 + Displacement), RegHL.High8);
									break;
								case 0x75: // LD (IY+d), L
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIY.Value16 + Displacement), RegHL.Low8);
									break;
								case 0x76: // HALT
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									this.Halt();
									break;
								case 0x77: // LD (IY+d), A
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIY.Value16 + Displacement), RegAF.High8);
									break;
								case 0x78: // LD A, B
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.High8 = RegBC.High8;
									break;
								case 0x79: // LD A, C
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.High8 = RegBC.Low8;
									break;
								case 0x7A: // LD A, D
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.High8 = RegDE.High8;
									break;
								case 0x7B: // LD A, E
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.High8 = RegDE.Low8;
									break;
								case 0x7C: // LD A, IYH
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.High8 = RegIY.High8;
									break;
								case 0x7D: // LD A, IYL
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.High8 = RegIY.Low8;
									break;
								case 0x7E: // LD A, (IY+d)
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.High8 = ReadMemory((ushort)(RegIY.Value16 + Displacement));
									break;
								case 0x7F: // LD A, A
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									break;
								case 0x80: // ADD A, B
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[0, RegAF.High8, RegBC.High8, 0];
									break;
								case 0x81: // ADD A, C
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[0, RegAF.High8, RegBC.Low8, 0];
									break;
								case 0x82: // ADD A, D
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[0, RegAF.High8, RegDE.High8, 0];
									break;
								case 0x83: // ADD A, E
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[0, RegAF.High8, RegDE.Low8, 0];
									break;
								case 0x84: // ADD A, IYH
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Value16 = TableALU[0, RegAF.High8, RegIY.High8, 0];
									break;
								case 0x85: // ADD A, IYL
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Value16 = TableALU[0, RegAF.High8, RegIY.Low8, 0];
									break;
								case 0x86: // ADD A, (IY+d)
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[0, RegAF.High8, ReadMemory((ushort)(RegIY.Value16 + Displacement)), 0];
									break;
								case 0x87: // ADD A, A
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[0, RegAF.High8, RegAF.High8, 0];
									break;
								case 0x88: // ADC A, B
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[1, RegAF.High8, RegBC.High8, RegFlagC ? 1 : 0];
									break;
								case 0x89: // ADC A, C
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[1, RegAF.High8, RegBC.Low8, RegFlagC ? 1 : 0];
									break;
								case 0x8A: // ADC A, D
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[1, RegAF.High8, RegDE.High8, RegFlagC ? 1 : 0];
									break;
								case 0x8B: // ADC A, E
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[1, RegAF.High8, RegDE.Low8, RegFlagC ? 1 : 0];
									break;
								case 0x8C: // ADC A, IYH
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Value16 = TableALU[1, RegAF.High8, RegIY.High8, RegFlagC ? 1 : 0];
									break;
								case 0x8D: // ADC A, IYL
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Value16 = TableALU[1, RegAF.High8, RegIY.Low8, RegFlagC ? 1 : 0];
									break;
								case 0x8E: // ADC A, (IY+d)
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[1, RegAF.High8, ReadMemory((ushort)(RegIY.Value16 + Displacement)), RegFlagC ? 1 : 0];
									break;
								case 0x8F: // ADC A, A
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[1, RegAF.High8, RegAF.High8, RegFlagC ? 1 : 0];
									break;
								case 0x90: // SUB B
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[2, RegAF.High8, RegBC.High8, 0];
									break;
								case 0x91: // SUB C
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[2, RegAF.High8, RegBC.Low8, 0];
									break;
								case 0x92: // SUB D
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[2, RegAF.High8, RegDE.High8, 0];
									break;
								case 0x93: // SUB E
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[2, RegAF.High8, RegDE.Low8, 0];
									break;
								case 0x94: // SUB IYH
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Value16 = TableALU[2, RegAF.High8, RegIY.High8, 0];
									break;
								case 0x95: // SUB IYL
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Value16 = TableALU[2, RegAF.High8, RegIY.Low8, 0];
									break;
								case 0x96: // SUB (IY+d)
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[2, RegAF.High8, ReadMemory((ushort)(RegIY.Value16 + Displacement)), 0];
									break;
								case 0x97: // SUB A, A
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[2, RegAF.High8, RegAF.High8, 0];
									break;
								case 0x98: // SBC A, B
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[3, RegAF.High8, RegBC.High8, RegFlagC ? 1 : 0];
									break;
								case 0x99: // SBC A, C
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[3, RegAF.High8, RegBC.Low8, RegFlagC ? 1 : 0];
									break;
								case 0x9A: // SBC A, D
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[3, RegAF.High8, RegDE.High8, RegFlagC ? 1 : 0];
									break;
								case 0x9B: // SBC A, E
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[3, RegAF.High8, RegDE.Low8, RegFlagC ? 1 : 0];
									break;
								case 0x9C: // SBC A, IYH
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Value16 = TableALU[3, RegAF.High8, RegIY.High8, RegFlagC ? 1 : 0];
									break;
								case 0x9D: // SBC A, IYL
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Value16 = TableALU[3, RegAF.High8, RegIY.Low8, RegFlagC ? 1 : 0];
									break;
								case 0x9E: // SBC A, (IY+d)
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[3, RegAF.High8, ReadMemory((ushort)(RegIY.Value16 + Displacement)), RegFlagC ? 1 : 0];
									break;
								case 0x9F: // SBC A, A
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[3, RegAF.High8, RegAF.High8, RegFlagC ? 1 : 0];
									break;
								case 0xA0: // AND B
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[4, RegAF.High8, RegBC.High8, 0];
									break;
								case 0xA1: // AND C
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[4, RegAF.High8, RegBC.Low8, 0];
									break;
								case 0xA2: // AND D
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[4, RegAF.High8, RegDE.High8, 0];
									break;
								case 0xA3: // AND E
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[4, RegAF.High8, RegDE.Low8, 0];
									break;
								case 0xA4: // AND IYH
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Value16 = TableALU[4, RegAF.High8, RegIY.High8, 0];
									break;
								case 0xA5: // AND IYL
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Value16 = TableALU[4, RegAF.High8, RegIY.Low8, 0];
									break;
								case 0xA6: // AND (IY+d)
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[4, RegAF.High8, ReadMemory((ushort)(RegIY.Value16 + Displacement)), 0];
									break;
								case 0xA7: // AND A
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[4, RegAF.High8, RegAF.High8, 0];
									break;
								case 0xA8: // XOR B
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[5, RegAF.High8, RegBC.High8, 0];
									break;
								case 0xA9: // XOR C
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[5, RegAF.High8, RegBC.Low8, 0];
									break;
								case 0xAA: // XOR D
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[5, RegAF.High8, RegDE.High8, 0];
									break;
								case 0xAB: // XOR E
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[5, RegAF.High8, RegDE.Low8, 0];
									break;
								case 0xAC: // XOR IYH
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Value16 = TableALU[5, RegAF.High8, RegIY.High8, 0];
									break;
								case 0xAD: // XOR IYL
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Value16 = TableALU[5, RegAF.High8, RegIY.Low8, 0];
									break;
								case 0xAE: // XOR (IY+d)
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[5, RegAF.High8, ReadMemory((ushort)(RegIY.Value16 + Displacement)), 0];
									break;
								case 0xAF: // XOR A
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[5, RegAF.High8, RegAF.High8, 0];
									break;
								case 0xB0: // OR B
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[6, RegAF.High8, RegBC.High8, 0];
									break;
								case 0xB1: // OR C
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[6, RegAF.High8, RegBC.Low8, 0];
									break;
								case 0xB2: // OR D
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[6, RegAF.High8, RegDE.High8, 0];
									break;
								case 0xB3: // OR E
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[6, RegAF.High8, RegDE.Low8, 0];
									break;
								case 0xB4: // OR IYH
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Value16 = TableALU[6, RegAF.High8, RegIY.High8, 0];
									break;
								case 0xB5: // OR IYL
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Value16 = TableALU[6, RegAF.High8, RegIY.Low8, 0];
									break;
								case 0xB6: // OR (IY+d)
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[6, RegAF.High8, ReadMemory((ushort)(RegIY.Value16 + Displacement)), 0];
									break;
								case 0xB7: // OR A
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[6, RegAF.High8, RegAF.High8, 0];
									break;
								case 0xB8: // CP B
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[7, RegAF.High8, RegBC.High8, 0];
									break;
								case 0xB9: // CP C
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[7, RegAF.High8, RegBC.Low8, 0];
									break;
								case 0xBA: // CP D
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[7, RegAF.High8, RegDE.High8, 0];
									break;
								case 0xBB: // CP E
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[7, RegAF.High8, RegDE.Low8, 0];
									break;
								case 0xBC: // CP IYH
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Value16 = TableALU[7, RegAF.High8, RegIY.High8, 0];
									break;
								case 0xBD: // CP IYL
									this.totalExecutedCycles += 9; this.pendingCycles -= 9;
									RegAF.Value16 = TableALU[7, RegAF.High8, RegIY.Low8, 0];
									break;
								case 0xBE: // CP (IY+d)
									this.totalExecutedCycles += 19; this.pendingCycles -= 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[7, RegAF.High8, ReadMemory((ushort)(RegIY.Value16 + Displacement)), 0];
									break;
								case 0xBF: // CP A
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									RegAF.Value16 = TableALU[7, RegAF.High8, RegAF.High8, 0];
									break;
								case 0xC0: // RET NZ
									if (!RegFlagZ) {
										this.totalExecutedCycles += 11; this.pendingCycles -= 11;
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									} else {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
									}
									break;
								case 0xC1: // POP BC
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									RegBC.Low8 = ReadMemory(RegSP.Value16++); RegBC.High8 = ReadMemory(RegSP.Value16++);
									break;
								case 0xC2: // JP NZ, nn
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagZ) {
										RegPC.Value16 = TUS;
									}
									break;
								case 0xC3: // JP nn
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									RegPC.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									break;
								case 0xC4: // CALL NZ, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagZ) {
										this.totalExecutedCycles += 17; this.pendingCycles -= 17;
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
									} else {
										this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									}
									break;
								case 0xC5: // PUSH BC
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									WriteMemory(--RegSP.Value16, RegBC.High8); WriteMemory(--RegSP.Value16, RegBC.Low8);
									break;
								case 0xC6: // ADD A, n
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									RegAF.Value16 = TableALU[0, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
									break;
								case 0xC7: // RST $00
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x00;
									break;
								case 0xC8: // RET Z
									if (RegFlagZ) {
										this.totalExecutedCycles += 11; this.pendingCycles -= 11;
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									} else {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
									}
									break;
								case 0xC9: // RET
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									break;
								case 0xCA: // JP Z, nn
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagZ) {
										RegPC.Value16 = TUS;
									}
									break;
								case 0xCB: // (Prefix)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									++RegR;
									switch (ReadMemory(RegPC.Value16++)) {
										case 0x00: // RLC (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x01: // RLC (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x02: // RLC (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x03: // RLC (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x04: // RLC (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x05: // RLC (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x06: // RLC (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x07: // RLC (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x08: // RRC (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x09: // RRC (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x0A: // RRC (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x0B: // RRC (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x0C: // RRC (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x0D: // RRC (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x0E: // RRC (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x0F: // RRC (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x10: // RL (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x11: // RL (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x12: // RL (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x13: // RL (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x14: // RL (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x15: // RL (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x16: // RL (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x17: // RL (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x18: // RR (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x19: // RR (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x1A: // RR (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x1B: // RR (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x1C: // RR (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x1D: // RR (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x1E: // RR (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x1F: // RR (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x20: // SLA (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x21: // SLA (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x22: // SLA (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x23: // SLA (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x24: // SLA (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x25: // SLA (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x26: // SLA (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x27: // SLA (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x28: // SRA (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x29: // SRA (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x2A: // SRA (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x2B: // SRA (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x2C: // SRA (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x2D: // SRA (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x2E: // SRA (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x2F: // SRA (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x30: // SL1 (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x31: // SL1 (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x32: // SL1 (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x33: // SL1 (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x34: // SL1 (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x35: // SL1 (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x36: // SL1 (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x37: // SL1 (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x38: // SRL (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x39: // SRL (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x3A: // SRL (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x3B: // SRL (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x3C: // SRL (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x3D: // SRL (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x3E: // SRL (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x3F: // SRL (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x40: // BIT 0, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x41: // BIT 0, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x42: // BIT 0, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x43: // BIT 0, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x44: // BIT 0, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x45: // BIT 0, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x46: // BIT 0, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x47: // BIT 0, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x48: // BIT 1, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x49: // BIT 1, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x4A: // BIT 1, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x4B: // BIT 1, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x4C: // BIT 1, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x4D: // BIT 1, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x4E: // BIT 1, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x4F: // BIT 1, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x50: // BIT 2, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x51: // BIT 2, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x52: // BIT 2, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x53: // BIT 2, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x54: // BIT 2, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x55: // BIT 2, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x56: // BIT 2, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x57: // BIT 2, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x58: // BIT 3, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x59: // BIT 3, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x5A: // BIT 3, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x5B: // BIT 3, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x5C: // BIT 3, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x5D: // BIT 3, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x5E: // BIT 3, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x5F: // BIT 3, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x60: // BIT 4, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x61: // BIT 4, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x62: // BIT 4, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x63: // BIT 4, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x64: // BIT 4, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x65: // BIT 4, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x66: // BIT 4, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x67: // BIT 4, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x68: // BIT 5, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x69: // BIT 5, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x6A: // BIT 5, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x6B: // BIT 5, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x6C: // BIT 5, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x6D: // BIT 5, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x6E: // BIT 5, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x6F: // BIT 5, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x70: // BIT 6, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x71: // BIT 6, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x72: // BIT 6, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x73: // BIT 6, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x74: // BIT 6, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x75: // BIT 6, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x76: // BIT 6, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x77: // BIT 6, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x78: // BIT 7, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x79: // BIT 7, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x7A: // BIT 7, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x7B: // BIT 7, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x7C: // BIT 7, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x7D: // BIT 7, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x7E: // BIT 7, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x7F: // BIT 7, (IY+d)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x80: // RES 0, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x01)));
											break;
										case 0x81: // RES 0, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x01)));
											break;
										case 0x82: // RES 0, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x01)));
											break;
										case 0x83: // RES 0, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x01)));
											break;
										case 0x84: // RES 0, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x01)));
											break;
										case 0x85: // RES 0, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x01)));
											break;
										case 0x86: // RES 0, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x01)));
											break;
										case 0x87: // RES 0, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x01)));
											break;
										case 0x88: // RES 1, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x02)));
											break;
										case 0x89: // RES 1, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x02)));
											break;
										case 0x8A: // RES 1, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x02)));
											break;
										case 0x8B: // RES 1, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x02)));
											break;
										case 0x8C: // RES 1, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x02)));
											break;
										case 0x8D: // RES 1, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x02)));
											break;
										case 0x8E: // RES 1, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x02)));
											break;
										case 0x8F: // RES 1, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x02)));
											break;
										case 0x90: // RES 2, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x04)));
											break;
										case 0x91: // RES 2, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x04)));
											break;
										case 0x92: // RES 2, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x04)));
											break;
										case 0x93: // RES 2, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x04)));
											break;
										case 0x94: // RES 2, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x04)));
											break;
										case 0x95: // RES 2, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x04)));
											break;
										case 0x96: // RES 2, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x04)));
											break;
										case 0x97: // RES 2, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x04)));
											break;
										case 0x98: // RES 3, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x08)));
											break;
										case 0x99: // RES 3, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x08)));
											break;
										case 0x9A: // RES 3, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x08)));
											break;
										case 0x9B: // RES 3, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x08)));
											break;
										case 0x9C: // RES 3, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x08)));
											break;
										case 0x9D: // RES 3, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x08)));
											break;
										case 0x9E: // RES 3, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x08)));
											break;
										case 0x9F: // RES 3, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x08)));
											break;
										case 0xA0: // RES 4, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x10)));
											break;
										case 0xA1: // RES 4, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x10)));
											break;
										case 0xA2: // RES 4, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x10)));
											break;
										case 0xA3: // RES 4, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x10)));
											break;
										case 0xA4: // RES 4, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x10)));
											break;
										case 0xA5: // RES 4, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x10)));
											break;
										case 0xA6: // RES 4, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x10)));
											break;
										case 0xA7: // RES 4, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x10)));
											break;
										case 0xA8: // RES 5, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x20)));
											break;
										case 0xA9: // RES 5, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x20)));
											break;
										case 0xAA: // RES 5, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x20)));
											break;
										case 0xAB: // RES 5, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x20)));
											break;
										case 0xAC: // RES 5, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x20)));
											break;
										case 0xAD: // RES 5, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x20)));
											break;
										case 0xAE: // RES 5, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x20)));
											break;
										case 0xAF: // RES 5, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x20)));
											break;
										case 0xB0: // RES 6, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x40)));
											break;
										case 0xB1: // RES 6, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x40)));
											break;
										case 0xB2: // RES 6, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x40)));
											break;
										case 0xB3: // RES 6, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x40)));
											break;
										case 0xB4: // RES 6, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x40)));
											break;
										case 0xB5: // RES 6, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x40)));
											break;
										case 0xB6: // RES 6, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x40)));
											break;
										case 0xB7: // RES 6, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x40)));
											break;
										case 0xB8: // RES 7, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x80)));
											break;
										case 0xB9: // RES 7, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x80)));
											break;
										case 0xBA: // RES 7, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x80)));
											break;
										case 0xBB: // RES 7, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x80)));
											break;
										case 0xBC: // RES 7, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x80)));
											break;
										case 0xBD: // RES 7, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x80)));
											break;
										case 0xBE: // RES 7, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x80)));
											break;
										case 0xBF: // RES 7, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x80)));
											break;
										case 0xC0: // SET 0, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x01)));
											break;
										case 0xC1: // SET 0, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x01)));
											break;
										case 0xC2: // SET 0, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x01)));
											break;
										case 0xC3: // SET 0, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x01)));
											break;
										case 0xC4: // SET 0, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x01)));
											break;
										case 0xC5: // SET 0, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x01)));
											break;
										case 0xC6: // SET 0, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x01)));
											break;
										case 0xC7: // SET 0, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x01)));
											break;
										case 0xC8: // SET 1, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x02)));
											break;
										case 0xC9: // SET 1, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x02)));
											break;
										case 0xCA: // SET 1, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x02)));
											break;
										case 0xCB: // SET 1, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x02)));
											break;
										case 0xCC: // SET 1, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x02)));
											break;
										case 0xCD: // SET 1, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x02)));
											break;
										case 0xCE: // SET 1, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x02)));
											break;
										case 0xCF: // SET 1, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x02)));
											break;
										case 0xD0: // SET 2, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x04)));
											break;
										case 0xD1: // SET 2, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x04)));
											break;
										case 0xD2: // SET 2, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x04)));
											break;
										case 0xD3: // SET 2, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x04)));
											break;
										case 0xD4: // SET 2, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x04)));
											break;
										case 0xD5: // SET 2, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x04)));
											break;
										case 0xD6: // SET 2, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x04)));
											break;
										case 0xD7: // SET 2, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x04)));
											break;
										case 0xD8: // SET 3, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x08)));
											break;
										case 0xD9: // SET 3, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x08)));
											break;
										case 0xDA: // SET 3, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x08)));
											break;
										case 0xDB: // SET 3, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x08)));
											break;
										case 0xDC: // SET 3, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x08)));
											break;
										case 0xDD: // SET 3, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x08)));
											break;
										case 0xDE: // SET 3, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x08)));
											break;
										case 0xDF: // SET 3, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x08)));
											break;
										case 0xE0: // SET 4, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x10)));
											break;
										case 0xE1: // SET 4, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x10)));
											break;
										case 0xE2: // SET 4, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x10)));
											break;
										case 0xE3: // SET 4, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x10)));
											break;
										case 0xE4: // SET 4, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x10)));
											break;
										case 0xE5: // SET 4, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x10)));
											break;
										case 0xE6: // SET 4, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x10)));
											break;
										case 0xE7: // SET 4, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x10)));
											break;
										case 0xE8: // SET 5, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x20)));
											break;
										case 0xE9: // SET 5, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x20)));
											break;
										case 0xEA: // SET 5, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x20)));
											break;
										case 0xEB: // SET 5, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x20)));
											break;
										case 0xEC: // SET 5, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x20)));
											break;
										case 0xED: // SET 5, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x20)));
											break;
										case 0xEE: // SET 5, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x20)));
											break;
										case 0xEF: // SET 5, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x20)));
											break;
										case 0xF0: // SET 6, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x40)));
											break;
										case 0xF1: // SET 6, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x40)));
											break;
										case 0xF2: // SET 6, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x40)));
											break;
										case 0xF3: // SET 6, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x40)));
											break;
										case 0xF4: // SET 6, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x40)));
											break;
										case 0xF5: // SET 6, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x40)));
											break;
										case 0xF6: // SET 6, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x40)));
											break;
										case 0xF7: // SET 6, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x40)));
											break;
										case 0xF8: // SET 7, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x80)));
											break;
										case 0xF9: // SET 7, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x80)));
											break;
										case 0xFA: // SET 7, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x80)));
											break;
										case 0xFB: // SET 7, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x80)));
											break;
										case 0xFC: // SET 7, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x80)));
											break;
										case 0xFD: // SET 7, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x80)));
											break;
										case 0xFE: // SET 7, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x80)));
											break;
										case 0xFF: // SET 7, (IY+d)
											this.totalExecutedCycles += 23; this.pendingCycles -= 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x80)));
											break;
									}
									break;
								case 0xCC: // CALL Z, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagZ) {
										this.totalExecutedCycles += 17; this.pendingCycles -= 17;
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
									} else {
										this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									}
									break;
								case 0xCD: // CALL nn
									this.totalExecutedCycles += 17; this.pendingCycles -= 17;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = TUS;
									break;
								case 0xCE: // ADC A, n
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									RegAF.Value16 = TableALU[1, RegAF.High8, ReadMemory(RegPC.Value16++), RegFlagC ? 1 : 0];
									break;
								case 0xCF: // RST $08
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x08;
									break;
								case 0xD0: // RET NC
									if (!RegFlagC) {
										this.totalExecutedCycles += 11; this.pendingCycles -= 11;
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									} else {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
									}
									break;
								case 0xD1: // POP DE
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									RegDE.Low8 = ReadMemory(RegSP.Value16++); RegDE.High8 = ReadMemory(RegSP.Value16++);
									break;
								case 0xD2: // JP NC, nn
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagC) {
										RegPC.Value16 = TUS;
									}
									break;
								case 0xD3: // OUT n, A
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									WriteHardware(ReadMemory(RegPC.Value16++), RegAF.High8);
									this.totalExecutedCycles += 3; this.pendingCycles -= 3;
									break;
								case 0xD4: // CALL NC, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagC) {
										this.totalExecutedCycles += 17; this.pendingCycles -= 17;
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
									} else {
										this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									}
									break;
								case 0xD5: // PUSH DE
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									WriteMemory(--RegSP.Value16, RegDE.High8); WriteMemory(--RegSP.Value16, RegDE.Low8);
									break;
								case 0xD6: // SUB n
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									RegAF.Value16 = TableALU[2, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
									break;
								case 0xD7: // RST $10
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x10;
									break;
								case 0xD8: // RET C
									if (RegFlagC) {
										this.totalExecutedCycles += 11; this.pendingCycles -= 11;
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									} else {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
									}
									break;
								case 0xD9: // EXX
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									TUS = RegBC.Value16; RegBC.Value16 = RegAltBC.Value16; RegAltBC.Value16 = TUS;
									TUS = RegDE.Value16; RegDE.Value16 = RegAltDE.Value16; RegAltDE.Value16 = TUS;
									TUS = RegHL.Value16; RegHL.Value16 = RegAltHL.Value16; RegAltHL.Value16 = TUS;
									break;
								case 0xDA: // JP C, nn
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagC) {
										RegPC.Value16 = TUS;
									}
									break;
								case 0xDB: // IN A, n
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									RegAF.High8 = ReadHardware((ushort)ReadMemory(RegPC.Value16++));
									break;
								case 0xDC: // CALL C, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagC) {
										this.totalExecutedCycles += 17; this.pendingCycles -= 17;
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
									} else {
										this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									}
									break;
								case 0xDD: // <-
									this.totalExecutedCycles += 1337; this.pendingCycles -= 1337;
									// Invalid sequence.
									break;
								case 0xDE: // SBC A, n
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									RegAF.Value16 = TableALU[3, RegAF.High8, ReadMemory(RegPC.Value16++), RegFlagC ? 1 : 0];
									break;
								case 0xDF: // RST $18
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x18;
									break;
								case 0xE0: // RET PO
									if (!RegFlagP) {
										this.totalExecutedCycles += 11; this.pendingCycles -= 11;
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									} else {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
									}
									break;
								case 0xE1: // POP IY
									this.totalExecutedCycles += 14; this.pendingCycles -= 14;
									RegIY.Low8 = ReadMemory(RegSP.Value16++); RegIY.High8 = ReadMemory(RegSP.Value16++);
									break;
								case 0xE2: // JP PO, nn
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagP) {
										RegPC.Value16 = TUS;
									}
									break;
								case 0xE3: // EX (SP), IY
									this.totalExecutedCycles += 23; this.pendingCycles -= 23;
									TUS = RegSP.Value16; TBL = ReadMemory(TUS++); TBH = ReadMemory(TUS--);
									WriteMemory(TUS++, RegIY.Low8); WriteMemory(TUS, RegIY.High8);
									RegIY.Low8 = TBL; RegIY.High8 = TBH;
									break;
								case 0xE4: // CALL C, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagC) {
										this.totalExecutedCycles += 17; this.pendingCycles -= 17;
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
									} else {
										this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									}
									break;
								case 0xE5: // PUSH IY
									this.totalExecutedCycles += 15; this.pendingCycles -= 15;
									WriteMemory(--RegSP.Value16, RegIY.High8); WriteMemory(--RegSP.Value16, RegIY.Low8);
									break;
								case 0xE6: // AND n
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									RegAF.Value16 = TableALU[4, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
									break;
								case 0xE7: // RST $20
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x20;
									break;
								case 0xE8: // RET PE
									if (RegFlagP) {
										this.totalExecutedCycles += 11; this.pendingCycles -= 11;
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									} else {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
									}
									break;
								case 0xE9: // JP IY
									this.totalExecutedCycles += 8; this.pendingCycles -= 8;
									RegPC.Value16 = RegIY.Value16;
									break;
								case 0xEA: // JP PE, nn
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagP) {
										RegPC.Value16 = TUS;
									}
									break;
								case 0xEB: // EX DE, HL
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									TUS = RegDE.Value16; RegDE.Value16 = RegHL.Value16; RegHL.Value16 = TUS;
									break;
								case 0xEC: // CALL PE, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagP) {
										this.totalExecutedCycles += 17; this.pendingCycles -= 17;
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
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
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											RegBC.High8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegBC.High8 > 127;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegBC.High8];
											RegFlagN = false;
											break;
										case 0x41: // OUT C, B
											this.totalExecutedCycles += 9; this.pendingCycles -= 9;
											WriteHardware(RegBC.Low8, RegBC.High8);
											this.totalExecutedCycles += 3; this.pendingCycles -= 3;
											break;
										case 0x42: // SBC HL, BC
											this.totalExecutedCycles += 15; this.pendingCycles -= 15;
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
											break;
										case 0x43: // LD (nn), BC
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											WriteMemory(TUS++, RegBC.Low8);
											WriteMemory(TUS, RegBC.High8);
											break;
										case 0x44: // NEG
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											RegAF.Value16 = TableNeg[RegAF.Value16];
											break;
										case 0x45: // RETN
											this.totalExecutedCycles += 14; this.pendingCycles -= 14;
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											this.IFF1 = this.IFF2;
											break;
										case 0x46: // IM $0
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											interruptMode = 0;
											break;
										case 0x47: // LD I, A
											this.totalExecutedCycles += 9; this.pendingCycles -= 9;
											RegI = RegAF.High8;
											break;
										case 0x48: // IN C, C
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											RegBC.Low8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegBC.Low8 > 127;
											RegFlagZ = RegBC.Low8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegBC.Low8];
											RegFlagN = false;
											break;
										case 0x49: // OUT C, C
											this.totalExecutedCycles += 9; this.pendingCycles -= 9;
											WriteHardware(RegBC.Low8, RegBC.Low8);
											this.totalExecutedCycles += 3; this.pendingCycles -= 3;
											break;
										case 0x4A: // ADC HL, BC
											this.totalExecutedCycles += 15; this.pendingCycles -= 15;
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
											break;
										case 0x4B: // LD BC, (nn)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											RegBC.Low8 = ReadMemory(TUS++); RegBC.High8 = ReadMemory(TUS);
											break;
										case 0x4C: // NEG
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											RegAF.Value16 = TableNeg[RegAF.Value16];
											break;
										case 0x4D: // RETI
											this.totalExecutedCycles += 14; this.pendingCycles -= 14;
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											break;
										case 0x4E: // IM $0
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											interruptMode = 0;
											break;
										case 0x4F: // LD R, A
											this.totalExecutedCycles += 9; this.pendingCycles -= 9;
											RegR = RegAF.High8;
											break;
										case 0x50: // IN D, C
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											RegDE.High8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegDE.High8 > 127;
											RegFlagZ = RegDE.High8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegDE.High8];
											RegFlagN = false;
											break;
										case 0x51: // OUT C, D
											this.totalExecutedCycles += 9; this.pendingCycles -= 9;
											WriteHardware(RegBC.Low8, RegDE.High8);
											this.totalExecutedCycles += 3; this.pendingCycles -= 3;
											break;
										case 0x52: // SBC HL, DE
											this.totalExecutedCycles += 15; this.pendingCycles -= 15;
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
											break;
										case 0x53: // LD (nn), DE
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											WriteMemory(TUS++, RegDE.Low8);
											WriteMemory(TUS, RegDE.High8);
											break;
										case 0x54: // NEG
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											RegAF.Value16 = TableNeg[RegAF.Value16];
											break;
										case 0x55: // RETN
											this.totalExecutedCycles += 14; this.pendingCycles -= 14;
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											this.IFF1 = this.IFF2;
											break;
										case 0x56: // IM $1
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											interruptMode = 1;
											break;
										case 0x57: // LD A, I
											this.totalExecutedCycles += 9; this.pendingCycles -= 9;
											RegAF.High8 = RegI;
											RegFlagS = RegI > 127;
											RegFlagZ = RegI == 0;
											RegFlagH = false;
											RegFlagN = false;
											RegFlagP = this.IFF2;
											break;
										case 0x58: // IN E, C
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											RegDE.Low8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegDE.Low8 > 127;
											RegFlagZ = RegDE.Low8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegDE.Low8];
											RegFlagN = false;
											break;
										case 0x59: // OUT C, E
											this.totalExecutedCycles += 9; this.pendingCycles -= 9;
											WriteHardware(RegBC.Low8, RegDE.Low8);
											this.totalExecutedCycles += 3; this.pendingCycles -= 3;
											break;
										case 0x5A: // ADC HL, DE
											this.totalExecutedCycles += 15; this.pendingCycles -= 15;
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
											break;
										case 0x5B: // LD DE, (nn)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											RegDE.Low8 = ReadMemory(TUS++); RegDE.High8 = ReadMemory(TUS);
											break;
										case 0x5C: // NEG
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											RegAF.Value16 = TableNeg[RegAF.Value16];
											break;
										case 0x5D: // RETI
											this.totalExecutedCycles += 14; this.pendingCycles -= 14;
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											break;
										case 0x5E: // IM $2
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											interruptMode = 2;
											break;
										case 0x5F: // LD A, R
											this.totalExecutedCycles += 9; this.pendingCycles -= 9;
											RegAF.High8 = (byte)(RegR & 0x7F);
											RegFlagS = (byte)(RegR & 0x7F) > 127;
											RegFlagZ = (byte)(RegR & 0x7F) == 0;
											RegFlagH = false;
											RegFlagN = false;
											RegFlagP = this.IFF2;
											break;
										case 0x60: // IN H, C
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											RegHL.High8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegHL.High8 > 127;
											RegFlagZ = RegHL.High8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegHL.High8];
											RegFlagN = false;
											break;
										case 0x61: // OUT C, H
											this.totalExecutedCycles += 9; this.pendingCycles -= 9;
											WriteHardware(RegBC.Low8, RegHL.High8);
											this.totalExecutedCycles += 3; this.pendingCycles -= 3;
											break;
										case 0x62: // SBC HL, HL
											this.totalExecutedCycles += 15; this.pendingCycles -= 15;
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
											break;
										case 0x63: // LD (nn), HL
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											WriteMemory(TUS++, RegHL.Low8);
											WriteMemory(TUS, RegHL.High8);
											break;
										case 0x64: // NEG
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											RegAF.Value16 = TableNeg[RegAF.Value16];
											break;
										case 0x65: // RETN
											this.totalExecutedCycles += 14; this.pendingCycles -= 14;
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											this.IFF1 = this.IFF2;
											break;
										case 0x66: // IM $0
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											interruptMode = 0;
											break;
										case 0x67: // RRD
											this.totalExecutedCycles += 18; this.pendingCycles -= 18;
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
											break;
										case 0x68: // IN L, C
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											RegHL.Low8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegHL.Low8 > 127;
											RegFlagZ = RegHL.Low8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegHL.Low8];
											RegFlagN = false;
											break;
										case 0x69: // OUT C, L
											this.totalExecutedCycles += 9; this.pendingCycles -= 9;
											WriteHardware(RegBC.Low8, RegHL.Low8);
											this.totalExecutedCycles += 3; this.pendingCycles -= 3;
											break;
										case 0x6A: // ADC HL, HL
											this.totalExecutedCycles += 15; this.pendingCycles -= 15;
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
											break;
										case 0x6B: // LD HL, (nn)
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											RegHL.Low8 = ReadMemory(TUS++); RegHL.High8 = ReadMemory(TUS);
											break;
										case 0x6C: // NEG
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											RegAF.Value16 = TableNeg[RegAF.Value16];
											break;
										case 0x6D: // RETI
											this.totalExecutedCycles += 14; this.pendingCycles -= 14;
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											break;
										case 0x6E: // IM $0
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											interruptMode = 0;
											break;
										case 0x6F: // RLD
											this.totalExecutedCycles += 18; this.pendingCycles -= 18;
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
											break;
										case 0x70: // IN 0, C
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											TB = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = TB > 127;
											RegFlagZ = TB == 0;
											RegFlagH = false;
											RegFlagP = TableParity[TB];
											RegFlagN = false;
											break;
										case 0x71: // OUT C, 0
											this.totalExecutedCycles += 9; this.pendingCycles -= 9;
											WriteHardware(RegBC.Low8, 0);
											this.totalExecutedCycles += 3; this.pendingCycles -= 3;
											break;
										case 0x72: // SBC HL, SP
											this.totalExecutedCycles += 15; this.pendingCycles -= 15;
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
											break;
										case 0x73: // LD (nn), SP
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											WriteMemory(TUS++, RegSP.Low8);
											WriteMemory(TUS, RegSP.High8);
											break;
										case 0x74: // NEG
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											RegAF.Value16 = TableNeg[RegAF.Value16];
											break;
										case 0x75: // RETN
											this.totalExecutedCycles += 14; this.pendingCycles -= 14;
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											this.IFF1 = this.IFF2;
											break;
										case 0x76: // IM $1
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											interruptMode = 1;
											break;
										case 0x77: // NOP
											this.totalExecutedCycles += 4; this.pendingCycles -= 4;
											break;
										case 0x78: // IN A, C
											this.totalExecutedCycles += 12; this.pendingCycles -= 12;
											RegAF.High8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegAF.High8 > 127;
											RegFlagZ = RegAF.High8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegAF.High8];
											RegFlagN = false;
											break;
										case 0x79: // OUT C, A
											this.totalExecutedCycles += 9; this.pendingCycles -= 9;
											WriteHardware(RegBC.Low8, RegAF.High8);
											this.totalExecutedCycles += 3; this.pendingCycles -= 3;
											break;
										case 0x7A: // ADC HL, SP
											this.totalExecutedCycles += 15; this.pendingCycles -= 15;
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
											break;
										case 0x7B: // LD SP, (nn)
											this.totalExecutedCycles += 20; this.pendingCycles -= 20;
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											RegSP.Low8 = ReadMemory(TUS++); RegSP.High8 = ReadMemory(TUS);
											break;
										case 0x7C: // NEG
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											RegAF.Value16 = TableNeg[RegAF.Value16];
											break;
										case 0x7D: // RETI
											this.totalExecutedCycles += 14; this.pendingCycles -= 14;
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											break;
										case 0x7E: // IM $2
											this.totalExecutedCycles += 8; this.pendingCycles -= 8;
											interruptMode = 2;
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
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											WriteMemory(RegDE.Value16++, TB1 = ReadMemory(RegHL.Value16++));
											TB1 += RegAF.High8; RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											RegFlagH = false;
											RegFlagN = false;
											break;
										case 0xA1: // CPI
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											TB1 = ReadMemory(RegHL.Value16++); TB2 = (byte)(RegAF.High8 - TB1);
											RegFlagN = true;
											RegFlagH = TableHalfBorrow[RegAF.High8, TB1];
											RegFlagZ = TB2 == 0;
											RegFlagS = TB2 > 127;
											TB1 = (byte)(RegAF.High8 - TB1 - (RegFlagH ? 1 : 0)); RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											break;
										case 0xA2: // INI
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											WriteMemory(RegHL.Value16++, ReadHardware(RegBC.Value16));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											break;
										case 0xA3: // OUTI
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											WriteHardware(RegBC.Value16, ReadMemory(RegHL.Value16++));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
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
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											WriteMemory(RegDE.Value16--, TB1 = ReadMemory(RegHL.Value16--));
											TB1 += RegAF.High8; RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											RegFlagH = false;
											RegFlagN = false;
											break;
										case 0xA9: // CPD
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											TB1 = ReadMemory(RegHL.Value16--); TB2 = (byte)(RegAF.High8 - TB1);
											RegFlagN = true;
											RegFlagH = TableHalfBorrow[RegAF.High8, TB1];
											RegFlagZ = TB2 == 0;
											RegFlagS = TB2 > 127;
											TB1 = (byte)(RegAF.High8 - TB1 - (RegFlagH ? 1 : 0)); RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											break;
										case 0xAA: // IND
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											WriteMemory(RegHL.Value16--, ReadHardware(RegBC.Value16));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											break;
										case 0xAB: // OUTD
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											WriteHardware(RegBC.Value16, ReadMemory(RegHL.Value16--));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
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
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											WriteMemory(RegDE.Value16++, TB1 = ReadMemory(RegHL.Value16++));
											TB1 += RegAF.High8; RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											RegFlagH = false;
											RegFlagN = false;
											if (RegBC.Value16 != 0) {
												this.totalExecutedCycles += 5; this.pendingCycles -= 5;
												RegPC.Value16 -= 2;
											}
											break;
										case 0xB1: // CPIR
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											TB1 = ReadMemory(RegHL.Value16++); TB2 = (byte)(RegAF.High8 - TB1);
											RegFlagN = true;
											RegFlagH = TableHalfBorrow[RegAF.High8, TB1];
											RegFlagZ = TB2 == 0;
											RegFlagS = TB2 > 127;
											TB1 = (byte)(RegAF.High8 - TB1 - (RegFlagH ? 1 : 0)); RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											if (RegBC.Value16 != 0 && !RegFlagZ) {
												this.totalExecutedCycles += 5; this.pendingCycles -= 5;
												RegPC.Value16 -= 2;
											}
											break;
										case 0xB2: // INIR
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											WriteMemory(RegHL.Value16++, ReadHardware(RegBC.Value16));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											if (RegBC.High8 != 0) {
												this.totalExecutedCycles += 5; this.pendingCycles -= 5;
												RegPC.Value16 -= 2;
											}
											break;
										case 0xB3: // OTIR
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											WriteHardware(RegBC.Value16, ReadMemory(RegHL.Value16++));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											if (RegBC.High8 != 0) {
												this.totalExecutedCycles += 5; this.pendingCycles -= 5;
												RegPC.Value16 -= 2;
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
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											WriteMemory(RegDE.Value16--, TB1 = ReadMemory(RegHL.Value16--));
											TB1 += RegAF.High8; RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											RegFlagH = false;
											RegFlagN = false;
											if (RegBC.Value16 != 0) {
												this.totalExecutedCycles += 5; this.pendingCycles -= 5;
												RegPC.Value16 -= 2;
											}
											break;
										case 0xB9: // CPDR
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											TB1 = ReadMemory(RegHL.Value16--); TB2 = (byte)(RegAF.High8 - TB1);
											RegFlagN = true;
											RegFlagH = TableHalfBorrow[RegAF.High8, TB1];
											RegFlagZ = TB2 == 0;
											RegFlagS = TB2 > 127;
											TB1 = (byte)(RegAF.High8 - TB1 - (RegFlagH ? 1 : 0)); RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											if (RegBC.Value16 != 0 && !RegFlagZ) {
												this.totalExecutedCycles += 5; this.pendingCycles -= 5;
												RegPC.Value16 -= 2;
											}
											break;
										case 0xBA: // INDR
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											WriteMemory(RegHL.Value16--, ReadHardware(RegBC.Value16));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											if (RegBC.High8 != 0) {
												this.totalExecutedCycles += 5; this.pendingCycles -= 5;
												RegPC.Value16 -= 2;
											}
											break;
										case 0xBB: // OTDR
											this.totalExecutedCycles += 16; this.pendingCycles -= 16;
											WriteHardware(RegBC.Value16, ReadMemory(RegHL.Value16--));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											if (RegBC.High8 != 0) {
												this.totalExecutedCycles += 5; this.pendingCycles -= 5;
												RegPC.Value16 -= 2;
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
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									RegAF.Value16 = TableALU[5, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
									break;
								case 0xEF: // RST $28
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x28;
									break;
								case 0xF0: // RET P
									if (!RegFlagS) {
										this.totalExecutedCycles += 11; this.pendingCycles -= 11;
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									} else {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
									}
									break;
								case 0xF1: // POP AF
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									RegAF.Low8 = ReadMemory(RegSP.Value16++); RegAF.High8 = ReadMemory(RegSP.Value16++);
									break;
								case 0xF2: // JP P, nn
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagS) {
										RegPC.Value16 = TUS;
									}
									break;
								case 0xF3: // DI
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									this.IFF1 = this.IFF2 = false;
									break;
								case 0xF4: // CALL P, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagS) {
										this.totalExecutedCycles += 17; this.pendingCycles -= 17;
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
									} else {
										this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									}
									break;
								case 0xF5: // PUSH AF
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									WriteMemory(--RegSP.Value16, RegAF.High8); WriteMemory(--RegSP.Value16, RegAF.Low8);
									break;
								case 0xF6: // OR n
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									RegAF.Value16 = TableALU[6, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
									break;
								case 0xF7: // RST $30
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x30;
									break;
								case 0xF8: // RET M
									if (RegFlagS) {
										this.totalExecutedCycles += 11; this.pendingCycles -= 11;
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									} else {
										this.totalExecutedCycles += 5; this.pendingCycles -= 5;
									}
									break;
								case 0xF9: // LD SP, IY
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									RegSP.Value16 = RegIY.Value16;
									break;
								case 0xFA: // JP M, nn
									this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagS) {
										RegPC.Value16 = TUS;
									}
									break;
								case 0xFB: // EI
									this.totalExecutedCycles += 4; this.pendingCycles -= 4;
									this.IFF1 = this.IFF2 = true;
									Interruptable = false;
									break;
								case 0xFC: // CALL M, nn
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagS) {
										this.totalExecutedCycles += 17; this.pendingCycles -= 17;
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
									} else {
										this.totalExecutedCycles += 10; this.pendingCycles -= 10;
									}
									break;
								case 0xFD: // <-
									this.totalExecutedCycles += 1337; this.pendingCycles -= 1337;
									// Invalid sequence.
									break;
								case 0xFE: // CP n
									this.totalExecutedCycles += 7; this.pendingCycles -= 7;
									RegAF.Value16 = TableALU[7, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
									break;
								case 0xFF: // RST $38
									this.totalExecutedCycles += 11; this.pendingCycles -= 11;
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x38;
									break;
							}
							break;
						case 0xFE: // CP n
							this.totalExecutedCycles += 7; this.pendingCycles -= 7;
							RegAF.Value16 = TableALU[7, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
							break;
						case 0xFF: // RST $38
							this.totalExecutedCycles += 11; this.pendingCycles -= 11;
							WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
							RegPC.Value16 = 0x38;
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
							this.totalExecutedCycles += 13; this.pendingCycles -= 13;
							WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
							RegPC.Value16 = 0x38;
							break;
						case 2:
							this.totalExecutedCycles += 19; this.pendingCycles -= 19;
							TUS = (ushort)(RegI * 256 + 0);
							WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
							RegPC.Low8 = ReadMemory(TUS++); RegPC.High8 = ReadMemory(TUS);
							break;
					}
				}

				
			}
		}
		
	}
}