using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Cogwheel.Devices {
    public partial class Floppy {

        /// <summary>A side of a floppy disk can store up to 160KB (163840 bytes).</summary>
        public const int MaximumCapacity = 0x28000;

        private byte[] data;
        public byte[] Data {
            get {
                return this.data;
            }
            set {
                this.data = value;
                Array.Resize<byte>(ref this.data, Floppy.MaximumCapacity);
            }
        }

        public Floppy() {
            this.data = new byte[Floppy.MaximumCapacity];
        }

        public Floppy(Stream diskImage) 
            : this() {
            this.Load(diskImage);
        }

        public Floppy(string diskImage)
            : this() {
            this.Load(diskImage);
        }

        /// <summary>Gets a value indicating whether this disk is a bootable system disk or not.</summary>
        public bool IsSystemDisk {
            get {
                return (this.data[0x0] == (byte)'S')
                    && (this.data[0x1] == (byte)'Y')
                    && (this.data[0x2] == (byte)'S')
                    && (this.data[0x3] == (byte)':');
            }
        }

        /// <summary>Gets the caption displayed when the system starts up from a system disk.</summary>
        /// <remarks>An exception is thrown if you try to get the caption from a non-system disk.</remarks>
        public string SystemBootCaption {
            get {
                if (!this.IsSystemDisk) throw new Exception("This is not a system disk.");
                StringBuilder SB = new StringBuilder(25);
                for (int i = 0x04; i < 0x20; ++i) {
                    if (this.data[i] == 0) break;
                    SB.Append((char)this.data[i]);
                }
                return SB.ToString();
            }
        }

        /// <summary>Load a disk image from an input stream.</summary>
        /// <param name="input">The stream to load the disk image from.</param>
        public void Load(Stream input) {
            this.data = new byte[Floppy.MaximumCapacity];
            input.Read(this.data, 0, Floppy.MaximumCapacity);
        }

        /// <summary>Load a disk image from a file.</summary>
        /// <param name="input">The file to load the disk image from.</param>
        /// <remarks>Compressed files are not supported.</remarks>
        public void Load(string filename) {
            using (Stream s = System.IO.File.OpenRead(filename)) 
                this.Load(s);
        }


    }
}