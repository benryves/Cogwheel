using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using BeeDevelopment.Sega8Bit;
using FMOD;

namespace BeeDevelopment.SgcPlayer {
	public partial class MainInterface : Form {

		#region Emulation

		private SgcFile currentSong;
		private Emulator emulator;

		#endregion

		#region Low-Level Sound

		private FMOD.System fmodSystem;
		private FMOD.Sound fmodSound;
		private FMOD.Channel fmodChannel;
		private SOUND_PCMREADCALLBACK fmodReadPcmCallback;

		private void ThrowExceptionForFmodError(FMOD.RESULT error) {
			if (error != FMOD.RESULT.OK) {
				throw new Exception(FMOD.Error.String(error));
			}
		}

		private void InitialiseSound() {
			ThrowExceptionForFmodError(FMOD.Factory.System_Create(ref this.fmodSystem));
			this.fmodSystem.init(32, INITFLAGS.NORMAL, IntPtr.Zero);

			this.fmodReadPcmCallback = new SOUND_PCMREADCALLBACK(PCMREADCALLBACK);

			var CreateSoundExInfo = new FMOD.CREATESOUNDEXINFO();
			CreateSoundExInfo.cbsize = System.Runtime.InteropServices.Marshal.SizeOf(CreateSoundExInfo);
			CreateSoundExInfo.fileoffset = 0;
			CreateSoundExInfo.length = 44100;
			CreateSoundExInfo.numchannels = 2;
			CreateSoundExInfo.defaultfrequency = 44100;
			CreateSoundExInfo.format = SOUND_FORMAT.PCM16;
			CreateSoundExInfo.pcmreadcallback = this.fmodReadPcmCallback;
			CreateSoundExInfo.pcmsetposcallback = null;
			CreateSoundExInfo.dlsname = null;

			ThrowExceptionForFmodError(this.fmodSystem.createSound((string)null, FMOD.MODE._2D | FMOD.MODE.DEFAULT | FMOD.MODE.OPENUSER | FMOD.MODE.LOOP_NORMAL | FMOD.MODE.SOFTWARE | MODE.CREATESTREAM, ref CreateSoundExInfo, ref this.fmodSound));
			ThrowExceptionForFmodError(this.fmodSystem.playSound(CHANNELINDEX.REUSE, this.fmodSound, false, ref this.fmodChannel));
		}

		private void DisposeSound() {
			if (this.fmodChannel != null) {
				this.fmodChannel.stop();
				this.fmodChannel = null;
			}
			if (this.fmodSound != null) {
				this.fmodSound.release();
				this.fmodSound = null;
			}
			if (this.fmodSystem != null) {
				this.fmodSystem.release();
				this.fmodSystem = null;
			}
			if (this.fmodReadPcmCallback != null) {
				this.fmodReadPcmCallback = null;
			}
		}
	
		#endregion

		#region Sound Renderer

		int sampleCounter;

		FMOD.RESULT PCMREADCALLBACK(IntPtr SoundRaw, IntPtr Data, uint DataLen) {
			short[] samples = new short[DataLen / 2];
			sampleCounter += (int)DataLen / 4;
			var emulator = this.emulator;
			if (emulator != null) {
				while (sampleCounter > 0) {
					this.currentSong.RunFrame(emulator);
					sampleCounter -= 44100 / emulator.Video.FrameRate;
				}
				emulator.Sound.CreateSamples(samples);

				// Do we need to generate FM sound?
				if (emulator.FmSoundEnabled) {
					var fmSamples = new short[samples.Length];
					emulator.FmSound.GenerateSamples(fmSamples);
					for (int i = 0; i < fmSamples.Length; ++i) {
						samples[i] = (short)(samples[i] + fmSamples[i] * 2);
					}
				}

			}
			Marshal.Copy(samples, 0, Data, samples.Length);
			return RESULT.OK;
		}

		#endregion

		#region Loading Files

