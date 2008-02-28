using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;

namespace CogwheelSlimDX {

	

	class Joysticks {

		#region WinMM

		[DllImport("winmm.dll")]
		private static extern int joyGetNumDevs();

		[DllImport("winmm.dll")]
		private static extern JoystickResult joyGetPosEx(int uJoyID, ref JoyInfoEx pji);

		[DllImport("winmm.dll", EntryPoint = "joyGetDevCapsW")]
		private static extern JoystickResult joyGetDevCaps(int id, ref JoyCaps lpCaps, int uSize);

		[Flags]
		enum InfoFlags {
			JOY_RETURNALL = (JOY_RETURNX | JOY_RETURNY | JOY_RETURNZ | JOY_RETURNR | JOY_RETURNU | JOY_RETURNV | JOY_RETURNPOV | JOY_RETURNBUTTONS),
			JOY_RETURNBUTTONS = 0x80,
			JOY_RETURNCENTERED = 0x400,
			JOY_RETURNPOV = 0x40,
			JOY_RETURNPOVCTS = 0x200,
			JOY_RETURNR = 0x8,
			JOY_RETURNRAWDATA = 0x100,
			JOY_RETURNU = 0x10,
			JOY_RETURNV = 0x20,
			JOY_RETURNX = 0x1,
			JOY_RETURNY = 0x2,
			JOY_RETURNZ = 0x4,
			JOY_USEDEADZONE = 0x800,
		};

		enum JoystickResult {
			MMSYSERR_BASE = 0,
			JOYERR_BASE = 160,
			JOYERR_UNPLUGGED = (JOYERR_BASE + 7),
			JOYERR_PARMS = (JOYERR_BASE + 5),
			MMSYSERR_NODRIVER = (MMSYSERR_BASE + 6),
			MMSYSERR_INVALPARAM = (MMSYSERR_BASE + 11),
			MMSYSERR_BADDEVICEID = (MMSYSERR_BASE + 2),
			JOYERR_NOERROR = (0),

		}

		[StructLayout(LayoutKind.Sequential)]
		private struct JoyInfoEx {
			public int Size; // size of structure dwSize;
			public InfoFlags Flags; // flags to indicate what to return dwFlags;
			public int X; // x position dwXpos;
			public int Y; // y position dwYpos;
			public int Z; // z position dwZpos;
			public int Rudder; // rudder/4th axis position dwRpos;
			public int U; // 5th axis position dwUpos;
			public int V; // 6th axis position dwVpos;
			public int Buttons; // button states dwButtons;
			public int ButtonNumber; // current button number pressed dwButtonNumber;
			public int PointOfView; // point of view state dwPOV;
			public int Reserved1; // reserved for communication between winmm driver dwReserved1;
			public int Reserved2; // reserved for future expansion dwReserved2;
		};

		private const int MAXPNAMELEN = 32;
		private const int MAX_JOYSTICKOEMVXDNAME = 260;

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
		private struct JoyCaps {
			public ushort wMid;
			public ushort wPid;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAXPNAMELEN)]
			public string szPname;
			public uint wXmin;
			public uint wXmax;
			public uint wYmin;
			public uint wYmax;
			public uint wZmin;
			public uint wZmax;
			public uint wNumButtons;
			public uint wPeriodMin;
			public uint wPeriodMax;
			public uint wRmin;
			public uint wRmax;
			public uint wUmin;
			public uint wUmax;
			public uint wVmin;
			public uint wVmax;
			public uint wCaps;
			public uint wMaxAxes;
			public uint wNumAxes;
			public uint wMaxButtons;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAXPNAMELEN)]
			public string szRegKey;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_JOYSTICKOEMVXDNAME)]
			public string szOEMVxD;

		}
		
		#endregion

		private static bool IsConnected(int id) {
			JoyInfoEx Info = new JoyInfoEx();
			Info.Size = Marshal.SizeOf(typeof(JoyInfoEx));
			Info.Flags = InfoFlags.JOY_RETURNBUTTONS | InfoFlags.JOY_RETURNCENTERED | InfoFlags.JOY_RETURNX | InfoFlags.JOY_RETURNY | InfoFlags.JOY_USEDEADZONE;

			return joyGetPosEx(id, ref Info) == JoystickResult.JOYERR_NOERROR;
		}

		/// <summary>
		/// Provides methods for accessing joystick data.
		/// </summary>
		public class Joystick {

			/// <summary>
			/// Gets the id of the joystick.
			/// </summary>
			public int Id { get; private set; }

			/// <summary>
			/// Gets the product name of the joystick.
			/// </summary>
			public string Name { get; private set; }

			/// <summary>
			/// Gets whether the joystick is currently connected.
			/// </summary>
			public bool IsConnected {
				get { return Joysticks.IsConnected(this.Id); }
			}

			public Joystick(int id) {
				this.Id = id;
				JoyCaps Caps = new JoyCaps();
				if (joyGetDevCaps(this.Id, ref Caps, Marshal.SizeOf(typeof(JoyCaps))) != JoystickResult.JOYERR_NOERROR) throw new IndexOutOfRangeException();
				this.Name = Caps.szPname;
			}

			



		}

		public Joystick[] Items { get; private set; }

		public Joysticks() {
			List<Joystick> Sticks = new List<Joystick>();
			int MaxJoysticks = joyGetNumDevs();
			for (int i = 0; i < MaxJoysticks; i++) {
				JoyCaps Caps = new JoyCaps();
				if (joyGetDevCaps(i, ref Caps, Marshal.SizeOf(typeof(JoyCaps))) == JoystickResult.JOYERR_NOERROR) {
					Sticks.Add(new Joystick(i));
				}
			}
			this.Items = Sticks.ToArray();
		}
		



	}
}
