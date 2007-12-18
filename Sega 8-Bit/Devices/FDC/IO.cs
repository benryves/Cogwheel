using System;

namespace BeeDevelopment.Sega8Bit.Devices {
    public partial class FDC {

        

        #region Pins

        /* PA0 FDC INT [signal from input from FDC]
         * PA2 FDC Pin 17 of the FDC ? [index?]
         * PC0 FDD /INUSE
         * PC1 FDD /MOTOR ON
         * PC2 FDD TC signal
         * PC3 FDC RESET signal
         * 
         * Port 0xE0 -> status
         * Port 0xE1 -> data
         */

        /// <summary>RESET (Reset) input pin.</summary>
        /// <remarks>The RESET input places the FDC in the idle state.</remarks>
        public bool PinReset {
            set {

            }
        }

        #endregion

        #region Ports

        



        public byte ReadData() {
            byte Value = 0;
            switch ((CommandCode)CommandData[0]) {
                
                case CommandCode.ReadDiagnostic:
                case CommandCode.ReadData:

                
                    break;

                default:
                    throw new Exception();
            }
            return Value;
        }

        #endregion

        public void Reset() {
        }

    }
}
