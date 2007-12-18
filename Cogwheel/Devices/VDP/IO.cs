using System;
using System.Collections.Generic;
using System.Text;

namespace BeeDevelopment.Cogwheel.Devices {
    
    public partial class VideoDisplayProcessor {

        

        /// <summary>
        /// Waiting for the second part of the control byte
        /// </summary>
        public bool WaitingForSecond = false;


        /// <summary>
        /// The current address
        /// </summary>
        private ushort AddressRegister;

        public enum CodeType {
            ReadWriteVRAM,
            WriteVRAM,
            RegisterWrite,
            CRAMWrite,
        }

        public CodeType CodeRegister = CodeType.ReadWriteVRAM;

        /// <summary>
        /// Write a byte of data to the data port
        /// </summary>
        /// <param name="data">Data to write</param>
        public void WriteData(byte data) {
            WaitingForSecond = false;
            ReadDataBuffer = data;
            if (CodeRegister == CodeType.CRAMWrite) {
                WritePalette(data);
            } else {
                vram[AddressRegister & 0x3FFF] = data;
                int ModifiedTileSlice = ((AddressRegister & 0x3FFF) / 4) * 8;
                int PlaneBitmask = 1 << (AddressRegister & 3);
                int InverseBitmask = ~PlaneBitmask;
                for (int i = 0; i < 8; ++i) {
                    if ((data & 0x80) == 0) {
                        FastPixelColourIndex[ModifiedTileSlice] &= InverseBitmask;
                    } else {
                        FastPixelColourIndex[ModifiedTileSlice] |= PlaneBitmask;                        
                    }
                    ++ModifiedTileSlice;
                    data <<= 1;
                }
                ++AddressRegister;
            }
        }

        /// <summary>
        /// Read a byte from the data port
        /// </summary>
        /// <returns>Data read</returns>
        public byte ReadData() {
            WaitingForSecond = false;
            byte b = ReadDataBuffer;
            ReadDataBuffer = vram[AddressRegister++ & 0x3FFF];
            return b;
        }

        private byte ReadDataBuffer = 0;
        /// <summary>
        /// Write a byte of data to the control port
        /// </summary>
        /// <param name="data">Data to write</param>
        public void WriteControl(byte data) {

            if (WaitingForSecond) {
                CodeRegister = (CodeType)(data >> 6);

                AddressRegister = (ushort)((AddressRegister & 0x00FF) + (data << 8));

                ReadDataBuffer = vram[AddressRegister & 0x3FFF];

                switch (CodeRegister) {
                    case CodeType.RegisterWrite:
                        Registers[data & 0xF] = (byte)(AddressRegister & 0xFF);
                        UpdateIRQ();
                        break;
                    case CodeType.ReadWriteVRAM:
                        ++AddressRegister;
                        break;

                }                
                WaitingForSecond = false;
            } else {
                AddressRegister = (ushort)((AddressRegister & 0xFF00) + data);
                WaitingForSecond = true;
            }
        }

        /// <summary>
        /// Read a byte from the control port
        /// </summary>
        /// <returns>Data read</returns>
        public byte ReadControl() {

            WaitingForSecond = false;
            byte b = (byte)(
                (FlagFrameInterruptPending ? 0x80 : 0x00) +
                (FlagSpriteOverflow ? 0x40 : 0x00) +
                (FlagSpriteCollision ? 0x20 : 0x00) +
                InvalidSpriteIndex
            );

            //if ((b & 0x80) == 0 && VariableDisplayVisible) Console.WriteLine();

            FlagFrameInterruptPending = false;
            FlagLineInterruptPending = false;
            FlagSpriteOverflow = false;
            FlagSpriteCollision = false;            
            UpdateIRQ();

            

            return b;
        }




    }
}
