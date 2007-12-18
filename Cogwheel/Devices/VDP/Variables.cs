using System;
using System.Collections.Generic;
using System.Text;

namespace BeeDevelopment.Cogwheel.Devices {
    public partial class VideoDisplayProcessor {

        #region Register $00 flags

        /// <summary>
        /// Set to disable vertical scrolling for columns 24-31.
        /// </summary>
        public bool VariableInhibitScrollY {
            get { return GetBit(0x0, 7); }
            set { AdjustBit(0x0, 7, value); }
        }

        /// <summary>
        /// Set to disable horizontal scrolling for rows 0-1.
        /// </summary>
        public bool VariableInhibitScrollX {
            get { return GetBit(0x0, 6); }
            set { AdjustBit(0x0, 6, value); }
        }

        /// <summary>
        /// Set to mask column 0 with border colour.
        /// </summary>
        public bool VariableMaskColumn0 {
            get { return GetBit(0x0, 5); }
            set { AdjustBit(0x0, 5, value); }
        }

        /// <summary>
        /// (IE1) Set to enable raster line interrupts.
        /// </summary>
        public bool VariableLineInterruptEnable {
            get { return GetBit(0x0, 4); }
            set { AdjustBit(0x0, 4, value); }
        }

        /// <summary>
        /// (EC) Set to shift sprites left by 8 pixels.
        /// </summary>
        public bool VariableEarlyClock {
            get { return GetBit(0x0, 3); }
            set { AdjustBit(0x0, 3, value); }
        }

        /// <summary>
        /// (M4) Set to use mode 4, reset to use TMS9918 modes.
        /// </summary>
        public bool VariableUseMode4 {
            get { return GetBit(0x0, 2); }
            set { AdjustBit(0x0, 2, value); }
        }

        /// <summary>
        /// (M3) Set to allow mode 4 screen height changes.
        /// </summary>
        public bool VariableAllowMode4HeightChanges {
            get { return GetBit(0x0, 1); }
            set { AdjustBit(0x0, 1, value); }
        }

        /// <summary>
        /// Set to disable synch.
        /// </summary>
        public bool VariableNoSynch {
            get { return GetBit(0x0, 0); }
            set { AdjustBit(0x0, 0, value); }
        }

        #endregion

        #region Register $01 flags

        /// <summary>
        /// (BLK) Set to make the display visible, reset to blank it.
        /// </summary>
        public bool VariableDisplayVisible {
            get { return GetBit(0x1, 6); }
            set { AdjustBit(0x1, 6, value); }
        }

        /// <summary>
        /// (IE0) Set to enable frame interrupts.
        /// </summary>
        public bool VariableFrameInterruptEnable {
            get { return GetBit(0x1, 5); }
            set { AdjustBit(0x1, 5, value); }
        }

        /// <summary>
        /// (M1) Set to use 224-line screen when in Mode 4.
        /// </summary>
        /// <remarks>M2 must also be set to allow the change.</remarks>
        public bool VariableUse224LineMode {
            get { return GetBit(0x1, 4); }
            set { AdjustBit(0x1, 4, value); }
        }

        /// <summary>
        /// (M3) Set to use 240-line screen when in Mode 4.
        /// </summary>
        /// <remarks>M2 must also be set to allow the change.</remarks>
        public bool VariableUse240LineMode {
            get { return GetBit(0x1, 3); }
            set { AdjustBit(0x1, 3, value); }
        }

        /// <summary>
        /// Set to join sprites (16x16 in TMS9918 modes, 8x16 in mode 4).
        /// </summary>
        public bool VariableJoinSprites {
            get { return GetBit(0x1, 1); }
            set { AdjustBit(0x1, 1, value); }
        }

        /// <summary>
        /// Set to zoom sprites to double size.
        /// </summary>
        public bool VariableZoomSprites {
            get { return GetBit(0x1, 0); }
            set { AdjustBit(0x1, 0, value); }
        }

        #endregion

