// This code has been adapted from ionic.utils.zip.CRC32 (another freeware zip handler).
namespace BeeDevelopment.Zip {

	/// <summary>
	/// Provides methods for CRC-32 checksum calculation.
	/// </summary>
	public static class Crc32 {

		// Lookup table for speed.
		static uint[] Crc32Table;

		static Crc32() {
			unchecked {
				Crc32Table = new uint[256];
				uint Crc;
				for (uint i = 0; i < 256; ++i) {
					Crc = i;
					for (int j = 8; j > 0; --j) {
						if ((Crc & 1) == 1) {
							Crc = (uint)((Crc >> 1) ^ 0xEDB88320);
						} else {
							Crc >>= 1;
						}
					}
					Crc32Table[i] = Crc;
				}
			}
		}

		/// <summary>
		/// Calculates a CRC-32 checksum from a stream.
		/// </summary>
		/// <param name="data">The data to calculate the checksum over.</param>
		/// <returns>The CRC-32 checksum.</returns>
		public static int Calculate(byte[] data) {
			unchecked {
				uint Result;
				Result = (uint)~0;
				foreach (var b in data) Result = (uint)(((Result) >> 8) ^ Crc32Table[b ^ ((Result) & 0xFF)]);				
				return (int)~Result;
			}
		}
	}
}