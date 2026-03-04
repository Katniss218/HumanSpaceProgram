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
            Separate ??= new ControlleeInput( SeparateListener );
        }

        [MapsInheritingFrom( typeof( FVesselSeparator ) )]
        public static IDescriptor FVesselSeparatorMapping()
        {
            return new MemberwiseDescriptor<FVesselSeparator>()
                .WithMember( "separate", o => o.Separate )
                .WithMember( "has_separated", o => o._hasSeparated );
        }
    }
}