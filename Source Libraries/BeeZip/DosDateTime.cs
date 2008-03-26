#region Using Directives
using System;
#endregion

namespace BeeDevelopment.Zip {

	/// <summary>
	/// Represents a date and time stored in the format used by MS-DOS.
	/// </summary>
	public class DosDateTime {

		/// <summary>
		/// Gets the seconds component of the date represented by this instance.
		/// </summary>
		public int Second { get; set; }

		/// <summary>
		/// Gets the minute component of the date represented by this instance.
		/// </summary>
		public int Minute { get; set; }

		/// <summary>
		/// Gets the hour component of the date represented by this instance.
		/// </summary>
		public int Hour { get; set; }

		/// <summary>
		/// Gets or sets the day of the month represented by this instance.
		/// </summary>
		public int Day { get; set; }

		/// <summary>
		/// Gets the month component of the date represented by this instance.
		/// </summary>
		public int Month { get; set; }

		/// <summary>
		/// Gets the year component of the date represented by this instance.
		/// </summary>
		public int Year { get; set; }


		/// <summary>
		/// Creates a new instance of the <see cref="DosDateTime"/> class representing specified year, month, day, hour, minute, and second.
		/// </summary>
		/// <param name="year">The year.</param>
		/// <param name="month">The month.</param>
		/// <param name="day">The day.</param>
		/// <param name="hour">The hours.</param>
		/// <param name="minute">The minutes.</param>
		/// <param name="second">The seconds.</param>
		public DosDateTime(int year, int month, int day, int hour, int minute, int second) {
			this.Year = year; this.Month = month; this.Day = day;
			this.Hour = hour; this.Minute = minute; this.Second = second;
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DosDateTime"/> class representing 12:00 AM on the 1st January 1980.
		/// </summary>
		public DosDateTime()
			: this(1980, 1, 1, 0, 0, 0) {
		}

		/// <summary>
		/// Creates a <see cref="DosDateTime"/> instance from a <see cref="DateTime"/>.
		/// </summary>
		/// <param name="dateTime">The <see cref="DateTime"/> to convert to a <see cref="DosDateTime"/>.</param>
		/// <returns>A new <see cref="DosDateTime"/> instance representing the date and time specified in the <see cref="DateTime"/>.</returns>
		public static DosDateTime FromDateTime(DateTime dateTime) {
			return new DosDateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
		}

		/// <summary>
		/// Converts this instance to a <see cref="DateTime"/> representing the date and time.
		/// </summary>
		/// <returns>The converted <see cref="DateTime"/> instance.</returns>
		public DateTime ToDateTime() {
			return new DateTime(Year, Month, Day, Hour, Minute, Second);
		}

		/// <summary>
		/// Serializes the current <see cref="DosDateTime"/> object to a 32-bit binary value that subsequently can be used to recreate the <see cref="DosDateTime"/> object. 
		/// </summary>
		public int ToBinary() {
			return (((((Year - 1980) << 9) & 0xFE00) | ((Month << 5) & 0x01E0) | (Day & 0x001F)) << 16) | (((Hour << 11) & 0xF800) | ((Minute << 5) & 0x07E0) | (Second & 0x001F));
		}

		/// <summary>
		/// Deserializes a 32-bit binary value and recreates an original serialized <see cref="DosDateTime"/> object. 
		/// </summary>
		/// <param name="dateData">A 32-bit integer that encode the date and time.</param>
		/// <returns>The <see cref="DosDateTime"/> instance serialised to a 32-bit integer value.</returns>
		public static DosDateTime FromBinary(int dateData) {
			DosDateTime Result = new DosDateTime();
			Result.Hour = (dateData & 0xF800) >> 11;
			Result.Minute = (dateData & 0x07E0) >> 5;
			Result.Second = dateData & 0x001F;
			dateData >>= 16;
			Result.Year = 1980 + ((dateData & 0xFE00) >> 9);
			Result.Month = (dateData & 0x01E0) >> 5;
			Result.Day = dateData & 0x001F;
			return Result;
		}

		/// <summary>
		/// Converts the value of the current <see cref="DosDateTime"/> to its equivalent string representation.
		/// </summary>
		public override string ToString() {
			return this.ToDateTime().ToString();
		}
	}
}
