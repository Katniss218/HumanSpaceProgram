using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.Serialization;

namespace KSS.Control.Controls
{
	/// <summary>
	/// Represents a control that consumes a control signal of type <typeparamref name="T"/>.
	/// </summary>
	public sealed class ControlleeInput<T> : ControlleeInputBase, IPersistsData
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

        public SerializedData GetData( IReverseReferenceMap s )
        {
            SerializedArray sa = new SerializedArray();
            foreach( var conn in outputs )
            {
                sa.Add( s.WriteObjectReference( conn ) );
            }
            return new SerializedObject()
            {
                { "connects_to", sa }
            };
        }

        public void SetData( SerializedData data, IForwardReferenceMap l )
        {
            if( data.TryGetValue( "connects_to", out var connectsTo ) )
            {
                this.outputs.Clear();
                foreach( var conn in (SerializedArray)connectsTo )
                {
                    var c = (ControllerOutput<T>)l.ReadObjectReference( conn );
                    Connect( this, c );
                }
            }
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
}