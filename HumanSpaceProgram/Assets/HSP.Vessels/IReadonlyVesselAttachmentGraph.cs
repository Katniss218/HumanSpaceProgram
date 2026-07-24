using System.Collections.Generic;

namespace HSP.Vessels
{
    public struct AttachmentEdge
    {
        public VesselPart Target;
        public bool IsRigid;
    }

    public interface IReadonlyVesselAttachmentGraph
    {
        bool HasNode( VesselPart part );
        IReadOnlyList<AttachmentEdge> GetEdges( VesselPart part );
        IReadOnlyCollection<VesselPart> Nodes { get; }
    }
}