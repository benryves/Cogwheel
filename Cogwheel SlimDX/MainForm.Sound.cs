using SlimDX;
using SlimDX.DirectSound;
using SlimDX.Multimedia;

namespace BeeDevelopment.Cogwheel {
	public partial class MainForm {

		private SecondarySoundBuffer SoundBuffer;

		private const int SoundBufferSampleCount = 4410 * 2;
		private const int SoundBufferSize = SoundBufferSampleCount * 2 * 2;
		private int SoundBufferPosition = (SoundBufferSampleCount / 2) * 2 * 2;
		private int SoundBufferCorrectionDirection = 0;

		private byte[] InternalSoundBuffer;

		private void InitialiseSound() {

			Program.DS.SetCooperativeLevel(this.Handle, CooperativeLevel.Priority);

			var Format = new WaveFormat() {
				Channels = 2,
				BitsPerSample = 16,
				FormatTag = WaveFormatTag.Pcm,
				SamplesPerSecond = 44100,
				BlockAlignment = 4,
				AverageBytesPerSecond = 44100 * 2 * 2
			};

			var Description = new SoundBufferDescription() {
				Format = Format,
				Flags = BufferFlags.GlobalFocus | BufferFlags.Software,
				SizeInBytes = SoundBufferSize,
			};

			this.SoundBuffer = new SecondarySoundBuffer(Program.DS, Description);
			this.InternalSoundBuffer = new byte[SoundBufferSize];

			if (!this.SoundMuted) this.StartPlayingSound();

		}

		private void StartPlayingSound() {
			this.InternalSoundBuffer = new byte[this.InternalSoundBuffer.Length];
			this.SoundBufferPosition = 0;
			this.SoundBuffer.CurrentPlayPosition = 0;
			this.SoundBuffer.Play(0, PlayFlags.Looping);
		}

		private void DisposeSound() {
			if (this.SoundBuffer != null && !this.SoundBuffer.Disposed) {
				this.SoundBuffer.Dispose();
				this.SoundBuffer = null;
			}
		}

		private bool SoundMuted = false;

	}
}
