using KSS.Core.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Control.Controls
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
            {
                ("on_invoke", new Member<ControlleeInput<T>, Action<T>>( o => o.onInvoke )),
                ("connects_to", new Member<ControlleeInput<T>, ControllerOutput<T>[]>( ArrayContext.Refs, o => o.outputs.ToArray(), (o, value) =>
                {
                    foreach( var c in value )
                    {
                        ControlleeInput<T>.Connect( o, c );
                    }
                } ))
            }
            .WithFactory( ( data, l ) => // Either this, or use mapping that instantiates on reference pass.
            {
                if( data == null )
                    return null;

#warning TODO - is this even needed now? I think so, because the delegate mapping creates the delegate in reference pass (because object must be present)
                // this could be resolved by having 2 contexts for the delegate - one creating it in Load and one in LoadReferences.

                Action<T> onInvoke = (Action<T>)Persistent_Delegate.ToDelegate( data["on_invoke"], l.RefMap );

                return new ControlleeInput<T>( onInvoke );
            } );
        }
    }
}