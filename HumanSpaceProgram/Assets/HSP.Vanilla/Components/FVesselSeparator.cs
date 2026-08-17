using HSP.ControlSystems;
using HSP.ControlSystems.Controls;
using HSP.Vessels;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Descriptors;

namespace HSP.Vanilla.Components
{
    /// <summary>
    /// 
    /// </summary>
    public class FVesselSeparator : FComponent
    {
        public FAttachNode NodeToSeparate;

        bool _hasSeparated = false;

        [NamedControl( "Separate", "Connect this to the sequencer, or a controller's separation output." )]
        public ControlleeInput Separate;
        private void SeparateListener()
        {
            if( _hasSeparated )
            {
                return;
            }

            if( NodeToSeparate != null )
            {
                VesselHierarchyUtils.Detach( NodeToSeparate );
            }

            _hasSeparated = true;
        }

        public FVesselSeparator()
        {
            Separate = new ControlleeInput( SeparateListener );
        }

        [MapsInheritingFrom( typeof( FVesselSeparator ) )]
        public static IDescriptor FVesselSeparatorMapping()
        {
            return new MemberwiseDescriptor<FVesselSeparator>()
                .WithMember( "separate", o => o.Separate )
                .WithMember( "has_separated", o => o._hasSeparated )
                .WithMember( "node_to_separate", o => o.NodeToSeparate );
        }
    }
}