		private void LoadFile(string filename) {
			try {
				this.currentSong = SgcFile.FromFile(filename);
			} catch (Exception ex) {
				MessageBox.Show(this, "Could not load file: " + ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}


			// Unload the current emulator.
			this.emulator = null;

			// Update the track bar.
			this.SongSelection.Enabled = false;
			this.SongSelection.Value = 0;
			this.SongSelection.Maximum = this.currentSong.SongCount;
			this.SongSelection.Value = this.currentSong.FirstSongIndex;
			this.SongSelection.Enabled = true;

			// Create a new emulator instance.
			this.emulator = this.currentSong.CreateEmulator();

			// Display metadata.
			this.SongNameLabel.Text = this.currentSong.SongName;
			this.AuthorLabel.Text = this.currentSong.Author;
			this.CopyrightLabel.Text = this.currentSong.Copyright;
			this.PlaySong(this.currentSong.FirstSongIndex);

			// Re-enable the controls.
			this.ControlButtonContainer.Enabled = true;
		}

		#endregion

		#region Drag-and-Drop

		private void MainInterface_DragDrop(object sender, DragEventArgs e) {
			string[] DropData;
			if (e.Data.GetDataPresent(DataFormats.FileDrop) && (DropData = (e.Data.GetData(DataFormats.FileDrop) as string[])) != null && DropData.Length == 1) {
				this.LoadFile(DropData[0]);
			}
		}

		private void MainInterface_DragOver(object sender, DragEventArgs e) {
			string[] DropData;
			if (e.Data.GetDataPresent(DataFormats.FileDrop) && (DropData = (e.Data.GetData(DataFormats.FileDrop) as string[])) != null && DropData.Length == 1) {
				e.Effect = DragDropEffects.Copy;
			}

		}

		#endregion

		#region Controlling Songs

		private void PlaySong(byte index) {
			if (SongSelection.Enabled && this.currentSong != null && this.emulator != null) {
				var newEmulator = this.currentSong.CreateEmulator();
				this.currentSong.LoadSong(newEmulator, index);
				this.emulator = newEmulator;
				sampleCounter = -44100 / 4;
				this.Play();
			}
		}

		private void SongSelection_ValueChanged(object sender, EventArgs e) {
			if (!SongSelectionMouseDown) {
				this.PlaySong((byte)this.SongSelection.Value);
			}
		}

		bool SongSelectionMouseDown = false;
		private void SongSelection_MouseDown(object sender, MouseEventArgs e) {
			SongSelectionMouseDown = true;
		}

		private void SongSelection_MouseUp(object sender, MouseEventArgs e) {
			SongSelectionMouseDown = false;
			SongSelection_ValueChanged(sender, new EventArgs());
		}

		private void ButtonNext_Click(object sender, EventArgs e) {
			if (this.SongSelection.Enabled && this.SongSelection.Value != this.SongSelection.Maximum) {
				++this.SongSelection.Value;
			}
		}

		private void ButtonPrevious_Click(object sender, EventArgs e) {
			if (this.SongSelection.Enabled && this.SongSelection.Value != this.SongSelection.Minimum) {
				--this.SongSelection.Value;
			}
		}

		private void TogglePlayPause() {
			bool isPaused = false;
			if (this.fmodChannel != null) this.fmodChannel.getPaused(ref isPaused);
			if (isPaused) {
				this.Play();
			} else {
				this.Pause();
			}
		}

		private void Play() {
			if (this.fmodChannel != null) this.fmodChannel.setPaused(false);
			this.ButtonPlayPause.Image = Properties.Resources.control_play;
		}
		
		private void Pause() {
			if (this.fmodChannel != null) this.fmodChannel.setPaused(true);
			this.ButtonPlayPause.Image = Properties.Resources.control_pause;
		}

		private void ButtonPlayPause_Click(object sender, EventArgs e) {
			this.TogglePlayPause();
		}

		#endregion

		#region File Menu

		private void FileExitMenu_Click(object sender, EventArgs e) {
			this.Close();
		}

		private void FileOpenMenu_Click(object sender, EventArgs e) {
			if (this.OpenSgcFileDialog.ShowDialog(this) == DialogResult.OK) {
				this.LoadFile(this.OpenSgcFileDialog.FileName);
			}
		}

		#endregion

		public MainInterface(string initialFilename) {
			InitializeComponent();
			this.Text = Application.ProductName;
			this.InitialiseSound();
			this.Disposed += new EventHandler(MainInterface_Disposed);
			if (initialFilename != null) this.LoadFile(initialFilename);
		}

		void MainInterface_Disposed(object sender, EventArgs e) {
			this.DisposeSound();
		}

	}
}
