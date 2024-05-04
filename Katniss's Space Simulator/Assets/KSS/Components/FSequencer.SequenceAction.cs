using KSS.Control.Controls;
using KSS.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.Serialization;

namespace KSS.Components
{
    /// <summary>
    /// Sequence actions are individual connections that send signals when a <see cref="SequenceElement"/> is triggered. <br/>
	/// This class represents an arbitrary type of such action.
    /// </summary>
	/// <remarks>
	/// This class exists solely to allow polymorphism between the empty and T-typed sequence actions.
	/// </remarks>
    public abstract class SequenceActionBase : ControlGroup, IPersistsObjects, IPersistsData // pass-through group with a single element. Required to be drawn.
    {
        public abstract ControllerOutputBase OnInvoke { get; }
        public abstract void TryInvoke();

        public abstract SerializedObject GetObjects( IReverseReferenceMap s );
        public abstract void SetObjects( SerializedObject data, IForwardReferenceMap l );
        public abstract SerializedData GetData( IReverseReferenceMap s );
        public abstract void SetData( SerializedData data, IForwardReferenceMap l );
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

        public override SerializedObject GetObjects( IReverseReferenceMap s )
        {
            SerializedObject data = Persistent_object.WriteObjectStub( this, this.GetType(), s );

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
        }
    }

    /// <summary>
    /// Represents a sequence action that sends a signal of type T.
    /// </summary>
    public class SequenceAction<T> : SequenceActionBase, IPersistsObjects, IPersistsData
    {
        public override ControllerOutputBase OnInvoke => OnInvokeTyped;

        [NamedControl( "x", Editable = false )]
        public ControllerOutput<T> OnInvokeTyped;

        public T SignalValue { get; set; }

        public override void TryInvoke()
        {
            OnInvokeTyped.TrySendSignal( SignalValue );
        }

        public override SerializedObject GetObjects( IReverseReferenceMap s )
        {
            SerializedObject data = Persistent_object.WriteObjectStub( this, this.GetType(), s );

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
                SignalValue = Activator.CreateInstance<T>(); // TODO - Provide a way of inline serialization. Maybe via 'new()' constraint.
                SignalValue.SetData( signalValue, l );
            }
        }
    }
}