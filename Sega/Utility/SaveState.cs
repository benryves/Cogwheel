using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using BeeDevelopment.Brazil;
using BeeDevelopment.Sega8Bit.Hardware.Controllers;
using BeeDevelopment.Sega8Bit.Mappers;
using BeeDevelopment.Zip;

namespace BeeDevelopment.Sega8Bit.Utility {
	public static class SaveState {

		#region Public Methods

		/// <summary>
		/// Saves the state of an <see cref="Emulator"/> to a <see cref="ZipFile"/>.
		/// </summary>
		/// <param name="emulator">The <see cref="Emulator"/> state to save.</param>
		/// <param name="zipFile">The <see cref="ZipFile"/> to save the state to.</param>
		public static void Save(Emulator emulator, ZipFile zipFile) {

			lock (emulator) {

				emulator.Sound.FlushQueuedWrites();

				IniSerialiseObject(emulator, "", "Main.ini", zipFile);
				IniSerialiseObject(emulator.Video, "Video", "Video.ini", zipFile);
				IniSerialiseObject(emulator.Sound, "Sound", "PSG.ini", zipFile);
				IniSerialiseObject(emulator.FmSound, @"Sound\OPLL", "YM2413.ini", zipFile);

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
		}

		/// <summary>
		/// Loads the state of an <see cref="Emulator"/> from a <see cref="ZipFile"/>.
		/// </summary>
		/// <param name="emulator">The emulator to load into.</param>
		/// <param name="zipFile">The <see cref="ZipFile"/> to load the state from.</param>
		public static void Load(out Emulator emulator, ZipFile zipFile) {
			emulator = (Emulator)IniDeserialiseObject(null, "", "Main.ini", zipFile);
			lock (emulator) {
				IniDeserialiseObject(emulator.Video, "Video", "Video.ini", zipFile);
				IniDeserialiseObject(emulator.Sound, "Sound", "PSG.ini", zipFile);
				IniDeserialiseObject(emulator.FmSound, @"Sound\OPLL", "YM2413.ini", zipFile);

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
							} else if (Property.PropertyType.IsArray && Property.PropertyType.GetElementType() == typeof(int)) {
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
			var IniLines = Array.ConvertAll(Encoding.UTF8.GetString(SourceData.Data).Split('\n'), s => s.Split(';')[0].Trim());

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
					var Parameter = Array.ConvertAll(IniItem.Split('='), s => s.Trim());
					if (Result == null) throw new InvalidDataException("Cannot set parameter before type is specified.");
					if (Parameter.Length != 2) throw new InvalidDataException("Invalid parameter.");

					var Property = Result.GetType().GetProperty(Parameter[0]);
					if (Property == null) continue; // No matching property found, but no matter.

					ZipFileEntry DumpedData = null;
					if (Parameter[1].StartsWith("Dump(") && Parameter[1].EndsWith(")")) {
						DumpedData = zipFile[Parameter[1].Substring(5, Parameter[1].Length - 6)];
					}

					if (Property.PropertyType.IsPrimitive) {
						Property.SetValue(Result, Convert.ChangeType(Parameter[1], Property.PropertyType), null);
					} else if (Property.PropertyType.BaseType == typeof(Enum)) {
						foreach (var EnumValue in Enum.GetValues(Property.PropertyType)) {
							if (EnumValue.ToString() == Parameter[1]) {
								Property.SetValue(Result, EnumValue, null);
							}
						}
					} else if (Property.PropertyType.IsArray && Property.PropertyType.GetElementType() == typeof(byte)) {
						if (DumpedData != null) { Property.SetValue(Result, DumpedData.Data, null); }
					} else if (Property.PropertyType.IsArray && Property.PropertyType.GetElementType() == typeof(int)) {
						if (DumpedData != null) {
							var IntArray = new int[DumpedData.Data.Length / 4];
							for (int i = 0; i < IntArray.Length; ++i) IntArray[i] = BitConverter.ToInt32(DumpedData.Data, i * 4);
							Property.SetValue(Result, IntArray, null); 
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
