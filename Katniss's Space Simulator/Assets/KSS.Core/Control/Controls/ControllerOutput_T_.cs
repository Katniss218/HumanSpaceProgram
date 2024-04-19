using System;
using UnityPlus.Serialization;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;

namespace KSS.Control.Controls
{
    /// <summary>
    /// Represents a control that produces a control signal of type <typeparamref name="T"/>.
    /// </summary>
    public sealed class ControllerOutput<T> : ControllerOutputBase
    {
        public ControlleeInput<T> Input { get; internal set; }

        public ControllerOutput()
        {
        }

        /// <summary>
        /// Tries to send a control signal from this controller to its connected inputs.
        /// </summary>
        /// <param name="signalValue">The value of the signal that will be passed to the connected inputs.</param>
        public void TrySendSignal( T signalValue )
        {
            Input?.onInvoke.Invoke( signalValue );
        }

        public override IEnumerable<Control> GetConnectedControls()
        {
            if( Input == null )
            {
                return Enumerable.Empty<Control>();
            }
            return new[] { Input };
        }

        public override bool TryConnect( Control other )
        {
            if( other is not ControlleeInput<T> input )
                return false;

            ControlleeInput<T>.Connect( input, this );
            return true;
        }

        public override bool TryDisconnect( Control other )
        {
            if( other is not ControlleeInput<T> input )
                return false;

            if( this.Input != input )
                return false;

            ControlleeInput<T>.Disconnect( this );
            return true;
        }

        public override bool TryDisconnectAll()
        {
            if( this.Input == null )
                return false;

            ControlleeInput<T>.Disconnect( this );
            return true;
        }
    }
}