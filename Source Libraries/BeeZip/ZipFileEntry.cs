#region Using Directives
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
#endregion

namespace BeeDevelopment.Zip {

	/// <summary>
	/// Represents a file stored inside a <see cref="ZipFile"/>.
	/// </summary>
	public class ZipFileEntry {

		#region Types

		/// <summary>
		/// Defines the method used to compress the data.
		/// </summary>
		/// <remarks>Only Store and Deflate methods are support at present.</remarks>
		public enum CompressionMethod : short {
			/// <summary>The file is stored (no compression).</summary>
			Store = 0,
			/// <summary>The file is Shrunk.</summary>
			Shrink = 1,
			/// <summary>The file is Reduced with compression factor 1.</summary>
			ReduceFactor1 = 2,
			/// <summary>The file is Reduced with compression factor 2.</summary>
			ReduceFactor2 = 3,
			/// <summary>The file is Reduced with compression factor 3.</summary>
			ReduceFactor3 = 4,
			/// <summary>The file is Reduced with compression factor 4.</summary>
			ReduceFactor4 = 5,
			/// <summary>The file is Imploded.</summary>
			Implode = 6,
			/// <summary>The file is Deflated.</summary>
			Deflate = 8,
			/// <summary>Enhanced Deflating using Deflate64™.</summary>
			Deflate64 = 9,
			/// <summary>PKWARE Data Compression Library Imploding (old IBM TERSE).</summary>
			IbmTerseOld = 10,
			/// <summary>File is compressed using BZIP2 algorithm.</summary>
			BZip2 = 12,
			/// <summary>LZMA (EFS).</summary>
			Lzma = 14,
			/// <summary>File is compressed using IBM TERSE (new).</summary>
			IbmTerseNew = 18,
			/// <summary>IBM LZ77 z Architecture (PFS).</summary>
			IbmLZ77 = 19,
			/// <summary>WavPack compressed data.</summary>
			WavPack = 97,
			/// <summary>PPMd version I, Rev 1.</summary>
			PPMd = 98,
		}

		/// <summary>
		/// Defines general-purpose flags for the compressed data.
		/// </summary>
		[Flags()]
		public enum AttributeBits : short {
			/// <summary>No special attributes are set.</summary>
			None = 0,
			/// <summary>If set, indicates that the file is encrypted.</summary>
			Encrypted = 1 << 0,
			/// <summary>If set, the usual CRC-32, compressed size and uncompressed size fields are set to zero and their real values are appended after the data section.</summary>
			SizeInformationAfterData = 1 << 3,
			/// <summary>Reserved for use with method 8 (deflate), for enhanced deflating.</summary>
			EnhancedDeflate = 1 << 4,
			/// <summary>If set, this indicates that the file is compressed patched data.</summary>
			CompressedPatchedData = 1 << 5,
			/// <summary>Strong encryption.</summary>
			/// <remarks>
			/// If set, you should set the version needed to extract value to at least 50 and you must also set bit 0.
			/// If AES encryption is used, the version needed to extract value must be at least 51.
			/// </remarks>
			StrongEncrypytion = 1 << 6,
			/// <summary>Language encoding flag (EFS).</summary>
			/// <remarks>If this bit is set, the filename and comment fields for this file must be encoded using UTF-8.</remarks>
			UnicodeNames = 1 << 11,
			/// <summary>Selected data values in the Local Header are masked to hide their actual values.</summary>
			EncryptedHeaders = 1 << 13,
		}

		#endregion

		#region Public Properties
		
		/// <summary>
		/// Gets or sets the name of the file.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="CompressionMethod"/> used to compress the file.
		/// </summary>
		public CompressionMethod Method { get; set; }

		/// <summary>
		/// Gets or sets the time when the <see cref="ZipFileEntry"/> was last written to.
		/// </summary>
		public DateTime LastWriteTime { get; set; }

		/// <summary>
		/// Gets or sets a comment attached to the file.
		/// </summary>
		public string Comment { get; set; }

		/// <summary>
		/// Gets or sets the data that the <see cref="ZipFileEntry"/> represents.
		/// </summary>
		public byte[] Data { get; set; }
		
		#endregion

		#region Internal Properties

