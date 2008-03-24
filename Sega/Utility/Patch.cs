using System;
using System.IO;
namespace BeeDevelopment.Sega8Bit.Utility {
	public class Patch {

		/// <summary>
		/// Patch a stream using an IPS patch file.
		/// </summary>
		/// <param name="toPatch">The stream you wish to patch.</param>
		/// <param name="patch">The IPS patch file.</param>
		public static void ApplyPatch(Stream toPatch, Stream patch) {

			// Check the 5-byte PATCH header.
			byte[] Header = new byte[5];
			if (patch.Read(Header, 0, 5) != 5) throw new InvalidDataException("Invalid patch file.");

			for (int i = 0; i < 5; ++i) {
				if (Header[i] != (byte)"PATCH"[i]) throw new InvalidDataException("Invalid patch file (missing PATCH header).");
			}

			for (; ; ) {

				uint Offset = 0;
				ushort Size = 0;

				// Get the record offset.
				if (!TryPatchRead24(patch, out Offset)) break;
				// EOF record?
				if (Offset == ('E' * 0x10000 + 'O' * 0x100 + 'F')) break;

				// Get the size.
				if (!TryPatchRead16(patch, out Size)) break;

				// Seek to the offset.
				toPatch.Seek(Offset, SeekOrigin.Begin);

				if (Size != 0) {
					// Apply a standard patch.
					byte[] PatchData = new byte[Size];
					if (patch.Read(PatchData, 0, Size) != Size) throw new InvalidDataException("Incomplete patch data.");

					toPatch.Write(PatchData, 0, Size);
				} else {
					// Apply an RLE patch.
					if (!TryPatchRead16(patch, out Size)) throw new InvalidDataException("Missing RLE record size.");
					int RleValue = patch.ReadByte();
					if (RleValue == -1) throw new InvalidDataException("Missing RLE record value.");

					byte[] RleExpandedData = new byte[Size];
					for (int i = 0; i < Size; ++i) RleExpandedData[i] = (byte)RleValue;

					toPatch.Write(RleExpandedData, 0, Size);
				}
			}
		}

		/// <summary>Read an unsigned 16-bit value from the patch file.</summary>
		private static bool TryPatchRead16(Stream patchFile, out ushort value) {
			value = 0;
			int Upper = patchFile.ReadByte();
			if (Upper == -1) return false;
			int Lower = patchFile.ReadByte();
			if (Lower == -1) return false;
			value = (ushort)(Upper * 0x100 + Lower);
			return true;
		}

		/// <summary>Read an unsigned 24-bit value from the patch file.</summary>
		private static bool TryPatchRead24(Stream patchFile, out uint value) {
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
