using HSP.CelestialBodies.Surfaces;
using System;
using UnityEngine;

namespace HSP.Vanilla.CelestialBodies
{
    public class LODQuadModifier_BakeCollisionData : ILODQuadModifier
    {
        public LODQuadMode QuadMode => LODQuadMode.Collider;

        public bool Convex { get; set; }

        public ILODQuadJob GetJob()
        {
            return new Job( this );
        }

        /// <summary>
        /// Bakes the PhysX collision data off the main thread.
        /// </summary>
        /// <remarks>
        /// Without this job, the data is baked when the mesh is assigned to the collider (blocking the main thread).
        /// </remarks>
        public struct Job : ILODQuadJob
        {
            bool convex;
            int instanceId;

            public Job( LODQuadModifier_BakeCollisionData modifier )
            {
                convex = modifier.Convex;

                instanceId = default;
            }

            public void Initialize( LODQuadRebuildData r, LODQuadRebuildAdditionalData _ )
            {
                instanceId = r.ResultMesh.GetInstanceID();
            }

            public void Finish( LODQuadRebuildData r )
            {
            }

            public void Dispose()
            {
            }

            public void Execute()
            {
                Physics.BakeMesh( instanceId, convex );
            }
        }
    }
}