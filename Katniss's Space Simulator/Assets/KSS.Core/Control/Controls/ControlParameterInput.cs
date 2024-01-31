using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.Serialization;

namespace KSS.Control.Controls
{
    public abstract class ControlParameterInput : Control { }

    /// <summary>
    /// Represents a control that is used to retrieve a parameter from the <see cref="ControlParameterOutput{T}"/> it's connected to.
    /// </summary>
    public sealed class ControlParameterInput<T> : ControlParameterInput, IPersistent
    {
        public ControlParameterOutput<T> Output { get; internal set; }

        public ControlParameterInput()
        {
        }

        /// <summary>
        /// Invokes the connected parameter getter to retrieve the parameter.
        /// </summary>
        /// <param name="value">If the returned value is `true`, contains the value of the connected parameter getter.</param>
        /// <returns>True if the value was returned successfully (i.e. connection is connected and healthy), otherwise false.</returns>
        public bool TryGet( out T value )
        {
            if( Output == null )
            {
                value = default;
                return false;
            }

            value = Output.getter.Invoke();
            return true;
        }

        public override IEnumerable<Control> GetConnections()
        {
            yield return Output;
        }

        public override bool TryConnect( Control other )
        {
            if( other is not ControlParameterOutput<T> output )
                return false;

            Connect( this, output );
            return true;
        }

        public override bool TryDisconnect( Control other )
        {
            if( other is not ControlParameterOutput<T> output )
                return false;

            if( this.Output != output )
                return false;

            Disconnect( this, output );
            return true;
        }

        public override bool TryDisconnectAll()
        {
            if( this.Output == null )
                return false;

            Disconnect( this, this.Output );
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
        internal static void Disconnect( ControlParameterInput<T> input, ControlParameterOutput<T> output )
        {
            input.Output = null;
            output.Input = null;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static void Connect( ControlParameterInput<T> input, ControlParameterOutput<T> output )
        {
            input.Output = output;
            output.Input = input;
        }
    }
}