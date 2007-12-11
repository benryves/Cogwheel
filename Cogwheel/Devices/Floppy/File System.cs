using System;
using System.Collections.Generic;
using System.Text;

namespace Cogwheel.Devices {
    public partial class Floppy {

        public class DirectoryEntry {


            /// <summary></summary>
            public enum FileAttribute {
                NonAscii = 0x00,
                Ascii = 0x01,
                HexadecimalData = 0x02,
                ReadOnlyNonAscii = 0x80,
                ReadOnlyAscii = 0x81,
                ReadOnlyHexadecimalData = 0x82,
            } 

            public FileAttribute Attribute = FileAttribute.NonAscii;

            public bool IsReadonly {
                get {
                    return ((int)this.Attribute & 0x80) != 0;
                }
            }           

            public string Filename;
            public string Extension;

            public int ClusterGroupIndex;

            public readonly Floppy Disk;

            public DirectoryEntry(Floppy disk, byte[] directoryEntry) {

                this.Disk = disk;

                // Check for a valid entry
                if (directoryEntry.Length != 16) throw new ArgumentException("The directory entry must be 16 bytes long.");

                StringBuilder Names = new StringBuilder(8);

                // Get the filename
                for (int i = 0; i < 8; ++i) {
                    if (directoryEntry[i] == 0x00) break;
                    Names.Append((char)directoryEntry[i]);
                }
                this.Filename = Names.ToString();
                Names.Length = 0;

                // Get the extension
                for (int i = 9; i < 12; ++i) {
                    if (directoryEntry[i] == 0x00) break;
                    Names.Append((char)directoryEntry[i]);
                }
                this.Extension = Names.ToString();

                // Grab cluster index
                this.ClusterGroupIndex = directoryEntry[12];

                // Set attribute
                this.Attribute = (FileAttribute)directoryEntry[13];
                
            }

            public override string ToString() {
                return this.Filename.TrimEnd() + "." + this.Extension + " + " + this.Attribute;
            }

            public byte[] Data {
                get {
                    List<byte> Result = new List<byte>();

                    for (int Cluster = this.ClusterGroupIndex; Cluster < 160; ++Cluster) {
                        byte FatRecord = this.Disk.Data[Floppy.FatIndex + Cluster];
                        if (FatRecord < 0xA0) {
                            // Clusters in use.
                        } else if (FatRecord > 0xC0 && FatRecord < 0xC5) {
                            // Last clusters in use.
                        } else if (FatRecord == 0xFE) {
                            // Reserved.
                        } else if (FatRecord == 0xFF) {
                            // Unused cluster.
                        }
                    }

                    return Result.ToArray();
                }
            }

        }

        public DirectoryEntry[] GetDirectoryEntries() {
            List<DirectoryEntry> Result = new List<DirectoryEntry>();

            byte[] DirectoryEntry = new byte[16];


            int TrackOffset = 20 * Floppy.TrackSizeBytes;

            for (int Sector = 0; Sector < 12; ++Sector) {
                for (int FileOffset = 0; FileOffset < Floppy.SectorSizeBytes; FileOffset += 16) {
                    Array.ConstrainedCopy(this.data, TrackOffset + Sector * Floppy.SectorSizeBytes + FileOffset, DirectoryEntry, 0, 16);
                    if (DirectoryEntry[0x8] == 0x2E) { // '.'
                        Result.Add(new DirectoryEntry(this, DirectoryEntry));
                    }    
                }
            }

            return Result.ToArray();
        }


    }
}
