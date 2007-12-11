using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Brazil {
    public partial class Z80A {

        [StructLayout(LayoutKind.Explicit)]
        struct RegisterPair {
            
            [FieldOffset(0)]
            public ushort Value16;

            [FieldOffset(0)]
            public byte Low8;

            [FieldOffset(1)]
            public byte High8;

            public RegisterPair(ushort value) {
                this.Value16 = value;
                this.Low8 = (byte)(this.Value16);
                this.High8 = (byte)(this.Value16 >> 8);
            }

            public static implicit operator ushort(RegisterPair rp) {
                return rp.Value16;
            }

            public static implicit operator RegisterPair(ushort value) {
                return new RegisterPair(value);
            }

        }

        #region Flags

        private bool RegFlagC {
            get { return (RegAF.Low8 & 0x01) != 0; }
            set { RegAF.Low8 = (byte)((RegAF.Low8 & ~0x01) | (value ? 0x01 : 0x00)); }
        }

        private bool RegFlagN {
            get { return (RegAF.Low8 & 0x02) != 0; }
            set { RegAF.Low8 = (byte)((RegAF.Low8 & ~0x02) | (value ? 0x02 : 0x00)); }
        }

        private bool RegFlagP {
            get { return (RegAF.Low8 & 0x04) != 0; }
            set { RegAF.Low8 = (byte)((RegAF.Low8 & ~0x04) | (value ? 0x04 : 0x00)); }
        }

        private bool RegFlagX {
            get { return (RegAF.Low8 & 0x08) != 0; }
            set { RegAF.Low8 = (byte)((RegAF.Low8 & ~0x08) | (value ? 0x08 : 0x00)); }
        }

        private bool RegFlagH {
            get { return (RegAF.Low8 & 0x10) != 0; }
            set { RegAF.Low8 = (byte)((RegAF.Low8 & ~0x10) | (value ? 0x10 : 0x00)); }
        }

        private bool RegFlagY {
            get { return (RegAF.Low8 & 0x20) != 0; }
            set { RegAF.Low8 = (byte)((RegAF.Low8 & ~0x20) | (value ? 0x20 : 0x00)); }
        }

        private bool RegFlagZ {
            get { return (RegAF.Low8 & 0x40) != 0; }
            set { RegAF.Low8 = (byte)((RegAF.Low8 & ~0x40) | (value ? 0x40 : 0x00)); }
        }

        private bool RegFlagS {
            get { return (RegAF.Low8 & 0x80) != 0; }
            set { RegAF.Low8 = (byte)((RegAF.Low8 & ~0x80) | (value ? 0x80 : 0x00)); }
        }

        #endregion

        #region Main Register Set

        private RegisterPair RegAF;
        private RegisterPair RegBC;
        private RegisterPair RegDE;
        private RegisterPair RegHL;

        #endregion

        #region Alternate Register Set

        private RegisterPair RegAltAF; // Shadow for A and F
        private RegisterPair RegAltBC; // Shadow for B and C
        private RegisterPair RegAltDE; // Shadow for D and E
        private RegisterPair RegAltHL; // Shadow for H and L
        
        #endregion

        #region Special Purpose Registers

        private byte RegI; // I (interrupt vector)
        private byte RegR; // R (memory refresh)

        private RegisterPair RegIX; // IX (index register x)
        private RegisterPair RegIY; // IY (index register y)

        private RegisterPair RegSP; // SP (stack pointer)
        private RegisterPair RegPC; // PC (program counter)

        #endregion


        private void ResetRegisters() {
            // Clear main registers
            RegAF = 0; RegBC = 0; RegDE = 0; RegHL = 0;
            // Clear alternate registers
            RegAltAF = 0; RegAltBC = 0; RegAltDE = 0; RegAltHL = 0;
            // Clear special purpose registers
            RegI = 0; RegR = 0;
            RegIX.Value16 = 0; RegIY.Value16 = 0;
            RegSP.Value16 = 0; RegPC.Value16 = 0;
        }

        #region Public accessors

        public byte RegisterA {
            get { return RegAF.High8; }
            set { RegAF.High8 = value; }
        }
        public byte RegisterF {
            get { return RegAF.Low8; }
            set { RegAF.Low8 = value; }
        }
        public ushort RegisterAF {
            get { return RegAF.Value16; }
            set { RegAF.Value16 = value; }
        }
        public byte RegisterB {
            get { return RegBC.High8; }
            set { RegBC.High8 = value; }
        }
        public byte RegisterC {
            get { return RegBC.Low8; }
            set { RegBC.Low8 = value; }
        }
        public ushort RegisterBC {
            get { return RegBC.Value16; }
            set { RegBC.Value16 = value; }
        }
        public byte RegisterD {
            get { return RegDE.High8; }
            set { RegDE.High8 = value; }
        }
        public byte RegisterE {
            get { return RegDE.Low8; }
            set { RegDE.Low8 = value; }
        }
        public ushort RegisterDE {
            get { return RegDE.Value16; }
            set { RegDE.Value16 = value; }
        }
        public byte RegisterH {
            get { return RegHL.High8; }
            set { RegHL.High8 = value; }
        }
        public byte RegisterL {
            get { return RegHL.Low8; }
            set { RegHL.Low8 = value; }
        }
        public ushort RegisterHL {
            get { return RegHL.Value16; }
            set { RegHL.Value16 = value; }
        }

        public ushort RegisterPC {
            get { return RegPC.Value16; }
            set { RegPC.Value16 = value; }
        }
        public ushort RegisterSP {
            get { return RegSP.Value16; }
            set { RegSP.Value16 = value; }
        }
        public ushort RegisterIX {
            get { return RegIX.Value16; }
            set { RegIX.Value16 = value; }
        }
        public ushort RegisterIY {
            get { return RegIY.Value16; }
            set { RegIY.Value16 = value; }
        }
        public byte RegisterI {
            get { return RegI; }
            set { RegI = value; }
        }
        public byte RegisterR {
            get { return RegR; }
            set { RegR = value; }
        }
        public ushort RegisterShadowAF {
            get { return RegAltAF.Value16; }
            set { RegAltAF.Value16 = value; }
        }
        public ushort RegisterShadowBC {
            get { return RegAltBC.Value16; }
            set { RegAltBC.Value16 = value; }
        }
        public ushort RegisterShadowDE {
            get { return RegAltDE.Value16; }
            set { RegAltDE.Value16 = value; }
        }
        public ushort RegisterShadowHL {
            get { return RegAltHL.Value16; }
            set { RegAltHL.Value16 = value; }
        }

        #endregion

    }
}