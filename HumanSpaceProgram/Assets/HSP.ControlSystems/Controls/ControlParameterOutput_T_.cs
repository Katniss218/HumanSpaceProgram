using System;
using System.Collections.Generic;
using System.Linq;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Descriptors;

namespace HSP.ControlSystems.Controls
{
    /// <summary>
    /// Represents a control that produces a parameter of type <typeparamref name="T"/>.
    /// </summary>
    public sealed class ControlParameterOutput<T> : ControlParameterOutputBase
	{
		internal Func<T> getter;

		internal List<ControlParameterInput<T>> inputs = new();
		public IEnumerable<ControlParameterInput<T>> Inputs { get => inputs; }

		/// <param name="getter">Returns the value of the parameter.</param>
		public ControlParameterOutput( Func<T> getter )
		{
			if( getter == null )
				throw new ArgumentNullException( nameof( getter ), $"The parameter getter must be initialized to a non-null value." );

			this.getter = getter;
		}

		public override IEnumerable<Control> GetConnectedControls()
		{
			return inputs;
		}

		public override bool TryConnect( Control other )
		{
			if( other is not ControlParameterInput<T> input )
				return false;

			ControlParameterInput<T>.Connect( input, this );
			return true;
		}

		public override bool TryDisconnect( Control other )
		{
			if( other is not ControlParameterInput<T> input )
				return false;

			if( !this.inputs.Contains( input ) )
				return false;

			ControlParameterInput<T>.Disconnect( input );
			return true;
		}

		public override bool TryDisconnectAll()
		{
			if( !this.inputs.Any() )
				return false;

			foreach( var connection in this.inputs.ToArray() )
			{
				ControlParameterInput<T>.Disconnect( connection );
			}
			return true;
		}
	}

	public static class Mappings_ControlParameterOutput_T_
	{
        [MapsInheritingFrom( typeof( ControlParameterOutput<> ) )]
        public static IDescriptor ControlParameterOutputMapping<T>()
        {
            return new MemberwiseDescriptor<ControlParameterOutput<T>>()
                .WithReadonlyMember( "getter", o => o.getter )
                .WithFactory<Func<T>>( onInvoke => new ControlParameterOutput<T>( onInvoke ), "getter" )
                .WithMember( "connects_to", typeof( Ctx.Array<Ctx.Ref> ), o => o.inputs, ( o, value ) =>
                {
                    if( value == null )
                        return;

                    foreach( var c in value.ToArray() )
                    {
                        if( c == null )
                            continue;

                        ControlParameterInput<T>.Connect( c, o );
                    }
                } );
        }
    }
}