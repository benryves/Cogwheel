using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Globalization;

namespace EnTech {

		/*

		UM_SETCUSTOMTIMING = WM_USER+200;
		wparam = monitor number, zero-based
		lparam = atom for string pointer
		lresult = -1 for failure else current pixel clock (integer in kHz)
		Note: pass full PowerStrip timing string* 

		UM_SETREFRESHRATE = WM_USER+201;
		wparam = monitor number, zero-based
		lparam = refresh rate (integer in Hz), or 0 for read-only
		lresult = -1 for failure else current refresh rate (integer in Hz)

		UM_SETPOLARITY = WM_USER+202;
		wparam = monitor number, zero-based
		lparam = polarity bits
		lresult = -1 for failure else current polarity bits+1

		UM_REMOTECONTROL = WM_USER+210;
		wparam = 99
		lparam = 
			0 to hide tray icon 
			1 to show tray icon, 
			2 to get build number 
		   10 to show Performance profiles
		   11 to show Color profiles
		   12 to show Display profiles
		   13 to show Application profiles
		   14 to show Adapter information
		   15 to show Monitor information
		   16 to show Hotkey manager
		   17 to show Resource manager
		   18 to show Preferences
		   19 to show Online services
		   20 to show About screen
		   21 to show Tip-of-the-day
		   22 to show Setup wizard
		   23 to show Screen fonts
		   24 to show Advanced timing options
		   25 to show Custom resolutions
		   99 to close PS
		lresult = -1 for failure else lparam+1 for success or build number (e.g., 335) if lparam was 2

		UM_SETGAMMARAMP = WM_USER+203;
		wparam = monitor number, zero-based
		lparam = atom for string pointer
		lresult = -1 for failure, 1 for success

		UM_CREATERESOLUTION = WM_USER+204;
		wparam = monitor number, zero-based
		lparam = atom for string pointer
		lresult = -1 for failure, 1 for success
		Note: pass full PowerStrip timing string*; reboot is usually necessary to see if the resolution is accepted by the display driver 

		UM_GETTIMING = WM_USER+205;
		wparam = monitor number, zero-based
		lparam = 1 to obtain pixel clock only instead of full timing string
		lresult = -1 for failure else GlobalAtom number identifiying the timing string*
		Note: be sure to call GlobalDeleteAtom after reading the string associated with the atom 

		UM_GETSETCLOCKS = WM_USER+206;
		wparam = monitor number, zero-based
		lparam = atom for string pointer
		lresult = -1 for failure else GlobalAtom number identifiying the performance string**
		Note: pass full PowerStrip performance string** to set the clocks, and ull to get clocks; be sure to call GlobalDeleteAtom after reading the string associated with the atom 

		UM_SETCUSTOMTIMINGFAST = WM_USER+211
		same parameters as UM_SETCUSTOMTIMING; result is >= 0 if successful (requires Build 534+) 


		PositiveHorizontalPolarity = 0x00;
		PositiveVerticalPolarity = 0x00;
		NegativeHorizontalPolarity = 0x02;
		NegativeVerticalPolarity = 0x04;

		*Timing string parameter definition: 
		 1 = horizontal active pixels 
		 2 = horizontal front porch 
		 3 = horizontal sync width 
		 4 = horizontal back porch 
		 5 = vertical active pixels 
		 6 = vertical front porch 
		 7 = vertical sync width 
		 8 = vertical back porch 
		 9 = pixel clock in kilohertz 
		10 = timing flags, where bit: 
			 1 = negative horizontal porlarity 
			 2 = negative vertical polarity 
			 3 = interlaced 
			 5 = composite sync 
			 7 = sync-on-green 
			 all other bits reserved 

		**Performance string parameter definition: 
		 1 = memory clock in kilohertz 
		 2 = engine clock in kilohertz 
		 3 = reserved 
		 4 = reserved 
		 5 = reserved 
		 6 = reserved 
		 7 = reserved 
		 8 = reserved 
		 9 = 2D memory clock in kilohertz (if different from 3D)
		10 = 2D engine clock in kilohertz (if different from 3D)

		*/

	/// <summary>
	/// Provides methods to remotely control PowerStrip.
	/// </summary>
	public static class PowerStrip {

		#region Types

		/// <summary>
		/// Defines polarity.
		/// </summary>
		public enum Polarity {
			/// <summary>Positive polarity.</summary>
			Positive,
			/// <summary>Negative polarity.</summary>
			Negative,
		}

