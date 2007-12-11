using System;
using System.Collections.Generic;
using System.Text;

namespace Cogwheel.Peripherals {

    /// <summary>
    /// The 3D glasses as used by certain 3D games.
    /// </summary>
    public class Glasses3D {

        /// <summary>
        /// Identifies a particular eye shutter.
        /// </summary>
        public enum GlassesEye {
            Left = 1,
            Right = 0,
        }


        private byte Status = 0;

        /// <summary>
        /// Returns the currently open eye shutter.
        /// </summary>
        public GlassesEye OpenEye {
            get { return (GlassesEye)(Status & 1); }
        }


        /// <summary>Write a control byte to the glasses register.</summary>
        /// <param name="value">The control byte to write.</param>
        public void Write(byte value) {
            this.Status = value;
        }

        /// <summary>Reset the glasses to their default state.</summary>
        public void Reset() {
            this.Status = 0;
        }
    }
}
