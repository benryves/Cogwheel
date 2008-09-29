using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace BeeDevelopment.Sega8Bit.Hardware {
	static class OPLL {
		[DllImport("emu2413.dll", EntryPoint="OPLL_new")]
		public static extern IntPtr New(int clk, int rate);
		
		[DllImport("emu2413.dll", EntryPoint = "OPLL_delete")]
		public static extern void Delete(IntPtr opll);
		
		[DllImport("emu2413.dll", EntryPoint = "OPLL_reset")]
		public static extern void Reset(IntPtr opll);

		[DllImport("emu2413.dll", EntryPoint = "OPLL_writeIO")]
		public static extern void WriteIO(IntPtr opll, int adr, int val);
		
		[DllImport("emu2413.dll", EntryPoint = "OPLL_writeReg")]
		public static extern void WriteRegister(IntPtr opll, int reg, int val);

		[DllImport("emu2413.dll", EntryPoint = "OPLL_calc_stereo")]
		public static extern void CalculateStereo(IntPtr opll, [MarshalAs(UnmanagedType.LPArray, SizeConst = 2)] int[] samples);

		[DllImport("emu2413.dll", EntryPoint = "OPLL_generate_samples")]
		public static extern void GenerateSamples(IntPtr opll, int sampleCount, [MarshalAs(UnmanagedType.LPArray)] short[] samples);
		
	}
}