		/// <summary>
		/// Represents a monitor's timing parameters.
		/// </summary>
		[ComVisible(true), ClassInterface(ClassInterfaceType.None)]
		[ProgId("EnTech.PowerStrip.TimingParameters"), Guid("f383bea8-1577-477e-a9fc-444c04f1bce0")]
		public class TimingParameters {

			/// <summary>
			/// Represents synchronization flags.
			/// </summary>
			[Flags]
			public enum SynchronizationFlags {
				/// <summary>Negative horizontal polarity.</summary>
				NegativeHorizontalPolarity = 1 << 1,
				/// <summary>Negative vertical polarity.</summary>
				NegativeVerticalPolarity = 1 << 2,
				/// <summary>Interlaced.</summary>
				Interlaced = 1 << 3,
				/// <summary>Composite synchronization.</summary>
				CompositeSync = 1 << 5,
				/// <summary>Sync-on-Green.</summary>
				SyncOnGreen = 1 << 7,
			}

			/// <summary>Horizontal active pixels.</summary>
			public int HorizontalActivePixels { get; set; }
			/// <summary>Horizontal front porch.</summary>
			public int HorizontalFrontPorch { get; set; }
			/// <summary>Horizontal synchronization width.</summary>
			public int HorizontalSyncWidth { get; set; }
			/// <summary>Horizontal back porch.</summary>
			public int HorizontalBackPorch { get; set; }
			/// <summary>Vertical active pixels.</summary>
			public int VerticalActivePixels { get; set; }
			/// <summary>Vertical front porch.</summary>
			public int VerticalFrontPorch { get; set; }
			/// <summary>Vertical synchronization width.</summary>
			public int VerticalSyncWidth { get; set; }
			/// <summary>Vertical back porch.</summary>
			public int VerticalBackPorch { get; set; }
			/// <summary>Pixel clock in kHz.</summary>
			public int PixelClock { get; set; }
			/// <summary>Synchronization flags.</summary>
			public SynchronizationFlags Synchronization { get; set; }

			/// <summary>
			/// Converts the timing parameters to a full PowerStrip timing string.
			/// </summary>
			/// <returns>A full PowerStrip timing string.</returns>
			public override string ToString() {
				return string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}", HorizontalActivePixels, HorizontalFrontPorch, HorizontalSyncWidth, HorizontalBackPorch, VerticalActivePixels, VerticalFrontPorch, VerticalSyncWidth, VerticalBackPorch, PixelClock, (int)Synchronization);
			}

			/// <summary>
			/// Parses a full PowerStrip timing string.
			/// </summary>
			/// <param name="timingString">The timing string to parse.</param>
			public void FromString(string timingString) {
				var Args = Array.ConvertAll(timingString.Split(','), s => int.Parse(s, CultureInfo.InvariantCulture));
				if (Args.Length != 10) throw new ArgumentException("Could not parse input string.", "timingString");
				this.HorizontalActivePixels = Args[0];
				this.HorizontalFrontPorch = Args[1];
				this.HorizontalSyncWidth = Args[2];
				this.HorizontalBackPorch = Args[3];
				this.VerticalActivePixels = Args[4];
				this.VerticalFrontPorch = Args[5];
				this.VerticalSyncWidth = Args[6];
				this.VerticalBackPorch = Args[7];
				this.PixelClock = Args[8];
				this.Synchronization = (SynchronizationFlags)Args[9];
			}

			/// <summary>
			/// Parse a full PowerStrip timing string into a <see cref="TimingParameters"/> instance.
			/// </summary>
			/// <param name="timingString">The timing string to parse.</param>
			/// <returns>A populated <see cref="TimingParameters"/> instance.</returns>
			public static TimingParameters Parse(string timingString) {
				var Result = new TimingParameters();
				Result.FromString(timingString);
				return Result;
			}
		}

		#endregion

		#region PowerStrip Definitions

		/// <summary>
		/// User-defined window messages.
		/// </summary>
		const int WM_USER = 0x400;

