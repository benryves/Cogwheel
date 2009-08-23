using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using BeeDevelopment.Brazil;
using BeeDevelopment.Sega8Bit.Hardware.Controllers;
using BeeDevelopment.Sega8Bit.Mappers;
using BeeDevelopment.Zip;
using System.Globalization;

#if SILVERLIGHT
using EnumEx = System.EnumEx;
using ArrayEx = System.ArrayEx;
#else
using EnumEx = System.Enum;
using ArrayEx = System.Array;
#endif

namespace BeeDevelopment.Sega8Bit.Utility {
	public static class SaveState {

		#region Public Methods

		/// <summary>
		/// Saves the state of an <see cref="Emulator"/> to a <see cref="ZipFile"/>.
		/// </summary>
		/// <param name="emulator">The <see cref="Emulator"/> state to save.</param>
		/// <param name="zipFile">The <see cref="ZipFile"/> to save the state to.</param>
		public static void Save(Emulator emulator, ZipFile zipFile) {

#if !SILVERLIGHT

			lock (emulator) {

				emulator.Sound.FlushQueuedWrites();
				emulator.FmSound.FlushQueuedWrites();

				IniSerialiseObject(emulator, "", "Main.ini", zipFile);
				IniSerialiseObject(emulator.Video, "Video", "Video.ini", zipFile);
				IniSerialiseObject(emulator.Sound, "Sound", "PSG.ini", zipFile);
#if EMU2413
				IniSerialiseObject(emulator.FmSound, @"Sound\OPLL", "YM2413.ini", zipFile);

				IniSerialiseObject(emulator.FmSound.Opll, @"Sound\OPLL\emu2413", "State.ini", zipFile);
				for (int i = 0; i < emulator.FmSound.Opll.Patch.Length; ++i) IniSerialiseObject(emulator.FmSound.Opll.Patch[i], @"Sound\OPLL\emu2413\Patches", string.Format(CultureInfo.InvariantCulture, "{0:D2}.ini", i), zipFile);
				for (int i = 0; i < emulator.FmSound.Opll.Slot.Length; ++i) {
					IniSerialiseObject(emulator.FmSound.Opll.Slot[i], string.Format(CultureInfo.InvariantCulture, @"Sound\OPLL\emu2413\Slots\{0:D2}", i), "Slot.ini", zipFile);
					IniSerialiseObject(emulator.FmSound.Opll.Slot[i].Patch, string.Format(CultureInfo.InvariantCulture, @"Sound\OPLL\emu2413\Slots\{0:D2}", i), "Patch.ini", zipFile);
				}
#endif
				for (int i = 0; i < 2; ++i) {
					IniSerialiseObject(emulator.SegaPorts[i], Path.Combine(@"Controllers\Sega", i.ToString()), "Port.ini", zipFile);
					foreach (var Property in emulator.SegaPorts[i].GetType().GetProperties()) {
						if (Property.PropertyType == typeof(SegaControllerPort.UnidirectionalPin) || Property.PropertyType == typeof(SegaControllerPort.BidirectionalPin)) {
							IniSerialiseObject(Property.GetValue(emulator.SegaPorts[i], null), Path.Combine(@"Controllers\Sega", i.ToString()), Property.Name + ".ini", zipFile);
						}
					}
				}

				foreach (var Property in emulator.GetType().GetProperties()) {
					if (Property.PropertyType == typeof(MemoryDevice)) {
						IniSerialiseObject(Property.GetValue(emulator, null), Path.Combine("Memory", Property.Name), Property.Name + ".ini", zipFile);
					}
				}
			}

#endif

		}

