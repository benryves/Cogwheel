using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace BeeDevelopment.Sega8Bit.Hardware.Controllers.VDrive {

	public enum NumericalMode {
		Binary,
		Ascii,
	}

	public enum CommandSet {
		Extended,
		Shortened,
	}

	public enum StatusMessage {
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
		DeviceDetected,
		DeviceRemoved,
	}

	enum IncomingDataPurpose {
		None,
		WriteFile,
	}

	public class VDrive {

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern int GetShortPathName(String pathName, StringBuilder shortName, int cbShortName);

		private string diskPath;
		public string DiskPath {
			get {
				return this.diskPath;
			}
			set {
				var hadDisk = this.HasDisk;
				var oldPath = this.diskPath;
				this.diskPath = value;

				if (this.HasDisk == hadDisk && this.diskPath == oldPath) return;

				if (this.HasDisk) {
					this.Write(StatusMessage.DeviceDetected);
					this.Write(StatusMessage.NoUpgrade);
					this.WritePrompt();
				} else {
					this.Write(StatusMessage.DeviceRemoved);
				}
			}
		}
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

		public void Reset() {
			this.incomingData.Clear();
			this.incomingDataPending = 0;
			this.incomingDataIsNumber = false;
			if (this.openFile != null) {
				this.openFile.Close();
				this.openFile = null;
			}
			this.openFileName = null;
			this.NumericalMode = NumericalMode.Binary;
			this.CommandSet = CommandSet.Extended;

			this.Write("\rVer V2DAP2.0.0-SP1 On-Line:\r");
			if (this.HasDisk) {
				this.Write(StatusMessage.DeviceDetected);
				this.Write(StatusMessage.NoUpgrade);
				this.WritePrompt();
			}
		}

		public VDrive(Emulator emulator) {
			this.emulator = emulator;

			this.incomingData = new List<byte>();
			this.emulator.SerialPort.DataRecieved += SerialPort_DataRecieved;

			this.Reset();
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

		static bool IsValidDirectoryName(string filename) {
			if (string.IsNullOrEmpty(filename)) return false;
			return filename == "." || filename == ".." || filename == "/" || IsValidFilename(filename);
		}

		static bool IsValidFilename(string filename) {
			if (string.IsNullOrEmpty(filename)) return false;
			filename = filename.ToUpperInvariant();
			return new Regex(@"^[$%'-_@~`!\(\){}\^#&A-Z0-9]{1,8}(\.[$%'-_@~`!\(\){}\^#&A-Z0-9]{1,3})?$").IsMatch(filename);
		}

		string GetFullPath(string filename) {
			if (!string.IsNullOrEmpty(this.CurrentDirectory)) {
				filename = Path.Combine(this.CurrentDirectory, filename);
			}

			filename = filename.Replace('\\', '/');

			while (filename.StartsWith("/")) filename = filename.Substring(1);

			if (!string.IsNullOrEmpty(this.DiskPath)) filename = Path.Combine(this.DiskPath, filename);

			return Path.GetFullPath(filename).Replace('\\', '/');
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
								this.Write(StatusMessage.NoDisk);
							} else if (this.openFile == null) {
								this.Write(StatusMessage.Invalid);
							} else {
								try {
									this.openFile.Write(incomingData.ToArray(), 0, incomingData.Count);
									this.openFile.SetLength(this.openFile.Position);
									this.WritePrompt();
								} catch {
									this.Write(StatusMessage.DiskFull);
								}
							}
							
							break;
						default:
							this.Write(StatusMessage.BadCommand);
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
						this.Write(StatusMessage.NoDisk);
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
					} else if ((commandString.StartsWith("SBD ") && CommandSet == CommandSet.Extended) || (commandString.StartsWith("\x14 ") && CommandSet == CommandSet.Shortened)) {
						this.WritePrompt();
						uint value = this.GetNumber(CommandSet == CommandSet.Extended ? 4 : 2, 3);
						value = (uint)(((value  & 0xFF0000) >> 16) | (value & 0x00FF00) | ((value & 0x0000FF) << 16));
						uint divisor = (value & 0x3FFF);
						uint fractionalDivisor = (value >> 14) & 0x3;
						if (divisor == 0) {
							this.emulator.SerialPort.BaudRate = 3000000;
						} else if (divisor == 1) {
							this.emulator.SerialPort.BaudRate = 2000000;
						} else {
							divisor = divisor * 8;
							switch (fractionalDivisor) {
								case 0:
									divisor += 0;
									break;
								case 1:
									divisor += 4;
									break;
								case 2:
									divisor += 2;
									break;
								case 3:
									divisor += 1;
									break;
							}
							this.emulator.SerialPort.BaudRate = (int)(24000000 / divisor);
						}
						this.WritePrompt();
					} else if ((commandString == "FWV" && CommandSet == CommandSet.Extended) || (incomingData.Count == 1 && incomingData[0] == 0x13 && CommandSet == CommandSet.Shortened)) {
						this.Write('\r');
						this.Write("MAIN 2.0.0-SP2\r");
						this.Write("RPRG ?\r");
						this.WritePrompt();
					} else if (commandString == "E" || commandString == "e") {
						this.Write(commandString + "\r");
					} else if ((commandString == "DIR" && CommandSet == CommandSet.Extended) || (incomingData.Count == 1 && incomingData[0] == 0x01 && CommandSet == CommandSet.Shortened)) {
						if (!this.HasDisk) {
							this.Write(StatusMessage.NoDisk);
						} else if (this.openFile != null) {
							this.Write(StatusMessage.FileOpen);
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
							this.Write(StatusMessage.NoDisk);
						} else if (this.openFile != null) {
							this.Write(StatusMessage.FileOpen);
						} else {
							var filename = commandString.Substring(CommandSet == CommandSet.Extended ? 4 : 2).ToUpperInvariant();
							if (!IsValidFilename(filename)) {
								this.Write(StatusMessage.FilenameInvalid);
							} else {
								var dirPath = GetFullPath(filename);
								FileSystemInfo fsi;
								if (Directory.Exists(dirPath)) {
									fsi = new DirectoryInfo(dirPath);
								} else {
									fsi = new FileInfo(dirPath);
								}
								if (!fsi.Exists) {
									this.Write(StatusMessage.CommandFailed);
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
							this.Write(StatusMessage.NoDisk);
						} else if (this.openFile != null) {
							this.Write(StatusMessage.FileOpen);
						} else {
							var dirname = commandString.Substring(CommandSet == CommandSet.Extended ? 3 : 2).ToUpperInvariant();
							if (!IsValidDirectoryName(dirname)) {
								this.Write(StatusMessage.FilenameInvalid);
							} else {
								var dirPath = GetFullPath(dirname);
								var fsi = new DirectoryInfo(dirPath);
								if (!fsi.Exists || !(dirPath + "/").StartsWith(this.DiskPath.Replace('\\', '/') + "/")) {
									this.Write(StatusMessage.CommandFailed);
								} else {
									this.CurrentDirectory = (dirPath + "/").Substring(this.DiskPath.Length + 1);
									this.WritePrompt();
								}
							}
						}
					} else if ((commandString.StartsWith("RD ") && CommandSet == CommandSet.Extended) || (commandString.StartsWith("\x04 ") && CommandSet == CommandSet.Shortened)) {
						if (!this.HasDisk) {
							this.Write(StatusMessage.NoDisk);
						} else if (this.openFile != null) {
							this.Write(StatusMessage.FileOpen);
						} else {
							var filename = commandString.Substring(CommandSet == CommandSet.Extended ? 3 : 2).ToUpperInvariant();
							if (!IsValidFilename(filename)) {
								this.Write(StatusMessage.FilenameInvalid);
							} else {
								var fsi = new FileInfo(GetFullPath(filename));
								if (!fsi.Exists) {
									this.Write(StatusMessage.CommandFailed);
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
							this.Write(StatusMessage.NoDisk);
						} else if (this.openFile != null) {
							this.Write(StatusMessage.FileOpen);
						} else {
							var filename = commandString.Substring(CommandSet == CommandSet.Extended ? 4 : 2).ToUpperInvariant().Split(' ')[0];
							if (!IsValidFilename(filename)) {
								this.Write(StatusMessage.FilenameInvalid);
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
									this.Write(StatusMessage.CommandFailed);
									openFile = null;
									return;
								}
								openFileName = filename;
								this.WritePrompt();
							}
						}
					} else if ((commandString.StartsWith("CLF ") && CommandSet == CommandSet.Extended) || (commandString.StartsWith("\x0A ") && CommandSet == CommandSet.Shortened)) {
						if (!this.HasDisk) {
							this.Write(StatusMessage.NoDisk);
						} else if (this.openFile == null) {
							this.Write(StatusMessage.CommandFailed);
						} else {
							this.openFile.Close();
							this.openFile = null;
							var filename = commandString.Substring(CommandSet == CommandSet.Extended ? 4 : 2).ToUpperInvariant();
							if (filename != this.openFileName) {
								this.Write(StatusMessage.CommandFailed);
							} else {
								this.WritePrompt();
							}
						}
					} else if ((commandString.StartsWith("WRF ") && CommandSet == CommandSet.Extended) || (commandString.StartsWith("\x08 ") && CommandSet == CommandSet.Shortened)) {
						if (!this.HasDisk) {
							this.Write(StatusMessage.NoDisk);
						} else if (this.openFile == null || !this.openFile.CanWrite) {
							this.Write(StatusMessage.Invalid);
						} else {
							this.incomingDataPending = this.GetNumber(CommandSet == CommandSet.Extended ? 4 : 2, 4);
							this.incomingDataPurpose = IncomingDataPurpose.WriteFile;
						}
					} else if ((commandString.StartsWith("RDF ") && CommandSet == CommandSet.Extended) || (commandString.StartsWith("\x0B ") && CommandSet == CommandSet.Shortened)) {
						if (!this.HasDisk) {
							this.Write(StatusMessage.NoDisk);
						} else if (this.openFile == null || !this.openFile.CanRead) {
							this.Write(StatusMessage.Invalid);
						} else {
							var data = new byte[this.GetNumber(CommandSet == CommandSet.Extended ? 4 : 2, 4)];
							int dataRead = 0;
							try {
								dataRead = this.openFile.Read(data, 0, data.Length);
							} catch { }
							this.Write(data);
							if (dataRead < data.Length) {
								this.Write(StatusMessage.CommandFailed);
							} else {
								this.WritePrompt();
							}
						}
					} else if ((commandString.StartsWith("SEK ") && CommandSet == CommandSet.Extended) || (commandString.StartsWith("\x28 ") && CommandSet == CommandSet.Shortened)) {
						if (!this.HasDisk) {
							this.Write(StatusMessage.NoDisk);
						} else if (this.openFile == null) {
							this.Write(StatusMessage.CommandFailed);
						} else {
							try {
								this.openFile.Position = this.GetNumber(CommandSet == CommandSet.Extended ? 4 : 2, 4);
								this.WritePrompt();
							} catch {
								this.Write(StatusMessage.CommandFailed);
							}
						}
					} else {
						this.Write(StatusMessage.BadCommand);
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
				} else if (
					((commandString == "SBD" && CommandSet == CommandSet.Extended) || (commandString == "\x14" && CommandSet == CommandSet.Shortened))
				) {
					this.incomingDataIsNumber = true;
					this.incomingDataPending = 3;
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

		private void Write(StatusMessage error) {
			var extendedErrorMessage = "Undefined Error";
			var shortenedErrorMessage = "UE";
			switch (error) {
				case StatusMessage.BadCommand:
					extendedErrorMessage = "Bad Command";
					shortenedErrorMessage = "BC";
					break;
				case StatusMessage.CommandFailed:
					extendedErrorMessage = "Command Failed";
					shortenedErrorMessage = "CF";
					break;
				case StatusMessage.DiskFull:
					extendedErrorMessage = "Disk Full";
					shortenedErrorMessage = "DF";
					break;
				case StatusMessage.Invalid:
					extendedErrorMessage = "Invalid";
					shortenedErrorMessage = "FI";
					break;
				case StatusMessage.ReadOnly:
					extendedErrorMessage = "Read Only";
					shortenedErrorMessage = "RO";
					break;
				case StatusMessage.FileOpen:
					extendedErrorMessage = "File Open";
					shortenedErrorMessage = "FO";
					break;
				case StatusMessage.DirNotEmpty:
					extendedErrorMessage = "Dir Not Empty";
					shortenedErrorMessage = "NE";
					break;
				case StatusMessage.FilenameInvalid:
					extendedErrorMessage = "Filename Invalid";
					shortenedErrorMessage = "FN";
					break;
				case StatusMessage.NoUpgrade:
					extendedErrorMessage = "No Upgrade";
					shortenedErrorMessage = "NU";
					break;
				case StatusMessage.NoDisk:
					extendedErrorMessage = "No Disk";
					shortenedErrorMessage = "ND";
					break;
				case StatusMessage.DeviceDetected:
					extendedErrorMessage = "Device Detected P2";
					shortenedErrorMessage = "DD2";
					break;
				case StatusMessage.DeviceRemoved:
					extendedErrorMessage = "Device Removed P2";
					shortenedErrorMessage = "DR2";
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
