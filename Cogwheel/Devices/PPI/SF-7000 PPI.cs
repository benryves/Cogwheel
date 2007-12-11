using System;
using System.Collections.Generic;
using System.Text;

namespace Cogwheel.Devices {
    public class Sf7000PPI : PPI {

        public override void Reset() {
            base.Reset();
            
        }

        public override void WritePortC(byte value) {
            this.FddInUse = (value & 0x01) == 0;
            this.FddMotorOn = (value & 0x02) == 0;
            this.FddTc = (value & 0x04) != 0;
            this.FddReset = (value & 0x08) != 0;

            this.IplRomEnabled = (value & 0x40) == 0;

            Console.WriteLine(this.IplRomEnabled);
            
        }

        public bool FddInUse = false;
        public bool FddMotorOn = false;
        public bool FddTc = false;
        public bool FddReset = false;
        public bool IplRomEnabled = true;

        public bool FdcInt = false;
        public bool Fdc17 = false;

        public override byte ReadPortA() {
            return (byte)((FdcInt ? 0x01 : 0x00) | (Fdc17 ? 0x02 : 0x00));
        }


    }
}