		/// <summary>
		/// PowerStrip window messages.
		/// </summary>
		enum WindowMessage {
			/// <summary>Set a monitor's custom timing.</summary>
			UM_SETCUSTOMTIMING = WM_USER + 200,
			/// <summary>Get or set a monitor's refresh rate.</summary>
			UM_SETREFRESHRATE = WM_USER + 201,
			/// <summary>Set polarity bits.</summary>
			UM_SETPOLARITY = WM_USER + 202,
			/// <summary>Remote-control the PowerStrip user interface.</summary>
			UM_REMOTECONTROL = WM_USER + 210,
			/// <summary>Set a monitor's gamma ramp.</summary>
			UM_SETGAMMARAMP = WM_USER + 203,
			/// <summary>Create a resolution for a monitor.</summary>
			UM_CREATERESOLUTION = WM_USER + 204,
			/// <summary>Get a monitor's current timing.</summary>
			UM_GETTIMING = WM_USER + 205,
			/// <summary>Get or set clock information.</summary>
			UM_GETSETCLOCKS = WM_USER + 206,
			/// <summary>Set a monitor's custom timing quickly.</summary>
			UM_SETCUSTOMTIMINGFAST = WM_USER + 21,
		}

		/// <summary>
		/// PowerStrip remote-control actions.
		/// </summary>
		enum RemoteControlAction {
			/// <summary>Hide the notification area icon.</summary>
			HideTrayIcon = 0,
			/// <summary>Show the notification area icon.</summary>
			ShowTrayIcon = 1,
			/// <summary>Get the build number.</summary>
			GetBuildNumber = 2,
			/// <summary>Show the Performance Profiles dialog.</summary>
			ShowPerformanceProfiles = 10,
			/// <summary>Show the Color Profiles dialog.</summary>
			ShowColorProfiles = 11,
			/// <summary>Show the Display Profiles dialog.</summary>
			ShowDisplayProfiles = 12,
			/// <summary>Show the Application Profiles dialog.</summary>
			ShowApplicationProfiles = 13,
			/// <summary>Show the Adapter Information dialog.</summary>
			ShowAdapterInformation = 14,
			/// <summary>Show the Monitor Information dialog.</summary>
			ShowMonitorInformation = 15,
			/// <summary>Show the HotKey Manager dialog.</summary>
			ShowHotkeyManager = 16,
			/// <summary>Show the Resource Manager dialog.</summary>
			ShowResourceManager = 17,
			/// <summary>Show the Preferences dialog.</summary>
			ShowPreferences = 18,
			/// <summary>Show the Online Services dialog.</summary>
			ShowOnlineServices = 19,
			/// <summary>Show the About dialog.</summary>
			ShowAboutScreen = 20,
			/// <summary>Show the Tip of the Day dialog.</summary>
			ShowTipOfTheDay = 21,
			/// <summary>Show the Setup Wizard dialog.</summary>
			ShowSetupWizard = 22,
			/// <summary>Show the Screen Fonts dialog.</summary>
			ShowScreenFonts = 23,
			/// <summary>Show the Advanced Timing Options dialog.</summary>
			ShowAdvancedTimingOptions = 24,
			/// <summary>Show the Custom Resolutions dialog.</summary>
			ShowCustomResolutions = 25,
			/// <summary>Close PowerStrip.</summary>
			ClosePowerStrip = 99,
		}

		/// <summary>
		/// Class name of the hidden PowerStrip API window.
		/// </summary>
		const string PowerStripWindowName = "TPShidden";

		#endregion

		#region Win32 Interop

		/// <summary>
		/// Retrieves a handle to the top-level window whose class name and window name match the specified strings.
		/// </summary>
		/// <param name="lpClassName">String that specifies the class name or a class atom created by a previous call to the RegisterClass or RegisterClassEx function.</param>
		/// <param name="lpWindowName">Pointer to a null-terminated string that specifies the window name (the window's title). If this parameter is NULL, all window names match.</param>
		/// <returns>If the function succeeds, the return value is a handle to the window that has the specified class name and window name. If the function fails, the return value is NULL.</returns>
		[DllImport("user32.dll")]
		static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		/// <summary>
		/// Sends the specified message to a window or windows.
		/// </summary>
		/// <param name="hwnd">Handle to the window whose window procedure will receive the message.</param>
		/// <param name="wMsg">Specifies the message to be sent.</param>
		/// <param name="wParam">Specifies additional message-specific information.</param>
		/// <param name="lParam">Specifies additional message-specific information.</param>
		[DllImport("user32.dll")]
		static extern IntPtr SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

