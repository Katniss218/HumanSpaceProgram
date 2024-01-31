using System;
using UnityPlus.Serialization;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace KSS.Control.Controls
{
    public abstract class ControllerOutput : Control { }

    /// <summary>
    /// Represents a control that generates a control signal. <br/>
    /// Used to control something by connecting it to the appropriate <see cref="ControlleeInput{T}"/>.
    /// </summary>
    public sealed class ControllerOutput<T> : ControllerOutput, IPersistent
    {
        public ControlleeInput<T> Input { get; internal set; }

        public ControllerOutput()
        {
        }

        public void TrySendSignal( T signalValue )
        {
            Input?.onInvoke.Invoke( signalValue );
        }

        public override IEnumerable<Control> GetConnections()
        {
            yield return Input;
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

            if( this.Input != input )
                return false;

            Disconnect( input, this );
            return true;
        }

        public override bool TryDisconnectAll()
        {
            if( this.Input == null )
                return false;

            Disconnect( this.Input, this );
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
        internal static void Disconnect( ControlleeInput<T> input, ControllerOutput<T> output )
        {
            input.Output = null;
            output.Input = null;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static void Connect( ControlleeInput<T> input, ControllerOutput<T> output )
        {

        }
    }
}