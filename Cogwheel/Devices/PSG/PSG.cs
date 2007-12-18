using System;
using System.Collections.Generic;
using System.Text;

namespace BeeDevelopment.Sega8Bit.Devices {
    
    public partial class ProgrammableSoundGenerator {

        #region State variables

        // Current state
        public int[,] StereoLevels = new int[4, 2];         // 1 or 0 (for stereo writes)
        public int[] VolumeLevels = { 15, 15, 15, 15 };	    // Volume level of the 4 channels.
        private int[] ToneRegisters = { 0, 0, 0, 0 };		// 10-bit tone registers.

        // Internal variables
        private int[] PsgToneCounters = { 0, 0, 0, 0 };
        private int PsgLatchedChannel = 0;
        private bool PsgLatchedTone = false;
        private bool PsgNoiseLatch = false;
        private int PsgNoiseValue = 0;
        private int PsgNoiseLength = 16;
        private int PsgNoiseTapped = 0x9;

        public int PsgTappedBits {
            get { return PsgNoiseTapped; }
            set { PsgNoiseTapped = value; }
        }
        public int PsgLsfrLength {
            get { return PsgNoiseLength; }
            set { PsgNoiseLength = value; }
        }


        /// <summary>Whether a channel is high or low.</summary>
        private int[] PsgStatus = { 0, 0, 0, 0 }; // Voltage level (0=low, 1=high)

        #endregion

        /// <summary>Create a new instance of the PSG emulator.</summary>

        public ProgrammableSoundGenerator() {
            this.Reset();
        }

        public void Reset() {
            for (int i = 0; i < 4; ++i) {
                ToneRegisters[i] = 0;
                VolumeLevels[i] = 15;
                PsgStatus[i] = 0;
                PsgToneCounters[i] = 0;
                for (int c = 0; c < 2; ++c) {
                    StereoLevels[i, c] = 1;
                }
            }
            PsgLatchedChannel = 0;
            PsgLatchedTone = false;
            PsgNoiseLatch = false;
            PsgNoiseValue = 1 << (PsgNoiseLength - 1);
            //this.BufferedWrites = new Queue<BufferedWrite>(1024);
        }

    }
}
