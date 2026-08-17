using HSP.ControlSystems;
using HSP.Vessels;
using System;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Descriptors;

namespace HSP.Vanilla.Components
{
    /// <summary>
    /// Represents a controller that can invoke an arbitrary control action from a queue.
    /// </summary>
    public class FSequencer : FComponent
    {
        [NamedControl( "Sequence", Editable = false )]
        public Sequence Sequence = new Sequence();

        public Action OnAfterInvoked;

        public override void OnEnable()
        {
            Sequence.TryInitialize();
        }

        public override void Update()
        {
            if( Sequence.TryInvoke() )
            {
                OnAfterInvoked?.Invoke();
            }
        }

        [MapsInheritingFrom( typeof( FSequencer ) )]
        public static IDescriptor FSequencerMapping()
        {
            return new MemberwiseDescriptor<FSequencer>()
                .WithMember( "sequence", o => o.Sequence );
        }
    }
}