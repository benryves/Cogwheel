using BeeDevelopment.Brazil;

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
		}


		/// <summary>
		/// Represents a bidirectional pin pin on the controller port.
		/// </summary>
		public class BidirectionalPin : UnidirectionalPin {


			/// <summary>
			/// Gets or sets the <see cref="ControllerPort"/> that this pin is part of.
			/// </summary>
			public ControllerPort Port { get; private set; }

			public BidirectionalPin(ControllerPort port) {
				this.Port = port;
			}

			/// <summary>
			/// Gets or sets the data direction of the pin.
			/// </summary>
			public PinDirection Direction { get; set; }

			public override bool State {
				get {
					if (this.Direction == PinDirection.Input) {
						return base.State;
					} else {
						return this.Port.Region == Region.Japanese ? false : base.State;
					}					
				}
				set { base.State = value; }
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
			this.Up = new UnidirectionalPin() { State = true };
			this.Down = new UnidirectionalPin() { State = true };
			this.Left = new UnidirectionalPin() { State = true };
			this.Right = new UnidirectionalPin() { State = true };
			this.TL = new UnidirectionalPin() { State = true };
			this.TR = new BidirectionalPin(this) { State = true };
			this.TH = new BidirectionalPin(this) { State = true };
		}


		internal void WriteState(int value) {
			this.TR.Direction = ((value & 0x01) != 0) ? PinDirection.Input : PinDirection.Output;
			this.TH.Direction = ((value & 0x02) != 0) ? PinDirection.Input : PinDirection.Output;
			this.TR.State = ((value & 0x10) != 0);
			this.TH.State = ((value & 0x20) != 0);
		}

	}
}