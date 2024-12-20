using HSP.CelestialBodies.Surfaces;
using System;
using UnityEngine;

namespace HSP.Vanilla.CelestialBodies
{
    public class LODQuadModifier_BakeCollisionData : ILODQuadModifier
    {
        public LODQuadMode QuadMode => LODQuadMode.Collider;

        public ILODQuadJob GetJob()
        {
            return new Job();
        }

        /// <summary>
        /// Bakes the PhysX collision data off the main thread.
        /// </summary>
        /// <remarks>
        /// Without this job, the data is baked when the mesh is assigned to the collider (blocking the main thread).
        /// </remarks>
        public struct Job : ILODQuadJob
        {
            int instanceId;

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
                Physics.BakeMesh( instanceId, false );
            }
        }
    }
}