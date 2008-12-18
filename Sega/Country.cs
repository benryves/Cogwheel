using System;
using System.Collections.Generic;
using System.Text;

#if SILVERLIGHT
using EnumEx = System.EnumEx;
#else
using EnumEx = System.Enum;
#endif

namespace BeeDevelopment.Sega8Bit {

	/// <summary>
	/// Defines country information.
	/// </summary>
	public enum Country {
		/// <summary>No specific country.</summary>
		None,
		/// <summary>Japan.</summary>
		Japan, // JP
		/// <summary>Brazil.</summary>
		Brazil, // BR
		/// <summary>The United States of America.</summary>
		UnitedStates, // US
		/// <summary>Korea.</summary>
		Korea, // KR
		/// <summary>France.</summary>
		France, // FR
		/// <summary>Spain.</summary>
		Spain, // ES
		/// <summary>Germany.</summary>
		Germany, // DE
		/// <summary>Italy.</summary>
		Italy, // IT
		/// <summary>England.</summary>
		England, // EN
		/// <summary>New Zealand.</summary>
		NewZealand, // NZ
	}

	/// <summary>
	/// Provides methods for dealing with converting country information.
	/// </summary>
	public static class Countries {

		/// <summary>
		/// Converts a <see cref="Country"/> to its two-character identifier.
		/// </summary>
		/// <param name="country">The <see cref="Country"/> to convert.</param>
		/// <returns>The two-character country identifier, or null if no identifier is defined,</returns>
		public static string CountryToIdentifier(Country country) {
			switch (country) {
				case Country.Japan: return "JP";
				case Country.Brazil: return "BR";
				case Country.UnitedStates: return "US";
				case Country.Korea: return "KR";
				case Country.France: return "FR";
				case Country.Spain: return "ES";
				case Country.Germany: return "DE";
				case Country.Italy: return "IT";
				case Country.England: return "EN";
				case Country.NewZealand: return "NZ";
			}
			return null;
		}

		/// <summary>
		/// Converts a two-character identifier into a <see cref="Country"/>.
		/// </summary>
		/// <param name="identifier">The identifier to convert.</param>
		/// <returns>The converted <see cref="Country"/> (set to <c>None</c> if no country could be matched).</returns>
		public static Country IdentifierToCountry(string identifier) {
			if (string.IsNullOrEmpty(identifier) || identifier.Length != 2) return Country.None;

			foreach (Country PossibleMatch in EnumEx.GetValues(typeof(Country))) {
				if (CountryToIdentifier(PossibleMatch) == identifier.ToUpperInvariant()) return PossibleMatch;
			}

			return Country.None;
		}

		/// <summary>
		/// Converts a <see cref="Country"/> into its corresponding <see cref="Region"/>.
		/// </summary>
		/// <param name="country">The <see cref="Country"/> to convert to a <see cref="Region"/>.</param>
		/// <returns>The converted region.</returns>
		public static Region CountryToRegion(Country country) {
			switch (country) {
				case Country.Japan:
				case Country.Korea:
					return Region.Japanese;
				default:
					return Region.Export;
			}
		}

	}

}
