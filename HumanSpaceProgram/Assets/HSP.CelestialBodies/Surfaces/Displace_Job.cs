using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Collections;
using UnityEditor.Search;

namespace HSP.CelestialBodies.Surfaces
{
    public struct Displace_Job : ILODQuadJob
    {
        int subdivisions;
        double radius;
        Vector2 center;
        int lN;
        Vector3 origin;

        NativeArray<Vector3> resultVertices;

        float size;

        int totalVerts;
        int numberOfEdges;
        int numberOfVertices;

        public void Initialize( LODQuad quad, LODQuad.State.Rebuild r )
        {
            subdivisions = quad.EdgeSubdivisions;
            radius = (float)quad.CelestialBody.Radius;
            center = quad.Node.Center;
            lN = quad.SubdivisionLevel;
            origin = quad.transform.localPosition;

            size = LODQuadTree_NodeUtils.GetSize( lN );

            numberOfEdges = 1 << subdivisions; // Fast 2^n for integer types.
            numberOfVertices = numberOfEdges + 1;
            totalVerts = numberOfVertices * numberOfVertices;

            resultVertices = r.resultVertices;
        }

        public void Finish( LODQuad quad, LODQuad.State.Rebuild r )
        {
        }

        public void Execute()
        {
            for( int index = 0; index < totalVerts; index++ )
            {
                Vector3Dbl posD = (resultVertices[index] + (Vector3Dbl)origin) / radius;

                Vector3 unitSpherePos = (Vector3)posD;

                Vector3Dbl temporaryHeightOffset_Removelater = posD * 5 * Math.Sin( (unitSpherePos.x + unitSpherePos.y + unitSpherePos.z) * radius );

                resultVertices[index] = (Vector3)(((posD * radius) + temporaryHeightOffset_Removelater) - (Vector3Dbl)origin);
            }
        }
    }
}
