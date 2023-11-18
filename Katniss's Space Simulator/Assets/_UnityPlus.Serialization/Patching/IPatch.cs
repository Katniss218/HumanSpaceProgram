using UnityPlus.Serialization.ReferenceMaps;

namespace UnityPlus.Serialization.Patching
{
    public interface IPatch
    {
        void Run( BidirectionalReferenceStore refMap );
    }
}