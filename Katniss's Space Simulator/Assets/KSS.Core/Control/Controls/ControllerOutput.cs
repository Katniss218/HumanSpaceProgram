using System;
using UnityPlus.Serialization;
using System.Runtime.CompilerServices;

namespace KSS.Control.Controls
{
    /// <summary>
    /// Represents a control that generates a control signal. <br/>
    /// Used to control something by connecting it to the appropriate <see cref="ControlleeInput{T}"/>.
    /// </summary>
    public sealed class ControllerOutput<T> : Control, IPersistent
    {
        public ControlleeInput<T> Input { get; internal set; }

        public ControllerOutput()
        {
        }

        public void TrySendSignal( T signalValue )
        {
            Input?.onInvoke.Invoke( signalValue );
        }

        public override void Disconnect()
        {
            Disconnect( this.Input, this );
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