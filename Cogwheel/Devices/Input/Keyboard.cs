using System;
using System.Collections.Generic;
using System.Text;

namespace Cogwheel.Devices.Input {
    public class Keyboard {

        public enum KeyName {
            D1 = 8 * 0, D2, D3, D4, D5, D6, D7,
            Q = 8 * 1, W, E, R, T, Y, U,
            A = 8 * 2, S, D, F, G, H, J,
            Z = 8 * 3, X, C, V, B, N, M,
            EngDiers = 8 * 4, Space, HomeClr, InsDel,
            Comma = 8 * 5, FullStop, Slash, Pi, Down, Left, Right,
            K = 8 * 6, L, SemiColon, Colon, RightBracket, Return, Up,
            I = 8 * 7, O, P, AtSign, LeftBracket,
            D8 = 8 * 8, D9, D0, Minus, Caret, Yen, Break,
            Graph = 8 * 9 + 6,
            Ctrl = 8 * 10 + 6,
            Func = 8 * 11 + 5, Shift
        }


        private readonly Cogwheel.Emulation.Sega8Bit Machine;

        public Keyboard(Cogwheel.Emulation.Sega8Bit machine) {
            this.Machine = machine;
        }

        private bool[,] KeyStatus = new bool[12, 8];

        public void SetKeyState(KeyName key, bool pressed) {
            if ((int)key < 0 || (int)key >= 12 * 8) throw new Exception("Invalid key number.");
            this.KeyStatus[(int)key / 8, (int)key & 7] = pressed;
        }

        internal byte RowStatusByteA {
            get {
                byte b = 0;
                for (int i = 0; i < 8; ++i) {
                    b >>= 1;
                    if (!this.KeyStatus[i, this.Machine.Sc3000PPI.KeyboardRow]) b |= 0x80;
                }
                return b;
            }
        }

        internal byte RowStatusByteB {
            get {
                byte b = 0;
                for (int i = 0; i < 4; ++i) {
                    b >>= 1;
                    if (!this.KeyStatus[i + 8, this.Machine.Sc3000PPI.KeyboardRow]) b |= 0x08;
                }
                return b;
            }
        }
    }
}
