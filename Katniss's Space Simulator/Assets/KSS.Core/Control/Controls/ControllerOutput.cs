using System;
using UnityPlus.Serialization;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;

namespace KSS.Control.Controls
{
	/// <summary>
	/// Represents a control that produces a control signal.
	/// </summary>
	public abstract class ControllerOutput : Control { }

	/// <summary>
	/// Represents a control that produces a control signal of type <typeparamref name="T"/>.
	/// </summary>
	public sealed class ControllerOutput<T> : ControllerOutput, IPersistent
	{
		internal List<ControlleeInput<T>> inputs = new();
		public IEnumerable<ControlleeInput<T>> Inputs { get => inputs; }

		public ControllerOutput()
		{
		}

		/// <summary>
		/// Tries to send a control signal from this controller to its connected inputs.
		/// </summary>
		/// <param name="signalValue">The value of the signal that will be passed to the connected inputs.</param>
		public void TrySendSignal( T signalValue )
		{
			foreach( var connection in inputs )
			{
				connection.onInvoke.Invoke( signalValue );
			}
		}

		public override IEnumerable<Control> GetConnectedControls()
		{
			return inputs;
		}

		public override bool TryConnect( Control other )
		{
			if( other is not ControlleeInput<T> input )
				return false;

			Connect( input, this );
			return true;
		}

		public override bool TryDisconnect( Control other )
		{
			if( other is not ControlleeInput<T> input )
				return false;

			if( !this.inputs.Contains( input ) )
				return false;

			Disconnect( input );
			return true;
		}

		public override bool TryDisconnectAll()
		{
			if( this.inputs == null )
				return false;

			foreach( var connection in inputs.ToArray() )
			{
				Disconnect( connection );
			}
			return true;
		}

		public SerializedData GetData( IReverseReferenceMap s )
		{
			// save what it is connected to.
			throw new NotImplementedException();
		}

		public void SetData( IForwardReferenceMap l, SerializedData data )
		{
			// load what it is connected to.
			throw new NotImplementedException();
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		internal static void Disconnect( ControlleeInput<T> input )
		{
			input.Output?.inputs.Remove( input );
			input.Output = null;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		internal static void Connect( ControlleeInput<T> input, ControllerOutput<T> output )
		{
			// disconnect from previous, if connected.
			Disconnect( input );

			input.Output = output;
			output.inputs.Add( input );
		}
	}
}