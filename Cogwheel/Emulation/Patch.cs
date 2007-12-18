using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace BeeDevelopment.Sega8Bit.Emulation {
    public partial class Emulator {

        /// <summary>
        /// Patch a stream using an IPS patch file.
        /// </summary>
        /// <param name="streamToPatch">The stream you wish to patch.</param>
        /// <param name="patchFile">The IPS patch file.</param>
        /// <returns>A new stream containing the patched file.</returns>
        public void Patch(Stream streamToPatch, Stream patchFile) {

            // Check the 5-byte PATCH header.
            byte[] Header = new byte[5];
            if (patchFile.Read(Header, 0, 5) != 5) throw new Exception("Invalid patch file.");

            for (int i = 0; i < 5; ++i) {
                if (Header[i] != (byte)"PATCH"[i]) throw new Exception("Invalid patch file.");
            }

            for (; ; ) {

                uint Offset = 0;
                ushort Size = 0;

                // Get the record offset.
                if (!TryPatchRead24(patchFile, out Offset)) break;
                // EOF record?
                if (Offset == ('E' * 0x10000 + 'O' * 0x100 + 'F')) break;

                // Get the size.
                if (!TryPatchRead16(patchFile, out Size)) break;

                // Seek to the offset.
                streamToPatch.Seek(Offset, SeekOrigin.Begin);

                if (Size != 0) {
                    // Apply a standard patch.
                    byte[] PatchData = new byte[Size];
                    if (patchFile.Read(PatchData, 0, Size) != Size) throw new Exception("Incomplete patch data.");
                    
                    streamToPatch.Write(PatchData, 0, Size);
                } else {
                    // Apply an RLE patch.
                    if (!TryPatchRead16(patchFile, out Size)) throw new Exception("Missing RLE record size.");
                    int RleValue = patchFile.ReadByte();
                    if (RleValue == -1) throw new Exception("Missing RLE record value.");

                    byte[] RleExpandedData = new byte[Size];                    
                    for (int i = 0; i < Size; ++i) RleExpandedData[i] = (byte)RleValue;

                    streamToPatch.Write(RleExpandedData, 0, Size);
                }

            }

        }

        /// <summary>Read an unsigned 16-bit value from the patch file.</summary>
        private bool TryPatchRead16(Stream patchFile, out ushort value) {
            value = 0;
            int Upper = patchFile.ReadByte();
            if (Upper == -1) return false;
            int Lower = patchFile.ReadByte();
            if (Lower == -1) return false;
            value = (ushort)(Upper * 0x100 + Lower);
            return true;
        }

        /// <summary>Read an unsigned 24-bit value from the patch file.</summary>
        private bool TryPatchRead24(Stream patchFile, out uint value) {
            value = 0;
            int Upper = patchFile.ReadByte();
            if (Upper == -1) return false;
            int Middle = patchFile.ReadByte();
            if (Middle == -1) return false;
            int Lower = patchFile.ReadByte();
            if (Lower == -1) return false;
            value = (uint)(Upper * 0x10000 + Middle * 0x100 + Lower);
            return true;

        }
        
    }
}
