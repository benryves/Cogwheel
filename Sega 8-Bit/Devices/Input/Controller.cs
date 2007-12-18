using System;
using System.Collections.Generic;
using System.Text;

namespace BeeDevelopment.Sega8Bit.Devices.Input {

    public class Controller {

        internal readonly Emulation.Emulator machine;
        public Controller(Emulation.Emulator machine) {
            this.machine = machine;
        }

        /// <summary>Enumeration covering the 6 pins that a controller might use.</summary>
        [Flags]
        public enum Pins {
            None = 0x00,
            Up = 0x01,
            Down = 0x02,
            Left = 0x04,
            Right = 0x08,
            TL = 0x10,
            TR = 0x20,
            TH = 0x40,
        }

        /// <summary>Returns the state of the pins of the currently connected controller.</summary>
        /// <remarks>A pressed button should return a set flag, a released button a reset flag.</remarks>
        public virtual Pins State {
            get { return Pins.None; }
            set { }
        }

    }
}
