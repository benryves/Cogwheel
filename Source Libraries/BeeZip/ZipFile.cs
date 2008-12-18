#region Using Directives
using System;
using System.Collections.Generic;
using System.IO;
#endregion

namespace BeeDevelopment.Zip {

	/// <summary>
	/// Provides functionality for reading and writing zip archive files.
	/// </summary>
	public class ZipFile : ICollection<ZipFileEntry> {

		#region Private Types

		/// <summary>
		/// Defines identifiers for the records in a zip archive.
		/// </summary>
		private enum RecordIdentifier {
			LocalFile = 0x04034B50,
			ExtraData = 0x08064B50,
			CentralDirectory = 0x02014B50,
			DigitalSignature = 0x05054B50,
			Zip64EndOfCentralDirectory = 0x06064B50,
			Zip64EndOfCentralDirectoryLocator = 0x07064B50,
			EndOfCentralDirectoryRecord = 0x06054B50,
		}

		#endregion

		#region Private Members

		/// <summary>
		/// The internal list used to store the files.
		/// </summary>
		private List<ZipFileEntry> Files;

		#endregion

		#region Public Methods

		/// <summary>
		/// Saves the <see cref="ZipFile"/> to a <see cref="Stream"/>.
		/// </summary>
		/// <param name="stream">The <see cref="Stream"/> to save the <see cref="ZipFile"/> to.</param>
		public void Save(Stream stream) {

			var HeaderOffsets = new Queue<long>();
			foreach (var File in this) {
				HeaderOffsets.Enqueue(stream.Position);
				File.Save(stream);
			}

			var StartOfDirectory = stream.Position;

			var DirectoryWriter = new BinaryWriter(stream);

			foreach (var File in this) {
				DirectoryWriter.Write((int)0x02014B50);            // Central file header signature.
				DirectoryWriter.Write((short)20);                  // Version made by (2.0).
				DirectoryWriter.Write((short)20);                  // Version needed to extract.
				DirectoryWriter.Write((short)File.Attributes);     // General purpose bit flag.
				DirectoryWriter.Write((short)File.Method);         // Compression method.
				DirectoryWriter.Write((int)DosDateTime.FromDateTime(File.LastWriteTime).ToBinary());
				DirectoryWriter.Write((int)File.Crc);              // CRC-32 checksum.
				DirectoryWriter.Write((int)File.CompressedSize);   // Compressed size.
				DirectoryWriter.Write((int)(File.Data == null ? 0 : File.Data.Length)); // Uncompressed size.

				byte[] EncodedName, EncodedComment;
				File.EncodeStrings(out EncodedName, out EncodedComment);

				DirectoryWriter.Write((short)EncodedName.Length);    // Filename length.
				DirectoryWriter.Write((short)0);                     // Extra field length.
				DirectoryWriter.Write((short)EncodedComment.Length); // Comment length.
				DirectoryWriter.Write((short)0);                     // Disk number start.
				DirectoryWriter.Write((short)0);                     // Internal file attributes.
				DirectoryWriter.Write((int)0);                       // External file attributes
				DirectoryWriter.Write((int)HeaderOffsets.Dequeue()); // Relative offset of local header.
				DirectoryWriter.Write(EncodedName);                  // Filename.
				DirectoryWriter.Write(EncodedComment);               // Comments.
			}

			var DirectorySize = stream.Position - StartOfDirectory;

			// End of central directory record.
			DirectoryWriter.Write((int)0x06054B50);
			DirectoryWriter.Write((short)0);              // Number of this disk.
			DirectoryWriter.Write((short)0);              // Number of the disk with the start of the central directory.
			DirectoryWriter.Write((short)this.Count);     // Number of files in central directory on this disk.
			DirectoryWriter.Write((short)this.Count);     // Number of files in central directory.
			DirectoryWriter.Write((int)DirectorySize);    // Size of central directory.
			DirectoryWriter.Write((int)StartOfDirectory); // Offset of central directory.
			DirectoryWriter.Write((short)0);              // Length of comment.
		

		}