        [Flags]
        private enum ModeFlags {
            None = 0x00,
            M2 = 0x02,
            M4 = 0x04,
            M3 = 0x08,
            M1 = 0x10,
        }

        private ModeFlags CurrentDisplayModeFlags {
            get { return (ModeFlags)((Registers[0x0] & 0x06) | (Registers[0x1] & 0x18)); }
        }

        private int VariableScreenHeight {
            get {
                if (!this.CapSupportsExtendedResolutions) {
                    return 192;
                } else {
                    ModeFlags M = CurrentDisplayModeFlags;

                    // If M2 is not set or M4 is not set, standard resolution.
                    if ((M & ModeFlags.M2) == 0 || (M & ModeFlags.M4) == 0) return 192;

                    // If M1 and M3 are both set, standard resolution.
                    if ((M & (ModeFlags.M1 | ModeFlags.M3)) == (ModeFlags.M1 | ModeFlags.M3)) {
                        return 192;
                    } else if ((M & ModeFlags.M1) == ModeFlags.M1) {
                        return 224;
                    } else if ((M & ModeFlags.M3) == ModeFlags.M3) {
                        return 240;
                    } else {
                        return 192;
                    }
                }
            }
        }

        public enum VdpMode {
            Graphic1 = 0,
            Text = 1,
            Graphic2 = 2,
            UndocumentedModes1And2 = 3,
            Multicolour = 4,
            UndocumentedModes1And3 = 5,
            UndocumentedModes2And3 = 6,
            UndocumentedModes1And2And3 = 7,
            Mode4 = 8,
            InvalidTextMode = 9,
            Mode4Resolution224 = 11,
            Mode4Resolution240 = 14,
        }

        public VdpMode Mode {
            get {

                ModeFlags MaskedFlags = CurrentDisplayModeFlags;
                if (!this.CapSupportsMode4) MaskedFlags &= ~ModeFlags.M4;

                switch (MaskedFlags) {

                    case ModeFlags.None:
                        return VdpMode.Graphic1;
                    case ModeFlags.M1:
                        return VdpMode.Text;
                    case ModeFlags.M2:
                        return VdpMode.Graphic2;
                    case ModeFlags.M3:
                        return VdpMode.Multicolour;

                    case ModeFlags.M1 | ModeFlags.M2:
                        return VdpMode.UndocumentedModes1And2;
                    case ModeFlags.M2 | ModeFlags.M3:
                        return VdpMode.UndocumentedModes2And3;
                    case ModeFlags.M1 | ModeFlags.M2 | ModeFlags.M3:
                        return VdpMode.UndocumentedModes1And2And3;

                    case ModeFlags.M4:
                    case ModeFlags.M4 | ModeFlags.M2:
                    case ModeFlags.M4 | ModeFlags.M3:
                        return VdpMode.Mode4;

                    case ModeFlags.M4 | ModeFlags.M1 | ModeFlags.M2:
                        return this.CapSupportsExtendedResolutions ?
                            VdpMode.Mode4Resolution224 : VdpMode.InvalidTextMode;

                    case ModeFlags.M4 | ModeFlags.M1 | ModeFlags.M2 | ModeFlags.M3:
                        return this.CapSupportsExtendedResolutions ?
                            VdpMode.Mode4 : VdpMode.InvalidTextMode;

                    default:
                        return VdpMode.InvalidTextMode;

                }
            }
        }

        #region Register bit manipulation and testing functions

        private bool GetBit(int register, int bitIndex) {
            return (Registers[register] & (1 << bitIndex)) != 0;
        }
        private void SetBit(int register, int bitIndex) {
            Registers[register] |= (byte) (1 << bitIndex);
        }
        private void ResetBit(int register, int bitIndex) {
            Registers[register] &= (byte)~(1 << bitIndex);
        }
        private void AdjustBit(int register, int bitIndex, bool set) {
            if (set) {
                SetBit(register, bitIndex);
            } else {
                ResetBit(register, bitIndex);
            }
        }

        #endregion
    }
}
