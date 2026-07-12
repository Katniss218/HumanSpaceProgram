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
    public class FVesselSeparator : MonoBehaviour
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

            if( NodeToSeparate != null && NodeToSeparate.ConnectedNode != null )
            {
                VesselHierarchyUtils.Detach( NodeToSeparate, NodeToSeparate.ConnectedNode );
            }

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
                .WithMember( "has_separated", o => o._hasSeparated )
                .WithMember( "node_to_separate", o => o.NodeToSeparate );
        }
    }
}