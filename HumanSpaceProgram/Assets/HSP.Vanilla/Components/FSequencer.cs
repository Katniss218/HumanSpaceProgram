using HSP.ControlSystems;
using System;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Descriptors;

namespace HSP.Vanilla.Components
{
    /// <summary>
    /// Represents a controller that can invoke an arbitrary control action from a queue.
    /// </summary>
    public class FSequencer : MonoBehaviour
    {
        [NamedControl( "Sequence", Editable = false )]
        public Sequence Sequence = new Sequence();

        public Action OnAfterInvoked;

        void Start()
        {
            Sequence.TryInitialize();
        }

        void Update()
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