using BeeDevelopment.Brazil;
using System;

namespace BeeDevelopment.Sega8Bit {


	/// <summary>
	/// Emulates a controller port.
	/// </summary>
	public class ControllerPort  {



		/// <summary>
		/// Represents an input-only pin on the controller port.
		/// </summary>
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
		public class BidirectionalPin : UnidirectionalPin {


			/// <summary>
			/// Gets or sets the <see cref="ControllerPort"/> that this pin is part of.
			/// </summary>
			public ControllerPort Port { get; private set; }

			/// <summary>
			/// Creates an instance of a <see cref="BidirectionalPin"/>.
			/// </summary>
			public BidirectionalPin(ControllerPort port) {
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
			/// Gets or sets the state of the pin as an input.
			/// </summary>
			public bool OutputState { get; set; }

			/// <summary>
			/// Gets or sets the state of the <see cref="BidirectionalPin"/>.
			/// </summary>
			/// <remarks>When setting the state, this modifies both <see cref="InputState"/> and <see cref="OutputState"/> properties.</remarks>
			public override bool State {
				get {
					if (this.Direction == PinDirection.Input) {
						return this.InputState;
					} else {
						return this.Port.Region == Region.Japanese ? false : this.OutputState;
					}					
				}
				set {
					this.InputState = value;
					this.OutputState = value;
				}
			}
			
		}

		/// <summary>
		/// Gets or sets the region of the controller port.
		/// </summary>
		public Region Region { get; set; }

		/// <summary>
		/// Gets or sets the state of the Up pin.
		/// </summary>
		public UnidirectionalPin Up { get; private set; }

		/// <summary>
		/// Gets or sets the state of the Down pin.
		/// </summary>
		public UnidirectionalPin Down { get; private set; }

		/// <summary>
		/// Gets or sets the state of the Left pin.
		/// </summary>
		public UnidirectionalPin Left { get; private set; }


		/// <summary>
		/// Gets or sets the state of the Right pin.
		/// </summary>
		public UnidirectionalPin Right { get; private set; }

		/// <summary>
		/// Gets or sets the state of the TL pin.
		/// </summary>
		public UnidirectionalPin TL { get; private set; }

		/// <summary>
		/// Gets the state of the TR pin.
		/// </summary>
		public BidirectionalPin TR { get; private set; }

		/// <summary>
		/// Gets the state of the TH pin.
		/// </summary>
		public BidirectionalPin TH { get; private set; }

		/// <summary>
		/// Creates an instance of a <see cref="ControllerPort"/>.
		/// </summary>
		public ControllerPort() {
			this.Up = new UnidirectionalPin();
			this.Down = new UnidirectionalPin();
			this.Left = new UnidirectionalPin();
			this.Right = new UnidirectionalPin();
			this.TL = new UnidirectionalPin();
			this.TR = new BidirectionalPin(this);
			this.TH = new BidirectionalPin(this);
		}


		internal void WriteState(int value) {
			if (this.Region == Region.Export) {
				this.TR.Direction = ((value & 0x01) != 0) ? PinDirection.Input : PinDirection.Output;
				this.TH.Direction = ((value & 0x02) != 0) ? PinDirection.Input : PinDirection.Output;
				this.TR.OutputState = ((value & 0x10) != 0);
				this.TH.OutputState = ((value & 0x20) != 0);
			}
		}

	}
}