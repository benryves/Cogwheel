using System;
using System.Collections.Generic;
using System.Text;

namespace Cogwheel.Devices {
    public partial class Floppy {

        public const int SectorSizeBytes = 0x100;
        public const int ClusterSizeBytes = Floppy.SectorSizeBytes * 4;
        public const int TrackSizeBytes = Floppy.ClusterSizeBytes * 4;

        private const int FatIndex = 20 * Floppy.TrackSizeBytes + 12 * Floppy.SectorSizeBytes;


        public struct Sector {
            public byte[] Data;
        }

        public Sector GetSector(int track, int cluster) {
            CheckTrackBounds(track);
            CheckClusterBounds(cluster);
            Sector Result = new Sector();
            Result.Data = new byte[Floppy.SectorSizeBytes];
            Array.ConstrainedCopy(this.data, track * Floppy.TrackSizeBytes + cluster * Floppy.ClusterSizeBytes, Result.Data, 0, Floppy.SectorSizeBytes);
            return Result;
        }


        private void CheckTrackBounds(int track) {
            if (track < 0 || track > 40) throw new IndexOutOfRangeException("Track number out of range.");
        }

        private void CheckClusterBounds(int cluster) {
            if (cluster < 0 || cluster > 16) throw new IndexOutOfRangeException("Cluster number out of range.");
        }

    }
}
