using System;
using System.Collections.Generic;
using System.Text;

namespace BeeDevelopment.Sega8Bit.Hardware {
	/// <summary>
	/// Provides an SDSC debug console.
	/// </summary>
	public class DebugConsole {

		#region Types

		/// <summary>
		/// Defines the colour that a character may be.
		/// </summary>
		public enum CharacterColour {
			/// <summary>The colour black.</summary>
			Black,
			/// <summary>A dark blue colour.</summary>
			DarkBlue,
			/// <summary>A dark green colour.</summary>
			DarkGreen,
			/// <summary>A dark cyan colour.</summary>
			DarkCyan,
			/// <summary>A dark red colour.</summary>
			DarkRed,
			/// <summary>A dark magenta colour.</summary>
			DarkMagenta,
			/// <summary>A dark yellow colour.</summary>
			DarkYellow,
			/// <summary>A dark grey colour.</summary>
			DarkGrey,
			/// <summary>A light grey colour.</summary>
			LightGrey,
			/// <summary>A light blue colour.</summary>
			LightBlue,
			/// <summary>A light green colour.</summary>
			LightGreen,
			/// <summary>A light cyan colour.</summary>
			LightCyan,
			/// <summary>A light red colour.</summary>
			LightRed,
			/// <summary>A light magenta colour.</summary>
			LightMagenta,
			/// <summary>A light yellow colour.</summary>
			LightYellow,
			/// <summary>The colour white.</summary>
			White,
		}

		/// <summary>
		/// Defines a foreground and a background colour pair.
		/// </summary>
		public struct ColourAttribute {

			private CharacterColour foreground;
			/// <summary>
			/// Gets or sets the foreground colour.
			/// </summary>
			public CharacterColour Foreground {
				get { return this.foreground; }
				set {
					if ((int)value < 0 || (int)value > 15) throw new ArgumentOutOfRangeException();
					this.foreground = value; 
				}
			}

			private CharacterColour background;
			/// <summary>
			/// Gets or sets the background colour.
			/// </summary>
			public CharacterColour Background {
				get { return this.background; }
				set {
					if ((int)value < 0 || (int)value > 15) throw new ArgumentOutOfRangeException();
					this.background = value;
				}
			}

			/// <summary>
			/// Creates a <see cref="ColourAttribute"/> instance from a foreground and a background colour.
			/// </summary>
			/// <param name="foreground">The <see cref="Foreground"/> colour.</param>
			/// <param name="background">The <see cref="Background"/> colour.</param>
			public ColourAttribute(CharacterColour foreground, CharacterColour background) {
				this.foreground = CharacterColour.Black;
				this.background = CharacterColour.Black;
				this.Foreground = foreground;
				this.Background = background;
			}

			/// <summary>
			/// Creates a <see cref="ColourAttribute"/> instance from an attribute value.
			/// </summary>
			/// <param name="attribute">The attribute value to create an instance from.</param>
			public ColourAttribute(byte attribute) {
				this.foreground = (CharacterColour)(attribute & 0x0F);
				this.background = (CharacterColour)(attribute >> 4);				
			}

		}

		/// <summary>
		/// Specifies the action that will be taken on the next byte written to the control port.
		/// </summary>
		public enum PendingControlByteActions {
			/// <summary>No special action is taken.</summary>
			None,
			/// <summary>The current colour attribute will be set.</summary>
			SetAttribute,
			/// <summary>The cursor row will be set.</summary>
			SetRow,
			/// <summary>The cursor column will be set.</summary>
			SetColumn,
		}


		public enum PendingDataByteActions {
			/// <summary>No special action is taken.</summary>
			None,
			/// <summary>The data port is waiting for a width specifier.</summary>
			Width,
			/// <summary>The data port is waiting for a format specifier.</summary>
			Format,
			/// <summary>The data port is waiting for a data type specifier.</summary>
			DataType,
			/// <summary>The data port is waiting for a parameter specifier.</summary>
			Parameter,
		}

		/// <summary>
		/// Represents a single character and its colouring on the debug console's output.
		/// </summary>
		public struct ConsoleCharacter {

			/// <summary>
			/// Gets or sets the character value.
			/// </summary>
			public char Character {
				get { return this.character; }
				set { this.character = value; }
			}
			private char character;

			/// <summary>
			/// Gets or sets the <see cref="ColourAttribute"/> of the character.
			/// </summary>
			public ColourAttribute Colour {
				get { return this.colour; }
				set { this.colour = value; }
			}
			private ColourAttribute colour;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the <see cref="Emulator"/> that the debug console is connected to.
		/// </summary>
		public Emulator Emulator { get; private set; }

