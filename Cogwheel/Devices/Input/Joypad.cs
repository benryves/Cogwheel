using System;
using System.Collections.Generic;
using System.Text;

namespace BeeDevelopment.Sega8Bit.Devices.Input {
    public class Joypad : Input.Controller {

        public Joypad(Emulation.Emulator machine)
            : base(machine) {
        }

        private Controller.Pins CurrentState;

        public override Controller.Pins State {
            get { return CurrentState; }
            set { CurrentState = value; }
        }

        #region Public button state accessors

        public bool ButtonUp {
            get { return (this.CurrentState & Pins.Up) != 0; }
            set { this.CurrentState = (this.CurrentState & ~Pins.Up) | (value ? Pins.Up : 0); }
        }

        public bool ButtonDown {
            get { return (this.CurrentState & Pins.Down) != 0; }
            set { this.CurrentState = (this.CurrentState & ~Pins.Down) | (value ? Pins.Down : 0); }
        }

        public bool ButtonLeft {
            get { return (this.CurrentState & Pins.Left) != 0; }
            set { this.CurrentState = (this.CurrentState & ~Pins.Left) | (value ? Pins.Left : 0); }
        }

        public bool ButtonRight {
            get { return (this.CurrentState & Pins.Right) != 0; }
            set { this.CurrentState = (this.CurrentState & ~Pins.Right) | (value ? Pins.Right : 0); }
        }

        public bool Button1 {
            get { return (this.CurrentState & Pins.TL) != 0; }
            set { this.CurrentState = (this.CurrentState & ~Pins.TL) | (value ? Pins.TL : 0); }
        }

        public bool Button2 {
            get { return (this.CurrentState & Pins.TR) != 0; }
            set { this.CurrentState = (this.CurrentState & ~Pins.TR) | (value ? Pins.TR : 0); }
        }

        #endregion

    }
}
