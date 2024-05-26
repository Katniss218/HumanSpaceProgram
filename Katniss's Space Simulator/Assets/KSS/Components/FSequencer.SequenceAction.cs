using KSS.Control.Controls;
using KSS.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.Serialization;
using UnityEngine;

namespace KSS.Components
{
    /// <summary>
    /// Sequence actions are individual connections that send signals when a <see cref="SequenceElement"/> is triggered. <br/>
	/// This class represents an arbitrary type of such action.
    /// </summary>
	/// <remarks>
	/// This class exists solely to allow polymorphism between the empty and T-typed sequence actions.
	/// </remarks>
    public abstract class SequenceActionBase : ControlGroup // pass-through group with a single element. Required to be drawn.
    {
        public abstract ControllerOutputBase OnInvoke { get; }
        public abstract void TryInvoke();

       // public abstract SerializedObject GetObjects( IReverseReferenceMap s );
       // public abstract void SetObjects( SerializedObject data, IForwardReferenceMap l );
       // public abstract SerializedData GetData( IReverseReferenceMap s );
       // public abstract void SetData( SerializedData data, IForwardReferenceMap l );
    }

    /// <summary>
    /// Represents a sequence action that sends an empty signal.
    /// </summary>
    public class SequenceAction : SequenceActionBase
    {
        public override ControllerOutputBase OnInvoke => OnInvokeTyped;

        [NamedControl( "x", Editable = false )]
        public ControllerOutput OnInvokeTyped;

        public override void TryInvoke()
        {
            OnInvokeTyped.TrySendSignal();
        }


        [SerializationMappingProvider( typeof( SequenceAction ) )]
        public static SerializationMapping SequenceActionMapping()
        {
            return new CompoundSerializationMapping<SequenceAction>()
            {
                ("on_invoke", new MemberReference<SequenceAction, ControllerOutput>( o => o.OnInvokeTyped ))
            };
        }
        /*
        public override SerializedObject GetObjects( IReverseReferenceMap s )
        {
            SerializedObject data = Persistent_object.WriteObjectTyped( this, this.GetType(), s );

            data.AddAll( new SerializedObject()
            {
                { "on_invoke", Persistent_object.WriteObjectStub( OnInvokeTyped, s ) }
            } );

            return data;
        }

        public override void SetObjects( SerializedObject data, IForwardReferenceMap l )
        {
            if( data.TryGetValue<SerializedObject>( "on_invoke", out var onInvokeTyped ) )
            {
                OnInvokeTyped = ObjectFactory.AsObject<ControllerOutput>( onInvokeTyped, l );
                //OnInvokeTyped = new();
                //l.SetObj( onInvokeTyped.ToGuid(), OnInvokeTyped );
            }
        }

        public override SerializedData GetData( IReverseReferenceMap s )
        {
            return new SerializedObject() { };
        }

        public override void SetData( SerializedData data, IForwardReferenceMap l )
        {
        }*/
    }

    /// <summary>
    /// Represents a sequence action that sends a signal of type T.
    /// </summary>
    public class SequenceAction<T> : SequenceActionBase
    {
        public override ControllerOutputBase OnInvoke => OnInvokeTyped;

        [NamedControl( "x", Editable = false )]
        public ControllerOutput<T> OnInvokeTyped;

        public T SignalValue { get; set; }

        public override void TryInvoke()
        {
            OnInvokeTyped.TrySendSignal( SignalValue );
        }

        [SerializationMappingProvider( typeof( SequenceAction ) )]
        public static SerializationMapping SequenceActionMapping<Tt>()
        {
#warning TODO - I think it needs to be in a non-generic class
            return new CompoundSerializationMapping<SequenceAction<Tt>>()
            {
                ("on_invoke", new MemberReference<SequenceAction<Tt>, ControllerOutput<Tt>>( o => o.OnInvokeTyped ))
            };
        }
        /*
        public override SerializedObject GetObjects( IReverseReferenceMap s )
        {
            SerializedObject data = Persistent_object.WriteObjectTyped( this, this.GetType(), s );

            data.AddAll( new SerializedObject()
            {
                { "on_invoke", Persistent_object.WriteObjectStub( OnInvokeTyped, s ) }
            } );

            return data;
        }

        public override void SetObjects( SerializedObject data, IForwardReferenceMap l )
        {
            if( data.TryGetValue<SerializedObject>( "on_invoke", out var onInvokeTyped ) )
            {
                OnInvokeTyped = ObjectFactory.AsObject<ControllerOutput<T>>( onInvokeTyped, l );
            }
        }

        public override SerializedData GetData( IReverseReferenceMap s )
        {
            return new SerializedObject()
            {
                { "signal_value", this.SignalValue.GetData( s ) }
            };
        }

        public override void SetData( SerializedData data, IForwardReferenceMap l )
        {
            if( data.TryGetValue( "signal_value", out var signalValue ) )
            {
#warning TODO - support value type instantiation from inline data.
                SignalValue = ObjectFactory.AsObject<T>( signalValue, l );

                //SignalValue.SetData( signalValue, l );
            }
        }*/
    }
}