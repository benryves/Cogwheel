using System;
using System.Collections.Generic;
using System.Text;

namespace Cogwheel.Devices {
    public class Sc3000PPI : PPI {

        public override void Reset() {
            base.Reset();
            this.keyboardRow = 0x7;
        }

        public override void WritePortC(byte value) {
            this.keyboardRow = value & 0x7;
        }

        private int keyboardRow;
        public int KeyboardRow {
            get {
                return this.keyboardRow;
            }
        }


    }
}