		private int cursorRow;
		/// <summary>
		/// Gets or sets the row that the cursor is currently on.
		/// </summary>
		public int CursorRow {
			get { return this.cursorRow; }
			set {
				if (value < 0 || value >= 25) throw new ArgumentOutOfRangeException();
				this.cursorRow = value; 
			}
		}

		private int cursorColumn;
		/// <summary>
		/// Gets or sets the row that the cursor is currently on.
		/// </summary>
		public int CursorColumn {
			get { return this.cursorColumn; }
			set {
				if (value < 0 || value >= 80) throw new ArgumentOutOfRangeException();
				this.cursorRow = value; 
			}
		}

		private ConsoleCharacter[,] outputBuffer = new ConsoleCharacter[80, 25];
		/// <summary>
		/// Gets or sets the output of the console as an array of <see cref="ConsoleCharacter"/>s.
		/// </summary>
		public ConsoleCharacter[,] OutputBuffer {
			get { return this.outputBuffer; }
			set { 
				if (value.GetLength(0) != this.outputBuffer.GetLength(0) || value.GetLength(1) != this.outputBuffer.GetLength(1)) throw new RankException();
				this.outputBuffer = value;
			}
		}

		/// <summary>
		/// Gets or sets the current <see cref="ColourAttribute"/>.
		/// </summary>
		public ColourAttribute CurrentColour { get; set; }

		/// <summary>
		/// Gets or sets the action that will be taken on the next byte written to the control port.
		/// </summary>
		public PendingControlByteActions PendingControlByteAction { get; set; }

		/// <summary>
		/// Gets or sets the action that will be taken on the next byte written to the data port.
		/// </summary>
		public PendingDataByteActions PendingDataByteAction { get; set; }

		/// <summary>
		/// Gets or sets the width of the formatted data.
		/// </summary>
		public string FormatWidth { get; set; }
		/// <summary>
		/// Gets or sets the mode used to format the data.
		/// </summary>
		public char FormatNumberFormat { get; set; }
		/// <summary>
		/// Gets or sets the formatting data type.
		/// </summary>
		public string FormatDataType { get; set; }
		/// <summary>
		/// Gets or sets the data parameter.
		/// </summary>
		public ushort FormatParameter { get; set; }
		/// <summary>
		/// Gets or sets the length of the data parameter in bytes.
		/// </summary>
		public int FormatParameterLength { get; set; }

		#endregion

		#region Public Methods

		/// <summary>
		/// Clears the output buffer.
		/// </summary>
		public void Clear() {
			this.OutputBuffer = new ConsoleCharacter[80, 25];
			this.CursorColumn = 0;
			this.CursorRow = 0;
			for (int c = 0; c < 80; ++c) {
				for (int r = 0; r < 25; ++r) {
					this.OutputBuffer[c, r] = new ConsoleCharacter() { 
						Character = ' ', 
						Colour = this.CurrentColour,
					};
				}
			}
			this.PendingControlByteAction = PendingControlByteActions.None;
			this.PendingDataByteAction = PendingDataByteActions.None;
		}

		/// <summary>
		/// Resets the debug console to its initial state.
		/// </summary>
		public void Reset() {
			this.CurrentColour = new ColourAttribute(CharacterColour.White, CharacterColour.Black);
			this.Clear();
		}

		/// <summary>
		/// Advances the cursor column one position.
		/// </summary>
		public void AdvanceCursorColumn() {
			++this.cursorColumn;
			if (this.cursorColumn >= 80) {
				this.AdvanceCursorRow();
			}
		}

		/// <summary>
		/// Advances the cursor row one position.
		/// </summary>
		public void AdvanceCursorRow() {
			this.cursorColumn = 0;
			++this.cursorRow;
			if (this.cursorRow >= 25) {
				this.cursorRow = 24;
				for (int r = 0; r < 24; ++r) {
					for (int c = 0; c < 80; ++c) {
						this.OutputBuffer[c, r] = this.OutputBuffer[c, r + 1];
					}
				}
				for (int c = 0; c < 80; ++c) {
					this.OutputBuffer[c, 24] = new ConsoleCharacter() {
						Character = ' ',
						Colour = this.CurrentColour,
					};
				}
			}
		}

		#endregion

		#region Events

		/// <summary>
		/// Event raised when data is output to the debug console.
		/// </summary>
		public event EventHandler DataOutput;

