using HSP.ControlSystems;
using HSP.ControlSystems.Controls;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Components
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

        [MapsInheritingFrom( typeof( SequenceAction ) )]
        public static SerializationMapping SequenceActionMapping()
        {
            return new MemberwiseSerializationMapping<SequenceAction>()
                .WithMember( "on_invoke", o => o.OnInvokeTyped );
        }
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
    }

    public static class Mappings_SequenceAction_T_
    {
        [MapsInheritingFrom( typeof( SequenceAction<> ) )]
        public static SerializationMapping SequenceActionMapping<T>()
        {
            return new MemberwiseSerializationMapping<SequenceAction<T>>()
                .WithMember( "on_invoke", o => o.OnInvokeTyped )
                .WithMember( "signal_value", o => o.SignalValue );
        }
    }
}