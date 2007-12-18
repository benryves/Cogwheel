using System;
using System.Collections.Generic;
using System.Text;

namespace BeeDevelopment.Cogwheel.Emulation {
    public partial class Sega8Bit {

        /// <summary>
        /// Decode a Game Genie code (in XXX-XXX-XXX format) into its component parts.
        /// </summary>
        /// <param name="code">The code to decode.</param>
        /// <param name="result">The resulting decoded Game Genie code.</param>
        /// <returns>True on successful decode, false on failure.</returns>
        public static bool TryDecodeGameGenieCode(string code, out GameGenieCode result) {


            if (code.Length != 11 || code[3] != '-' || code[7] != '-') goto Failed;

            string[] Chunks = new string[] { code.Substring(0, 3), code.Substring(4, 3), code.Substring(8, 3) };

            string CompleteVariable = "";

            for (int i = 0; i < 3; ++i) {
                for (int j = 0; j < 3; ++j) {
                    char c = char.ToLowerInvariant(Chunks[i][j]);
                    if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f'))) goto Failed;
                }
                CompleteVariable += Chunks[i];
            }

            ulong CodeValue = Convert.ToUInt64(CompleteVariable, 16);

            result = new GameGenieCode((byte)(CodeValue >> 28), (ushort)(((CodeValue >> 16) & 0x0FFF) | ((CodeValue & 0xF000) ^ 0xF000)), (int)(((CodeValue >> 12) ^ (CodeValue >> 8)) & 0xF), (byte)((((CodeValue >> 2) & 0x03) | ((CodeValue >> 6) & 0x3C) | ((CodeValue << 6) & 0xC0)) ^ 0xBA));

            return true;

        Failed:
            result = new GameGenieCode();
            return false;

        }

        public struct GameGenieCode {
            public readonly byte WriteValue;
            public readonly ushort Address;
            public readonly int Cloak;
            public readonly byte Reference;

            public GameGenieCode(byte writeValue, ushort address, int cloak, byte reference) {
                this.WriteValue = writeValue;
                this.Address = address;
                this.Cloak = cloak;
                this.Reference = reference;
            }
        }

        private Dictionary<ushort, GameGenieCode> GameGenie = new Dictionary<ushort,GameGenieCode>();

        public void GameGenieClearCodes() {
            GameGenie.Clear();
        }

        public void GameGenieAddCode(string code) {
            GameGenieCode g;
            if (!TryDecodeGameGenieCode(code, out g)) {
                throw new Exception("Invalid Game Genie code.");
            } else {
                GameGenie.Add(g.Address, g);
            }
        }

        public void GameGenieRemoveCode(string code) {
            GameGenieCode g;
            if (!TryDecodeGameGenieCode(code, out g)) {
                throw new Exception("Invalid Game Genie code.");
            } else {
                GameGenie.Remove(g.Address);
            }
        }
        

    }
}
