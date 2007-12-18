using System;
using System.Collections.Generic;
using System.Text;

namespace BeeDevelopment.Sega8Bit.Devices {

    /// <summary>Emulated Intel 8255 PPI.</summary>
    public class PPI {

        public virtual void Reset() {
            CurrentPortC = 0;
        }

        public enum PortNumber {
            PortA,
            PortB,
            PortC,
            Control,
        }



        public bool PortAInput = true;
        public bool PortBInput = true;
        public bool PortCUpperInput = true;
        public bool PortCLowerInput = true;

        public int PortAPortCUpperMode = 0;
        public int PortBPortCLowerMode = 0;

        internal byte CurrentPortC = 0;

        public void Write(PortNumber port, byte value) {
            switch (port) {
                case PortNumber.Control:

                    if ((value & 0x80) != 0) {
                        // Configuration control byte.
                        PortAPortCUpperMode = (value & 0x60) >> 5;
                        PortAInput = (value & 0x10) != 0;
                        PortCUpperInput = (value & 0x08) != 0;
                        PortBPortCLowerMode = (value & 0x04) >> 2;                        
                        PortBInput = (value & 0x02) != 0;
                        PortCLowerInput = (value & 0x01) != 0;
                    } else {
                        // Bit Set/Reset control byte.
                        int BitToSet = 1 << ((value >> 1) & 0x7);
                        int NewPortCValue = this.CurrentPortC;
                        if ((value & 1) == 1) {
                            NewPortCValue |= BitToSet;
                        } else {
                            NewPortCValue &= ~BitToSet;
                        }
                        this.CurrentPortC = (byte)NewPortCValue;
                        WritePortC(this.CurrentPortC);
                    }
                    break;
                
                case PortNumber.PortA:
                    if (!this.PortAInput) this.WritePortA(value);
                    break;

                case PortNumber.PortB:
                    if (!this.PortBInput) this.WritePortB(value);
                    break;

                case PortNumber.PortC:
                    if (!this.PortCLowerInput || !this.PortCUpperInput) {
                        this.CurrentPortC = value;
                        this.WritePortC(value);
                    }
                    break;
            }
        }

        public byte Read(PortNumber port) {
            switch (port) {
                case PortNumber.PortA:
                    return this.PortAInput ? this.ReadPortA() : (byte)0xFF;
                case PortNumber.PortB:
                    return this.PortBInput ? this.ReadPortB() : (byte)0xFF;
                case PortNumber.PortC:
                    return (byte)((this.PortCLowerInput ? this.ReadPortCLower() : (byte)0xF) +
                        (this.PortCUpperInput ? this.ReadPortCUpper() : (byte)0xF) * 16);
                default:
                    return 0xFF;
            }
        }

        public virtual byte ReadPortA() {
            return 0xFF;
        }

        public virtual byte ReadPortB() {
            return 0xFF;
        }

        public virtual byte ReadPortCUpper() {
            return 0xFF;
        }

        public virtual byte ReadPortCLower() {
            return 0xFF;
        }

        public virtual void WritePortA(byte value) {
        }

        public virtual void WritePortB(byte value) {
        }

        public virtual void WritePortC(byte value) {
        }

    }
}
