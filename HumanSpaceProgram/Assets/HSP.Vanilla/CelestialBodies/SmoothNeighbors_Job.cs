using HSP.CelestialBodies.Surfaces;
using Unity.Collections;
using UnityEngine;

namespace HSP.Vanilla.CelestialBodies
{
    public struct SmoothNeighbors_Job : ILODQuadJob
    {
        int stepXn;
        int stepXp;
        int stepYn;
        int stepYp;

        int numberOfEdges;
        int numberOfVertices;

        NativeArray<Vector3> resultVertices;

        public LODQuadMode QuadMode => LODQuadMode.VisualAndCollider;

        public void Initialize( LODQuadRebuildData r, LODQuadRebuildAdditionalData _ )
        {
            numberOfEdges = r.SideEdges;
            numberOfVertices = r.SideVertices;

            // A quad with subdivision level N will never border more than 1 quad with subdivision level >= N per side.
            // It can however border more than 1 quad with larger subdivision level than itself.
            stepXn = 1 << (r.Node.SubdivisionLevel - r.Node.Xn.SubdivisionLevel);
            stepXp = 1 << (r.Node.SubdivisionLevel - r.Node.Xp.SubdivisionLevel);
            stepYn = 1 << (r.Node.SubdivisionLevel - r.Node.Yn.SubdivisionLevel);
            stepYp = 1 << (r.Node.SubdivisionLevel - r.Node.Yp.SubdivisionLevel);
            resultVertices = r.ResultVertices;
        }

        public void Finish( LODQuadRebuildData r )
        {
            //r.Mesh.RecalculateNormals();
            //r.Mesh.RecalculateTangents();
            //r.Mesh.FixTangents(); // fix broken tangents.
        }

        public void Dispose()
        {
        }

        public ILODQuadJob Clone()
        {
            return new SmoothNeighbors_Job();
        }

        int GetIndex( int x, int y )
        {
            return (x * numberOfEdges) + x + y;
        }

        public void Execute()
        {
            if( stepXn != 0 )
            {
                int x = 0;
                for( int y = 0; y < numberOfVertices - stepXn; y += stepXn )
                {
                    int indexMin = GetIndex( x, y );
                    int indexMax = GetIndex( x, y + stepXn );
                    for( int y2 = 0; y2 < stepXn; y2++ )
                    {
                        int index = GetIndex( x, y + y2 );
                        // find index of interval.
                        // smoothly blend.

                        resultVertices[index] = Vector3.Lerp( resultVertices[indexMin], resultVertices[indexMax], (float)y2 / stepXn );
                    }
                }
            }

            if( stepXp != 0 )
            {
                int x = numberOfVertices - 1;
                for( int y = 0; y < numberOfVertices - stepXp; y += stepXp )
                {
                    int indexMin = GetIndex( x, y );
                    int indexMax = GetIndex( x, y + stepXp );
                    for( int y2 = 0; y2 < stepXp; y2++ )
                    {
                        int index = GetIndex( x, y + y2 );
                        // find index of interval.
                        // smoothly blend.

                        resultVertices[index] = Vector3.Lerp( resultVertices[indexMin], resultVertices[indexMax], (float)y2 / stepXp );
                    }
                }
            }

            if( stepYn != 0 )
            {
                int y = 0;
                for( int x = 0; x < numberOfVertices - stepYn; x += stepYn )
                {
                    int indexMin = GetIndex( x, y );
                    int indexMax = GetIndex( x + stepYn, y );
                    for( int x2 = 0; x2 < stepYn; x2++ )
                    {
                        int index = GetIndex( x + x2, y );
                        // find index of interval.
                        // smoothly blend.

                        resultVertices[index] = Vector3.Lerp( resultVertices[indexMin], resultVertices[indexMax], (float)x2 / stepYn );
                    }
                }
            }
            if( stepYp != 0 )
            {
                int y = numberOfVertices - 1;
                for( int x = 0; x < numberOfVertices - stepYp; x += stepYp )
                {
                    int indexMin = GetIndex( x, y );
                    int indexMax = GetIndex( x + stepYp, y );
                    for( int x2 = 0; x2 < stepYp; x2++ )
                    {
                        int index = GetIndex( x + x2, y );
                        // find index of interval.
                        // smoothly blend.

                        resultVertices[index] = Vector3.Lerp( resultVertices[indexMin], resultVertices[indexMax], (float)x2 / stepYp );
                    }
                }
            }
        }
    }
}