using System;
using BeeDevelopment.Sega8Bit.Mappers;
using BeeDevelopment.Brazil;

namespace BeeDevelopment.Sega8Bit {
	/// <summary>
	/// Represents a potential memory device.
	/// </summary>
	[Serializable()]
	public class MemoryDevice {

		#region Types

		/// <summary>Defines the different ways in which the acccessibility of a <see cref="MemoryDevice"/> can be modified.</summary>
		public enum AccessibilityMode {
			/// <summary>The memory is never accessible.</summary>
			Never,
			/// <summary>The state of the memory's accessibility depends on its<see cref="Enabled"/> flag.</summary>
			Optional,
			/// <summary>The memory is always accessible.</summary>
			Always,
		}

		#endregion

		#region Properties

		private IMemoryMapper memory;
		/// <summary>Gets or sets the underlying <see cref="IMemoryMapper"/> that provides access to the memory.</summary>
		/// <remarks>Set this to <c>null</c> to represent memory that is not present (for example, an empty cartridge slot).</remarks>
		public IMemoryMapper Memory {
			get { return this.memory; }
			set { this.memory = value; }
		}

		private AccessibilityMode accessibility;
		/// <summary>Gets or sets the <see cref="AccessibilityMode"/> of the memory.</summary>
		public AccessibilityMode Accessibility {
			get { return this.accessibility; }
			set {
				this.accessibility = value;
				this.UpdateAccessibility();
			}
		}

		private bool enabled;
		/// <summary>Gets or sets whether the device is enabled or disabled.</summary>
		/// <remarks>Some devices cannot be disabled, in which case this flag is ignored.</remarks>
		public bool Enabled {
			get { return this.enabled; }
			set {
				this.enabled = value;
				this.UpdateAccessibility();
			}
		}

		/// <summary>
		/// Update whether the <see cref="MemoryDevice"/> is accessible or not.
		/// </summary>
		private void UpdateAccessibility() {
			switch (this.accessibility) {
				case AccessibilityMode.Always:
					this.isAccessible = true;
					break;
				case AccessibilityMode.Optional:
					this.isAccessible = this.enabled;
					break;
				case AccessibilityMode.Never:
					this.isAccessible = false;
					break;
				default:
					this.isAccessible = false;
					break;
			}
		}

		private bool isAccessible;
		/// <summary>Gets whether the memory is currently accessible or not.</summary>
		[StateNotSaved()]
		public bool IsAccessible {
			get { return this.isAccessible; }
		}

		#endregion

		#region Methods

		/// <summary>Resets the <see cref="MemoryDevice"/> and its <see cref="Memory"/> if available.</summary>
		/// <remarks>This will re-enable the device.</remarks>
		public void Reset() {
			this.Enabled = true;
			this.Accessibility = AccessibilityMode.Optional;
			if (this.Memory != null) this.Memory.Reset();
		}

		/// <summary>Writes a byte to memory.</summary>
		/// <param name="address">The address to write to.</param>
		/// <param name="value">The value to write.</param>
		/// <remarks>Memory is only written to if <see cref="IsAccessible"/> is <c>true</c>.</remarks>
		public void Write(ushort address, byte value) {
			if (this.isAccessible && this.memory != null) this.memory.WriteMemory(address, value);
		}

		/// <summary>Reads a byte from memory.</summary>
		/// <param name="address">The address to read from.</param>
		/// <returns>The value read from memory if it is accessible and present; otherwise, 0xFF.</returns>
		/// <remarks>If the memory is not present or <see cref="IsAccessible"/> is <c>false</c>, this method returns 0xFF.</remarks>
		public byte Read(ushort address) {
			return (this.isAccessible && this.Memory != null) ? this.Memory.ReadMemory(address) : (byte)0xFF;
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Creates an instance of <see cref="MemoryDevice"/>.
		/// </summary>
		public MemoryDevice() {
			this.Reset();
		}

		#endregion
	}
}
