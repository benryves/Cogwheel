using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Cogwheel.Devices {
    public partial class VDP {

        /// <summary>Stores the 10 VDP control registers.</summary>
        public byte[] Registers;

        /// <summary>Clear the registers to their default values.</summary>
        public void ResetRegisters() {
            Registers = new byte[10];
        }

        #region Named access to registers

        /// <summary>Register $00 - Mode control 1.</summary>
        public byte RegisterModeControl1 {
            get { return Registers[0x0]; }
            set { Registers[0x0] = value; }
        }

        /// <summary>Register $01 - Mode control 2.</summary>
        public byte RegisterModeControl2 {
            get { return Registers[0x1]; }
            set { Registers[0x1] = value; }
        }

        /// <summary>Register $02 - Name table base address.</summary>
        public byte RegisterNameTableAddress {
            get { return Registers[0x2]; }
            set { Registers[0x2] = value; }
        }

        /// <summary>Register $03 - Colour table base address.</summary>
        public byte RegisterColourTableAddress {
            get { return Registers[0x3]; }
            set { Registers[0x3] = value; }
        }

        /// <summary>Register $04 - Background pattern generator base address.</summary>
        public byte RegisterBackgroundPatternGeneratorAddress {
            get { return Registers[0x4]; }
            set { Registers[0x4] = value;}
        }

        /// <summary>Register $05 - Sprite attribute table base address.</summary>
        public byte RegisterSpriteAttributeTableAddress {
            get { return Registers[0x5]; }
            set { Registers[0x5] = value; }
        }

        /// <summary>Register $06 - Sprite pattern generator base address.</summary>
        public byte RegisterSpritePatternGeneratorAddress {
            get { return Registers[0x6]; }
            set { Registers[0x6] = value; }
        }

        /// <summary>Register $07 - Border colour.</summary>
        public byte RegisterBorderColour {
            get { return Registers[0x7]; }
            set { Registers[0x7] = value; }
        }

        /// <summary>Register $08 - Background X scroll.</summary>
        public byte RegisterBackgroundScrollX {
            get { return Registers[0x8]; }
            set { Registers[0x8] = value; }
        }

        /// <summary>Register $09 - Background Y scroll.</summary>
        public byte RegisterBackgroundScrollY {
            get { return Registers[0x9]; }
            set { Registers[0x9] = value; }
        }

        /// <summary>Register $0A - Line interrupt counter.</summary>
        public byte RegisterLineInterruptCounter {
            get { return Registers[0xA]; }
            set { Registers[0xA] = value; }
        }

        #endregion

    }
}
