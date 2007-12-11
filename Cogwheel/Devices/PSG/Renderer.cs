using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Cogwheel.Devices {
    public partial class ProgrammableSoundGenerator {

        /// <summary>
        /// Calculate the parity level used for white noise generation.
        /// </summary>
        /// <param name="val">The value to calculate the parity for.</param>
        /// <returns>Voltage level (0 or 1).</returns>
        private int Parity(int val) {
            val ^= val >> 8;
            val ^= val >> 4;
            val ^= val >> 2;
            val ^= val >> 1;
            return val & 1;
        }

        /// <summary>
        /// Run the PSG for a single PSG CPU cycle.
        /// </summary>
        public void PsgTick() {
            for (int i = 0; i < 4; ++i) {
                if (PsgToneCounters[i] <= 0) {
                    if (i != 3) {
                        // Tone channels:
                        PsgToneCounters[i] = ToneRegisters[i];
                        if (ToneRegisters[i] != 0) {
                            PsgStatus[i] = 1 - PsgStatus[i];
                        } else {
                            PsgStatus[i] = 0;
                        }
                    } else {
                        // Noise channel:
                        int NoiseVal = ToneRegisters[3] & 0x3;
                        switch (NoiseVal) {
                            case 0: PsgToneCounters[i] = 0x10; break;
                            case 1: PsgToneCounters[i] = 0x20; break;
                            case 2: PsgToneCounters[i] = 0x40; break;
                            case 3: PsgToneCounters[i] = ToneRegisters[2]; break;
                        }
                        PsgNoiseLatch = !PsgNoiseLatch;
                        if (PsgNoiseLatch) {
                            PsgNoiseValue = (PsgNoiseValue >> 1) |
                                ((((ToneRegisters[3] & 0x4) != 0)
                                ? Parity(PsgNoiseValue & PsgNoiseTapped)
                                : PsgNoiseValue & 1) << (PsgNoiseLength - 1));
                            PsgStatus[3] = (PsgNoiseValue & 0x1);

                        }
                    }
                }
                --PsgToneCounters[i];
            }
        }
        
    }
}
