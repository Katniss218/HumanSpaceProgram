using HSP.CelestialBodies.Surfaces;
using UnityEngine;

namespace HSP.Vanilla.CelestialBodies
{
    /// <summary>
    /// Bakes the PhysX collision data off the main thread.
    /// </summary>
    /// <remarks>
    /// Without this job, the data is baked when the mesh is assigned to the collider (blocking the main thread).
    /// </remarks>
    public struct BakeCollisionData_Job : ILODQuadJob
    {
        public bool Convex;

        int instanceId;

        public LODQuadMode QuadMode => LODQuadMode.Collider;

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

        public ILODQuadJob Clone()
        {
            return new BakeCollisionData_Job()
            {
                Convex = Convex,
            };
        }

        public void Execute()
        {
            Physics.BakeMesh( instanceId, Convex );
        }
    }
}