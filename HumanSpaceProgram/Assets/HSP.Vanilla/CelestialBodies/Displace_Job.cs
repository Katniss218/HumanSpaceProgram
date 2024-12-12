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

                Vector3Dbl temporaryHeightOffset_Removelater = unitSpherePos * Math.Sin( (unitSpherePos.x * unitSpherePos.y * unitSpherePos.z) / 3 * radius ) * 9;
                
                //Vector3Dbl temporaryHeightOffset_Removelater = unitSpherePos * 1000 + unitSpherePos * Math.Sin( 1 + (unitSpherePos.x * unitSpherePos.y * unitSpherePos.z) / 4190.0 * radius ) * 4000;
                //temporaryHeightOffset_Removelater += unitSpherePos * Math.Sin( -0.1 + (unitSpherePos.x + unitSpherePos.y - unitSpherePos.z) / 1156.0 * radius ) * 1000;
                //temporaryHeightOffset_Removelater += unitSpherePos * Math.Sin( 4 + (unitSpherePos.x - unitSpherePos.y + unitSpherePos.z) / 768.0 * radius ) * 200;
                //temporaryHeightOffset_Removelater += unitSpherePos * Math.Sin( 143 + (unitSpherePos.x * unitSpherePos.y + unitSpherePos.z) / 358.0 * radius ) * 100;
               // temporaryHeightOffset_Removelater += unitSpherePos * Math.Sin( -2 + (unitSpherePos.x + unitSpherePos.y * unitSpherePos.z) / 221.0 * radius ) * 63;
                temporaryHeightOffset_Removelater += unitSpherePos * Math.Sin( -12 + (unitSpherePos.x + unitSpherePos.y * unitSpherePos.z) / 2.53 * radius ) * 2;
                //temporaryHeightOffset_Removelater += unitSpherePos * Math.Sin( -2 + (unitSpherePos.x + unitSpherePos.y * unitSpherePos.z) / 3.0 * radius ) * 17;
                
                resultVertices[index] = pos + temporaryHeightOffset_Removelater;
            }
        }
    }
}