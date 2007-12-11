using System;
using System.Collections.Generic;
using System.Text;

namespace Cogwheel.Devices {
    public partial class FDC {

        public const int DiskSizeBytes = 160 * 1024;
        public const int SectorSizeBytes = 0x100;
        public const int ClusterSizeBytes = Floppy.SectorSizeBytes * 4;
        public const int TrackSizeBytes = Floppy.ClusterSizeBytes * 4;

        /// <summary>Is true when the motor is on.</summary>
        public bool MotorOn {
            get { return this.motorOn; }
            set { this.MotorOn = value; }
        }
        private bool motorOn = false;

        public readonly Drive[] Drives = new Drive[] { new Drive(0), new Drive(1), new Drive(2), new Drive(3) };

    }
}
