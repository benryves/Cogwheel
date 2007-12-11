using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Cogwheel.Devices {

    public partial class VideoDisplayProcessor {

        private bool generateOutputBitmap = true;
        public bool GenerateOutputBitmap {
            get { return this.generateOutputBitmap; }
            set { this.generateOutputBitmap = value; }
        }

        public class BlankEventArgs : EventArgs {

            public readonly bool Dirty;
            public readonly Bitmap Output;
            public readonly int[] Pixels;
            public readonly Color BorderColour;

            public BlankEventArgs(bool dirty, int[] pixels, Bitmap output, Color borderColour) {
                this.Dirty = dirty;
                this.Output = output;
                this.BorderColour = borderColour;
                this.Pixels = pixels;

            }

        }

        public delegate void BlankEventHandler(object sender, BlankEventArgs e);

        public event BlankEventHandler VerticalBlank;
        protected virtual void OnVerticalBlank(BlankEventArgs e) {
            if (VerticalBlank != null) VerticalBlank(this, e);
        }

    }
}
