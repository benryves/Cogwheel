
namespace BeeDevelopment.Sega8Bit.Utility {
	public static class FrameBlender {

		/// <summary>
		/// Defines the mode used to blend two frames together.
		/// </summary>
		public enum BlendMode {
			/// <summary>The resulting image is made by blending the two images together equally.</summary>
			Half,
			/// <summary>The resulting image alternates pixels between the primary and secondary.</summary>
			HalfDithered,
			/// <summary>A greyscale 3D anaglyph where the primary image is for the left eye and the secondary image is for the right eye.</summary>
			Anaglyph,
			/// <summary>A coloured 3D anaglyph where the primary image is for the left eye and the secondary image is for the right eye.</summary>
			AnaglyphColour,
		}

		/// <summary>
		/// Blend two frames together.
		/// </summary>
		/// <param name="mode">The <see cref="BlendMode"/> used to blend the two frames together.</param>
		/// <param name="primaryData">The primary (main) frame data as an array of 32-bit XRGB values.</param>
		/// <param name="primaryWidth">The width of the primary frame in pixels.</param>
		/// <param name="primaryHeight">The height of the primary frame in pixels.</param>
		/// <param name="secondaryData">The secondary frame data as an array of 32-bit XRGB values</param>
		/// <param name="secondaryWidth">The width of the secondary frame in pixels.</param>
		/// <param name="secondaryHeight">The height of the seconary frame in pixels.</param>
		/// <returns>An array of 32-bit XRGB pixel values that represent the blended frame.</returns>
		/// <remarks>The resulting pixel data will match the dimensions of the primary frame.</remarks>
		public static int[] Blend(BlendMode mode, int[] primaryData, int primaryWidth, int primaryHeight, int[] secondaryData, int secondaryWidth, int secondaryHeight) {

			// Can we blend?
			if (primaryData == null || secondaryData == null || primaryWidth != secondaryWidth || primaryHeight != secondaryHeight || primaryData.Length != secondaryData.Length || primaryData.Length != primaryWidth * primaryHeight) {
				// Nope, there was a problem.
				return primaryData;
			}

			var Result = new int[primaryData.Length];

			switch (mode) {
				case BlendMode.Half:
					for (int i = 0; i < Result.Length; ++i) Result[i] = Hardware.VideoDisplayProcessor.BlendHalfRgb(primaryData[i], secondaryData[i]);
					break;
				case BlendMode.HalfDithered:
					for (int y = 0, p = 0; y < primaryHeight; ++y) {
						for (int x = 0; x < primaryWidth; ++x, ++p) {
							Result[p] = ((((x + y) & 1) == 0) ? primaryData : secondaryData)[p];
						}
					}
					break;
				case BlendMode.Anaglyph:
					for (int i = 0; i < Result.Length; ++i) {
						int LumaLeft = GetLumaFromRgb(primaryData[i]), LumaRight = GetLumaFromRgb(secondaryData[i]);
						Result[i] = (LumaLeft << 16) | (LumaRight << 8) | (LumaRight);
					}
					break;
				case BlendMode.AnaglyphColour:
					for (int i = 0; i < Result.Length; ++i) {
						Result[i] = (primaryData[i] & 0xFF0000) | (secondaryData[i] & 0x00FF00) | (secondaryData[i] & 0x0000FF);
					}
					break;
			}
			

			return Result;

		}

		private static int GetLumaFromRgb(int rgb) {
			return (54 * ((rgb >> 16) & 0xFF) + 183 * ((rgb >> 8) & 0xFF) + 19 * (rgb & 0xFF)) >> 8;

		}

	}
}
