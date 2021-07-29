using System;
using BeeDevelopment.Brazil;

namespace BeeDevelopment.Z80Tester {

	/// <summary>
	/// Implements a simple Z80 machine for testing instructions.
	/// </summary>
	class Tester : Z80A {

		/// <summary>
		/// The strength of the test.
		/// </summary>
		public enum Strength {
			/// <summary>
			/// Only documented flags and instructions are tested.
			/// </summary>
			Documented,
			/// <summary>
			/// All flags and instructions are tested.
			/// </summary>
			All,
		}

		private byte[] Memory;

		public Tester(Strength strength) {
			this.Memory = new byte[0x10000];
			
			// Patch memory for OS routines.
			this.Memory[0x05] = 0xD3; // OUT (n), A
			this.Memory[0x06] = 0xCB; // 0xCB
			this.Memory[0x07] = 0xC9; // RET

			this.Memory[0x38] = 0xFB; // EI
			this.Memory[0x39] = 0xC9; // RET

			// Load tester into memory.
			switch (strength) {
				case Strength.Documented:
					Array.Copy(Properties.Resources.zexdoc, 0, this.Memory, 0x100, Properties.Resources.zexdoc.Length);
					break;
				case Strength.All:
					Array.Copy(Properties.Resources.zexall, 0, this.Memory, 0x100, Properties.Resources.zexall.Length);
					break;
			}

			this.RegisterPC = 0x100;
		}

		public override byte ReadMemory(ushort address) {
			return this.Memory[address];
		}

		public override void WriteMemory(ushort address, byte value) {
			this.Memory[address] = value;
		}

		string lastOutputLine = "";
		public bool TestsComplete { get; set; }

		private void WriteChar(char c) {
			Console.Write(c);
			lastOutputLine += c;
			if (lastOutputLine.EndsWith("\r") || lastOutputLine.EndsWith("\n")) {
				lastOutputLine = "";
			} else if (lastOutputLine.EndsWith("Tests complete")) {
				TestsComplete = true;
				this.Memory[0] = 0xF3; // DI
				this.Memory[1] = 0x76; // HALT
				this.Memory[2] = 0x3C; // JP
				this.Memory[3] = 0x00;
				this.Memory[4] = 0x00;
				this.RegisterPC = 0;
			}
		}

		public override void WriteHardware(ushort port, byte value) {
			if ((port & 0xFF) == 0xCB) {
				switch (this.RegisterC) {
					case 2:
						WriteChar((char)this.RegisterE);
						break;
					case 9:
						for (int i = this.RegisterDE; this.Memory[i] != '$'; ++i) {
							WriteChar((char)this.Memory[i]);
						}
						break;
				}
			} else {
				base.WriteHardware(port, value);
			}
		}

	}

	class Program {
		static void Main(string[] args) {
			Console.Title = "Z80 Tester";
			Tester TestMachine = null;
			while (TestMachine == null) {
				if (args.Length < 1) {
					Console.Write("Select mode (All/Documented): ");
					args = new[] { Console.ReadLine() };
				}
				switch (args[0].Length < 1 ? ' ' : char.ToLowerInvariant(args[0][0])) {
					case 'a':
						TestMachine = new Tester(Tester.Strength.All);
						break;
					case 'd':
						TestMachine = new Tester(Tester.Strength.Documented);
						break;
					default:
						args = new string[0];
						break;
				}
			}
			const int CyclesPerLoop = 100000000;
			DateTime LastCycle = DateTime.Now;
			while (!(Console.KeyAvailable && Console.ReadKey(false).Key == ConsoleKey.Escape)) {
				TestMachine.FetchExecute(CyclesPerLoop);
				var ThisCycle = DateTime.Now;
				Console.Title = string.Format("Running at ~{0:N2} MHz", (CyclesPerLoop / 1000d) / (ThisCycle - LastCycle).TotalMilliseconds);
				LastCycle = ThisCycle;
				if (TestMachine.TestsComplete) {
					break;
				}
			}
			Console.Title = "Z80 Tester";
			if (TestMachine.TestsComplete) {
				Console.WriteLine();
				Console.WriteLine("Press any key to exit.");
				Console.ReadKey(false);
			}
		}
	}
}
