using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;

namespace BeeDevelopment.Cogwheel.Devices {
    public partial class VideoDisplayProcessor {

        
        /// <summary>Hard-coded TMS9918 colour palette.</summary>
        private int[,] PaletteTMS9918 = new int[,] { 
            {
                unchecked((int)0xFF000000),
                unchecked((int)0xFF000000),
                unchecked((int)0xFF47B73B),
                unchecked((int)0xFF7CCF6F),
                unchecked((int)0xFF5D4EFF),
                unchecked((int)0xFF8072FF),
                unchecked((int)0xFFB66247),
                unchecked((int)0xFF5DC8ED),
                unchecked((int)0xFFD76B48),
                unchecked((int)0xFFFB8F6C),
                unchecked((int)0xFFC3CD41),
                unchecked((int)0xFFD3DA76),
                unchecked((int)0xFF3E9F2F),
                unchecked((int)0xFFB664C7),
                unchecked((int)0xFFCCCCCC),
                unchecked((int)0xFFFFFFFF)
            } , {
                unchecked((int)0xFF000000),
                unchecked((int)0xFF000000),
                unchecked((int)0xFF00AA00),
                unchecked((int)0xFF00FF00),
                unchecked((int)0xFF000055),
                unchecked((int)0xFF0000FF),
                unchecked((int)0xFF550000),
                unchecked((int)0xFF00FFFF),
                unchecked((int)0xFFAA0000),
                unchecked((int)0xFFFF0000),
                unchecked((int)0xFF555500),
                unchecked((int)0xFFFFFF00),
                unchecked((int)0xFF005500),
                unchecked((int)0xFFFF00FF),
                unchecked((int)0xFF555555),
                unchecked((int)0xFFFFFFFF)
            }
        };


        public Color[][] ColouredTMS9918Palette;

        private void FillTMS9918Palettes() {
            unchecked {
                this.ColouredTMS9918Palette = new Color[2][];
                for (int j = 0; j < 2; j++) {

                    this.ColouredTMS9918Palette[j] = new Color[16];
                    /*for (int i = 0; i < 16; ++i) {
                        if (j == 1) {
                            Console.Write("{1:X2}{0:X2}", ((PaletteTMS9918[0, i] & 0x0000F0) >> 4), ((PaletteTMS9918[0, i] & 0x00F000) >> 8) | ((PaletteTMS9918[0, i] & 0xF00000) >> 20));
                        } else {
                            for (int k = 0; k < 2; ++k) {
                                int ix = ((i * 2 + k) & 0xF);
                                int c = PaletteTMS9918[0, ix] & 0xC0C0C0;
                                if (ix == 14) {
                                    c = 0x808080;
                                } else if (ix == 10) {
                                    c = 0x808040;
                                }
                                Console.Write("{0:X2}", ((c & 0x0000C0) >> 2) | ((c & 0x00C000) >> 12) | ((c & 0xC00000) >> 22));
                            }
                            
                        }
                        this.ColouredTMS9918Palette[j][i] = Color.FromArgb((int)((i == 0 ? 0 : 0xFF000000) + PaletteTMS9918[j, i]));
                    }*/
                }

            }
        }

        public Color[] CurrentPalette {
            get {
                if (this.CapFixedPalette) {
                    switch (this.CurrentVdpMode) {
                        case VdpMode.Mode4:
                        case VdpMode.Mode4Resolution224:
                        case VdpMode.Mode4Resolution240:
                            return this.CramPalette;
                        default:
                            if (this.CapFixedPaletteIndex == 0) {
                                return ColouredTMS9918Palette[0];
                            } else {
                                return ColouredTMS9918Palette[1];
                            }
                    }
                } else {
                    return this.CramPalette;
                }
            }
        }

        private Color[] colourPalette = new Color[32];
        /// <summary>The current contents of colour RAM.</summary>
        /// <remarks>The legacy TMS9918 modes normally use a fixed palette, and so any values in here can be ignored. However, the Game Gear uses these values.</remarks>
        public Color[] CramPalette {
            get {
                unchecked {
                    for (int i = 0; i < 32; ++i) {
                        this.colourPalette[i] = Color.FromArgb((int)(0xFF000000 + this.PaletteArgb[i]));
                    }
                    return this.colourPalette;
                }
            }
        }

        private int[] PaletteArgb = new int[32];
        
        private byte[] palette = new byte[64];

        /// <summary>
        /// Write a byte of data to the palette
        /// </summary>
        private void WritePalette(byte data) {

            //Console.WriteLine(this.cpu.TotalExecutedCycles);

            if (this.CapSupportsExtendedPalette && ((AddressRegister & 1) == 0)) {
                // Latch a byte
                paletteLatch = data;
            } else {
                // Write data to palette
                if (this.CapSupportsExtendedPalette) palette[(AddressRegister - 1) & 31] = paletteLatch;
                palette[AddressRegister & 31] = data;
                // Decode to 32-bit and copy to colourPalette
                int r, g, b;
                int paletteIndex;
                if (this.CapSupportsExtendedPalette) {
                    paletteIndex = (AddressRegister / 2) & 31;
                    b = palette[AddressRegister & 31] & 0x0F; b += b * 16;
                    g = palette[(AddressRegister - 1) & 31] & 0xF0; g += g / 16;
                    r = palette[(AddressRegister - 1) & 31] & 0x0F; r += r * 16;
                    //colourPalette[paletteIndex] = Color.FromArgb(0xFF, r, g, b);

                } else {
                    b = (palette[AddressRegister & 31] >> 4) & 0x3; b += b * 4; b += b * 16;
                    g = (palette[AddressRegister & 31] >> 2) & 0x3; g += g * 4; g += g * 16;
                    r = (palette[AddressRegister & 31]) & 0x3; r += r * 4; r += r * 16;
                    paletteIndex = AddressRegister & 31;
                    //colourPalette[paletteIndex] = Color.FromArgb(0xFF, r, g, b);
                }
                PaletteArgb[paletteIndex] = (int)(0xFF000000 + r * 0x10000 + g * 0x100 + b);// colourPalette[paletteIndex].ToArgb();
            }

            // Auto-increment address register
            ++AddressRegister;
        }

        private byte paletteLatch = 0x00;

        public void UpdatePalette(ref ColorPalette p) {
            for (int i = 0; i < 32; ++i) {
                p.Entries[i] = colourPalette[i];
            }
        }
    }
}
