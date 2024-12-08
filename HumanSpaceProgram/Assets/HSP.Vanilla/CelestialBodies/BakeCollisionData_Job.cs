using HSP.CelestialBodies.Surfaces;
using System;
using UnityEngine;

namespace HSP.Vanilla.CelestialBodies
{
    /// <summary>
    /// Makes the PhysX collision data
    /// </summary>
    public struct BakeCollisionData_Job : ILODQuadJob
    {
        public bool Convex;

        int instanceId;

        public LODQuadMode QuadMode => LODQuadMode.Collider;

        public void Initialize( LODQuadRebuildData r )
        {
            instanceId = r.Mesh.GetInstanceID();
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