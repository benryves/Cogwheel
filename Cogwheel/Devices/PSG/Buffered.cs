using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;

namespace BeeDevelopment.Cogwheel.Devices {
    public partial class ProgrammableSoundGenerator {

        private enum WriteDestination {
            Psg, Stereo,
        }

        private struct BufferedWrite {
            public WriteDestination Destination;
            public byte Value;
            public int Time;
            public BufferedWrite(WriteDestination destination, byte value, int time) {
                this.Destination = destination;
                this.Value = value;
                this.Time = time;
            }
        }

        private Queue<short> GeneratedSamples = new Queue<short>();

        /// <summary>Start buffering PSG writes.</summary>
        /// <param name="time">The time that buffering started.</param>
        public void BufferedStart(int time) {
            //this.BufferStart = time;
        }

        private Queue<BufferedWrite> BufferedWrites = new Queue<BufferedWrite>();

        public void BufferedWriteByteToPsg(int time, byte value) {
            this.BufferedWrites.Enqueue(new BufferedWrite(WriteDestination.Psg, value, time));
        }

        public void BufferedWriteByteToStereo(int time, byte value) {
            this.BufferedWrites.Enqueue(new BufferedWrite(WriteDestination.Stereo, value, time));
        }

        int lastCpu = 0;
        int totalSamples = 0;

        public short[] BufferedEnd(int time, int samplesRequired) {

            totalSamples += samplesRequired;

            lastCpu = time;
            
            short[] returnValue = new short[samplesRequired * 2];

            for (int i = 0; i < samplesRequired; ++i) {


                while (BufferedWrites.Count > 0) {// && BufferedWrites.Peek().Time <= SampleTime) {

                    switch (BufferedWrites.Peek().Destination) {
                        case WriteDestination.Psg:
                            this.WriteByteToPsg(BufferedWrites.Dequeue().Value);
                            break;
                        case WriteDestination.Stereo:
                            this.WriteByteToStereo(BufferedWrites.Dequeue().Value);
                            break;
                    }
                }

                for (int j = 0; j < 5; ++j) {
                    this.PsgTick();
                }

                double L = 0;
                for (int c = 0; c < 4; ++c) {
                    L += PsgLogScale[VolumeLevels[c]] * (double)(PsgStatus[c]);
                }
                returnValue[i * 2 + 0] = (short)(L * 2000d);
                returnValue[i * 2 + 1] = returnValue[i * 2 + 0];
                
            }
            

            return returnValue;
        }

        private double[] PsgLogScale = { 1.0d, 0.794335765d, 0.630970183d, 0.501174963d, 0.398113956d, 0.316232795d, 0.251197851d, 0.20044557d, 0.15848262d, 0.125888852d, 0.100009156d, 0.07943968d, 0.063081759d, 0.050111393d, 0.039796136d, 0.0d };
    }
}