		/// <summary>
		/// Loads the state of an <see cref="Emulator"/> from a <see cref="ZipFile"/>.
		/// </summary>
		/// <param name="emulator">The emulator to load into.</param>
		/// <param name="zipFile">The <see cref="ZipFile"/> to load the state from.</param>
		public static void Load(out Emulator emulator, ZipFile zipFile) {
#if SILVERLIGHT
			emulator = null;
#else
			emulator = (Emulator)IniDeserialiseObject(null, "", "Main.ini", zipFile);
			lock (emulator) {
				IniDeserialiseObject(emulator.Video, "Video", "Video.ini", zipFile);
				IniDeserialiseObject(emulator.Sound, "Sound", "PSG.ini", zipFile);
#if EMU2413
				IniDeserialiseObject(emulator.FmSound, @"Sound\OPLL", "YM2413.ini", zipFile);
				IniDeserialiseObject(emulator.FmSound.Opll, @"Sound\OPLL\emu2413", "State.ini", zipFile);
				for (int i = 0; i < emulator.FmSound.Opll.Patch.Length; ++i) {
					var P = emulator.FmSound.Opll.Patch[i];
					IniDeserialiseObject(P, @"Sound\OPLL\emu2413\Patches", string.Format(CultureInfo.InvariantCulture, "{0:D2}.ini", i), zipFile);
					emulator.FmSound.Opll.Patch[i] = P;
				}
				for (int i = 0; i < emulator.FmSound.Opll.Slot.Length; ++i) {
					var S = emulator.FmSound.Opll.Slot[i];
					IniDeserialiseObject(S, string.Format(CultureInfo.InvariantCulture, @"Sound\OPLL\emu2413\Slots\{0:D2}", i), "Slot.ini", zipFile);
					var P = S.Patch;
					IniDeserialiseObject(P, string.Format(CultureInfo.InvariantCulture, @"Sound\OPLL\emu2413\Slots\{0:D2}", i), "Patch.ini", zipFile);
					S.Patch = P;
					emulator.FmSound.Opll.Slot[i] = S;
				}
				emulator.FmSound.Update();
#endif
				for (int i = 0; i < 2; ++i) {
					IniDeserialiseObject(emulator.SegaPorts[i], Path.Combine(@"Controllers\Sega", i.ToString()), "Port.ini", zipFile);
					foreach (var Property in emulator.SegaPorts[i].GetType().GetProperties()) {
						if (Property.PropertyType == typeof(SegaControllerPort.UnidirectionalPin) || Property.PropertyType == typeof(SegaControllerPort.BidirectionalPin)) {
							var DeserialisedPort = IniDeserialiseObject(Property.GetValue(emulator.SegaPorts[i], null), Path.Combine(@"Controllers\Sega", i.ToString()), Property.Name + ".ini", zipFile);
							if (DeserialisedPort != null) Property.SetValue(emulator.SegaPorts[i], DeserialisedPort, null);
						}
					}
				}

				foreach (var Property in emulator.GetType().GetProperties()) {
					if (Property.PropertyType == typeof(MemoryDevice)) {
						IniDeserialiseObject(Property.GetValue(emulator, null), Path.Combine("Memory", Property.Name), Property.Name + ".ini", zipFile);
					}
				}
			}
#endif
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Serialise an object to an ini file inside a zip archive.
		/// </summary>
		/// <param name="toSerialise">The object to serialise.</param>
		/// <param name="iniDirectory">The directory inside the zip archive to store the ini file in.</param>
		/// <param name="iniFilename">The name of the ini file.</param>
		/// <param name="zipFile">The zip file to add the ini file to.</param>
		private static void IniSerialiseObject(object toSerialise, string iniDirectory, string iniFilename, ZipFile zipFile) {

			var IniFile = new StringBuilder(2048);

			IniFile.AppendLine(string.Format(@"[Type({0})]", toSerialise.GetType().FullName));

			foreach (var Property in toSerialise.GetType().GetProperties()) {
				if (Property.CanRead) {
					if (Property.GetCustomAttributes(typeof(StateNotSavedAttribute), false).Length == 0) {
						var Value = Property.GetValue(toSerialise, null);
						if (Value != null) {
							if (Property.PropertyType.IsPrimitive || Value is Enum) {
								IniFile.AppendLine(string.Format("{0}={1}", Property.Name, Value));
							} else if (Property.PropertyType.IsArray && Property.PropertyType.GetElementType() == typeof(byte)) {
								string RawDataName = Path.Combine(iniDirectory, Property.Name + ".bin");
								zipFile.Add(new ZipFileEntry() {
									Name = RawDataName,
									Data = (byte[])Value,
								});
								IniFile.AppendLine(string.Format("{0}=Dump({1})", Property.Name, RawDataName));
							} else if (Property.PropertyType.IsArray && (Property.PropertyType.GetElementType() == typeof(short) || Property.PropertyType.GetElementType() == typeof(ushort))) {
								string RawDataName = Path.Combine(iniDirectory, Property.Name + ".bin");
								using (var DumpedShorts= new MemoryStream()) {
									using (var ShortWriter = new BinaryWriter(DumpedShorts)) {
										foreach (var i in (short[])Value) ShortWriter.Write(i);
									}
									zipFile.Add(new ZipFileEntry() {
										Name = RawDataName,
										Data = DumpedShorts.ToArray(),
									});
								}
								IniFile.AppendLine(string.Format("{0}=Dump({1})", Property.Name, RawDataName));
							} else if (Property.PropertyType.IsArray && (Property.PropertyType.GetElementType() == typeof(int) || Property.PropertyType.GetElementType() == typeof(uint))) {
								string RawDataName = Path.Combine(iniDirectory, Property.Name + ".bin");
								using (var DumpedInts = new MemoryStream()) {
									using (var IntWriter = new BinaryWriter(DumpedInts)) {
										foreach (var i in (int[])Value) IntWriter.Write(i);
									}
									zipFile.Add(new ZipFileEntry() {
										Name = RawDataName,
										Data = DumpedInts.ToArray(),
									});
								}
								IniFile.AppendLine(string.Format("{0}=Dump({1})", Property.Name, RawDataName));
							} else {
								IniFile.AppendLine(string.Format("; {0} (Type: {1})", Property.Name, Property.PropertyType));
							}
						}
					}
				}

			}

			if (toSerialise is MemoryDevice && ((MemoryDevice)toSerialise).Memory != null) {
				IniSerialiseObject(((MemoryDevice)toSerialise).Memory, iniDirectory, "Memory.ini", zipFile);
			}

			zipFile.Add(new ZipFileEntry() {
				Name = Path.Combine(iniDirectory, iniFilename),
				Data = Encoding.UTF8.GetBytes(IniFile.ToString()),
			});
		}


		
		private static object IniDeserialiseObject(object toDeserialiseInto, string iniDirectory, string iniFilename, ZipFile zipFile) {

			var SourceData = zipFile[Path.Combine(iniDirectory, iniFilename)];
			if (SourceData == null) return null; // No entry in the zip file.
			var IniLines = ArrayEx.ConvertAll(Encoding.UTF8.GetString(SourceData.Data).Split('\n'), s => s.Split(';')[0].Trim());

			object Result = toDeserialiseInto;

			foreach (var IniItem in IniLines) {
				
				if (string.IsNullOrEmpty(IniItem)) continue;

				if (IniItem.StartsWith("[") && IniItem.EndsWith("]")) {
					// Section and/or attribute.
					var SectionHeader = IniItem.Substring(1, IniItem.Length - 2);
					if (SectionHeader.StartsWith("Type(") && SectionHeader.EndsWith(")")) {
						if (toDeserialiseInto == null) {
							var TypeName = SectionHeader.Substring(5, SectionHeader.Length - 6);
							var ObjectType = Type.GetType(TypeName);
							if (ObjectType == null) throw new InvalidDataException("Cannot deserialise object of type '" + TypeName + "'.");
							Result = ObjectType.GetConstructor(Type.EmptyTypes).Invoke(null);
						}
					}
				} else {
					var Parameter = ArrayEx.ConvertAll(IniItem.Split('='), s => s.Trim());
					if (Result == null) throw new InvalidDataException("Cannot set parameter before type is specified.");
					if (Parameter.Length != 2) throw new InvalidDataException("Invalid parameter.");

					var Property = Result.GetType().GetProperty(Parameter[0]);
					if (Property == null) continue; // No matching property found, but no matter.
					if (!Property.CanWrite) continue; // No setter, but no matter.

					ZipFileEntry DumpedData = null;
					if (Parameter[1].StartsWith("Dump(") && Parameter[1].EndsWith(")")) {
						DumpedData = zipFile[Parameter[1].Substring(5, Parameter[1].Length - 6)];
					}

					if (Property.PropertyType.IsPrimitive) {
						Property.SetValue(Result, Convert.ChangeType(Parameter[1], Property.PropertyType, CultureInfo.CurrentCulture), null);
					} else if (Property.PropertyType.BaseType == typeof(Enum)) {
						foreach (var EnumValue in EnumEx.GetValues(Property.PropertyType)) {
							if (EnumValue.ToString() == Parameter[1]) {
								Property.SetValue(Result, EnumValue, null);
							}
						}
					} else if (Property.PropertyType.IsArray && Property.PropertyType.GetElementType() == typeof(byte)) {
						if (DumpedData != null) { Property.SetValue(Result, DumpedData.Data, null); }
					} else if (Property.PropertyType.IsArray && (Property.PropertyType.GetElementType() == typeof(short) || Property.PropertyType.GetElementType() == typeof(ushort))) {
						if (DumpedData != null) {
							var ShortArray = new short[DumpedData.Data.Length / 2];
							for (int i = 0; i < ShortArray.Length; ++i) ShortArray[i] = BitConverter.ToInt16(DumpedData.Data, i * 2);
							if (Property.PropertyType.GetElementType() == typeof(short)) {
								Property.SetValue(Result, ShortArray, null);
							} else if (Property.PropertyType.GetElementType() == typeof(ushort)) {
								Property.SetValue(Result, ArrayEx.ConvertAll(ShortArray, i => (ushort)i), null);
							}

						}
					} else if (Property.PropertyType.IsArray && (Property.PropertyType.GetElementType() == typeof(int) || Property.PropertyType.GetElementType() == typeof(uint))) {
						if (DumpedData != null) {
							var IntArray = new int[DumpedData.Data.Length / 4];
							for (int i = 0; i < IntArray.Length; ++i) IntArray[i] = BitConverter.ToInt32(DumpedData.Data, i * 4);
							if (Property.PropertyType.GetElementType() == typeof(int)) {
								Property.SetValue(Result, IntArray, null);
							} else if (Property.PropertyType.GetElementType() == typeof(uint)) {
								Property.SetValue(Result, ArrayEx.ConvertAll(IntArray, i => (uint)i), null);
							}
							
						}
					} else {
						// Not deserialised.
					}

				}
			}

			if (Result is MemoryDevice) {
				((MemoryDevice)Result).Memory = IniDeserialiseObject(null, iniDirectory, "Memory.ini", zipFile) as IMemoryMapper;
			}

			if (Result is IDeserializationCallback) ((IDeserializationCallback)Result).OnDeserialization(null);

			return Result;
		}

		#endregion
	}
}
