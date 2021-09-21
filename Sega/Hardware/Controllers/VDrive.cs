using System;
using System.Collections.Generic;
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

	public class VDrive : SerialPort {

		private string path;
		public string Path {
			get { return this.path; }
			set {
				this.path = value;
			}
		}

		public NumericalMode NumericalMode { get; private set; }

		public CommandSet CommandSet { get; private set; }

		public VDrive(Emulator emulator) : base(emulator) {
			this.NumericalMode = NumericalMode.Binary;
			this.CommandSet = CommandSet.Extended;
		}

		public VDrive(Emulator emulator, string path) : this(emulator) {
			this.Path = path;
		}

		protected override void OnDataRecieved(SerialDataReceivedEventArgs e) {
			Console.WriteLine();
			base.OnDataRecieved(e);
		}
	}
}
