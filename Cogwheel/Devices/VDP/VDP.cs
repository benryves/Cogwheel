using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace BeeDevelopment.Sega8Bit.Devices {
    
    public partial class VideoDisplayProcessor {

        private Emulation.Emulator cpu;

        /// <summary>
        /// Create a new instance of the VDP emulator.
        /// </summary>
        /// <param name="cpu">The CPU that is attached to the VDP (required for interrupts).</param>

        public VideoDisplayProcessor(Emulation.Emulator cpu) {
            this.cpu = cpu;
            this.SetupVCounter();
            this.Reset();
            this.StartFrame();
            this.FillTMS9918Palettes();
        }

        private byte[] vram;
        public byte[] VRam {
            get { return this.vram; }
            set { this.vram = value; }
        }

        private int[] FastPixelColourIndex;


        public bool GameGearLcdEffects = false;

        public void Reset() {
            this.vram = new byte[16 * 1024];
            this.FastPixelColourIndex = new int[512 * 8 * 8];
            this.palette = new byte[64];
            this.colourPalette = new Color[32];
            this.PaletteArgb = new int[32];

            for (int i = 0; i < 32; ++i) {
                this.colourPalette[i] = Color.Black;
                this.PaletteArgb[i] = this.colourPalette[i].ToArgb();
            }

            this.Registers = new byte[] { 0x06, 0x80, 0xFF, 0xFF, 0xFF, 0xFF, 0xFB, 0xF0, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00 };
            this.FlagLineInterruptPending = false;
            this.FlagFrameInterruptPending = false;

            this.TempPixelBuffer = new int[256 * 240];
        }

    }
}
