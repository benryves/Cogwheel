using System.Globalization;
using System.Collections.Generic;
using System.Reflection;

namespace System {

	class SerializableAttribute : Attribute { }
	class NonSerializedAttribute : Attribute { }
	
	interface IDeserializationCallback {
		void OnDeserialization(object sender);
	}
	
	public static class Extensions {
		public static string ToLowerInvariant(this string s) { return s.ToLower(CultureInfo.InvariantCulture); }
		public static string ToUpperInvariant(this string s) { return s.ToUpper(CultureInfo.InvariantCulture); }
	}

	static class EnumEx {
		public static Enum[] GetValues(Type enumType) {
			List<Enum> values = new List<Enum>();
			var fields = enumType.GetFields();
			foreach (FieldInfo field in fields) {
				if (!field.IsLiteral) continue;
				object value = field.GetValue(enumType);
				values.Add((Enum)value);
			}
			return values.ToArray();
		}
		public static Enum[] GetValues(Enum en) {
			return GetValues(en.GetType());			
		}
	}

	static class CharEx {
		public static char ToUpperInvariant(char c) {
			return char.ToUpper(c, CultureInfo.InvariantCulture);
		}
	}

	static class ArrayEx {
		public static TOut[] ConvertAll<TIn, TOut>(TIn[] input, Func<TIn, TOut> fn) {
			TOut[] result = new TOut[input.Length];
			for (int i = 0; i < input.Length; i++) {
				result[i] = fn(input[i]);
			}
			return result;
		}
	}
}

namespace System.IO {
	class InvalidDataException : Exception {
		public InvalidDataException() {  }
		public InvalidDataException(string message) : base(message) { }
	}
	static class FileEx {
		public static byte[] ReadAllBytes(string filename) {
			using (var s = File.OpenRead(filename)) {
				var Result = new byte[s.Length];
				s.Read(Result, 0, Result.Length);
				return Result;
			}
			
		}
	}
}

namespace System.Text {
	public static class Extensions {
		public static string GetString(this Encoding encoding, byte[] data) {
			return encoding.GetString(data, 0, data.Length);
		}
	}
}