using System.Collections.Generic;

namespace HSP.Vessels
{
    public enum AttachmentEdgeType
    {
        /// <summary>
        /// An island-forming edge.
        /// </summary>
        Rigid,
        /// <summary>
        /// An edge that does not form an island.
        /// </summary>
        NonRigid
    }

    public struct AttachmentEdge
    {
        public AttachmentEdgeType Type;
        public VesselPart Target;
    }

    public interface IReadonlyVesselAttachmentGraph
    {
        IReadOnlyCollection<VesselPart> Nodes { get; }
        bool HasNode( VesselPart part );
        IReadOnlyList<AttachmentEdge> GetEdges( VesselPart part );
        bool HasEdge( VesselPart nodeA, VesselPart nodeB );
    }
}