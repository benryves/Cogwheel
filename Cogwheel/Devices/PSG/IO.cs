using System;
using System.Collections.Generic;
using System.Text;

namespace Cogwheel.Devices {
    
    public partial class PSG {

        /// <summary>Write a byte to the PSG.</summary>
        /// <param name="value">The dara written to the port.</param>
        public void WriteByteToPsg(byte value) {
            if ((value & 0x80) != 0) {
                // It is a LATCH command
                PsgLatchedChannel = (value >> 5) & 0x3;
                if ((value & 0x10) == 0) {
                    PsgLatchedTone = true;
                    ToneRegisters[PsgLatchedChannel] &= 0xFFF0;
                    ToneRegisters[PsgLatchedChannel] |= (value & 0xF);
                    // Reset the LSFR?
                    if (PsgLatchedChannel == 3) {
                        PsgNoiseValue = 1 << (PsgNoiseLength - 1);
                    }
                } else {
                    PsgLatchedTone = false;
                    VolumeLevels[PsgLatchedChannel] = value & 0xF;
                    
                }
            } else {
                // It is a DATA command
                if (PsgLatchedTone) {
                    ToneRegisters[PsgLatchedChannel] &= 0xF;
                    ToneRegisters[PsgLatchedChannel] |= (value & 0x3F) << 4;
                } else {
                    VolumeLevels[PsgLatchedChannel] = value & 0xF;
                }
            }
        }

        /// <summary>Write a byte to the Game Gear's stereo control port.</summary>
        /// <param name="value">The data written to the port.</param>
        public void WriteByteToStereo(byte value) {
            for (int c = 0; c < 2; ++c) {
                for (int r = 0; r < 4; ++r) {
                    StereoLevels[r, c] = (value & 1);
                    value >>= 1;
                }
            }
        }


        //private static double[] PsgLogScale = { 1.0d, 0.794335765d, 0.630970183d, 0.501174963d, 0.398113956d, 0.316232795d, 0.251197851d, 0.20044557d, 0.15848262d, 0.125888852d, 0.100009156d, 0.07943968d, 0.063081759d, 0.050111393d, 0.039796136d, 0.0d };


    }
}