		/// <summary>
		/// Sends the specified message to a window or windows.
		/// </summary>
		/// <param name="hwnd">Handle to the window whose window procedure will receive the message.</param>
		/// <param name="wMsg">Specifies the message to be sent.</param>
		/// <param name="wParam">Specifies additional message-specific information.</param>
		/// <param name="lParam">Specifies additional message-specific information.</param>
		/// <returns>True if the function succeeds, false if it fails.</returns>
		[DllImport("user32.dll")]
		static extern bool PostMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

		/// <summary>
		/// Deletes a global atom.
		/// </summary>
		/// <param name="nAtom">The atom to delete.</param>
		/// <returns>0 (always).</returns>
		[DllImport("kernel32.dll")]
		static extern Int16 GlobalDeleteAtom(short nAtom);

		/// <summary>
		/// Gets a global atom's name.
		/// </summary>
		/// <param name="nAtom">The atom to get the name of.</param>
		/// <param name="lpBuffer">A buffer to receive the name into.</param>
		/// <param name="nSize">The size of the buffer.</param>
		/// <returns>The number of characters successfully returned.</returns>
		[DllImport("kernel32.dll")]
		static extern int GlobalGetAtomName(short nAtom, StringBuilder lpBuffer, int nSize);

		/// <summary>
		/// Adds a character string to the global atom table and returns a unique value (an atom) identifying the string.
		/// </summary>
		/// <param name="lpString">String to be added</param>
		/// <returns>If the function succeeds, the return value is the newly created atom. If the function fails, the return value is zero.</returns>
		[DllImport("kernel32.dll")]
		static extern short GlobalAddAtom(string lpString);

		#endregion

		#region Private Helpers

		/// <summary>
		/// Gets the handle of the PowerStrip window. Throws an exception if it cannot be found.
		/// </summary>
		/// <returns></returns>
		static IntPtr GetPowerStripWindowHandle() {
			var Handle = FindWindow(PowerStripWindowName, null);
			if (Handle == IntPtr.Zero) {
				throw new Exception("PowerStrip not found.");
			} else {
				return Handle;
			}
		}

		/// <summary>
		/// Performs a remote control action.
		/// </summary>
		/// <param name="action">The action to perform.</param>
		/// <returns>The result code if the action carried out successfully. Throws an exception on error.</returns>
		static IntPtr RemoteControl(RemoteControlAction action) {
			var Result = SendMessage(GetPowerStripWindowHandle(), (int)WindowMessage.UM_REMOTECONTROL, (IntPtr)99, (IntPtr)action);
			if (Result == (IntPtr)(-1)) {
				throw new Exception("Could not perform action.");
			} else {
				return Result;
			}
		}

		/// <summary>
		/// Performs a remote control action to display a dialog.
		/// </summary>
		/// <param name="action">The action to perform.</param>
		static void RemoteControlDialog(RemoteControlAction action) {
			if (!PostMessage(GetPowerStripWindowHandle(), (int)WindowMessage.UM_REMOTECONTROL, (IntPtr)99, (IntPtr)action)) {
				throw new Exception("Could not perform action.");
			}
		}

		/// <summary>
		/// Gets the string value of a global atom and deletes the atom.
		/// </summary>
		/// <param name="atom">The atom to get the string value of.</param>
		/// <returns>The string value of the atom.</returns>
		static string GetGlobalAtomString(short atom) {
			var Result = new StringBuilder("".PadRight(256));
			GlobalGetAtomName(atom, Result, 255);
			GlobalDeleteAtom(atom);
			return Result.ToString();
		}

		#endregion

		#region Public Helper API

		/// <summary>
		/// Check whether PowerStrip is available, and return true if it is.
		/// </summary>
		public static bool IsAvailable {
			get { return GetPowerStripWindowHandle() != ((IntPtr)(-1)); }
		}

		#endregion

		#region Public API

		/// <summary>
		/// Sets a monitor's timing from a string.
		/// </summary>
		/// <param name="monitor">The monitor to set the timing on.</param>
		/// <param name="timingString">The full PowerStrip timing string to set the monitor to.</param>
		public static void SetTiming(int monitor, string timingString) {
			if (timingString.Length > 255) throw new ArgumentException("Timing string is over 255 characters long.", "timingString");
			var Atom = GlobalAddAtom(timingString);
			if (Atom == 0) throw new Exception("Could not set timing.");
			try {
				IntPtr Result = SendMessage(GetPowerStripWindowHandle(), (int)WindowMessage.UM_SETCUSTOMTIMING, (IntPtr)monitor, (IntPtr)Atom);
				if (Result == (IntPtr)(-1)) throw new Exception("Could not set timing.");
			} finally {
				GlobalDeleteAtom(Atom);
			}
		}

