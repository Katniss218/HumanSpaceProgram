﻿using System;
using Unity.Collections;
using UnityEngine;

namespace HSP.CelestialBodies.Surfaces
{
    public struct Displace_Job : ILODQuadJob
    {
        double radius;
        Vector3Dbl origin;

        int totalVertices;

        int sideVertices;
        int sideEdges;

        NativeArray<Vector3> resultVertices;

        public LODQuadMode QuadMode => LODQuadMode.VisualAndCollider;

        public void Initialize( LODQuadRebuildData r )
        {
            radius = (float)r.CelestialBody.Radius;
            origin = r.Node.SphereCenter * radius;

            sideEdges = r.SideEdges;
            sideVertices = r.SideVertices;
            totalVertices = sideVertices * sideVertices;

            resultVertices = r.ResultVertices;
        }

        public void Finish( LODQuadRebuildData r )
        {
        }

        public ILODQuadJob Clone()
        {
            return new Displace_Job();
        }

        public void Execute()
        {
            for( int index = 0; index < totalVertices; index++ )
            {
                Vector3Dbl posD = (resultVertices[index] + origin) / radius;

                Vector3 unitSpherePos = (Vector3)posD;

                Vector3Dbl temporaryHeightOffset_Removelater = posD * 5 * Math.Sin( (unitSpherePos.x + unitSpherePos.y + unitSpherePos.z) * radius );

                resultVertices[index] = (Vector3)(((posD * radius) + temporaryHeightOffset_Removelater) - origin);
            }
        }
    }
}