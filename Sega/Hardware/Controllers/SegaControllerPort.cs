using System;
using BeeDevelopment.Brazil;

namespace BeeDevelopment.Sega8Bit.Hardware.Controllers {


	/// <summary>
	/// Emulates a Sega controller port.
	/// </summary>
	[Serializable()]
	public class SegaControllerPort  {

		/// <summary>
		/// Represents an input-only pin on the controller port.
		/// </summary>
		[Serializable()]
		public class UnidirectionalPin {

			/// <summary>
			/// Gets or sets the state of the pin.
			/// </summary>
			public virtual bool State { get; set; }


			/// <summary>
			/// Creates an instance of a <see cref="UnidirectionalPin"/>.
			/// </summary>
			public UnidirectionalPin() {
				this.State = true;
			}
		}


		/// <summary>
		/// Represents a bidirectional pin pin on the controller port.
		/// </summary>
		[Serializable()]
		public class BidirectionalPin : UnidirectionalPin {


			/// <summary>
			/// Gets or sets the <see cref="ControllerPort"/> that this pin is part of.
			/// </summary>
			public SegaControllerPort Port { get; private set; }

			/// <summary>
			/// Creates an instance of a <see cref="BidirectionalPin"/>.
			/// </summary>
			public BidirectionalPin(SegaControllerPort port) {
				this.Port = port;
				this.InputState = true;
				this.OutputState = false;
				this.Direction = PinDirection.Input;
			}

			/// <summary>
			/// Gets or sets the data direction of the pin.
			/// </summary>
			public PinDirection Direction { get; set; }

			/// <summary>
			/// Gets or sets the state of the pin as an input.
			/// </summary>
			public bool InputState { get; set; }

			/// <summary>
			/// Gets or sets the state of the pin as an output.
			/// </summary>
			public bool OutputState { get; set; }

			/// <summary>
			/// Gets or sets the state of the <see cref="BidirectionalPin"/>.
			/// </summary>
			/// <remarks>When setting the state, this modifies both <see cref="InputState"/> and <see cref="OutputState"/> properties.</remarks>
			public override bool State {
				get {
					if (this.Direction == PinDirection.Output && this.Port.SupportsOutput) {
						return this.OutputState;
					} else {
						return this.InputState;
					}
				}
				set {
					this.InputState = value;
					this.OutputState = value;
				}
			}
			
		}

		/// <summary>
		/// Gets or sets whether the controller port can output data as well as input data on its bidirectional pins.
		/// </summary>
		public bool SupportsOutput { get; set; }

		/// <summary>
		/// Gets or sets the state of the Up pin.
		/// </summary>
		[StateNotSaved()]
		public UnidirectionalPin Up { get; private set; }

		/// <summary>
		/// Gets or sets the state of the Down pin.
		/// </summary>
		[StateNotSaved()]
		public UnidirectionalPin Down { get; private set; }

		/// <summary>
		/// Gets or sets the state of the Left pin.
		/// </summary>
		[StateNotSaved()]
		public UnidirectionalPin Left { get; private set; }


		/// <summary>
		/// Gets or sets the state of the Right pin.
		/// </summary>
		[StateNotSaved()]
		public UnidirectionalPin Right { get; private set; }

		/// <summary>
		/// Gets or sets the state of the TL pin.
		/// </summary>
		[StateNotSaved()]
		public UnidirectionalPin TL { get; private set; }

		/// <summary>
		/// Gets the state of the TR pin.
		/// </summary>
		[StateNotSaved()]
		public BidirectionalPin TR { get; private set; }

		/// <summary>
		/// Gets the state of the TH pin.
		/// </summary>
		[StateNotSaved()]
		public BidirectionalPin TH { get; private set; }

		/// <summary>
		/// Creates an instance of a <see cref="ControllerPort"/>.
		/// </summary>
		public SegaControllerPort() {
			this.Up = new UnidirectionalPin();
			this.Down = new UnidirectionalPin();
			this.Left = new UnidirectionalPin();
			this.Right = new UnidirectionalPin();
			this.TL = new UnidirectionalPin();
			this.TR = new BidirectionalPin(this);
			this.TH = new BidirectionalPin(this);
		}


		internal void WriteState(int value) {
			this.TR.Direction = ((value & 0x01) != 0) ? PinDirection.Input : PinDirection.Output;
			this.TH.Direction = ((value & 0x02) != 0) ? PinDirection.Input : PinDirection.Output;
			this.TR.OutputState = ((value & 0x10) != 0);
			this.TH.OutputState = ((value & 0x20) != 0);
		}

	}
}