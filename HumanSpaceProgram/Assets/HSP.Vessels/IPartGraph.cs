using System.Collections.Generic;

namespace HSP.Vessels
{
    public interface IPartGraph
    {
        IReadonlyVesselAttachmentGraph Attachments { get; }
        IEnumerable<IReadonlyVesselIsland> Islands { get; }
        IEnumerable<VesselPart> Parts { get; }

        void SetGraph(VesselAttachmentGraph graph);

        IReadOnlyList<T> GetFComponents<T>() where T : class;
    }
}