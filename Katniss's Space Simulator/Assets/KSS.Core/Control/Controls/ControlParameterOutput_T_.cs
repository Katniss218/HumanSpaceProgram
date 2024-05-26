using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Control.Controls
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

        [SerializationMappingProvider( typeof( ControlParameterOutput<> ) )]
        public static SerializationMapping ControlParameterOutputMapping<Tt>()
        {
#warning TODO - I think it needs to be in a non-generic class
            return new CompoundSerializationMapping<ControlParameterOutput<Tt>>()
			{
				("connects_to", new MemberReferenceArray<ControlParameterOutput<Tt>, ControlParameterInput<Tt>>( o => o.inputs.ToArray(), (o, value) =>
				{
					foreach( var c in value )
					{
						ControlParameterInput<Tt>.Connect( c, o );
					}
				} ))
			};
        }
		/*
        public SerializedData GetData( IReverseReferenceMap s )
		{
			SerializedArray sa = new SerializedArray();
			foreach( var conn in inputs )
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
				this.inputs.Clear();
				foreach( var conn in (SerializedArray)connectsTo )
				{
					var c = (ControlParameterInput<T>)l.ReadObjectReference( conn );
					ControlParameterInput<T>.Connect( c, this );
				}
			}
		}*/
	}
}