		/// <summary>
		/// Event raised when data is output to the debug console.
		/// </summary>
		protected virtual void OnDataOutput(EventArgs e) {
			if (DataOutput != null)
				DataOutput(this, e);
		}


		#endregion

		#region I/O

		/// <summary>
		/// Write a byte to the control port.
		/// </summary>
		/// <param name="value">The value to write to the control port.</param>
		public void WriteControl(byte value) {
			switch (this.PendingControlByteAction) {
				case PendingControlByteActions.None:
					switch (value) {
						case 1:
							// Suspend emulation (not done).
							break;
						case 2:
							this.Clear();
							break;
						case 3:
							this.PendingControlByteAction = PendingControlByteActions.SetAttribute;
							break;
						case 4:
							this.PendingControlByteAction = PendingControlByteActions.SetRow;
							break;

					}
					break;
				case PendingControlByteActions.SetRow:
					this.CursorRow = value % 25;
					this.PendingControlByteAction = PendingControlByteActions.SetColumn;
					break;
				case PendingControlByteActions.SetColumn:
					this.CursorRow = value % 80;
					this.PendingControlByteAction = PendingControlByteActions.None;
					break;
				case PendingControlByteActions.SetAttribute:
					this.PendingControlByteAction = PendingControlByteActions.None;
					break;
			}
		}

		/// <summary>
		/// Writes a single character to the debug console.
		/// </summary>
		/// <param name="character">The character to write.</param>
		public void WriteCharacter(byte character) {

			switch (character) {
				case (byte)'\r':
					this.cursorColumn = 0;
					break;
				case (byte)'\n':
					this.AdvanceCursorRow();
					break;
				default:
					this.OutputBuffer[this.CursorColumn, this.CursorRow] = new ConsoleCharacter() {
						Character = (char)character,
						Colour = this.CurrentColour,
					};
					break;
			}

			
			this.AdvanceCursorColumn();
			this.OnDataOutput(new EventArgs());
		}

		/// <summary>
		/// Writes a string to the debug console.
		/// </summary>
		/// <param name="value">The string to write.</param>
		public void WriteString(string s) {
			foreach (var Character in s) {
				if (Character < 128) {
					this.WriteCharacter((byte)Character);
				} else {
					this.WriteCharacter((byte)'?');
				}
			}
		}
		

