using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BeeDevelopment.Sega8Bit;
using BeeDevelopment.Sega8Bit.Utility;
using InteractSw.GenerateBitmap;

namespace BeeDevelopment.Cogwheel.Silverlight {
	public partial class Page : UserControl {

		private Emulator Emulator = null;

		private PngGenerator ScreenGenerator = null;
		private BitmapImage ScreenImage = null;

		private int ScreenWidth = -1;
		private int ScreenHeight = -1;

		private int BackgroundColour = -1;

		private bool paused;
		public bool Paused {
			get { return this.paused; }
			set {
				this.paused = value;
				this.PauseButton.Source = new BitmapImage(new Uri(string.Format("Resources/control_{0}_blue.png", this.paused ? "play" : "pause"), UriKind.Relative));
			}
		}
		private bool ForcePaused = false;

		private DateTime LastFrame;

		private int FrameSkip = 0;

		public Page(IDictionary<string,string> initParams) {
			InitializeComponent();
	
			this.Emulator = new Emulator();
			this.ScreenImage = new BitmapImage();
			this.LastFrame = DateTime.Now;

			if (initParams.ContainsKey("rom")) {
				this.LoadRomFromUri(new Uri(initParams["rom"], UriKind.Relative));
			}

		}

		private void LoadRom(string name, byte[] data) {
			HardwareModel Model = HardwareModel.MasterSystem2;
			switch (Path.GetExtension(name).ToLowerInvariant()) {
				case ".gg":
					Model = HardwareModel.GameGear;
					break;
			}
			this.Emulator.RemoveAllMedia();
			this.Emulator.ResetAll();
			this.Emulator.SetCapabilitiesByModelAndRegion(Model, Region.Export);
			this.Emulator.CartridgeSlot.Memory = new RomIdentifier().CreateMapper(data);
			this.Emulator.Bios.Enabled = false;
			this.Emulator.CartridgeSlot.Enabled = true;
			this.Emulator.ExpansionSlot.Enabled = false;
		}

		private void LoadRomFromUri(Uri file) {
			var WC = new WebClient();
			WC.OpenReadCompleted += (sender, e) => {
				if (e.Error != null) {
					MessageBox.Show("Error loading ROM: " + e.Error.Message);
					return;
				}
				this.ForcePaused = true;
				var Bytes = new byte[e.Result.Length];
				e.Result.Read(Bytes, 0, Bytes.Length);
				this.LoadRom(Path.GetFileName(file.OriginalString), Bytes);
				e.Result.Dispose();
				this.ForcePaused = false;
			};
			WC.OpenReadAsync(file);
		}

		private void SetKeyState(KeyEventArgs e, bool state) {
			bool Handled = true;
			switch (e.Key) {
				case Key.Up:
					this.Emulator.SegaPorts[0].Up.State = !state;
					break;
				case Key.Down:
					this.Emulator.SegaPorts[0].Down.State = !state;
					break;
				case Key.Left:
					this.Emulator.SegaPorts[0].Left.State = !state;
					break;
				case Key.Right:
					this.Emulator.SegaPorts[0].Right.State = !state;
					break;
				case Key.Ctrl:
				case Key.Z:
					this.Emulator.SegaPorts[0].TL.State = !state;
					break;
				case Key.Shift:
				case Key.Alt:
				case Key.X:
					this.Emulator.SegaPorts[0].TR.State = !state;
					break;
				case Key.Space:
				case Key.Enter:
					if (this.Emulator.HasPauseButton) this.Emulator.PauseButton=state;
					if (this.Emulator.HasStartButton) this.Emulator.StartButton = state;
					break;
				default:
					Handled = false;
					break;
			}
			e.Handled |= Handled;
		}

		private void OutputImage_KeyDown(object sender, KeyEventArgs e) {
			SetKeyState(e, true);
		}

		private void OutputImage_KeyUp(object sender, KeyEventArgs e) {
			SetKeyState(e, false);
		}

		private void Button_KeyDown(object sender, KeyEventArgs e) {
			e.Handled = false;
		}

		private void OpenLocalRom_Click(object sender, MouseButtonEventArgs e) {
			this.ForcePaused = true;
			var OpenRom = new OpenFileDialog();
			OpenRom.Filter = "Sega Master System / Game Gear ROMs (*.sms;*.gg)|*.sms;*.gg";
			if (OpenRom.ShowDialog() ?? false) {
				
				using (var s = OpenRom.File.OpenRead()) {
					var Bytes = new byte[s.Length];
					if (s.Read(Bytes, 0, Bytes.Length) != Bytes.Length) {
						MessageBox.Show("ROM not fully read.");
					} else {
						this.LoadRom(OpenRom.File.Name, Bytes);
					}
				}				
			}
			this.ForcePaused = false;
		}

		private void Pause_Click(object sender, MouseButtonEventArgs e) {
			this.Paused ^= true;
		}

		private void ControlBar_MouseEnter(object sender, MouseEventArgs e) {
			ControlBar.Opacity = 1.0d;
		}

		private void ControlBar_MouseLeave(object sender, MouseEventArgs e) {
			ControlBar.Opacity = 0.25d;
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e) {
			this.UserControlRoot.AttachRenderLoop(() => {

				var ThisFrame = DateTime.Now;

				if (ForcePaused || Paused) {
					LastFrame = ThisFrame;
					return;
				}
				try {
					var TimeDelta = ThisFrame - LastFrame;

					int FramesToRun = (int)Math.Floor(TimeDelta.TotalSeconds * 60d);

					if (FramesToRun < FrameSkip) return;

					for (int i = 0; i < FramesToRun; i++) {
						this.Emulator.RunFrame();
					}
					LastFrame += TimeSpan.FromSeconds(FramesToRun / 60d);

					int[] FramePixels = this.Emulator.Video.LastCompleteFrame;
					int FrameWidth = this.Emulator.Video.LastCompleteFrameWidth;
					int FrameHeight = this.Emulator.Video.LastCompleteFrameHeight;
					int FrameBackground = this.Emulator.Video.LastBackdropColour;
					if (this.ScreenWidth != FrameWidth || this.ScreenHeight != FrameHeight || this.ScreenGenerator == null) {
						this.ScreenGenerator = new PngGenerator(FrameWidth, FrameHeight);
						this.ScreenWidth = FrameWidth;
						this.ScreenHeight = FrameHeight;
						this.OutputImage.Source = this.ScreenImage;
					}
					Color[] FrameColours = new Color[FramePixels.Length];
					for (int i = 0; i < FramePixels.Length; ++i) {
						int c = FramePixels[i];
						FrameColours[i] = Color.FromArgb(0xFF, (byte)(c >> 16), (byte)(c >> 8), (byte)c);
					}
					this.ScreenGenerator.SetPixelColorData(FrameColours);
					this.ScreenImage.SetSource(ScreenGenerator.CreateStream());
					if (this.BackgroundColour != FrameBackground) {
						this.BackgroundColour = FrameBackground;
						LayoutRoot.Background = new SolidColorBrush(Color.FromArgb(0xFF, (byte)(FrameBackground >> 16), (byte)(FrameBackground >> 8), (byte)FrameBackground));
					}
				} catch {
					LastFrame = ThisFrame;
				}
			});
		}
	}
}
