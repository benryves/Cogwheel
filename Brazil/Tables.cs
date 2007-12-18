using System;
using System.Collections.Generic;
using System.Text;
using int8_t = System.SByte;

namespace BeeDevelopment.Brazil {
    public partial class Z80A {

        private void InitTables() {
            InitTableInc();
            InitTableDec();
            InitTableParity();
            InitTableALU();
            InitTableRotShift();
            InitTableHalfBorrow();
            InitTableHalfCarry();
            InitTableNeg();
            InitTableDaa();

            CheckCoBB();
        }

        #region 8-bit increment
        private byte[] TableInc;
        private void InitTableInc() {
            TableInc = new byte[256];
            for (int i = 0; i < 256; ++i)
                TableInc[i] = FlagByte(false, false, i == 0x80, UndocumentedX(i), (i & 0xF) == 0x0, UndocumentedY(i), i == 0, i > 127);
        }
        #endregion

        #region 8-bit decrement
        private byte[] TableDec;
        private void InitTableDec() {
            TableDec = new byte[256];
            for (int i = 0; i < 256; ++i)
                TableDec[i] = FlagByte(false, true, i == 0x7F, UndocumentedX(i), (i & 0xF) == 0xF, UndocumentedY(i), i == 0, i > 127);
        }
        #endregion

        #region Parity
        private bool[] TableParity;
        private void InitTableParity() {
            TableParity = new bool[256];
            for (int i = 0; i < 256; ++i) {
                int Bits = 0;
                for (int j = 0; j < 8; ++j) {
                    Bits += (i >> j) & 1;
                }
                TableParity[i] = (Bits & 1) == 0;
            }
        }
        #endregion

        #region ALU operations

        private ushort[, , ,] TableALU;
        private void InitTableALU() {
            TableALU = new ushort[8, 256, 256, 2]; // Class, OP1, OP2, Carry

            for (int i = 0; i < 8; ++i) {
                for (int op1 = 0; op1 < 256; ++op1) {
                    for (int op2 = 0; op2 < 256; ++op2) {
                        for (int c = 0; c < 2; ++c) {

                            int ac = (i == 1 || i == 3) ? c : 0;

                            bool S = false;
                            bool Z = false;
                            bool C = false;
                            bool H = false;
                            bool N = false;
                            bool P = false;

                            byte result_b = 0;
                            int result_si = 0;
                            int result_ui = 0;

                            // Fetch result
                            switch (i) {
                                case 0:
                                case 1:
                                    result_si = (sbyte)op1 + (sbyte)op2 + ac;
                                    result_ui = op1 + op2 + ac;
                                    break;
                                case 2:
                                case 3:
                                case 7:
                                    result_si = (sbyte)op1 - (sbyte)op2 - ac;
                                    result_ui = op1 - op2 - ac;
                                    break;
                                case 4:
                                    result_si = op1 & op2;
                                    break;
                                case 5:
                                    result_si = op1 ^ op2;
                                    break;
                                case 6:
                                    result_si = op1 | op2;
                                    break;
                            }

                            result_b = (byte)result_si;

                            // Parity/Carry

                            switch (i) {
                                case 0:
                                case 1:
                                case 2:
                                case 3:
                                case 7:
                                    P = result_si < -128 || result_si > 127;
                                    C = result_ui < 0 || result_ui > 255;
                                    break;
                                case 4:
                                case 5:
                                case 6:
                                    P = TableParity[result_b];
                                    C = false;
                                    break;
                            }

                            // Subtraction
                            N = i == 2 || i == 3 || i == 7;

                            // Half carry
                            switch (i) {
                                case 0:
                                case 1:
                                    H = ((op1 & 0xF) + (op2 & 0xF) + (ac & 0xF)) > 0xF;
                                    break;
                                case 2:
                                case 3:
                                case 7:
                                    H = ((op1 & 0xF) - (op2 & 0xF) - (ac & 0xF)) < 0x0;
                                    break;
                                case 4:
                                case 5:
                                case 6:
                                    H = false;
                                    break;
                            }

                            S = result_b > 127;
                            Z = result_b == 0;

                            if (i == 7) result_b = (byte)op1;

                            TableALU[i, op1, op2, c] = (ushort)(
                                result_b * 256 +
                                ((C ? 0x01 : 0) + (N ? 0x02 : 0) + (P ? 0x04 : 0) + (H ? 0x10 : 0) + (Z ? 0x40 : 0) + (S ? 0x80 : 0)) +
                                (result_si & 0x28));

                        }
                    }
                }
            }    
        }

        #endregion

        #region 8-bit Half Carry/Borrow

