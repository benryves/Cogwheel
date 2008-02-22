using System;
using System.Collections.Generic;
using System.Text;

namespace BeeDevelopment.Sega8Bit {
	/// <summary>
	/// Defines a number of predefined hardware versions.
	/// </summary>
	public enum HardwareModel {
		/// <summary>A non-specific hardware version.</summary>
		Default,
		/// <summary>Represents the SG-1000 (Sega Game 1000).</summary>
		SG1000,
		/// <summary>Represents the SC-1000 (Sega Computer 3000).</summary>
		SC3000,
		/// <summary>Represents the Sega Master System or Sega Mark III.</summary>
		MasterSystem,
		/// <summary>Represents the Sega Master System 2.</summary>
		MasterSystem2,
		/// <summary>Represents the Sega Game Gear.</summary>
		GameGear,
		/// <summary>Represents the Sega Game Gear in Master System mode.</summary>
		GameGearMasterSystem,
	}
}
