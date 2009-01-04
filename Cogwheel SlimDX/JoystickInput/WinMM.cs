using System;
using System.Runtime.InteropServices;

namespace CogwheelSlimDX.JoystickInput {
	/// <summary>
	/// Provides access to WinMM types and functions.
	/// </summary>
	static class WinMM {

		#region Constants

		private const int MAXPNAMELEN = 32;
		private const int MAX_JOYSTICKOEMVXDNAME = 260;

		#endregion

		#region Enumerations

		/// <summary>
		/// Defines error codes.
		/// </summary>
		public enum Result {
			/// <summary>
			/// Defines the base index of multimedia error code indices.
			/// </summary>
			MMSYSERR_BASE = 0,
			/// <summary>
			/// The specified identifier is invalid.
			/// </summary>
			MMSYSERR_BADDEVICEID = (MMSYSERR_BASE + 2),
			/// <summary>
			/// The device driver is not present.
			/// </summary>
			MMSYSERR_NODRIVER = (MMSYSERR_BASE + 6),
			/// <summary>
			/// An invalid parameter was passed. 
			/// </summary>
			MMSYSERR_INVALPARAM = (MMSYSERR_BASE + 11),
			/// <summary>
			/// Defines the base index of joystick error code indices.
			/// </summary>
			JOYERR_BASE = 160,
			/// <summary>
			/// No error occurred.
			/// </summary>
			JOYERR_NOERROR = (0),
			/// <summary>
			/// An invalid joystick parameter was passed. 
			/// </summary>
			JOYERR_PARMS = (JOYERR_BASE + 5),
			/// <summary>
			/// The specified joystick is not connected to the system.
			/// </summary>
			JOYERR_UNPLUGGED = (JOYERR_BASE + 7),
		}

		/// <summary>
		/// Flags indicating the valid information returned in the JOYINFOEX structure.
		/// </summary>
		[Flags]
		public enum InfoFlags {
			/// <summary>
			/// Equivalent to setting all of the JOY_RETURN bits except JOY_RETURNRAWDATA.
			/// </summary>
			JOY_RETURNALL = (JOY_RETURNX | JOY_RETURNY | JOY_RETURNZ | JOY_RETURNR | JOY_RETURNU | JOY_RETURNV | JOY_RETURNPOV | JOY_RETURNBUTTONS),
			/// <summary>
			/// The Buttons member contains valid information about the state of each joystick button.
			/// </summary>
			JOY_RETURNBUTTONS = 0x80,
			/// <summary>
			/// Centers the joystick neutral position to the middle value of each axis of movement.
			/// </summary>
			JOY_RETURNCENTERED = 0x400,
			/// <summary>
			/// The POV member contains valid information about the point-of-view control, expressed in discrete units.
			/// </summary>
			JOY_RETURNPOV = 0x40,
			/// <summary>
			/// The POV member contains valid information about the point-of-view control expressed in continuous, one-hundredth degree units.
			/// </summary>
			JOY_RETURNPOVCTS = 0x200,
			/// <summary>
			/// The Rpos member contains valid rudder pedal data. This information represents another (fourth) axis.
			/// </summary>
			JOY_RETURNR = 0x8,
			/// <summary>
			/// Data stored in this structure is uncalibrated joystick readings.
			/// </summary>
			JOY_RETURNRAWDATA = 0x100,
			/// <summary>
			/// The Upos member contains valid data for a fifth axis of the joystick, if such an axis is available, or returns zero otherwise.
			/// </summary>
			JOY_RETURNU = 0x10,
			/// <summary>
			/// The Vpos member contains valid data for a sixth axis of the joystick, if such an axis is available, or returns zero otherwise.
			/// </summary>
			JOY_RETURNV = 0x20,
			/// <summary>
			/// The Xpos member contains valid data for the x-coordinate of the joystick.
			/// </summary>
			JOY_RETURNX = 0x1,
			/// <summary>
			/// The Ypos member contains valid data for the y-coordinate of the joystick.
			/// </summary>
			JOY_RETURNY = 0x2,
			/// <summary>
			/// The Zpos member contains valid data for the z-coordinate of the joystick.
			/// </summary>
			JOY_RETURNZ = 0x4,
			/// <summary>
			/// Expands the range for the neutral position of the joystick and calls this range the dead zone. The joystick driver returns a constant value for all positions in the dead zone.
			/// </summary>
			JOY_USEDEADZONE = 0x800,
		}

		#endregion

		#region Types

		[StructLayout(LayoutKind.Sequential)]
		public struct JOYINFOEX {
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

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct JOYCAPS {
			public ushort VendorId;
			public ushort ProductId;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAXPNAMELEN)]
			public string ProductName;
			public uint XMin;
			public uint XMax;
			public uint YMin;
			public uint YMax;
			public uint ZMin;
			public uint ZMax;
			public uint ButtonCount;
			public uint PeriodMin;
			public uint PeriodMax;
			public uint RMin;
			public uint RMax;
			public uint UMin;
			public uint UMax;
			public uint VMin;
			public uint VMax;
			public uint Capabilities;
			public uint MaximumAxisCount;
			public uint AxisCount;
			public uint MaximumButtonCount;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAXPNAMELEN)]
			public string RegistryKey;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_JOYSTICKOEMVXDNAME)]
			public string OemVxD;

		}

		#endregion

		#region Declarations

		[DllImport("winmm.dll")]
		public static extern int joyGetNumDevs();

		[DllImport("winmm.dll")]
		public static extern Result joyGetPosEx(int uJoyID, ref JOYINFOEX pji);

		[DllImport("winmm.dll", EntryPoint = "joyGetDevCapsW")]
		public static extern Result joyGetDevCaps(int id, ref JOYCAPS lpCaps, int uSize);

		#endregion

	}
}
