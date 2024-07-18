using HSP.ControlSystems.Controls;
using HSP.ControlSystems;
using HSP.Core;
using System;
using UnityEngine;
using UnityPlus.Serialization;
using HSP.Vessels;

namespace HSP.Components
{
    public class FVesselSeparator : MonoBehaviour
    {
        bool _hasSeparated = false;

        [NamedControl( "Separate", "Connect this to the sequencer, or a controller's separation output." )]
        public ControlleeInput Separate;
        private void SeparateListener()
        {
            if( _hasSeparated )
            {
                return;
            }

            VesselHierarchyUtils.SetParent( this.transform, null );
            _hasSeparated = true;
        }

        void Awake()
        {
            Separate = new ControlleeInput( SeparateListener );
        }

        [MapsInheritingFrom( typeof( FVesselSeparator ) )]
        public static SerializationMapping FVesselSeparatorMapping()
        {
            return new MemberwiseSerializationMapping<FVesselSeparator>()
            {
                ("separate", new Member<FVesselSeparator, ControlleeInput>( o => o.Separate )),
                ("has_separated", new Member<FVesselSeparator, bool>( o => o._hasSeparated ))
            };
        }
    }
}