using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization.ReferenceMaps
{
    public static class ReferenceMap_Ex
    {
        /// <summary>
        /// Remaps the reference store 1-to-1, with a new random identifier for each existing identifier.
        /// </summary>
        public static BidirectionalReferenceStore RemapRandomly( this BidirectionalReferenceStore refStore )
        {
            var remappedRefStore = new BidirectionalReferenceStore();

            remappedRefStore.AddAll( refStore.GetAll().Select( tuple => (Guid.NewGuid(), tuple.val) ) );

            return remappedRefStore;
        }

        /// <summary>
        /// Remaps the reference store 1-to-1, with a new random identifier for each existing identifier.
        /// </summary>
        public static ReverseReferenceStore RemapRandomly( this ReverseReferenceStore refStore )
        {
            var remappedRefStore = new ReverseReferenceStore();

            remappedRefStore.AddAll( refStore.GetAll().Select( tuple => (Guid.NewGuid(), tuple.val) ) );

            return remappedRefStore;
        }

        /// <summary>
        /// Remaps the reference store 1-to-1, with a new random identifier for each existing identifier.
        /// </summary>
        public static ForwardReferenceStore RemapRandomly( this ForwardReferenceStore refStore )
        {
            var remappedRefStore = new ForwardReferenceStore();

            remappedRefStore.AddAll( refStore.GetAll().Select( tuple => (Guid.NewGuid(), tuple.val) ) );

            return remappedRefStore;
        }
    }
}