		/// <summary>
		/// Saves the <see cref="ZipFile"/> to a file.
		/// </summary>
		/// <param name="filename">The name of the file to save the <see cref="ZipFile"/> to.</param>
		public void Save(string filename) {
			using (var File = new FileStream(filename, FileMode.Create, FileAccess.Write)) {
				this.Save(File);
			}
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets a <see cref="ZipFileEntry"/> by name.
		/// </summary>
		/// <param name="name">The name of the <see cref="ZipFileEntry"/> to retrieve.</param>
		/// <returns>The corresponding <see cref="ZipFileEntry"/> or <c>null</c> if a matching one wasn't found.</returns>
		public ZipFileEntry this[string name] {
			get {
				string NormalisedName = name.Replace('\\', '/').ToLowerInvariant();
				foreach (var File in this.Files) {
					if (File.Name.Replace('\\', '/').ToLowerInvariant() == NormalisedName) return File;
				}
				return null;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Creates an empty instance of <see cref="ZipFile"/>.
		/// </summary>
		public ZipFile() {
			this.Files = new List<ZipFileEntry>();
		}

		/// <summary>
		/// Creates an instance of a <see cref="ZipFile"/> from an existing zip file stored in a <see cref="Stream"/>.
		/// </summary>
		/// <param name="stream">The <see cref="Stream"/> to load the <see cref="ZipFile"/> from.</param>
		/// <returns>The <see cref="ZipFile"/> loaded from <paramref name="stream"/>.</returns>
		public static ZipFile FromStream(Stream stream) {

			var Result = new ZipFile();

			var ZipFileReader = new BinaryReader(stream);

			// True whilst there are still files to read (false when we've hit the directory).
			var MoreLocalFilesAvailable = true;

			// Have we hit the end of central directory record?
			var EncounteredEndOfCentralDirectory = false;


			while (!EncounteredEndOfCentralDirectory) {

				var Signature = (RecordIdentifier)ZipFileReader.ReadInt32();

				switch (Signature) {
					case RecordIdentifier.ExtraData:
						MoreLocalFilesAvailable = false;
						break;
					case RecordIdentifier.CentralDirectory:
						MoreLocalFilesAvailable = false;
						Result.Files.Add(ZipFileEntry.FromZipFileStream(stream, false));
						break;
					case RecordIdentifier.EndOfCentralDirectoryRecord:
						EncounteredEndOfCentralDirectory = true;
						break;
					default:
						if (MoreLocalFilesAvailable) {
							// Skip the local file header...
							var LocalHeader = ZipFileEntry.FromZipFileStream(stream, true);
							stream.Seek(LocalHeader.CompressedSize, SeekOrigin.Current);
						}
						break;
				}
			}

			return Result;
		}

		/// <summary>
		/// Creates an instance of a <see cref="ZipFile"/> from an existing zip file.
		/// </summary>
		/// <param name="filename">The name of the file to load the <see cref="ZipFile"/> from.</param>
		/// <returns>The <see cref="ZipFile"/> loaded from <paramref name="filename"/>.</returns>
		public static ZipFile FromFile(string filename) {
			using (var ZipStream = new FileStream(filename, FileMode.Open, FileAccess.Read)) {
				return ZipFile.FromStream(ZipStream);
			}
		}

		#endregion

		#region ICollection<ZipFileEntry> Members

		/// <summary>
		/// Adds a <see cref="ZipFileEntry"/> to the <see cref="ZipFile"/>.
		/// </summary>
		/// <param name="item">The <see cref="ZipFileEntry"/> to add.</param>
		public void Add(ZipFileEntry item) {
			this.Files.Add(item);
		}

		/// <summary>
		/// Removes all <see cref="ZipFileEntry"/> elements from the <see cref="ZipFile"/>.
		/// </summary>
		public void Clear() {
			this.Files.Clear();
		}

		/// <summary>
		/// Determines whether a <see cref="ZipFileEntry"/> is in the <see cref="ZipFile"/>.
		/// </summary>
		/// <param name="item">The <see cref="ZipFileEntry"/> to locate in the <see cref="ZipFile"/>.</param>
		/// <returns>True if the <see cref="ZipFileEntry"/> is found in the <see cref="ZipFile"/>; otherwise, false.</returns>
		public bool Contains(ZipFileEntry item) {
			return this.Files.Contains(item);
		}

		/// <summary>
		/// Copies all <see cref="ZipFileEntry"/> elements from the <see cref="ZipFile"/> to an array, starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The one-dimensional array that is the target of the elements copied from the <see cref="ZipFile"/>.</param>
		/// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
		public void CopyTo(ZipFileEntry[] array, int arrayIndex) {
			this.Files.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Gets the number of <see cref="ZipFileEntry"/> elements contained in the <see cref="ZipFile"/>.
		/// </summary>
		public int Count {
			get { return this.Files.Count; }
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="ZipFile"/> is read-only.
		/// </summary>
		/// <remarks>This always return false.</remarks>
		public bool IsReadOnly {
			get { return false; }
		}

		/// <summary>
		/// Removes the first occurance of a specific <see cref="ZipFileEntry"/> from the <see cref="ZipFile"/>.
		/// </summary>
		/// <param name="item">The <see cref="ZipFileEntry"/> to remove from the <see cref="ZipFile"/>.</param>
		/// <returns>True if item is successfully removed; otherwise, false. This method also returns false if item was not found in the <see cref="ZipFile"/>.</returns>
		public bool Remove(ZipFileEntry item) {
			return this.Files.Remove(item);
		}

		#endregion

		#region IEnumerable<ZipFileEntry> Members

		/// <summary>
		/// Returns an enumerator that iterates through the <see cref="ZipFile"/>.
		/// </summary>
		/// <returns>An enumerator that iterates through the <see cref="ZipFile"/>.</returns>
		public IEnumerator<ZipFileEntry> GetEnumerator() {
			return this.Files.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		/// <summary>
		/// Returns an enumerator that iterates through the <see cref="ZipFile"/>.
		/// </summary>
		/// <returns>An enumerator that iterates through the <see cref="ZipFile"/>.</returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return this.Files.GetEnumerator();
		}

		#endregion
	}
}