        private bool[,] TableHalfBorrow;
        private void InitTableHalfBorrow() {
            TableHalfBorrow = new bool[256, 256];
            for (int i = 0; i < 256; i++) {
                for (int j = 0; j < 256; j++) {
                    TableHalfBorrow[i, j] = ((i & 0xF) - (j & 0xF)) < 0;
                }
            }
        }

        private bool[,] TableHalfCarry;
        private void InitTableHalfCarry() {
            TableHalfCarry = new bool[256, 256];
            for (int i = 0; i < 256; i++) {
                for (int j = 0; j < 256; j++) {
                    TableHalfCarry[i, j] = ((i & 0xF) + (j & 0xF)) > 0xF;
                }
            }
        }

        #endregion

        #region Rotate and Shift

        private ushort[, ,] TableRotShift;
        private void InitTableRotShift() {
            TableRotShift = new ushort[2, 8, 65536]; // All, operation, AF
            for (int all = 0; all < 2; all++) {
                for (int y = 0; y < 8; ++y) {
                    for (int af = 0; af < 65536; af++) {


                        byte Old = (byte)(af >> 8);
                        bool OldCarry = (af & 0x01) != 0;

                        ushort newAf = (ushort)(af & ~(0x13)); // Clear HALF-CARRY, SUBTRACT and CARRY flags

                        byte New = Old;
                        if ((y & 1) == 0) {

                            if ((Old & 0x80) != 0) ++newAf;

                            New <<= 1;

                            if ((y & 0x04) == 0) {
                                if (((y & 0x02) == 0) ? ((newAf & 0x01) != 0) : OldCarry) New |= 0x01;
                            } else {
                                if ((y & 0x02) != 0) New |= 0x01;
                            }

                        } else {

                            if ((Old & 0x01) != 0) ++newAf;

                            New >>= 1;

                            if ((y & 0x04) == 0) {
                                if (((y & 0x02) == 0) ? ((newAf & 0x01) != 0) : OldCarry) New |= 0x80;
                            } else {
                                if ((y & 0x02) == 0) New |= (byte)(Old & 0x80);
                            }
                        }

                        newAf &= 0xFF;
                        newAf |= (ushort)(New * 256);

                        if (all == 1) {
                            newAf &= unchecked((ushort)~0xC4); // Clear S, Z, P
                            if (New > 127) newAf |= 0x80;
                            if (New == 0) newAf |= 0x40;
                            if (TableParity[New]) newAf |= 0x04;
                            
                        }

                        TableRotShift[all, y, af] = newAf;
                    }
                }
            }
            
        }

        #endregion

        #region Negation
        private ushort[] TableNeg;
        private void InitTableNeg() {
            TableNeg = new ushort[65536];
            for (int af = 0; af < 65536; af++) {
                ushort raf = 0;
                byte b = (byte)(af >> 8);
                byte a = (byte)-b;
                raf |= (ushort)(a * 256);
                raf |= FlagByte(b != 0x00, true, b == 0x80, UndocumentedX(b), TableHalfCarry[a, b], UndocumentedY(b), a == 0, a > 127);
                TableNeg[af] = raf;
            }
        }
        #endregion

        #region DAA
        private ushort[] TableDaa;
        private void InitTableDaa() {
            TableDaa = new ushort[65536];
            for (int af = 0; af < 65536; ++af) {

                byte a = (byte)(af >> 8);
                byte tmp = a;

                if (IsN(af)) {
                    if (IsH(af) || ((a & 0x0F) > 0x09)) tmp -= 0x06;
                    if (IsC(af) || a > 0x99) tmp -= 0x60;
                } else {
                    if (IsH(af) || ((a & 0x0F) > 0x09)) tmp += 0x06;
                    if (IsC(af) || a > 0x99) tmp += 0x60;
                }

                TableDaa[af] = (ushort)((tmp * 256) + FlagByte(IsC(af) || a > 0x99, IsN(af), TableParity[tmp], UndocumentedX(a), ((a ^ tmp) & 0x10) != 0, UndocumentedY(a), tmp == 0, tmp > 127));
            }
        }
        #endregion

        private byte FlagByte(bool C, bool N, bool P, bool X, bool H, bool Y, bool Z, bool S) {
            return (byte)(
                (C ? 0x01 : 0) +
                (N ? 0x02 : 0) +
                (P ? 0x04 : 0) +
                (X ? 0x08 : 0) +
                (H ? 0x10 : 0) +
                (Y ? 0x20 : 0) +
                (Z ? 0x40 : 0) +
                (S ? 0x80 : 0)
             );
        }

        private bool UndocumentedX(int value) {
            return (value & 0x08) != 0;
        }

