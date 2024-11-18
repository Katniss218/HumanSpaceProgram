using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Collections;

namespace HSP.CelestialBodies.Surfaces
{
    public struct SmoothNeighbors_Job : ILODQuadJob
    {
        NativeArray<int> edgeSubdivisionRelative;
        NativeArray<Vector3> resultVertices;

        int numberOfEdges;
        int numberOfVertices;

        public void Initialize( LODQuad quad, LODQuad.State.Rebuild r )
        {
            numberOfEdges = 1 << quad.EdgeSubdivisions; // Fast 2^n for integer types.
            numberOfVertices = numberOfEdges + 1;

            edgeSubdivisionRelative = new NativeArray<int>( 4, Allocator.TempJob );
            for( int i = 0; i < edgeSubdivisionRelative.Length; i++ )
            {
                if( quad.Edges[i] == null )
                {
                    edgeSubdivisionRelative[i] = 0;
                    continue;
                }
                edgeSubdivisionRelative[i] = quad.Edges[i].SubdivisionLevel - quad.SubdivisionLevel;
            }

            resultVertices = r.resultVertices;
        }

        public void Finish( LODQuad quad, LODQuad.State.Rebuild r )
        {
            edgeSubdivisionRelative.Dispose();
        }

        int GetIndex( int x, int y )
        {
            return (x * numberOfEdges) + x + y;
        }

        public void Execute()
        {
            if( edgeSubdivisionRelative[0] < 0 )
            {
                int step = (1 << -edgeSubdivisionRelative[0]);
                int x = 0;
                for( int y = 0; y < numberOfVertices - step; y += step )
                {
                    int indexMin = GetIndex( x, y );
                    int indexMax = GetIndex( x, y + step );
                    for( int y2 = 0; y2 < step; y2++ )
                    {
                        int index = GetIndex( x, y + y2 );
                        // find index of interval.
                        // smoothly blend.

                        resultVertices[index] = Vector3.Lerp( resultVertices[indexMin], resultVertices[indexMax], (float)y2 / step );
                    }
                }
            }

            if( edgeSubdivisionRelative[1] < 0 )
            {
                int step = (1 << -edgeSubdivisionRelative[1]);
                int x = numberOfVertices - 1;
                for( int y = 0; y < numberOfVertices - step; y += step )
                {
                    int indexMin = GetIndex( x, y );
                    int indexMax = GetIndex( x, y + step );
                    for( int y2 = 0; y2 < step; y2++ )
                    {
                        int index = GetIndex( x, y + y2 );
                        // find index of interval.
                        // smoothly blend.

                        resultVertices[index] = Vector3.Lerp( resultVertices[indexMin], resultVertices[indexMax], (float)y2 / step );
                    }
                }
            }

            if( edgeSubdivisionRelative[2] < 0 )
            {
                int step = (1 << -edgeSubdivisionRelative[2]);
                int y = 0;
                for( int x = 0; x < numberOfVertices - step; x += step )
                {
                    int indexMin = GetIndex( x, y );
                    int indexMax = GetIndex( x + step, y );
                    for( int x2 = 0; x2 < step; x2++ )
                    {
                        int index = GetIndex( x + x2, y );
                        // find index of interval.
                        // smoothly blend.

                        resultVertices[index] = Vector3.Lerp( resultVertices[indexMin], resultVertices[indexMax], (float)x2 / step );
                    }
                }
            }
            if( edgeSubdivisionRelative[3] < 0 )
            {
                int step = (1 << -edgeSubdivisionRelative[3]);
                int y = numberOfVertices - 1;
                for( int x = 0; x < numberOfVertices - step; x += step )
                {
                    int indexMin = GetIndex( x, y );
                    int indexMax = GetIndex( x + step, y );
                    for( int x2 = 0; x2 < step; x2++ )
                    {
                        int index = GetIndex( x + x2, y );
                        // find index of interval.
                        // smoothly blend.

                        resultVertices[index] = Vector3.Lerp( resultVertices[indexMin], resultVertices[indexMax], (float)x2 / step );
                    }
                }
            }
        }
    }
}
