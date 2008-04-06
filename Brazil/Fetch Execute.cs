using System.Collections.Generic;
namespace BeeDevelopment.Brazil {
	public partial class Z80A {
	
		/// <summary>
		/// Gets or sets the total number of clock cycles actually executed.
		/// </summary>
		public int TotalExecutedCycles { get; set; }

		/// <summary>
		/// Gets or sets the number of clock cycles that have been requested to be executed.
		/// </summary>		
		public int ExpectedExecutedCycles { get; set; }
		
		/// <summary>
		/// Gets or sets the number of clock cycles left to run.
		/// </summary>
		public int PendingCycles { get; set; }
		
		/// <summary>
		/// Runs the CPU for a particular number of clock cycles.
		/// </summary>
		/// <param name="cycles">The number of cycles to run the CPU emulator for. Specify -1 to run for a single instruction.</param>
		public void FetchExecute(int cycles) {
			//*/
			
			this.ExpectedExecutedCycles += cycles;
			this.PendingCycles += cycles;
			
			int ClockCycles = 4;
			
			sbyte Displacement;
			
			byte TB; byte TBH; byte TBL; byte TB1; byte TB2; sbyte TSB; ushort TUS; int TI1; int TI2; int TIR;

			while (this.PendingCycles > 0) {

				#region Interrupts

				// Handle interrupts

				if (this.InstructionsUntilEI > 0) {
					if (--InstructionsUntilEI == 0) {
						this.IFF1 = true;
						this.IFF2 = true;
					}
				}

				// IRQ
				if (this.IFF1 && this.Interrupt) { // Are interrupts enabled?

					this.Halted = false;

					this.IFF1 = false;
					this.IFF2 = false;

					switch (interruptMode) {
						case 0:
							ClockCycles += 11;
							break;
						case 1:
							WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
							RegPC.Value16 = 0x38;
							ClockCycles += 13;
							break;
						case 2:
							TUS = (ushort)(RegI * 256 + 0);
							WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
							RegPC.Low8 = ReadMemory(TUS++); RegPC.High8 = ReadMemory(TUS);
							ClockCycles += 19;
							break;
					}
					
				}


				// NMI
				if (this.NonMaskableInterruptPending) {

					this.Halted = false;
					this.NonMaskableInterruptPending = false;

					this.IFF2 = this.IFF1;
					this.IFF1 = false;

					WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
					RegPC.Value16 = 0x66;
					ClockCycles += 11;
				}
				
				#endregion

				if (this.Halted) {
					++RegR;
					ClockCycles = 4;
				} else {

					++RegR;
					switch (ReadMemory(RegPC.Value16++)) {
						case 0x00: // NOP
							ClockCycles = 4;
							break;
						case 0x01: // LD BC, nn
							ClockCycles = 10;
							RegBC.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							break;
						case 0x02: // LD (BC), A
							ClockCycles = 7;
							WriteMemory(RegBC.Value16, RegAF.High8);
							break;
						case 0x03: // INC BC
							ClockCycles = 6;
							++RegBC.Value16;
							break;
						case 0x04: // INC B
							ClockCycles = 4;
							RegAF.Low8 = (byte)(TableInc[++RegBC.High8] | (RegAF.Low8 & 1));
							break;
						case 0x05: // DEC B
							ClockCycles = 4;
							RegAF.Low8 = (byte)(TableDec[--RegBC.High8] | (RegAF.Low8 & 1));
							break;
						case 0x06: // LD B, n
							ClockCycles = 7;
							RegBC.High8 = ReadMemory(RegPC.Value16++);
							break;
						case 0x07: // RLCA
							ClockCycles = 4;
							RegAF.Value16 = TableRotShift[0, 0, RegAF.Value16];
							break;
						case 0x08: // EX AF, AF'
							ClockCycles = 4;
							TUS = RegAF.Value16; RegAF.Value16 = RegAltAF.Value16; RegAltAF.Value16 = TUS;
							break;
						case 0x09: // ADD HL, BC
							ClockCycles = 11;
							TI1 = (short)RegHL.Value16; TI2 = (short)RegBC.Value16; TIR = TI1 + TI2;
							TUS = (ushort)TIR;
							RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
							RegFlagN = false;
							RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
							RegHL.Value16 = TUS;
							break;
						case 0x0A: // LD A, (BC)
							ClockCycles = 7;
							RegAF.High8 = ReadMemory(RegBC.Value16);
							break;
						case 0x0B: // DEC BC
							ClockCycles = 6;
							--RegBC.Value16;
							break;
						case 0x0C: // INC C
							ClockCycles = 4;
							RegAF.Low8 = (byte)(TableInc[++RegBC.Low8] | (RegAF.Low8 & 1));
							break;
						case 0x0D: // DEC C
							ClockCycles = 4;
							RegAF.Low8 = (byte)(TableDec[--RegBC.Low8] | (RegAF.Low8 & 1));
							break;
						case 0x0E: // LD C, n
							ClockCycles = 7;
							RegBC.Low8 = ReadMemory(RegPC.Value16++);
							break;
						case 0x0F: // RRCA
							ClockCycles = 4;
							RegAF.Value16 = TableRotShift[0, 1, RegAF.Value16];
							break;
						case 0x10: // DJNZ d
							ClockCycles = 13;
							TSB = (sbyte)ReadMemory(RegPC.Value16++);
							if (--RegBC.High8 != 0) {
								RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
							} else {
								ClockCycles = 8;
							}
							break;
						case 0x11: // LD DE, nn
							ClockCycles = 10;
							RegDE.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							break;
						case 0x12: // LD (DE), A
							ClockCycles = 7;
							WriteMemory(RegDE.Value16, RegAF.High8);
							break;
						case 0x13: // INC DE
							ClockCycles = 6;
							++RegDE.Value16;
							break;
						case 0x14: // INC D
							ClockCycles = 4;
							RegAF.Low8 = (byte)(TableInc[++RegDE.High8] | (RegAF.Low8 & 1));
							break;
						case 0x15: // DEC D
							ClockCycles = 4;
							RegAF.Low8 = (byte)(TableDec[--RegDE.High8] | (RegAF.Low8 & 1));
							break;
						case 0x16: // LD D, n
							ClockCycles = 7;
							RegDE.High8 = ReadMemory(RegPC.Value16++);
							break;
						case 0x17: // RLA
							ClockCycles = 4;
							RegAF.Value16 = TableRotShift[0, 2, RegAF.Value16];
							break;
						case 0x18: // JR d
							ClockCycles = 12;
							TSB = (sbyte)ReadMemory(RegPC.Value16++);
							RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
							break;
						case 0x19: // ADD HL, DE
							ClockCycles = 11;
							TI1 = (short)RegHL.Value16; TI2 = (short)RegDE.Value16; TIR = TI1 + TI2;
							TUS = (ushort)TIR;
							RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
							RegFlagN = false;
							RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
							RegHL.Value16 = TUS;
							break;
						case 0x1A: // LD A, (DE)
							ClockCycles = 7;
							RegAF.High8 = ReadMemory(RegDE.Value16);
							break;
						case 0x1B: // DEC DE
							ClockCycles = 6;
							--RegDE.Value16;
							break;
						case 0x1C: // INC E
							ClockCycles = 4;
							RegAF.Low8 = (byte)(TableInc[++RegDE.Low8] | (RegAF.Low8 & 1));
							break;
						case 0x1D: // DEC E
							ClockCycles = 4;
							RegAF.Low8 = (byte)(TableDec[--RegDE.Low8] | (RegAF.Low8 & 1));
							break;
						case 0x1E: // LD E, n
							ClockCycles = 7;
							RegDE.Low8 = ReadMemory(RegPC.Value16++);
							break;
						case 0x1F: // RRA
							ClockCycles = 4;
							RegAF.Value16 = TableRotShift[0, 3, RegAF.Value16];
							break;
						case 0x20: // JR NZ, d
							ClockCycles = 12;
							TSB = (sbyte)ReadMemory(RegPC.Value16++);
							if (!RegFlagZ) {
								RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
							} else {
								ClockCycles = 7;
							}
							break;
						case 0x21: // LD HL, nn
							ClockCycles = 10;
							RegHL.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							break;
						case 0x22: // LD (nn), HL
							ClockCycles = 20;
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							WriteMemory(TUS++, RegHL.Low8);
							WriteMemory(TUS, RegHL.High8);
							break;
						case 0x23: // INC HL
							ClockCycles = 6;
							++RegHL.Value16;
							break;
						case 0x24: // INC H
							ClockCycles = 4;
							RegAF.Low8 = (byte)(TableInc[++RegHL.High8] | (RegAF.Low8 & 1));
							break;
						case 0x25: // DEC H
							ClockCycles = 4;
							RegAF.Low8 = (byte)(TableDec[--RegHL.High8] | (RegAF.Low8 & 1));
							break;
						case 0x26: // LD H, n
							ClockCycles = 7;
							RegHL.High8 = ReadMemory(RegPC.Value16++);
							break;
						case 0x27: // DAA
							ClockCycles = 4;
							RegAF.Value16 = TableDaa[RegAF.Value16];
							break;
						case 0x28: // JR Z, d
							ClockCycles = 12;
							TSB = (sbyte)ReadMemory(RegPC.Value16++);
							if (RegFlagZ) {
								RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
							} else {
								ClockCycles = 7;
							}
							break;
						case 0x29: // ADD HL, HL
							ClockCycles = 11;
							TI1 = (short)RegHL.Value16; TI2 = (short)RegHL.Value16; TIR = TI1 + TI2;
							TUS = (ushort)TIR;
							RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
							RegFlagN = false;
							RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
							RegHL.Value16 = TUS;
							break;
						case 0x2A: // LD HL, (nn)
							ClockCycles = 20;
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							RegHL.Low8 = ReadMemory(TUS++); RegHL.High8 = ReadMemory(TUS);
							break;
						case 0x2B: // DEC HL
							ClockCycles = 6;
							--RegHL.Value16;
							break;
						case 0x2C: // INC L
							ClockCycles = 4;
							RegAF.Low8 = (byte)(TableInc[++RegHL.Low8] | (RegAF.Low8 & 1));
							break;
						case 0x2D: // DEC L
							ClockCycles = 4;
							RegAF.Low8 = (byte)(TableDec[--RegHL.Low8] | (RegAF.Low8 & 1));
							break;
						case 0x2E: // LD L, n
							ClockCycles = 7;
							RegHL.Low8 = ReadMemory(RegPC.Value16++);
							break;
						case 0x2F: // CPL
							ClockCycles = 4;
							RegAF.High8 ^= 0xFF; RegFlagH = true; RegFlagN = true;
							break;
						case 0x30: // JR NC, d
							ClockCycles = 12;
							TSB = (sbyte)ReadMemory(RegPC.Value16++);
							if (!RegFlagC) {
								RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
							} else {
								ClockCycles = 7;
							}
							break;
						case 0x31: // LD SP, nn
							ClockCycles = 10;
							RegSP.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							break;
						case 0x32: // LD (nn), A
							ClockCycles = 13;
							WriteMemory((ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256), RegAF.High8);
							break;
						case 0x33: // INC SP
							ClockCycles = 6;
							++RegSP.Value16;
							break;
						case 0x34: // INC (HL)
							ClockCycles = 7;
							TB = ReadMemory(RegHL.Value16); RegAF.Low8 = (byte)(TableInc[++TB] | (RegAF.Low8 & 1)); WriteMemory(RegHL.Value16, TB);
							break;
						case 0x35: // DEC (HL)
							ClockCycles = 7;
							TB = ReadMemory(RegHL.Value16); RegAF.Low8 = (byte)(TableDec[--TB] | (RegAF.Low8 & 1)); WriteMemory(RegHL.Value16, TB);
							break;
						case 0x36: // LD (HL), n
							ClockCycles = 10;
							WriteMemory(RegHL.Value16, ReadMemory(RegPC.Value16++));
							break;
						case 0x37: // SCF
							ClockCycles = 4;
							RegFlagH = false; RegFlagN = false; RegFlagC = true;
							break;
						case 0x38: // JR C, d
							ClockCycles = 12;
							TSB = (sbyte)ReadMemory(RegPC.Value16++);
							if (RegFlagC) {
								RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
							} else {
								ClockCycles = 7;
							}
							break;
						case 0x39: // ADD HL, SP
							ClockCycles = 11;
							TI1 = (short)RegHL.Value16; TI2 = (short)RegSP.Value16; TIR = TI1 + TI2;
							TUS = (ushort)TIR;
							RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
							RegFlagN = false;
							RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
							RegHL.Value16 = TUS;
							break;
						case 0x3A: // LD A, (nn)
							ClockCycles = 13;
							RegAF.High8 = ReadMemory((ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256));
							break;
						case 0x3B: // DEC SP
							ClockCycles = 6;
							--RegSP.Value16;
							break;
						case 0x3C: // INC A
							ClockCycles = 4;
							RegAF.Low8 = (byte)(TableInc[++RegAF.High8] | (RegAF.Low8 & 1));
							break;
						case 0x3D: // DEC A
							ClockCycles = 4;
							RegAF.Low8 = (byte)(TableDec[--RegAF.High8] | (RegAF.Low8 & 1));
							break;
						case 0x3E: // LD A, n
							ClockCycles = 7;
							RegAF.High8 = ReadMemory(RegPC.Value16++);
							break;
						case 0x3F: // CCF
							ClockCycles = 4;
							RegFlagH = RegFlagC; RegFlagN = false; RegFlagC ^= true;
							break;
						case 0x40: // LD B, B
							ClockCycles = 4;
							break;
						case 0x41: // LD B, C
							ClockCycles = 4;
							RegBC.High8 = RegBC.Low8;
							break;
						case 0x42: // LD B, D
							ClockCycles = 4;
							RegBC.High8 = RegDE.High8;
							break;
						case 0x43: // LD B, E
							ClockCycles = 4;
							RegBC.High8 = RegDE.Low8;
							break;
						case 0x44: // LD B, H
							ClockCycles = 4;
							RegBC.High8 = RegHL.High8;
							break;
						case 0x45: // LD B, L
							ClockCycles = 4;
							RegBC.High8 = RegHL.Low8;
							break;
						case 0x46: // LD B, (HL)
							ClockCycles = 7;
							RegBC.High8 = ReadMemory(RegHL.Value16);
							break;
						case 0x47: // LD B, A
							ClockCycles = 4;
							RegBC.High8 = RegAF.High8;
							break;
						case 0x48: // LD C, B
							ClockCycles = 4;
							RegBC.Low8 = RegBC.High8;
							break;
						case 0x49: // LD C, C
							ClockCycles = 4;
							break;
						case 0x4A: // LD C, D
							ClockCycles = 4;
							RegBC.Low8 = RegDE.High8;
							break;
						case 0x4B: // LD C, E
							ClockCycles = 4;
							RegBC.Low8 = RegDE.Low8;
							break;
						case 0x4C: // LD C, H
							ClockCycles = 4;
							RegBC.Low8 = RegHL.High8;
							break;
						case 0x4D: // LD C, L
							ClockCycles = 4;
							RegBC.Low8 = RegHL.Low8;
							break;
						case 0x4E: // LD C, (HL)
							ClockCycles = 7;
							RegBC.Low8 = ReadMemory(RegHL.Value16);
							break;
						case 0x4F: // LD C, A
							ClockCycles = 4;
							RegBC.Low8 = RegAF.High8;
							break;
						case 0x50: // LD D, B
							ClockCycles = 4;
							RegDE.High8 = RegBC.High8;
							break;
						case 0x51: // LD D, C
							ClockCycles = 4;
							RegDE.High8 = RegBC.Low8;
							break;
						case 0x52: // LD D, D
							ClockCycles = 4;
							break;
						case 0x53: // LD D, E
							ClockCycles = 4;
							RegDE.High8 = RegDE.Low8;
							break;
						case 0x54: // LD D, H
							ClockCycles = 4;
							RegDE.High8 = RegHL.High8;
							break;
						case 0x55: // LD D, L
							ClockCycles = 4;
							RegDE.High8 = RegHL.Low8;
							break;
						case 0x56: // LD D, (HL)
							ClockCycles = 7;
							RegDE.High8 = ReadMemory(RegHL.Value16);
							break;
						case 0x57: // LD D, A
							ClockCycles = 4;
							RegDE.High8 = RegAF.High8;
							break;
						case 0x58: // LD E, B
							ClockCycles = 4;
							RegDE.Low8 = RegBC.High8;
							break;
						case 0x59: // LD E, C
							ClockCycles = 4;
							RegDE.Low8 = RegBC.Low8;
							break;
						case 0x5A: // LD E, D
							ClockCycles = 4;
							RegDE.Low8 = RegDE.High8;
							break;
						case 0x5B: // LD E, E
							ClockCycles = 4;
							break;
						case 0x5C: // LD E, H
							ClockCycles = 4;
							RegDE.Low8 = RegHL.High8;
							break;
						case 0x5D: // LD E, L
							ClockCycles = 4;
							RegDE.Low8 = RegHL.Low8;
							break;
						case 0x5E: // LD E, (HL)
							ClockCycles = 7;
							RegDE.Low8 = ReadMemory(RegHL.Value16);
							break;
						case 0x5F: // LD E, A
							ClockCycles = 4;
							RegDE.Low8 = RegAF.High8;
							break;
						case 0x60: // LD H, B
							ClockCycles = 4;
							RegHL.High8 = RegBC.High8;
							break;
						case 0x61: // LD H, C
							ClockCycles = 4;
							RegHL.High8 = RegBC.Low8;
							break;
						case 0x62: // LD H, D
							ClockCycles = 4;
							RegHL.High8 = RegDE.High8;
							break;
						case 0x63: // LD H, E
							ClockCycles = 4;
							RegHL.High8 = RegDE.Low8;
							break;
						case 0x64: // LD H, H
							ClockCycles = 4;
							break;
						case 0x65: // LD H, L
							ClockCycles = 4;
							RegHL.High8 = RegHL.Low8;
							break;
						case 0x66: // LD H, (HL)
							ClockCycles = 7;
							RegHL.High8 = ReadMemory(RegHL.Value16);
							break;
						case 0x67: // LD H, A
							ClockCycles = 4;
							RegHL.High8 = RegAF.High8;
							break;
						case 0x68: // LD L, B
							ClockCycles = 4;
							RegHL.Low8 = RegBC.High8;
							break;
						case 0x69: // LD L, C
							ClockCycles = 4;
							RegHL.Low8 = RegBC.Low8;
							break;
						case 0x6A: // LD L, D
							ClockCycles = 4;
							RegHL.Low8 = RegDE.High8;
							break;
						case 0x6B: // LD L, E
							ClockCycles = 4;
							RegHL.Low8 = RegDE.Low8;
							break;
						case 0x6C: // LD L, H
							ClockCycles = 4;
							RegHL.Low8 = RegHL.High8;
							break;
						case 0x6D: // LD L, L
							ClockCycles = 4;
							break;
						case 0x6E: // LD L, (HL)
							ClockCycles = 7;
							RegHL.Low8 = ReadMemory(RegHL.Value16);
							break;
						case 0x6F: // LD L, A
							ClockCycles = 4;
							RegHL.Low8 = RegAF.High8;
							break;
						case 0x70: // LD (HL), B
							ClockCycles = 7;
							WriteMemory(RegHL.Value16, RegBC.High8);
							break;
						case 0x71: // LD (HL), C
							ClockCycles = 7;
							WriteMemory(RegHL.Value16, RegBC.Low8);
							break;
						case 0x72: // LD (HL), D
							ClockCycles = 7;
							WriteMemory(RegHL.Value16, RegDE.High8);
							break;
						case 0x73: // LD (HL), E
							ClockCycles = 7;
							WriteMemory(RegHL.Value16, RegDE.Low8);
							break;
						case 0x74: // LD (HL), H
							ClockCycles = 7;
							WriteMemory(RegHL.Value16, RegHL.High8);
							break;
						case 0x75: // LD (HL), L
							ClockCycles = 7;
							WriteMemory(RegHL.Value16, RegHL.Low8);
							break;
						case 0x76: // HALT
							ClockCycles = 4;
							this.Halt();
							break;
						case 0x77: // LD (HL), A
							ClockCycles = 7;
							WriteMemory(RegHL.Value16, RegAF.High8);
							break;
						case 0x78: // LD A, B
							ClockCycles = 4;
							RegAF.High8 = RegBC.High8;
							break;
						case 0x79: // LD A, C
							ClockCycles = 4;
							RegAF.High8 = RegBC.Low8;
							break;
						case 0x7A: // LD A, D
							ClockCycles = 4;
							RegAF.High8 = RegDE.High8;
							break;
						case 0x7B: // LD A, E
							ClockCycles = 4;
							RegAF.High8 = RegDE.Low8;
							break;
						case 0x7C: // LD A, H
							ClockCycles = 4;
							RegAF.High8 = RegHL.High8;
							break;
						case 0x7D: // LD A, L
							ClockCycles = 4;
							RegAF.High8 = RegHL.Low8;
							break;
						case 0x7E: // LD A, (HL)
							ClockCycles = 7;
							RegAF.High8 = ReadMemory(RegHL.Value16);
							break;
						case 0x7F: // LD A, A
							ClockCycles = 4;
							break;
						case 0x80: // ADD A, B
							ClockCycles = 4;
							RegAF.Value16 = TableALU[0, RegAF.High8, RegBC.High8, 0];
							break;
						case 0x81: // ADD A, C
							ClockCycles = 4;
							RegAF.Value16 = TableALU[0, RegAF.High8, RegBC.Low8, 0];
							break;
						case 0x82: // ADD A, D
							ClockCycles = 4;
							RegAF.Value16 = TableALU[0, RegAF.High8, RegDE.High8, 0];
							break;
						case 0x83: // ADD A, E
							ClockCycles = 4;
							RegAF.Value16 = TableALU[0, RegAF.High8, RegDE.Low8, 0];
							break;
						case 0x84: // ADD A, H
							ClockCycles = 4;
							RegAF.Value16 = TableALU[0, RegAF.High8, RegHL.High8, 0];
							break;
						case 0x85: // ADD A, L
							ClockCycles = 4;
							RegAF.Value16 = TableALU[0, RegAF.High8, RegHL.Low8, 0];
							break;
						case 0x86: // ADD A, (HL)
							ClockCycles = 7;
							RegAF.Value16 = TableALU[0, RegAF.High8, ReadMemory(RegHL.Value16), 0];
							break;
						case 0x87: // ADD A, A
							ClockCycles = 4;
							RegAF.Value16 = TableALU[0, RegAF.High8, RegAF.High8, 0];
							break;
						case 0x88: // ADC A, B
							ClockCycles = 4;
							RegAF.Value16 = TableALU[1, RegAF.High8, RegBC.High8, RegFlagC ? 1 : 0];
							break;
						case 0x89: // ADC A, C
							ClockCycles = 4;
							RegAF.Value16 = TableALU[1, RegAF.High8, RegBC.Low8, RegFlagC ? 1 : 0];
							break;
						case 0x8A: // ADC A, D
							ClockCycles = 4;
							RegAF.Value16 = TableALU[1, RegAF.High8, RegDE.High8, RegFlagC ? 1 : 0];
							break;
						case 0x8B: // ADC A, E
							ClockCycles = 4;
							RegAF.Value16 = TableALU[1, RegAF.High8, RegDE.Low8, RegFlagC ? 1 : 0];
							break;
						case 0x8C: // ADC A, H
							ClockCycles = 4;
							RegAF.Value16 = TableALU[1, RegAF.High8, RegHL.High8, RegFlagC ? 1 : 0];
							break;
						case 0x8D: // ADC A, L
							ClockCycles = 4;
							RegAF.Value16 = TableALU[1, RegAF.High8, RegHL.Low8, RegFlagC ? 1 : 0];
							break;
						case 0x8E: // ADC A, (HL)
							ClockCycles = 7;
							RegAF.Value16 = TableALU[1, RegAF.High8, ReadMemory(RegHL.Value16), RegFlagC ? 1 : 0];
							break;
						case 0x8F: // ADC A, A
							ClockCycles = 4;
							RegAF.Value16 = TableALU[1, RegAF.High8, RegAF.High8, RegFlagC ? 1 : 0];
							break;
						case 0x90: // SUB B
							ClockCycles = 4;
							RegAF.Value16 = TableALU[2, RegAF.High8, RegBC.High8, 0];
							break;
						case 0x91: // SUB C
							ClockCycles = 4;
							RegAF.Value16 = TableALU[2, RegAF.High8, RegBC.Low8, 0];
							break;
						case 0x92: // SUB D
							ClockCycles = 4;
							RegAF.Value16 = TableALU[2, RegAF.High8, RegDE.High8, 0];
							break;
						case 0x93: // SUB E
							ClockCycles = 4;
							RegAF.Value16 = TableALU[2, RegAF.High8, RegDE.Low8, 0];
							break;
						case 0x94: // SUB H
							ClockCycles = 4;
							RegAF.Value16 = TableALU[2, RegAF.High8, RegHL.High8, 0];
							break;
						case 0x95: // SUB L
							ClockCycles = 4;
							RegAF.Value16 = TableALU[2, RegAF.High8, RegHL.Low8, 0];
							break;
						case 0x96: // SUB (HL)
							ClockCycles = 7;
							RegAF.Value16 = TableALU[2, RegAF.High8, ReadMemory(RegHL.Value16), 0];
							break;
						case 0x97: // SUB A, A
							ClockCycles = 4;
							RegAF.Value16 = TableALU[2, RegAF.High8, RegAF.High8, 0];
							break;
						case 0x98: // SBC A, B
							ClockCycles = 4;
							RegAF.Value16 = TableALU[3, RegAF.High8, RegBC.High8, RegFlagC ? 1 : 0];
							break;
						case 0x99: // SBC A, C
							ClockCycles = 4;
							RegAF.Value16 = TableALU[3, RegAF.High8, RegBC.Low8, RegFlagC ? 1 : 0];
							break;
						case 0x9A: // SBC A, D
							ClockCycles = 4;
							RegAF.Value16 = TableALU[3, RegAF.High8, RegDE.High8, RegFlagC ? 1 : 0];
							break;
						case 0x9B: // SBC A, E
							ClockCycles = 4;
							RegAF.Value16 = TableALU[3, RegAF.High8, RegDE.Low8, RegFlagC ? 1 : 0];
							break;
						case 0x9C: // SBC A, H
							ClockCycles = 4;
							RegAF.Value16 = TableALU[3, RegAF.High8, RegHL.High8, RegFlagC ? 1 : 0];
							break;
						case 0x9D: // SBC A, L
							ClockCycles = 4;
							RegAF.Value16 = TableALU[3, RegAF.High8, RegHL.Low8, RegFlagC ? 1 : 0];
							break;
						case 0x9E: // SBC A, (HL)
							ClockCycles = 7;
							RegAF.Value16 = TableALU[3, RegAF.High8, ReadMemory(RegHL.Value16), RegFlagC ? 1 : 0];
							break;
						case 0x9F: // SBC A, A
							ClockCycles = 4;
							RegAF.Value16 = TableALU[3, RegAF.High8, RegAF.High8, RegFlagC ? 1 : 0];
							break;
						case 0xA0: // AND B
							ClockCycles = 4;
							RegAF.Value16 = TableALU[4, RegAF.High8, RegBC.High8, 0];
							break;
						case 0xA1: // AND C
							ClockCycles = 4;
							RegAF.Value16 = TableALU[4, RegAF.High8, RegBC.Low8, 0];
							break;
						case 0xA2: // AND D
							ClockCycles = 4;
							RegAF.Value16 = TableALU[4, RegAF.High8, RegDE.High8, 0];
							break;
						case 0xA3: // AND E
							ClockCycles = 4;
							RegAF.Value16 = TableALU[4, RegAF.High8, RegDE.Low8, 0];
							break;
						case 0xA4: // AND H
							ClockCycles = 4;
							RegAF.Value16 = TableALU[4, RegAF.High8, RegHL.High8, 0];
							break;
						case 0xA5: // AND L
							ClockCycles = 4;
							RegAF.Value16 = TableALU[4, RegAF.High8, RegHL.Low8, 0];
							break;
						case 0xA6: // AND (HL)
							ClockCycles = 7;
							RegAF.Value16 = TableALU[4, RegAF.High8, ReadMemory(RegHL.Value16), 0];
							break;
						case 0xA7: // AND A
							ClockCycles = 4;
							RegAF.Value16 = TableALU[4, RegAF.High8, RegAF.High8, 0];
							break;
						case 0xA8: // XOR B
							ClockCycles = 4;
							RegAF.Value16 = TableALU[5, RegAF.High8, RegBC.High8, 0];
							break;
						case 0xA9: // XOR C
							ClockCycles = 4;
							RegAF.Value16 = TableALU[5, RegAF.High8, RegBC.Low8, 0];
							break;
						case 0xAA: // XOR D
							ClockCycles = 4;
							RegAF.Value16 = TableALU[5, RegAF.High8, RegDE.High8, 0];
							break;
						case 0xAB: // XOR E
							ClockCycles = 4;
							RegAF.Value16 = TableALU[5, RegAF.High8, RegDE.Low8, 0];
							break;
						case 0xAC: // XOR H
							ClockCycles = 4;
							RegAF.Value16 = TableALU[5, RegAF.High8, RegHL.High8, 0];
							break;
						case 0xAD: // XOR L
							ClockCycles = 4;
							RegAF.Value16 = TableALU[5, RegAF.High8, RegHL.Low8, 0];
							break;
						case 0xAE: // XOR (HL)
							ClockCycles = 7;
							RegAF.Value16 = TableALU[5, RegAF.High8, ReadMemory(RegHL.Value16), 0];
							break;
						case 0xAF: // XOR A
							ClockCycles = 4;
							RegAF.Value16 = TableALU[5, RegAF.High8, RegAF.High8, 0];
							break;
						case 0xB0: // OR B
							ClockCycles = 4;
							RegAF.Value16 = TableALU[6, RegAF.High8, RegBC.High8, 0];
							break;
						case 0xB1: // OR C
							ClockCycles = 4;
							RegAF.Value16 = TableALU[6, RegAF.High8, RegBC.Low8, 0];
							break;
						case 0xB2: // OR D
							ClockCycles = 4;
							RegAF.Value16 = TableALU[6, RegAF.High8, RegDE.High8, 0];
							break;
						case 0xB3: // OR E
							ClockCycles = 4;
							RegAF.Value16 = TableALU[6, RegAF.High8, RegDE.Low8, 0];
							break;
						case 0xB4: // OR H
							ClockCycles = 4;
							RegAF.Value16 = TableALU[6, RegAF.High8, RegHL.High8, 0];
							break;
						case 0xB5: // OR L
							ClockCycles = 4;
							RegAF.Value16 = TableALU[6, RegAF.High8, RegHL.Low8, 0];
							break;
						case 0xB6: // OR (HL)
							ClockCycles = 7;
							RegAF.Value16 = TableALU[6, RegAF.High8, ReadMemory(RegHL.Value16), 0];
							break;
						case 0xB7: // OR A
							ClockCycles = 4;
							RegAF.Value16 = TableALU[6, RegAF.High8, RegAF.High8, 0];
							break;
						case 0xB8: // CP B
							ClockCycles = 4;
							RegAF.Value16 = TableALU[7, RegAF.High8, RegBC.High8, 0];
							break;
						case 0xB9: // CP C
							ClockCycles = 4;
							RegAF.Value16 = TableALU[7, RegAF.High8, RegBC.Low8, 0];
							break;
						case 0xBA: // CP D
							ClockCycles = 4;
							RegAF.Value16 = TableALU[7, RegAF.High8, RegDE.High8, 0];
							break;
						case 0xBB: // CP E
							ClockCycles = 4;
							RegAF.Value16 = TableALU[7, RegAF.High8, RegDE.Low8, 0];
							break;
						case 0xBC: // CP H
							ClockCycles = 4;
							RegAF.Value16 = TableALU[7, RegAF.High8, RegHL.High8, 0];
							break;
						case 0xBD: // CP L
							ClockCycles = 4;
							RegAF.Value16 = TableALU[7, RegAF.High8, RegHL.Low8, 0];
							break;
						case 0xBE: // CP (HL)
							ClockCycles = 7;
							RegAF.Value16 = TableALU[7, RegAF.High8, ReadMemory(RegHL.Value16), 0];
							break;
						case 0xBF: // CP A
							ClockCycles = 4;
							RegAF.Value16 = TableALU[7, RegAF.High8, RegAF.High8, 0];
							break;
						case 0xC0: // RET NZ
							ClockCycles = 11;
							if (!RegFlagZ) {
								RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
							} else {
								ClockCycles = 5;
							}
							break;
						case 0xC1: // POP BC
							ClockCycles = 10;
							RegBC.Low8 = ReadMemory(RegSP.Value16++); RegBC.High8 = ReadMemory(RegSP.Value16++);
							break;
						case 0xC2: // JP NZ, nn
							ClockCycles = 10;
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (!RegFlagZ) {
								RegPC.Value16 = TUS;
							} else {
								ClockCycles = 10;
							}
							break;
						case 0xC3: // JP nn
							ClockCycles = 10;
							RegPC.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							break;
						case 0xC4: // CALL NZ, nn
							ClockCycles = 17;
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (!RegFlagZ) {
								WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
								RegPC.Value16 = TUS;
							} else {
								ClockCycles = 10;
							}
							break;
						case 0xC5: // PUSH BC
							ClockCycles = 11;
							WriteMemory(--RegSP.Value16, RegBC.High8); WriteMemory(--RegSP.Value16, RegBC.Low8);
							break;
						case 0xC6: // ADD A, n
							ClockCycles = 7;
							RegAF.Value16 = TableALU[0, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
							break;
						case 0xC7: // RST $00
							ClockCycles = 11;
							WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
							RegPC.Value16 = 0x00;
							break;
						case 0xC8: // RET Z
							ClockCycles = 11;
							if (RegFlagZ) {
								RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
							} else {
								ClockCycles = 5;
							}
							break;
						case 0xC9: // RET
							ClockCycles = 10;
							RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
							break;
						case 0xCA: // JP Z, nn
							ClockCycles = 10;
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (RegFlagZ) {
								RegPC.Value16 = TUS;
							} else {
								ClockCycles = 10;
							}
							break;
						case 0xCB: // (Prefix)
							++RegR;
							switch (ReadMemory(RegPC.Value16++)) {
								case 0x00: // RLC B
									ClockCycles = 8;
									TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * RegBC.High8];
									RegBC.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x01: // RLC C
									ClockCycles = 8;
									TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * RegBC.Low8];
									RegBC.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x02: // RLC D
									ClockCycles = 8;
									TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * RegDE.High8];
									RegDE.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x03: // RLC E
									ClockCycles = 8;
									TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * RegDE.Low8];
									RegDE.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x04: // RLC H
									ClockCycles = 8;
									TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * RegHL.High8];
									RegHL.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x05: // RLC L
									ClockCycles = 8;
									TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * RegHL.Low8];
									RegHL.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x06: // RLC (HL)
									ClockCycles = 15;
									TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory(RegHL.Value16)];
									WriteMemory(RegHL.Value16, (byte)(TUS >> 8));
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x07: // RLC A
									ClockCycles = 8;
									TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * RegAF.High8];
									RegAF.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x08: // RRC B
									ClockCycles = 8;
									TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * RegBC.High8];
									RegBC.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x09: // RRC C
									ClockCycles = 8;
									TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * RegBC.Low8];
									RegBC.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x0A: // RRC D
									ClockCycles = 8;
									TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * RegDE.High8];
									RegDE.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x0B: // RRC E
									ClockCycles = 8;
									TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * RegDE.Low8];
									RegDE.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x0C: // RRC H
									ClockCycles = 8;
									TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * RegHL.High8];
									RegHL.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x0D: // RRC L
									ClockCycles = 8;
									TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * RegHL.Low8];
									RegHL.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x0E: // RRC (HL)
									ClockCycles = 15;
									TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory(RegHL.Value16)];
									WriteMemory(RegHL.Value16, (byte)(TUS >> 8));
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x0F: // RRC A
									ClockCycles = 8;
									TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * RegAF.High8];
									RegAF.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x10: // RL B
									ClockCycles = 8;
									TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * RegBC.High8];
									RegBC.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x11: // RL C
									ClockCycles = 8;
									TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * RegBC.Low8];
									RegBC.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x12: // RL D
									ClockCycles = 8;
									TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * RegDE.High8];
									RegDE.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x13: // RL E
									ClockCycles = 8;
									TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * RegDE.Low8];
									RegDE.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x14: // RL H
									ClockCycles = 8;
									TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * RegHL.High8];
									RegHL.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x15: // RL L
									ClockCycles = 8;
									TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * RegHL.Low8];
									RegHL.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x16: // RL (HL)
									ClockCycles = 15;
									TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory(RegHL.Value16)];
									WriteMemory(RegHL.Value16, (byte)(TUS >> 8));
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x17: // RL A
									ClockCycles = 8;
									TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * RegAF.High8];
									RegAF.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x18: // RR B
									ClockCycles = 8;
									TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * RegBC.High8];
									RegBC.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x19: // RR C
									ClockCycles = 8;
									TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * RegBC.Low8];
									RegBC.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x1A: // RR D
									ClockCycles = 8;
									TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * RegDE.High8];
									RegDE.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x1B: // RR E
									ClockCycles = 8;
									TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * RegDE.Low8];
									RegDE.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x1C: // RR H
									ClockCycles = 8;
									TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * RegHL.High8];
									RegHL.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x1D: // RR L
									ClockCycles = 8;
									TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * RegHL.Low8];
									RegHL.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x1E: // RR (HL)
									ClockCycles = 15;
									TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory(RegHL.Value16)];
									WriteMemory(RegHL.Value16, (byte)(TUS >> 8));
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x1F: // RR A
									ClockCycles = 8;
									TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * RegAF.High8];
									RegAF.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x20: // SLA B
									ClockCycles = 8;
									TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * RegBC.High8];
									RegBC.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x21: // SLA C
									ClockCycles = 8;
									TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * RegBC.Low8];
									RegBC.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x22: // SLA D
									ClockCycles = 8;
									TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * RegDE.High8];
									RegDE.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x23: // SLA E
									ClockCycles = 8;
									TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * RegDE.Low8];
									RegDE.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x24: // SLA H
									ClockCycles = 8;
									TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * RegHL.High8];
									RegHL.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x25: // SLA L
									ClockCycles = 8;
									TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * RegHL.Low8];
									RegHL.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x26: // SLA (HL)
									ClockCycles = 15;
									TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory(RegHL.Value16)];
									WriteMemory(RegHL.Value16, (byte)(TUS >> 8));
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x27: // SLA A
									ClockCycles = 8;
									TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * RegAF.High8];
									RegAF.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x28: // SRA B
									ClockCycles = 8;
									TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * RegBC.High8];
									RegBC.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x29: // SRA C
									ClockCycles = 8;
									TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * RegBC.Low8];
									RegBC.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x2A: // SRA D
									ClockCycles = 8;
									TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * RegDE.High8];
									RegDE.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x2B: // SRA E
									ClockCycles = 8;
									TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * RegDE.Low8];
									RegDE.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x2C: // SRA H
									ClockCycles = 8;
									TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * RegHL.High8];
									RegHL.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x2D: // SRA L
									ClockCycles = 8;
									TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * RegHL.Low8];
									RegHL.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x2E: // SRA (HL)
									ClockCycles = 15;
									TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory(RegHL.Value16)];
									WriteMemory(RegHL.Value16, (byte)(TUS >> 8));
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x2F: // SRA A
									ClockCycles = 8;
									TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * RegAF.High8];
									RegAF.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x30: // SL1 B
									ClockCycles = 8;
									TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * RegBC.High8];
									RegBC.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x31: // SL1 C
									ClockCycles = 8;
									TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * RegBC.Low8];
									RegBC.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x32: // SL1 D
									ClockCycles = 8;
									TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * RegDE.High8];
									RegDE.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x33: // SL1 E
									ClockCycles = 8;
									TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * RegDE.Low8];
									RegDE.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x34: // SL1 H
									ClockCycles = 8;
									TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * RegHL.High8];
									RegHL.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x35: // SL1 L
									ClockCycles = 8;
									TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * RegHL.Low8];
									RegHL.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x36: // SL1 (HL)
									ClockCycles = 15;
									TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory(RegHL.Value16)];
									WriteMemory(RegHL.Value16, (byte)(TUS >> 8));
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x37: // SL1 A
									ClockCycles = 8;
									TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * RegAF.High8];
									RegAF.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x38: // SRL B
									ClockCycles = 8;
									TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * RegBC.High8];
									RegBC.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x39: // SRL C
									ClockCycles = 8;
									TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * RegBC.Low8];
									RegBC.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x3A: // SRL D
									ClockCycles = 8;
									TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * RegDE.High8];
									RegDE.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x3B: // SRL E
									ClockCycles = 8;
									TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * RegDE.Low8];
									RegDE.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x3C: // SRL H
									ClockCycles = 8;
									TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * RegHL.High8];
									RegHL.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x3D: // SRL L
									ClockCycles = 8;
									TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * RegHL.Low8];
									RegHL.Low8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x3E: // SRL (HL)
									ClockCycles = 15;
									TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory(RegHL.Value16)];
									WriteMemory(RegHL.Value16, (byte)(TUS >> 8));
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x3F: // SRL A
									ClockCycles = 8;
									TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * RegAF.High8];
									RegAF.High8 = (byte)(TUS >> 8);
									RegAF.Low8 = (byte)TUS;
									break;
								case 0x40: // BIT 0, B
									ClockCycles = 8;
									RegFlagZ = (RegBC.High8 & 0x01) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x41: // BIT 0, C
									ClockCycles = 8;
									RegFlagZ = (RegBC.Low8 & 0x01) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x42: // BIT 0, D
									ClockCycles = 8;
									RegFlagZ = (RegDE.High8 & 0x01) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x43: // BIT 0, E
									ClockCycles = 8;
									RegFlagZ = (RegDE.Low8 & 0x01) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x44: // BIT 0, H
									ClockCycles = 8;
									RegFlagZ = (RegHL.High8 & 0x01) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x45: // BIT 0, L
									ClockCycles = 8;
									RegFlagZ = (RegHL.Low8 & 0x01) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x46: // BIT 0, (HL)
									ClockCycles = 12;
									RegFlagZ = (ReadMemory(RegHL.Value16) & 0x01) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x47: // BIT 0, A
									ClockCycles = 8;
									RegFlagZ = (RegAF.High8 & 0x01) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x48: // BIT 1, B
									ClockCycles = 8;
									RegFlagZ = (RegBC.High8 & 0x02) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x49: // BIT 1, C
									ClockCycles = 8;
									RegFlagZ = (RegBC.Low8 & 0x02) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x4A: // BIT 1, D
									ClockCycles = 8;
									RegFlagZ = (RegDE.High8 & 0x02) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x4B: // BIT 1, E
									ClockCycles = 8;
									RegFlagZ = (RegDE.Low8 & 0x02) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x4C: // BIT 1, H
									ClockCycles = 8;
									RegFlagZ = (RegHL.High8 & 0x02) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x4D: // BIT 1, L
									ClockCycles = 8;
									RegFlagZ = (RegHL.Low8 & 0x02) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x4E: // BIT 1, (HL)
									ClockCycles = 12;
									RegFlagZ = (ReadMemory(RegHL.Value16) & 0x02) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x4F: // BIT 1, A
									ClockCycles = 8;
									RegFlagZ = (RegAF.High8 & 0x02) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x50: // BIT 2, B
									ClockCycles = 8;
									RegFlagZ = (RegBC.High8 & 0x04) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x51: // BIT 2, C
									ClockCycles = 8;
									RegFlagZ = (RegBC.Low8 & 0x04) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x52: // BIT 2, D
									ClockCycles = 8;
									RegFlagZ = (RegDE.High8 & 0x04) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x53: // BIT 2, E
									ClockCycles = 8;
									RegFlagZ = (RegDE.Low8 & 0x04) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x54: // BIT 2, H
									ClockCycles = 8;
									RegFlagZ = (RegHL.High8 & 0x04) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x55: // BIT 2, L
									ClockCycles = 8;
									RegFlagZ = (RegHL.Low8 & 0x04) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x56: // BIT 2, (HL)
									ClockCycles = 12;
									RegFlagZ = (ReadMemory(RegHL.Value16) & 0x04) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x57: // BIT 2, A
									ClockCycles = 8;
									RegFlagZ = (RegAF.High8 & 0x04) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x58: // BIT 3, B
									ClockCycles = 8;
									RegFlagZ = (RegBC.High8 & 0x08) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x59: // BIT 3, C
									ClockCycles = 8;
									RegFlagZ = (RegBC.Low8 & 0x08) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x5A: // BIT 3, D
									ClockCycles = 8;
									RegFlagZ = (RegDE.High8 & 0x08) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x5B: // BIT 3, E
									ClockCycles = 8;
									RegFlagZ = (RegDE.Low8 & 0x08) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x5C: // BIT 3, H
									ClockCycles = 8;
									RegFlagZ = (RegHL.High8 & 0x08) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x5D: // BIT 3, L
									ClockCycles = 8;
									RegFlagZ = (RegHL.Low8 & 0x08) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x5E: // BIT 3, (HL)
									ClockCycles = 12;
									RegFlagZ = (ReadMemory(RegHL.Value16) & 0x08) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x5F: // BIT 3, A
									ClockCycles = 8;
									RegFlagZ = (RegAF.High8 & 0x08) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x60: // BIT 4, B
									ClockCycles = 8;
									RegFlagZ = (RegBC.High8 & 0x10) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x61: // BIT 4, C
									ClockCycles = 8;
									RegFlagZ = (RegBC.Low8 & 0x10) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x62: // BIT 4, D
									ClockCycles = 8;
									RegFlagZ = (RegDE.High8 & 0x10) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x63: // BIT 4, E
									ClockCycles = 8;
									RegFlagZ = (RegDE.Low8 & 0x10) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x64: // BIT 4, H
									ClockCycles = 8;
									RegFlagZ = (RegHL.High8 & 0x10) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x65: // BIT 4, L
									ClockCycles = 8;
									RegFlagZ = (RegHL.Low8 & 0x10) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x66: // BIT 4, (HL)
									ClockCycles = 12;
									RegFlagZ = (ReadMemory(RegHL.Value16) & 0x10) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x67: // BIT 4, A
									ClockCycles = 8;
									RegFlagZ = (RegAF.High8 & 0x10) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x68: // BIT 5, B
									ClockCycles = 8;
									RegFlagZ = (RegBC.High8 & 0x20) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x69: // BIT 5, C
									ClockCycles = 8;
									RegFlagZ = (RegBC.Low8 & 0x20) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x6A: // BIT 5, D
									ClockCycles = 8;
									RegFlagZ = (RegDE.High8 & 0x20) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x6B: // BIT 5, E
									ClockCycles = 8;
									RegFlagZ = (RegDE.Low8 & 0x20) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x6C: // BIT 5, H
									ClockCycles = 8;
									RegFlagZ = (RegHL.High8 & 0x20) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x6D: // BIT 5, L
									ClockCycles = 8;
									RegFlagZ = (RegHL.Low8 & 0x20) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x6E: // BIT 5, (HL)
									ClockCycles = 12;
									RegFlagZ = (ReadMemory(RegHL.Value16) & 0x20) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x6F: // BIT 5, A
									ClockCycles = 8;
									RegFlagZ = (RegAF.High8 & 0x20) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x70: // BIT 6, B
									ClockCycles = 8;
									RegFlagZ = (RegBC.High8 & 0x40) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x71: // BIT 6, C
									ClockCycles = 8;
									RegFlagZ = (RegBC.Low8 & 0x40) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x72: // BIT 6, D
									ClockCycles = 8;
									RegFlagZ = (RegDE.High8 & 0x40) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x73: // BIT 6, E
									ClockCycles = 8;
									RegFlagZ = (RegDE.Low8 & 0x40) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x74: // BIT 6, H
									ClockCycles = 8;
									RegFlagZ = (RegHL.High8 & 0x40) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x75: // BIT 6, L
									ClockCycles = 8;
									RegFlagZ = (RegHL.Low8 & 0x40) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x76: // BIT 6, (HL)
									ClockCycles = 12;
									RegFlagZ = (ReadMemory(RegHL.Value16) & 0x40) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x77: // BIT 6, A
									ClockCycles = 8;
									RegFlagZ = (RegAF.High8 & 0x40) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = false;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x78: // BIT 7, B
									ClockCycles = 8;
									RegFlagZ = (RegBC.High8 & 0x80) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = !RegFlagZ;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x79: // BIT 7, C
									ClockCycles = 8;
									RegFlagZ = (RegBC.Low8 & 0x80) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = !RegFlagZ;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x7A: // BIT 7, D
									ClockCycles = 8;
									RegFlagZ = (RegDE.High8 & 0x80) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = !RegFlagZ;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x7B: // BIT 7, E
									ClockCycles = 8;
									RegFlagZ = (RegDE.Low8 & 0x80) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = !RegFlagZ;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x7C: // BIT 7, H
									ClockCycles = 8;
									RegFlagZ = (RegHL.High8 & 0x80) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = !RegFlagZ;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x7D: // BIT 7, L
									ClockCycles = 8;
									RegFlagZ = (RegHL.Low8 & 0x80) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = !RegFlagZ;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x7E: // BIT 7, (HL)
									ClockCycles = 12;
									RegFlagZ = (ReadMemory(RegHL.Value16) & 0x80) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = !RegFlagZ;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x7F: // BIT 7, A
									ClockCycles = 8;
									RegFlagZ = (RegAF.High8 & 0x80) == 0;
									RegFlagP = RegFlagZ;
									RegFlagS = !RegFlagZ;
									RegFlagH = true;
									RegFlagN = false;
									break;
								case 0x80: // RES 0, B
									ClockCycles = 8;
									RegBC.High8 &= unchecked((byte)~0x01);
									break;
								case 0x81: // RES 0, C
									ClockCycles = 8;
									RegBC.Low8 &= unchecked((byte)~0x01);
									break;
								case 0x82: // RES 0, D
									ClockCycles = 8;
									RegDE.High8 &= unchecked((byte)~0x01);
									break;
								case 0x83: // RES 0, E
									ClockCycles = 8;
									RegDE.Low8 &= unchecked((byte)~0x01);
									break;
								case 0x84: // RES 0, H
									ClockCycles = 8;
									RegHL.High8 &= unchecked((byte)~0x01);
									break;
								case 0x85: // RES 0, L
									ClockCycles = 8;
									RegHL.Low8 &= unchecked((byte)~0x01);
									break;
								case 0x86: // RES 0, (HL)
									ClockCycles = 12;
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) & unchecked((byte)~0x01)));
									break;
								case 0x87: // RES 0, A
									ClockCycles = 8;
									RegAF.High8 &= unchecked((byte)~0x01);
									break;
								case 0x88: // RES 1, B
									ClockCycles = 8;
									RegBC.High8 &= unchecked((byte)~0x02);
									break;
								case 0x89: // RES 1, C
									ClockCycles = 8;
									RegBC.Low8 &= unchecked((byte)~0x02);
									break;
								case 0x8A: // RES 1, D
									ClockCycles = 8;
									RegDE.High8 &= unchecked((byte)~0x02);
									break;
								case 0x8B: // RES 1, E
									ClockCycles = 8;
									RegDE.Low8 &= unchecked((byte)~0x02);
									break;
								case 0x8C: // RES 1, H
									ClockCycles = 8;
									RegHL.High8 &= unchecked((byte)~0x02);
									break;
								case 0x8D: // RES 1, L
									ClockCycles = 8;
									RegHL.Low8 &= unchecked((byte)~0x02);
									break;
								case 0x8E: // RES 1, (HL)
									ClockCycles = 12;
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) & unchecked((byte)~0x02)));
									break;
								case 0x8F: // RES 1, A
									ClockCycles = 8;
									RegAF.High8 &= unchecked((byte)~0x02);
									break;
								case 0x90: // RES 2, B
									ClockCycles = 8;
									RegBC.High8 &= unchecked((byte)~0x04);
									break;
								case 0x91: // RES 2, C
									ClockCycles = 8;
									RegBC.Low8 &= unchecked((byte)~0x04);
									break;
								case 0x92: // RES 2, D
									ClockCycles = 8;
									RegDE.High8 &= unchecked((byte)~0x04);
									break;
								case 0x93: // RES 2, E
									ClockCycles = 8;
									RegDE.Low8 &= unchecked((byte)~0x04);
									break;
								case 0x94: // RES 2, H
									ClockCycles = 8;
									RegHL.High8 &= unchecked((byte)~0x04);
									break;
								case 0x95: // RES 2, L
									ClockCycles = 8;
									RegHL.Low8 &= unchecked((byte)~0x04);
									break;
								case 0x96: // RES 2, (HL)
									ClockCycles = 12;
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) & unchecked((byte)~0x04)));
									break;
								case 0x97: // RES 2, A
									ClockCycles = 8;
									RegAF.High8 &= unchecked((byte)~0x04);
									break;
								case 0x98: // RES 3, B
									ClockCycles = 8;
									RegBC.High8 &= unchecked((byte)~0x08);
									break;
								case 0x99: // RES 3, C
									ClockCycles = 8;
									RegBC.Low8 &= unchecked((byte)~0x08);
									break;
								case 0x9A: // RES 3, D
									ClockCycles = 8;
									RegDE.High8 &= unchecked((byte)~0x08);
									break;
								case 0x9B: // RES 3, E
									ClockCycles = 8;
									RegDE.Low8 &= unchecked((byte)~0x08);
									break;
								case 0x9C: // RES 3, H
									ClockCycles = 8;
									RegHL.High8 &= unchecked((byte)~0x08);
									break;
								case 0x9D: // RES 3, L
									ClockCycles = 8;
									RegHL.Low8 &= unchecked((byte)~0x08);
									break;
								case 0x9E: // RES 3, (HL)
									ClockCycles = 12;
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) & unchecked((byte)~0x08)));
									break;
								case 0x9F: // RES 3, A
									ClockCycles = 8;
									RegAF.High8 &= unchecked((byte)~0x08);
									break;
								case 0xA0: // RES 4, B
									ClockCycles = 8;
									RegBC.High8 &= unchecked((byte)~0x10);
									break;
								case 0xA1: // RES 4, C
									ClockCycles = 8;
									RegBC.Low8 &= unchecked((byte)~0x10);
									break;
								case 0xA2: // RES 4, D
									ClockCycles = 8;
									RegDE.High8 &= unchecked((byte)~0x10);
									break;
								case 0xA3: // RES 4, E
									ClockCycles = 8;
									RegDE.Low8 &= unchecked((byte)~0x10);
									break;
								case 0xA4: // RES 4, H
									ClockCycles = 8;
									RegHL.High8 &= unchecked((byte)~0x10);
									break;
								case 0xA5: // RES 4, L
									ClockCycles = 8;
									RegHL.Low8 &= unchecked((byte)~0x10);
									break;
								case 0xA6: // RES 4, (HL)
									ClockCycles = 12;
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) & unchecked((byte)~0x10)));
									break;
								case 0xA7: // RES 4, A
									ClockCycles = 8;
									RegAF.High8 &= unchecked((byte)~0x10);
									break;
								case 0xA8: // RES 5, B
									ClockCycles = 8;
									RegBC.High8 &= unchecked((byte)~0x20);
									break;
								case 0xA9: // RES 5, C
									ClockCycles = 8;
									RegBC.Low8 &= unchecked((byte)~0x20);
									break;
								case 0xAA: // RES 5, D
									ClockCycles = 8;
									RegDE.High8 &= unchecked((byte)~0x20);
									break;
								case 0xAB: // RES 5, E
									ClockCycles = 8;
									RegDE.Low8 &= unchecked((byte)~0x20);
									break;
								case 0xAC: // RES 5, H
									ClockCycles = 8;
									RegHL.High8 &= unchecked((byte)~0x20);
									break;
								case 0xAD: // RES 5, L
									ClockCycles = 8;
									RegHL.Low8 &= unchecked((byte)~0x20);
									break;
								case 0xAE: // RES 5, (HL)
									ClockCycles = 12;
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) & unchecked((byte)~0x20)));
									break;
								case 0xAF: // RES 5, A
									ClockCycles = 8;
									RegAF.High8 &= unchecked((byte)~0x20);
									break;
								case 0xB0: // RES 6, B
									ClockCycles = 8;
									RegBC.High8 &= unchecked((byte)~0x40);
									break;
								case 0xB1: // RES 6, C
									ClockCycles = 8;
									RegBC.Low8 &= unchecked((byte)~0x40);
									break;
								case 0xB2: // RES 6, D
									ClockCycles = 8;
									RegDE.High8 &= unchecked((byte)~0x40);
									break;
								case 0xB3: // RES 6, E
									ClockCycles = 8;
									RegDE.Low8 &= unchecked((byte)~0x40);
									break;
								case 0xB4: // RES 6, H
									ClockCycles = 8;
									RegHL.High8 &= unchecked((byte)~0x40);
									break;
								case 0xB5: // RES 6, L
									ClockCycles = 8;
									RegHL.Low8 &= unchecked((byte)~0x40);
									break;
								case 0xB6: // RES 6, (HL)
									ClockCycles = 12;
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) & unchecked((byte)~0x40)));
									break;
								case 0xB7: // RES 6, A
									ClockCycles = 8;
									RegAF.High8 &= unchecked((byte)~0x40);
									break;
								case 0xB8: // RES 7, B
									ClockCycles = 8;
									RegBC.High8 &= unchecked((byte)~0x80);
									break;
								case 0xB9: // RES 7, C
									ClockCycles = 8;
									RegBC.Low8 &= unchecked((byte)~0x80);
									break;
								case 0xBA: // RES 7, D
									ClockCycles = 8;
									RegDE.High8 &= unchecked((byte)~0x80);
									break;
								case 0xBB: // RES 7, E
									ClockCycles = 8;
									RegDE.Low8 &= unchecked((byte)~0x80);
									break;
								case 0xBC: // RES 7, H
									ClockCycles = 8;
									RegHL.High8 &= unchecked((byte)~0x80);
									break;
								case 0xBD: // RES 7, L
									ClockCycles = 8;
									RegHL.Low8 &= unchecked((byte)~0x80);
									break;
								case 0xBE: // RES 7, (HL)
									ClockCycles = 12;
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) & unchecked((byte)~0x80)));
									break;
								case 0xBF: // RES 7, A
									ClockCycles = 8;
									RegAF.High8 &= unchecked((byte)~0x80);
									break;
								case 0xC0: // SET 0, B
									ClockCycles = 8;
									RegBC.High8 |= unchecked((byte)0x01);
									break;
								case 0xC1: // SET 0, C
									ClockCycles = 8;
									RegBC.Low8 |= unchecked((byte)0x01);
									break;
								case 0xC2: // SET 0, D
									ClockCycles = 8;
									RegDE.High8 |= unchecked((byte)0x01);
									break;
								case 0xC3: // SET 0, E
									ClockCycles = 8;
									RegDE.Low8 |= unchecked((byte)0x01);
									break;
								case 0xC4: // SET 0, H
									ClockCycles = 8;
									RegHL.High8 |= unchecked((byte)0x01);
									break;
								case 0xC5: // SET 0, L
									ClockCycles = 8;
									RegHL.Low8 |= unchecked((byte)0x01);
									break;
								case 0xC6: // SET 0, (HL)
									ClockCycles = 12;
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) | unchecked((byte)0x01)));
									break;
								case 0xC7: // SET 0, A
									ClockCycles = 8;
									RegAF.High8 |= unchecked((byte)0x01);
									break;
								case 0xC8: // SET 1, B
									ClockCycles = 8;
									RegBC.High8 |= unchecked((byte)0x02);
									break;
								case 0xC9: // SET 1, C
									ClockCycles = 8;
									RegBC.Low8 |= unchecked((byte)0x02);
									break;
								case 0xCA: // SET 1, D
									ClockCycles = 8;
									RegDE.High8 |= unchecked((byte)0x02);
									break;
								case 0xCB: // SET 1, E
									ClockCycles = 8;
									RegDE.Low8 |= unchecked((byte)0x02);
									break;
								case 0xCC: // SET 1, H
									ClockCycles = 8;
									RegHL.High8 |= unchecked((byte)0x02);
									break;
								case 0xCD: // SET 1, L
									ClockCycles = 8;
									RegHL.Low8 |= unchecked((byte)0x02);
									break;
								case 0xCE: // SET 1, (HL)
									ClockCycles = 12;
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) | unchecked((byte)0x02)));
									break;
								case 0xCF: // SET 1, A
									ClockCycles = 8;
									RegAF.High8 |= unchecked((byte)0x02);
									break;
								case 0xD0: // SET 2, B
									ClockCycles = 8;
									RegBC.High8 |= unchecked((byte)0x04);
									break;
								case 0xD1: // SET 2, C
									ClockCycles = 8;
									RegBC.Low8 |= unchecked((byte)0x04);
									break;
								case 0xD2: // SET 2, D
									ClockCycles = 8;
									RegDE.High8 |= unchecked((byte)0x04);
									break;
								case 0xD3: // SET 2, E
									ClockCycles = 8;
									RegDE.Low8 |= unchecked((byte)0x04);
									break;
								case 0xD4: // SET 2, H
									ClockCycles = 8;
									RegHL.High8 |= unchecked((byte)0x04);
									break;
								case 0xD5: // SET 2, L
									ClockCycles = 8;
									RegHL.Low8 |= unchecked((byte)0x04);
									break;
								case 0xD6: // SET 2, (HL)
									ClockCycles = 12;
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) | unchecked((byte)0x04)));
									break;
								case 0xD7: // SET 2, A
									ClockCycles = 8;
									RegAF.High8 |= unchecked((byte)0x04);
									break;
								case 0xD8: // SET 3, B
									ClockCycles = 8;
									RegBC.High8 |= unchecked((byte)0x08);
									break;
								case 0xD9: // SET 3, C
									ClockCycles = 8;
									RegBC.Low8 |= unchecked((byte)0x08);
									break;
								case 0xDA: // SET 3, D
									ClockCycles = 8;
									RegDE.High8 |= unchecked((byte)0x08);
									break;
								case 0xDB: // SET 3, E
									ClockCycles = 8;
									RegDE.Low8 |= unchecked((byte)0x08);
									break;
								case 0xDC: // SET 3, H
									ClockCycles = 8;
									RegHL.High8 |= unchecked((byte)0x08);
									break;
								case 0xDD: // SET 3, L
									ClockCycles = 8;
									RegHL.Low8 |= unchecked((byte)0x08);
									break;
								case 0xDE: // SET 3, (HL)
									ClockCycles = 12;
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) | unchecked((byte)0x08)));
									break;
								case 0xDF: // SET 3, A
									ClockCycles = 8;
									RegAF.High8 |= unchecked((byte)0x08);
									break;
								case 0xE0: // SET 4, B
									ClockCycles = 8;
									RegBC.High8 |= unchecked((byte)0x10);
									break;
								case 0xE1: // SET 4, C
									ClockCycles = 8;
									RegBC.Low8 |= unchecked((byte)0x10);
									break;
								case 0xE2: // SET 4, D
									ClockCycles = 8;
									RegDE.High8 |= unchecked((byte)0x10);
									break;
								case 0xE3: // SET 4, E
									ClockCycles = 8;
									RegDE.Low8 |= unchecked((byte)0x10);
									break;
								case 0xE4: // SET 4, H
									ClockCycles = 8;
									RegHL.High8 |= unchecked((byte)0x10);
									break;
								case 0xE5: // SET 4, L
									ClockCycles = 8;
									RegHL.Low8 |= unchecked((byte)0x10);
									break;
								case 0xE6: // SET 4, (HL)
									ClockCycles = 12;
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) | unchecked((byte)0x10)));
									break;
								case 0xE7: // SET 4, A
									ClockCycles = 8;
									RegAF.High8 |= unchecked((byte)0x10);
									break;
								case 0xE8: // SET 5, B
									ClockCycles = 8;
									RegBC.High8 |= unchecked((byte)0x20);
									break;
								case 0xE9: // SET 5, C
									ClockCycles = 8;
									RegBC.Low8 |= unchecked((byte)0x20);
									break;
								case 0xEA: // SET 5, D
									ClockCycles = 8;
									RegDE.High8 |= unchecked((byte)0x20);
									break;
								case 0xEB: // SET 5, E
									ClockCycles = 8;
									RegDE.Low8 |= unchecked((byte)0x20);
									break;
								case 0xEC: // SET 5, H
									ClockCycles = 8;
									RegHL.High8 |= unchecked((byte)0x20);
									break;
								case 0xED: // SET 5, L
									ClockCycles = 8;
									RegHL.Low8 |= unchecked((byte)0x20);
									break;
								case 0xEE: // SET 5, (HL)
									ClockCycles = 12;
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) | unchecked((byte)0x20)));
									break;
								case 0xEF: // SET 5, A
									ClockCycles = 8;
									RegAF.High8 |= unchecked((byte)0x20);
									break;
								case 0xF0: // SET 6, B
									ClockCycles = 8;
									RegBC.High8 |= unchecked((byte)0x40);
									break;
								case 0xF1: // SET 6, C
									ClockCycles = 8;
									RegBC.Low8 |= unchecked((byte)0x40);
									break;
								case 0xF2: // SET 6, D
									ClockCycles = 8;
									RegDE.High8 |= unchecked((byte)0x40);
									break;
								case 0xF3: // SET 6, E
									ClockCycles = 8;
									RegDE.Low8 |= unchecked((byte)0x40);
									break;
								case 0xF4: // SET 6, H
									ClockCycles = 8;
									RegHL.High8 |= unchecked((byte)0x40);
									break;
								case 0xF5: // SET 6, L
									ClockCycles = 8;
									RegHL.Low8 |= unchecked((byte)0x40);
									break;
								case 0xF6: // SET 6, (HL)
									ClockCycles = 12;
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) | unchecked((byte)0x40)));
									break;
								case 0xF7: // SET 6, A
									ClockCycles = 8;
									RegAF.High8 |= unchecked((byte)0x40);
									break;
								case 0xF8: // SET 7, B
									ClockCycles = 8;
									RegBC.High8 |= unchecked((byte)0x80);
									break;
								case 0xF9: // SET 7, C
									ClockCycles = 8;
									RegBC.Low8 |= unchecked((byte)0x80);
									break;
								case 0xFA: // SET 7, D
									ClockCycles = 8;
									RegDE.High8 |= unchecked((byte)0x80);
									break;
								case 0xFB: // SET 7, E
									ClockCycles = 8;
									RegDE.Low8 |= unchecked((byte)0x80);
									break;
								case 0xFC: // SET 7, H
									ClockCycles = 8;
									RegHL.High8 |= unchecked((byte)0x80);
									break;
								case 0xFD: // SET 7, L
									ClockCycles = 8;
									RegHL.Low8 |= unchecked((byte)0x80);
									break;
								case 0xFE: // SET 7, (HL)
									ClockCycles = 12;
									WriteMemory(RegHL.Value16, (byte)(ReadMemory(RegHL.Value16) | unchecked((byte)0x80)));
									break;
								case 0xFF: // SET 7, A
									ClockCycles = 8;
									RegAF.High8 |= unchecked((byte)0x80);
									break;
							}
							break;
						case 0xCC: // CALL Z, nn
							ClockCycles = 17;
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (RegFlagZ) {
								WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
								RegPC.Value16 = TUS;
							} else {
								ClockCycles = 10;
							}
							break;
						case 0xCD: // CALL nn
							ClockCycles = 17;
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
							RegPC.Value16 = TUS;
							break;
						case 0xCE: // ADC A, n
							ClockCycles = 7;
							RegAF.Value16 = TableALU[1, RegAF.High8, ReadMemory(RegPC.Value16++), RegFlagC ? 1 : 0];
							break;
						case 0xCF: // RST $08
							ClockCycles = 11;
							WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
							RegPC.Value16 = 0x08;
							break;
						case 0xD0: // RET NC
							ClockCycles = 11;
							if (!RegFlagC) {
								RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
							} else {
								ClockCycles = 5;
							}
							break;
						case 0xD1: // POP DE
							ClockCycles = 10;
							RegDE.Low8 = ReadMemory(RegSP.Value16++); RegDE.High8 = ReadMemory(RegSP.Value16++);
							break;
						case 0xD2: // JP NC, nn
							ClockCycles = 10;
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (!RegFlagC) {
								RegPC.Value16 = TUS;
							} else {
								ClockCycles = 10;
							}
							break;
						case 0xD3: // OUT n, A
							ClockCycles = 11;
							WriteHardware(ReadMemory(RegPC.Value16++), RegAF.High8);
							break;
						case 0xD4: // CALL NC, nn
							ClockCycles = 17;
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (!RegFlagC) {
								WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
								RegPC.Value16 = TUS;
							} else {
								ClockCycles = 10;
							}
							break;
						case 0xD5: // PUSH DE
							ClockCycles = 11;
							WriteMemory(--RegSP.Value16, RegDE.High8); WriteMemory(--RegSP.Value16, RegDE.Low8);
							break;
						case 0xD6: // SUB n
							ClockCycles = 7;
							RegAF.Value16 = TableALU[2, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
							break;
						case 0xD7: // RST $10
							ClockCycles = 11;
							WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
							RegPC.Value16 = 0x10;
							break;
						case 0xD8: // RET C
							ClockCycles = 11;
							if (RegFlagC) {
								RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
							} else {
								ClockCycles = 5;
							}
							break;
						case 0xD9: // EXX
							ClockCycles = 4;
							TUS = RegBC.Value16; RegBC.Value16 = RegAltBC.Value16; RegAltBC.Value16 = TUS;
							TUS = RegDE.Value16; RegDE.Value16 = RegAltDE.Value16; RegAltDE.Value16 = TUS;
							TUS = RegHL.Value16; RegHL.Value16 = RegAltHL.Value16; RegAltHL.Value16 = TUS;
							break;
						case 0xDA: // JP C, nn
							ClockCycles = 10;
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (RegFlagC) {
								RegPC.Value16 = TUS;
							} else {
								ClockCycles = 10;
							}
							break;
						case 0xDB: // IN A, n
							ClockCycles = 11;
							RegAF.High8 = ReadHardware((ushort)ReadMemory(RegPC.Value16++));
							break;
						case 0xDC: // CALL C, nn
							ClockCycles = 17;
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (RegFlagC) {
								WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
								RegPC.Value16 = TUS;
							} else {
								ClockCycles = 10;
							}
							break;
						case 0xDD: // (Prefix)
							++RegR;
							switch (ReadMemory(RegPC.Value16++)) {
								case 0x00: // NOP
									ClockCycles = 4;
									break;
								case 0x01: // LD BC, nn
									ClockCycles = 10;
									RegBC.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									break;
								case 0x02: // LD (BC), A
									ClockCycles = 7;
									WriteMemory(RegBC.Value16, RegAF.High8);
									break;
								case 0x03: // INC BC
									ClockCycles = 6;
									++RegBC.Value16;
									break;
								case 0x04: // INC B
									ClockCycles = 4;
									RegAF.Low8 = (byte)(TableInc[++RegBC.High8] | (RegAF.Low8 & 1));
									break;
								case 0x05: // DEC B
									ClockCycles = 4;
									RegAF.Low8 = (byte)(TableDec[--RegBC.High8] | (RegAF.Low8 & 1));
									break;
								case 0x06: // LD B, n
									ClockCycles = 7;
									RegBC.High8 = ReadMemory(RegPC.Value16++);
									break;
								case 0x07: // RLCA
									ClockCycles = 4;
									RegAF.Value16 = TableRotShift[0, 0, RegAF.Value16];
									break;
								case 0x08: // EX AF, AF'
									ClockCycles = 4;
									TUS = RegAF.Value16; RegAF.Value16 = RegAltAF.Value16; RegAltAF.Value16 = TUS;
									break;
								case 0x09: // ADD IX, BC
									ClockCycles = 15;
									TI1 = (short)RegIX.Value16; TI2 = (short)RegBC.Value16; TIR = TI1 + TI2;
									TUS = (ushort)TIR;
									RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
									RegFlagN = false;
									RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
									RegIX.Value16 = TUS;
									break;
								case 0x0A: // LD A, (BC)
									ClockCycles = 7;
									RegAF.High8 = ReadMemory(RegBC.Value16);
									break;
								case 0x0B: // DEC BC
									ClockCycles = 6;
									--RegBC.Value16;
									break;
								case 0x0C: // INC C
									ClockCycles = 4;
									RegAF.Low8 = (byte)(TableInc[++RegBC.Low8] | (RegAF.Low8 & 1));
									break;
								case 0x0D: // DEC C
									ClockCycles = 4;
									RegAF.Low8 = (byte)(TableDec[--RegBC.Low8] | (RegAF.Low8 & 1));
									break;
								case 0x0E: // LD C, n
									ClockCycles = 7;
									RegBC.Low8 = ReadMemory(RegPC.Value16++);
									break;
								case 0x0F: // RRCA
									ClockCycles = 4;
									RegAF.Value16 = TableRotShift[0, 1, RegAF.Value16];
									break;
								case 0x10: // DJNZ d
									ClockCycles = 13;
									TSB = (sbyte)ReadMemory(RegPC.Value16++);
									if (--RegBC.High8 != 0) {
										RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
									} else {
										ClockCycles = 8;
									}
									break;
								case 0x11: // LD DE, nn
									ClockCycles = 10;
									RegDE.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									break;
								case 0x12: // LD (DE), A
									ClockCycles = 7;
									WriteMemory(RegDE.Value16, RegAF.High8);
									break;
								case 0x13: // INC DE
									ClockCycles = 6;
									++RegDE.Value16;
									break;
								case 0x14: // INC D
									ClockCycles = 4;
									RegAF.Low8 = (byte)(TableInc[++RegDE.High8] | (RegAF.Low8 & 1));
									break;
								case 0x15: // DEC D
									ClockCycles = 4;
									RegAF.Low8 = (byte)(TableDec[--RegDE.High8] | (RegAF.Low8 & 1));
									break;
								case 0x16: // LD D, n
									ClockCycles = 7;
									RegDE.High8 = ReadMemory(RegPC.Value16++);
									break;
								case 0x17: // RLA
									ClockCycles = 4;
									RegAF.Value16 = TableRotShift[0, 2, RegAF.Value16];
									break;
								case 0x18: // JR d
									ClockCycles = 12;
									TSB = (sbyte)ReadMemory(RegPC.Value16++);
									RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
									break;
								case 0x19: // ADD IX, DE
									ClockCycles = 15;
									TI1 = (short)RegIX.Value16; TI2 = (short)RegDE.Value16; TIR = TI1 + TI2;
									TUS = (ushort)TIR;
									RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
									RegFlagN = false;
									RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
									RegIX.Value16 = TUS;
									break;
								case 0x1A: // LD A, (DE)
									ClockCycles = 7;
									RegAF.High8 = ReadMemory(RegDE.Value16);
									break;
								case 0x1B: // DEC DE
									ClockCycles = 6;
									--RegDE.Value16;
									break;
								case 0x1C: // INC E
									ClockCycles = 4;
									RegAF.Low8 = (byte)(TableInc[++RegDE.Low8] | (RegAF.Low8 & 1));
									break;
								case 0x1D: // DEC E
									ClockCycles = 4;
									RegAF.Low8 = (byte)(TableDec[--RegDE.Low8] | (RegAF.Low8 & 1));
									break;
								case 0x1E: // LD E, n
									ClockCycles = 7;
									RegDE.Low8 = ReadMemory(RegPC.Value16++);
									break;
								case 0x1F: // RRA
									ClockCycles = 4;
									RegAF.Value16 = TableRotShift[0, 3, RegAF.Value16];
									break;
								case 0x20: // JR NZ, d
									ClockCycles = 12;
									TSB = (sbyte)ReadMemory(RegPC.Value16++);
									if (!RegFlagZ) {
										RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
									} else {
										ClockCycles = 7;
									}
									break;
								case 0x21: // LD IX, nn
									ClockCycles = 14;
									RegIX.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									break;
								case 0x22: // LD (nn), IX
									ClockCycles = 20;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									WriteMemory(TUS++, RegIX.Low8);
									WriteMemory(TUS, RegIX.High8);
									break;
								case 0x23: // INC IX
									ClockCycles = 10;
									++RegIX.Value16;
									break;
								case 0x24: // INC IXH
									ClockCycles = 9;
									RegAF.Low8 = (byte)(TableInc[++RegIX.High8] | (RegAF.Low8 & 1));
									break;
								case 0x25: // DEC IXH
									ClockCycles = 9;
									RegAF.Low8 = (byte)(TableDec[--RegIX.High8] | (RegAF.Low8 & 1));
									break;
								case 0x26: // LD IXH, n
									ClockCycles = 9;
									RegIX.High8 = ReadMemory(RegPC.Value16++);
									break;
								case 0x27: // DAA
									ClockCycles = 4;
									RegAF.Value16 = TableDaa[RegAF.Value16];
									break;
								case 0x28: // JR Z, d
									ClockCycles = 12;
									TSB = (sbyte)ReadMemory(RegPC.Value16++);
									if (RegFlagZ) {
										RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
									} else {
										ClockCycles = 7;
									}
									break;
								case 0x29: // ADD IX, IX
									ClockCycles = 15;
									TI1 = (short)RegIX.Value16; TI2 = (short)RegIX.Value16; TIR = TI1 + TI2;
									TUS = (ushort)TIR;
									RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
									RegFlagN = false;
									RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
									RegIX.Value16 = TUS;
									break;
								case 0x2A: // LD IX, (nn)
									ClockCycles = 20;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									RegIX.Low8 = ReadMemory(TUS++); RegIX.High8 = ReadMemory(TUS);
									break;
								case 0x2B: // DEC IX
									ClockCycles = 10;
									--RegIX.Value16;
									break;
								case 0x2C: // INC IXL
									ClockCycles = 9;
									RegAF.Low8 = (byte)(TableInc[++RegIX.Low8] | (RegAF.Low8 & 1));
									break;
								case 0x2D: // DEC IXL
									ClockCycles = 9;
									RegAF.Low8 = (byte)(TableDec[--RegIX.Low8] | (RegAF.Low8 & 1));
									break;
								case 0x2E: // LD IXL, n
									ClockCycles = 9;
									RegIX.Low8 = ReadMemory(RegPC.Value16++);
									break;
								case 0x2F: // CPL
									ClockCycles = 4;
									RegAF.High8 ^= 0xFF; RegFlagH = true; RegFlagN = true;
									break;
								case 0x30: // JR NC, d
									ClockCycles = 12;
									TSB = (sbyte)ReadMemory(RegPC.Value16++);
									if (!RegFlagC) {
										RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
									} else {
										ClockCycles = 7;
									}
									break;
								case 0x31: // LD SP, nn
									ClockCycles = 10;
									RegSP.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									break;
								case 0x32: // LD (nn), A
									ClockCycles = 13;
									WriteMemory((ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256), RegAF.High8);
									break;
								case 0x33: // INC SP
									ClockCycles = 6;
									++RegSP.Value16;
									break;
								case 0x34: // INC (IX+d)
									ClockCycles = 23;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									TB = ReadMemory((ushort)(RegIX.Value16 + Displacement)); RegAF.Low8 = (byte)(TableInc[++TB] | (RegAF.Low8 & 1)); WriteMemory((ushort)(RegIX.Value16 + Displacement), TB);
									break;
								case 0x35: // DEC (IX+d)
									ClockCycles = 23;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									TB = ReadMemory((ushort)(RegIX.Value16 + Displacement)); RegAF.Low8 = (byte)(TableDec[--TB] | (RegAF.Low8 & 1)); WriteMemory((ushort)(RegIX.Value16 + Displacement), TB);
									break;
								case 0x36: // LD (IX+d), n
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIX.Value16 + Displacement), ReadMemory(RegPC.Value16++));
									break;
								case 0x37: // SCF
									ClockCycles = 4;
									RegFlagH = false; RegFlagN = false; RegFlagC = true;
									break;
								case 0x38: // JR C, d
									ClockCycles = 12;
									TSB = (sbyte)ReadMemory(RegPC.Value16++);
									if (RegFlagC) {
										RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
									} else {
										ClockCycles = 7;
									}
									break;
								case 0x39: // ADD IX, SP
									ClockCycles = 15;
									TI1 = (short)RegIX.Value16; TI2 = (short)RegSP.Value16; TIR = TI1 + TI2;
									TUS = (ushort)TIR;
									RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
									RegFlagN = false;
									RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
									RegIX.Value16 = TUS;
									break;
								case 0x3A: // LD A, (nn)
									ClockCycles = 13;
									RegAF.High8 = ReadMemory((ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256));
									break;
								case 0x3B: // DEC SP
									ClockCycles = 6;
									--RegSP.Value16;
									break;
								case 0x3C: // INC A
									ClockCycles = 4;
									RegAF.Low8 = (byte)(TableInc[++RegAF.High8] | (RegAF.Low8 & 1));
									break;
								case 0x3D: // DEC A
									ClockCycles = 4;
									RegAF.Low8 = (byte)(TableDec[--RegAF.High8] | (RegAF.Low8 & 1));
									break;
								case 0x3E: // LD A, n
									ClockCycles = 7;
									RegAF.High8 = ReadMemory(RegPC.Value16++);
									break;
								case 0x3F: // CCF
									ClockCycles = 4;
									RegFlagH = RegFlagC; RegFlagN = false; RegFlagC ^= true;
									break;
								case 0x40: // LD B, B
									ClockCycles = 4;
									break;
								case 0x41: // LD B, C
									ClockCycles = 4;
									RegBC.High8 = RegBC.Low8;
									break;
								case 0x42: // LD B, D
									ClockCycles = 4;
									RegBC.High8 = RegDE.High8;
									break;
								case 0x43: // LD B, E
									ClockCycles = 4;
									RegBC.High8 = RegDE.Low8;
									break;
								case 0x44: // LD B, IXH
									ClockCycles = 9;
									RegBC.High8 = RegIX.High8;
									break;
								case 0x45: // LD B, IXL
									ClockCycles = 9;
									RegBC.High8 = RegIX.Low8;
									break;
								case 0x46: // LD B, (IX+d)
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegBC.High8 = ReadMemory((ushort)(RegIX.Value16 + Displacement));
									break;
								case 0x47: // LD B, A
									ClockCycles = 4;
									RegBC.High8 = RegAF.High8;
									break;
								case 0x48: // LD C, B
									ClockCycles = 4;
									RegBC.Low8 = RegBC.High8;
									break;
								case 0x49: // LD C, C
									ClockCycles = 4;
									break;
								case 0x4A: // LD C, D
									ClockCycles = 4;
									RegBC.Low8 = RegDE.High8;
									break;
								case 0x4B: // LD C, E
									ClockCycles = 4;
									RegBC.Low8 = RegDE.Low8;
									break;
								case 0x4C: // LD C, IXH
									ClockCycles = 9;
									RegBC.Low8 = RegIX.High8;
									break;
								case 0x4D: // LD C, IXL
									ClockCycles = 9;
									RegBC.Low8 = RegIX.Low8;
									break;
								case 0x4E: // LD C, (IX+d)
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegBC.Low8 = ReadMemory((ushort)(RegIX.Value16 + Displacement));
									break;
								case 0x4F: // LD C, A
									ClockCycles = 4;
									RegBC.Low8 = RegAF.High8;
									break;
								case 0x50: // LD D, B
									ClockCycles = 4;
									RegDE.High8 = RegBC.High8;
									break;
								case 0x51: // LD D, C
									ClockCycles = 4;
									RegDE.High8 = RegBC.Low8;
									break;
								case 0x52: // LD D, D
									ClockCycles = 4;
									break;
								case 0x53: // LD D, E
									ClockCycles = 4;
									RegDE.High8 = RegDE.Low8;
									break;
								case 0x54: // LD D, IXH
									ClockCycles = 9;
									RegDE.High8 = RegIX.High8;
									break;
								case 0x55: // LD D, IXL
									ClockCycles = 9;
									RegDE.High8 = RegIX.Low8;
									break;
								case 0x56: // LD D, (IX+d)
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegDE.High8 = ReadMemory((ushort)(RegIX.Value16 + Displacement));
									break;
								case 0x57: // LD D, A
									ClockCycles = 4;
									RegDE.High8 = RegAF.High8;
									break;
								case 0x58: // LD E, B
									ClockCycles = 4;
									RegDE.Low8 = RegBC.High8;
									break;
								case 0x59: // LD E, C
									ClockCycles = 4;
									RegDE.Low8 = RegBC.Low8;
									break;
								case 0x5A: // LD E, D
									ClockCycles = 4;
									RegDE.Low8 = RegDE.High8;
									break;
								case 0x5B: // LD E, E
									ClockCycles = 4;
									break;
								case 0x5C: // LD E, IXH
									ClockCycles = 9;
									RegDE.Low8 = RegIX.High8;
									break;
								case 0x5D: // LD E, IXL
									ClockCycles = 9;
									RegDE.Low8 = RegIX.Low8;
									break;
								case 0x5E: // LD E, (IX+d)
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegDE.Low8 = ReadMemory((ushort)(RegIX.Value16 + Displacement));
									break;
								case 0x5F: // LD E, A
									ClockCycles = 4;
									RegDE.Low8 = RegAF.High8;
									break;
								case 0x60: // LD IXH, B
									ClockCycles = 9;
									RegIX.High8 = RegBC.High8;
									break;
								case 0x61: // LD IXH, C
									ClockCycles = 9;
									RegIX.High8 = RegBC.Low8;
									break;
								case 0x62: // LD IXH, D
									ClockCycles = 9;
									RegIX.High8 = RegDE.High8;
									break;
								case 0x63: // LD IXH, E
									ClockCycles = 9;
									RegIX.High8 = RegDE.Low8;
									break;
								case 0x64: // LD IXH, IXH
									ClockCycles = 9;
									break;
								case 0x65: // LD IXH, IXL
									ClockCycles = 9;
									RegIX.High8 = RegIX.Low8;
									break;
								case 0x66: // LD H, (IX+d)
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegHL.High8 = ReadMemory((ushort)(RegIX.Value16 + Displacement));
									break;
								case 0x67: // LD IXH, A
									ClockCycles = 9;
									RegIX.High8 = RegAF.High8;
									break;
								case 0x68: // LD IXL, B
									ClockCycles = 9;
									RegIX.Low8 = RegBC.High8;
									break;
								case 0x69: // LD IXL, C
									ClockCycles = 9;
									RegIX.Low8 = RegBC.Low8;
									break;
								case 0x6A: // LD IXL, D
									ClockCycles = 9;
									RegIX.Low8 = RegDE.High8;
									break;
								case 0x6B: // LD IXL, E
									ClockCycles = 9;
									RegIX.Low8 = RegDE.Low8;
									break;
								case 0x6C: // LD IXL, IXH
									ClockCycles = 9;
									RegIX.Low8 = RegIX.High8;
									break;
								case 0x6D: // LD IXL, IXL
									ClockCycles = 9;
									break;
								case 0x6E: // LD L, (IX+d)
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegHL.Low8 = ReadMemory((ushort)(RegIX.Value16 + Displacement));
									break;
								case 0x6F: // LD IXL, A
									ClockCycles = 9;
									RegIX.Low8 = RegAF.High8;
									break;
								case 0x70: // LD (IX+d), B
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
									break;
								case 0x71: // LD (IX+d), C
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
									break;
								case 0x72: // LD (IX+d), D
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
									break;
								case 0x73: // LD (IX+d), E
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
									break;
								case 0x74: // LD (IX+d), H
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
									break;
								case 0x75: // LD (IX+d), L
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
									break;
								case 0x76: // HALT
									ClockCycles = 4;
									this.Halt();
									break;
								case 0x77: // LD (IX+d), A
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
									break;
								case 0x78: // LD A, B
									ClockCycles = 4;
									RegAF.High8 = RegBC.High8;
									break;
								case 0x79: // LD A, C
									ClockCycles = 4;
									RegAF.High8 = RegBC.Low8;
									break;
								case 0x7A: // LD A, D
									ClockCycles = 4;
									RegAF.High8 = RegDE.High8;
									break;
								case 0x7B: // LD A, E
									ClockCycles = 4;
									RegAF.High8 = RegDE.Low8;
									break;
								case 0x7C: // LD A, IXH
									ClockCycles = 9;
									RegAF.High8 = RegIX.High8;
									break;
								case 0x7D: // LD A, IXL
									ClockCycles = 9;
									RegAF.High8 = RegIX.Low8;
									break;
								case 0x7E: // LD A, (IX+d)
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.High8 = ReadMemory((ushort)(RegIX.Value16 + Displacement));
									break;
								case 0x7F: // LD A, A
									ClockCycles = 4;
									break;
								case 0x80: // ADD A, B
									ClockCycles = 4;
									RegAF.Value16 = TableALU[0, RegAF.High8, RegBC.High8, 0];
									break;
								case 0x81: // ADD A, C
									ClockCycles = 4;
									RegAF.Value16 = TableALU[0, RegAF.High8, RegBC.Low8, 0];
									break;
								case 0x82: // ADD A, D
									ClockCycles = 4;
									RegAF.Value16 = TableALU[0, RegAF.High8, RegDE.High8, 0];
									break;
								case 0x83: // ADD A, E
									ClockCycles = 4;
									RegAF.Value16 = TableALU[0, RegAF.High8, RegDE.Low8, 0];
									break;
								case 0x84: // ADD A, IXH
									ClockCycles = 9;
									RegAF.Value16 = TableALU[0, RegAF.High8, RegIX.High8, 0];
									break;
								case 0x85: // ADD A, IXL
									ClockCycles = 9;
									RegAF.Value16 = TableALU[0, RegAF.High8, RegIX.Low8, 0];
									break;
								case 0x86: // ADD A, (IX+d)
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[0, RegAF.High8, ReadMemory((ushort)(RegIX.Value16 + Displacement)), 0];
									break;
								case 0x87: // ADD A, A
									ClockCycles = 4;
									RegAF.Value16 = TableALU[0, RegAF.High8, RegAF.High8, 0];
									break;
								case 0x88: // ADC A, B
									ClockCycles = 4;
									RegAF.Value16 = TableALU[1, RegAF.High8, RegBC.High8, RegFlagC ? 1 : 0];
									break;
								case 0x89: // ADC A, C
									ClockCycles = 4;
									RegAF.Value16 = TableALU[1, RegAF.High8, RegBC.Low8, RegFlagC ? 1 : 0];
									break;
								case 0x8A: // ADC A, D
									ClockCycles = 4;
									RegAF.Value16 = TableALU[1, RegAF.High8, RegDE.High8, RegFlagC ? 1 : 0];
									break;
								case 0x8B: // ADC A, E
									ClockCycles = 4;
									RegAF.Value16 = TableALU[1, RegAF.High8, RegDE.Low8, RegFlagC ? 1 : 0];
									break;
								case 0x8C: // ADC A, IXH
									ClockCycles = 9;
									RegAF.Value16 = TableALU[1, RegAF.High8, RegIX.High8, RegFlagC ? 1 : 0];
									break;
								case 0x8D: // ADC A, IXL
									ClockCycles = 9;
									RegAF.Value16 = TableALU[1, RegAF.High8, RegIX.Low8, RegFlagC ? 1 : 0];
									break;
								case 0x8E: // ADC A, (IX+d)
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[1, RegAF.High8, ReadMemory((ushort)(RegIX.Value16 + Displacement)), RegFlagC ? 1 : 0];
									break;
								case 0x8F: // ADC A, A
									ClockCycles = 4;
									RegAF.Value16 = TableALU[1, RegAF.High8, RegAF.High8, RegFlagC ? 1 : 0];
									break;
								case 0x90: // SUB B
									ClockCycles = 4;
									RegAF.Value16 = TableALU[2, RegAF.High8, RegBC.High8, 0];
									break;
								case 0x91: // SUB C
									ClockCycles = 4;
									RegAF.Value16 = TableALU[2, RegAF.High8, RegBC.Low8, 0];
									break;
								case 0x92: // SUB D
									ClockCycles = 4;
									RegAF.Value16 = TableALU[2, RegAF.High8, RegDE.High8, 0];
									break;
								case 0x93: // SUB E
									ClockCycles = 4;
									RegAF.Value16 = TableALU[2, RegAF.High8, RegDE.Low8, 0];
									break;
								case 0x94: // SUB IXH
									ClockCycles = 9;
									RegAF.Value16 = TableALU[2, RegAF.High8, RegIX.High8, 0];
									break;
								case 0x95: // SUB IXL
									ClockCycles = 9;
									RegAF.Value16 = TableALU[2, RegAF.High8, RegIX.Low8, 0];
									break;
								case 0x96: // SUB (IX+d)
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[2, RegAF.High8, ReadMemory((ushort)(RegIX.Value16 + Displacement)), 0];
									break;
								case 0x97: // SUB A, A
									ClockCycles = 4;
									RegAF.Value16 = TableALU[2, RegAF.High8, RegAF.High8, 0];
									break;
								case 0x98: // SBC A, B
									ClockCycles = 4;
									RegAF.Value16 = TableALU[3, RegAF.High8, RegBC.High8, RegFlagC ? 1 : 0];
									break;
								case 0x99: // SBC A, C
									ClockCycles = 4;
									RegAF.Value16 = TableALU[3, RegAF.High8, RegBC.Low8, RegFlagC ? 1 : 0];
									break;
								case 0x9A: // SBC A, D
									ClockCycles = 4;
									RegAF.Value16 = TableALU[3, RegAF.High8, RegDE.High8, RegFlagC ? 1 : 0];
									break;
								case 0x9B: // SBC A, E
									ClockCycles = 4;
									RegAF.Value16 = TableALU[3, RegAF.High8, RegDE.Low8, RegFlagC ? 1 : 0];
									break;
								case 0x9C: // SBC A, IXH
									ClockCycles = 9;
									RegAF.Value16 = TableALU[3, RegAF.High8, RegIX.High8, RegFlagC ? 1 : 0];
									break;
								case 0x9D: // SBC A, IXL
									ClockCycles = 9;
									RegAF.Value16 = TableALU[3, RegAF.High8, RegIX.Low8, RegFlagC ? 1 : 0];
									break;
								case 0x9E: // SBC A, (IX+d)
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[3, RegAF.High8, ReadMemory((ushort)(RegIX.Value16 + Displacement)), RegFlagC ? 1 : 0];
									break;
								case 0x9F: // SBC A, A
									ClockCycles = 4;
									RegAF.Value16 = TableALU[3, RegAF.High8, RegAF.High8, RegFlagC ? 1 : 0];
									break;
								case 0xA0: // AND B
									ClockCycles = 4;
									RegAF.Value16 = TableALU[4, RegAF.High8, RegBC.High8, 0];
									break;
								case 0xA1: // AND C
									ClockCycles = 4;
									RegAF.Value16 = TableALU[4, RegAF.High8, RegBC.Low8, 0];
									break;
								case 0xA2: // AND D
									ClockCycles = 4;
									RegAF.Value16 = TableALU[4, RegAF.High8, RegDE.High8, 0];
									break;
								case 0xA3: // AND E
									ClockCycles = 4;
									RegAF.Value16 = TableALU[4, RegAF.High8, RegDE.Low8, 0];
									break;
								case 0xA4: // AND IXH
									ClockCycles = 9;
									RegAF.Value16 = TableALU[4, RegAF.High8, RegIX.High8, 0];
									break;
								case 0xA5: // AND IXL
									ClockCycles = 9;
									RegAF.Value16 = TableALU[4, RegAF.High8, RegIX.Low8, 0];
									break;
								case 0xA6: // AND (IX+d)
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[4, RegAF.High8, ReadMemory((ushort)(RegIX.Value16 + Displacement)), 0];
									break;
								case 0xA7: // AND A
									ClockCycles = 4;
									RegAF.Value16 = TableALU[4, RegAF.High8, RegAF.High8, 0];
									break;
								case 0xA8: // XOR B
									ClockCycles = 4;
									RegAF.Value16 = TableALU[5, RegAF.High8, RegBC.High8, 0];
									break;
								case 0xA9: // XOR C
									ClockCycles = 4;
									RegAF.Value16 = TableALU[5, RegAF.High8, RegBC.Low8, 0];
									break;
								case 0xAA: // XOR D
									ClockCycles = 4;
									RegAF.Value16 = TableALU[5, RegAF.High8, RegDE.High8, 0];
									break;
								case 0xAB: // XOR E
									ClockCycles = 4;
									RegAF.Value16 = TableALU[5, RegAF.High8, RegDE.Low8, 0];
									break;
								case 0xAC: // XOR IXH
									ClockCycles = 9;
									RegAF.Value16 = TableALU[5, RegAF.High8, RegIX.High8, 0];
									break;
								case 0xAD: // XOR IXL
									ClockCycles = 9;
									RegAF.Value16 = TableALU[5, RegAF.High8, RegIX.Low8, 0];
									break;
								case 0xAE: // XOR (IX+d)
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[5, RegAF.High8, ReadMemory((ushort)(RegIX.Value16 + Displacement)), 0];
									break;
								case 0xAF: // XOR A
									ClockCycles = 4;
									RegAF.Value16 = TableALU[5, RegAF.High8, RegAF.High8, 0];
									break;
								case 0xB0: // OR B
									ClockCycles = 4;
									RegAF.Value16 = TableALU[6, RegAF.High8, RegBC.High8, 0];
									break;
								case 0xB1: // OR C
									ClockCycles = 4;
									RegAF.Value16 = TableALU[6, RegAF.High8, RegBC.Low8, 0];
									break;
								case 0xB2: // OR D
									ClockCycles = 4;
									RegAF.Value16 = TableALU[6, RegAF.High8, RegDE.High8, 0];
									break;
								case 0xB3: // OR E
									ClockCycles = 4;
									RegAF.Value16 = TableALU[6, RegAF.High8, RegDE.Low8, 0];
									break;
								case 0xB4: // OR IXH
									ClockCycles = 9;
									RegAF.Value16 = TableALU[6, RegAF.High8, RegIX.High8, 0];
									break;
								case 0xB5: // OR IXL
									ClockCycles = 9;
									RegAF.Value16 = TableALU[6, RegAF.High8, RegIX.Low8, 0];
									break;
								case 0xB6: // OR (IX+d)
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[6, RegAF.High8, ReadMemory((ushort)(RegIX.Value16 + Displacement)), 0];
									break;
								case 0xB7: // OR A
									ClockCycles = 4;
									RegAF.Value16 = TableALU[6, RegAF.High8, RegAF.High8, 0];
									break;
								case 0xB8: // CP B
									ClockCycles = 4;
									RegAF.Value16 = TableALU[7, RegAF.High8, RegBC.High8, 0];
									break;
								case 0xB9: // CP C
									ClockCycles = 4;
									RegAF.Value16 = TableALU[7, RegAF.High8, RegBC.Low8, 0];
									break;
								case 0xBA: // CP D
									ClockCycles = 4;
									RegAF.Value16 = TableALU[7, RegAF.High8, RegDE.High8, 0];
									break;
								case 0xBB: // CP E
									ClockCycles = 4;
									RegAF.Value16 = TableALU[7, RegAF.High8, RegDE.Low8, 0];
									break;
								case 0xBC: // CP IXH
									ClockCycles = 9;
									RegAF.Value16 = TableALU[7, RegAF.High8, RegIX.High8, 0];
									break;
								case 0xBD: // CP IXL
									ClockCycles = 9;
									RegAF.Value16 = TableALU[7, RegAF.High8, RegIX.Low8, 0];
									break;
								case 0xBE: // CP (IX+d)
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[7, RegAF.High8, ReadMemory((ushort)(RegIX.Value16 + Displacement)), 0];
									break;
								case 0xBF: // CP A
									ClockCycles = 4;
									RegAF.Value16 = TableALU[7, RegAF.High8, RegAF.High8, 0];
									break;
								case 0xC0: // RET NZ
									ClockCycles = 11;
									if (!RegFlagZ) {
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									} else {
										ClockCycles = 5;
									}
									break;
								case 0xC1: // POP BC
									ClockCycles = 10;
									RegBC.Low8 = ReadMemory(RegSP.Value16++); RegBC.High8 = ReadMemory(RegSP.Value16++);
									break;
								case 0xC2: // JP NZ, nn
									ClockCycles = 10;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagZ) {
										RegPC.Value16 = TUS;
									} else {
										ClockCycles = 10;
									}
									break;
								case 0xC3: // JP nn
									ClockCycles = 10;
									RegPC.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									break;
								case 0xC4: // CALL NZ, nn
									ClockCycles = 17;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagZ) {
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
									} else {
										ClockCycles = 10;
									}
									break;
								case 0xC5: // PUSH BC
									ClockCycles = 11;
									WriteMemory(--RegSP.Value16, RegBC.High8); WriteMemory(--RegSP.Value16, RegBC.Low8);
									break;
								case 0xC6: // ADD A, n
									ClockCycles = 7;
									RegAF.Value16 = TableALU[0, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
									break;
								case 0xC7: // RST $00
									ClockCycles = 11;
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x00;
									break;
								case 0xC8: // RET Z
									ClockCycles = 11;
									if (RegFlagZ) {
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									} else {
										ClockCycles = 5;
									}
									break;
								case 0xC9: // RET
									ClockCycles = 10;
									RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									break;
								case 0xCA: // JP Z, nn
									ClockCycles = 10;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagZ) {
										RegPC.Value16 = TUS;
									} else {
										ClockCycles = 10;
									}
									break;
								case 0xCB: // (Prefix)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									++RegR;
									switch (ReadMemory(RegPC.Value16++)) {
										case 0x00: // RLC (IX+d)→B
											ClockCycles = 23;
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.High8 = (byte)TUS;
											break;
										case 0x01: // RLC (IX+d)→C
											ClockCycles = 23;
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.Low8 = (byte)TUS;
											break;
										case 0x02: // RLC (IX+d)→D
											ClockCycles = 23;
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.High8 = (byte)TUS;
											break;
										case 0x03: // RLC (IX+d)→E
											ClockCycles = 23;
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.Low8 = (byte)TUS;
											break;
										case 0x04: // RLC (IX+d)→H
											ClockCycles = 23;
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.High8 = (byte)TUS;
											break;
										case 0x05: // RLC (IX+d)→L
											ClockCycles = 23;
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.Low8 = (byte)TUS;
											break;
										case 0x06: // RLC (IX+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x07: // RLC (IX+d)→A
											ClockCycles = 23;
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegAF.High8 = (byte)TUS;
											break;
										case 0x08: // RRC (IX+d)→B
											ClockCycles = 23;
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.High8 = (byte)TUS;
											break;
										case 0x09: // RRC (IX+d)→C
											ClockCycles = 23;
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.Low8 = (byte)TUS;
											break;
										case 0x0A: // RRC (IX+d)→D
											ClockCycles = 23;
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.High8 = (byte)TUS;
											break;
										case 0x0B: // RRC (IX+d)→E
											ClockCycles = 23;
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.Low8 = (byte)TUS;
											break;
										case 0x0C: // RRC (IX+d)→H
											ClockCycles = 23;
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.High8 = (byte)TUS;
											break;
										case 0x0D: // RRC (IX+d)→L
											ClockCycles = 23;
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.Low8 = (byte)TUS;
											break;
										case 0x0E: // RRC (IX+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x0F: // RRC (IX+d)→A
											ClockCycles = 23;
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegAF.High8 = (byte)TUS;
											break;
										case 0x10: // RL (IX+d)→B
											ClockCycles = 23;
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.High8 = (byte)TUS;
											break;
										case 0x11: // RL (IX+d)→C
											ClockCycles = 23;
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.Low8 = (byte)TUS;
											break;
										case 0x12: // RL (IX+d)→D
											ClockCycles = 23;
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.High8 = (byte)TUS;
											break;
										case 0x13: // RL (IX+d)→E
											ClockCycles = 23;
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.Low8 = (byte)TUS;
											break;
										case 0x14: // RL (IX+d)→H
											ClockCycles = 23;
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.High8 = (byte)TUS;
											break;
										case 0x15: // RL (IX+d)→L
											ClockCycles = 23;
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.Low8 = (byte)TUS;
											break;
										case 0x16: // RL (IX+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x17: // RL (IX+d)→A
											ClockCycles = 23;
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegAF.High8 = (byte)TUS;
											break;
										case 0x18: // RR (IX+d)→B
											ClockCycles = 23;
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.High8 = (byte)TUS;
											break;
										case 0x19: // RR (IX+d)→C
											ClockCycles = 23;
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.Low8 = (byte)TUS;
											break;
										case 0x1A: // RR (IX+d)→D
											ClockCycles = 23;
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.High8 = (byte)TUS;
											break;
										case 0x1B: // RR (IX+d)→E
											ClockCycles = 23;
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.Low8 = (byte)TUS;
											break;
										case 0x1C: // RR (IX+d)→H
											ClockCycles = 23;
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.High8 = (byte)TUS;
											break;
										case 0x1D: // RR (IX+d)→L
											ClockCycles = 23;
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.Low8 = (byte)TUS;
											break;
										case 0x1E: // RR (IX+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x1F: // RR (IX+d)→A
											ClockCycles = 23;
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegAF.High8 = (byte)TUS;
											break;
										case 0x20: // SLA (IX+d)→B
											ClockCycles = 23;
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.High8 = (byte)TUS;
											break;
										case 0x21: // SLA (IX+d)→C
											ClockCycles = 23;
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.Low8 = (byte)TUS;
											break;
										case 0x22: // SLA (IX+d)→D
											ClockCycles = 23;
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.High8 = (byte)TUS;
											break;
										case 0x23: // SLA (IX+d)→E
											ClockCycles = 23;
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.Low8 = (byte)TUS;
											break;
										case 0x24: // SLA (IX+d)→H
											ClockCycles = 23;
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.High8 = (byte)TUS;
											break;
										case 0x25: // SLA (IX+d)→L
											ClockCycles = 23;
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.Low8 = (byte)TUS;
											break;
										case 0x26: // SLA (IX+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x27: // SLA (IX+d)→A
											ClockCycles = 23;
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegAF.High8 = (byte)TUS;
											break;
										case 0x28: // SRA (IX+d)→B
											ClockCycles = 23;
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.High8 = (byte)TUS;
											break;
										case 0x29: // SRA (IX+d)→C
											ClockCycles = 23;
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.Low8 = (byte)TUS;
											break;
										case 0x2A: // SRA (IX+d)→D
											ClockCycles = 23;
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.High8 = (byte)TUS;
											break;
										case 0x2B: // SRA (IX+d)→E
											ClockCycles = 23;
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.Low8 = (byte)TUS;
											break;
										case 0x2C: // SRA (IX+d)→H
											ClockCycles = 23;
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.High8 = (byte)TUS;
											break;
										case 0x2D: // SRA (IX+d)→L
											ClockCycles = 23;
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.Low8 = (byte)TUS;
											break;
										case 0x2E: // SRA (IX+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x2F: // SRA (IX+d)→A
											ClockCycles = 23;
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegAF.High8 = (byte)TUS;
											break;
										case 0x30: // SL1 (IX+d)→B
											ClockCycles = 23;
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.High8 = (byte)TUS;
											break;
										case 0x31: // SL1 (IX+d)→C
											ClockCycles = 23;
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.Low8 = (byte)TUS;
											break;
										case 0x32: // SL1 (IX+d)→D
											ClockCycles = 23;
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.High8 = (byte)TUS;
											break;
										case 0x33: // SL1 (IX+d)→E
											ClockCycles = 23;
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.Low8 = (byte)TUS;
											break;
										case 0x34: // SL1 (IX+d)→H
											ClockCycles = 23;
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.High8 = (byte)TUS;
											break;
										case 0x35: // SL1 (IX+d)→L
											ClockCycles = 23;
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.Low8 = (byte)TUS;
											break;
										case 0x36: // SL1 (IX+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x37: // SL1 (IX+d)→A
											ClockCycles = 23;
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegAF.High8 = (byte)TUS;
											break;
										case 0x38: // SRL (IX+d)→B
											ClockCycles = 23;
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.High8 = (byte)TUS;
											break;
										case 0x39: // SRL (IX+d)→C
											ClockCycles = 23;
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegBC.Low8 = (byte)TUS;
											break;
										case 0x3A: // SRL (IX+d)→D
											ClockCycles = 23;
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.High8 = (byte)TUS;
											break;
										case 0x3B: // SRL (IX+d)→E
											ClockCycles = 23;
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegDE.Low8 = (byte)TUS;
											break;
										case 0x3C: // SRL (IX+d)→H
											ClockCycles = 23;
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.High8 = (byte)TUS;
											break;
										case 0x3D: // SRL (IX+d)→L
											ClockCycles = 23;
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegHL.Low8 = (byte)TUS;
											break;
										case 0x3E: // SRL (IX+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x3F: // SRL (IX+d)→A
											ClockCycles = 23;
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIX.Value16 + Displacement))];
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											RegAF.High8 = (byte)TUS;
											break;
										case 0x40: // BIT 0, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x41: // BIT 0, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x42: // BIT 0, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x43: // BIT 0, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x44: // BIT 0, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x45: // BIT 0, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x46: // BIT 0, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x47: // BIT 0, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x48: // BIT 1, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x49: // BIT 1, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x4A: // BIT 1, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x4B: // BIT 1, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x4C: // BIT 1, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x4D: // BIT 1, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x4E: // BIT 1, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x4F: // BIT 1, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x50: // BIT 2, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x51: // BIT 2, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x52: // BIT 2, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x53: // BIT 2, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x54: // BIT 2, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x55: // BIT 2, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x56: // BIT 2, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x57: // BIT 2, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x58: // BIT 3, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x59: // BIT 3, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x5A: // BIT 3, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x5B: // BIT 3, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x5C: // BIT 3, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x5D: // BIT 3, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x5E: // BIT 3, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x5F: // BIT 3, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x60: // BIT 4, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x61: // BIT 4, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x62: // BIT 4, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x63: // BIT 4, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x64: // BIT 4, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x65: // BIT 4, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x66: // BIT 4, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x67: // BIT 4, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x68: // BIT 5, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x69: // BIT 5, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x6A: // BIT 5, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x6B: // BIT 5, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x6C: // BIT 5, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x6D: // BIT 5, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x6E: // BIT 5, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x6F: // BIT 5, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x70: // BIT 6, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x71: // BIT 6, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x72: // BIT 6, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x73: // BIT 6, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x74: // BIT 6, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x75: // BIT 6, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x76: // BIT 6, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x77: // BIT 6, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x78: // BIT 7, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x79: // BIT 7, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x7A: // BIT 7, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x7B: // BIT 7, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x7C: // BIT 7, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x7D: // BIT 7, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x7E: // BIT 7, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x7F: // BIT 7, (IX+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIX.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x80: // RES 0, (IX+d)→B
											ClockCycles = 23;
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											break;
										case 0x81: // RES 0, (IX+d)→C
											ClockCycles = 23;
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											break;
										case 0x82: // RES 0, (IX+d)→D
											ClockCycles = 23;
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											break;
										case 0x83: // RES 0, (IX+d)→E
											ClockCycles = 23;
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											break;
										case 0x84: // RES 0, (IX+d)→H
											ClockCycles = 23;
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											break;
										case 0x85: // RES 0, (IX+d)→L
											ClockCycles = 23;
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											break;
										case 0x86: // RES 0, (IX+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x01)));
											break;
										case 0x87: // RES 0, (IX+d)→A
											ClockCycles = 23;
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											break;
										case 0x88: // RES 1, (IX+d)→B
											ClockCycles = 23;
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											break;
										case 0x89: // RES 1, (IX+d)→C
											ClockCycles = 23;
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											break;
										case 0x8A: // RES 1, (IX+d)→D
											ClockCycles = 23;
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											break;
										case 0x8B: // RES 1, (IX+d)→E
											ClockCycles = 23;
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											break;
										case 0x8C: // RES 1, (IX+d)→H
											ClockCycles = 23;
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											break;
										case 0x8D: // RES 1, (IX+d)→L
											ClockCycles = 23;
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											break;
										case 0x8E: // RES 1, (IX+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x02)));
											break;
										case 0x8F: // RES 1, (IX+d)→A
											ClockCycles = 23;
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											break;
										case 0x90: // RES 2, (IX+d)→B
											ClockCycles = 23;
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											break;
										case 0x91: // RES 2, (IX+d)→C
											ClockCycles = 23;
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											break;
										case 0x92: // RES 2, (IX+d)→D
											ClockCycles = 23;
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											break;
										case 0x93: // RES 2, (IX+d)→E
											ClockCycles = 23;
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											break;
										case 0x94: // RES 2, (IX+d)→H
											ClockCycles = 23;
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											break;
										case 0x95: // RES 2, (IX+d)→L
											ClockCycles = 23;
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											break;
										case 0x96: // RES 2, (IX+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x04)));
											break;
										case 0x97: // RES 2, (IX+d)→A
											ClockCycles = 23;
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											break;
										case 0x98: // RES 3, (IX+d)→B
											ClockCycles = 23;
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											break;
										case 0x99: // RES 3, (IX+d)→C
											ClockCycles = 23;
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											break;
										case 0x9A: // RES 3, (IX+d)→D
											ClockCycles = 23;
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											break;
										case 0x9B: // RES 3, (IX+d)→E
											ClockCycles = 23;
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											break;
										case 0x9C: // RES 3, (IX+d)→H
											ClockCycles = 23;
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											break;
										case 0x9D: // RES 3, (IX+d)→L
											ClockCycles = 23;
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											break;
										case 0x9E: // RES 3, (IX+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x08)));
											break;
										case 0x9F: // RES 3, (IX+d)→A
											ClockCycles = 23;
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											break;
										case 0xA0: // RES 4, (IX+d)→B
											ClockCycles = 23;
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											break;
										case 0xA1: // RES 4, (IX+d)→C
											ClockCycles = 23;
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											break;
										case 0xA2: // RES 4, (IX+d)→D
											ClockCycles = 23;
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											break;
										case 0xA3: // RES 4, (IX+d)→E
											ClockCycles = 23;
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											break;
										case 0xA4: // RES 4, (IX+d)→H
											ClockCycles = 23;
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											break;
										case 0xA5: // RES 4, (IX+d)→L
											ClockCycles = 23;
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											break;
										case 0xA6: // RES 4, (IX+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x10)));
											break;
										case 0xA7: // RES 4, (IX+d)→A
											ClockCycles = 23;
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											break;
										case 0xA8: // RES 5, (IX+d)→B
											ClockCycles = 23;
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											break;
										case 0xA9: // RES 5, (IX+d)→C
											ClockCycles = 23;
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											break;
										case 0xAA: // RES 5, (IX+d)→D
											ClockCycles = 23;
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											break;
										case 0xAB: // RES 5, (IX+d)→E
											ClockCycles = 23;
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											break;
										case 0xAC: // RES 5, (IX+d)→H
											ClockCycles = 23;
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											break;
										case 0xAD: // RES 5, (IX+d)→L
											ClockCycles = 23;
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											break;
										case 0xAE: // RES 5, (IX+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x20)));
											break;
										case 0xAF: // RES 5, (IX+d)→A
											ClockCycles = 23;
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											break;
										case 0xB0: // RES 6, (IX+d)→B
											ClockCycles = 23;
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											break;
										case 0xB1: // RES 6, (IX+d)→C
											ClockCycles = 23;
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											break;
										case 0xB2: // RES 6, (IX+d)→D
											ClockCycles = 23;
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											break;
										case 0xB3: // RES 6, (IX+d)→E
											ClockCycles = 23;
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											break;
										case 0xB4: // RES 6, (IX+d)→H
											ClockCycles = 23;
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											break;
										case 0xB5: // RES 6, (IX+d)→L
											ClockCycles = 23;
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											break;
										case 0xB6: // RES 6, (IX+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x40)));
											break;
										case 0xB7: // RES 6, (IX+d)→A
											ClockCycles = 23;
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											break;
										case 0xB8: // RES 7, (IX+d)→B
											ClockCycles = 23;
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											break;
										case 0xB9: // RES 7, (IX+d)→C
											ClockCycles = 23;
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											break;
										case 0xBA: // RES 7, (IX+d)→D
											ClockCycles = 23;
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											break;
										case 0xBB: // RES 7, (IX+d)→E
											ClockCycles = 23;
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											break;
										case 0xBC: // RES 7, (IX+d)→H
											ClockCycles = 23;
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											break;
										case 0xBD: // RES 7, (IX+d)→L
											ClockCycles = 23;
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											break;
										case 0xBE: // RES 7, (IX+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x80)));
											break;
										case 0xBF: // RES 7, (IX+d)→A
											ClockCycles = 23;
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) & unchecked((byte)~0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											break;
										case 0xC0: // SET 0, (IX+d)→B
											ClockCycles = 23;
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											break;
										case 0xC1: // SET 0, (IX+d)→C
											ClockCycles = 23;
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											break;
										case 0xC2: // SET 0, (IX+d)→D
											ClockCycles = 23;
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											break;
										case 0xC3: // SET 0, (IX+d)→E
											ClockCycles = 23;
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											break;
										case 0xC4: // SET 0, (IX+d)→H
											ClockCycles = 23;
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											break;
										case 0xC5: // SET 0, (IX+d)→L
											ClockCycles = 23;
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											break;
										case 0xC6: // SET 0, (IX+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x01)));
											break;
										case 0xC7: // SET 0, (IX+d)→A
											ClockCycles = 23;
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x01));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											break;
										case 0xC8: // SET 1, (IX+d)→B
											ClockCycles = 23;
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											break;
										case 0xC9: // SET 1, (IX+d)→C
											ClockCycles = 23;
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											break;
										case 0xCA: // SET 1, (IX+d)→D
											ClockCycles = 23;
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											break;
										case 0xCB: // SET 1, (IX+d)→E
											ClockCycles = 23;
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											break;
										case 0xCC: // SET 1, (IX+d)→H
											ClockCycles = 23;
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											break;
										case 0xCD: // SET 1, (IX+d)→L
											ClockCycles = 23;
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											break;
										case 0xCE: // SET 1, (IX+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x02)));
											break;
										case 0xCF: // SET 1, (IX+d)→A
											ClockCycles = 23;
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x02));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											break;
										case 0xD0: // SET 2, (IX+d)→B
											ClockCycles = 23;
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											break;
										case 0xD1: // SET 2, (IX+d)→C
											ClockCycles = 23;
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											break;
										case 0xD2: // SET 2, (IX+d)→D
											ClockCycles = 23;
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											break;
										case 0xD3: // SET 2, (IX+d)→E
											ClockCycles = 23;
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											break;
										case 0xD4: // SET 2, (IX+d)→H
											ClockCycles = 23;
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											break;
										case 0xD5: // SET 2, (IX+d)→L
											ClockCycles = 23;
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											break;
										case 0xD6: // SET 2, (IX+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x04)));
											break;
										case 0xD7: // SET 2, (IX+d)→A
											ClockCycles = 23;
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x04));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											break;
										case 0xD8: // SET 3, (IX+d)→B
											ClockCycles = 23;
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											break;
										case 0xD9: // SET 3, (IX+d)→C
											ClockCycles = 23;
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											break;
										case 0xDA: // SET 3, (IX+d)→D
											ClockCycles = 23;
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											break;
										case 0xDB: // SET 3, (IX+d)→E
											ClockCycles = 23;
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											break;
										case 0xDC: // SET 3, (IX+d)→H
											ClockCycles = 23;
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											break;
										case 0xDD: // SET 3, (IX+d)→L
											ClockCycles = 23;
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											break;
										case 0xDE: // SET 3, (IX+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x08)));
											break;
										case 0xDF: // SET 3, (IX+d)→A
											ClockCycles = 23;
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x08));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											break;
										case 0xE0: // SET 4, (IX+d)→B
											ClockCycles = 23;
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											break;
										case 0xE1: // SET 4, (IX+d)→C
											ClockCycles = 23;
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											break;
										case 0xE2: // SET 4, (IX+d)→D
											ClockCycles = 23;
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											break;
										case 0xE3: // SET 4, (IX+d)→E
											ClockCycles = 23;
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											break;
										case 0xE4: // SET 4, (IX+d)→H
											ClockCycles = 23;
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											break;
										case 0xE5: // SET 4, (IX+d)→L
											ClockCycles = 23;
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											break;
										case 0xE6: // SET 4, (IX+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x10)));
											break;
										case 0xE7: // SET 4, (IX+d)→A
											ClockCycles = 23;
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x10));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											break;
										case 0xE8: // SET 5, (IX+d)→B
											ClockCycles = 23;
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											break;
										case 0xE9: // SET 5, (IX+d)→C
											ClockCycles = 23;
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											break;
										case 0xEA: // SET 5, (IX+d)→D
											ClockCycles = 23;
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											break;
										case 0xEB: // SET 5, (IX+d)→E
											ClockCycles = 23;
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											break;
										case 0xEC: // SET 5, (IX+d)→H
											ClockCycles = 23;
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											break;
										case 0xED: // SET 5, (IX+d)→L
											ClockCycles = 23;
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											break;
										case 0xEE: // SET 5, (IX+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x20)));
											break;
										case 0xEF: // SET 5, (IX+d)→A
											ClockCycles = 23;
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x20));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											break;
										case 0xF0: // SET 6, (IX+d)→B
											ClockCycles = 23;
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											break;
										case 0xF1: // SET 6, (IX+d)→C
											ClockCycles = 23;
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											break;
										case 0xF2: // SET 6, (IX+d)→D
											ClockCycles = 23;
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											break;
										case 0xF3: // SET 6, (IX+d)→E
											ClockCycles = 23;
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											break;
										case 0xF4: // SET 6, (IX+d)→H
											ClockCycles = 23;
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											break;
										case 0xF5: // SET 6, (IX+d)→L
											ClockCycles = 23;
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											break;
										case 0xF6: // SET 6, (IX+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x40)));
											break;
										case 0xF7: // SET 6, (IX+d)→A
											ClockCycles = 23;
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x40));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											break;
										case 0xF8: // SET 7, (IX+d)→B
											ClockCycles = 23;
											RegBC.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.High8);
											break;
										case 0xF9: // SET 7, (IX+d)→C
											ClockCycles = 23;
											RegBC.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegBC.Low8);
											break;
										case 0xFA: // SET 7, (IX+d)→D
											ClockCycles = 23;
											RegDE.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.High8);
											break;
										case 0xFB: // SET 7, (IX+d)→E
											ClockCycles = 23;
											RegDE.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegDE.Low8);
											break;
										case 0xFC: // SET 7, (IX+d)→H
											ClockCycles = 23;
											RegHL.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.High8);
											break;
										case 0xFD: // SET 7, (IX+d)→L
											ClockCycles = 23;
											RegHL.Low8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegHL.Low8);
											break;
										case 0xFE: // SET 7, (IX+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIX.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x80)));
											break;
										case 0xFF: // SET 7, (IX+d)→A
											ClockCycles = 23;
											RegAF.High8 = (byte)(ReadMemory((ushort)(RegIX.Value16 + Displacement)) | unchecked((byte)0x80));
											WriteMemory((ushort)(RegIX.Value16 + Displacement), RegAF.High8);
											break;
									}
									break;
								case 0xCC: // CALL Z, nn
									ClockCycles = 17;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagZ) {
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
									} else {
										ClockCycles = 10;
									}
									break;
								case 0xCD: // CALL nn
									ClockCycles = 17;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = TUS;
									break;
								case 0xCE: // ADC A, n
									ClockCycles = 7;
									RegAF.Value16 = TableALU[1, RegAF.High8, ReadMemory(RegPC.Value16++), RegFlagC ? 1 : 0];
									break;
								case 0xCF: // RST $08
									ClockCycles = 11;
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x08;
									break;
								case 0xD0: // RET NC
									ClockCycles = 11;
									if (!RegFlagC) {
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									} else {
										ClockCycles = 5;
									}
									break;
								case 0xD1: // POP DE
									ClockCycles = 10;
									RegDE.Low8 = ReadMemory(RegSP.Value16++); RegDE.High8 = ReadMemory(RegSP.Value16++);
									break;
								case 0xD2: // JP NC, nn
									ClockCycles = 10;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagC) {
										RegPC.Value16 = TUS;
									} else {
										ClockCycles = 10;
									}
									break;
								case 0xD3: // OUT n, A
									ClockCycles = 11;
									WriteHardware(ReadMemory(RegPC.Value16++), RegAF.High8);
									break;
								case 0xD4: // CALL NC, nn
									ClockCycles = 17;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagC) {
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
									} else {
										ClockCycles = 10;
									}
									break;
								case 0xD5: // PUSH DE
									ClockCycles = 11;
									WriteMemory(--RegSP.Value16, RegDE.High8); WriteMemory(--RegSP.Value16, RegDE.Low8);
									break;
								case 0xD6: // SUB n
									ClockCycles = 7;
									RegAF.Value16 = TableALU[2, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
									break;
								case 0xD7: // RST $10
									ClockCycles = 11;
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x10;
									break;
								case 0xD8: // RET C
									ClockCycles = 11;
									if (RegFlagC) {
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									} else {
										ClockCycles = 5;
									}
									break;
								case 0xD9: // EXX
									ClockCycles = 4;
									TUS = RegBC.Value16; RegBC.Value16 = RegAltBC.Value16; RegAltBC.Value16 = TUS;
									TUS = RegDE.Value16; RegDE.Value16 = RegAltDE.Value16; RegAltDE.Value16 = TUS;
									TUS = RegHL.Value16; RegHL.Value16 = RegAltHL.Value16; RegAltHL.Value16 = TUS;
									break;
								case 0xDA: // JP C, nn
									ClockCycles = 10;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagC) {
										RegPC.Value16 = TUS;
									} else {
										ClockCycles = 10;
									}
									break;
								case 0xDB: // IN A, n
									ClockCycles = 11;
									RegAF.High8 = ReadHardware((ushort)ReadMemory(RegPC.Value16++));
									break;
								case 0xDC: // CALL C, nn
									ClockCycles = 17;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagC) {
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
									} else {
										ClockCycles = 10;
									}
									break;
								case 0xDD: // <-
									ClockCycles = 1337;
									// Invalid sequence.
									break;
								case 0xDE: // SBC A, n
									ClockCycles = 7;
									RegAF.Value16 = TableALU[3, RegAF.High8, ReadMemory(RegPC.Value16++), RegFlagC ? 1 : 0];
									break;
								case 0xDF: // RST $18
									ClockCycles = 11;
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x18;
									break;
								case 0xE0: // RET PO
									ClockCycles = 11;
									if (!RegFlagP) {
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									} else {
										ClockCycles = 5;
									}
									break;
								case 0xE1: // POP IX
									ClockCycles = 14;
									RegIX.Low8 = ReadMemory(RegSP.Value16++); RegIX.High8 = ReadMemory(RegSP.Value16++);
									break;
								case 0xE2: // JP PO, nn
									ClockCycles = 10;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagP) {
										RegPC.Value16 = TUS;
									} else {
										ClockCycles = 10;
									}
									break;
								case 0xE3: // EX (SP), IX
									ClockCycles = 23;
									TUS = RegSP.Value16; TBL = ReadMemory(TUS++); TBH = ReadMemory(TUS--);
									WriteMemory(TUS++, RegIX.Low8); WriteMemory(TUS, RegIX.High8);
									RegIX.Low8 = TBL; RegIX.High8 = TBH;
									break;
								case 0xE4: // CALL C, nn
									ClockCycles = 17;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagC) {
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
									} else {
										ClockCycles = 10;
									}
									break;
								case 0xE5: // PUSH IX
									ClockCycles = 15;
									WriteMemory(--RegSP.Value16, RegIX.High8); WriteMemory(--RegSP.Value16, RegIX.Low8);
									break;
								case 0xE6: // AND n
									ClockCycles = 7;
									RegAF.Value16 = TableALU[4, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
									break;
								case 0xE7: // RST $20
									ClockCycles = 11;
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x20;
									break;
								case 0xE8: // RET PE
									ClockCycles = 11;
									if (RegFlagP) {
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									} else {
										ClockCycles = 5;
									}
									break;
								case 0xE9: // JP IX
									ClockCycles = 8;
									RegPC.Value16 = RegIX.Value16;
									break;
								case 0xEA: // JP PE, nn
									ClockCycles = 10;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagP) {
										RegPC.Value16 = TUS;
									} else {
										ClockCycles = 10;
									}
									break;
								case 0xEB: // EX DE, HL
									ClockCycles = 4;
									TUS = RegDE.Value16; RegDE.Value16 = RegHL.Value16; RegHL.Value16 = TUS;
									break;
								case 0xEC: // CALL PE, nn
									ClockCycles = 17;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagP) {
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
									} else {
										ClockCycles = 10;
									}
									break;
								case 0xED: // (Prefix)
									++RegR;
									switch (ReadMemory(RegPC.Value16++)) {
										case 0x00: // NOP
											ClockCycles = 4;
											break;
										case 0x01: // NOP
											ClockCycles = 4;
											break;
										case 0x02: // NOP
											ClockCycles = 4;
											break;
										case 0x03: // NOP
											ClockCycles = 4;
											break;
										case 0x04: // NOP
											ClockCycles = 4;
											break;
										case 0x05: // NOP
											ClockCycles = 4;
											break;
										case 0x06: // NOP
											ClockCycles = 4;
											break;
										case 0x07: // NOP
											ClockCycles = 4;
											break;
										case 0x08: // NOP
											ClockCycles = 4;
											break;
										case 0x09: // NOP
											ClockCycles = 4;
											break;
										case 0x0A: // NOP
											ClockCycles = 4;
											break;
										case 0x0B: // NOP
											ClockCycles = 4;
											break;
										case 0x0C: // NOP
											ClockCycles = 4;
											break;
										case 0x0D: // NOP
											ClockCycles = 4;
											break;
										case 0x0E: // NOP
											ClockCycles = 4;
											break;
										case 0x0F: // NOP
											ClockCycles = 4;
											break;
										case 0x10: // NOP
											ClockCycles = 4;
											break;
										case 0x11: // NOP
											ClockCycles = 4;
											break;
										case 0x12: // NOP
											ClockCycles = 4;
											break;
										case 0x13: // NOP
											ClockCycles = 4;
											break;
										case 0x14: // NOP
											ClockCycles = 4;
											break;
										case 0x15: // NOP
											ClockCycles = 4;
											break;
										case 0x16: // NOP
											ClockCycles = 4;
											break;
										case 0x17: // NOP
											ClockCycles = 4;
											break;
										case 0x18: // NOP
											ClockCycles = 4;
											break;
										case 0x19: // NOP
											ClockCycles = 4;
											break;
										case 0x1A: // NOP
											ClockCycles = 4;
											break;
										case 0x1B: // NOP
											ClockCycles = 4;
											break;
										case 0x1C: // NOP
											ClockCycles = 4;
											break;
										case 0x1D: // NOP
											ClockCycles = 4;
											break;
										case 0x1E: // NOP
											ClockCycles = 4;
											break;
										case 0x1F: // NOP
											ClockCycles = 4;
											break;
										case 0x20: // NOP
											ClockCycles = 4;
											break;
										case 0x21: // NOP
											ClockCycles = 4;
											break;
										case 0x22: // NOP
											ClockCycles = 4;
											break;
										case 0x23: // NOP
											ClockCycles = 4;
											break;
										case 0x24: // NOP
											ClockCycles = 4;
											break;
										case 0x25: // NOP
											ClockCycles = 4;
											break;
										case 0x26: // NOP
											ClockCycles = 4;
											break;
										case 0x27: // NOP
											ClockCycles = 4;
											break;
										case 0x28: // NOP
											ClockCycles = 4;
											break;
										case 0x29: // NOP
											ClockCycles = 4;
											break;
										case 0x2A: // NOP
											ClockCycles = 4;
											break;
										case 0x2B: // NOP
											ClockCycles = 4;
											break;
										case 0x2C: // NOP
											ClockCycles = 4;
											break;
										case 0x2D: // NOP
											ClockCycles = 4;
											break;
										case 0x2E: // NOP
											ClockCycles = 4;
											break;
										case 0x2F: // NOP
											ClockCycles = 4;
											break;
										case 0x30: // NOP
											ClockCycles = 4;
											break;
										case 0x31: // NOP
											ClockCycles = 4;
											break;
										case 0x32: // NOP
											ClockCycles = 4;
											break;
										case 0x33: // NOP
											ClockCycles = 4;
											break;
										case 0x34: // NOP
											ClockCycles = 4;
											break;
										case 0x35: // NOP
											ClockCycles = 4;
											break;
										case 0x36: // NOP
											ClockCycles = 4;
											break;
										case 0x37: // NOP
											ClockCycles = 4;
											break;
										case 0x38: // NOP
											ClockCycles = 4;
											break;
										case 0x39: // NOP
											ClockCycles = 4;
											break;
										case 0x3A: // NOP
											ClockCycles = 4;
											break;
										case 0x3B: // NOP
											ClockCycles = 4;
											break;
										case 0x3C: // NOP
											ClockCycles = 4;
											break;
										case 0x3D: // NOP
											ClockCycles = 4;
											break;
										case 0x3E: // NOP
											ClockCycles = 4;
											break;
										case 0x3F: // NOP
											ClockCycles = 4;
											break;
										case 0x40: // IN B, C
											ClockCycles = 12;
											RegBC.High8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegBC.High8 > 127;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegBC.High8];
											RegFlagN = false;
											break;
										case 0x41: // OUT C, B
											ClockCycles = 12;
											WriteHardware(RegBC.Low8, RegBC.High8);
											break;
										case 0x42: // SBC HL, BC
											ClockCycles = 15;
											TI1 = (short)RegHL.Value16; TI2 = (short)RegBC.Value16; TIR = TI1 - TI2;
											if (RegFlagC) { --TIR; ++TI2; }
											TUS = (ushort)TIR;
											RegFlagH = ((TI1 & 0xFFF) - (TI2 & 0xFFF)) < 0;
											RegFlagN = true;
											RegFlagC = ((ushort)TI1 - (ushort)TI2) < 0;
											RegFlagP = TIR > 32767 || TIR < -32768;
											RegFlagS = TUS > 32767;
											RegFlagZ = TUS == 0;
											RegHL.Value16 = TUS;
											break;
										case 0x43: // LD (nn), BC
											ClockCycles = 20;
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											WriteMemory(TUS++, RegBC.Low8);
											WriteMemory(TUS, RegBC.High8);
											break;
										case 0x44: // NEG
											ClockCycles = 8;
											RegAF.Value16 = TableNeg[RegAF.Value16];
											break;
										case 0x45: // RETN
											ClockCycles = 14;
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											this.IFF1 = this.IFF2;
											break;
										case 0x46: // IM $0
											ClockCycles = 8;
											interruptMode = 0;
											break;
										case 0x47: // LD I, A
											ClockCycles = 9;
											RegI = RegAF.High8;
											break;
										case 0x48: // IN C, C
											ClockCycles = 12;
											RegBC.Low8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegBC.Low8 > 127;
											RegFlagZ = RegBC.Low8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegBC.Low8];
											RegFlagN = false;
											break;
										case 0x49: // OUT C, C
											ClockCycles = 12;
											WriteHardware(RegBC.Low8, RegBC.Low8);
											break;
										case 0x4A: // ADC HL, BC
											ClockCycles = 15;
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
											break;
										case 0x4B: // LD BC, (nn)
											ClockCycles = 20;
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											RegBC.Low8 = ReadMemory(TUS++); RegBC.High8 = ReadMemory(TUS);
											break;
										case 0x4C: // NEG
											ClockCycles = 8;
											RegAF.Value16 = TableNeg[RegAF.Value16];
											break;
										case 0x4D: // RETI
											ClockCycles = 14;
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											break;
										case 0x4E: // IM $0
											ClockCycles = 8;
											interruptMode = 0;
											break;
										case 0x4F: // LD R, A
											ClockCycles = 9;
											RegR = RegAF.High8;
											break;
										case 0x50: // IN D, C
											ClockCycles = 12;
											RegDE.High8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegDE.High8 > 127;
											RegFlagZ = RegDE.High8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegDE.High8];
											RegFlagN = false;
											break;
										case 0x51: // OUT C, D
											ClockCycles = 12;
											WriteHardware(RegBC.Low8, RegDE.High8);
											break;
										case 0x52: // SBC HL, DE
											ClockCycles = 15;
											TI1 = (short)RegHL.Value16; TI2 = (short)RegDE.Value16; TIR = TI1 - TI2;
											if (RegFlagC) { --TIR; ++TI2; }
											TUS = (ushort)TIR;
											RegFlagH = ((TI1 & 0xFFF) - (TI2 & 0xFFF)) < 0;
											RegFlagN = true;
											RegFlagC = ((ushort)TI1 - (ushort)TI2) < 0;
											RegFlagP = TIR > 32767 || TIR < -32768;
											RegFlagS = TUS > 32767;
											RegFlagZ = TUS == 0;
											RegHL.Value16 = TUS;
											break;
										case 0x53: // LD (nn), DE
											ClockCycles = 20;
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											WriteMemory(TUS++, RegDE.Low8);
											WriteMemory(TUS, RegDE.High8);
											break;
										case 0x54: // NEG
											ClockCycles = 8;
											RegAF.Value16 = TableNeg[RegAF.Value16];
											break;
										case 0x55: // RETN
											ClockCycles = 14;
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											this.IFF1 = this.IFF2;
											break;
										case 0x56: // IM $1
											ClockCycles = 8;
											interruptMode = 1;
											break;
										case 0x57: // LD A, I
											ClockCycles = 9;
											RegAF.High8 = RegI;
											break;
										case 0x58: // IN E, C
											ClockCycles = 12;
											RegDE.Low8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegDE.Low8 > 127;
											RegFlagZ = RegDE.Low8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegDE.Low8];
											RegFlagN = false;
											break;
										case 0x59: // OUT C, E
											ClockCycles = 12;
											WriteHardware(RegBC.Low8, RegDE.Low8);
											break;
										case 0x5A: // ADC HL, DE
											ClockCycles = 15;
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
											break;
										case 0x5B: // LD DE, (nn)
											ClockCycles = 20;
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											RegDE.Low8 = ReadMemory(TUS++); RegDE.High8 = ReadMemory(TUS);
											break;
										case 0x5C: // NEG
											ClockCycles = 8;
											RegAF.Value16 = TableNeg[RegAF.Value16];
											break;
										case 0x5D: // RETI
											ClockCycles = 14;
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											break;
										case 0x5E: // IM $2
											ClockCycles = 8;
											interruptMode = 2;
											break;
										case 0x5F: // LD A, R
											ClockCycles = 9;
											RegAF.High8 = (byte)(RegR & 0x7F);
											break;
										case 0x60: // IN H, C
											ClockCycles = 12;
											RegHL.High8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegHL.High8 > 127;
											RegFlagZ = RegHL.High8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegHL.High8];
											RegFlagN = false;
											break;
										case 0x61: // OUT C, H
											ClockCycles = 12;
											WriteHardware(RegBC.Low8, RegHL.High8);
											break;
										case 0x62: // SBC HL, HL
											ClockCycles = 15;
											TI1 = (short)RegHL.Value16; TI2 = (short)RegHL.Value16; TIR = TI1 - TI2;
											if (RegFlagC) { --TIR; ++TI2; }
											TUS = (ushort)TIR;
											RegFlagH = ((TI1 & 0xFFF) - (TI2 & 0xFFF)) < 0;
											RegFlagN = true;
											RegFlagC = ((ushort)TI1 - (ushort)TI2) < 0;
											RegFlagP = TIR > 32767 || TIR < -32768;
											RegFlagS = TUS > 32767;
											RegFlagZ = TUS == 0;
											RegHL.Value16 = TUS;
											break;
										case 0x63: // LD (nn), HL
											ClockCycles = 20;
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											WriteMemory(TUS++, RegHL.Low8);
											WriteMemory(TUS, RegHL.High8);
											break;
										case 0x64: // NEG
											ClockCycles = 8;
											RegAF.Value16 = TableNeg[RegAF.Value16];
											break;
										case 0x65: // RETN
											ClockCycles = 14;
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											this.IFF1 = this.IFF2;
											break;
										case 0x66: // IM $0
											ClockCycles = 8;
											interruptMode = 0;
											break;
										case 0x67: // RRD
											ClockCycles = 18;
											TB1 = RegAF.High8; TB2 = ReadMemory(RegHL.Value16);
											WriteMemory(RegHL.Value16, (byte)((TB2 >> 4) + (TB1 << 4)));
											RegAF.High8 = (byte)((TB1 & 0xF0) + (TB2 & 0x0F));
											RegFlagS = RegAF.High8 > 127;
											RegFlagZ = RegAF.High8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegAF.High8];
											RegFlagN = false;
											break;
										case 0x68: // IN L, C
											ClockCycles = 12;
											RegHL.Low8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegHL.Low8 > 127;
											RegFlagZ = RegHL.Low8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegHL.Low8];
											RegFlagN = false;
											break;
										case 0x69: // OUT C, L
											ClockCycles = 12;
											WriteHardware(RegBC.Low8, RegHL.Low8);
											break;
										case 0x6A: // ADC HL, HL
											ClockCycles = 15;
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
											break;
										case 0x6B: // LD HL, (nn)
											ClockCycles = 20;
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											RegHL.Low8 = ReadMemory(TUS++); RegHL.High8 = ReadMemory(TUS);
											break;
										case 0x6C: // NEG
											ClockCycles = 8;
											RegAF.Value16 = TableNeg[RegAF.Value16];
											break;
										case 0x6D: // RETI
											ClockCycles = 14;
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											break;
										case 0x6E: // IM $0
											ClockCycles = 8;
											interruptMode = 0;
											break;
										case 0x6F: // RLD
											ClockCycles = 18;
											TB1 = RegAF.High8; TB2 = ReadMemory(RegHL.Value16);
											WriteMemory(RegHL.Value16, (byte)((TB1 & 0x0F) + (TB2 << 4)));
											RegAF.High8 = (byte)((TB1 & 0xF0) + (TB2 >> 4));
											RegFlagS = RegAF.High8 > 127;
											RegFlagZ = RegAF.High8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegAF.High8];
											RegFlagN = false;
											break;
										case 0x70: // IN 0, C
											ClockCycles = 12;
											TB = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = TB > 127;
											RegFlagZ = TB == 0;
											RegFlagH = false;
											RegFlagP = TableParity[TB];
											RegFlagN = false;
											break;
										case 0x71: // OUT C, 0
											ClockCycles = 12;
											WriteHardware(RegBC.Low8, 0);
											break;
										case 0x72: // SBC HL, SP
											ClockCycles = 15;
											TI1 = (short)RegHL.Value16; TI2 = (short)RegSP.Value16; TIR = TI1 - TI2;
											if (RegFlagC) { --TIR; ++TI2; }
											TUS = (ushort)TIR;
											RegFlagH = ((TI1 & 0xFFF) - (TI2 & 0xFFF)) < 0;
											RegFlagN = true;
											RegFlagC = ((ushort)TI1 - (ushort)TI2) < 0;
											RegFlagP = TIR > 32767 || TIR < -32768;
											RegFlagS = TUS > 32767;
											RegFlagZ = TUS == 0;
											RegHL.Value16 = TUS;
											break;
										case 0x73: // LD (nn), SP
											ClockCycles = 20;
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											WriteMemory(TUS++, RegSP.Low8);
											WriteMemory(TUS, RegSP.High8);
											break;
										case 0x74: // NEG
											ClockCycles = 8;
											RegAF.Value16 = TableNeg[RegAF.Value16];
											break;
										case 0x75: // RETN
											ClockCycles = 14;
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											this.IFF1 = this.IFF2;
											break;
										case 0x76: // IM $1
											ClockCycles = 8;
											interruptMode = 1;
											break;
										case 0x77: // NOP
											ClockCycles = 4;
											break;
										case 0x78: // IN A, C
											ClockCycles = 12;
											RegAF.High8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegAF.High8 > 127;
											RegFlagZ = RegAF.High8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegAF.High8];
											RegFlagN = false;
											break;
										case 0x79: // OUT C, A
											ClockCycles = 12;
											WriteHardware(RegBC.Low8, RegAF.High8);
											break;
										case 0x7A: // ADC HL, SP
											ClockCycles = 15;
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
											break;
										case 0x7B: // LD SP, (nn)
											ClockCycles = 20;
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											RegSP.Low8 = ReadMemory(TUS++); RegSP.High8 = ReadMemory(TUS);
											break;
										case 0x7C: // NEG
											ClockCycles = 8;
											RegAF.Value16 = TableNeg[RegAF.Value16];
											break;
										case 0x7D: // RETI
											ClockCycles = 14;
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											break;
										case 0x7E: // IM $2
											ClockCycles = 8;
											interruptMode = 2;
											break;
										case 0x7F: // NOP
											ClockCycles = 4;
											break;
										case 0x80: // NOP
											ClockCycles = 4;
											break;
										case 0x81: // NOP
											ClockCycles = 4;
											break;
										case 0x82: // NOP
											ClockCycles = 4;
											break;
										case 0x83: // NOP
											ClockCycles = 4;
											break;
										case 0x84: // NOP
											ClockCycles = 4;
											break;
										case 0x85: // NOP
											ClockCycles = 4;
											break;
										case 0x86: // NOP
											ClockCycles = 4;
											break;
										case 0x87: // NOP
											ClockCycles = 4;
											break;
										case 0x88: // NOP
											ClockCycles = 4;
											break;
										case 0x89: // NOP
											ClockCycles = 4;
											break;
										case 0x8A: // NOP
											ClockCycles = 4;
											break;
										case 0x8B: // NOP
											ClockCycles = 4;
											break;
										case 0x8C: // NOP
											ClockCycles = 4;
											break;
										case 0x8D: // NOP
											ClockCycles = 4;
											break;
										case 0x8E: // NOP
											ClockCycles = 4;
											break;
										case 0x8F: // NOP
											ClockCycles = 4;
											break;
										case 0x90: // NOP
											ClockCycles = 4;
											break;
										case 0x91: // NOP
											ClockCycles = 4;
											break;
										case 0x92: // NOP
											ClockCycles = 4;
											break;
										case 0x93: // NOP
											ClockCycles = 4;
											break;
										case 0x94: // NOP
											ClockCycles = 4;
											break;
										case 0x95: // NOP
											ClockCycles = 4;
											break;
										case 0x96: // NOP
											ClockCycles = 4;
											break;
										case 0x97: // NOP
											ClockCycles = 4;
											break;
										case 0x98: // NOP
											ClockCycles = 4;
											break;
										case 0x99: // NOP
											ClockCycles = 4;
											break;
										case 0x9A: // NOP
											ClockCycles = 4;
											break;
										case 0x9B: // NOP
											ClockCycles = 4;
											break;
										case 0x9C: // NOP
											ClockCycles = 4;
											break;
										case 0x9D: // NOP
											ClockCycles = 4;
											break;
										case 0x9E: // NOP
											ClockCycles = 4;
											break;
										case 0x9F: // NOP
											ClockCycles = 4;
											break;
										case 0xA0: // LDI
											ClockCycles = 16;
											WriteMemory(RegDE.Value16++, ReadMemory(RegHL.Value16++));
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											RegFlagH = false;
											RegFlagN = false;
											break;
										case 0xA1: // CPI
											ClockCycles = 16;
											TB1 = ReadMemory(RegHL.Value16++); TB2 = (byte)(RegAF.High8 - TB1);
											RegFlagN = true;
											RegFlagH = TableHalfBorrow[RegAF.High8, TB1];
											RegFlagZ = TB2 == 0;
											RegFlagS = TB2 > 127;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											break;
										case 0xA2: // INI
											ClockCycles = 16;
											WriteMemory(RegHL.Value16++, ReadHardware(RegBC.Value16));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											break;
										case 0xA3: // OUTI
											ClockCycles = 16;
											WriteHardware(RegBC.Value16, ReadMemory(RegHL.Value16++));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											break;
										case 0xA4: // NOP
											ClockCycles = 4;
											break;
										case 0xA5: // NOP
											ClockCycles = 4;
											break;
										case 0xA6: // NOP
											ClockCycles = 4;
											break;
										case 0xA7: // NOP
											ClockCycles = 4;
											break;
										case 0xA8: // LDD
											ClockCycles = 16;
											WriteMemory(RegDE.Value16--, ReadMemory(RegHL.Value16--));
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											RegFlagH = false;
											RegFlagN = false;
											break;
										case 0xA9: // CPD
											ClockCycles = 16;
											TB1 = ReadMemory(RegHL.Value16--); TB2 = (byte)(RegAF.High8 - TB1);
											RegFlagN = true;
											RegFlagH = TableHalfBorrow[RegAF.High8, TB1];
											RegFlagZ = TB2 == 0;
											RegFlagS = TB2 > 127;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											break;
										case 0xAA: // IND
											ClockCycles = 16;
											WriteMemory(RegHL.Value16--, ReadHardware(RegBC.Value16));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											break;
										case 0xAB: // OUTD
											ClockCycles = 16;
											WriteHardware(RegBC.Value16, ReadMemory(RegHL.Value16--));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											break;
										case 0xAC: // NOP
											ClockCycles = 4;
											break;
										case 0xAD: // NOP
											ClockCycles = 4;
											break;
										case 0xAE: // NOP
											ClockCycles = 4;
											break;
										case 0xAF: // NOP
											ClockCycles = 4;
											break;
										case 0xB0: // LDIR
											ClockCycles = 21;
											WriteMemory(RegDE.Value16++, ReadMemory(RegHL.Value16++));
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											RegFlagH = false;
											RegFlagN = false;
											if (RegBC.Value16 != 0) {
												RegPC.Value16 -= 2;
											} else {
												ClockCycles = 16;
											}
											break;
										case 0xB1: // CPIR
											ClockCycles = 21;
											TB1 = ReadMemory(RegHL.Value16++); TB2 = (byte)(RegAF.High8 - TB1);
											RegFlagN = true;
											RegFlagH = TableHalfBorrow[RegAF.High8, TB1];
											RegFlagZ = TB2 == 0;
											RegFlagS = TB2 > 127;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											if (RegBC.Value16 != 0 && !RegFlagZ) {
												RegPC.Value16 -= 2;
											} else {
												ClockCycles = 16;
											}
											break;
										case 0xB2: // INIR
											ClockCycles = 21;
											WriteMemory(RegHL.Value16++, ReadHardware(RegBC.Value16));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											if (RegBC.High8 != 0) {
												RegPC.Value16 -= 2;
											} else {
												ClockCycles = 16;
											}
											break;
										case 0xB3: // OTIR
											ClockCycles = 21;
											WriteHardware(RegBC.Value16, ReadMemory(RegHL.Value16++));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											if (RegBC.High8 != 0) {
												RegPC.Value16 -= 2;
											} else {
												ClockCycles = 16;
											}
											break;
										case 0xB4: // NOP
											ClockCycles = 4;
											break;
										case 0xB5: // NOP
											ClockCycles = 4;
											break;
										case 0xB6: // NOP
											ClockCycles = 4;
											break;
										case 0xB7: // NOP
											ClockCycles = 4;
											break;
										case 0xB8: // LDDR
											ClockCycles = 21;
											WriteMemory(RegDE.Value16--, ReadMemory(RegHL.Value16--));
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											RegFlagH = false;
											RegFlagN = false;
											if (RegBC.Value16 != 0) {
												RegPC.Value16 -= 2;
											} else {
												ClockCycles = 16;
											}
											break;
										case 0xB9: // CPDR
											ClockCycles = 21;
											TB1 = ReadMemory(RegHL.Value16--); TB2 = (byte)(RegAF.High8 - TB1);
											RegFlagN = true;
											RegFlagH = TableHalfBorrow[RegAF.High8, TB1];
											RegFlagZ = TB2 == 0;
											RegFlagS = TB2 > 127;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											if (RegBC.Value16 != 0 && !RegFlagZ) {
												RegPC.Value16 -= 2;
											} else {
												ClockCycles = 16;
											}
											break;
										case 0xBA: // INDR
											ClockCycles = 21;
											WriteMemory(RegHL.Value16--, ReadHardware(RegBC.Value16));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											if (RegBC.High8 != 0) {
												RegPC.Value16 -= 2;
											} else {
												ClockCycles = 16;
											}
											break;
										case 0xBB: // OTDR
											ClockCycles = 21;
											WriteHardware(RegBC.Value16, ReadMemory(RegHL.Value16--));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											if (RegBC.High8 != 0) {
												RegPC.Value16 -= 2;
											} else {
												ClockCycles = 16;
											}
											break;
										case 0xBC: // NOP
											ClockCycles = 4;
											break;
										case 0xBD: // NOP
											ClockCycles = 4;
											break;
										case 0xBE: // NOP
											ClockCycles = 4;
											break;
										case 0xBF: // NOP
											ClockCycles = 4;
											break;
										case 0xC0: // NOP
											ClockCycles = 4;
											break;
										case 0xC1: // NOP
											ClockCycles = 4;
											break;
										case 0xC2: // NOP
											ClockCycles = 4;
											break;
										case 0xC3: // NOP
											ClockCycles = 4;
											break;
										case 0xC4: // NOP
											ClockCycles = 4;
											break;
										case 0xC5: // NOP
											ClockCycles = 4;
											break;
										case 0xC6: // NOP
											ClockCycles = 4;
											break;
										case 0xC7: // NOP
											ClockCycles = 4;
											break;
										case 0xC8: // NOP
											ClockCycles = 4;
											break;
										case 0xC9: // NOP
											ClockCycles = 4;
											break;
										case 0xCA: // NOP
											ClockCycles = 4;
											break;
										case 0xCB: // NOP
											ClockCycles = 4;
											break;
										case 0xCC: // NOP
											ClockCycles = 4;
											break;
										case 0xCD: // NOP
											ClockCycles = 4;
											break;
										case 0xCE: // NOP
											ClockCycles = 4;
											break;
										case 0xCF: // NOP
											ClockCycles = 4;
											break;
										case 0xD0: // NOP
											ClockCycles = 4;
											break;
										case 0xD1: // NOP
											ClockCycles = 4;
											break;
										case 0xD2: // NOP
											ClockCycles = 4;
											break;
										case 0xD3: // NOP
											ClockCycles = 4;
											break;
										case 0xD4: // NOP
											ClockCycles = 4;
											break;
										case 0xD5: // NOP
											ClockCycles = 4;
											break;
										case 0xD6: // NOP
											ClockCycles = 4;
											break;
										case 0xD7: // NOP
											ClockCycles = 4;
											break;
										case 0xD8: // NOP
											ClockCycles = 4;
											break;
										case 0xD9: // NOP
											ClockCycles = 4;
											break;
										case 0xDA: // NOP
											ClockCycles = 4;
											break;
										case 0xDB: // NOP
											ClockCycles = 4;
											break;
										case 0xDC: // NOP
											ClockCycles = 4;
											break;
										case 0xDD: // NOP
											ClockCycles = 4;
											break;
										case 0xDE: // NOP
											ClockCycles = 4;
											break;
										case 0xDF: // NOP
											ClockCycles = 4;
											break;
										case 0xE0: // NOP
											ClockCycles = 4;
											break;
										case 0xE1: // NOP
											ClockCycles = 4;
											break;
										case 0xE2: // NOP
											ClockCycles = 4;
											break;
										case 0xE3: // NOP
											ClockCycles = 4;
											break;
										case 0xE4: // NOP
											ClockCycles = 4;
											break;
										case 0xE5: // NOP
											ClockCycles = 4;
											break;
										case 0xE6: // NOP
											ClockCycles = 4;
											break;
										case 0xE7: // NOP
											ClockCycles = 4;
											break;
										case 0xE8: // NOP
											ClockCycles = 4;
											break;
										case 0xE9: // NOP
											ClockCycles = 4;
											break;
										case 0xEA: // NOP
											ClockCycles = 4;
											break;
										case 0xEB: // NOP
											ClockCycles = 4;
											break;
										case 0xEC: // NOP
											ClockCycles = 4;
											break;
										case 0xED: // NOP
											ClockCycles = 4;
											break;
										case 0xEE: // NOP
											ClockCycles = 4;
											break;
										case 0xEF: // NOP
											ClockCycles = 4;
											break;
										case 0xF0: // NOP
											ClockCycles = 4;
											break;
										case 0xF1: // NOP
											ClockCycles = 4;
											break;
										case 0xF2: // NOP
											ClockCycles = 4;
											break;
										case 0xF3: // NOP
											ClockCycles = 4;
											break;
										case 0xF4: // NOP
											ClockCycles = 4;
											break;
										case 0xF5: // NOP
											ClockCycles = 4;
											break;
										case 0xF6: // NOP
											ClockCycles = 4;
											break;
										case 0xF7: // NOP
											ClockCycles = 4;
											break;
										case 0xF8: // NOP
											ClockCycles = 4;
											break;
										case 0xF9: // NOP
											ClockCycles = 4;
											break;
										case 0xFA: // NOP
											ClockCycles = 4;
											break;
										case 0xFB: // NOP
											ClockCycles = 4;
											break;
										case 0xFC: // NOP
											ClockCycles = 4;
											break;
										case 0xFD: // NOP
											ClockCycles = 4;
											break;
										case 0xFE: // NOP
											ClockCycles = 4;
											break;
										case 0xFF: // NOP
											ClockCycles = 4;
											break;
									}
									break;
								case 0xEE: // XOR n
									ClockCycles = 7;
									RegAF.Value16 = TableALU[5, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
									break;
								case 0xEF: // RST $28
									ClockCycles = 11;
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x28;
									break;
								case 0xF0: // RET P
									ClockCycles = 11;
									if (!RegFlagS) {
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									} else {
										ClockCycles = 5;
									}
									break;
								case 0xF1: // POP AF
									ClockCycles = 10;
									RegAF.Low8 = ReadMemory(RegSP.Value16++); RegAF.High8 = ReadMemory(RegSP.Value16++);
									break;
								case 0xF2: // JP P, nn
									ClockCycles = 10;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagS) {
										RegPC.Value16 = TUS;
									} else {
										ClockCycles = 10;
									}
									break;
								case 0xF3: // DI
									ClockCycles = 4;
									this.DisableInterrupts();
									break;
								case 0xF4: // CALL P, nn
									ClockCycles = 17;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagS) {
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
									} else {
										ClockCycles = 10;
									}
									break;
								case 0xF5: // PUSH AF
									ClockCycles = 11;
									WriteMemory(--RegSP.Value16, RegAF.High8); WriteMemory(--RegSP.Value16, RegAF.Low8);
									break;
								case 0xF6: // OR n
									ClockCycles = 7;
									RegAF.Value16 = TableALU[6, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
									break;
								case 0xF7: // RST $30
									ClockCycles = 11;
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x30;
									break;
								case 0xF8: // RET M
									ClockCycles = 11;
									if (RegFlagS) {
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									} else {
										ClockCycles = 5;
									}
									break;
								case 0xF9: // LD SP, IX
									ClockCycles = 10;
									RegSP.Value16 = RegIX.Value16;
									break;
								case 0xFA: // JP M, nn
									ClockCycles = 10;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagS) {
										RegPC.Value16 = TUS;
									} else {
										ClockCycles = 10;
									}
									break;
								case 0xFB: // EI
									ClockCycles = 4;
									this.EnableInterrupts();
									break;
								case 0xFC: // CALL M, nn
									ClockCycles = 17;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagS) {
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
									} else {
										ClockCycles = 10;
									}
									break;
								case 0xFD: // <-
									ClockCycles = 1337;
									// Invalid sequence.
									break;
								case 0xFE: // CP n
									ClockCycles = 7;
									RegAF.Value16 = TableALU[7, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
									break;
								case 0xFF: // RST $38
									ClockCycles = 11;
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x38;
									break;
							}
							break;
						case 0xDE: // SBC A, n
							ClockCycles = 7;
							RegAF.Value16 = TableALU[3, RegAF.High8, ReadMemory(RegPC.Value16++), RegFlagC ? 1 : 0];
							break;
						case 0xDF: // RST $18
							ClockCycles = 11;
							WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
							RegPC.Value16 = 0x18;
							break;
						case 0xE0: // RET PO
							ClockCycles = 11;
							if (!RegFlagP) {
								RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
							} else {
								ClockCycles = 5;
							}
							break;
						case 0xE1: // POP HL
							ClockCycles = 10;
							RegHL.Low8 = ReadMemory(RegSP.Value16++); RegHL.High8 = ReadMemory(RegSP.Value16++);
							break;
						case 0xE2: // JP PO, nn
							ClockCycles = 10;
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (!RegFlagP) {
								RegPC.Value16 = TUS;
							} else {
								ClockCycles = 10;
							}
							break;
						case 0xE3: // EX (SP), HL
							ClockCycles = 19;
							TUS = RegSP.Value16; TBL = ReadMemory(TUS++); TBH = ReadMemory(TUS--);
							WriteMemory(TUS++, RegHL.Low8); WriteMemory(TUS, RegHL.High8);
							RegHL.Low8 = TBL; RegHL.High8 = TBH;
							break;
						case 0xE4: // CALL C, nn
							ClockCycles = 17;
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (RegFlagC) {
								WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
								RegPC.Value16 = TUS;
							} else {
								ClockCycles = 10;
							}
							break;
						case 0xE5: // PUSH HL
							ClockCycles = 11;
							WriteMemory(--RegSP.Value16, RegHL.High8); WriteMemory(--RegSP.Value16, RegHL.Low8);
							break;
						case 0xE6: // AND n
							ClockCycles = 7;
							RegAF.Value16 = TableALU[4, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
							break;
						case 0xE7: // RST $20
							ClockCycles = 11;
							WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
							RegPC.Value16 = 0x20;
							break;
						case 0xE8: // RET PE
							ClockCycles = 11;
							if (RegFlagP) {
								RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
							} else {
								ClockCycles = 5;
							}
							break;
						case 0xE9: // JP HL
							ClockCycles = 4;
							RegPC.Value16 = RegHL.Value16;
							break;
						case 0xEA: // JP PE, nn
							ClockCycles = 10;
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (RegFlagP) {
								RegPC.Value16 = TUS;
							} else {
								ClockCycles = 10;
							}
							break;
						case 0xEB: // EX DE, HL
							ClockCycles = 4;
							TUS = RegDE.Value16; RegDE.Value16 = RegHL.Value16; RegHL.Value16 = TUS;
							break;
						case 0xEC: // CALL PE, nn
							ClockCycles = 17;
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (RegFlagP) {
								WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
								RegPC.Value16 = TUS;
							} else {
								ClockCycles = 10;
							}
							break;
						case 0xED: // (Prefix)
							++RegR;
							switch (ReadMemory(RegPC.Value16++)) {
								case 0x00: // NOP
									ClockCycles = 4;
									break;
								case 0x01: // NOP
									ClockCycles = 4;
									break;
								case 0x02: // NOP
									ClockCycles = 4;
									break;
								case 0x03: // NOP
									ClockCycles = 4;
									break;
								case 0x04: // NOP
									ClockCycles = 4;
									break;
								case 0x05: // NOP
									ClockCycles = 4;
									break;
								case 0x06: // NOP
									ClockCycles = 4;
									break;
								case 0x07: // NOP
									ClockCycles = 4;
									break;
								case 0x08: // NOP
									ClockCycles = 4;
									break;
								case 0x09: // NOP
									ClockCycles = 4;
									break;
								case 0x0A: // NOP
									ClockCycles = 4;
									break;
								case 0x0B: // NOP
									ClockCycles = 4;
									break;
								case 0x0C: // NOP
									ClockCycles = 4;
									break;
								case 0x0D: // NOP
									ClockCycles = 4;
									break;
								case 0x0E: // NOP
									ClockCycles = 4;
									break;
								case 0x0F: // NOP
									ClockCycles = 4;
									break;
								case 0x10: // NOP
									ClockCycles = 4;
									break;
								case 0x11: // NOP
									ClockCycles = 4;
									break;
								case 0x12: // NOP
									ClockCycles = 4;
									break;
								case 0x13: // NOP
									ClockCycles = 4;
									break;
								case 0x14: // NOP
									ClockCycles = 4;
									break;
								case 0x15: // NOP
									ClockCycles = 4;
									break;
								case 0x16: // NOP
									ClockCycles = 4;
									break;
								case 0x17: // NOP
									ClockCycles = 4;
									break;
								case 0x18: // NOP
									ClockCycles = 4;
									break;
								case 0x19: // NOP
									ClockCycles = 4;
									break;
								case 0x1A: // NOP
									ClockCycles = 4;
									break;
								case 0x1B: // NOP
									ClockCycles = 4;
									break;
								case 0x1C: // NOP
									ClockCycles = 4;
									break;
								case 0x1D: // NOP
									ClockCycles = 4;
									break;
								case 0x1E: // NOP
									ClockCycles = 4;
									break;
								case 0x1F: // NOP
									ClockCycles = 4;
									break;
								case 0x20: // NOP
									ClockCycles = 4;
									break;
								case 0x21: // NOP
									ClockCycles = 4;
									break;
								case 0x22: // NOP
									ClockCycles = 4;
									break;
								case 0x23: // NOP
									ClockCycles = 4;
									break;
								case 0x24: // NOP
									ClockCycles = 4;
									break;
								case 0x25: // NOP
									ClockCycles = 4;
									break;
								case 0x26: // NOP
									ClockCycles = 4;
									break;
								case 0x27: // NOP
									ClockCycles = 4;
									break;
								case 0x28: // NOP
									ClockCycles = 4;
									break;
								case 0x29: // NOP
									ClockCycles = 4;
									break;
								case 0x2A: // NOP
									ClockCycles = 4;
									break;
								case 0x2B: // NOP
									ClockCycles = 4;
									break;
								case 0x2C: // NOP
									ClockCycles = 4;
									break;
								case 0x2D: // NOP
									ClockCycles = 4;
									break;
								case 0x2E: // NOP
									ClockCycles = 4;
									break;
								case 0x2F: // NOP
									ClockCycles = 4;
									break;
								case 0x30: // NOP
									ClockCycles = 4;
									break;
								case 0x31: // NOP
									ClockCycles = 4;
									break;
								case 0x32: // NOP
									ClockCycles = 4;
									break;
								case 0x33: // NOP
									ClockCycles = 4;
									break;
								case 0x34: // NOP
									ClockCycles = 4;
									break;
								case 0x35: // NOP
									ClockCycles = 4;
									break;
								case 0x36: // NOP
									ClockCycles = 4;
									break;
								case 0x37: // NOP
									ClockCycles = 4;
									break;
								case 0x38: // NOP
									ClockCycles = 4;
									break;
								case 0x39: // NOP
									ClockCycles = 4;
									break;
								case 0x3A: // NOP
									ClockCycles = 4;
									break;
								case 0x3B: // NOP
									ClockCycles = 4;
									break;
								case 0x3C: // NOP
									ClockCycles = 4;
									break;
								case 0x3D: // NOP
									ClockCycles = 4;
									break;
								case 0x3E: // NOP
									ClockCycles = 4;
									break;
								case 0x3F: // NOP
									ClockCycles = 4;
									break;
								case 0x40: // IN B, C
									ClockCycles = 12;
									RegBC.High8 = ReadHardware((ushort)RegBC.Low8);
									RegFlagS = RegBC.High8 > 127;
									RegFlagZ = RegBC.High8 == 0;
									RegFlagH = false;
									RegFlagP = TableParity[RegBC.High8];
									RegFlagN = false;
									break;
								case 0x41: // OUT C, B
									ClockCycles = 12;
									WriteHardware(RegBC.Low8, RegBC.High8);
									break;
								case 0x42: // SBC HL, BC
									ClockCycles = 15;
									TI1 = (short)RegHL.Value16; TI2 = (short)RegBC.Value16; TIR = TI1 - TI2;
									if (RegFlagC) { --TIR; ++TI2; }
									TUS = (ushort)TIR;
									RegFlagH = ((TI1 & 0xFFF) - (TI2 & 0xFFF)) < 0;
									RegFlagN = true;
									RegFlagC = ((ushort)TI1 - (ushort)TI2) < 0;
									RegFlagP = TIR > 32767 || TIR < -32768;
									RegFlagS = TUS > 32767;
									RegFlagZ = TUS == 0;
									RegHL.Value16 = TUS;
									break;
								case 0x43: // LD (nn), BC
									ClockCycles = 20;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									WriteMemory(TUS++, RegBC.Low8);
									WriteMemory(TUS, RegBC.High8);
									break;
								case 0x44: // NEG
									ClockCycles = 8;
									RegAF.Value16 = TableNeg[RegAF.Value16];
									break;
								case 0x45: // RETN
									ClockCycles = 14;
									RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									this.IFF1 = this.IFF2;
									break;
								case 0x46: // IM $0
									ClockCycles = 8;
									interruptMode = 0;
									break;
								case 0x47: // LD I, A
									ClockCycles = 9;
									RegI = RegAF.High8;
									break;
								case 0x48: // IN C, C
									ClockCycles = 12;
									RegBC.Low8 = ReadHardware((ushort)RegBC.Low8);
									RegFlagS = RegBC.Low8 > 127;
									RegFlagZ = RegBC.Low8 == 0;
									RegFlagH = false;
									RegFlagP = TableParity[RegBC.Low8];
									RegFlagN = false;
									break;
								case 0x49: // OUT C, C
									ClockCycles = 12;
									WriteHardware(RegBC.Low8, RegBC.Low8);
									break;
								case 0x4A: // ADC HL, BC
									ClockCycles = 15;
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
									break;
								case 0x4B: // LD BC, (nn)
									ClockCycles = 20;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									RegBC.Low8 = ReadMemory(TUS++); RegBC.High8 = ReadMemory(TUS);
									break;
								case 0x4C: // NEG
									ClockCycles = 8;
									RegAF.Value16 = TableNeg[RegAF.Value16];
									break;
								case 0x4D: // RETI
									ClockCycles = 14;
									RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									break;
								case 0x4E: // IM $0
									ClockCycles = 8;
									interruptMode = 0;
									break;
								case 0x4F: // LD R, A
									ClockCycles = 9;
									RegR = RegAF.High8;
									break;
								case 0x50: // IN D, C
									ClockCycles = 12;
									RegDE.High8 = ReadHardware((ushort)RegBC.Low8);
									RegFlagS = RegDE.High8 > 127;
									RegFlagZ = RegDE.High8 == 0;
									RegFlagH = false;
									RegFlagP = TableParity[RegDE.High8];
									RegFlagN = false;
									break;
								case 0x51: // OUT C, D
									ClockCycles = 12;
									WriteHardware(RegBC.Low8, RegDE.High8);
									break;
								case 0x52: // SBC HL, DE
									ClockCycles = 15;
									TI1 = (short)RegHL.Value16; TI2 = (short)RegDE.Value16; TIR = TI1 - TI2;
									if (RegFlagC) { --TIR; ++TI2; }
									TUS = (ushort)TIR;
									RegFlagH = ((TI1 & 0xFFF) - (TI2 & 0xFFF)) < 0;
									RegFlagN = true;
									RegFlagC = ((ushort)TI1 - (ushort)TI2) < 0;
									RegFlagP = TIR > 32767 || TIR < -32768;
									RegFlagS = TUS > 32767;
									RegFlagZ = TUS == 0;
									RegHL.Value16 = TUS;
									break;
								case 0x53: // LD (nn), DE
									ClockCycles = 20;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									WriteMemory(TUS++, RegDE.Low8);
									WriteMemory(TUS, RegDE.High8);
									break;
								case 0x54: // NEG
									ClockCycles = 8;
									RegAF.Value16 = TableNeg[RegAF.Value16];
									break;
								case 0x55: // RETN
									ClockCycles = 14;
									RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									this.IFF1 = this.IFF2;
									break;
								case 0x56: // IM $1
									ClockCycles = 8;
									interruptMode = 1;
									break;
								case 0x57: // LD A, I
									ClockCycles = 9;
									RegAF.High8 = RegI;
									break;
								case 0x58: // IN E, C
									ClockCycles = 12;
									RegDE.Low8 = ReadHardware((ushort)RegBC.Low8);
									RegFlagS = RegDE.Low8 > 127;
									RegFlagZ = RegDE.Low8 == 0;
									RegFlagH = false;
									RegFlagP = TableParity[RegDE.Low8];
									RegFlagN = false;
									break;
								case 0x59: // OUT C, E
									ClockCycles = 12;
									WriteHardware(RegBC.Low8, RegDE.Low8);
									break;
								case 0x5A: // ADC HL, DE
									ClockCycles = 15;
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
									break;
								case 0x5B: // LD DE, (nn)
									ClockCycles = 20;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									RegDE.Low8 = ReadMemory(TUS++); RegDE.High8 = ReadMemory(TUS);
									break;
								case 0x5C: // NEG
									ClockCycles = 8;
									RegAF.Value16 = TableNeg[RegAF.Value16];
									break;
								case 0x5D: // RETI
									ClockCycles = 14;
									RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									break;
								case 0x5E: // IM $2
									ClockCycles = 8;
									interruptMode = 2;
									break;
								case 0x5F: // LD A, R
									ClockCycles = 9;
									RegAF.High8 = (byte)(RegR & 0x7F);
									break;
								case 0x60: // IN H, C
									ClockCycles = 12;
									RegHL.High8 = ReadHardware((ushort)RegBC.Low8);
									RegFlagS = RegHL.High8 > 127;
									RegFlagZ = RegHL.High8 == 0;
									RegFlagH = false;
									RegFlagP = TableParity[RegHL.High8];
									RegFlagN = false;
									break;
								case 0x61: // OUT C, H
									ClockCycles = 12;
									WriteHardware(RegBC.Low8, RegHL.High8);
									break;
								case 0x62: // SBC HL, HL
									ClockCycles = 15;
									TI1 = (short)RegHL.Value16; TI2 = (short)RegHL.Value16; TIR = TI1 - TI2;
									if (RegFlagC) { --TIR; ++TI2; }
									TUS = (ushort)TIR;
									RegFlagH = ((TI1 & 0xFFF) - (TI2 & 0xFFF)) < 0;
									RegFlagN = true;
									RegFlagC = ((ushort)TI1 - (ushort)TI2) < 0;
									RegFlagP = TIR > 32767 || TIR < -32768;
									RegFlagS = TUS > 32767;
									RegFlagZ = TUS == 0;
									RegHL.Value16 = TUS;
									break;
								case 0x63: // LD (nn), HL
									ClockCycles = 20;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									WriteMemory(TUS++, RegHL.Low8);
									WriteMemory(TUS, RegHL.High8);
									break;
								case 0x64: // NEG
									ClockCycles = 8;
									RegAF.Value16 = TableNeg[RegAF.Value16];
									break;
								case 0x65: // RETN
									ClockCycles = 14;
									RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									this.IFF1 = this.IFF2;
									break;
								case 0x66: // IM $0
									ClockCycles = 8;
									interruptMode = 0;
									break;
								case 0x67: // RRD
									ClockCycles = 18;
									TB1 = RegAF.High8; TB2 = ReadMemory(RegHL.Value16);
									WriteMemory(RegHL.Value16, (byte)((TB2 >> 4) + (TB1 << 4)));
									RegAF.High8 = (byte)((TB1 & 0xF0) + (TB2 & 0x0F));
									RegFlagS = RegAF.High8 > 127;
									RegFlagZ = RegAF.High8 == 0;
									RegFlagH = false;
									RegFlagP = TableParity[RegAF.High8];
									RegFlagN = false;
									break;
								case 0x68: // IN L, C
									ClockCycles = 12;
									RegHL.Low8 = ReadHardware((ushort)RegBC.Low8);
									RegFlagS = RegHL.Low8 > 127;
									RegFlagZ = RegHL.Low8 == 0;
									RegFlagH = false;
									RegFlagP = TableParity[RegHL.Low8];
									RegFlagN = false;
									break;
								case 0x69: // OUT C, L
									ClockCycles = 12;
									WriteHardware(RegBC.Low8, RegHL.Low8);
									break;
								case 0x6A: // ADC HL, HL
									ClockCycles = 15;
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
									break;
								case 0x6B: // LD HL, (nn)
									ClockCycles = 20;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									RegHL.Low8 = ReadMemory(TUS++); RegHL.High8 = ReadMemory(TUS);
									break;
								case 0x6C: // NEG
									ClockCycles = 8;
									RegAF.Value16 = TableNeg[RegAF.Value16];
									break;
								case 0x6D: // RETI
									ClockCycles = 14;
									RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									break;
								case 0x6E: // IM $0
									ClockCycles = 8;
									interruptMode = 0;
									break;
								case 0x6F: // RLD
									ClockCycles = 18;
									TB1 = RegAF.High8; TB2 = ReadMemory(RegHL.Value16);
									WriteMemory(RegHL.Value16, (byte)((TB1 & 0x0F) + (TB2 << 4)));
									RegAF.High8 = (byte)((TB1 & 0xF0) + (TB2 >> 4));
									RegFlagS = RegAF.High8 > 127;
									RegFlagZ = RegAF.High8 == 0;
									RegFlagH = false;
									RegFlagP = TableParity[RegAF.High8];
									RegFlagN = false;
									break;
								case 0x70: // IN 0, C
									ClockCycles = 12;
									TB = ReadHardware((ushort)RegBC.Low8);
									RegFlagS = TB > 127;
									RegFlagZ = TB == 0;
									RegFlagH = false;
									RegFlagP = TableParity[TB];
									RegFlagN = false;
									break;
								case 0x71: // OUT C, 0
									ClockCycles = 12;
									WriteHardware(RegBC.Low8, 0);
									break;
								case 0x72: // SBC HL, SP
									ClockCycles = 15;
									TI1 = (short)RegHL.Value16; TI2 = (short)RegSP.Value16; TIR = TI1 - TI2;
									if (RegFlagC) { --TIR; ++TI2; }
									TUS = (ushort)TIR;
									RegFlagH = ((TI1 & 0xFFF) - (TI2 & 0xFFF)) < 0;
									RegFlagN = true;
									RegFlagC = ((ushort)TI1 - (ushort)TI2) < 0;
									RegFlagP = TIR > 32767 || TIR < -32768;
									RegFlagS = TUS > 32767;
									RegFlagZ = TUS == 0;
									RegHL.Value16 = TUS;
									break;
								case 0x73: // LD (nn), SP
									ClockCycles = 20;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									WriteMemory(TUS++, RegSP.Low8);
									WriteMemory(TUS, RegSP.High8);
									break;
								case 0x74: // NEG
									ClockCycles = 8;
									RegAF.Value16 = TableNeg[RegAF.Value16];
									break;
								case 0x75: // RETN
									ClockCycles = 14;
									RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									this.IFF1 = this.IFF2;
									break;
								case 0x76: // IM $1
									ClockCycles = 8;
									interruptMode = 1;
									break;
								case 0x77: // NOP
									ClockCycles = 4;
									break;
								case 0x78: // IN A, C
									ClockCycles = 12;
									RegAF.High8 = ReadHardware((ushort)RegBC.Low8);
									RegFlagS = RegAF.High8 > 127;
									RegFlagZ = RegAF.High8 == 0;
									RegFlagH = false;
									RegFlagP = TableParity[RegAF.High8];
									RegFlagN = false;
									break;
								case 0x79: // OUT C, A
									ClockCycles = 12;
									WriteHardware(RegBC.Low8, RegAF.High8);
									break;
								case 0x7A: // ADC HL, SP
									ClockCycles = 15;
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
									break;
								case 0x7B: // LD SP, (nn)
									ClockCycles = 20;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									RegSP.Low8 = ReadMemory(TUS++); RegSP.High8 = ReadMemory(TUS);
									break;
								case 0x7C: // NEG
									ClockCycles = 8;
									RegAF.Value16 = TableNeg[RegAF.Value16];
									break;
								case 0x7D: // RETI
									ClockCycles = 14;
									RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									break;
								case 0x7E: // IM $2
									ClockCycles = 8;
									interruptMode = 2;
									break;
								case 0x7F: // NOP
									ClockCycles = 4;
									break;
								case 0x80: // NOP
									ClockCycles = 4;
									break;
								case 0x81: // NOP
									ClockCycles = 4;
									break;
								case 0x82: // NOP
									ClockCycles = 4;
									break;
								case 0x83: // NOP
									ClockCycles = 4;
									break;
								case 0x84: // NOP
									ClockCycles = 4;
									break;
								case 0x85: // NOP
									ClockCycles = 4;
									break;
								case 0x86: // NOP
									ClockCycles = 4;
									break;
								case 0x87: // NOP
									ClockCycles = 4;
									break;
								case 0x88: // NOP
									ClockCycles = 4;
									break;
								case 0x89: // NOP
									ClockCycles = 4;
									break;
								case 0x8A: // NOP
									ClockCycles = 4;
									break;
								case 0x8B: // NOP
									ClockCycles = 4;
									break;
								case 0x8C: // NOP
									ClockCycles = 4;
									break;
								case 0x8D: // NOP
									ClockCycles = 4;
									break;
								case 0x8E: // NOP
									ClockCycles = 4;
									break;
								case 0x8F: // NOP
									ClockCycles = 4;
									break;
								case 0x90: // NOP
									ClockCycles = 4;
									break;
								case 0x91: // NOP
									ClockCycles = 4;
									break;
								case 0x92: // NOP
									ClockCycles = 4;
									break;
								case 0x93: // NOP
									ClockCycles = 4;
									break;
								case 0x94: // NOP
									ClockCycles = 4;
									break;
								case 0x95: // NOP
									ClockCycles = 4;
									break;
								case 0x96: // NOP
									ClockCycles = 4;
									break;
								case 0x97: // NOP
									ClockCycles = 4;
									break;
								case 0x98: // NOP
									ClockCycles = 4;
									break;
								case 0x99: // NOP
									ClockCycles = 4;
									break;
								case 0x9A: // NOP
									ClockCycles = 4;
									break;
								case 0x9B: // NOP
									ClockCycles = 4;
									break;
								case 0x9C: // NOP
									ClockCycles = 4;
									break;
								case 0x9D: // NOP
									ClockCycles = 4;
									break;
								case 0x9E: // NOP
									ClockCycles = 4;
									break;
								case 0x9F: // NOP
									ClockCycles = 4;
									break;
								case 0xA0: // LDI
									ClockCycles = 16;
									WriteMemory(RegDE.Value16++, ReadMemory(RegHL.Value16++));
									--RegBC.Value16;
									RegFlagP = RegBC.Value16 != 0;
									RegFlagH = false;
									RegFlagN = false;
									break;
								case 0xA1: // CPI
									ClockCycles = 16;
									TB1 = ReadMemory(RegHL.Value16++); TB2 = (byte)(RegAF.High8 - TB1);
									RegFlagN = true;
									RegFlagH = TableHalfBorrow[RegAF.High8, TB1];
									RegFlagZ = TB2 == 0;
									RegFlagS = TB2 > 127;
									--RegBC.Value16;
									RegFlagP = RegBC.Value16 != 0;
									break;
								case 0xA2: // INI
									ClockCycles = 16;
									WriteMemory(RegHL.Value16++, ReadHardware(RegBC.Value16));
									--RegBC.High8;
									RegFlagZ = RegBC.High8 == 0;
									RegFlagN = true;
									break;
								case 0xA3: // OUTI
									ClockCycles = 16;
									WriteHardware(RegBC.Value16, ReadMemory(RegHL.Value16++));
									--RegBC.High8;
									RegFlagZ = RegBC.High8 == 0;
									RegFlagN = true;
									break;
								case 0xA4: // NOP
									ClockCycles = 4;
									break;
								case 0xA5: // NOP
									ClockCycles = 4;
									break;
								case 0xA6: // NOP
									ClockCycles = 4;
									break;
								case 0xA7: // NOP
									ClockCycles = 4;
									break;
								case 0xA8: // LDD
									ClockCycles = 16;
									WriteMemory(RegDE.Value16--, ReadMemory(RegHL.Value16--));
									--RegBC.Value16;
									RegFlagP = RegBC.Value16 != 0;
									RegFlagH = false;
									RegFlagN = false;
									break;
								case 0xA9: // CPD
									ClockCycles = 16;
									TB1 = ReadMemory(RegHL.Value16--); TB2 = (byte)(RegAF.High8 - TB1);
									RegFlagN = true;
									RegFlagH = TableHalfBorrow[RegAF.High8, TB1];
									RegFlagZ = TB2 == 0;
									RegFlagS = TB2 > 127;
									--RegBC.Value16;
									RegFlagP = RegBC.Value16 != 0;
									break;
								case 0xAA: // IND
									ClockCycles = 16;
									WriteMemory(RegHL.Value16--, ReadHardware(RegBC.Value16));
									--RegBC.High8;
									RegFlagZ = RegBC.High8 == 0;
									RegFlagN = true;
									break;
								case 0xAB: // OUTD
									ClockCycles = 16;
									WriteHardware(RegBC.Value16, ReadMemory(RegHL.Value16--));
									--RegBC.High8;
									RegFlagZ = RegBC.High8 == 0;
									RegFlagN = true;
									break;
								case 0xAC: // NOP
									ClockCycles = 4;
									break;
								case 0xAD: // NOP
									ClockCycles = 4;
									break;
								case 0xAE: // NOP
									ClockCycles = 4;
									break;
								case 0xAF: // NOP
									ClockCycles = 4;
									break;
								case 0xB0: // LDIR
									ClockCycles = 21;
									WriteMemory(RegDE.Value16++, ReadMemory(RegHL.Value16++));
									--RegBC.Value16;
									RegFlagP = RegBC.Value16 != 0;
									RegFlagH = false;
									RegFlagN = false;
									if (RegBC.Value16 != 0) {
										RegPC.Value16 -= 2;
									} else {
										ClockCycles = 16;
									}
									break;
								case 0xB1: // CPIR
									ClockCycles = 21;
									TB1 = ReadMemory(RegHL.Value16++); TB2 = (byte)(RegAF.High8 - TB1);
									RegFlagN = true;
									RegFlagH = TableHalfBorrow[RegAF.High8, TB1];
									RegFlagZ = TB2 == 0;
									RegFlagS = TB2 > 127;
									--RegBC.Value16;
									RegFlagP = RegBC.Value16 != 0;
									if (RegBC.Value16 != 0 && !RegFlagZ) {
										RegPC.Value16 -= 2;
									} else {
										ClockCycles = 16;
									}
									break;
								case 0xB2: // INIR
									ClockCycles = 21;
									WriteMemory(RegHL.Value16++, ReadHardware(RegBC.Value16));
									--RegBC.High8;
									RegFlagZ = RegBC.High8 == 0;
									RegFlagN = true;
									if (RegBC.High8 != 0) {
										RegPC.Value16 -= 2;
									} else {
										ClockCycles = 16;
									}
									break;
								case 0xB3: // OTIR
									ClockCycles = 21;
									WriteHardware(RegBC.Value16, ReadMemory(RegHL.Value16++));
									--RegBC.High8;
									RegFlagZ = RegBC.High8 == 0;
									RegFlagN = true;
									if (RegBC.High8 != 0) {
										RegPC.Value16 -= 2;
									} else {
										ClockCycles = 16;
									}
									break;
								case 0xB4: // NOP
									ClockCycles = 4;
									break;
								case 0xB5: // NOP
									ClockCycles = 4;
									break;
								case 0xB6: // NOP
									ClockCycles = 4;
									break;
								case 0xB7: // NOP
									ClockCycles = 4;
									break;
								case 0xB8: // LDDR
									ClockCycles = 21;
									WriteMemory(RegDE.Value16--, ReadMemory(RegHL.Value16--));
									--RegBC.Value16;
									RegFlagP = RegBC.Value16 != 0;
									RegFlagH = false;
									RegFlagN = false;
									if (RegBC.Value16 != 0) {
										RegPC.Value16 -= 2;
									} else {
										ClockCycles = 16;
									}
									break;
								case 0xB9: // CPDR
									ClockCycles = 21;
									TB1 = ReadMemory(RegHL.Value16--); TB2 = (byte)(RegAF.High8 - TB1);
									RegFlagN = true;
									RegFlagH = TableHalfBorrow[RegAF.High8, TB1];
									RegFlagZ = TB2 == 0;
									RegFlagS = TB2 > 127;
									--RegBC.Value16;
									RegFlagP = RegBC.Value16 != 0;
									if (RegBC.Value16 != 0 && !RegFlagZ) {
										RegPC.Value16 -= 2;
									} else {
										ClockCycles = 16;
									}
									break;
								case 0xBA: // INDR
									ClockCycles = 21;
									WriteMemory(RegHL.Value16--, ReadHardware(RegBC.Value16));
									--RegBC.High8;
									RegFlagZ = RegBC.High8 == 0;
									RegFlagN = true;
									if (RegBC.High8 != 0) {
										RegPC.Value16 -= 2;
									} else {
										ClockCycles = 16;
									}
									break;
								case 0xBB: // OTDR
									ClockCycles = 21;
									WriteHardware(RegBC.Value16, ReadMemory(RegHL.Value16--));
									--RegBC.High8;
									RegFlagZ = RegBC.High8 == 0;
									RegFlagN = true;
									if (RegBC.High8 != 0) {
										RegPC.Value16 -= 2;
									} else {
										ClockCycles = 16;
									}
									break;
								case 0xBC: // NOP
									ClockCycles = 4;
									break;
								case 0xBD: // NOP
									ClockCycles = 4;
									break;
								case 0xBE: // NOP
									ClockCycles = 4;
									break;
								case 0xBF: // NOP
									ClockCycles = 4;
									break;
								case 0xC0: // NOP
									ClockCycles = 4;
									break;
								case 0xC1: // NOP
									ClockCycles = 4;
									break;
								case 0xC2: // NOP
									ClockCycles = 4;
									break;
								case 0xC3: // NOP
									ClockCycles = 4;
									break;
								case 0xC4: // NOP
									ClockCycles = 4;
									break;
								case 0xC5: // NOP
									ClockCycles = 4;
									break;
								case 0xC6: // NOP
									ClockCycles = 4;
									break;
								case 0xC7: // NOP
									ClockCycles = 4;
									break;
								case 0xC8: // NOP
									ClockCycles = 4;
									break;
								case 0xC9: // NOP
									ClockCycles = 4;
									break;
								case 0xCA: // NOP
									ClockCycles = 4;
									break;
								case 0xCB: // NOP
									ClockCycles = 4;
									break;
								case 0xCC: // NOP
									ClockCycles = 4;
									break;
								case 0xCD: // NOP
									ClockCycles = 4;
									break;
								case 0xCE: // NOP
									ClockCycles = 4;
									break;
								case 0xCF: // NOP
									ClockCycles = 4;
									break;
								case 0xD0: // NOP
									ClockCycles = 4;
									break;
								case 0xD1: // NOP
									ClockCycles = 4;
									break;
								case 0xD2: // NOP
									ClockCycles = 4;
									break;
								case 0xD3: // NOP
									ClockCycles = 4;
									break;
								case 0xD4: // NOP
									ClockCycles = 4;
									break;
								case 0xD5: // NOP
									ClockCycles = 4;
									break;
								case 0xD6: // NOP
									ClockCycles = 4;
									break;
								case 0xD7: // NOP
									ClockCycles = 4;
									break;
								case 0xD8: // NOP
									ClockCycles = 4;
									break;
								case 0xD9: // NOP
									ClockCycles = 4;
									break;
								case 0xDA: // NOP
									ClockCycles = 4;
									break;
								case 0xDB: // NOP
									ClockCycles = 4;
									break;
								case 0xDC: // NOP
									ClockCycles = 4;
									break;
								case 0xDD: // NOP
									ClockCycles = 4;
									break;
								case 0xDE: // NOP
									ClockCycles = 4;
									break;
								case 0xDF: // NOP
									ClockCycles = 4;
									break;
								case 0xE0: // NOP
									ClockCycles = 4;
									break;
								case 0xE1: // NOP
									ClockCycles = 4;
									break;
								case 0xE2: // NOP
									ClockCycles = 4;
									break;
								case 0xE3: // NOP
									ClockCycles = 4;
									break;
								case 0xE4: // NOP
									ClockCycles = 4;
									break;
								case 0xE5: // NOP
									ClockCycles = 4;
									break;
								case 0xE6: // NOP
									ClockCycles = 4;
									break;
								case 0xE7: // NOP
									ClockCycles = 4;
									break;
								case 0xE8: // NOP
									ClockCycles = 4;
									break;
								case 0xE9: // NOP
									ClockCycles = 4;
									break;
								case 0xEA: // NOP
									ClockCycles = 4;
									break;
								case 0xEB: // NOP
									ClockCycles = 4;
									break;
								case 0xEC: // NOP
									ClockCycles = 4;
									break;
								case 0xED: // NOP
									ClockCycles = 4;
									break;
								case 0xEE: // NOP
									ClockCycles = 4;
									break;
								case 0xEF: // NOP
									ClockCycles = 4;
									break;
								case 0xF0: // NOP
									ClockCycles = 4;
									break;
								case 0xF1: // NOP
									ClockCycles = 4;
									break;
								case 0xF2: // NOP
									ClockCycles = 4;
									break;
								case 0xF3: // NOP
									ClockCycles = 4;
									break;
								case 0xF4: // NOP
									ClockCycles = 4;
									break;
								case 0xF5: // NOP
									ClockCycles = 4;
									break;
								case 0xF6: // NOP
									ClockCycles = 4;
									break;
								case 0xF7: // NOP
									ClockCycles = 4;
									break;
								case 0xF8: // NOP
									ClockCycles = 4;
									break;
								case 0xF9: // NOP
									ClockCycles = 4;
									break;
								case 0xFA: // NOP
									ClockCycles = 4;
									break;
								case 0xFB: // NOP
									ClockCycles = 4;
									break;
								case 0xFC: // NOP
									ClockCycles = 4;
									break;
								case 0xFD: // NOP
									ClockCycles = 4;
									break;
								case 0xFE: // NOP
									ClockCycles = 4;
									break;
								case 0xFF: // NOP
									ClockCycles = 4;
									break;
							}
							break;
						case 0xEE: // XOR n
							ClockCycles = 7;
							RegAF.Value16 = TableALU[5, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
							break;
						case 0xEF: // RST $28
							ClockCycles = 11;
							WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
							RegPC.Value16 = 0x28;
							break;
						case 0xF0: // RET P
							ClockCycles = 11;
							if (!RegFlagS) {
								RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
							} else {
								ClockCycles = 5;
							}
							break;
						case 0xF1: // POP AF
							ClockCycles = 10;
							RegAF.Low8 = ReadMemory(RegSP.Value16++); RegAF.High8 = ReadMemory(RegSP.Value16++);
							break;
						case 0xF2: // JP P, nn
							ClockCycles = 10;
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (!RegFlagS) {
								RegPC.Value16 = TUS;
							} else {
								ClockCycles = 10;
							}
							break;
						case 0xF3: // DI
							ClockCycles = 4;
							this.DisableInterrupts();
							break;
						case 0xF4: // CALL P, nn
							ClockCycles = 17;
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (!RegFlagS) {
								WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
								RegPC.Value16 = TUS;
							} else {
								ClockCycles = 10;
							}
							break;
						case 0xF5: // PUSH AF
							ClockCycles = 11;
							WriteMemory(--RegSP.Value16, RegAF.High8); WriteMemory(--RegSP.Value16, RegAF.Low8);
							break;
						case 0xF6: // OR n
							ClockCycles = 7;
							RegAF.Value16 = TableALU[6, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
							break;
						case 0xF7: // RST $30
							ClockCycles = 11;
							WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
							RegPC.Value16 = 0x30;
							break;
						case 0xF8: // RET M
							ClockCycles = 11;
							if (RegFlagS) {
								RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
							} else {
								ClockCycles = 5;
							}
							break;
						case 0xF9: // LD SP, HL
							ClockCycles = 6;
							RegSP.Value16 = RegHL.Value16;
							break;
						case 0xFA: // JP M, nn
							ClockCycles = 10;
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (RegFlagS) {
								RegPC.Value16 = TUS;
							} else {
								ClockCycles = 10;
							}
							break;
						case 0xFB: // EI
							ClockCycles = 4;
							this.EnableInterrupts();
							break;
						case 0xFC: // CALL M, nn
							ClockCycles = 17;
							TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
							if (RegFlagS) {
								WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
								RegPC.Value16 = TUS;
							} else {
								ClockCycles = 10;
							}
							break;
						case 0xFD: // (Prefix)
							++RegR;
							switch (ReadMemory(RegPC.Value16++)) {
								case 0x00: // NOP
									ClockCycles = 4;
									break;
								case 0x01: // LD BC, nn
									ClockCycles = 10;
									RegBC.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									break;
								case 0x02: // LD (BC), A
									ClockCycles = 7;
									WriteMemory(RegBC.Value16, RegAF.High8);
									break;
								case 0x03: // INC BC
									ClockCycles = 6;
									++RegBC.Value16;
									break;
								case 0x04: // INC B
									ClockCycles = 4;
									RegAF.Low8 = (byte)(TableInc[++RegBC.High8] | (RegAF.Low8 & 1));
									break;
								case 0x05: // DEC B
									ClockCycles = 4;
									RegAF.Low8 = (byte)(TableDec[--RegBC.High8] | (RegAF.Low8 & 1));
									break;
								case 0x06: // LD B, n
									ClockCycles = 7;
									RegBC.High8 = ReadMemory(RegPC.Value16++);
									break;
								case 0x07: // RLCA
									ClockCycles = 4;
									RegAF.Value16 = TableRotShift[0, 0, RegAF.Value16];
									break;
								case 0x08: // EX AF, AF'
									ClockCycles = 4;
									TUS = RegAF.Value16; RegAF.Value16 = RegAltAF.Value16; RegAltAF.Value16 = TUS;
									break;
								case 0x09: // ADD IY, BC
									ClockCycles = 15;
									TI1 = (short)RegIY.Value16; TI2 = (short)RegBC.Value16; TIR = TI1 + TI2;
									TUS = (ushort)TIR;
									RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
									RegFlagN = false;
									RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
									RegIY.Value16 = TUS;
									break;
								case 0x0A: // LD A, (BC)
									ClockCycles = 7;
									RegAF.High8 = ReadMemory(RegBC.Value16);
									break;
								case 0x0B: // DEC BC
									ClockCycles = 6;
									--RegBC.Value16;
									break;
								case 0x0C: // INC C
									ClockCycles = 4;
									RegAF.Low8 = (byte)(TableInc[++RegBC.Low8] | (RegAF.Low8 & 1));
									break;
								case 0x0D: // DEC C
									ClockCycles = 4;
									RegAF.Low8 = (byte)(TableDec[--RegBC.Low8] | (RegAF.Low8 & 1));
									break;
								case 0x0E: // LD C, n
									ClockCycles = 7;
									RegBC.Low8 = ReadMemory(RegPC.Value16++);
									break;
								case 0x0F: // RRCA
									ClockCycles = 4;
									RegAF.Value16 = TableRotShift[0, 1, RegAF.Value16];
									break;
								case 0x10: // DJNZ d
									ClockCycles = 13;
									TSB = (sbyte)ReadMemory(RegPC.Value16++);
									if (--RegBC.High8 != 0) {
										RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
									} else {
										ClockCycles = 8;
									}
									break;
								case 0x11: // LD DE, nn
									ClockCycles = 10;
									RegDE.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									break;
								case 0x12: // LD (DE), A
									ClockCycles = 7;
									WriteMemory(RegDE.Value16, RegAF.High8);
									break;
								case 0x13: // INC DE
									ClockCycles = 6;
									++RegDE.Value16;
									break;
								case 0x14: // INC D
									ClockCycles = 4;
									RegAF.Low8 = (byte)(TableInc[++RegDE.High8] | (RegAF.Low8 & 1));
									break;
								case 0x15: // DEC D
									ClockCycles = 4;
									RegAF.Low8 = (byte)(TableDec[--RegDE.High8] | (RegAF.Low8 & 1));
									break;
								case 0x16: // LD D, n
									ClockCycles = 7;
									RegDE.High8 = ReadMemory(RegPC.Value16++);
									break;
								case 0x17: // RLA
									ClockCycles = 4;
									RegAF.Value16 = TableRotShift[0, 2, RegAF.Value16];
									break;
								case 0x18: // JR d
									ClockCycles = 12;
									TSB = (sbyte)ReadMemory(RegPC.Value16++);
									RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
									break;
								case 0x19: // ADD IY, DE
									ClockCycles = 15;
									TI1 = (short)RegIY.Value16; TI2 = (short)RegDE.Value16; TIR = TI1 + TI2;
									TUS = (ushort)TIR;
									RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
									RegFlagN = false;
									RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
									RegIY.Value16 = TUS;
									break;
								case 0x1A: // LD A, (DE)
									ClockCycles = 7;
									RegAF.High8 = ReadMemory(RegDE.Value16);
									break;
								case 0x1B: // DEC DE
									ClockCycles = 6;
									--RegDE.Value16;
									break;
								case 0x1C: // INC E
									ClockCycles = 4;
									RegAF.Low8 = (byte)(TableInc[++RegDE.Low8] | (RegAF.Low8 & 1));
									break;
								case 0x1D: // DEC E
									ClockCycles = 4;
									RegAF.Low8 = (byte)(TableDec[--RegDE.Low8] | (RegAF.Low8 & 1));
									break;
								case 0x1E: // LD E, n
									ClockCycles = 7;
									RegDE.Low8 = ReadMemory(RegPC.Value16++);
									break;
								case 0x1F: // RRA
									ClockCycles = 4;
									RegAF.Value16 = TableRotShift[0, 3, RegAF.Value16];
									break;
								case 0x20: // JR NZ, d
									ClockCycles = 12;
									TSB = (sbyte)ReadMemory(RegPC.Value16++);
									if (!RegFlagZ) {
										RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
									} else {
										ClockCycles = 7;
									}
									break;
								case 0x21: // LD IY, nn
									ClockCycles = 14;
									RegIY.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									break;
								case 0x22: // LD (nn), IY
									ClockCycles = 20;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									WriteMemory(TUS++, RegIY.Low8);
									WriteMemory(TUS, RegIY.High8);
									break;
								case 0x23: // INC IY
									ClockCycles = 10;
									++RegIY.Value16;
									break;
								case 0x24: // INC IYH
									ClockCycles = 9;
									RegAF.Low8 = (byte)(TableInc[++RegIY.High8] | (RegAF.Low8 & 1));
									break;
								case 0x25: // DEC IYH
									ClockCycles = 9;
									RegAF.Low8 = (byte)(TableDec[--RegIY.High8] | (RegAF.Low8 & 1));
									break;
								case 0x26: // LD IYH, n
									ClockCycles = 9;
									RegIY.High8 = ReadMemory(RegPC.Value16++);
									break;
								case 0x27: // DAA
									ClockCycles = 4;
									RegAF.Value16 = TableDaa[RegAF.Value16];
									break;
								case 0x28: // JR Z, d
									ClockCycles = 12;
									TSB = (sbyte)ReadMemory(RegPC.Value16++);
									if (RegFlagZ) {
										RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
									} else {
										ClockCycles = 7;
									}
									break;
								case 0x29: // ADD IY, IY
									ClockCycles = 15;
									TI1 = (short)RegIY.Value16; TI2 = (short)RegIY.Value16; TIR = TI1 + TI2;
									TUS = (ushort)TIR;
									RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
									RegFlagN = false;
									RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
									RegIY.Value16 = TUS;
									break;
								case 0x2A: // LD IY, (nn)
									ClockCycles = 20;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									RegIY.Low8 = ReadMemory(TUS++); RegIY.High8 = ReadMemory(TUS);
									break;
								case 0x2B: // DEC IY
									ClockCycles = 10;
									--RegIY.Value16;
									break;
								case 0x2C: // INC IYL
									ClockCycles = 9;
									RegAF.Low8 = (byte)(TableInc[++RegIY.Low8] | (RegAF.Low8 & 1));
									break;
								case 0x2D: // DEC IYL
									ClockCycles = 9;
									RegAF.Low8 = (byte)(TableDec[--RegIY.Low8] | (RegAF.Low8 & 1));
									break;
								case 0x2E: // LD IYL, n
									ClockCycles = 9;
									RegIY.Low8 = ReadMemory(RegPC.Value16++);
									break;
								case 0x2F: // CPL
									ClockCycles = 4;
									RegAF.High8 ^= 0xFF; RegFlagH = true; RegFlagN = true;
									break;
								case 0x30: // JR NC, d
									ClockCycles = 12;
									TSB = (sbyte)ReadMemory(RegPC.Value16++);
									if (!RegFlagC) {
										RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
									} else {
										ClockCycles = 7;
									}
									break;
								case 0x31: // LD SP, nn
									ClockCycles = 10;
									RegSP.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									break;
								case 0x32: // LD (nn), A
									ClockCycles = 13;
									WriteMemory((ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256), RegAF.High8);
									break;
								case 0x33: // INC SP
									ClockCycles = 6;
									++RegSP.Value16;
									break;
								case 0x34: // INC (IY+d)
									ClockCycles = 23;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									TB = ReadMemory((ushort)(RegIY.Value16 + Displacement)); RegAF.Low8 = (byte)(TableInc[++TB] | (RegAF.Low8 & 1)); WriteMemory((ushort)(RegIY.Value16 + Displacement), TB);
									break;
								case 0x35: // DEC (IY+d)
									ClockCycles = 23;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									TB = ReadMemory((ushort)(RegIY.Value16 + Displacement)); RegAF.Low8 = (byte)(TableDec[--TB] | (RegAF.Low8 & 1)); WriteMemory((ushort)(RegIY.Value16 + Displacement), TB);
									break;
								case 0x36: // LD (IY+d), n
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIY.Value16 + Displacement), ReadMemory(RegPC.Value16++));
									break;
								case 0x37: // SCF
									ClockCycles = 4;
									RegFlagH = false; RegFlagN = false; RegFlagC = true;
									break;
								case 0x38: // JR C, d
									ClockCycles = 12;
									TSB = (sbyte)ReadMemory(RegPC.Value16++);
									if (RegFlagC) {
										RegPC.Value16 = (ushort)(RegPC.Value16 + TSB);
									} else {
										ClockCycles = 7;
									}
									break;
								case 0x39: // ADD IY, SP
									ClockCycles = 15;
									TI1 = (short)RegIY.Value16; TI2 = (short)RegSP.Value16; TIR = TI1 + TI2;
									TUS = (ushort)TIR;
									RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
									RegFlagN = false;
									RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
									RegIY.Value16 = TUS;
									break;
								case 0x3A: // LD A, (nn)
									ClockCycles = 13;
									RegAF.High8 = ReadMemory((ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256));
									break;
								case 0x3B: // DEC SP
									ClockCycles = 6;
									--RegSP.Value16;
									break;
								case 0x3C: // INC A
									ClockCycles = 4;
									RegAF.Low8 = (byte)(TableInc[++RegAF.High8] | (RegAF.Low8 & 1));
									break;
								case 0x3D: // DEC A
									ClockCycles = 4;
									RegAF.Low8 = (byte)(TableDec[--RegAF.High8] | (RegAF.Low8 & 1));
									break;
								case 0x3E: // LD A, n
									ClockCycles = 7;
									RegAF.High8 = ReadMemory(RegPC.Value16++);
									break;
								case 0x3F: // CCF
									ClockCycles = 4;
									RegFlagH = RegFlagC; RegFlagN = false; RegFlagC ^= true;
									break;
								case 0x40: // LD B, B
									ClockCycles = 4;
									break;
								case 0x41: // LD B, C
									ClockCycles = 4;
									RegBC.High8 = RegBC.Low8;
									break;
								case 0x42: // LD B, D
									ClockCycles = 4;
									RegBC.High8 = RegDE.High8;
									break;
								case 0x43: // LD B, E
									ClockCycles = 4;
									RegBC.High8 = RegDE.Low8;
									break;
								case 0x44: // LD B, IYH
									ClockCycles = 9;
									RegBC.High8 = RegIY.High8;
									break;
								case 0x45: // LD B, IYL
									ClockCycles = 9;
									RegBC.High8 = RegIY.Low8;
									break;
								case 0x46: // LD B, (IY+d)
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegBC.High8 = ReadMemory((ushort)(RegIY.Value16 + Displacement));
									break;
								case 0x47: // LD B, A
									ClockCycles = 4;
									RegBC.High8 = RegAF.High8;
									break;
								case 0x48: // LD C, B
									ClockCycles = 4;
									RegBC.Low8 = RegBC.High8;
									break;
								case 0x49: // LD C, C
									ClockCycles = 4;
									break;
								case 0x4A: // LD C, D
									ClockCycles = 4;
									RegBC.Low8 = RegDE.High8;
									break;
								case 0x4B: // LD C, E
									ClockCycles = 4;
									RegBC.Low8 = RegDE.Low8;
									break;
								case 0x4C: // LD C, IYH
									ClockCycles = 9;
									RegBC.Low8 = RegIY.High8;
									break;
								case 0x4D: // LD C, IYL
									ClockCycles = 9;
									RegBC.Low8 = RegIY.Low8;
									break;
								case 0x4E: // LD C, (IY+d)
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegBC.Low8 = ReadMemory((ushort)(RegIY.Value16 + Displacement));
									break;
								case 0x4F: // LD C, A
									ClockCycles = 4;
									RegBC.Low8 = RegAF.High8;
									break;
								case 0x50: // LD D, B
									ClockCycles = 4;
									RegDE.High8 = RegBC.High8;
									break;
								case 0x51: // LD D, C
									ClockCycles = 4;
									RegDE.High8 = RegBC.Low8;
									break;
								case 0x52: // LD D, D
									ClockCycles = 4;
									break;
								case 0x53: // LD D, E
									ClockCycles = 4;
									RegDE.High8 = RegDE.Low8;
									break;
								case 0x54: // LD D, IYH
									ClockCycles = 9;
									RegDE.High8 = RegIY.High8;
									break;
								case 0x55: // LD D, IYL
									ClockCycles = 9;
									RegDE.High8 = RegIY.Low8;
									break;
								case 0x56: // LD D, (IY+d)
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegDE.High8 = ReadMemory((ushort)(RegIY.Value16 + Displacement));
									break;
								case 0x57: // LD D, A
									ClockCycles = 4;
									RegDE.High8 = RegAF.High8;
									break;
								case 0x58: // LD E, B
									ClockCycles = 4;
									RegDE.Low8 = RegBC.High8;
									break;
								case 0x59: // LD E, C
									ClockCycles = 4;
									RegDE.Low8 = RegBC.Low8;
									break;
								case 0x5A: // LD E, D
									ClockCycles = 4;
									RegDE.Low8 = RegDE.High8;
									break;
								case 0x5B: // LD E, E
									ClockCycles = 4;
									break;
								case 0x5C: // LD E, IYH
									ClockCycles = 9;
									RegDE.Low8 = RegIY.High8;
									break;
								case 0x5D: // LD E, IYL
									ClockCycles = 9;
									RegDE.Low8 = RegIY.Low8;
									break;
								case 0x5E: // LD E, (IY+d)
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegDE.Low8 = ReadMemory((ushort)(RegIY.Value16 + Displacement));
									break;
								case 0x5F: // LD E, A
									ClockCycles = 4;
									RegDE.Low8 = RegAF.High8;
									break;
								case 0x60: // LD IYH, B
									ClockCycles = 9;
									RegIY.High8 = RegBC.High8;
									break;
								case 0x61: // LD IYH, C
									ClockCycles = 9;
									RegIY.High8 = RegBC.Low8;
									break;
								case 0x62: // LD IYH, D
									ClockCycles = 9;
									RegIY.High8 = RegDE.High8;
									break;
								case 0x63: // LD IYH, E
									ClockCycles = 9;
									RegIY.High8 = RegDE.Low8;
									break;
								case 0x64: // LD IYH, IYH
									ClockCycles = 9;
									break;
								case 0x65: // LD IYH, IYL
									ClockCycles = 9;
									RegIY.High8 = RegIY.Low8;
									break;
								case 0x66: // LD H, (IY+d)
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegHL.High8 = ReadMemory((ushort)(RegIY.Value16 + Displacement));
									break;
								case 0x67: // LD IYH, A
									ClockCycles = 9;
									RegIY.High8 = RegAF.High8;
									break;
								case 0x68: // LD IYL, B
									ClockCycles = 9;
									RegIY.Low8 = RegBC.High8;
									break;
								case 0x69: // LD IYL, C
									ClockCycles = 9;
									RegIY.Low8 = RegBC.Low8;
									break;
								case 0x6A: // LD IYL, D
									ClockCycles = 9;
									RegIY.Low8 = RegDE.High8;
									break;
								case 0x6B: // LD IYL, E
									ClockCycles = 9;
									RegIY.Low8 = RegDE.Low8;
									break;
								case 0x6C: // LD IYL, IYH
									ClockCycles = 9;
									RegIY.Low8 = RegIY.High8;
									break;
								case 0x6D: // LD IYL, IYL
									ClockCycles = 9;
									break;
								case 0x6E: // LD L, (IY+d)
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegHL.Low8 = ReadMemory((ushort)(RegIY.Value16 + Displacement));
									break;
								case 0x6F: // LD IYL, A
									ClockCycles = 9;
									RegIY.Low8 = RegAF.High8;
									break;
								case 0x70: // LD (IY+d), B
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIY.Value16 + Displacement), RegBC.High8);
									break;
								case 0x71: // LD (IY+d), C
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIY.Value16 + Displacement), RegBC.Low8);
									break;
								case 0x72: // LD (IY+d), D
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIY.Value16 + Displacement), RegDE.High8);
									break;
								case 0x73: // LD (IY+d), E
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIY.Value16 + Displacement), RegDE.Low8);
									break;
								case 0x74: // LD (IY+d), H
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIY.Value16 + Displacement), RegHL.High8);
									break;
								case 0x75: // LD (IY+d), L
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIY.Value16 + Displacement), RegHL.Low8);
									break;
								case 0x76: // HALT
									ClockCycles = 4;
									this.Halt();
									break;
								case 0x77: // LD (IY+d), A
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									WriteMemory((ushort)(RegIY.Value16 + Displacement), RegAF.High8);
									break;
								case 0x78: // LD A, B
									ClockCycles = 4;
									RegAF.High8 = RegBC.High8;
									break;
								case 0x79: // LD A, C
									ClockCycles = 4;
									RegAF.High8 = RegBC.Low8;
									break;
								case 0x7A: // LD A, D
									ClockCycles = 4;
									RegAF.High8 = RegDE.High8;
									break;
								case 0x7B: // LD A, E
									ClockCycles = 4;
									RegAF.High8 = RegDE.Low8;
									break;
								case 0x7C: // LD A, IYH
									ClockCycles = 9;
									RegAF.High8 = RegIY.High8;
									break;
								case 0x7D: // LD A, IYL
									ClockCycles = 9;
									RegAF.High8 = RegIY.Low8;
									break;
								case 0x7E: // LD A, (IY+d)
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.High8 = ReadMemory((ushort)(RegIY.Value16 + Displacement));
									break;
								case 0x7F: // LD A, A
									ClockCycles = 4;
									break;
								case 0x80: // ADD A, B
									ClockCycles = 4;
									RegAF.Value16 = TableALU[0, RegAF.High8, RegBC.High8, 0];
									break;
								case 0x81: // ADD A, C
									ClockCycles = 4;
									RegAF.Value16 = TableALU[0, RegAF.High8, RegBC.Low8, 0];
									break;
								case 0x82: // ADD A, D
									ClockCycles = 4;
									RegAF.Value16 = TableALU[0, RegAF.High8, RegDE.High8, 0];
									break;
								case 0x83: // ADD A, E
									ClockCycles = 4;
									RegAF.Value16 = TableALU[0, RegAF.High8, RegDE.Low8, 0];
									break;
								case 0x84: // ADD A, IYH
									ClockCycles = 9;
									RegAF.Value16 = TableALU[0, RegAF.High8, RegIY.High8, 0];
									break;
								case 0x85: // ADD A, IYL
									ClockCycles = 9;
									RegAF.Value16 = TableALU[0, RegAF.High8, RegIY.Low8, 0];
									break;
								case 0x86: // ADD A, (IY+d)
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[0, RegAF.High8, ReadMemory((ushort)(RegIY.Value16 + Displacement)), 0];
									break;
								case 0x87: // ADD A, A
									ClockCycles = 4;
									RegAF.Value16 = TableALU[0, RegAF.High8, RegAF.High8, 0];
									break;
								case 0x88: // ADC A, B
									ClockCycles = 4;
									RegAF.Value16 = TableALU[1, RegAF.High8, RegBC.High8, RegFlagC ? 1 : 0];
									break;
								case 0x89: // ADC A, C
									ClockCycles = 4;
									RegAF.Value16 = TableALU[1, RegAF.High8, RegBC.Low8, RegFlagC ? 1 : 0];
									break;
								case 0x8A: // ADC A, D
									ClockCycles = 4;
									RegAF.Value16 = TableALU[1, RegAF.High8, RegDE.High8, RegFlagC ? 1 : 0];
									break;
								case 0x8B: // ADC A, E
									ClockCycles = 4;
									RegAF.Value16 = TableALU[1, RegAF.High8, RegDE.Low8, RegFlagC ? 1 : 0];
									break;
								case 0x8C: // ADC A, IYH
									ClockCycles = 9;
									RegAF.Value16 = TableALU[1, RegAF.High8, RegIY.High8, RegFlagC ? 1 : 0];
									break;
								case 0x8D: // ADC A, IYL
									ClockCycles = 9;
									RegAF.Value16 = TableALU[1, RegAF.High8, RegIY.Low8, RegFlagC ? 1 : 0];
									break;
								case 0x8E: // ADC A, (IY+d)
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[1, RegAF.High8, ReadMemory((ushort)(RegIY.Value16 + Displacement)), RegFlagC ? 1 : 0];
									break;
								case 0x8F: // ADC A, A
									ClockCycles = 4;
									RegAF.Value16 = TableALU[1, RegAF.High8, RegAF.High8, RegFlagC ? 1 : 0];
									break;
								case 0x90: // SUB B
									ClockCycles = 4;
									RegAF.Value16 = TableALU[2, RegAF.High8, RegBC.High8, 0];
									break;
								case 0x91: // SUB C
									ClockCycles = 4;
									RegAF.Value16 = TableALU[2, RegAF.High8, RegBC.Low8, 0];
									break;
								case 0x92: // SUB D
									ClockCycles = 4;
									RegAF.Value16 = TableALU[2, RegAF.High8, RegDE.High8, 0];
									break;
								case 0x93: // SUB E
									ClockCycles = 4;
									RegAF.Value16 = TableALU[2, RegAF.High8, RegDE.Low8, 0];
									break;
								case 0x94: // SUB IYH
									ClockCycles = 9;
									RegAF.Value16 = TableALU[2, RegAF.High8, RegIY.High8, 0];
									break;
								case 0x95: // SUB IYL
									ClockCycles = 9;
									RegAF.Value16 = TableALU[2, RegAF.High8, RegIY.Low8, 0];
									break;
								case 0x96: // SUB (IY+d)
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[2, RegAF.High8, ReadMemory((ushort)(RegIY.Value16 + Displacement)), 0];
									break;
								case 0x97: // SUB A, A
									ClockCycles = 4;
									RegAF.Value16 = TableALU[2, RegAF.High8, RegAF.High8, 0];
									break;
								case 0x98: // SBC A, B
									ClockCycles = 4;
									RegAF.Value16 = TableALU[3, RegAF.High8, RegBC.High8, RegFlagC ? 1 : 0];
									break;
								case 0x99: // SBC A, C
									ClockCycles = 4;
									RegAF.Value16 = TableALU[3, RegAF.High8, RegBC.Low8, RegFlagC ? 1 : 0];
									break;
								case 0x9A: // SBC A, D
									ClockCycles = 4;
									RegAF.Value16 = TableALU[3, RegAF.High8, RegDE.High8, RegFlagC ? 1 : 0];
									break;
								case 0x9B: // SBC A, E
									ClockCycles = 4;
									RegAF.Value16 = TableALU[3, RegAF.High8, RegDE.Low8, RegFlagC ? 1 : 0];
									break;
								case 0x9C: // SBC A, IYH
									ClockCycles = 9;
									RegAF.Value16 = TableALU[3, RegAF.High8, RegIY.High8, RegFlagC ? 1 : 0];
									break;
								case 0x9D: // SBC A, IYL
									ClockCycles = 9;
									RegAF.Value16 = TableALU[3, RegAF.High8, RegIY.Low8, RegFlagC ? 1 : 0];
									break;
								case 0x9E: // SBC A, (IY+d)
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[3, RegAF.High8, ReadMemory((ushort)(RegIY.Value16 + Displacement)), RegFlagC ? 1 : 0];
									break;
								case 0x9F: // SBC A, A
									ClockCycles = 4;
									RegAF.Value16 = TableALU[3, RegAF.High8, RegAF.High8, RegFlagC ? 1 : 0];
									break;
								case 0xA0: // AND B
									ClockCycles = 4;
									RegAF.Value16 = TableALU[4, RegAF.High8, RegBC.High8, 0];
									break;
								case 0xA1: // AND C
									ClockCycles = 4;
									RegAF.Value16 = TableALU[4, RegAF.High8, RegBC.Low8, 0];
									break;
								case 0xA2: // AND D
									ClockCycles = 4;
									RegAF.Value16 = TableALU[4, RegAF.High8, RegDE.High8, 0];
									break;
								case 0xA3: // AND E
									ClockCycles = 4;
									RegAF.Value16 = TableALU[4, RegAF.High8, RegDE.Low8, 0];
									break;
								case 0xA4: // AND IYH
									ClockCycles = 9;
									RegAF.Value16 = TableALU[4, RegAF.High8, RegIY.High8, 0];
									break;
								case 0xA5: // AND IYL
									ClockCycles = 9;
									RegAF.Value16 = TableALU[4, RegAF.High8, RegIY.Low8, 0];
									break;
								case 0xA6: // AND (IY+d)
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[4, RegAF.High8, ReadMemory((ushort)(RegIY.Value16 + Displacement)), 0];
									break;
								case 0xA7: // AND A
									ClockCycles = 4;
									RegAF.Value16 = TableALU[4, RegAF.High8, RegAF.High8, 0];
									break;
								case 0xA8: // XOR B
									ClockCycles = 4;
									RegAF.Value16 = TableALU[5, RegAF.High8, RegBC.High8, 0];
									break;
								case 0xA9: // XOR C
									ClockCycles = 4;
									RegAF.Value16 = TableALU[5, RegAF.High8, RegBC.Low8, 0];
									break;
								case 0xAA: // XOR D
									ClockCycles = 4;
									RegAF.Value16 = TableALU[5, RegAF.High8, RegDE.High8, 0];
									break;
								case 0xAB: // XOR E
									ClockCycles = 4;
									RegAF.Value16 = TableALU[5, RegAF.High8, RegDE.Low8, 0];
									break;
								case 0xAC: // XOR IYH
									ClockCycles = 9;
									RegAF.Value16 = TableALU[5, RegAF.High8, RegIY.High8, 0];
									break;
								case 0xAD: // XOR IYL
									ClockCycles = 9;
									RegAF.Value16 = TableALU[5, RegAF.High8, RegIY.Low8, 0];
									break;
								case 0xAE: // XOR (IY+d)
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[5, RegAF.High8, ReadMemory((ushort)(RegIY.Value16 + Displacement)), 0];
									break;
								case 0xAF: // XOR A
									ClockCycles = 4;
									RegAF.Value16 = TableALU[5, RegAF.High8, RegAF.High8, 0];
									break;
								case 0xB0: // OR B
									ClockCycles = 4;
									RegAF.Value16 = TableALU[6, RegAF.High8, RegBC.High8, 0];
									break;
								case 0xB1: // OR C
									ClockCycles = 4;
									RegAF.Value16 = TableALU[6, RegAF.High8, RegBC.Low8, 0];
									break;
								case 0xB2: // OR D
									ClockCycles = 4;
									RegAF.Value16 = TableALU[6, RegAF.High8, RegDE.High8, 0];
									break;
								case 0xB3: // OR E
									ClockCycles = 4;
									RegAF.Value16 = TableALU[6, RegAF.High8, RegDE.Low8, 0];
									break;
								case 0xB4: // OR IYH
									ClockCycles = 9;
									RegAF.Value16 = TableALU[6, RegAF.High8, RegIY.High8, 0];
									break;
								case 0xB5: // OR IYL
									ClockCycles = 9;
									RegAF.Value16 = TableALU[6, RegAF.High8, RegIY.Low8, 0];
									break;
								case 0xB6: // OR (IY+d)
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[6, RegAF.High8, ReadMemory((ushort)(RegIY.Value16 + Displacement)), 0];
									break;
								case 0xB7: // OR A
									ClockCycles = 4;
									RegAF.Value16 = TableALU[6, RegAF.High8, RegAF.High8, 0];
									break;
								case 0xB8: // CP B
									ClockCycles = 4;
									RegAF.Value16 = TableALU[7, RegAF.High8, RegBC.High8, 0];
									break;
								case 0xB9: // CP C
									ClockCycles = 4;
									RegAF.Value16 = TableALU[7, RegAF.High8, RegBC.Low8, 0];
									break;
								case 0xBA: // CP D
									ClockCycles = 4;
									RegAF.Value16 = TableALU[7, RegAF.High8, RegDE.High8, 0];
									break;
								case 0xBB: // CP E
									ClockCycles = 4;
									RegAF.Value16 = TableALU[7, RegAF.High8, RegDE.Low8, 0];
									break;
								case 0xBC: // CP IYH
									ClockCycles = 9;
									RegAF.Value16 = TableALU[7, RegAF.High8, RegIY.High8, 0];
									break;
								case 0xBD: // CP IYL
									ClockCycles = 9;
									RegAF.Value16 = TableALU[7, RegAF.High8, RegIY.Low8, 0];
									break;
								case 0xBE: // CP (IY+d)
									ClockCycles = 19;
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									RegAF.Value16 = TableALU[7, RegAF.High8, ReadMemory((ushort)(RegIY.Value16 + Displacement)), 0];
									break;
								case 0xBF: // CP A
									ClockCycles = 4;
									RegAF.Value16 = TableALU[7, RegAF.High8, RegAF.High8, 0];
									break;
								case 0xC0: // RET NZ
									ClockCycles = 11;
									if (!RegFlagZ) {
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									} else {
										ClockCycles = 5;
									}
									break;
								case 0xC1: // POP BC
									ClockCycles = 10;
									RegBC.Low8 = ReadMemory(RegSP.Value16++); RegBC.High8 = ReadMemory(RegSP.Value16++);
									break;
								case 0xC2: // JP NZ, nn
									ClockCycles = 10;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagZ) {
										RegPC.Value16 = TUS;
									} else {
										ClockCycles = 10;
									}
									break;
								case 0xC3: // JP nn
									ClockCycles = 10;
									RegPC.Value16 = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									break;
								case 0xC4: // CALL NZ, nn
									ClockCycles = 17;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagZ) {
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
									} else {
										ClockCycles = 10;
									}
									break;
								case 0xC5: // PUSH BC
									ClockCycles = 11;
									WriteMemory(--RegSP.Value16, RegBC.High8); WriteMemory(--RegSP.Value16, RegBC.Low8);
									break;
								case 0xC6: // ADD A, n
									ClockCycles = 7;
									RegAF.Value16 = TableALU[0, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
									break;
								case 0xC7: // RST $00
									ClockCycles = 11;
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x00;
									break;
								case 0xC8: // RET Z
									ClockCycles = 11;
									if (RegFlagZ) {
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									} else {
										ClockCycles = 5;
									}
									break;
								case 0xC9: // RET
									ClockCycles = 10;
									RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									break;
								case 0xCA: // JP Z, nn
									ClockCycles = 10;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagZ) {
										RegPC.Value16 = TUS;
									} else {
										ClockCycles = 10;
									}
									break;
								case 0xCB: // (Prefix)
									Displacement = (sbyte)ReadMemory(RegPC.Value16++);
									++RegR;
									switch (ReadMemory(RegPC.Value16++)) {
										case 0x00: // RLC (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x01: // RLC (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x02: // RLC (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x03: // RLC (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x04: // RLC (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x05: // RLC (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x06: // RLC (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x07: // RLC (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 0, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x08: // RRC (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x09: // RRC (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x0A: // RRC (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x0B: // RRC (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x0C: // RRC (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x0D: // RRC (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x0E: // RRC (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x0F: // RRC (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 1, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x10: // RL (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x11: // RL (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x12: // RL (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x13: // RL (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x14: // RL (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x15: // RL (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x16: // RL (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x17: // RL (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 2, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x18: // RR (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x19: // RR (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x1A: // RR (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x1B: // RR (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x1C: // RR (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x1D: // RR (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x1E: // RR (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x1F: // RR (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 3, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x20: // SLA (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x21: // SLA (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x22: // SLA (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x23: // SLA (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x24: // SLA (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x25: // SLA (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x26: // SLA (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x27: // SLA (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 4, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x28: // SRA (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x29: // SRA (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x2A: // SRA (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x2B: // SRA (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x2C: // SRA (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x2D: // SRA (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x2E: // SRA (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x2F: // SRA (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 5, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x30: // SL1 (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x31: // SL1 (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x32: // SL1 (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x33: // SL1 (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x34: // SL1 (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x35: // SL1 (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x36: // SL1 (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x37: // SL1 (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 6, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x38: // SRL (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x39: // SRL (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x3A: // SRL (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x3B: // SRL (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x3C: // SRL (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x3D: // SRL (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x3E: // SRL (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x3F: // SRL (IY+d)
											ClockCycles = 23;
											TUS = TableRotShift[1, 7, RegAF.Low8 + 256 * ReadMemory((ushort)(RegIY.Value16 + Displacement))];
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(TUS >> 8));
											RegAF.Low8 = (byte)TUS;
											break;
										case 0x40: // BIT 0, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x41: // BIT 0, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x42: // BIT 0, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x43: // BIT 0, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x44: // BIT 0, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x45: // BIT 0, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x46: // BIT 0, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x47: // BIT 0, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x01) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x48: // BIT 1, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x49: // BIT 1, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x4A: // BIT 1, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x4B: // BIT 1, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x4C: // BIT 1, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x4D: // BIT 1, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x4E: // BIT 1, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x4F: // BIT 1, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x02) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x50: // BIT 2, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x51: // BIT 2, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x52: // BIT 2, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x53: // BIT 2, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x54: // BIT 2, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x55: // BIT 2, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x56: // BIT 2, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x57: // BIT 2, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x04) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x58: // BIT 3, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x59: // BIT 3, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x5A: // BIT 3, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x5B: // BIT 3, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x5C: // BIT 3, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x5D: // BIT 3, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x5E: // BIT 3, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x5F: // BIT 3, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x08) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x60: // BIT 4, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x61: // BIT 4, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x62: // BIT 4, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x63: // BIT 4, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x64: // BIT 4, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x65: // BIT 4, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x66: // BIT 4, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x67: // BIT 4, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x10) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x68: // BIT 5, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x69: // BIT 5, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x6A: // BIT 5, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x6B: // BIT 5, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x6C: // BIT 5, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x6D: // BIT 5, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x6E: // BIT 5, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x6F: // BIT 5, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x20) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x70: // BIT 6, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x71: // BIT 6, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x72: // BIT 6, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x73: // BIT 6, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x74: // BIT 6, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x75: // BIT 6, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x76: // BIT 6, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x77: // BIT 6, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x40) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = false;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x78: // BIT 7, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x79: // BIT 7, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x7A: // BIT 7, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x7B: // BIT 7, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x7C: // BIT 7, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x7D: // BIT 7, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x7E: // BIT 7, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x7F: // BIT 7, (IY+d)
											ClockCycles = 20;
											RegFlagZ = (ReadMemory((ushort)(RegIY.Value16 + Displacement)) & 0x80) == 0;
											RegFlagP = RegFlagZ;
											RegFlagS = !RegFlagZ;
											RegFlagH = true;
											RegFlagN = false;
											break;
										case 0x80: // RES 0, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x01)));
											break;
										case 0x81: // RES 0, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x01)));
											break;
										case 0x82: // RES 0, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x01)));
											break;
										case 0x83: // RES 0, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x01)));
											break;
										case 0x84: // RES 0, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x01)));
											break;
										case 0x85: // RES 0, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x01)));
											break;
										case 0x86: // RES 0, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x01)));
											break;
										case 0x87: // RES 0, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x01)));
											break;
										case 0x88: // RES 1, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x02)));
											break;
										case 0x89: // RES 1, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x02)));
											break;
										case 0x8A: // RES 1, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x02)));
											break;
										case 0x8B: // RES 1, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x02)));
											break;
										case 0x8C: // RES 1, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x02)));
											break;
										case 0x8D: // RES 1, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x02)));
											break;
										case 0x8E: // RES 1, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x02)));
											break;
										case 0x8F: // RES 1, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x02)));
											break;
										case 0x90: // RES 2, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x04)));
											break;
										case 0x91: // RES 2, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x04)));
											break;
										case 0x92: // RES 2, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x04)));
											break;
										case 0x93: // RES 2, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x04)));
											break;
										case 0x94: // RES 2, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x04)));
											break;
										case 0x95: // RES 2, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x04)));
											break;
										case 0x96: // RES 2, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x04)));
											break;
										case 0x97: // RES 2, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x04)));
											break;
										case 0x98: // RES 3, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x08)));
											break;
										case 0x99: // RES 3, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x08)));
											break;
										case 0x9A: // RES 3, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x08)));
											break;
										case 0x9B: // RES 3, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x08)));
											break;
										case 0x9C: // RES 3, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x08)));
											break;
										case 0x9D: // RES 3, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x08)));
											break;
										case 0x9E: // RES 3, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x08)));
											break;
										case 0x9F: // RES 3, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x08)));
											break;
										case 0xA0: // RES 4, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x10)));
											break;
										case 0xA1: // RES 4, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x10)));
											break;
										case 0xA2: // RES 4, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x10)));
											break;
										case 0xA3: // RES 4, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x10)));
											break;
										case 0xA4: // RES 4, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x10)));
											break;
										case 0xA5: // RES 4, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x10)));
											break;
										case 0xA6: // RES 4, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x10)));
											break;
										case 0xA7: // RES 4, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x10)));
											break;
										case 0xA8: // RES 5, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x20)));
											break;
										case 0xA9: // RES 5, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x20)));
											break;
										case 0xAA: // RES 5, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x20)));
											break;
										case 0xAB: // RES 5, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x20)));
											break;
										case 0xAC: // RES 5, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x20)));
											break;
										case 0xAD: // RES 5, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x20)));
											break;
										case 0xAE: // RES 5, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x20)));
											break;
										case 0xAF: // RES 5, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x20)));
											break;
										case 0xB0: // RES 6, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x40)));
											break;
										case 0xB1: // RES 6, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x40)));
											break;
										case 0xB2: // RES 6, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x40)));
											break;
										case 0xB3: // RES 6, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x40)));
											break;
										case 0xB4: // RES 6, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x40)));
											break;
										case 0xB5: // RES 6, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x40)));
											break;
										case 0xB6: // RES 6, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x40)));
											break;
										case 0xB7: // RES 6, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x40)));
											break;
										case 0xB8: // RES 7, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x80)));
											break;
										case 0xB9: // RES 7, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x80)));
											break;
										case 0xBA: // RES 7, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x80)));
											break;
										case 0xBB: // RES 7, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x80)));
											break;
										case 0xBC: // RES 7, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x80)));
											break;
										case 0xBD: // RES 7, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x80)));
											break;
										case 0xBE: // RES 7, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x80)));
											break;
										case 0xBF: // RES 7, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) & unchecked((byte)~0x80)));
											break;
										case 0xC0: // SET 0, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x01)));
											break;
										case 0xC1: // SET 0, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x01)));
											break;
										case 0xC2: // SET 0, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x01)));
											break;
										case 0xC3: // SET 0, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x01)));
											break;
										case 0xC4: // SET 0, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x01)));
											break;
										case 0xC5: // SET 0, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x01)));
											break;
										case 0xC6: // SET 0, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x01)));
											break;
										case 0xC7: // SET 0, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x01)));
											break;
										case 0xC8: // SET 1, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x02)));
											break;
										case 0xC9: // SET 1, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x02)));
											break;
										case 0xCA: // SET 1, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x02)));
											break;
										case 0xCB: // SET 1, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x02)));
											break;
										case 0xCC: // SET 1, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x02)));
											break;
										case 0xCD: // SET 1, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x02)));
											break;
										case 0xCE: // SET 1, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x02)));
											break;
										case 0xCF: // SET 1, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x02)));
											break;
										case 0xD0: // SET 2, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x04)));
											break;
										case 0xD1: // SET 2, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x04)));
											break;
										case 0xD2: // SET 2, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x04)));
											break;
										case 0xD3: // SET 2, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x04)));
											break;
										case 0xD4: // SET 2, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x04)));
											break;
										case 0xD5: // SET 2, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x04)));
											break;
										case 0xD6: // SET 2, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x04)));
											break;
										case 0xD7: // SET 2, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x04)));
											break;
										case 0xD8: // SET 3, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x08)));
											break;
										case 0xD9: // SET 3, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x08)));
											break;
										case 0xDA: // SET 3, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x08)));
											break;
										case 0xDB: // SET 3, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x08)));
											break;
										case 0xDC: // SET 3, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x08)));
											break;
										case 0xDD: // SET 3, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x08)));
											break;
										case 0xDE: // SET 3, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x08)));
											break;
										case 0xDF: // SET 3, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x08)));
											break;
										case 0xE0: // SET 4, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x10)));
											break;
										case 0xE1: // SET 4, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x10)));
											break;
										case 0xE2: // SET 4, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x10)));
											break;
										case 0xE3: // SET 4, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x10)));
											break;
										case 0xE4: // SET 4, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x10)));
											break;
										case 0xE5: // SET 4, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x10)));
											break;
										case 0xE6: // SET 4, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x10)));
											break;
										case 0xE7: // SET 4, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x10)));
											break;
										case 0xE8: // SET 5, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x20)));
											break;
										case 0xE9: // SET 5, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x20)));
											break;
										case 0xEA: // SET 5, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x20)));
											break;
										case 0xEB: // SET 5, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x20)));
											break;
										case 0xEC: // SET 5, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x20)));
											break;
										case 0xED: // SET 5, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x20)));
											break;
										case 0xEE: // SET 5, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x20)));
											break;
										case 0xEF: // SET 5, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x20)));
											break;
										case 0xF0: // SET 6, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x40)));
											break;
										case 0xF1: // SET 6, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x40)));
											break;
										case 0xF2: // SET 6, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x40)));
											break;
										case 0xF3: // SET 6, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x40)));
											break;
										case 0xF4: // SET 6, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x40)));
											break;
										case 0xF5: // SET 6, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x40)));
											break;
										case 0xF6: // SET 6, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x40)));
											break;
										case 0xF7: // SET 6, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x40)));
											break;
										case 0xF8: // SET 7, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x80)));
											break;
										case 0xF9: // SET 7, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x80)));
											break;
										case 0xFA: // SET 7, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x80)));
											break;
										case 0xFB: // SET 7, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x80)));
											break;
										case 0xFC: // SET 7, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x80)));
											break;
										case 0xFD: // SET 7, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x80)));
											break;
										case 0xFE: // SET 7, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x80)));
											break;
										case 0xFF: // SET 7, (IY+d)
											ClockCycles = 23;
											WriteMemory((ushort)(RegIY.Value16 + Displacement), (byte)(ReadMemory((ushort)(RegIY.Value16 + Displacement)) | unchecked((byte)0x80)));
											break;
									}
									break;
								case 0xCC: // CALL Z, nn
									ClockCycles = 17;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagZ) {
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
									} else {
										ClockCycles = 10;
									}
									break;
								case 0xCD: // CALL nn
									ClockCycles = 17;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = TUS;
									break;
								case 0xCE: // ADC A, n
									ClockCycles = 7;
									RegAF.Value16 = TableALU[1, RegAF.High8, ReadMemory(RegPC.Value16++), RegFlagC ? 1 : 0];
									break;
								case 0xCF: // RST $08
									ClockCycles = 11;
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x08;
									break;
								case 0xD0: // RET NC
									ClockCycles = 11;
									if (!RegFlagC) {
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									} else {
										ClockCycles = 5;
									}
									break;
								case 0xD1: // POP DE
									ClockCycles = 10;
									RegDE.Low8 = ReadMemory(RegSP.Value16++); RegDE.High8 = ReadMemory(RegSP.Value16++);
									break;
								case 0xD2: // JP NC, nn
									ClockCycles = 10;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagC) {
										RegPC.Value16 = TUS;
									} else {
										ClockCycles = 10;
									}
									break;
								case 0xD3: // OUT n, A
									ClockCycles = 11;
									WriteHardware(ReadMemory(RegPC.Value16++), RegAF.High8);
									break;
								case 0xD4: // CALL NC, nn
									ClockCycles = 17;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagC) {
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
									} else {
										ClockCycles = 10;
									}
									break;
								case 0xD5: // PUSH DE
									ClockCycles = 11;
									WriteMemory(--RegSP.Value16, RegDE.High8); WriteMemory(--RegSP.Value16, RegDE.Low8);
									break;
								case 0xD6: // SUB n
									ClockCycles = 7;
									RegAF.Value16 = TableALU[2, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
									break;
								case 0xD7: // RST $10
									ClockCycles = 11;
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x10;
									break;
								case 0xD8: // RET C
									ClockCycles = 11;
									if (RegFlagC) {
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									} else {
										ClockCycles = 5;
									}
									break;
								case 0xD9: // EXX
									ClockCycles = 4;
									TUS = RegBC.Value16; RegBC.Value16 = RegAltBC.Value16; RegAltBC.Value16 = TUS;
									TUS = RegDE.Value16; RegDE.Value16 = RegAltDE.Value16; RegAltDE.Value16 = TUS;
									TUS = RegHL.Value16; RegHL.Value16 = RegAltHL.Value16; RegAltHL.Value16 = TUS;
									break;
								case 0xDA: // JP C, nn
									ClockCycles = 10;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagC) {
										RegPC.Value16 = TUS;
									} else {
										ClockCycles = 10;
									}
									break;
								case 0xDB: // IN A, n
									ClockCycles = 11;
									RegAF.High8 = ReadHardware((ushort)ReadMemory(RegPC.Value16++));
									break;
								case 0xDC: // CALL C, nn
									ClockCycles = 17;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagC) {
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
									} else {
										ClockCycles = 10;
									}
									break;
								case 0xDD: // <-
									ClockCycles = 1337;
									// Invalid sequence.
									break;
								case 0xDE: // SBC A, n
									ClockCycles = 7;
									RegAF.Value16 = TableALU[3, RegAF.High8, ReadMemory(RegPC.Value16++), RegFlagC ? 1 : 0];
									break;
								case 0xDF: // RST $18
									ClockCycles = 11;
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x18;
									break;
								case 0xE0: // RET PO
									ClockCycles = 11;
									if (!RegFlagP) {
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									} else {
										ClockCycles = 5;
									}
									break;
								case 0xE1: // POP IY
									ClockCycles = 14;
									RegIY.Low8 = ReadMemory(RegSP.Value16++); RegIY.High8 = ReadMemory(RegSP.Value16++);
									break;
								case 0xE2: // JP PO, nn
									ClockCycles = 10;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagP) {
										RegPC.Value16 = TUS;
									} else {
										ClockCycles = 10;
									}
									break;
								case 0xE3: // EX (SP), IY
									ClockCycles = 23;
									TUS = RegSP.Value16; TBL = ReadMemory(TUS++); TBH = ReadMemory(TUS--);
									WriteMemory(TUS++, RegIY.Low8); WriteMemory(TUS, RegIY.High8);
									RegIY.Low8 = TBL; RegIY.High8 = TBH;
									break;
								case 0xE4: // CALL C, nn
									ClockCycles = 17;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagC) {
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
									} else {
										ClockCycles = 10;
									}
									break;
								case 0xE5: // PUSH IY
									ClockCycles = 15;
									WriteMemory(--RegSP.Value16, RegIY.High8); WriteMemory(--RegSP.Value16, RegIY.Low8);
									break;
								case 0xE6: // AND n
									ClockCycles = 7;
									RegAF.Value16 = TableALU[4, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
									break;
								case 0xE7: // RST $20
									ClockCycles = 11;
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x20;
									break;
								case 0xE8: // RET PE
									ClockCycles = 11;
									if (RegFlagP) {
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									} else {
										ClockCycles = 5;
									}
									break;
								case 0xE9: // JP IY
									ClockCycles = 8;
									RegPC.Value16 = RegIY.Value16;
									break;
								case 0xEA: // JP PE, nn
									ClockCycles = 10;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagP) {
										RegPC.Value16 = TUS;
									} else {
										ClockCycles = 10;
									}
									break;
								case 0xEB: // EX DE, HL
									ClockCycles = 4;
									TUS = RegDE.Value16; RegDE.Value16 = RegHL.Value16; RegHL.Value16 = TUS;
									break;
								case 0xEC: // CALL PE, nn
									ClockCycles = 17;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagP) {
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
									} else {
										ClockCycles = 10;
									}
									break;
								case 0xED: // (Prefix)
									++RegR;
									switch (ReadMemory(RegPC.Value16++)) {
										case 0x00: // NOP
											ClockCycles = 4;
											break;
										case 0x01: // NOP
											ClockCycles = 4;
											break;
										case 0x02: // NOP
											ClockCycles = 4;
											break;
										case 0x03: // NOP
											ClockCycles = 4;
											break;
										case 0x04: // NOP
											ClockCycles = 4;
											break;
										case 0x05: // NOP
											ClockCycles = 4;
											break;
										case 0x06: // NOP
											ClockCycles = 4;
											break;
										case 0x07: // NOP
											ClockCycles = 4;
											break;
										case 0x08: // NOP
											ClockCycles = 4;
											break;
										case 0x09: // NOP
											ClockCycles = 4;
											break;
										case 0x0A: // NOP
											ClockCycles = 4;
											break;
										case 0x0B: // NOP
											ClockCycles = 4;
											break;
										case 0x0C: // NOP
											ClockCycles = 4;
											break;
										case 0x0D: // NOP
											ClockCycles = 4;
											break;
										case 0x0E: // NOP
											ClockCycles = 4;
											break;
										case 0x0F: // NOP
											ClockCycles = 4;
											break;
										case 0x10: // NOP
											ClockCycles = 4;
											break;
										case 0x11: // NOP
											ClockCycles = 4;
											break;
										case 0x12: // NOP
											ClockCycles = 4;
											break;
										case 0x13: // NOP
											ClockCycles = 4;
											break;
										case 0x14: // NOP
											ClockCycles = 4;
											break;
										case 0x15: // NOP
											ClockCycles = 4;
											break;
										case 0x16: // NOP
											ClockCycles = 4;
											break;
										case 0x17: // NOP
											ClockCycles = 4;
											break;
										case 0x18: // NOP
											ClockCycles = 4;
											break;
										case 0x19: // NOP
											ClockCycles = 4;
											break;
										case 0x1A: // NOP
											ClockCycles = 4;
											break;
										case 0x1B: // NOP
											ClockCycles = 4;
											break;
										case 0x1C: // NOP
											ClockCycles = 4;
											break;
										case 0x1D: // NOP
											ClockCycles = 4;
											break;
										case 0x1E: // NOP
											ClockCycles = 4;
											break;
										case 0x1F: // NOP
											ClockCycles = 4;
											break;
										case 0x20: // NOP
											ClockCycles = 4;
											break;
										case 0x21: // NOP
											ClockCycles = 4;
											break;
										case 0x22: // NOP
											ClockCycles = 4;
											break;
										case 0x23: // NOP
											ClockCycles = 4;
											break;
										case 0x24: // NOP
											ClockCycles = 4;
											break;
										case 0x25: // NOP
											ClockCycles = 4;
											break;
										case 0x26: // NOP
											ClockCycles = 4;
											break;
										case 0x27: // NOP
											ClockCycles = 4;
											break;
										case 0x28: // NOP
											ClockCycles = 4;
											break;
										case 0x29: // NOP
											ClockCycles = 4;
											break;
										case 0x2A: // NOP
											ClockCycles = 4;
											break;
										case 0x2B: // NOP
											ClockCycles = 4;
											break;
										case 0x2C: // NOP
											ClockCycles = 4;
											break;
										case 0x2D: // NOP
											ClockCycles = 4;
											break;
										case 0x2E: // NOP
											ClockCycles = 4;
											break;
										case 0x2F: // NOP
											ClockCycles = 4;
											break;
										case 0x30: // NOP
											ClockCycles = 4;
											break;
										case 0x31: // NOP
											ClockCycles = 4;
											break;
										case 0x32: // NOP
											ClockCycles = 4;
											break;
										case 0x33: // NOP
											ClockCycles = 4;
											break;
										case 0x34: // NOP
											ClockCycles = 4;
											break;
										case 0x35: // NOP
											ClockCycles = 4;
											break;
										case 0x36: // NOP
											ClockCycles = 4;
											break;
										case 0x37: // NOP
											ClockCycles = 4;
											break;
										case 0x38: // NOP
											ClockCycles = 4;
											break;
										case 0x39: // NOP
											ClockCycles = 4;
											break;
										case 0x3A: // NOP
											ClockCycles = 4;
											break;
										case 0x3B: // NOP
											ClockCycles = 4;
											break;
										case 0x3C: // NOP
											ClockCycles = 4;
											break;
										case 0x3D: // NOP
											ClockCycles = 4;
											break;
										case 0x3E: // NOP
											ClockCycles = 4;
											break;
										case 0x3F: // NOP
											ClockCycles = 4;
											break;
										case 0x40: // IN B, C
											ClockCycles = 12;
											RegBC.High8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegBC.High8 > 127;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegBC.High8];
											RegFlagN = false;
											break;
										case 0x41: // OUT C, B
											ClockCycles = 12;
											WriteHardware(RegBC.Low8, RegBC.High8);
											break;
										case 0x42: // SBC HL, BC
											ClockCycles = 15;
											TI1 = (short)RegHL.Value16; TI2 = (short)RegBC.Value16; TIR = TI1 - TI2;
											if (RegFlagC) { --TIR; ++TI2; }
											TUS = (ushort)TIR;
											RegFlagH = ((TI1 & 0xFFF) - (TI2 & 0xFFF)) < 0;
											RegFlagN = true;
											RegFlagC = ((ushort)TI1 - (ushort)TI2) < 0;
											RegFlagP = TIR > 32767 || TIR < -32768;
											RegFlagS = TUS > 32767;
											RegFlagZ = TUS == 0;
											RegHL.Value16 = TUS;
											break;
										case 0x43: // LD (nn), BC
											ClockCycles = 20;
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											WriteMemory(TUS++, RegBC.Low8);
											WriteMemory(TUS, RegBC.High8);
											break;
										case 0x44: // NEG
											ClockCycles = 8;
											RegAF.Value16 = TableNeg[RegAF.Value16];
											break;
										case 0x45: // RETN
											ClockCycles = 14;
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											this.IFF1 = this.IFF2;
											break;
										case 0x46: // IM $0
											ClockCycles = 8;
											interruptMode = 0;
											break;
										case 0x47: // LD I, A
											ClockCycles = 9;
											RegI = RegAF.High8;
											break;
										case 0x48: // IN C, C
											ClockCycles = 12;
											RegBC.Low8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegBC.Low8 > 127;
											RegFlagZ = RegBC.Low8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegBC.Low8];
											RegFlagN = false;
											break;
										case 0x49: // OUT C, C
											ClockCycles = 12;
											WriteHardware(RegBC.Low8, RegBC.Low8);
											break;
										case 0x4A: // ADC HL, BC
											ClockCycles = 15;
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
											break;
										case 0x4B: // LD BC, (nn)
											ClockCycles = 20;
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											RegBC.Low8 = ReadMemory(TUS++); RegBC.High8 = ReadMemory(TUS);
											break;
										case 0x4C: // NEG
											ClockCycles = 8;
											RegAF.Value16 = TableNeg[RegAF.Value16];
											break;
										case 0x4D: // RETI
											ClockCycles = 14;
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											break;
										case 0x4E: // IM $0
											ClockCycles = 8;
											interruptMode = 0;
											break;
										case 0x4F: // LD R, A
											ClockCycles = 9;
											RegR = RegAF.High8;
											break;
										case 0x50: // IN D, C
											ClockCycles = 12;
											RegDE.High8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegDE.High8 > 127;
											RegFlagZ = RegDE.High8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegDE.High8];
											RegFlagN = false;
											break;
										case 0x51: // OUT C, D
											ClockCycles = 12;
											WriteHardware(RegBC.Low8, RegDE.High8);
											break;
										case 0x52: // SBC HL, DE
											ClockCycles = 15;
											TI1 = (short)RegHL.Value16; TI2 = (short)RegDE.Value16; TIR = TI1 - TI2;
											if (RegFlagC) { --TIR; ++TI2; }
											TUS = (ushort)TIR;
											RegFlagH = ((TI1 & 0xFFF) - (TI2 & 0xFFF)) < 0;
											RegFlagN = true;
											RegFlagC = ((ushort)TI1 - (ushort)TI2) < 0;
											RegFlagP = TIR > 32767 || TIR < -32768;
											RegFlagS = TUS > 32767;
											RegFlagZ = TUS == 0;
											RegHL.Value16 = TUS;
											break;
										case 0x53: // LD (nn), DE
											ClockCycles = 20;
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											WriteMemory(TUS++, RegDE.Low8);
											WriteMemory(TUS, RegDE.High8);
											break;
										case 0x54: // NEG
											ClockCycles = 8;
											RegAF.Value16 = TableNeg[RegAF.Value16];
											break;
										case 0x55: // RETN
											ClockCycles = 14;
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											this.IFF1 = this.IFF2;
											break;
										case 0x56: // IM $1
											ClockCycles = 8;
											interruptMode = 1;
											break;
										case 0x57: // LD A, I
											ClockCycles = 9;
											RegAF.High8 = RegI;
											break;
										case 0x58: // IN E, C
											ClockCycles = 12;
											RegDE.Low8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegDE.Low8 > 127;
											RegFlagZ = RegDE.Low8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegDE.Low8];
											RegFlagN = false;
											break;
										case 0x59: // OUT C, E
											ClockCycles = 12;
											WriteHardware(RegBC.Low8, RegDE.Low8);
											break;
										case 0x5A: // ADC HL, DE
											ClockCycles = 15;
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
											break;
										case 0x5B: // LD DE, (nn)
											ClockCycles = 20;
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											RegDE.Low8 = ReadMemory(TUS++); RegDE.High8 = ReadMemory(TUS);
											break;
										case 0x5C: // NEG
											ClockCycles = 8;
											RegAF.Value16 = TableNeg[RegAF.Value16];
											break;
										case 0x5D: // RETI
											ClockCycles = 14;
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											break;
										case 0x5E: // IM $2
											ClockCycles = 8;
											interruptMode = 2;
											break;
										case 0x5F: // LD A, R
											ClockCycles = 9;
											RegAF.High8 = (byte)(RegR & 0x7F);
											break;
										case 0x60: // IN H, C
											ClockCycles = 12;
											RegHL.High8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegHL.High8 > 127;
											RegFlagZ = RegHL.High8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegHL.High8];
											RegFlagN = false;
											break;
										case 0x61: // OUT C, H
											ClockCycles = 12;
											WriteHardware(RegBC.Low8, RegHL.High8);
											break;
										case 0x62: // SBC HL, HL
											ClockCycles = 15;
											TI1 = (short)RegHL.Value16; TI2 = (short)RegHL.Value16; TIR = TI1 - TI2;
											if (RegFlagC) { --TIR; ++TI2; }
											TUS = (ushort)TIR;
											RegFlagH = ((TI1 & 0xFFF) - (TI2 & 0xFFF)) < 0;
											RegFlagN = true;
											RegFlagC = ((ushort)TI1 - (ushort)TI2) < 0;
											RegFlagP = TIR > 32767 || TIR < -32768;
											RegFlagS = TUS > 32767;
											RegFlagZ = TUS == 0;
											RegHL.Value16 = TUS;
											break;
										case 0x63: // LD (nn), HL
											ClockCycles = 20;
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											WriteMemory(TUS++, RegHL.Low8);
											WriteMemory(TUS, RegHL.High8);
											break;
										case 0x64: // NEG
											ClockCycles = 8;
											RegAF.Value16 = TableNeg[RegAF.Value16];
											break;
										case 0x65: // RETN
											ClockCycles = 14;
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											this.IFF1 = this.IFF2;
											break;
										case 0x66: // IM $0
											ClockCycles = 8;
											interruptMode = 0;
											break;
										case 0x67: // RRD
											ClockCycles = 18;
											TB1 = RegAF.High8; TB2 = ReadMemory(RegHL.Value16);
											WriteMemory(RegHL.Value16, (byte)((TB2 >> 4) + (TB1 << 4)));
											RegAF.High8 = (byte)((TB1 & 0xF0) + (TB2 & 0x0F));
											RegFlagS = RegAF.High8 > 127;
											RegFlagZ = RegAF.High8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegAF.High8];
											RegFlagN = false;
											break;
										case 0x68: // IN L, C
											ClockCycles = 12;
											RegHL.Low8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegHL.Low8 > 127;
											RegFlagZ = RegHL.Low8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegHL.Low8];
											RegFlagN = false;
											break;
										case 0x69: // OUT C, L
											ClockCycles = 12;
											WriteHardware(RegBC.Low8, RegHL.Low8);
											break;
										case 0x6A: // ADC HL, HL
											ClockCycles = 15;
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
											break;
										case 0x6B: // LD HL, (nn)
											ClockCycles = 20;
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											RegHL.Low8 = ReadMemory(TUS++); RegHL.High8 = ReadMemory(TUS);
											break;
										case 0x6C: // NEG
											ClockCycles = 8;
											RegAF.Value16 = TableNeg[RegAF.Value16];
											break;
										case 0x6D: // RETI
											ClockCycles = 14;
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											break;
										case 0x6E: // IM $0
											ClockCycles = 8;
											interruptMode = 0;
											break;
										case 0x6F: // RLD
											ClockCycles = 18;
											TB1 = RegAF.High8; TB2 = ReadMemory(RegHL.Value16);
											WriteMemory(RegHL.Value16, (byte)((TB1 & 0x0F) + (TB2 << 4)));
											RegAF.High8 = (byte)((TB1 & 0xF0) + (TB2 >> 4));
											RegFlagS = RegAF.High8 > 127;
											RegFlagZ = RegAF.High8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegAF.High8];
											RegFlagN = false;
											break;
										case 0x70: // IN 0, C
											ClockCycles = 12;
											TB = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = TB > 127;
											RegFlagZ = TB == 0;
											RegFlagH = false;
											RegFlagP = TableParity[TB];
											RegFlagN = false;
											break;
										case 0x71: // OUT C, 0
											ClockCycles = 12;
											WriteHardware(RegBC.Low8, 0);
											break;
										case 0x72: // SBC HL, SP
											ClockCycles = 15;
											TI1 = (short)RegHL.Value16; TI2 = (short)RegSP.Value16; TIR = TI1 - TI2;
											if (RegFlagC) { --TIR; ++TI2; }
											TUS = (ushort)TIR;
											RegFlagH = ((TI1 & 0xFFF) - (TI2 & 0xFFF)) < 0;
											RegFlagN = true;
											RegFlagC = ((ushort)TI1 - (ushort)TI2) < 0;
											RegFlagP = TIR > 32767 || TIR < -32768;
											RegFlagS = TUS > 32767;
											RegFlagZ = TUS == 0;
											RegHL.Value16 = TUS;
											break;
										case 0x73: // LD (nn), SP
											ClockCycles = 20;
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											WriteMemory(TUS++, RegSP.Low8);
											WriteMemory(TUS, RegSP.High8);
											break;
										case 0x74: // NEG
											ClockCycles = 8;
											RegAF.Value16 = TableNeg[RegAF.Value16];
											break;
										case 0x75: // RETN
											ClockCycles = 14;
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											this.IFF1 = this.IFF2;
											break;
										case 0x76: // IM $1
											ClockCycles = 8;
											interruptMode = 1;
											break;
										case 0x77: // NOP
											ClockCycles = 4;
											break;
										case 0x78: // IN A, C
											ClockCycles = 12;
											RegAF.High8 = ReadHardware((ushort)RegBC.Low8);
											RegFlagS = RegAF.High8 > 127;
											RegFlagZ = RegAF.High8 == 0;
											RegFlagH = false;
											RegFlagP = TableParity[RegAF.High8];
											RegFlagN = false;
											break;
										case 0x79: // OUT C, A
											ClockCycles = 12;
											WriteHardware(RegBC.Low8, RegAF.High8);
											break;
										case 0x7A: // ADC HL, SP
											ClockCycles = 15;
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
											break;
										case 0x7B: // LD SP, (nn)
											ClockCycles = 20;
											TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
											RegSP.Low8 = ReadMemory(TUS++); RegSP.High8 = ReadMemory(TUS);
											break;
										case 0x7C: // NEG
											ClockCycles = 8;
											RegAF.Value16 = TableNeg[RegAF.Value16];
											break;
										case 0x7D: // RETI
											ClockCycles = 14;
											RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
											break;
										case 0x7E: // IM $2
											ClockCycles = 8;
											interruptMode = 2;
											break;
										case 0x7F: // NOP
											ClockCycles = 4;
											break;
										case 0x80: // NOP
											ClockCycles = 4;
											break;
										case 0x81: // NOP
											ClockCycles = 4;
											break;
										case 0x82: // NOP
											ClockCycles = 4;
											break;
										case 0x83: // NOP
											ClockCycles = 4;
											break;
										case 0x84: // NOP
											ClockCycles = 4;
											break;
										case 0x85: // NOP
											ClockCycles = 4;
											break;
										case 0x86: // NOP
											ClockCycles = 4;
											break;
										case 0x87: // NOP
											ClockCycles = 4;
											break;
										case 0x88: // NOP
											ClockCycles = 4;
											break;
										case 0x89: // NOP
											ClockCycles = 4;
											break;
										case 0x8A: // NOP
											ClockCycles = 4;
											break;
										case 0x8B: // NOP
											ClockCycles = 4;
											break;
										case 0x8C: // NOP
											ClockCycles = 4;
											break;
										case 0x8D: // NOP
											ClockCycles = 4;
											break;
										case 0x8E: // NOP
											ClockCycles = 4;
											break;
										case 0x8F: // NOP
											ClockCycles = 4;
											break;
										case 0x90: // NOP
											ClockCycles = 4;
											break;
										case 0x91: // NOP
											ClockCycles = 4;
											break;
										case 0x92: // NOP
											ClockCycles = 4;
											break;
										case 0x93: // NOP
											ClockCycles = 4;
											break;
										case 0x94: // NOP
											ClockCycles = 4;
											break;
										case 0x95: // NOP
											ClockCycles = 4;
											break;
										case 0x96: // NOP
											ClockCycles = 4;
											break;
										case 0x97: // NOP
											ClockCycles = 4;
											break;
										case 0x98: // NOP
											ClockCycles = 4;
											break;
										case 0x99: // NOP
											ClockCycles = 4;
											break;
										case 0x9A: // NOP
											ClockCycles = 4;
											break;
										case 0x9B: // NOP
											ClockCycles = 4;
											break;
										case 0x9C: // NOP
											ClockCycles = 4;
											break;
										case 0x9D: // NOP
											ClockCycles = 4;
											break;
										case 0x9E: // NOP
											ClockCycles = 4;
											break;
										case 0x9F: // NOP
											ClockCycles = 4;
											break;
										case 0xA0: // LDI
											ClockCycles = 16;
											WriteMemory(RegDE.Value16++, ReadMemory(RegHL.Value16++));
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											RegFlagH = false;
											RegFlagN = false;
											break;
										case 0xA1: // CPI
											ClockCycles = 16;
											TB1 = ReadMemory(RegHL.Value16++); TB2 = (byte)(RegAF.High8 - TB1);
											RegFlagN = true;
											RegFlagH = TableHalfBorrow[RegAF.High8, TB1];
											RegFlagZ = TB2 == 0;
											RegFlagS = TB2 > 127;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											break;
										case 0xA2: // INI
											ClockCycles = 16;
											WriteMemory(RegHL.Value16++, ReadHardware(RegBC.Value16));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											break;
										case 0xA3: // OUTI
											ClockCycles = 16;
											WriteHardware(RegBC.Value16, ReadMemory(RegHL.Value16++));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											break;
										case 0xA4: // NOP
											ClockCycles = 4;
											break;
										case 0xA5: // NOP
											ClockCycles = 4;
											break;
										case 0xA6: // NOP
											ClockCycles = 4;
											break;
										case 0xA7: // NOP
											ClockCycles = 4;
											break;
										case 0xA8: // LDD
											ClockCycles = 16;
											WriteMemory(RegDE.Value16--, ReadMemory(RegHL.Value16--));
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											RegFlagH = false;
											RegFlagN = false;
											break;
										case 0xA9: // CPD
											ClockCycles = 16;
											TB1 = ReadMemory(RegHL.Value16--); TB2 = (byte)(RegAF.High8 - TB1);
											RegFlagN = true;
											RegFlagH = TableHalfBorrow[RegAF.High8, TB1];
											RegFlagZ = TB2 == 0;
											RegFlagS = TB2 > 127;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											break;
										case 0xAA: // IND
											ClockCycles = 16;
											WriteMemory(RegHL.Value16--, ReadHardware(RegBC.Value16));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											break;
										case 0xAB: // OUTD
											ClockCycles = 16;
											WriteHardware(RegBC.Value16, ReadMemory(RegHL.Value16--));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											break;
										case 0xAC: // NOP
											ClockCycles = 4;
											break;
										case 0xAD: // NOP
											ClockCycles = 4;
											break;
										case 0xAE: // NOP
											ClockCycles = 4;
											break;
										case 0xAF: // NOP
											ClockCycles = 4;
											break;
										case 0xB0: // LDIR
											ClockCycles = 21;
											WriteMemory(RegDE.Value16++, ReadMemory(RegHL.Value16++));
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											RegFlagH = false;
											RegFlagN = false;
											if (RegBC.Value16 != 0) {
												RegPC.Value16 -= 2;
											} else {
												ClockCycles = 16;
											}
											break;
										case 0xB1: // CPIR
											ClockCycles = 21;
											TB1 = ReadMemory(RegHL.Value16++); TB2 = (byte)(RegAF.High8 - TB1);
											RegFlagN = true;
											RegFlagH = TableHalfBorrow[RegAF.High8, TB1];
											RegFlagZ = TB2 == 0;
											RegFlagS = TB2 > 127;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											if (RegBC.Value16 != 0 && !RegFlagZ) {
												RegPC.Value16 -= 2;
											} else {
												ClockCycles = 16;
											}
											break;
										case 0xB2: // INIR
											ClockCycles = 21;
											WriteMemory(RegHL.Value16++, ReadHardware(RegBC.Value16));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											if (RegBC.High8 != 0) {
												RegPC.Value16 -= 2;
											} else {
												ClockCycles = 16;
											}
											break;
										case 0xB3: // OTIR
											ClockCycles = 21;
											WriteHardware(RegBC.Value16, ReadMemory(RegHL.Value16++));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											if (RegBC.High8 != 0) {
												RegPC.Value16 -= 2;
											} else {
												ClockCycles = 16;
											}
											break;
										case 0xB4: // NOP
											ClockCycles = 4;
											break;
										case 0xB5: // NOP
											ClockCycles = 4;
											break;
										case 0xB6: // NOP
											ClockCycles = 4;
											break;
										case 0xB7: // NOP
											ClockCycles = 4;
											break;
										case 0xB8: // LDDR
											ClockCycles = 21;
											WriteMemory(RegDE.Value16--, ReadMemory(RegHL.Value16--));
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											RegFlagH = false;
											RegFlagN = false;
											if (RegBC.Value16 != 0) {
												RegPC.Value16 -= 2;
											} else {
												ClockCycles = 16;
											}
											break;
										case 0xB9: // CPDR
											ClockCycles = 21;
											TB1 = ReadMemory(RegHL.Value16--); TB2 = (byte)(RegAF.High8 - TB1);
											RegFlagN = true;
											RegFlagH = TableHalfBorrow[RegAF.High8, TB1];
											RegFlagZ = TB2 == 0;
											RegFlagS = TB2 > 127;
											--RegBC.Value16;
											RegFlagP = RegBC.Value16 != 0;
											if (RegBC.Value16 != 0 && !RegFlagZ) {
												RegPC.Value16 -= 2;
											} else {
												ClockCycles = 16;
											}
											break;
										case 0xBA: // INDR
											ClockCycles = 21;
											WriteMemory(RegHL.Value16--, ReadHardware(RegBC.Value16));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											if (RegBC.High8 != 0) {
												RegPC.Value16 -= 2;
											} else {
												ClockCycles = 16;
											}
											break;
										case 0xBB: // OTDR
											ClockCycles = 21;
											WriteHardware(RegBC.Value16, ReadMemory(RegHL.Value16--));
											--RegBC.High8;
											RegFlagZ = RegBC.High8 == 0;
											RegFlagN = true;
											if (RegBC.High8 != 0) {
												RegPC.Value16 -= 2;
											} else {
												ClockCycles = 16;
											}
											break;
										case 0xBC: // NOP
											ClockCycles = 4;
											break;
										case 0xBD: // NOP
											ClockCycles = 4;
											break;
										case 0xBE: // NOP
											ClockCycles = 4;
											break;
										case 0xBF: // NOP
											ClockCycles = 4;
											break;
										case 0xC0: // NOP
											ClockCycles = 4;
											break;
										case 0xC1: // NOP
											ClockCycles = 4;
											break;
										case 0xC2: // NOP
											ClockCycles = 4;
											break;
										case 0xC3: // NOP
											ClockCycles = 4;
											break;
										case 0xC4: // NOP
											ClockCycles = 4;
											break;
										case 0xC5: // NOP
											ClockCycles = 4;
											break;
										case 0xC6: // NOP
											ClockCycles = 4;
											break;
										case 0xC7: // NOP
											ClockCycles = 4;
											break;
										case 0xC8: // NOP
											ClockCycles = 4;
											break;
										case 0xC9: // NOP
											ClockCycles = 4;
											break;
										case 0xCA: // NOP
											ClockCycles = 4;
											break;
										case 0xCB: // NOP
											ClockCycles = 4;
											break;
										case 0xCC: // NOP
											ClockCycles = 4;
											break;
										case 0xCD: // NOP
											ClockCycles = 4;
											break;
										case 0xCE: // NOP
											ClockCycles = 4;
											break;
										case 0xCF: // NOP
											ClockCycles = 4;
											break;
										case 0xD0: // NOP
											ClockCycles = 4;
											break;
										case 0xD1: // NOP
											ClockCycles = 4;
											break;
										case 0xD2: // NOP
											ClockCycles = 4;
											break;
										case 0xD3: // NOP
											ClockCycles = 4;
											break;
										case 0xD4: // NOP
											ClockCycles = 4;
											break;
										case 0xD5: // NOP
											ClockCycles = 4;
											break;
										case 0xD6: // NOP
											ClockCycles = 4;
											break;
										case 0xD7: // NOP
											ClockCycles = 4;
											break;
										case 0xD8: // NOP
											ClockCycles = 4;
											break;
										case 0xD9: // NOP
											ClockCycles = 4;
											break;
										case 0xDA: // NOP
											ClockCycles = 4;
											break;
										case 0xDB: // NOP
											ClockCycles = 4;
											break;
										case 0xDC: // NOP
											ClockCycles = 4;
											break;
										case 0xDD: // NOP
											ClockCycles = 4;
											break;
										case 0xDE: // NOP
											ClockCycles = 4;
											break;
										case 0xDF: // NOP
											ClockCycles = 4;
											break;
										case 0xE0: // NOP
											ClockCycles = 4;
											break;
										case 0xE1: // NOP
											ClockCycles = 4;
											break;
										case 0xE2: // NOP
											ClockCycles = 4;
											break;
										case 0xE3: // NOP
											ClockCycles = 4;
											break;
										case 0xE4: // NOP
											ClockCycles = 4;
											break;
										case 0xE5: // NOP
											ClockCycles = 4;
											break;
										case 0xE6: // NOP
											ClockCycles = 4;
											break;
										case 0xE7: // NOP
											ClockCycles = 4;
											break;
										case 0xE8: // NOP
											ClockCycles = 4;
											break;
										case 0xE9: // NOP
											ClockCycles = 4;
											break;
										case 0xEA: // NOP
											ClockCycles = 4;
											break;
										case 0xEB: // NOP
											ClockCycles = 4;
											break;
										case 0xEC: // NOP
											ClockCycles = 4;
											break;
										case 0xED: // NOP
											ClockCycles = 4;
											break;
										case 0xEE: // NOP
											ClockCycles = 4;
											break;
										case 0xEF: // NOP
											ClockCycles = 4;
											break;
										case 0xF0: // NOP
											ClockCycles = 4;
											break;
										case 0xF1: // NOP
											ClockCycles = 4;
											break;
										case 0xF2: // NOP
											ClockCycles = 4;
											break;
										case 0xF3: // NOP
											ClockCycles = 4;
											break;
										case 0xF4: // NOP
											ClockCycles = 4;
											break;
										case 0xF5: // NOP
											ClockCycles = 4;
											break;
										case 0xF6: // NOP
											ClockCycles = 4;
											break;
										case 0xF7: // NOP
											ClockCycles = 4;
											break;
										case 0xF8: // NOP
											ClockCycles = 4;
											break;
										case 0xF9: // NOP
											ClockCycles = 4;
											break;
										case 0xFA: // NOP
											ClockCycles = 4;
											break;
										case 0xFB: // NOP
											ClockCycles = 4;
											break;
										case 0xFC: // NOP
											ClockCycles = 4;
											break;
										case 0xFD: // NOP
											ClockCycles = 4;
											break;
										case 0xFE: // NOP
											ClockCycles = 4;
											break;
										case 0xFF: // NOP
											ClockCycles = 4;
											break;
									}
									break;
								case 0xEE: // XOR n
									ClockCycles = 7;
									RegAF.Value16 = TableALU[5, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
									break;
								case 0xEF: // RST $28
									ClockCycles = 11;
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x28;
									break;
								case 0xF0: // RET P
									ClockCycles = 11;
									if (!RegFlagS) {
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									} else {
										ClockCycles = 5;
									}
									break;
								case 0xF1: // POP AF
									ClockCycles = 10;
									RegAF.Low8 = ReadMemory(RegSP.Value16++); RegAF.High8 = ReadMemory(RegSP.Value16++);
									break;
								case 0xF2: // JP P, nn
									ClockCycles = 10;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagS) {
										RegPC.Value16 = TUS;
									} else {
										ClockCycles = 10;
									}
									break;
								case 0xF3: // DI
									ClockCycles = 4;
									this.DisableInterrupts();
									break;
								case 0xF4: // CALL P, nn
									ClockCycles = 17;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (!RegFlagS) {
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
									} else {
										ClockCycles = 10;
									}
									break;
								case 0xF5: // PUSH AF
									ClockCycles = 11;
									WriteMemory(--RegSP.Value16, RegAF.High8); WriteMemory(--RegSP.Value16, RegAF.Low8);
									break;
								case 0xF6: // OR n
									ClockCycles = 7;
									RegAF.Value16 = TableALU[6, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
									break;
								case 0xF7: // RST $30
									ClockCycles = 11;
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x30;
									break;
								case 0xF8: // RET M
									ClockCycles = 11;
									if (RegFlagS) {
										RegPC.Low8 = ReadMemory(RegSP.Value16++); RegPC.High8 = ReadMemory(RegSP.Value16++);
									} else {
										ClockCycles = 5;
									}
									break;
								case 0xF9: // LD SP, IY
									ClockCycles = 10;
									RegSP.Value16 = RegIY.Value16;
									break;
								case 0xFA: // JP M, nn
									ClockCycles = 10;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagS) {
										RegPC.Value16 = TUS;
									} else {
										ClockCycles = 10;
									}
									break;
								case 0xFB: // EI
									ClockCycles = 4;
									this.EnableInterrupts();
									break;
								case 0xFC: // CALL M, nn
									ClockCycles = 17;
									TUS = (ushort)(ReadMemory(RegPC.Value16++) + ReadMemory(RegPC.Value16++) * 256);
									if (RegFlagS) {
										WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
										RegPC.Value16 = TUS;
									} else {
										ClockCycles = 10;
									}
									break;
								case 0xFD: // <-
									ClockCycles = 1337;
									// Invalid sequence.
									break;
								case 0xFE: // CP n
									ClockCycles = 7;
									RegAF.Value16 = TableALU[7, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
									break;
								case 0xFF: // RST $38
									ClockCycles = 11;
									WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
									RegPC.Value16 = 0x38;
									break;
							}
							break;
						case 0xFE: // CP n
							ClockCycles = 7;
							RegAF.Value16 = TableALU[7, RegAF.High8, ReadMemory(RegPC.Value16++), 0];
							break;
						case 0xFF: // RST $38
							ClockCycles = 11;
							WriteMemory(--RegSP.Value16, RegPC.High8); WriteMemory(--RegSP.Value16, RegPC.Low8);
							RegPC.Value16 = 0x38;
							break;
					}

					
				}
				
				this.PendingCycles -= ClockCycles;
				this.TotalExecutedCycles += ClockCycles;
				
			}			
		}
	}
}