		/// <summary>
		/// Gets the compressed size of the data <see cref="ZipFileEntry"/>.
		/// </summary>
		internal int CompressedSize { get; private set; }

		/// <summary>
		/// Gets the CRC-32 checksum of the <see cref="ZipFileEntry"/>.
		/// </summary>
		internal int Crc { get; private set; }

		/// <summary>
		/// Gets the internal general-purpose attribute bits.
		/// </summary>
		internal AttributeBits Attributes { get; private set; }

		#endregion

		#region Internal Methods

		/// <summary>
		/// Saves the <see cref="ZipFileEntry"/> to a <see cref="Stream"/>.
		/// </summary>
		/// <param name="stream">The <see cref="Stream"/> to write the <see cref="ZipFileEntry"/> to.</param>
		internal void Save(Stream stream) {

			var EntryWriter = new BinaryWriter(stream);

			// Encode the filename and comment.
			byte[] EncodedName, EncodedComment;
			var IsUnicode = this.EncodeStrings(out EncodedName, out EncodedComment);

			// Write the basic header.

			EntryWriter.Write((int)0x04034B50);        // Local file header signature.
			EntryWriter.Write((short)20);              // Version to extract = 2.0.
			EntryWriter.Write((short)this.Attributes); // General purpose.
			EntryWriter.Write((short)this.Method);
			EntryWriter.Write((int)DosDateTime.FromDateTime(this.LastWriteTime).ToBinary());

			// Compress the file.

			var DataToCompress = this.Data ?? new byte[0];

			using (var CompressedStream = new MemoryStream(DataToCompress.Length / 4)) {
				Stream CompressingStream;

				switch (this.Method) {
					case CompressionMethod.Store:
						CompressingStream = CompressedStream;
						break;
					case CompressionMethod.Deflate:
						CompressingStream = new DeflateStream(CompressedStream, CompressionMode.Compress, true);
						break;
					default:
						throw new NotSupportedException();
				}

				CompressingStream.Write(DataToCompress, 0, DataToCompress.Length);

				if (CompressingStream != CompressedStream) {
					CompressingStream.Close();
					CompressingStream.Dispose();
				}

				this.CompressedSize = (int)CompressedStream.Length;

				// Calculate the CRC-32 checksum of the data.
				this.Crc = Crc32.Calculate(this.Data);

				EntryWriter.Write((int)this.Crc);
				EntryWriter.Write((int)this.CompressedSize);
				EntryWriter.Write((int)Data.Length);


				EntryWriter.Write((short)EncodedName.Length);
				EntryWriter.Write((short)EncodedComment.Length);

				EntryWriter.Write(EncodedName);
				EntryWriter.Write(EncodedComment);

				EntryWriter.Write(CompressedStream.ToArray());
			}
		}

		/// <summary>
		/// Encode the filename and comment fields associated with this entry.
		/// </summary>
		/// <param name="filename">Outputs the encoded filename.</param>
		/// <param name="comment">Outputs the encoded comment.</param>
		/// <returns>True if the strings are encoded using UTF-8, false if using legacy codepage 437.</returns>
		internal bool EncodeStrings(out byte[] filename, out byte[] comment) {
			
			var LegacyEncoding = Encoding.GetEncoding(437);
		
			var RequiresUnicode =
				(LegacyEncoding.GetString(LegacyEncoding.GetBytes(this.Name ?? "")) != (this.Name ?? "")) ||
				(LegacyEncoding.GetString(LegacyEncoding.GetBytes(this.Comment ?? "")) != (this.Comment ?? ""));

			if (RequiresUnicode) {
				filename = Encoding.UTF8.GetBytes(this.Name ?? "");
				comment = Encoding.UTF8.GetBytes(this.Comment ?? "");
				this.Attributes |= AttributeBits.UnicodeNames;
			} else {
				filename = LegacyEncoding.GetBytes(this.Name ?? "");
				comment = LegacyEncoding.GetBytes(this.Comment ?? "");
				this.Attributes &= ~AttributeBits.UnicodeNames;
			}

			return RequiresUnicode;
		}

		#endregion

		#region Public Constructors

