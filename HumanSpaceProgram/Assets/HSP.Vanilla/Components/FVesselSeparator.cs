using HSP.ControlSystems;
using HSP.ControlSystems.Controls;
using HSP.Vessels;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Components
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