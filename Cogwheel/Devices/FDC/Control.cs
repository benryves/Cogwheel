using System;
using System.Collections.Generic;
using System.Text;

namespace BeeDevelopment.Sega8Bit.Devices {
    public partial class FDC {

        /// <summary>Command codes used when controlling the FDC.</summary>
        private enum CommandCode {
            ReadData             = 0x06,
            ReadDeletedData      = 0x0C,
            WriteData            = 0x05,
            WriteDeletedData     = 0x09,
            ReadDiagnostic       = 0x02,
            ReadId               = 0x0A,
            WriteId              = 0x0D,
            ScanEqual            = 0x11,
            ScanLowOrEqual       = 0x19,
            ScanHighOrEqual      = 0x1D,
            Recalibrate          = 0x07,
            SenseInterruptStatus = 0x08,
            Specify              = 0x03,
            SenseDriveStatus     = 0x04,
            Version              = 0x10,
            Seek                 = 0x0F,
        }

        /// <summary>Retrieve the total length of a command.</summary>
        /// <param name="command">The command to find the length of.</param>
        /// <returns>The length of the command in bytes.</returns>
        private int GetCommandLength(CommandCode command) {
            switch (command) {

                case CommandCode.ReadData:
                case CommandCode.ReadDeletedData:
                case CommandCode.WriteData:
                case CommandCode.WriteDeletedData:
                case CommandCode.ReadDiagnostic:
                case CommandCode.ScanEqual:
                case CommandCode.ScanLowOrEqual:
                case CommandCode.ScanHighOrEqual:                
                    return 9;
                
                case CommandCode.WriteId:
                    return 6;
                
                case CommandCode.Specify:
                case CommandCode.Seek:
                    return 3;
                
                case CommandCode.ReadId:
                case CommandCode.Recalibrate:
                case CommandCode.SenseDriveStatus:                
                    return 2;

                case CommandCode.SenseInterruptStatus:
                case CommandCode.Version:
                default:
                    return 1;
            }
        }


        private byte[] CommandData = new byte[9];
        private int CommandDataIndex = 0;

        public void WriteData(byte value) {

            if (CommandDataIndex == 0) {
                // Starting a new command.
                CurrentMainStatus |= MainStatus.FdcBusy;
                CommandData[CommandDataIndex++] = (byte)(value & 0x1F);
            } else {
                // Adding to the current command.
                CommandData[CommandDataIndex++] = value;
            }

            if (CommandDataIndex >= GetCommandLength((CommandCode)CommandData[0])) {
                // Command is complete!
                CommandDataIndex = 0; // Prepare for the next command.
                CurrentMainStatus |= MainStatus.ExecutionModeBusy;
            }

        }

        private void ExecuteData(byte value) {

            // Work out which drive we're working with.

            switch ((CommandCode)CommandData[0]) {
                case CommandCode.ReadDiagnostic:
                    
                    WorkingDrive.CurrentSide = CommandHead;
                    WorkingDrive.CurrentTrack = CommandCylinderNumber;

                    if (!WorkingDrive.DiskInserted) {
                        CurrentStatus0 |= Status0.NotReady | Status0.EquipmentCheck | (Status0)(0xD0 | WorkingDrive.DriveNumber);
                    } else {

                    }

                    
                    break;
            }

        }

        private Drive WorkingDrive {
            get { return this.Drives[CommandUnitSelect]; }
        }


        /// <summary>Unit Select stands for a selected drive number 0..3.</summary>
        private int CommandUnitSelect { // US0..US1
            get { return this.CommandData[1] & 3; }
        }

        /// <summary>Head stands for the logical head number 0 or 1 and controls the polarity of pin 27.</summary>
        /// <remarks>Head address, H equals Head, HD in all command words.</remarks>
        private int CommandHead { // HD
            get { return (this.CommandData[1] >> 2) & 1; }
        }

        private byte CommandCylinderNumber { // C
            get { return this.CommandData[2]; }
        }
        private byte CommandHeadAddress { // H
            get { return this.CommandData[3]; }
        }
        private byte CommandRecord { // R
            get { return this.CommandData[4]; }
        }
        private byte CommandNumber { // N
            get { return this.CommandData[5]; }
        }
        private byte CommandEndOfTrack { // EOT
            get { return this.CommandData[6]; }
        }
        private byte CommandGapLength { // GPL
            get { return this.CommandData[7]; }
        }
        private byte CommandDataLength { // DTL
            get { return this.CommandData[8]; }
        }


    }
}
