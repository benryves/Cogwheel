using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace BeeDevelopment.Sega8Bit.Hardware.Controllers.VDrive {

	public enum NumericalMode {
		Binary,
		Ascii,
	}

	public enum CommandSet {
		Extended,
		Shortened,
	}

	public enum ErrorMessage {
		BadCommand,
		CommandFailed,
		DiskFull,
		Invalid,
		ReadOnly,
		FileOpen,
		DirNotEmpty,
		FilenameInvalid,
		NoUpgrade,
		NoDisk,
	}

	enum IncomingDataPurpose {
		None,
		WriteFile,
	}

	public class VDrive {

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern int GetShortPathName(String pathName, StringBuilder shortName, int cbShortName);

		public string DiskPath { get; set; }
		public string CurrentDirectory { get; set; }

		public NumericalMode NumericalMode { get; private set; }

		public CommandSet CommandSet { get; private set; }

		
		private Emulator emulator;
		List<byte> incomingData;
		uint incomingDataPending = 0;
		bool incomingDataIsNumber = false;

		IncomingDataPurpose incomingDataPurpose = IncomingDataPurpose.None;

		private FileStream openFile = null;
		private string openFileName = null;

		public VDrive(Emulator emulator) {
			this.emulator = emulator;
			this.NumericalMode = NumericalMode.Binary;
			this.CommandSet = CommandSet.Extended;

			this.incomingData = new List<byte>();
			this.emulator.SerialPort.DataRecieved += SerialPort_DataRecieved;
		}

		public bool HasDisk {
			get {
				return !string.IsNullOrEmpty(this.DiskPath) && Directory.Exists(this.DiskPath);
			}
		}

		static string NormaliseFilename(string path) {
			var name = Path.GetFileName(path);
			var shortName = new StringBuilder(260);
			if (GetShortPathName(path, shortName, shortName.Capacity) != 0) {
				name = Path.GetFileName(shortName.ToString());
			}
			return name.ToUpperInvariant();
		}

		static bool IsValidFilename(string filename) {
			if (string.IsNullOrEmpty(filename)) return false;
			foreach (var item in Path.GetInvalidFileNameChars()) {
				if (filename.Contains(item.ToString())) {
					return false;
				}
			}
			return true;
		}

		string GetFullPath(string filename) {
			if (!string.IsNullOrEmpty(this.CurrentDirectory)) filename = Path.Combine(this.CurrentDirectory, filename);
			if (!string.IsNullOrEmpty(this.DiskPath)) filename = Path.Combine(this.DiskPath, filename);
			return filename;
		}

		uint GetNumber(int offset, int size) {
			switch (this.NumericalMode) {
				case NumericalMode.Binary:
					uint result = 0;
					for (int i = 0; i < size; i++) {
						result <<= 8;
						result |= this.incomingData[offset + i];
					}
					return result;
				case NumericalMode.Ascii:
					try {
						var numericString = Encoding.ASCII.GetString(this.incomingData.ToArray(), offset, this.incomingData.Count - offset).TrimEnd();
						if (numericString.StartsWith("$")) {
							return Convert.ToUInt32(numericString.Substring(1), 16);
						} else if (numericString.StartsWith("0x")) {
							return Convert.ToUInt32(numericString.Substring(2), 16);
						} else {
							return Convert.ToUInt32(numericString, 10);
						}
					} catch {
						return 0;
					}
			}
			return 0;
		}

		private void SerialPort_DataRecieved(object sender, SerialDataReceivedEventArgs e) {
			if (incomingDataIsNumber) {
				switch (this.NumericalMode) {
					case NumericalMode.Binary:
						this.incomingData.Add(e.Data);
						if (incomingDataPending > 0) {
							--incomingDataPending;
						}
						if (incomingDataPending == 0) {
							incomingDataIsNumber = false;
						}
						return;
					case NumericalMode.Ascii:
						if ("$0123456789ABCDEFx".Contains(((char)e.Data).ToString())) {
							this.incomingData.Add(e.Data);
							return;
						} else {
							incomingDataIsNumber = false;
							incomingDataPending = 0;
						}
						break;
				}
			}
			if (incomingDataPending > 0) {
				this.incomingData.Add(e.Data);
				if (--incomingDataPending == 0) {
					switch (incomingDataPurpose) {
						case IncomingDataPurpose.WriteFile:
							if (!this.HasDisk) {
								this.Write(ErrorMessage.NoDisk);
							} else if (this.openFile == null) {
								this.Write(ErrorMessage.Invalid);
							} else {
								try {
									this.openFile.Write(incomingData.ToArray(), 0, incomingData.Count);
									this.WritePrompt();
								} catch {
									this.Write(ErrorMessage.DiskFull);
								}
							}
							
							break;
						default:
							this.Write(ErrorMessage.BadCommand);
							break;
					}
					incomingData.Clear();
					incomingDataPurpose = IncomingDataPurpose.None;
				}
			} else if (e.Data == 0x0D) {
				if (this.incomingData.Count == 0) {
					if (this.HasDisk) {
						this.WritePrompt();
					} else {
						this.Write(ErrorMessage.NoDisk);
					}
				} else {
					var commandString = Encoding.ASCII.GetString(this.incomingData.ToArray());
					if (commandString == "SCS" || (incomingData.Count == 1 && incomingData[0] == 0x10)) {
						this.CommandSet = CommandSet.Shortened;
						this.WritePrompt();
					} else if (commandString == "ECS" || (incomingData.Count == 1 && incomingData[0] == 0x11)) {
						this.CommandSet = CommandSet.Extended;
						this.WritePrompt();
					} else if ((commandString == "IPA" && CommandSet == CommandSet.Extended) || (incomingData.Count == 1 && incomingData[0] == 0x90 && CommandSet == CommandSet.Shortened)) {
						this.NumericalMode = NumericalMode.Ascii;
						this.WritePrompt();
					} else if ((commandString == "IPH" && CommandSet == CommandSet.Extended) || (incomingData.Count == 1 && incomingData[0] == 0x91 && CommandSet == CommandSet.Shortened)) {
						this.NumericalMode = NumericalMode.Binary;
						this.WritePrompt();
					} else if (commandString == "E" || commandString == "e") {
						this.Write(commandString + "\r");
					} else if ((commandString == "DIR" && CommandSet == CommandSet.Extended) || (incomingData.Count == 1 && incomingData[0] == 0x01 && CommandSet == CommandSet.Shortened)) {
						if (!this.HasDisk) {
							this.Write(ErrorMessage.NoDisk);
						} else if (this.openFile != null) {
							this.Write(ErrorMessage.FileOpen);
						} else {
							this.emulator.SerialPort.Write(0x0D);
							foreach (var fsi in Directory.GetFileSystemEntries(GetFullPath(""))) {
								var name = NormaliseFilename(fsi);
								if (Directory.Exists(fsi)) {
									name += " DIR";
								}
								this.Write(name + "\r");
							}
							this.WritePrompt();
						}
					} else if ((commandString.StartsWith("DIR ") && CommandSet == CommandSet.Extended) || (commandString.StartsWith("\x01 ") && CommandSet == CommandSet.Shortened)) {
						if (!this.HasDisk) {
							this.Write(ErrorMessage.NoDisk);
						} else if (this.openFile != null) {
							this.Write(ErrorMessage.FileOpen);
						} else {
							var filename = commandString.Substring(CommandSet == CommandSet.Extended ? 4 : 2).ToUpperInvariant();
							if (!IsValidFilename(filename)) {
								this.Write(ErrorMessage.FilenameInvalid);
							} else {
								var dirPath = GetFullPath(filename);
								FileSystemInfo fsi;
								if (Directory.Exists(dirPath)) {
									fsi = new DirectoryInfo(dirPath);
								} else {
									fsi = new FileInfo(dirPath);
								}
								if (!fsi.Exists) {
									this.Write(ErrorMessage.CommandFailed);
								} else {
									this.Write(NormaliseFilename(fsi.FullName));
									this.Write(' ');
									if (fsi is FileInfo) {
										this.Write((uint)((FileInfo)fsi).Length);
									} else {
										this.Write((uint)0);
									}
									this.Write('\r');
									this.WritePrompt();
								}
							}
						}
					} else if ((commandString.StartsWith("CD ") && CommandSet == CommandSet.Extended) || (commandString.StartsWith("\x02 ") && CommandSet == CommandSet.Shortened)) {
						if (!this.HasDisk) {
							this.Write(ErrorMessage.NoDisk);
						} else if (this.openFile != null) {
							this.Write(ErrorMessage.FileOpen);
						} else {
							var dirname = commandString.Substring(CommandSet == CommandSet.Extended ? 3 : 2).ToUpperInvariant();
							if (!IsValidFilename(dirname)) {
								this.Write(ErrorMessage.FilenameInvalid);
							} else {
								var dirPath = GetFullPath(dirname);
								var fsi = new DirectoryInfo(dirPath);
								if (!fsi.Exists) {
									this.Write(ErrorMessage.CommandFailed);
								} else {
									this.CurrentDirectory = Path.Combine(this.CurrentDirectory ?? "", Path.GetFileName(dirPath));
									this.WritePrompt();
								}
							}
						}
					} else if ((commandString.StartsWith("RD ") && CommandSet == CommandSet.Extended) || (commandString.StartsWith("\x04 ") && CommandSet == CommandSet.Shortened)) {
						if (!this.HasDisk) {
							this.Write(ErrorMessage.NoDisk);
						} else if (this.openFile != null) {
							this.Write(ErrorMessage.FileOpen);
						} else {
							var filename = commandString.Substring(CommandSet == CommandSet.Extended ? 3 : 2).ToUpperInvariant();
							if (!IsValidFilename(filename)) {
								this.Write(ErrorMessage.FilenameInvalid);
							} else {
								var fsi = new FileInfo(GetFullPath(filename));
								if (!fsi.Exists) {
									this.Write(ErrorMessage.CommandFailed);
								} else {
									this.Write(File.ReadAllBytes(fsi.FullName));
									this.WritePrompt();
								}
							}
						}
					} else if (
						((commandString.StartsWith("OPW ") && CommandSet == CommandSet.Extended) || (commandString.StartsWith("\x09 ") && CommandSet == CommandSet.Shortened))
					||
						((commandString.StartsWith("OPR ") && CommandSet == CommandSet.Extended) || (commandString.StartsWith("\x0E ") && CommandSet == CommandSet.Shortened))
					) {
						if (!this.HasDisk) {
							this.Write(ErrorMessage.NoDisk);
						} else if (this.openFile != null) {
							this.Write(ErrorMessage.FileOpen);
						} else {
							var filename = commandString.Substring(CommandSet == CommandSet.Extended ? 4 : 2).ToUpperInvariant().Split(' ')[0];
							if (!IsValidFilename(filename)) {
								this.Write(ErrorMessage.FilenameInvalid);
							} else {
								var fsi = new FileInfo(GetFullPath(filename));
								try {
									if ((commandString.StartsWith("OPW ") && CommandSet == CommandSet.Extended) || (commandString.StartsWith("\x09 ") && CommandSet == CommandSet.Shortened)) {
										openFile = File.Open(fsi.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
										openFile.Seek(0, SeekOrigin.End);
									} else {
										openFile = File.Open(fsi.FullName, FileMode.Open, FileAccess.Read);
									}
								} catch {
									this.Write(ErrorMessage.CommandFailed);
									openFile = null;
									return;
								}
								openFileName = filename;
								this.WritePrompt();
							}
						}
					} else if ((commandString.StartsWith("CLF ") && CommandSet == CommandSet.Extended) || (commandString.StartsWith("\x0A ") && CommandSet == CommandSet.Shortened)) {
						if (!this.HasDisk) {
							this.Write(ErrorMessage.NoDisk);
						} else if (this.openFile == null) {
							this.Write(ErrorMessage.CommandFailed);
						} else {
							this.openFile.Close();
							this.openFile = null;
							var filename = commandString.Substring(CommandSet == CommandSet.Extended ? 4 : 2).ToUpperInvariant();
							if (filename != this.openFileName) {
								this.Write(ErrorMessage.CommandFailed);
							} else {
								this.WritePrompt();
							}
						}
					} else if ((commandString.StartsWith("WRF ") && CommandSet == CommandSet.Extended) || (commandString.StartsWith("\x08 ") && CommandSet == CommandSet.Shortened)) {
						if (!this.HasDisk) {
							this.Write(ErrorMessage.NoDisk);
						} else if (this.openFile == null) {
							this.Write(ErrorMessage.CommandFailed);
						} else {
							this.incomingDataPending = this.GetNumber(CommandSet == CommandSet.Extended ? 4 : 2, 4);
							this.incomingDataPurpose = IncomingDataPurpose.WriteFile;
						}
					} else if ((commandString.StartsWith("SEK ") && CommandSet == CommandSet.Extended) || (commandString.StartsWith("\x28 ") && CommandSet == CommandSet.Shortened)) {
						if (!this.HasDisk) {
							this.Write(ErrorMessage.NoDisk);
						} else if (this.openFile == null) {
							this.Write(ErrorMessage.CommandFailed);
						} else {
							try {
								this.openFile.Position = this.GetNumber(CommandSet == CommandSet.Extended ? 4 : 2, 4);
								this.WritePrompt();
							} catch {
								this.Write(ErrorMessage.CommandFailed);
							}
						}
					} else {
						this.Write(ErrorMessage.BadCommand);
					}
				}
				this.incomingData.Clear();
			} else if (e.Data == 0x20) {
				var commandString = Encoding.ASCII.GetString(this.incomingData.ToArray());
				if (
					((commandString == "WRF" && CommandSet == CommandSet.Extended) || (commandString == "\x08" && CommandSet == CommandSet.Shortened)) ||
					((commandString == "RDF" && CommandSet == CommandSet.Extended) || (commandString == "\x0B" && CommandSet == CommandSet.Shortened)) ||
					((commandString == "SEK" && CommandSet == CommandSet.Extended) || (commandString == "\x28" && CommandSet == CommandSet.Shortened))
				) {
					this.incomingDataIsNumber = true;
					this.incomingDataPending = 4;
				}
				this.incomingData.Add(e.Data);
			} else {
				this.incomingData.Add(e.Data);
			}
		}

		private void Write(byte b) {
			this.emulator.SerialPort.Write(b);
		}
		private void Write(char c) {
			this.Write((byte)c);
		}
		private void Write(IEnumerable<byte> bytes) {
			foreach (var item in bytes) {
				this.Write(item);
			}
		}
		private void Write(string s) {
			this.Write(Encoding.ASCII.GetBytes(s));
		}
		private void Write(uint dword) {
			for (int i = 0; i < 4; ++i) {
				byte b = (byte)dword;
				dword >>= 8;
				switch (this.NumericalMode) {
					case NumericalMode.Binary:
						this.Write(b);
						break;
					case NumericalMode.Ascii:
						this.Write(string.Format("${0:X2} ", b));
						break;
				}
			}
			
		}

		private void Write(ErrorMessage error) {
			var extendedErrorMessage = "Undefined Error";
			var shortenedErrorMessage = "UE";
			switch (error) {
				case ErrorMessage.BadCommand:
					extendedErrorMessage = "Bad Command";
					shortenedErrorMessage = "BC";
					break;
				case ErrorMessage.CommandFailed:
					extendedErrorMessage = "Command Failed";
					shortenedErrorMessage = "CF";
					break;
				case ErrorMessage.DiskFull:
					extendedErrorMessage = "Disk Full";
					shortenedErrorMessage = "DF";
					break;
				case ErrorMessage.Invalid:
					extendedErrorMessage = "Invalid";
					shortenedErrorMessage = "FI";
					break;
				case ErrorMessage.ReadOnly:
					extendedErrorMessage = "Read Only";
					shortenedErrorMessage = "RO";
					break;
				case ErrorMessage.FileOpen:
					extendedErrorMessage = "File Open";
					shortenedErrorMessage = "FO";
					break;
				case ErrorMessage.DirNotEmpty:
					extendedErrorMessage = "Dir Not Empty";
					shortenedErrorMessage = "NE";
					break;
				case ErrorMessage.FilenameInvalid:
					extendedErrorMessage = "Filename Invalid";
					shortenedErrorMessage = "FN";
					break;
				case ErrorMessage.NoUpgrade:
					extendedErrorMessage = "No Upgrade";
					shortenedErrorMessage = "NU";
					break;
				case ErrorMessage.NoDisk:
					extendedErrorMessage = "No Disk";
					shortenedErrorMessage = "ND";
					break;

			}
			switch (this.CommandSet) {
				case CommandSet.Extended:
					this.emulator.SerialPort.Write(Encoding.ASCII.GetBytes(extendedErrorMessage));
					break;
				case CommandSet.Shortened:
					this.emulator.SerialPort.Write(Encoding.ASCII.GetBytes(shortenedErrorMessage));
					break;
			}
			this.emulator.SerialPort.Write(0x0D);
		}

		private void WritePrompt() {
			switch (this.CommandSet) {
				case CommandSet.Extended:
					this.emulator.SerialPort.Write(Encoding.ASCII.GetBytes(@"D:\>"));
					break;
				case CommandSet.Shortened:
					this.emulator.SerialPort.Write(0x3E);
					break;
			}
			this.emulator.SerialPort.Write(0x0D);
		}
		
	}
}
