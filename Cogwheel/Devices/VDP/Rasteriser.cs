using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Cogwheel.Devices {
    public partial class VDP {

        #region Construction and storage of a .NET Bitmap to represent the display.

        /// <summary>The current VDP mode.</summary>
        private VdpMode CurrentVdpMode;

        /// <summary>The current resolution of the display.</summary>
        public Size Resolution {
            get { return BlitterRectangle.Size; }
        }

        /// <summary>The video format the VDP generates.</summary>
        public enum VideoStandardType { NTSC, PAL }
        
        /// <summary>Whether the VDP uses NTSC or PAL timing.</summary>
        public VideoStandardType VideoStandard;

        /// <summary>The different video formats and resolutions available.</summary>
        private enum DisplayFormatAndResolution {
            NTSC192, NTSC224, NTSC240,
            PAL192, PAL224, PAL240,
        }
        private DisplayFormatAndResolution DisplayMode;

        /// <summary>The 32-bit RGB bitmap data is copied to at the end of a frame.</summary>
        private Bitmap OutputBitmap;

        /// <summary>The size of the current output bitmap.</summary>
        private Size CurrentBitmapSize;

        /// <summary>Buffer used to build up the image.</summary>
        private int[] PixelBuffer;

        /// <summary>Buffer used to blend subsequent frames together.</summary>
        private int[] TempPixelBuffer;

        /// <summary>Rectangle used to outline the area used for blitting.</summary>
        private Rectangle BlitterRectangle;

        /// <summary>Check to see if the Game Gear status has been changed.</summary>
        private Cogwheel.Emulation.MachineType LastMachine = (Cogwheel.Emulation.MachineType)(-1);

        /// <summary>Pixel data offset to top-left corner when cropping the Game Gear's display.</summary>
        private int GameGearCropOffset = 0;

        /// <summary>If the existing bitmap is the wrong size, update it.</summary>
        private void RecreateBitmap() {

            this.CurrentVdpMode = this.Mode;

            Size DesiredSize = new Size(256, VariableScreenHeight);

            if (OutputBitmap == null || CurrentBitmapSize != DesiredSize || (this.MachineType != LastMachine)) {
                if (OutputBitmap != null) OutputBitmap.Dispose();

                if (this.CapCroppedScreen) {
                    OutputBitmap = new Bitmap(160, 144, PixelFormat.Format32bppRgb);
                } else {
                    OutputBitmap = new Bitmap(DesiredSize.Width, DesiredSize.Height, PixelFormat.Format32bppRgb);
                }
                
                PixelBuffer = new int[DesiredSize.Width * DesiredSize.Height];
                BlitterRectangle = new Rectangle(0, 0, OutputBitmap.Width, OutputBitmap.Height);

                switch (DesiredSize.Height) {
                    case 192: DisplayMode = VideoStandard == VideoStandardType.NTSC ? DisplayFormatAndResolution.NTSC192 : DisplayFormatAndResolution.PAL192; break;
                    case 224: DisplayMode = VideoStandard == VideoStandardType.NTSC ? DisplayFormatAndResolution.NTSC224 : DisplayFormatAndResolution.PAL224; break;
                    case 240: DisplayMode = VideoStandard == VideoStandardType.NTSC ? DisplayFormatAndResolution.NTSC240 : DisplayFormatAndResolution.PAL240; break;
                }
                CurrentBitmapSize = DesiredSize;

                this.OnResolutionChange(new ResolutionChangeEventArgs(OutputBitmap.Size));

                GameGearCropOffset = ((256 - 160) / 2) + (128 * (DesiredSize.Height - 144));

                LastMachine = this.MachineType;
            }
        }



        #endregion

        #region Begin and end frames

        /// <summary>Reset all counters to start scanning a new frame.</summary>
        public void StartFrame() {
            // Set up the bitmap we are about to draw into.
            RecreateBitmap();

            // Set the current scanline to the start of the active display period.
            ScanlinePosition = ScanlinePositionType.ActiveDisplay;
            RemainingScanlinesCounter = ScreenSpaceCounters[(int)DisplayMode, 0];

            // Pick up and store the current Y scroll offset.
            LockedScrollY = RegisterBackgroundScrollY;
            if (CurrentBitmapSize.Height == 192) LockedScrollY %= 224;

            //LineInterruptCounter = RegisterLineInterruptCounter;

            DoneLinesCounter = 0;
            LastLinesCounter = 0;
            VBlankPending = 0;

            CurrentVCounterIndex = 0;
        }


        /// <summary>Produce a useable output frame at the end of the active display.</summary>
        public void FinishFrame() {

            int DataLength = PixelBuffer.Length;

            if (LastMachine == Cogwheel.Emulation.MachineType.GameGear) {
                int SrcPointer = GameGearCropOffset; // 0x1830;
                int DestPointer = 0;
                for (int y = 0; y < 144; y++) {
                    for (int x = 0; x < 160; ++x) {
                        PixelBuffer[DestPointer] = GameGearLcdEffects ? BlendQuarterRgb(PixelBuffer[SrcPointer++], TempPixelBuffer[DestPointer]) : PixelBuffer[SrcPointer++];
                        TempPixelBuffer[DestPointer] = PixelBuffer[DestPointer];
                        ++DestPointer;
                    }

                    if (GameGearLcdEffects && y > 1) {
                        DestPointer -= 320;
                        for (int x = 0; x < 160; ++x) {
                            PixelBuffer[DestPointer++] = BlendHalfRgb(PixelBuffer[DestPointer], PixelBuffer[DestPointer + 160]);
                        }
                        DestPointer += 160;
                    }

                    SrcPointer += (256 - 160);
                }
                DataLength = 160 * 144;

            } else if (LastMachine == Cogwheel.Emulation.MachineType.GameGearMasterSystem) {

                Array.Copy(PixelBuffer, TempPixelBuffer, PixelBuffer.Length);

                int SrcPointer = 8;
                int DestPointer = 160 * 8;
                int MaxY = 128;

                if (this.CurrentBitmapSize.Height == 224) {
                    SrcPointer = 8 + 256 * 3;
                    DestPointer = 0;
                    MaxY = 144;
                }

                int[,] PixelGrid = new int[5, 3];

                for (int y = 0; y < MaxY; y += 2) {
                    for (int x = 0; x < 160; x += 2) {

                        if (this.CurrentBitmapSize.Height == 224) {
                            for (int i = 0; i < 3; ++i) {
                                PixelGrid[0, i] = SrcPointer < 256 ? this.LastBackdropColour : TempPixelBuffer[SrcPointer - 256];
                                PixelGrid[1, i] = TempPixelBuffer[SrcPointer + 000];
                                PixelGrid[2, i] = TempPixelBuffer[SrcPointer + 256];
                                PixelGrid[3, i] = TempPixelBuffer[SrcPointer + 512];
                                PixelGrid[4, i] = TempPixelBuffer[SrcPointer + 768];
                                ++SrcPointer;
                            }
                        } else {
                            for (int i = 0; i < 3; ++i) {
                                PixelGrid[0, i] = SrcPointer < 512 ? this.LastBackdropColour : TempPixelBuffer[SrcPointer - 512];
                                PixelGrid[1, i] = SrcPointer < 256 ? this.LastBackdropColour : TempPixelBuffer[SrcPointer - 256];
                                PixelGrid[2, i] = TempPixelBuffer[SrcPointer + 000];
                                PixelGrid[3, i] = TempPixelBuffer[SrcPointer + 256];
                                PixelGrid[4, i] = TempPixelBuffer[SrcPointer + 512];
                                ++SrcPointer;
                            }
                        }


                        for (int i = 0; i < 5; ++i) {
                            int Pixel1 = (PixelGrid[i, 0] & 0xFFFF00) | (PixelGrid[i, 1] & 0x0000FF);
                            int Pixel2 = (PixelGrid[i, 1] & 0xFF0000) | (PixelGrid[i, 2] & 0x00FFFF);
                            PixelGrid[i, 0] = Pixel1;
                            PixelGrid[i, 1] = Pixel2;
                        }


                        if (this.CurrentBitmapSize.Height == 224) {
                            for (int i = 0; i < 2; ++i) {
                                PixelBuffer[DestPointer + 000] = BlendThirdRgb(PixelGrid[1, i], PixelGrid[2, i], BlendHalfRgb(PixelGrid[0, i], PixelGrid[3, i]));
                                PixelBuffer[DestPointer + 160] = BlendThirdRgb(PixelGrid[2, i], PixelGrid[3, i], BlendHalfRgb(PixelGrid[1, i], PixelGrid[4, i]));
                                ++DestPointer;
                            }
                        } else {
                            for (int i = 0; i < 2; ++i) {
                                PixelBuffer[DestPointer + 000] = BlendThirdRgb(PixelGrid[1, i], PixelGrid[2, i], BlendHalfRgb(PixelGrid[0, i], PixelGrid[3, i]));
                                PixelBuffer[DestPointer + 160] = BlendThirdRgb(PixelGrid[2, i], PixelGrid[3, i], PixelGrid[4, i]);
                                ++DestPointer;
                            }
                        }
 
                    }
                    DestPointer += 160;
                    SrcPointer += 16 + 512;
                }

                if (this.CurrentBitmapSize.Height == 192) {
                    for (int i = 0; i < 160 * 8; ++i) {
                        PixelBuffer[i] = this.LastBackdropColour;
                        PixelBuffer[i + 0x5500] = this.LastBackdropColour;
                    }
                }

                DataLength = 160 * 144;
            }

            if (this.GenerateOutputBitmap) {
                BitmapData bd = OutputBitmap.LockBits(BlitterRectangle, ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
                Marshal.Copy(PixelBuffer, 0, bd.Scan0, DataLength);
                OutputBitmap.UnlockBits(bd);
            }

            this.RunFramePending = true;

        }

        private int BlendHalfRgb(int a, int b) {
            return unchecked((int)0xFF000000) | ((((a & 0x0000FF) + (b & 0x0000FF)) / 2) & 0x0000FF) | ((((a & 0x00FF00) + (b & 0x00FF00)) / 2) & 0x00FF00) | ((((a & 0xFF0000) + (b & 0xFF0000)) / 2) & 0xFF0000);
        }

        private int BlendQuarterRgb(int a, int b) {
            return unchecked((int)0xFF000000) | ((((a & 0x0000FF) + (b & 0x0000FF) * 3) / 4) & 0x0000FF) | ((((a & 0x00FF00) + (b & 0x00FF00) * 3) / 4) & 0x00FF00) | ((((a & 0xFF0000) + (b & 0xFF0000) * 3) / 4) & 0xFF0000);
        }

        private int BlendEighthRgb(int a, int b) {
            return unchecked((int)0xFF000000) | ((((a & 0x0000FF) + (b & 0x0000FF) * 7) / 8) & 0x0000FF) | ((((a & 0x00FF00) + (b & 0x00FF00) * 7) / 8) & 0x00FF00) | ((((a & 0xFF0000) + (b & 0xFF0000) * 7) / 8) & 0xFF0000);
        }

        private int BlendThirdRgb(int a, int b) {
            return unchecked((int)0xFF000000) | ((((a & 0x0000FF) + (b & 0x0000FF) * 2) / 3) & 0x0000FF) | ((((a & 0x00FF00) + (b & 0x00FF00) * 2) / 3) & 0x00FF00) | ((((a & 0xFF0000) + (b & 0xFF0000) * 2) / 3) & 0xFF0000);
        }
        private int BlendThirdRgb(int a, int b, int c) {
            return unchecked((int)0xFF000000) | 
                   ((((a & 0x0000FF) + (b & 0x0000FF) + (c & 0x0000FF)) / 3) & 0x0000FF)
                 | ((((a & 0x00FF00) + (b & 0x00FF00) + (c & 0x00FF00)) / 3) & 0x00FF00)
                 | ((((a & 0xFF0000) + (b & 0xFF0000) + (c & 0xFF0000)) / 3) & 0xFF0000);
        }

        #endregion

        #region Scanline rasteriser

        /// <summary>The different parts of a frame that a scanline might be in.</summary>
        private enum ScanlinePositionType {
            ActiveDisplay,
            BottomBorder,
            BottomBlanking,
            VerticalBlanking,
            TopBlanking,
            TopBorder,
        }
        /// <summary>The part of the frame the current scanline is in.</summary>
        private ScanlinePositionType ScanlinePosition;

        /// <summary>Counter used to calculate when to move into the next area of the frame.</summary>
        private int RemainingScanlinesCounter;

        /// <summary>Table used to preload the scanline counter when timing frame zones.</summary>
        private int[,] ScreenSpaceCounters = { 
            { 192, 24, 3, 3, 13, 27 },
            { 224, 8,  3, 3, 13, 11 },
            { 240, 0,  0, 0, 0,  0  },
            { 192, 48, 3, 3, 13, 54 },
            { 224, 32, 3, 3, 13, 38 },
            { 240, 24, 3, 3, 13, 30 }
        };

        /// <summary>The Y scroll offset.</summary>
        /// <remarks>The Y scroll offset cannot be changed mid-frame.</remarks>
        private int LockedScrollY;

        /// <summary>Tracks how many scanlines have been generated thus far.</summary>
        private int DoneLinesCounter;

        private int LastLinesCounter;

        private int LastBackdropColour = 0;

        internal bool RunFramePending = false;

        /// <summary>Render a single scanline.</summary>
        public void RasteriseLine() {

            this.CurrentVCounter = ReturnedVCounters[(int)this.DisplayMode, this.CurrentVCounterIndex++];

            int ScanlineY = DoneLinesCounter;
            LastLinesCounter = DoneLinesCounter;

            bool[] ForegroundBackground = new bool[256];

            bool UsingMode4 = false;

            if (ScanlinePosition == ScanlinePositionType.ActiveDisplay) {

                bool amZoomed = VariableZoomSprites;

                int startPixel = ScanlineY * 256;

                if (!VariableDisplayVisible) {
                    // Screen is off
                    switch (CurrentVdpMode) {
                        case VdpMode.Graphic1:
                        case VdpMode.Graphic2:
                        case VdpMode.Multicolour:
                        case VdpMode.Text:
                            this.LastBackdropColour = this.PaletteTMS9918[CapFixedPaletteIndex, this.Registers[0x7] & 0xF];
                            break;
                        default:
                            this.LastBackdropColour = PaletteArgb[(Registers[0x7] & 0xF) + 16];
                            break;
                    }
                    for (int i = 0; i < 256; ++i) this.PixelBuffer[startPixel++] = this.LastBackdropColour;
                } else {

                    int PaletteTMS9918Mode = this.cpu.Type == Cogwheel.Emulation.MachineType.Sg1000 ? 0 : 1;

                    this.LastBackdropColour = PaletteArgb[(Registers[0x7] & 0xF) + 16];

                    switch (CurrentVdpMode) {

                        case VdpMode.Mode4:
                        case VdpMode.Mode4Resolution224:
                        case VdpMode.Mode4Resolution240: {
                                #region Mode 4 background layer

                                // Get name table base address

                                int row = (ScanlineY + LockedScrollY) / 8;
                                if (CurrentBitmapSize.Height == 192) {
                                    row %= 28;
                                } else {
                                    row %= 32;
                                }

                                int nameTableOffset = (row * 64) + (CurrentBitmapSize.Height == 192 ? ((Registers[0x2] & 0xE) * 1024) : ((Registers[0x2] & 0xC) * 1024 + 0x700));

                                int tileNum;
                                int tileOffset;

                                bool flippedX;
                                bool flippedY;
                                int tileRowOffset;

                                int paletteNumber;

                                bool foregroundTile;

                                byte upperByte;

                                byte currentXScroll = VariableInhibitScrollX && (ScanlineY < 16) ? (byte)0 : Registers[0x8];

                                int masterTileRowOffset = ((ScanlineY + LockedScrollY) & 7);

                                int[] colours = new int[8];

                                for (int acol = 0; acol < 32; ++acol) {

                                    int col = (acol - currentXScroll / 8) & 31;
                                    int colExtra = currentXScroll & 7;

                                    

                                    upperByte = vram[nameTableOffset + col * 2 + 1];

                                    flippedX = (upperByte & 0x2) != 0;
                                    flippedY = (upperByte & 0x4) != 0;

                                    paletteNumber = (upperByte & 0x8) * 2;

                                    foregroundTile = (upperByte & 0x10) != 0;

                                    tileNum = vram[nameTableOffset + col * 2] + (upperByte & 1) * 256;
                                    tileOffset = tileNum * 64;


                                    if (flippedY) {
                                        tileRowOffset = 7 - masterTileRowOffset;
                                    } else {
                                        tileRowOffset = masterTileRowOffset;
                                    }

                                    tileOffset += tileRowOffset * 8;


                                    if (!flippedX) {
                                        for (int p = 0; p <= 7; ++p) {
                                            colours[p] = FastPixelColourIndex[tileOffset++];
                                        }
                                    } else {
                                        for (int p = 7; p >= 0; --p) {
                                            colours[p] = FastPixelColourIndex[tileOffset++];
                                        }
                                    }

                                    int o = (acol * 8 + colExtra) & 0xFF;

                                    for (int p = 0; p < 8; p++) {
                                        PixelBuffer[startPixel + o] = PaletteArgb[colours[p] + paletteNumber];
                                        ForegroundBackground[o] = foregroundTile && colours[p] != 0;
                                        if (++o == 256) break;
                                    }
                                    
                                }

                                #endregion
                                UsingMode4 = true;
                            } break;


                        case VdpMode.Graphic1:
                        case VdpMode.Graphic2: {
                            #region Graphic 1/2 background layer

                            this.LastBackdropColour = this.PaletteTMS9918[CapFixedPaletteIndex, this.Registers[0x7] & 0xF];

                            int NameTableStartAddress = (this.Registers[0x2] & 0x0F) << 10;

                            int PatternGeneratorAddress = (this.Registers[0x4] & 0x04) << 11;

                            int CharacterOffset = (DoneLinesCounter / 64) * 0x100;
                            
                            int ColourTableAddress = (this.Registers[0x3] & 0x80) << 6;

                            if (CurrentVdpMode == VdpMode.Graphic1) {
                                NameTableStartAddress = (this.Registers[0x2] & 0x0F) * 0x400;
                                CharacterOffset = 0;
                                PatternGeneratorAddress = (this.Registers[0x4] & 7) << 11;
                                ColourTableAddress = this.Registers[0x3] * 0x40;
                            }

                            int CharacterColour;
                            for (int Column = 0; Column < 32; ++Column) {

                                int CharacterIndex = vram[NameTableStartAddress + Column + (DoneLinesCounter / 8) * 32] + CharacterOffset;

                                int CharacterPixelRow = vram[CharacterIndex * 8 + (DoneLinesCounter & 7) + PatternGeneratorAddress];


                                if (CurrentVdpMode == VdpMode.Graphic1) {
                                    CharacterColour = vram[CharacterIndex / 8 + ColourTableAddress];
                                } else {
                                    CharacterColour = vram[CharacterIndex * 8 + (DoneLinesCounter & 7) + ColourTableAddress];
                                }

                                int ColourForeground = LastBackdropColour;;
                                int ColourBackground = LastBackdropColour;

                                if ((CharacterColour & 0xF0) != 0) ColourForeground = this.PaletteTMS9918[CapFixedPaletteIndex, CharacterColour >> 04];
                                if ((CharacterColour & 0x0F) != 0) ColourBackground = this.PaletteTMS9918[CapFixedPaletteIndex, CharacterColour & 0xF];

                                for (int Pixel = 0; Pixel < 8; ++Pixel) {
                                    PixelBuffer[startPixel + Column * 8 + Pixel] = (CharacterPixelRow & 0x80) != 0 ? ColourForeground : ColourBackground;
                                    CharacterPixelRow <<= 1;
                                }
                            }

                            #endregion
                        } break;

                        case VdpMode.Text: {
                            #region Text background layer

                                int ColourForeground = this.PaletteTMS9918[CapFixedPaletteIndex, this.Registers[0x7] >> 04];
                                int ColourBackground = this.PaletteTMS9918[CapFixedPaletteIndex, this.Registers[0x7] & 0xF];

                            int NameTableStartAddress = (this.Registers[0x2] & 0x0F) << 10;
                            int PatternGeneratorAddress = (this.Registers[0x4] & 0x07) << 11;


                            // Fill in the backdrop colour for the 8 pixels either side that are not in use.
                            for (int i = 0; i < 8; ++i) {
                                PixelBuffer[startPixel + i] = ColourBackground;
                                PixelBuffer[startPixel + i + 248] = ColourBackground;
                            }

                            // Fill in a line of text
                            int PixelPosition = startPixel + 8;
                            for (int Column = 0; Column < 40; ++Column) {

                                int CharacterIndex = vram[NameTableStartAddress + Column + (DoneLinesCounter / 8) * 40];
                                int CharacterPixelRow = vram[CharacterIndex * 8 + (DoneLinesCounter & 7) + PatternGeneratorAddress];


                                for (int j = 0; j < 6; ++j) {
                                    PixelBuffer[PixelPosition++] = (CharacterPixelRow & 0x80) != 0 ? ColourForeground : ColourBackground;
                                    CharacterPixelRow <<= 1;
                                }

                            }


                            #endregion
                        } break;

                        default:
                            #region Random noise
                            for (int i = 0; i < 256; ++i) {
                                int j = PixelNoise.Next(0xFF);
                                PixelBuffer[startPixel + i] = j | (j * 0x100) | (j * 0x10000) | unchecked((int)0xFF000000);
                            }
                            break;
                            #endregion

                    }


                    switch (CurrentVdpMode) {

                        case VdpMode.Mode4:
                        case VdpMode.Mode4Resolution224:
                        case VdpMode.Mode4Resolution240:
                            #region Sprites

                            int SAT = (Registers[5] & 0x7E) * 128;

                            int ec = VariableEarlyClock ? -8 : 0;

                            bool ds = VariableJoinSprites;
                            int sh = (ds ? 16 : 8) * (amZoomed ? 2 : 1);
                            int EigthBit = (Registers[0x6] & 0x4) << 6;

                            bool[] spriteCollisionBuffer = new bool[256];
                            byte pixelX;

                            int maxSprites = CapMaxSpritesPerScaline;
                            int maxZoomedSprites = CapMaxZoomedSpritesPerScanline;

                            for (int i = 0; i < 64; ++i) {

                                int y = vram[SAT + i] + 1;
                                if (y == 0xD1 && CurrentBitmapSize.Height == 192) break;

                                if (y >= 224) y -= 256;

                                if (y > ScanlineY || (y + sh) <= ScanlineY) continue;

                                if (maxSprites-- == 0) {
                                    FlagSpriteOverflow = true;
                                    break;
                                }

                                int x = vram[SAT + i * 2 + 0x80] + ec;
                                int n = vram[SAT + i * 2 + 0x81];

                                if (ds) n &= ~1;

                                int spritePtr = (n + EigthBit) * 64 + ((ScanlineY - y) / (amZoomed ? 2 : 1)) * 8;

                                int[] colours = new int[8];

                                for (int p = 0; p < 8; ++p) {
                                    colours[p] = this.FastPixelColourIndex[spritePtr++];
                                }

                                pixelX = (byte)x;

                                if (amZoomed && (--maxZoomedSprites > 0)) {
                                    for (int p = 0; p < 16; ++p) {
                                        if (!ForegroundBackground[pixelX]) {
                                            if (colours[p / 2] != 0) {
                                                if (spriteCollisionBuffer[pixelX]) {
                                                    FlagSpriteCollision = true;
                                                } else {
                                                    spriteCollisionBuffer[pixelX] = true;
                                                    PixelBuffer[startPixel + pixelX] = PaletteArgb[colours[p / 2] + 16];
                                                }
                                            }
                                        }
                                        if (pixelX == 0xFF) break;
                                        ++pixelX;
                                    }
                                } else {
                                    for (int p = 0; p < 8; ++p) {
                                        if (!ForegroundBackground[pixelX]) {
                                            if (colours[p] != 0) {
                                                if (spriteCollisionBuffer[pixelX]) {
                                                    FlagSpriteCollision = true;
                                                } else {
                                                    spriteCollisionBuffer[pixelX] = true;
                                                    PixelBuffer[startPixel + pixelX] = PaletteArgb[colours[p] + 16];
                                                }
                                            }
                                        }
                                        if (pixelX == 0xFF) break;
                                        ++pixelX;
                                    }
                                }

                            }

                            #endregion
                            break;

                        case VdpMode.Graphic1:
                        case VdpMode.Graphic2:
                            #region Sprites

                            bool[] SpriteCollisions = new bool[320];
                            bool[] DrawnPixel = new bool[256];

                            int SpriteAttributeTable = (this.Registers[0x5] & 0x7F) * 0x80;
                            int SpritePatternGenerator = (this.Registers[0x6] & 0x07) * 0x800;
                            int SpritePerScanlineCap = CapMaxSpritesPerScaline;

                            bool MagMode = (this.Registers[0x1] & 0x1) != 0;
                            bool SizeMode = (this.Registers[0x1] & 0x2) != 0;

                            int SpriteSize = SizeMode ? (MagMode ? 32 : 16) : (MagMode ? 16 : 8);
                            int HalfSize = SpriteSize / 2 - 1;

                            for (int SpriteNum = 0; SpriteNum < 32; ++SpriteNum) {
                                int SpriteY = this.vram[SpriteAttributeTable++];
                                int SpriteX = this.vram[SpriteAttributeTable++];
                                int SpriteIndex = this.vram[SpriteAttributeTable++];
                                int SpriteFlags = this.vram[SpriteAttributeTable++];

                                if (SpriteY == 0xD0) break; // Special terminator value.

                                // Sprites can bleed off the top of the display:
                                if (SpriteY > 224) SpriteY -= 256;

                                // Sprites appear one scanline below the declared line:
                                ++SpriteY;

                                // Handle Early Clock bit (shift sprite left 32 pixels if set).
                                if ((SpriteFlags & 0x80) != 0) SpriteX -= 32;

                                // Is the sprite visible on this scanline?
                                if (SpriteY > DoneLinesCounter || (SpriteY + SpriteSize) <= DoneLinesCounter) continue;

                                // Check we haven't drawn too many sprites on this scanline!
                                if (SpritePerScanlineCap-- == 0) {
                                    FlagSpriteOverflow = true;
                                    InvalidSpriteIndex = SpriteNum;
                                    break;
                                }

                                if (SizeMode) SpriteIndex &= 0xFC;
                                int SpriteTextureCoord = DoneLinesCounter - SpriteY;
                                if (MagMode) SpriteTextureCoord /= 2;

                                int SpritePatternDataAddress = SpritePatternGenerator + SpriteIndex * 8 + SpriteTextureCoord;
                                int SpritePixelRow = this.vram[SpritePatternDataAddress];

                                for (int x = 0; x < SpriteSize; ++x) {

                                    // Position of the pixel on-screen.
                                    int PixelOffset = x + SpriteX;

                                    // Check for collisions
                                    if (SpriteCollisions[PixelOffset + 32]) {
                                        FlagSpriteCollision = true;
                                    } else {
                                        SpriteCollisions[PixelOffset + 32] = true;
                                    }

                                    if (PixelOffset >= 0 && PixelOffset < 256 && !DrawnPixel[PixelOffset]) {
                                        bool PixelSet = (SpritePixelRow & 0x80) != 0;
                                        if (PixelSet && (SpriteFlags & 0x0F) != 0) {
                                            PixelBuffer[startPixel + PixelOffset] = this.PaletteTMS9918[CapFixedPaletteIndex, SpriteFlags & 0x0F];
                                            DrawnPixel[PixelOffset] = true;
                                        }
                                    }

                                    // Move to next pixel:
                                    if (!MagMode || (x & 1) == 1) {
                                        if (SizeMode && x == HalfSize) {
                                            SpritePixelRow = this.vram[SpritePatternDataAddress + 16];
                                        } else {
                                            SpritePixelRow <<= 1;
                                        }

                                    }


                                }

                            }

                            #endregion
                            break;
                    }

                    if (UsingMode4 && VariableMaskColumn0) {
                        for (int i = 0; i < 8; i++) PixelBuffer[startPixel + i] = LastBackdropColour;
                    }

                } 

            }


            ++DoneLinesCounter;

            if (this.CapSupportsLineInterrupts) {
                if (DoneLinesCounter <= CurrentBitmapSize.Height) {
                    if (LineInterruptCounter-- <= 0) {
                        FlagLineInterruptPending = true;
                        LineInterruptCounter = RegisterLineInterruptCounter;
                    }
                } else {
                    LineInterruptCounter = RegisterLineInterruptCounter;
                }
            }

            if (VBlankPending == 1) {
                ++VBlankPending;
            } else if (VBlankPending == 2) {
                VBlankPending = 0;
                FinishFrame();
                OnVerticalBlank(new BlankEventArgs(true, this.PixelBuffer, this.OutputBitmap, Color.FromArgb(unchecked((int)(0xFF000000 + LastBackdropColour)))));
                FlagFrameInterruptPending = true;
            }

            // Well, that was that...
            if (--RemainingScanlinesCounter == 0) {
                // Move on to the next part of the display!
                while (RemainingScanlinesCounter == 0) {
                    ScanlinePosition = (ScanlinePositionType)(((int)ScanlinePosition + 1) % 6);
                    RemainingScanlinesCounter = ScreenSpaceCounters[(int)DisplayMode, (int)ScanlinePosition];
                }
                // What are we doing now?
                switch (ScanlinePosition) {
                    case ScanlinePositionType.ActiveDisplay:
                        StartFrame();
                        break;
                    case ScanlinePositionType.BottomBorder:
                        VBlankPending = 1;
                        break;
                }
            }





            UpdateIRQ();
        }
        int VBlankPending;

        #endregion

        #region Output format

        private enum DisplayType {
            NTSC192, NTSC224, NTSC240,
            PAL192, PAL224, PAL240,
        }

        #endregion

        #region Vertical line counter

        private byte[,] ReturnedVCounters;

        private void SetupVCounter() {

            ReturnedVCounters = new byte[6, 313];

            for (int i = 0; i < 6; ++i) {
                int VCountReturn = 0;
                for (int j = 0; j < 313; ++j) {
                    ReturnedVCounters[i, j] = (byte)VCountReturn;
                    ++VCountReturn;
                    switch ((DisplayType)i) {
                        case DisplayType.NTSC192: if (VCountReturn == 0xDB) VCountReturn = 0x1D5; break;
                        case DisplayType.NTSC224: if (VCountReturn == 0xEB) VCountReturn = 0x1E5; break;
                        case DisplayType.PAL192: if (VCountReturn == 0xF3) VCountReturn = 0x1BA; break;
                        case DisplayType.PAL224: if (VCountReturn == 0x103) VCountReturn = 0x1CA; break;
                        case DisplayType.PAL240: if (VCountReturn == 0x10B) VCountReturn = 0x1D2; break;
                    }
                }
            }
        }

        private int CurrentVCounter;
        private int CurrentVCounterIndex;
        public byte VCounter {
            get { return (byte)(CurrentVCounter); }
        }

        #endregion

        private Random PixelNoise = new Random();


    }
}
