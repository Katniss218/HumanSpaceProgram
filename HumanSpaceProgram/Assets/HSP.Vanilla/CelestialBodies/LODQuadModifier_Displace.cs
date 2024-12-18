using HSP.CelestialBodies.Surfaces;
using System;
using Unity.Collections;
using UnityEngine;

namespace HSP.Vanilla.CelestialBodies
{
    public class LODQuadModifier_Displace : ILODQuadModifier
    {
        public LODQuadMode QuadMode => LODQuadMode.VisualAndCollider;

        public double Offset { get; set; }

        public ILODQuadJob GetJob()
        {
            return new Job( this );
        }

        public struct Job : ILODQuadJob
        {
            double offset;

            int totalVertices;

            int sideVertices;

            NativeArray<Vector3Dbl> resultVertices;

            public Job( LODQuadModifier_Displace modifier )
            {
                this.offset = modifier.Offset;

                this.sideVertices = default;
                this.totalVertices = default;
                this.resultVertices = default;
            }

            public void Initialize( LODQuadRebuildData r, LODQuadRebuildAdditionalData _ )
            {
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

                    resultVertices[index] += dir * offset;
                }
            }
        }
    }
}