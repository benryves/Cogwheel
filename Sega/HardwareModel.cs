﻿
namespace BeeDevelopment.Sega8Bit {

	/// <summary>
	/// Defines a number of predefined hardware versions.
	/// </summary>
	/// <remarks>A <see cref="HardwareModel"/> defines an individual model; to group these together by standard features use <see cref="HardwareFamily"/>.</remarks>
	public enum HardwareModel {
		/// <summary>A non-specific hardware version.</summary>
		Default,
		/// <summary>Represents the SG-1000 (Sega Game 1000).</summary>
		SG1000,
		/// <summary>Represents the SC-3000 (Sega Computer 3000).</summary>
		SC3000,
		/// <summary>Represents the SF-7000 expansion for the SC-3000.</summary>
		SF7000,
		/// <summary>Represents the Sega Master System or Sega Mark III.</summary>
		MasterSystem,
		/// <summary>Represents the Sega Master System 2.</summary>
		MasterSystem2,
		/// <summary>Represents the Sega Game Gear.</summary>
		GameGear,
		/// <summary>Represents the Sega Game Gear in Master System mode.</summary>
		GameGearMasterSystem,
		/// <summary>Represents the ColecoVision.</summary>
		ColecoVision,
	}

	/// <summary>
	/// Defines a general hardware family.
	/// </summary>
	/// <remarks>This is used to indicate which I/O and memory maps to use. For a more specific device representation, use the <see cref="HardwareModel"/> enumeration.</remarks>
	public enum HardwareFamily { 
		/// <summary>A non-specific hardware family.</summary>
		Default,
		/// <summary>Sega SC-3000 and compatible consoles.</summary>
		SC3000,
		/// <summary>Sega SF-7000 and compatible consoles.</summary>
		SF7000,
		/// <summary>Sega Master System and compatible consoles.</summary>
		MasterSystem,
		/// <summary>The Coleco Industries ColecoVision.</summary>
		ColecoVision,
	}

}
