﻿using HSP.Control;
using HSP.Control.Controls;
using HSP.Core;
using HSP.Core.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Components
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
        public static SerializationMapping FSequencerMapping()
        {
            return new MemberwiseSerializationMapping<FSequencer>()
            {
                ("sequence", new Member<FSequencer, Sequence>( o => o.Sequence ))
            };
        }
    }
}