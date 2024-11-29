using System;
using Unity.Collections;
using UnityEngine;

namespace HSP.CelestialBodies.Surfaces
{
    public struct Displace_Job : ILODQuadJob
    {
        double radius;
        Vector3Dbl origin;

        int totalVerts;
        int numberOfEdges;
        int numberOfVertices;

        NativeArray<Vector3> resultVertices;

        public LODQuadMode QuadMode => LODQuadMode.VisualAndCollider;

        public void Initialize( LODQuadRebuildData r )
        {
            radius = (float)r.radius;
            origin = r.node.SphereCenter * radius;

            numberOfEdges = r.numberOfEdges;
            numberOfVertices = r.numberOfVertices;
            totalVerts = numberOfVertices * numberOfVertices;

            resultVertices = r.resultVertices;
        }

        public void Finish( LODQuadRebuildData r )
        {
        }

        public void Execute()
        {
            for( int index = 0; index < totalVerts; index++ )
            {
                Vector3Dbl posD = (resultVertices[index] + origin) / radius;

                Vector3 unitSpherePos = (Vector3)posD;

                Vector3Dbl temporaryHeightOffset_Removelater = posD * 5 * Math.Sin( (unitSpherePos.x + unitSpherePos.y + unitSpherePos.z) * radius );

                resultVertices[index] = (Vector3)(((posD * radius) + temporaryHeightOffset_Removelater) - origin);
            }
        }
    }
}
