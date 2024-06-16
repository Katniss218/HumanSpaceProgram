using KSS.Components;
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
    public class Sequence : ControlGroup
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

        [SerializationMappingProvider( typeof( Sequence ) )]
        public static SerializationMapping SequenceMapping()
        {
            return new MemberwiseSerializationMapping<Sequence>()
            {
                ("elements", new Member<Sequence, SequenceElement[]>( o => o.Elements.ToArray(), (o, value) => o.Elements = value.ToList() )),
                ("current", new Member<Sequence, int>( o => o.Current ))
            }
            .WithFactory( ( data, l ) => new Sequence() );
        }
    }
}