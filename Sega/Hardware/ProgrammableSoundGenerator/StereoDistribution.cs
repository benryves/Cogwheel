using System;
namespace BeeDevelopment.Sega8Bit.Hardware {
	public partial class ProgrammableSoundGenerator {

		#region Internal State

		/// <summary>
		/// Stores the stereo distribution value.
		/// </summary>
		private byte StereoDistribution;

		[Flags()]
		private enum StereoDistributionChannels : byte {
			None = 0,
			Right0 = 0x01,
			Right1 = 0x02,
			Right2 = 0x04,
			Right3 = 0x08,
			Left0 = 0x10,
			Left1 = 0x20,
			Left2 = 0x40,
			Left3 = 0x80,
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Gets the state of a stereo distribution channel.
		/// </summary>
		/// <param name="channel">The channel to get the state of.</param>
		/// <returns>True if the channel is enabled; false if it is disabled.</returns>
		private bool GetStereoChannel(StereoDistributionChannels channel) {
			return (this.StereoDistribution & (byte)channel) != 0;
		}

		/// <summary>
		/// Sets the state of a stereo distribution channel.
		/// </summary>
		/// <param name="channel">The channel to set the state of.</param>
		/// <param name="value">True to enable the channel; false to disable it.</param>
		private void SetStereoChannel(StereoDistributionChannels channel, bool value) {
			if (value) {
				this.StereoDistribution |= (byte)channel;
			} else {
				this.StereoDistribution &= (byte)~channel;
			}
		}

		#endregion

		#region Public Properties

		/// <summary>Gets or sets the state of the right stereo distribution channel for tone generator 0.</summary>
		public bool StereoDistributionRight0 {
			get { return this.GetStereoChannel(StereoDistributionChannels.Right0); }
			set { this.SetStereoChannel(StereoDistributionChannels.Right0, value); }
		}

		/// <summary>Gets or sets the state of the right stereo distribution channel for tone generator 1.</summary>
		public bool StereoDistributionRight1 {
			get { return this.GetStereoChannel(StereoDistributionChannels.Right1); }
			set { this.SetStereoChannel(StereoDistributionChannels.Right1, value); }
		}

		/// <summary>Gets or sets the state of the right stereo distribution channel for tone generator 2.</summary>
		public bool StereoDistributionRight2 {
			get { return this.GetStereoChannel(StereoDistributionChannels.Right2); }
			set { this.SetStereoChannel(StereoDistributionChannels.Right2, value); }
		}

		/// <summary>Gets or sets the state of the right stereo distribution channel for tone generator 3.</summary>
		public bool StereoDistributionRight3 {
			get { return this.GetStereoChannel(StereoDistributionChannels.Right3); }
			set { this.SetStereoChannel(StereoDistributionChannels.Right3, value); }
		}

		/// <summary>Gets or sets the state of the left stereo distribution channel for tone generator 0.</summary>
		public bool StereoDistributionLeft0 {
			get { return this.GetStereoChannel(StereoDistributionChannels.Left0); }
			set { this.SetStereoChannel(StereoDistributionChannels.Left0, value); }
		}

		/// <summary>Gets or sets the state of the left stereo distribution channel for tone generator 1.</summary>
		public bool StereoDistributionLeft1 {
			get { return this.GetStereoChannel(StereoDistributionChannels.Left1); }
			set { this.SetStereoChannel(StereoDistributionChannels.Left1, value); }
		}

		/// <summary>Gets or sets the state of the left stereo distribution channel for tone generator 2.</summary>
		public bool StereoDistributionLeft2 {
			get { return this.GetStereoChannel(StereoDistributionChannels.Left2); }
			set { this.SetStereoChannel(StereoDistributionChannels.Left2, value); }
		}

		/// <summary>Gets or sets the state of the left stereo distribution channel for tone generator 3.</summary>
		public bool StereoDistributionLeft3 {
			get { return this.GetStereoChannel(StereoDistributionChannels.Left3); }
			set { this.SetStereoChannel(StereoDistributionChannels.Left3, value); }
		}

		#endregion

		#region Events

		/// <summary>
		/// An event that is triggered when a stereo distribution data has been written to the <see cref="ProgrammableSoundGenerator"/>.
		/// </summary>
		public event EventHandler<DataWrittenEventArgs> StereoDistributionDataWritten;
		
		/// <summary>
		/// An event that is triggered when a stereo distribution data has been written to the <see cref="ProgrammableSoundGenerator"/>.
		/// </summary>
		/// <param name="e">The data that was written.</param>
		protected virtual void OnStereoDistributionDataWritten(DataWrittenEventArgs e) {
			if (this.StereoDistributionDataWritten != null) {
				this.StereoDistributionDataWritten(this, e);
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Writes a control byte to the stereo distribution device.
		/// </summary>
		/// <param name="value">The value to write to the port.</param>
		public void WriteStereoDistributionImmediate(byte value) {
			this.StereoDistribution = value;
		}


		/// <summary>
		/// Queus a byte to later write to the stereo distribution device.
		/// </summary>
		/// <param name="value">The value to write to the port.</param>
		/// <remarks>The writes are committed by the <see cref="CreateSamples"/> method.</remarks>
		public void WriteStereoDistributionQueued(byte value) {
			this.OnStereoDistributionDataWritten(new DataWrittenEventArgs(value));
			this.QueuedWrites.Enqueue(new QueuedWrite(this) {
				Time = this.Emulator.ExpectedExecutedCycles,
				Value = value,
				Destination = WriteDestination.StereoDistribution,
			});
		}

		#endregion

	}
}
