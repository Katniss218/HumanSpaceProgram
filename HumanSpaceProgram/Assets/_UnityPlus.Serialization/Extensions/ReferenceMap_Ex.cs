using System;
using System.Linq;

namespace UnityPlus.Serialization.ReferenceMaps
{
    public static class ReferenceMap_Ex
    {
        /// <summary>
        /// Remaps the reference store 1-to-1, with a new random identifier for each existing identifier.
        /// </summary>
        public static IForwardReferenceMap RemapRandomly( this IForwardReferenceMap refStore )
        {
            switch( refStore )
            {
                case BidirectionalReferenceStore bidirectionalRefStore:
                    return RemapRandomly( bidirectionalRefStore );
                case ForwardReferenceStore forwardRefStore:
                    return RemapRandomly( forwardRefStore );
                default:
                    throw new ArgumentException( $"Unsupported reference store type: {refStore.GetType()}" );
            }
        }

        /// <summary>
        /// Remaps the reference store 1-to-1, with a new random identifier for each existing identifier.
        /// </summary>
        public static IReverseReferenceMap RemapRandomly( this IReverseReferenceMap refStore )
        {
            switch( refStore )
            {
                case BidirectionalReferenceStore bidirectionalRefStore:
                    return RemapRandomly( bidirectionalRefStore );
                case ReverseReferenceStore reverseReferenceStore:
                    return RemapRandomly( reverseReferenceStore );
                default:
                    throw new ArgumentException( $"Unsupported reference store type: {refStore.GetType()}" );
            }
        }

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