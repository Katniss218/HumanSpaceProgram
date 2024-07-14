using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.Serialization;

namespace HSP.Control.Controls
{
	/// <summary>
	/// Represents a control that consumes a parameter of type <typeparamref name="T"/>.
	/// </summary>
	public sealed class ControlParameterInput<T> : ControlParameterInputBase
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

			Disconnect( this );
			return true;
		}

		public override bool TryDisconnectAll()
		{
			if( this.Output == null )
				return false;

			Disconnect( this );
			return true;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		internal static void Disconnect( ControlParameterInput<T> input )
		{
			input.Output?.inputs.Remove( input );
			input.Output = null;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		internal static void Connect( ControlParameterInput<T> input, ControlParameterOutput<T> output )
		{
			// disconnect from previous, if connected.
			Disconnect( input );

			input.Output = output;
			output.inputs.Add( input );
		}
    }

    public static class Mappings_ControlParameterInput_T_
    {
        [MapsInheritingFrom( typeof( ControlParameterInput<> ) )]
        public static SerializationMapping ControlParameterInputMapping<T>()
        {
            return new MemberwiseSerializationMapping<ControlParameterInput<T>>(); // empty dummy referencable thing.
        }
    }
}