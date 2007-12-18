using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace BeeDevelopment.Sega8Bit.Devices {
    public partial class FDC {
        
        public class Drive {

            public readonly int DriveNumber;

            public Drive(int driveNumber) {
                this.DriveNumber = driveNumber;

            }

            private byte[] data;

            private bool diskInserted = false;
            public bool DiskInserted {
                get { return this.diskInserted; }
                set { this.diskInserted = value; }
            }

            /// <summary>Insert a floppy disk into the emulated drive.</summary>
            /// <param name="data">Stream to read the disk image from.</param>
            public void InsertDisk(Stream data) {
                data.Read(this.data, 0, FDC.DiskSizeBytes);
                Array.Resize<byte>(ref this.data, FDC.DiskSizeBytes);
                this.diskInserted = true;
            }

            /// <summary>Remove a floppy disk from the emulated drive.</summary>
            public void RemoveDisk() {
                this.diskInserted = false;
            }

            public int CurrentSide = 0;
            public int CurrentTrack = 0;

            public byte ReadData(int track, int sector, int offset) {
                return this.data[track * FDC.TrackSizeBytes + sector * FDC.SectorSizeBytes + offset];
            }

        }
    }
}