        private bool UndocumentedY(int value) {
            return (value & 0x20) != 0;
        }

        private bool IsC(int value) { return (value & 0x01) != 0; }
        private bool IsN(int value) { return (value & 0x02) != 0; }
        private bool IsP(int value) { return (value & 0x04) != 0; }
        private bool IsX(int value) { return (value & 0x08) != 0; }
        private bool IsH(int value) { return (value & 0x10) != 0; }
        private bool IsY(int value) { return (value & 0x20) != 0; }
        private bool IsZ(int value) { return (value & 0x40) != 0; }
        private bool IsS(int value) { return (value & 0x80) != 0; }

        #region CoBB

        private int B2I(bool b) { return b ? 1 : 0; }
        private bool I2B(int i) { return i != 0; }

        private void CheckCoBB() {


            // Flag bitmasks
            int F_C = 0x01;
            int F_N = 0x02;
            int F_P = 0x04;
            int F_3 = 0x08;
            int F_H = 0x10;
            int F_5 = 0x20;
            int F_Z = 0x40;
            int F_S = 0x80;

            // Tables
            int[] par = new int[0x100];
            int[] log_f = new int[0x100];
            int[] inc_r = new int[0x100];
            int[] inc_f = new int[0x10000];
            int[] dec_f = new int[0x10000];
            int[] rlca_lut = new int[0x10000];
            int[] rrca_lut = new int[0x10000];
            int[] rla_lut = new int[0x10000];
            int[] rra_lut = new int[0x10000];
            int[] cpl_lut = new int[0x10000];
            int[] scf_lut = new int[0x10000];
            int[] ccf_lut = new int[0x10000];
            
            int[] adc_lut = new int[0x20000];
            int[] sbc_lut = new int[0x20000];

            int[] in_f = new int[0x10000];
            int[] rlc_lut = new int[0x10000];
            int[] rrc_lut = new int[0x10000];
            int[] rl_lut = new int[0x10000];
            int[] rr_lut = new int[0x10000];

            int[] sla_lut = new int[0x10000];
            int[] sll_lut = new int[0x10000];
            int[] sra_lut = new int[0x10000];
            int[] srl_lut = new int[0x10000];

            int[] daa_lut = new int[0x10000];
            

            int c, h, l, a, f;
            // Parity, logic 
            for (c = 0; c < 0x100; c++) {
                for (h = 1, l = c; l > 0; l >>= 1) h ^= (l & 1);
                par[c] = h * F_P;
                log_f[c] = (c & (F_S | F_5 | F_3)) | (F_Z * B2I(c == 0)) | par[c];
                inc_r[c] = ((c + 1) & 0x7f) | (c & 0x80);
            }
            // Arithmetic 
            for (c = 0; c < 0x10000; c++) {
                h = c >> 8;
                l = c & 0xff;
                // INC: SZ!H!PNC **r*rV0- 
                inc_f[c] = (l & F_C) | (h & (F_S | F_5 | F_3)) | (F_Z * B2I(h == 0)) |
                   (F_P * B2I(h == 0x80)) | (F_H * B2I((h & 0x0f) == 0));
                // DEC: SZ!H!PNC **r*rV1- 
                dec_f[c] = (l & F_C) | (h & (F_S | F_5 | F_3)) | (F_Z * B2I(h == 0)) |
                   (F_P * B2I(h == 0x7f)) | (F_H * B2I((h & 0x0f) == 0x0f)) | F_N;
                // RLCA, RRCA, RLA, RRA: SZ!H!PNC --r0r-0* 
                a = ((h << 1) | (h >> 7)) & 0xff;
                f = (l & (F_S | F_Z | F_P)) | (a & (F_5 | F_3 | F_C));
                rlca_lut[c] = (a << 8) + f;
                a = ((h >> 1) | (h << 7)) & 0xff;
                f = (l & (F_S | F_Z | F_P)) | (a & (F_5 | F_3)) | (h & F_C);
                rrca_lut[c] = (a << 8) + f;
                a = ((h << 1) | (l & F_C)) & 0xff;
                f = (l & (F_S | F_Z | F_P)) | (a & (F_5 | F_3)) | ((h >> 7) & F_C);
                rla_lut[c] = (a << 8) + f;
                a = ((h >> 1) | (l << 7)) & 0xff;
                f = (l & (F_S | F_Z | F_P)) | (a & (F_5 | F_3)) | (h & F_C);
                rra_lut[c] = (a << 8) + f;
                // CPL: SZ!H!PNC --r1r-1- 
                a = (~h) & 0xff;
                f = (l & (F_S | F_Z | F_P | F_C)) | (a & (F_5 | F_3)) | F_N | F_H;
                cpl_lut[c] = (a << 8) + f;
                // SCF: SZ!H!PNC --|0|-01 
                scf_lut[c] = (h << 8) + ((l & (F_S | F_Z | F_P | F_5 | F_3)) | (h & (F_5 | F_3)) | F_C);
                // CCF: SZ!H!PNC --|c|-0* 
                ccf_lut[c] = (h << 8) + (((l ^ 1) & (F_S | F_Z | F_P | F_C | F_5 | F_3)) | (h & (F_5 | F_3)) | ((l << 4) & F_H));
                // ADD: SZ!H!PNC **r*rV0* 
                a = (h + l) & 0xff;
                f = (int8_t)h + (int8_t)l;
                f = (a & (F_S | F_5 | F_3)) | (F_Z * B2I(a == 0)) | ((a ^ h ^ l) & F_H) |
                   (F_P * B2I(f != (int8_t)f)) | ((h + l) >> 8);
                adc_lut[c] = (a << 8) + f;
                // ADD+1 
                a = (h + l + 1) & 0xff;
                f = (int8_t)h + (int8_t)l + 1;
                f = (a & (F_S | F_5 | F_3)) | (F_Z * B2I(a == 0)) | ((a ^ h ^ l) & F_H) |
                   (F_P * B2I(f != (int8_t)f)) | ((h + l + 1) >> 8);
                adc_lut[c + 0x10000] = (a << 8) + f;
                // SUB: SZ!H!PNC **r*rV1* 
                a = (h - l) & 0xff;
                f = (int8_t)h - (int8_t)l;
                f = (a & (F_S | F_5 | F_3)) | (F_Z * B2I(a == 0)) | ((a ^ h ^ l) & F_H) |
                   (F_P * B2I(f != (int8_t)f)) | (F_C * B2I(l > h)) | F_N;
                sbc_lut[c] = (a << 8) + f;
                // SUB-1 
                a = (h - l - 1) & 0xff;
                f = (int8_t)h - (int8_t)l - 1;
                f = (a & (F_S | F_5 | F_3)) | (F_Z * B2I(a == 0)) | ((a ^ h ^ l) & F_H) |
                   (F_P * B2I(f != (int8_t)f)) | (F_C * B2I(l + 1 > h)) | F_N;
                sbc_lut[c + 0x10000] = (a << 8) + f;
                // IN: SZ!H!PNC **r*rP0- 
                in_f[c] = (l & F_C) | (h & (F_S | F_5 | F_3)) | (F_Z * B2I(h == 0)) | par[h];
                // RLC, RRC, RL, RR, SLA, SRA, SLL, SRL: SZ!H!PNC **r0rP0* 
                a = ((h << 1) | (h >> 7)) & 0xff;
                f = (a & (F_S | F_5 | F_3 | F_C)) | (F_Z * B2I(a == 0)) | par[a];
                rlc_lut[c] = (a << 8) + f;
                a = ((h >> 1) | (h << 7)) & 0xff;
                f = (a & (F_S | F_5 | F_3)) | (F_Z * B2I(a == 0)) | par[a] | (h & F_C);
                rrc_lut[c] = (a << 8) + f;
                a = ((h << 1) | (l & F_C)) & 0xff;
                f = (a & (F_S | F_5 | F_3)) | (F_Z * B2I(a == 0)) | par[a] | ((h >> 7) & F_C);
                rl_lut[c] = (a << 8) + f;
                a = ((h >> 1) | (l << 7)) & 0xff;
                f = (a & (F_S | F_5 | F_3)) | (F_Z * B2I(a == 0)) | par[a] | (h & F_C);
                rr_lut[c] = (a << 8) + f;
                a = (h << 1) & 0xff;
                f = (a & (F_S | F_5 | F_3)) | (F_Z * B2I(a == 0)) | par[a] | ((h >> 7) & F_C);
                sla_lut[c] = (a << 8) + f;
                a |= 1;
                f = (a & (F_S | F_5 | F_3)) | (F_Z * B2I(a == 0)) | par[a] | ((h >> 7) & F_C);
                sll_lut[c] = (a << 8) + f;
                a = (h >> 1) & 0xff;
                f = (a & (F_5 | F_3)) | (F_Z * B2I(a == 0)) | par[a] | (h & F_C);
                srl_lut[c] = (a << 8) + f;
                a |= h & F_S;
                f = (a & (F_S | F_5 | F_3)) | (F_Z * B2I(a == 0)) | par[a] | (h & F_C);
                sra_lut[c] = (a << 8) + f;
                // DAA: SZ!H!PNC **r*rP-* 
                a = h;
                if (I2B(l & F_N)) {
                    if (I2B(l & F_H) || ((h & 0x0f) > 9)) a -= 0x06;
                    if (I2B(l & F_C) || (h > 0x99)) a -= 0x60;
                } else {
                    if (I2B(l & F_H) || ((h & 0x0f) > 9)) a += 0x06;
                    if (I2B(l & F_C) || (h > 0x99)) a += 0x60;
                }
                f = log_f[a & 0xff] | B2I(h > 0x99) | (l & (F_N | F_C)) | ((h ^ a) & F_H);
                daa_lut[c] = ((a & 0xff) << 8) + f;
            }


            for (int alu_c = 0; alu_c < 8; ++alu_c) {

                int[] valid = null;

                bool Logical = false; 

                switch (alu_c) {
                    case 0: // ADC
                    case 1: // ADD
                        valid = adc_lut;
                        break;
                    case 2: // SBC
                    case 3: // SUB
                    case 7: // CP
                        valid = sbc_lut;
                        break;
                    case 4: // AND
                    case 5: // XOR
                    case 6: // OR
                        valid = log_f;
                        Logical = true;
                        break;
                }

                for (int op1 = 0; op1 < 256; ++op1) {
                    for (int op2 = 0; op2 < 256; ++op2) {
                        for (int carry = 1; carry < 2; ++carry) {

                            ushort AF;

                            byte CoBB_A = 0; byte Ben_A = 0;
                            byte CoBB_F = 0; byte Ben_F = 0;

                            if (Logical) {
                                switch (alu_c) {
                                    case 4:
                                        CoBB_A = (byte)(op1 & op2);
                                        break;
                                    case 5:
                                        CoBB_A = (byte)(op1 ^ op2);
                                        break;
                                    case 6:
                                        CoBB_A = (byte)(op1 | op2);
                                        break;
                                }
                                CoBB_F = (byte)valid[CoBB_A];
                            } else {
                                AF = (ushort)valid[op1 * 256 + op2 + ((carry * 0x10000) * (alu_c == 7 ? 0 : (alu_c & 1)))];

                                if (alu_c == 7) AF = (ushort)((AF & 0x00FF) + (op1 * 256));

                                CoBB_A = (byte)(AF >> 8);
                                CoBB_F = (byte)AF;
                                
                            }

                            //TableALU[alu_c, op1, op2, carry] = (ushort)(CoBB_A * 256 + CoBB_F);

                            AF = TableALU[alu_c, op1, op2, carry];
                            Ben_A = (byte)(AF >> 8);
                            Ben_F = (byte)AF;

                            /*Ben_F |= (byte)(F_3 | F_5);
                            CoBB_F |= (byte)(F_3 | F_5);*/

                            if (Ben_A != CoBB_A) throw new Exception(string.Format("A: ALU[{0}] {1:X2}, {2:X2} [+{3}]", alu_c, op1, op2, carry));
                            if (Ben_F != CoBB_F) throw new Exception(string.Format("F: ALU[{0}] {1:X2}, {2:X2} [+{3}]", alu_c, op1, op2, carry));
                        }
                    }
                }
            }

            for (int i = 0; i < 65536; ++i) {

                if (!CompareAF(TableRotShift[0, 0, i], rlca_lut[i])) throw new Exception();
                if (!CompareAF(TableRotShift[0, 1, i], rrca_lut[i])) throw new Exception();
                if (!CompareAF(TableRotShift[0, 2, i], rla_lut[i])) throw new Exception();
                if (!CompareAF(TableRotShift[0, 3, i], rra_lut[i])) throw new Exception();

                if (!CompareAF(TableRotShift[1, 0, i], rlc_lut[i])) throw new Exception();
                if (!CompareAF(TableRotShift[1, 1, i], rrc_lut[i])) throw new Exception();
                if (!CompareAF(TableRotShift[1, 2, i], rl_lut[i])) throw new Exception();
                if (!CompareAF(TableRotShift[1, 3, i], rr_lut[i])) throw new Exception();
                if (!CompareAF(TableRotShift[1, 4, i], sla_lut[i])) throw new Exception();
                if (!CompareAF(TableRotShift[1, 5, i], sra_lut[i])) throw new Exception();
                if (!CompareAF(TableRotShift[1, 6, i], sll_lut[i])) throw new Exception();
                if (!CompareAF(TableRotShift[1, 7, i], srl_lut[i])) throw new Exception();
            }

            


        }

        private bool CompareAF(int af1, int af2) {
            return (af1 | 0x28) == (af2 | 0x28);
        }


        #endregion


    }
}