		/// <summary>
		/// Sets a monitor's timing from a <see cref="TimingParameters"/> instance.
		/// </summary>
		/// <param name="monitor">The monitor to set the timing on.</param>
		/// <param name="parameters">The <see cref="TimingParameters"/> to set the monitor to.</param>
		public static void SetTiming(int monitor, TimingParameters parameters) {
			SetTiming(monitor, parameters.ToString());
		}

		/// <summary>
		/// Sets a monitor's refresh rate.
		/// </summary>
		/// <param name="monitor">The index of the monitor to set the refresh rate on (zero based).</param>
		/// <param name="refreshRate">Refresh rate in Hz.</param>
		public static void SetRefreshRate(int monitor, int refreshRate) {
			if (SendMessage(GetPowerStripWindowHandle(), (int)WindowMessage.UM_SETREFRESHRATE, (IntPtr)monitor, (IntPtr)refreshRate) == (IntPtr)(-1)) {
				throw new Exception("Could not set refresh rate.");
			}
		}

		/// <summary>
		/// Gets a monitor's current refresh rate.
		/// </summary>
		/// <param name="monitor">The index of the monitor to get the refresh rate from (zero based).</param>
		/// <returns>Refresh rate in Hz.</returns>
		public static int GetRefreshRate(int monitor) {
			var Result = (int)SendMessage(GetPowerStripWindowHandle(), (int)WindowMessage.UM_SETREFRESHRATE, (IntPtr)monitor, (IntPtr)0);
			if (Result == -1) {
				throw new Exception("Could not get refresh rate.");
			} else {
				return Result;
			}
		}

		/// <summary>
		/// Sets the synchronization polarity of a monitor.
		/// </summary>
		/// <param name="monitor">The monitor to set the synchronization polarity on.</param>
		/// <param name="horizontal">The horizontal synchronization polarity.</param>
		/// <param name="vertical">The vertical synchronization polarity.</param>
		public static void SetSynchronizationPolarity(int monitor, Polarity horizontal, Polarity vertical) {
			if (horizontal != Polarity.Positive && horizontal != Polarity.Negative) throw new ArgumentException("Invalid polarity.", "horizontal");
			if (vertical != Polarity.Positive && vertical != Polarity.Negative) throw new ArgumentException("Invalid polarity.", "vertical");
			int PolarityBits = 0;
			if (horizontal == Polarity.Negative) PolarityBits |= 0x02;
			if (vertical == Polarity.Negative) PolarityBits |= 0x04;
			IntPtr Result = SendMessage(GetPowerStripWindowHandle(), (int)WindowMessage.UM_SETPOLARITY, (IntPtr)monitor, (IntPtr)PolarityBits);
			if (Result == (IntPtr)(-1)) throw new Exception("Could not set synchronization polarity.");
		}

		/// <summary>
		/// Hide PowerStrip's notification area icon.
		/// </summary>
		public static void HideNotifyIcon() { RemoteControl(RemoteControlAction.HideTrayIcon); }

		/// <summary>
		/// Show PowerStrip's notification area icon.
		/// </summary>
		public static void ShowNotifyIcon() { RemoteControl(RemoteControlAction.ShowTrayIcon); }

		/// <summary>
		/// Get PowerStrip's build number.
		/// </summary>
		/// <returns>PowerStrip's build number.</returns>
		public static int GetBuildNumber() { return (int)RemoteControl(RemoteControlAction.GetBuildNumber); }

		/// <summary>
		/// Show the Performance Profiles dialog.
		/// </summary>
		public static void ShowPerformanceProfiles() { RemoteControlDialog(RemoteControlAction.ShowPerformanceProfiles); }

		/// <summary>
		/// Show the Color Profiles dialog.
		/// </summary>
		public static void ShowColorProfiles() { RemoteControlDialog(RemoteControlAction.ShowColorProfiles); }

		/// <summary>
		/// Show the Display Profiles dialog.
		/// </summary>
		public static void ShowDisplayProfiles() { RemoteControlDialog(RemoteControlAction.ShowDisplayProfiles); }

		/// <summary>
		/// Show the Application Profiles dialog.
		/// </summary>
		public static void ShowApplicationProfiles() { RemoteControlDialog(RemoteControlAction.ShowApplicationProfiles); }

