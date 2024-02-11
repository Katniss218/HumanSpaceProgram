using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSS.Control.Controls
{
	/// <summary>
	/// Represents a control that consumes a control signal.
	/// </summary>
	public abstract class ControlleeInput : Control { }

	/// <summary>
	/// Represents a control that consumes a control signal of type <typeparamref name="T"/>.
	/// </summary>
	public sealed class ControlleeInput<T> : ControlleeInput
	{
		internal Action<T> onInvoke;

		public ControllerOutput<T> Output { get; internal set; }

		/// <param name="signalResponse">The action to perform when a control signal is sent to this input.</param>
		public ControlleeInput( Action<T> signalResponse )
		{
			this.onInvoke = signalResponse;
		}

		public override IEnumerable<Control> GetConnectedControls()
		{
			if( Output == null )
			{
				return Enumerable.Empty<Control>();
			}
			return new[] { Output };
		}

		public override bool TryConnect( Control other )
		{
			if( other is not ControllerOutput<T> output )
				return false;

			ControllerOutput<T>.Connect( this, output );
			return true;
		}

		public override bool TryDisconnect( Control other )
		{
			if( other is not ControllerOutput<T> output )
				return false;

			if( this.Output != output )
				return false;

			ControllerOutput<T>.Disconnect( this );
			return true;
		}

		public override bool TryDisconnectAll()
		{
			if( this.Output == null )
				return false;

			ControllerOutput<T>.Disconnect( this );
			return true;
		}
	}
}