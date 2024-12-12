using HSP.CelestialBodies.Surfaces;
using System;
using Unity.Collections;
using UnityEngine;

namespace HSP.Vanilla.CelestialBodies
{
    public struct Displace_Job : ILODQuadJob
    {
        double radius;
        Vector3Dbl origin;

        int totalVertices;

        int sideVertices;
        int sideEdges;

        NativeArray<Vector3Dbl> resultVertices;

        public LODQuadMode QuadMode => LODQuadMode.VisualAndCollider;

        public void Initialize( LODQuadRebuildData r, LODQuadRebuildAdditionalData _ )
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

        public void Dispose()
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
                Vector3Dbl pos = resultVertices[index];
                Vector3Dbl unitSpherePos = resultVertices[index].normalized;

                Vector3Dbl temporaryHeightOffset_Removelater = unitSpherePos * 4000 + unitSpherePos * Math.Sin( (unitSpherePos.x * unitSpherePos.y * unitSpherePos.z) / 4190.0 * radius ) * 4000;
                temporaryHeightOffset_Removelater += unitSpherePos * Math.Sin( 0.1 + (unitSpherePos.x + unitSpherePos.y - unitSpherePos.z) / 1156.0 * radius ) * 1000;
                temporaryHeightOffset_Removelater += unitSpherePos * Math.Sin( 4 + (unitSpherePos.x - unitSpherePos.y + unitSpherePos.z) / 768.0 * radius ) * 500;
                temporaryHeightOffset_Removelater += unitSpherePos * Math.Sin( 143 + (unitSpherePos.x * unitSpherePos.y + unitSpherePos.z) / 358.0 * radius ) * 500;
                temporaryHeightOffset_Removelater += unitSpherePos * Math.Sin( -2 + (unitSpherePos.x + unitSpherePos.y * unitSpherePos.z) / 221.0 * radius ) * 203;
                temporaryHeightOffset_Removelater += unitSpherePos * Math.Sin( -12 + (unitSpherePos.x + unitSpherePos.y * unitSpherePos.z) / 22.0 * radius ) * 23;
                temporaryHeightOffset_Removelater += unitSpherePos * Math.Sin( -2 + (unitSpherePos.x + unitSpherePos.y * unitSpherePos.z) / 3.0 * radius ) * 7;

                resultVertices[index] = pos + temporaryHeightOffset_Removelater;
            }
        }
    }
}