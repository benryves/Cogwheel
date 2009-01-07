using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Management;

namespace BeeDevelopment.Cogwheel.JoystickInput {
	/// <summary>
	/// Provides methods for retrieving the state from a joystick.
	/// </summary>
	public class Joystick {

		/// <summary>
		/// Gets the identifier of the <see cref="Joystick"/>.
		/// </summary>
		/// <remarks>This is the identifier as used by the Windows multimedia joystick functions.</remarks>
		public int Id { get; private set; }

		/// <summary>
		/// Gets the vendor identifier of the <see cref="Joystick"/>.
		/// </summary>
		public int VendorId { get; private set; }

		/// <summary>
		/// Gets the product identifier of the <see cref="Joystick"/>.
		/// </summary>
		public int ProductId { get; private set; }

		/// <summary>
		/// Gets the name of the joystick.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the number of buttons supported by the joystick.
		/// </summary>
		public int ButtonCount { get; private set; }

		/// <summary>Gets whether the joystick has an X-axis.</summary>
		public bool HasXAxis { get; private set; }
		/// <summary>Gets whether the joystick has a Y-axis.</summary>
		public bool HasYAxis { get; private set; }
		/// <summary>Gets whether the joystick has a Z-axis.</summary>
		public bool HasZAxis { get; private set; }
		/// <summary>Gets whether the joystick has a rudder.</summary>
		public bool HasRudder { get; private set; }
		/// <summary>Gets whether the joystick has a U-axis.</summary>
		public bool HasUAxis { get; private set; }
		/// <summary>Gets whether the joystick has a V-axis.</summary>
		public bool HasVAxis { get; private set; }

		private WinMM.JOYCAPS Caps { get; set; }

		/// <summary>
		/// Determines whether the device is an XInput device or not. Returns true if it is, false if it isn't.
		/// </summary>
		public bool IsXInputDevice {
			get {
				var ParseIds = new Regex(@"([VP])ID_([\da-fA-F]{4})");
				using (var QueryPnp = new System.Management.ManagementObjectSearcher(@"\\.\root\cimv2", string.Format("Select * FROM Win32_PNPEntity"), new EnumerationOptions() {
					BlockSize = 20,
				})) {
					foreach (var PNP in QueryPnp.Get()) {
						var DeviceId = (string)PNP.Properties["DeviceID"].Value;
						if (DeviceId.Contains("IG_")) {
							var Ids = ParseIds.Matches(DeviceId);
							if (Ids.Count == 2) {
								ushort? VId = null, PId = null;
								foreach (Match M in Ids) {
									ushort Value = ushort.Parse(M.Groups[2].Value, NumberStyles.HexNumber);
									switch (M.Groups[1].Value) {
										case "V": VId = Value; break;
										case "P": PId = Value; break;
									}
								}
								if (VId.HasValue && this.VendorId == VId && PId.HasValue && this.ProductId == PId) return true;
							}
						}
					}
				}
				return false;
			}
		}

		/// <summary>
		/// Creates an instance of the <see cref="Joystick"/> class.
		/// </summary>
		/// <param name="id">The identifier of the joystick.</param>
		public Joystick(int id) {
			this.Id = id;
			WinMM.JOYCAPS Caps = new WinMM.JOYCAPS();
			if (WinMM.joyGetDevCaps(this.Id, ref Caps, Marshal.SizeOf(Caps)) != WinMM.Result.JOYERR_NOERROR) throw new InvalidOperationException();
			this.Caps = Caps;
			var FullName = GetNameFromId(Caps.VendorId, Caps.ProductId);
			this.Name = FullName ?? Caps.ProductName;
			this.VendorId = Caps.VendorId;
			this.ProductId = Caps.ProductId;
			this.ButtonCount = (int)Caps.ButtonCount;
			this.HasXAxis = Caps.AxisCount > 0;
			this.HasYAxis = Caps.AxisCount > 1;
			this.HasZAxis = Caps.AxisCount > 2;
			this.HasRudder = Caps.AxisCount > 3;
			this.HasUAxis = Caps.AxisCount > 4;
			this.HasVAxis = Caps.AxisCount > 5;
		}

		/// <summary>
		/// Gets the state of the joystick.
		/// </summary>
		/// <returns>The state of the joystick or <c>null</c>.</returns>
		public JoystickState GetState() {
			var	Info = new WinMM.JOYINFOEX();
			Info.Size = Marshal.SizeOf(Info);
			Info.Flags = WinMM.InfoFlags.JOY_RETURNBUTTONS | WinMM.InfoFlags.JOY_RETURNPOVCTS;
			var Result = WinMM.joyGetPosEx(this.Id, ref Info);
			if (Result != WinMM.Result.JOYERR_NOERROR) return null;
			var State = new JoystickState();
			State.Buttons = (Buttons)Info.Buttons;

			State.XAxis = this.Caps.AxisCount > 0 ? GetNormalisedAxisPoint(Info.X, this.Caps.XMin, this.Caps.XMax) : 0f;
			State.YAxis = this.Caps.AxisCount > 1 ? GetNormalisedAxisPoint(Info.Y, this.Caps.YMin, this.Caps.YMax) : 0f;
			State.ZAxis = this.Caps.AxisCount > 2 ? GetNormalisedAxisPoint(Info.Z, this.Caps.ZMin, this.Caps.ZMax) : 0f;
			State.Rudder = this.Caps.AxisCount > 3 ? GetNormalisedAxisPoint(Info.Rudder, this.Caps.RMin, this.Caps.RMax) : 0f;
			State.UAxis = this.Caps.AxisCount > 4 ? GetNormalisedAxisPoint(Info.U, this.Caps.UMin, this.Caps.UMax) : 0f;
			State.VAxis = this.Caps.AxisCount > 5 ? GetNormalisedAxisPoint(Info.V, this.Caps.VMin, this.Caps.VMax) : 0f;

			if (Info.PointOfView != 0xFFFF) {
				State.PointOfView = Info.PointOfView / 100f;
			}

			return State;
		}

		private static float GetNormalisedAxisPoint(int value, uint min, uint max) {
			return Math.Max(-1f, Math.Min(+1f, (float)(value - min) / (float)(max - min) * 2f - 1f));
		}

		/// <summary>
		/// Gets the name of a joystick from its vendor and product ids.
		/// </summary>
		/// <param name="vid">The vendor id.</param>
		/// <param name="pid">The product id.</param>
		/// <returns>The name of the joystick, or null if none was found.</returns>
		/// <remarks>This traverses the registry, so is probably very naughty.</remarks>
		private static string GetNameFromId(ushort vid, ushort pid) {
			var JoystickKey = Registry.CurrentUser.OpenSubKey(string.Format(@"System\CurrentControlSet\Control\MediaProperties\PrivateProperties\Joystick\OEM\VID_{0:X4}&PID_{1:X4}", vid, pid));
			if (JoystickKey == null) return null;
			return JoystickKey.GetValue("OEMName") as string;
		}

	}
}
