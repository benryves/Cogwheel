using System;
using System.Collections.Generic;
using System.Text;

namespace BeeDevelopment.Brazil {
    public partial class Z80A {

        /// <summary>
        /// Override this method to allow the Z80 to read from memory.
        /// </summary>
        /// <param name="address">16-bit memory address</param>
        /// <returns>Value of the byte at the memory address requested</returns>
        public virtual byte ReadMemory(ushort address) {
            return 0;
        }

        /// <summary>
        /// Override this method to allow the Z80 to write to memory.
        /// </summary>
        /// <param name="address">16-bit memory address</param>
        /// <param name="value">The value of the byte to be written to the memory address requested</param>
        public virtual void WriteMemory(ushort address, byte value) {

        }

    }
}
