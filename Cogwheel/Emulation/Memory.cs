using System;
using System.IO;
using System.Collections.Generic;

namespace BeeDevelopment.Cogwheel.Emulation {
    public partial class Sega8Bit {

        /// <summary>
        /// Cached inserted cartridge
        /// </summary>
        private byte[] InsertedCartridge = null;

        private byte[][] MemoryModel;
        
        private byte[][] CartridgeRom;
        private byte[] CartridgeRam;

        private int BankNumberOffset = 0;
        private bool CartridgeRamEnabled = false;
        private bool WorkRamEnabled = true;

        public void ResetMemory() {

            this.BankNumbers = new int[3];

            this.CartridgeRom = new byte[1][];
            PadCartSize();

            this.MemoryModel = new byte[4][];
            for (int i = 0; i < 4; ++i) this.MemoryModel[i] = new byte[0x4000];

            this.CartridgeRam = new byte[0x4000];

            this.WorkRamEnabled = true;
            this.CartridgeRamEnabled = false;

            if (this.InsertedCartridge != null) {
                using (MemoryStream M = new MemoryStream(this.InsertedCartridge)) this.LoadCartridge(M);
            }
        }

        private void CopyCartridge(Stream s) {

            // Create a temporary list to store the data.
            List<byte[]> ReadingData = new List<byte[]>();

            // Read the data in 16KB (1 page) chunks.
            for (; ; ) {

                // Array to store the data
                byte[] Page = new byte[0x4000];

                // Read 16KB
                int BytesRead = s.Read(Page, 0, Page.Length);

                // No more data!
                if (BytesRead == 0) break;

                // If we're not 16KB large, pad.
                if (BytesRead != 0x4000) Array.Resize<byte>(ref Page, 0x4000);

                // Add this bank.
                ReadingData.Add(Page);
            }

            // Convert to an array...
            this.CartridgeRom = ReadingData.ToArray();

            if (this.CartridgeRom.Length == 0) throw new Exception("No data in cartridge.");

            PadCartSize();

            // Standard mapper initialisation
            WriteMemory(0xFFFC, 0);
            WriteMemory(0xFFFD, 0);
            WriteMemory(0xFFFE, 1);
            WriteMemory(0xFFFF, 2);

            // Codemasters initialisation
            WriteMemory(0x0000, 0);
            WriteMemory(0x4000, 1);
            WriteMemory(0x8000, 2);


        }


        public void LoadCartridge(Stream s) {
            this.InsertedCartridge = new byte[s.Length];
            s.Read(this.InsertedCartridge, 0, (int)s.Length);
            s.Seek(0, SeekOrigin.Begin);
            this.CopyCartridge(s);
        }

        private void PadCartSize() {
            int PadToTwo = 2;
            while (PadToTwo < this.CartridgeRom.Length) PadToTwo <<= 1;
            Array.Resize<byte[]>(ref this.CartridgeRom, PadToTwo);
            for (int i = 0; i < this.CartridgeRom.Length; ++i) {
                if (this.CartridgeRom[i] == null) {
                    CartridgeRom[i] = new byte[0x4000];
                } else if (this.CartridgeRom[i].Length != 0x4000) {
                    Array.Resize<byte>(ref this.CartridgeRom[i], 0x4000);
                }
            }
        }


        public override byte ReadMemory(ushort address) {
            switch (address & 0xC000) {
                case 0x0000:
                    if (address < 1024) {
                        return this.CartridgeRom[0][address];
                    } else {
                        return this.MemoryModel[0][address & 0x3FFF];
                    }
                case 0x4000:
                    return this.MemoryModel[1][address & 0x3FFF];
                case 0x8000:
                    return this.MemoryModel[2][address & 0x3FFF];
                case 0xC000:
                    if (this.WorkRamEnabled) {
                        return this.MemoryModel[3][address & 0x1FFF]; // 8KB RAM
                    } else {
                        break;
                    }
            }
            return 0xFF;
        }

        private int[] BankNumbers = new int[3];

        public override void WriteMemory(ushort address, byte value) {

            if (address >= 0xC000) {
                // Writes to RAM
                if (this.WorkRamEnabled) this.MemoryModel[3][address & 0x1FFF] = value;
            }

            // Handle standard mapper paging stuff.
            if (this.Mapper == MapperType.Standard) {

                if (address == 0xFFFC) {

                    this.BankNumberOffset = (value & 3) * 8;

                    if ((value & 0x08) != 0) {
                        // Cartridge RAM enabled
                        this.CartridgeRamEnabled = true;
                        this.MemoryModel[2] = CartridgeRam;
                    } else {
                        // Cartridge ROM enabled
                        this.CartridgeRamEnabled = false;
                        this.MemoryModel[2] = this.CartridgeRom[this.BankNumbers[2]];
                    }

                    this.WorkRamEnabled = (value & 0x10) == 0;

                } else if (address > 0xFFFC) {
                    int SwitchedBank = address - 0xFFFD;
                    this.BankNumbers[SwitchedBank] = (value + BankNumberOffset) % this.CartridgeRom.Length;

                    if (!(SwitchedBank == 2 && this.CartridgeRamEnabled)) {
                        this.MemoryModel[SwitchedBank] = this.CartridgeRom[this.BankNumbers[SwitchedBank]];
                    }
                }

                if (this.CartridgeRamEnabled && (address & 0xC000) == 0x8000) {
                    this.CartridgeRam[address & 0x3FFF] = value;
                }
            } else if (this.Mapper == MapperType.Codemasters) {
                switch (address) {
                    case 0x0000:
                    case 0x4000:
                    case 0x8000:
                        int SwitchedBank = address / 0x4000;
                        this.BankNumbers[SwitchedBank] = value % this.CartridgeRom.Length;
                        this.MemoryModel[SwitchedBank] = this.CartridgeRom[this.BankNumbers[SwitchedBank]];
                        break;
                }
            }

        }

        /// <summary>Unload a cartridge from the slot.</summary>
        public void UnloadCartridge() {
            this.InsertedCartridge = null;
        }

        #region TODO



        public void WriteToMemoryControl(byte value) {

        }


        public bool IgnoreBios;
        public void LoadBios(Stream s) { }
        public enum MapperType {
            Ram,
            Standard,
            Codemasters,
        }
        public MapperType Mapper = MapperType.Standard;
        public bool AutodetectMapper;

        #endregion


    }
}
