using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using BeeDevelopment.Sega8Bit;
using SlimDX.DirectSound;

namespace BeeDevelopment.Cogwheel {
	public partial class MainForm {

		private void RepaintVideo(bool present) {
			var BackdropColour = Color.FromArgb(unchecked((int)0xFF000000 | Emulator.Video.LastBackdropColour));

			// Send the current frame to the renderer.
			var Video = this.Emulator.Video;
			this.Dumper.BackgroundColour = Color.FromArgb(Video.LastBackdropColour);
			if (Video.LastCompleteFrameWidth > 0 && Video.LastCompleteFrameHeight > 0) {
				var Eye = Video.LastOpenGlassesShutter == Emulator.GlassesShutter.Left ? PixelDumper3D.Eye.Left : PixelDumper3D.Eye.Right;
				this.Dumper.SetImage(Eye, Video.LastCompleteFrame, Video.LastCompleteFrameWidth, Video.LastCompleteFrameHeight);
				this.Dumper.SetBackdrop(Eye, Video.LastCompleteBackdrop, Video.LastCompleteFrameHeight);
			}

			// Work out how recently the eye changed.
			if (Video.LastOpenGlassesShutter != this.LastEye) {
				this.LastEye = Video.LastOpenGlassesShutter;
				this.FramesSinceEyeWasUpdated = 0;
			} else {
				if (this.IsLiveFrame) {
					++this.FramesSinceEyeWasUpdated;
					if (this.FramesSinceEyeWasUpdated > 100) this.FramesSinceEyeWasUpdated = 100;
					this.IsLiveFrame = false;
				}
			}

			if (this.FramesSinceEyeWasUpdated < 4 && this.Emulator.Family == HardwareFamily.MasterSystem && this.Emulator.Video.ResizingMode == BeeDevelopment.Sega8Bit.Hardware.VideoDisplayProcessor.ResizingModes.Normal) {
				// If we have received a change in eye recently, enable a 3D mode.
				if (this.Dumper.DisplayMode != this.ThreeDeeDisplayMode) this.Dumper.DisplayMode = this.ThreeDeeDisplayMode;
			} else {
				// If we haven't received a change in eye recently, disable the 3D mode.
				if (this.Dumper.DisplayMode != PixelDumper3D.StereoscopicDisplayMode.MostRecentEyeOnly) this.Dumper.DisplayMode = PixelDumper3D.StereoscopicDisplayMode.MostRecentEyeOnly;
			}

			// Repaint.
			if (present) {
				this.Dumper.Render();
				this.RenderPanel.BackColor = BackdropColour;
			}
		}

		[System.Security.SuppressUnmanagedCodeSecurity, DllImport("User32.dll", CharSet = CharSet.Auto)]
		public static extern bool PeekMessage(out Message msg, IntPtr hWnd, UInt32 msgFilterMin, UInt32 msgFilterMax, UInt32 flags);

		int RefreshStepper = 0;
		bool IsLiveFrame = false;

		Stopwatch FrameTime = new Stopwatch();

