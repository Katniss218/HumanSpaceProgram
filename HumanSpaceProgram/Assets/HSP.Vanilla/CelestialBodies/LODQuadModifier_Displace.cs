using HSP.CelestialBodies.Surfaces;
using System;
using Unity.Collections;
using UnityEngine;

namespace HSP.Vanilla.CelestialBodies
{
    public class LODQuadModifier_Displace : ILODQuadModifier
    {
        public LODQuadMode QuadMode => LODQuadMode.VisualAndCollider;

        public ILODQuadJob GetJob()
        {
            return new Job();
        }

        public struct Job : ILODQuadJob
        {
            double radius;

            int totalVertices;

            int sideVertices;
            int sideEdges;

            NativeArray<Vector3Dbl> resultVertices;

            public void Initialize( LODQuadRebuildData r, LODQuadRebuildAdditionalData _ )
            {
                radius = (float)r.CelestialBody.Radius;

                sideEdges = r.SideEdges;
                sideVertices = r.SideVertices;
                totalVertices = sideVertices * sideVertices;

                resultVertices = r.ResultVertices;
            }

            public void Finish( LODQuadRebuildData r )
            {
            }

            public void Dispose()
            {
            }

            public void Execute()
            {
                for( int index = 0; index < totalVertices; index++ )
                {
                    Vector3Dbl dir = resultVertices[index].normalized;

                    Vector3Dbl temporaryHeightOffset_Removelater = dir * Math.Sin( (dir.x * dir.y * dir.z) / 3 * radius ) * 9;

                    //Vector3Dbl temporaryHeightOffset_Removelater = unitSpherePos * 1000 + unitSpherePos * Math.Sin( 1 + (unitSpherePos.x * unitSpherePos.y * unitSpherePos.z) / 4190.0 * radius ) * 4000;
                    //temporaryHeightOffset_Removelater += unitSpherePos * Math.Sin( -0.1 + (unitSpherePos.x + unitSpherePos.y - unitSpherePos.z) / 1156.0 * radius ) * 1000;
                    //temporaryHeightOffset_Removelater += unitSpherePos * Math.Sin( 4 + (unitSpherePos.x - unitSpherePos.y + unitSpherePos.z) / 768.0 * radius ) * 200;
                    //temporaryHeightOffset_Removelater += unitSpherePos * Math.Sin( 143 + (unitSpherePos.x * unitSpherePos.y + unitSpherePos.z) / 358.0 * radius ) * 100;
                    // temporaryHeightOffset_Removelater += unitSpherePos * Math.Sin( -2 + (unitSpherePos.x + unitSpherePos.y * unitSpherePos.z) / 221.0 * radius ) * 63;
                    temporaryHeightOffset_Removelater += dir * Math.Sin( -12 + (dir.x + dir.y * dir.z) / 2.53 * radius ) * 2;
                    //temporaryHeightOffset_Removelater += unitSpherePos * Math.Sin( -2 + (unitSpherePos.x + unitSpherePos.y * unitSpherePos.z) / 3.0 * radius ) * 17;

                    resultVertices[index] += temporaryHeightOffset_Removelater;
                }
            }
        }
    }
}