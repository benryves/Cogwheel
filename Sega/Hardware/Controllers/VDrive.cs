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

	public class VDrive {

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern int GetShortPathName(String pathName, StringBuilder shortName, int cbShortName);

		public string DiskPath { get; set; }
		public string CurrentDirectory { get; set; }

		public NumericalMode NumericalMode { get; private set; }

		public CommandSet CommandSet { get; private set; }

		
		private Emulator emulator;
		List<byte> incomingData;
		uint incomingDataPennding = 0;

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

		private void SerialPort_DataRecieved(object sender, SerialDataReceivedEventArgs e) {
			if (incomingDataPennding > 0) {
				this.incomingData.Add(e.Data);
				--incomingDataPennding;
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
					} else {
						this.Write(ErrorMessage.BadCommand);
					}
				}
				this.incomingData.Clear();
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