		/// <summary>
		/// Write a byte to the data port.
		/// </summary>
		/// <param name="value">The value to write to the data port.</param>
		public void WriteData(byte value) {
			char Data = (char)value;
			if (this.PendingDataByteAction == PendingDataByteActions.None) {

				if (Data == '%') {
					this.FormatWidth = "";
					this.FormatDataType = "";
					this.FormatNumberFormat = ' ';
					this.FormatParameter = 0;
					this.PendingDataByteAction = PendingDataByteActions.Width;
				} else {
					this.WriteCharacter(value);
				}
			} else {
				if (this.PendingDataByteAction ==  PendingDataByteActions.Width && Data == '%') {
					this.WriteCharacter(value);
					this.PendingDataByteAction = PendingDataByteActions.None;
				} else {
					switch (this.PendingDataByteAction) {
						case PendingDataByteActions.Width:
							if (Data >= '0' && Data <= '9') {
								this.FormatWidth += Data;
							} else {
								this.FormatNumberFormat = Data;
								switch (this.FormatNumberFormat) {
									case 'd':
									case 'u':
									case 'x':
									case 'X':
									case 'b':
									case 'a':
									case 's':
										this.PendingDataByteAction = PendingDataByteActions.DataType;
										break;
									default:
										this.WriteString("%" + FormatWidth + FormatNumberFormat);
										break;
								}
							}
							break;
						case PendingDataByteActions.DataType:
							FormatDataType += Data;
							if (FormatDataType.Length >= 2) {
								switch (this.FormatDataType) {
									case "mw":
									case "mb":
									case "vw":
									case "vb":
										this.FormatParameterLength = 2;
										this.PendingDataByteAction = PendingDataByteActions.Parameter;
										break;
									case "pr":
									case "vr":
										this.FormatParameterLength = 1;
										this.PendingDataByteAction = PendingDataByteActions.Parameter;
										break;
									default:
										this.WriteString("%" + FormatWidth + this.FormatNumberFormat + FormatDataType);
										this.PendingDataByteAction = PendingDataByteActions.None;
										break;
								}
							}
							break;
						case PendingDataByteActions.Parameter:
							this.FormatParameter <<= 8;
							this.FormatParameter |= (ushort)(value);
							if (--this.FormatParameterLength <= 0) {
								ushort foundData = 0;
								int numberBytes = 2;
								switch (FormatDataType) {
									case "mw":
										foundData = (ushort)(this.Emulator.ReadMemory(FormatParameter) +this.Emulator.ReadMemory((ushort)(FormatParameter + 1)) * 256);
										break;
									case "mb":
										foundData = this.Emulator.ReadMemory(FormatParameter);
										numberBytes = 1;
										break;
									case "pr":

										switch (FormatParameter) {
											case 0x00: foundData = this.Emulator.RegisterB; numberBytes = 1; break;
											case 0x01: foundData = this.Emulator.RegisterC; numberBytes = 1; break;
											case 0x02: foundData = this.Emulator.RegisterD; numberBytes = 1; break;
											case 0x03: foundData = this.Emulator.RegisterE; numberBytes = 1; break;
											case 0x04: foundData = this.Emulator.RegisterH; numberBytes = 1; break;
											case 0x05: foundData = this.Emulator.RegisterL; numberBytes = 1; break;
											case 0x06: foundData = this.Emulator.RegisterF; numberBytes = 1; break;
											case 0x07: foundData = this.Emulator.RegisterA; numberBytes = 1; break;
											case 0x08: foundData = this.Emulator.RegisterPC; break;
											case 0x09: foundData = this.Emulator.RegisterSP; break;
											case 0x0A: foundData = this.Emulator.RegisterIX; break;
											case 0x0B: foundData = this.Emulator.RegisterIY; break;
											case 0x0C: foundData = this.Emulator.RegisterBC; break;
											case 0x0D: foundData = this.Emulator.RegisterDE; break;
											case 0x0E: foundData = this.Emulator.RegisterHL; break;
											case 0x0F: foundData = this.Emulator.RegisterAF; break;
											case 0x10: foundData = this.Emulator.RegisterR; numberBytes = 1; break;
											case 0x11: foundData = this.Emulator.RegisterI; numberBytes = 1; break;
											case 0x12: foundData = this.Emulator.RegisterShadowBC; break;
											case 0x13: foundData = this.Emulator.RegisterShadowHL; break;
											case 0x15: foundData = this.Emulator.RegisterShadowAF; break;
										}
										break;

								}

								string outputData = "?";
								switch (this.FormatNumberFormat) {
									case 'd':
										if (numberBytes == 1) {
											outputData = ((sbyte)foundData).ToString();
										} else {
											outputData = ((short)foundData).ToString();
										}
										break;
									case 'u':
										if (numberBytes == 1) {
											outputData = ((byte)foundData).ToString();
										} else {
											outputData = ((ushort)foundData).ToString();
										}
										break;
									case 'x':
										if (numberBytes == 1) {
											outputData = ((byte)foundData).ToString("x");
										} else {
											outputData = foundData.ToString("x");
										}
										break;
									case 'X':
										if (numberBytes == 1) {
											outputData = ((byte)foundData).ToString("X");
										} else {
											outputData = foundData.ToString("X");
										}
										break;
									case 'b':
										outputData = "";
										for (int i = 0; i < numberBytes * 8; ++i) {
											outputData = (((foundData & 1) != 0) ? '1' : '0') + outputData;
											foundData >>= 1;
										}
										break;
									case 'a':
										outputData = "" + (char)(byte)foundData;
										break;
									case 's':
										break;
								}

								if (FormatWidth != "") {
									int width;
									if (int.TryParse(FormatWidth, out width)) {
										if (outputData.Length < width) {
											outputData = outputData.PadLeft(width, (this.FormatNumberFormat == 'x' || this.FormatNumberFormat == 'X' || this.FormatNumberFormat == 'b') ? '0' : ' ');
										}
										if (outputData.Length > width) {
											outputData = outputData.Substring(outputData.Length - width);
										}
									}
								}

								this.WriteString(outputData);
								this.PendingDataByteAction = PendingDataByteActions.None;
							}
							break;

					}
				}
			}

		}

		#endregion

		#region Constructor

		/// <summary>
		/// Creates an instance of the <see cref="DebugConsole"/>.
		/// </summary>
		/// <param name="emulator">The <see cref="Emulator"/> instance that is connected to this debug console.</param>
		public DebugConsole(Emulator emulator) {
			this.Emulator = emulator;
			this.Clear();
		}

		#endregion
	}
}
