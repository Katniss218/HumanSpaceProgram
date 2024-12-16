using System.Collections.Generic;

namespace HSP.CelestialBodies.Surfaces
{
    public sealed class LODQuadRebuildAdditionalData
    {
        public struct Entry
        {
            public readonly LODQuadRebuildData newR;
            public readonly LODQuadRebuildData oldR;

            public bool HasNew => newR != null;
            public bool HasOld => oldR != null;

            public Entry( LODQuadRebuildData @new, LODQuadRebuildData old )
            {
                this.newR = @new;
                this.oldR = old;
            }
        }

        public IReadOnlyDictionary<LODQuadTreeNode, Entry> allQuads;

        public LODQuadRebuildAdditionalData( Dictionary<LODQuadTreeNode, Entry> quads )
        {
            this.allQuads = quads;
        }
    }
}