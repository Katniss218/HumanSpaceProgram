using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.ControlSystems.Controls
{
    /// <summary>
    /// Represents a control that consumes a control signal of type <typeparamref name="T"/>.
    /// </summary>
    public sealed class ControlleeInput<T> : ControlleeInputBase
    {
		internal Action<T> onInvoke;

        internal List<ControllerOutput<T>> outputs = new();
        public IEnumerable<ControllerOutput<T>> Outputs { get => outputs; }

		/// <param name="signalResponse">The action to perform when a control signal is sent to this input.</param>
		public ControlleeInput( Action<T> signalResponse )
		{
			this.onInvoke = signalResponse;
		}

        public override IEnumerable<Control> GetConnectedControls()
        {
            return outputs;
        }

        public override bool TryConnect( Control other )
        {
            if( other is not ControllerOutput<T> output )
                return false;

            Connect( this, output );
            return true;
        }

        public override bool TryDisconnect( Control other )
        {
            if( other is not ControllerOutput<T> output )
                return false;

            if( !this.outputs.Contains( output ) )
                return false;

            Disconnect( output );
            return true;
        }

        public override bool TryDisconnectAll()
        {
            if( this.outputs == null )
                return false;

            foreach( var connection in outputs.ToArray() )
            {
                Disconnect( connection );
            }
            return true;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static void Disconnect( ControllerOutput<T> output )
        {
            output.Input?.outputs.Remove( output );
            output.Input = null;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static void Connect( ControlleeInput<T> input, ControllerOutput<T> output )
        {
            // disconnect from previous, if connected.
            Disconnect( output );

            output.Input = input;
            input.outputs.Add( output );
        }
    }

    public static class Mappings_ControlleeInput_T_
    {
        [MapsInheritingFrom( typeof( ControlleeInput<> ) )]
        public static SerializationMapping ControlleeInputMapping<T>()
        {
            return new MemberwiseSerializationMapping<ControlleeInput<T>>()
                .WithReadonlyMember( "on_invoke", o => o.onInvoke )
                .WithFactory<Action<T>>( onInvoke => new ControlleeInput<T>( onInvoke ) )
                .WithMember( "connects_to", ArrayContext.Refs, o => o.outputs, ( o, value ) =>
                {
                    if( value == null ) 
                        return;

                    foreach( var c in value )
                    {
                        if( c == null )
                            continue;

                        ControlleeInput<T>.Connect( o, c );
                    }
                } );
        }
    }
}