
using System;
using System.Collections;
using System.Collections.Generic;
namespace BeeDevelopment.Sega8Bit {
	/// <summary>
	/// Represents a cheat that works by patching memory at runtime.
	/// </summary>
	public class MemoryCheat {

		/// <summary>
		/// Gets or sets the address that this cheat modifies.
		/// </summary>
		public ushort Address { get; set; }

		/// <summary>
		/// Gets or sets the replacement byte.
		/// </summary>
		public byte Replacement { get; set; }

		/// <summary>
		/// Gets or sets the original byte that we are to replace.
		/// </summary>
		public byte Original { get; set; }

		/// <summary>
		/// Gets or sets the "cloak" value.
		/// </summary>
		public byte Cloak { get; set; }

		/// <summary>
		/// Tries to parse a Game Genie cheat into a <see cref="MemoryCheat"/> instance.
		/// </summary>
		/// <param name="gameGenieCode">The code to parse in the format XXX-XXX-XXX.</param>
		/// <param name="cheat">Outputs a parsed <see cref="MemoryCheat"/> instance if the code was in a recognised format.</param>
		/// <returns>True if the code could be parsed, false otherwise.</returns>
		public static bool TryParse(string gameGenieCode, out MemoryCheat cheat) {
			cheat = default(MemoryCheat);
			if (gameGenieCode == null) return false;
			string TrimmedCode = "";
			foreach (char c in gameGenieCode.Trim().ToUpperInvariant()) {
				if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F')) TrimmedCode += c;
			}
			if (TrimmedCode.Length != 9) {
				return false;
			}
			
			Int64 RawCode;

			try {
				RawCode = Convert.ToInt64(TrimmedCode, 16);
			} catch { return false; }


			cheat = new MemoryCheat() {
				Replacement = (byte)(RawCode >> 28),
				Address = (ushort)(((RawCode >> 16) & 0x0FFF) | ((RawCode & 0xF000) ^ 0xF000)),
				Cloak = (byte)(((RawCode >> 8) ^ (RawCode >> 4)) & 0xF),
				Original = (byte)((((RawCode >> 2) & 0x03) | ((RawCode >> 6) & 0x3C) | ((RawCode << 6) & 0xC0)) ^ 0xBA),				
			};

			return true;
		}

		/// <summary>
		/// Parse a Game Genie cheat into a <see cref="MemoryCheat"/> instance.
		/// </summary>
		/// <param name="gameGenieCode">The code to parse in the format XXX-XXX-XXX.</param>
		/// <returns>A <see cref="MemoryCheat"/> instance that represents the specified Game Genie code.</returns>
		public static MemoryCheat Parse(string gameGenieCode){
			MemoryCheat Result;
			if (!MemoryCheat.TryParse(gameGenieCode, out Result)) {
				throw new FormatException();
			} else {
				return Result;
			}
		}

		/// <summary>
		/// Converts the <see cref="MemoryCheat"/> back into a Game Genie code.
		/// </summary>
		/// <returns></returns>
		public override string ToString() {
			ushort MangledAddress = (ushort)(((this.Address << 4) | (this.Address >> 12)) ^ 0xF);

			int CloakRef = (this.Original ^ 0xBA);
			CloakRef = (CloakRef << 2) | (CloakRef >> 6);
			CloakRef = ((CloakRef & 0x0F0) << 4) | (CloakRef & 0x00F);
			CloakRef |= ((this.Cloak ^ (CloakRef >> 8)) << 4) & 0x0F0;

			return string.Format("{0:X2}{1:X1}-{2:X3}-{3:X3}", this.Replacement, (MangledAddress >> 12) & 0xF, MangledAddress & 0xFFF, CloakRef);
		}
	}


	/// <summary>
	/// Provides a class for maintaining collections of cheats.
	/// </summary>
	public class MemoryCheatCollection : ICollection<MemoryCheat> {

		private List<MemoryCheat> Cheats;
		private MemoryCheat[] QuickIndex;

		/// <summary>
		/// Gets or sets whether the cheats are enabled.
		/// </summary>
		public bool Enabled { get; set; }

		/// <summary>
		/// Creates an instance of the <see cref="MemoryCheatCollection"/>.
		/// </summary>
		public MemoryCheatCollection() {
			this.Enabled = true;
			this.Cheats = new List<MemoryCheat>();
			this.Clear();
		}

		/// <summary>
		/// Returns the cheat that exists at a particular location.
		/// </summary>
		/// <param name="address">The address to search for chearts at.</param>
		/// <returns>The cheat at the address, or null if one doesn't exist.</returns>
		public MemoryCheat this[ushort address] {
			get { return this.QuickIndex[address]; }
		}


		#region ICollection<MemoryCheat> Members

		/// <summary>
		/// Adds a cheat.
		/// </summary>
		/// <param name="item"></param>
		public void Add(MemoryCheat item) {
			if (this.QuickIndex[item.Address] != null) throw new InvalidOperationException("A cheat already resides at that memory location.");
			this.QuickIndex[item.Address] = item;
			this.Cheats.Add(item);
		}


		/// <summary>
		/// Clears all cheats from the collection.
		/// </summary>
		public void Clear() {
			this.Cheats.Clear();
			this.QuickIndex = new MemoryCheat[0x10000];
		}

		/// <summary>
		/// Checks whether a particular cheat exists in the collection.
		/// </summary>
		/// <param name="item">The cheat to search for.</param>
		/// <returns>True if the collection contains the cheat, false otherwise.</returns>
		public bool Contains(MemoryCheat item) {
			return this.Cheats.Contains(item);
		}

		/// <summary>
		/// Copies the collection to an array.
		/// </summary>
		/// <param name="array">The array to copy the collection to.</param>
		/// <param name="arrayIndex">The index to start copying the collection to.</param>
		public void CopyTo(MemoryCheat[] array, int arrayIndex) {
			this.Cheats.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Gets the number of cheats in the collection.
		/// </summary>
		public int Count {
			get { return this.Cheats.Count; }
		}

		/// <summary>
		/// Gets whether the collection is read only or not.
		/// </summary>
		public bool IsReadOnly {
			get { return false; }
		}

		/// <summary>
		/// Removes a <see cref="MemoryCheat"/> from the collection.
		/// </summary>
		/// <param name="item">The item to remove.</param>
		/// <returns>True if the item was removed, false otherwise.</returns>
		public bool Remove(MemoryCheat item) {
			bool Result = this.Cheats.Remove(item);
			if (Result) this.QuickIndex[item.Address] = null;
			return Result;
		}

		#endregion

		#region IEnumerable<MemoryCheat> Members

		/// <summary>
		/// Returns an enumerator for the collection.
		/// </summary>
		public IEnumerator<MemoryCheat> GetEnumerator() {
			return this.Cheats.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		/// <summary>
		/// Returns an enumerator for the collection.
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator() {
			return this.Cheats.GetEnumerator();
		}

		#endregion
	}


}
