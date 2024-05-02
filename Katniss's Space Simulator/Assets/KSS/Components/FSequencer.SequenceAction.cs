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
    public abstract class SequenceActionBase : ControlGroup, IPersistsObjects // pass-through group with a single element. Required to be drawn.
	{
		public abstract ControllerOutputBase OnInvoke { get; }
		public abstract void TryInvoke();

		public abstract SerializedObject GetObjects( IReverseReferenceMap s );
		public abstract void SetObjects( SerializedObject data, IForwardReferenceMap l );
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
			return new SerializedObject()
			{
				{ "on_invoke", s.GetID( OnInvokeTyped ).GetData() }
			};
		}

		public override void SetObjects( SerializedObject data, IForwardReferenceMap l )
		{
			if( data.TryGetValue( "on_invoke", out var onInvokeTyped ) )
			{
				OnInvokeTyped = new();
				l.SetObj( onInvokeTyped.ToGuid(), OnInvokeTyped );
			}
		}
	}

    /// <summary>
    /// Represents a sequence action that sends a signal of type T.
    /// </summary>
    public class SequenceAction<T> : SequenceActionBase, IPersistsObjects
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
			return new SerializedObject()
            {
#warning TODO - needs a common method to create an object stub (and use it in every component).
				{ "on_invoke", s.GetID( OnInvokeTyped ).GetData() }
			};
		}

		public override void SetObjects( SerializedObject data, IForwardReferenceMap l )
		{
			if( data.TryGetValue( "on_invoke", out var onInvokeTyped ) )
			{
				OnInvokeTyped = new();
				l.SetObj( onInvokeTyped.ToGuid(), OnInvokeTyped );
			}
		}
	}
}