		void Application_Idle(object sender, EventArgs e) {
			int SystemRefreshRate = this.Dumper.CurrentRefreshRate;
			Message msg;
			while (!PeekMessage(out msg, IntPtr.Zero, 0, 0, 0)) {
				if (this.WindowState == FormWindowState.Minimized) {
					Thread.Sleep(100);
				} else {
					int FramesToRender = (int)(Math.Round(this.FrameTime.Elapsed.TotalSeconds * SystemRefreshRate));
					this.FrameTime.Reset();
					this.FrameTime.Start();
					if (FramesToRender < 1) {
						FramesToRender = 1;
					} else if (FramesToRender > 5) {
						FramesToRender = 5;
					}

					if (!this.Paused) {
						RefreshStepper -= this.Emulator.Video.FrameRate * FramesToRender;
						this.Input.Poll();
						this.Input.UpdateEmulatorState(this.Emulator);
						while (RefreshStepper <= 0) {
							RefreshStepper += SystemRefreshRate;
							this.Emulator.RunFrame();
							this.IsLiveFrame = true;
							if (!this.SoundMuted) {

								// How many frames of sound should we generate? Usually, one.
								int FramesOfSoundToGenerate = 1;

								// How many samples do we need to calculate per frame?
								int SamplesPerFrame = 44100 / this.Emulator.Video.FrameRate;

								// Offset (backwards) to align within the sound buffer when changing video refresh rates.
								this.SoundBufferPosition -= (this.SoundBufferPosition % (SamplesPerFrame * 2 * 2));

								// Determine how far the "write" buffer pointer is ahead of the "read" buffer pointer.
								int WriteAheadOfPlay = 0;
								lock (this.SoundBuffer) {
									WriteAheadOfPlay = this.SoundBufferPosition - this.SoundBuffer.CurrentPlayPosition;
								}
								while (WriteAheadOfPlay < 0) WriteAheadOfPlay += SoundBufferSize;

								if (SoundBufferCorrectionDirection == 0) {
									// If the write pointer is less than third of a buffer away, we're reading faster than writing.
									if ((WriteAheadOfPlay / 4) < (SoundBufferSampleCount / 3)) {
										Debug.WriteLine("Sound glitch (too slow) at " + DateTime.Now.TimeOfDay);
										SoundBufferCorrectionDirection = +1;
									}
									// If the write pointer is over two thirds of a buffer away, we're writing faster than reading.
									if ((WriteAheadOfPlay / 4) > ((SoundBufferSampleCount * 2) / 3)) {
										Debug.WriteLine("Sound glitch (too fast) at " + DateTime.Now.TimeOfDay);
										SoundBufferCorrectionDirection = -1;
									}
								} else if (SoundBufferCorrectionDirection > 0) {
									// We're speeding up:
									if ((WriteAheadOfPlay / 4) > (SoundBufferSampleCount / 2) - SamplesPerFrame) {
										SoundBufferCorrectionDirection = 0;
									}
								} else if (SoundBufferCorrectionDirection < 0) {
									// We're slowing down:
									if ((WriteAheadOfPlay / 4) < (SoundBufferSampleCount / 2) + SamplesPerFrame) {
										SoundBufferCorrectionDirection = 0;
									}
								}
								FramesOfSoundToGenerate += SoundBufferCorrectionDirection;



								// Generate samples as appropriate.
								if (FramesOfSoundToGenerate > 0) {
									short[] Buffer = new short[SamplesPerFrame * 2 * FramesOfSoundToGenerate];
									this.Emulator.Sound.CreateSamples(Buffer);
#if EMU2413
									if (this.Emulator.FmSoundEnabled) {
										var FmBuffer = new short[Buffer.Length];
										this.Emulator.FmSound.GenerateSamples(FmBuffer);
										for (int i = 0; i < FmBuffer.Length; ++i) {
											Buffer[i] = (short)(Buffer[i] + FmBuffer[i] * 2);
										}
									}
#endif
									// Convert 16-bit samples to 8-bit bytes of data.
									for (int i = 0; i < Buffer.Length; i += SamplesPerFrame * 2) {
										for (int j = 0; j < SamplesPerFrame * 2; ++j) {
											InternalSoundBuffer[this.SoundBufferPosition++] = (byte)Buffer[i + j];
											InternalSoundBuffer[this.SoundBufferPosition++] = (byte)(Buffer[i + j] >> 8);
										}
										if (this.SoundBufferPosition >= SoundBufferSize) this.SoundBufferPosition = 0;
									}
									lock (this.SoundBuffer) {
										this.SoundBuffer.Write(this.InternalSoundBuffer, 0, LockFlags.EntireBuffer);
									}
								}
							}
							this.RepaintVideo(false);
						}
						this.Dumper.Render();
					} else {
						this.RepaintVideo(true);
					}
				}
			}
		}
	}
}