		/// <summary>
		/// Creates an empty <see cref="ZipFileEntry"/> instance.
		/// </summary>
		public ZipFileEntry() {
			this.LastWriteTime = DateTime.Now;
			this.Method = CompressionMethod.Deflate;
		}

		/// <summary>
		/// Creates a <see cref="ZipFileEntry"/> instance from an existing file.
		/// </summary>
		/// <param name="filename">The name of the file to load.</param>
		/// <returns>A <see cref="ZipFileEntry"/> created from <paramref name="filename"/>.</returns>
		public static ZipFileEntry FromFile(string filename) {
			var SourceInfo = new FileInfo(filename);
			if (!SourceInfo.Exists) throw new FileNotFoundException();

			return new ZipFileEntry() {
				Name = filename,
				LastWriteTime = SourceInfo.LastWriteTime,
				Data = File.ReadAllBytes(filename),
			};

		}

		#endregion

		#region Internal Constructors

		/// <summary>
		/// Creates a <see cref="ZipFileEntry"/> from a stream containing a zip file.
		/// </summary>
		/// <param name="stream">The stream to create the <see cref="ZipFileEntry"/> from.</param>
		/// <param name="isLocalFileHeader">True if this is a local file header, false if it is from the central directory.</param>
		static internal ZipFileEntry FromZipFileStream(Stream stream, bool isLocalFileHeader) {
			var Result = new ZipFileEntry();

			var EntryReader = new BinaryReader(stream);

			if (!isLocalFileHeader) EntryReader.ReadInt16(); // Version made by.
			EntryReader.ReadInt16(); // Version needed to extract.

			Result.Attributes = (AttributeBits)EntryReader.ReadInt16();
			Result.Method = (CompressionMethod)EntryReader.ReadInt16();
			Result.LastWriteTime = DosDateTime.FromBinary(EntryReader.ReadInt32()).ToDateTime();
			Result.Crc = EntryReader.ReadInt32();
			Result.CompressedSize = EntryReader.ReadInt32();

			uint DataSize = EntryReader.ReadUInt32();

			var EncodedNameLength = EntryReader.ReadUInt16();
			var ExtraFieldLength = EntryReader.ReadUInt16();
			ushort CommentLength = 0;

			uint LocalHeaderLocation = 0;

			if (!isLocalFileHeader) {
				CommentLength = EntryReader.ReadUInt16();
				stream.Seek(8, SeekOrigin.Current); // Disk number + Internal/External file attributes.
				LocalHeaderLocation = EntryReader.ReadUInt32();
			}


			var StringEncoder = ((Result.Attributes & AttributeBits.UnicodeNames) == AttributeBits.None) ? Encoding.GetEncoding(437) : Encoding.UTF8;

			Result.Name = StringEncoder.GetString(EntryReader.ReadBytes(EncodedNameLength));

			stream.Seek(ExtraFieldLength, SeekOrigin.Current);

			Result.Comment = StringEncoder.GetString(EntryReader.ReadBytes(CommentLength));

			if (!isLocalFileHeader) {
				var OldPosition = stream.Position;
				stream.Seek(LocalHeaderLocation + 4, SeekOrigin.Begin);

				var LocalEntry = ZipFileEntry.FromZipFileStream(stream, true);

				var DecompressingStream = stream;

				switch (Result.Method) {
					case CompressionMethod.Store:
						break;
					case CompressionMethod.Deflate:
						DecompressingStream = new DeflateStream(stream, CompressionMode.Decompress, true);
						break;
					default:
						DecompressingStream = null;
						break;
				}

				if (DecompressingStream != null) {
					var Decompressed = new byte[DataSize];
					DecompressingStream.Read(Decompressed, 0, Decompressed.Length);
					Result.Data = Decompressed;
					if (DecompressingStream != stream) {
						DecompressingStream.Close();
						DecompressingStream.Dispose();
					}
				}

				stream.Seek(OldPosition, SeekOrigin.Begin);
			}

			return Result;

		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Returns a string representation of the <see cref="ZipFileEntry"/>.
		/// </summary>
		/// <returns>A string representation of the <see cref="ZipFileEntry"/>.</returns>
		public override string ToString() {
			return this.Name;
		}
		
		#endregion

	}
}