﻿using KSS.Components;
using KSS.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Components
{
    /// <summary>
    /// A sequence contained within a <see cref="FSequencer"/>.
    /// </summary>
    public class Sequence : ControlGroup, IPersistsObjects, IPersistsData
    {
        [NamedControl( "Elements", Editable = false )]
        public List<SequenceElement> Elements = new();

        public IEnumerable<SequenceElement> InvokedElements => Elements.Take( Current );

        public IEnumerable<SequenceElement> RemainingElements => Elements.Skip( Current );

        public int Current { get; private set; } = 0;

        /// <summary>
        /// Tries to initialize the current element of the sequence.
        /// </summary>
        public bool TryInitialize()
        {
            if( Current < 0 || Current >= Elements.Count )
            {
                return false;
            }

            try
            {
                Elements[Current].Initialize();
                return true;
            }
            catch( Exception ex )
            {
                Debug.LogException( ex );
            }

            return false;
        }

        /// <summary>
        /// Tries to invoke the current element of the sequence.
        /// </summary>
        public bool TryInvoke()
        {
            if( Current < 0 || Current >= Elements.Count )
            {
                return false;
            }

            SequenceElement elem = Elements[Current];

            try
            {
                if( elem.CanInvoke() )
                {
                    elem.Invoke();
                    Current++;

                    TryInitialize(); // Initialize the next element.
                    return true;
                }
            }
            catch( Exception ex )
            {
                Debug.LogException( ex );
            }

            return false;
        }

        public SerializedObject GetObjects( IReverseReferenceMap s )
        {
            SerializedArray array = new SerializedArray( Elements.Select( e => e.GetObjects( s ) ) );

            return new SerializedObject()
            {
                { "elements", array }
            };
        }

        public void SetObjects( SerializedObject data, IForwardReferenceMap l )
        {
            if( data.TryGetValue<SerializedArray>( "elements", out var elements ) )
            {
                Elements = new();
                foreach( var serElem in elements.Cast<SerializedObject>() )
                {
                    SequenceElement elem = serElem.AsObject<SequenceElement>( l );

                    elem.SetObjects( serElem, l );

                    Elements.Add( elem );
                }
            }
        }

        public SerializedData GetData( IReverseReferenceMap s )
        {
            SerializedArray array = new SerializedArray( Elements.Select( e => e.GetData( s ) ) );

            return new SerializedObject()
            {
                { "current", Current.GetData() },
                { "elements", array }
            };
        }

        public void SetData( SerializedData data, IForwardReferenceMap l )
        {
            if( data.TryGetValue( "current", out var current ) )
                Current = current.AsInt32();

            if( data.TryGetValue<SerializedArray>( "elements", out var elements ) )
            {
                int i = 0;
                foreach( var serElem in elements.Cast<SerializedObject>() )
                {
                    Elements[i].SetData( serElem, l );

                    i++;
                }
            }
        }
    }
}