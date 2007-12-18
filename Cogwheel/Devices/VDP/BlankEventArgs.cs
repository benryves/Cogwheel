using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace BeeDevelopment.Cogwheel.Devices {

    public partial class VideoDisplayProcessor {

        public class ResolutionChangeEventArgs : EventArgs {

            public readonly Size Resolution;

            public ResolutionChangeEventArgs(Size resolution) {
                this.Resolution = resolution;
            }

        }

        public delegate void ResolutionChangeEventHandler(object sender, ResolutionChangeEventArgs e);

        public event ResolutionChangeEventHandler ResolutionChange;
        protected virtual void OnResolutionChange(ResolutionChangeEventArgs e) {
            if (ResolutionChange != null) ResolutionChange(this, e);
        }

    }
}