		/// <summary>
		/// Show the Adapter Information dialog.
		/// </summary>
		public static void ShowAdapterInformation() { RemoteControlDialog(RemoteControlAction.ShowAdapterInformation); }

		/// <summary>
		/// Show the Monitor Information dialog.
		/// </summary>
		public static void ShowMonitorInformation() { RemoteControlDialog(RemoteControlAction.ShowMonitorInformation); }

		/// <summary>
		/// Show the Hotkey Manager dialog.
		/// </summary>
		public static void ShowHotkeyManager() { RemoteControlDialog(RemoteControlAction.ShowHotkeyManager); }

		/// <summary>
		/// Show the Resource Manager dialog.
		/// </summary>
		public static void ShowResourceManager() { RemoteControlDialog(RemoteControlAction.ShowResourceManager); }

		/// <summary>
		/// Show the Preferences dialog.
		/// </summary>
		public static void ShowPreferences() { RemoteControlDialog(RemoteControlAction.ShowPreferences); }

		/// <summary>
		/// Show the Online Services dialog.
		/// </summary>
		public static void ShowOnlineServices() { RemoteControlDialog(RemoteControlAction.ShowOnlineServices); }

		/// <summary>
		/// Show the About screen.
		/// </summary>
		public static void ShowAboutScreen() { RemoteControl(RemoteControlAction.ShowAboutScreen); }

		/// <summary>
		/// Show the Tip of the Day dialog.
		/// </summary>
		public static void ShowTipOfTheDay() { RemoteControlDialog(RemoteControlAction.ShowTipOfTheDay); }

		/// <summary>
		/// Show the Setup Wizard dialog.
		/// </summary>
		public static void ShowSetupWizard() { RemoteControlDialog(RemoteControlAction.ShowSetupWizard); }

		/// <summary>
		/// Show the Screen Fonts dialog.
		/// </summary>
		public static void ShowScreenFonts() { RemoteControlDialog(RemoteControlAction.ShowScreenFonts); }

		/// <summary>
		/// Show the Advanced Timing Options dialog.
		/// </summary>
		public static void ShowAdvancedTimingOptions() { RemoteControlDialog(RemoteControlAction.ShowAdvancedTimingOptions); }

		/// <summary>
		/// Show the Custom Resolutions dialog.
		/// </summary>
		public static void ShowCustomResolutions() { RemoteControlDialog(RemoteControlAction.ShowCustomResolutions); }

		/// <summary>
		/// Close PowerStrip.
		/// </summary>
		public static void Close() { RemoteControl(RemoteControlAction.ClosePowerStrip); }

		/// <summary>
		/// Gets a full timing string for a monitor.
		/// </summary>
		/// <param name="monitor">The monitor to get a timing string for.</param>
		/// <returns>The full timing string for the specified monitor.</returns>
		public static string GetTimingString(int monitor) {
			IntPtr Result = SendMessage(GetPowerStripWindowHandle(), (int)WindowMessage.UM_GETTIMING, (IntPtr)monitor, (IntPtr)0);
			if (Result == (IntPtr)(-1)) throw new Exception("Could not get timing string.");
			return GetGlobalAtomString((short)Result);
		}

		/// <summary>
		/// Gets the <see cref="TimingParameters"/> for a monitor.
		/// </summary>
		/// <param name="monitor">The monitor to get the timing parameters for.</param>
		/// <returns>The <see cref="TimingParameters"/> for the specified monitor.</returns>
		public static TimingParameters GetTimingParameters(int monitor) {
			return TimingParameters.Parse(GetTimingString(monitor));
		}

		/// <summary>
		/// Gets the pixel clock in kHz for a monitor.
		/// </summary>
		/// <param name="monitor">The monitor to get the pixel clock from.</param>
		/// <returns>The pixel clock of the specified monitor in kHz.</returns>
		public static decimal GetPixelClock(int monitor) {
			IntPtr Result = SendMessage(GetPowerStripWindowHandle(), (int)WindowMessage.UM_GETTIMING, (IntPtr)monitor, (IntPtr)1);
			if (Result == (IntPtr)(-1)) throw new Exception("Could not get pixel clock.");
			return decimal.Parse(GetGlobalAtomString((short)Result), CultureInfo.InvariantCulture);
		}

		#endregion


	}
}
