using HSP.CelestialBodies.Surfaces;
using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;

namespace HSP.Vanilla.CelestialBodies
{
    /// <summary>
    /// Displaces the vertices along the direction from the center of the body.
    /// </summary>
    public struct HeightMap_Job : ILODQuadJob
    {
#error TODO - for this to work it would have to be separated into the job, and a job class that actually holds the data. 
        // maybe store the job on the LODsphere only, and the job class can create a job struct of the same type from it to put into the rebuilder?

        Texture2D _heightmapXn;
        Texture2D _heightmapXp;
        Texture2D _heightmapYn;
        Texture2D _heightmapYp;
        Texture2D _heightmapZn;
        Texture2D _heightmapZp;
        int _widthXn, _heightXn;
        int _widthXp, _heightXp;
        int _widthYn, _heightYn;
        int _widthYp, _heightYp;
        int _widthZn, _heightZn;
        int _widthZp, _heightZp;

        public Texture2D HeightmapXn
        {
            get => _heightmapXn;
            set
            {
                _heightmapXn = value;
                _widthXn = value.width;
                _heightXn = value.height;
            }
        }

        public Texture2D HeightmapXp
        {
            get => _heightmapXp;
            set
            {
                _heightmapXp = value;
                _widthXp = value.width;
                _heightXp = value.height;
            }
        }

        public Texture2D HeightmapYn
        {
            get => _heightmapYn;
            set
            {
                _heightmapYn = value;
                _widthYn = value.width;
                _heightYn = value.height;
            }
        }

        public Texture2D HeightmapYp
        {
            get => _heightmapYp;
            set
            {
                _heightmapYp = value;
                _widthYp = value.width;
                _heightYp = value.height;
            }
        }

        public Texture2D HeightmapZn
        {
            get => _heightmapZn;
            set
            {
                _heightmapZn = value;
                _widthZn = value.width;
                _heightZn = value.height;
            }
        }

        public Texture2D HeightmapZp
        {
            get => _heightmapZp;
            set
            {
                _heightmapZp = value;
                _widthZp = value.width;
                _heightZp = value.height;
            }
        }

        public double MinLevel;
        public double MaxLevel;

        //
        //
        //

        [ReadOnly]
        NativeArray<short> _heightmapArray16bitXn;
        [ReadOnly]
        NativeArray<short> _heightmapArray16bitXp;
        [ReadOnly]
        NativeArray<short> _heightmapArray16bitYn;
        [ReadOnly]
        NativeArray<short> _heightmapArray16bitYp;
        [ReadOnly]
        NativeArray<short> _heightmapArray16bitZn;
        [ReadOnly]
        NativeArray<short> _heightmapArray16bitZp;

        double radius;

        int totalVertices;
        Vector2 faceCenter;
        float edgeLength;
        float halfSize;

        int sideVertices;
        int sideEdges;
        Direction3D face;

        NativeArray<Vector3Dbl> resultVertices;

        public LODQuadMode QuadMode => LODQuadMode.VisualAndCollider;

        public void Initialize( LODQuadRebuildData r, LODQuadRebuildAdditionalData _ )
        {
            radius = (float)r.CelestialBody.Radius;
            face = r.Node.Face;

            sideEdges = r.SideEdges;
            sideVertices = r.SideVertices;
            totalVertices = sideVertices * sideVertices;
            edgeLength = r.Node.Size / sideEdges;
            faceCenter = r.Node.FaceCenter;// - (Vector2.one * (r.Node.Size / 2));
            halfSize = r.Node.Size / 2;

            resultVertices = r.ResultVertices;
            SetHeightmapData();
        }

        public void Finish( LODQuadRebuildData r )
        {
        }

        public void Dispose()
        {
        }

        public ILODQuadJob Clone()
        {
            return new HeightMap_Job()
            {
                HeightmapXn = HeightmapXn,
                HeightmapXp = HeightmapXp,
                HeightmapYn = HeightmapYn,
                HeightmapYp = HeightmapYp,
                HeightmapZn = HeightmapZn,
                HeightmapZp = HeightmapZp,
                MinLevel = MinLevel,
                MaxLevel = MaxLevel,
            };
        }

        public void SetHeightmapData()
        {
            if( _heightmapXn.format == TextureFormat.R16 )
                _heightmapArray16bitXn = _heightmapXn.GetPixelData<short>( 0 );
            if( _heightmapXp.format == TextureFormat.R16 )
                _heightmapArray16bitXp = _heightmapXp.GetPixelData<short>( 0 );
            if( _heightmapYn.format == TextureFormat.R16 )
                _heightmapArray16bitYn = _heightmapYn.GetPixelData<short>( 0 );
            if( _heightmapYp.format == TextureFormat.R16 )
                _heightmapArray16bitYp = _heightmapYp.GetPixelData<short>( 0 );
            if( _heightmapZn.format == TextureFormat.R16 )
                _heightmapArray16bitZn = _heightmapZn.GetPixelData<short>( 0 );
            if( _heightmapZp.format == TextureFormat.R16 )
                _heightmapArray16bitZp = _heightmapZp.GetPixelData<short>( 0 );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        int GetIndex( int x, int y )
        {
            return (x * sideEdges) + x + y;
        }
        double lerp( double a, double b, double f )
        {
            return a + f * (b - a);
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private double SampleHeightmap( double x, double y )
        {
            //if( this.face == Direction3D.Xn )
            {
                double halfRes = _widthXn * 0.5;
                double u = (x + 1) * halfRes - 0.5f;
                double v = (y + 1) * halfRes - 0.5f;

                int x0 = Mathf.Clamp( (int)Math.Floor( u ), 0, _widthXn - 1 );
                int y0 = Mathf.Clamp( (int)Math.Floor( v ), 0, _widthXn - 1 );

                int x1 = Mathf.Clamp( x0 + 1, 0, _widthXn - 1 );
                int y1 = Mathf.Clamp( y0 + 1, 0, _widthXn - 1 );

                short h00 = _heightmapArray16bitXn[y0 * _widthXn + x0];
                short h10 = _heightmapArray16bitXn[y0 * _widthXn + x1];
                short h01 = _heightmapArray16bitXn[y1 * _widthXn + x0];
                short h11 = _heightmapArray16bitXn[y1 * _widthXn + x1];

                double dx = u - x0;
                double dy = v - y0;

                double h0 = lerp( h00, h10, dx );
                double h1 = lerp( h01, h11, dx );
                double interpolatedHeight = lerp( h0, h1, dy );

                return ((interpolatedHeight / 32767.0) * (MaxLevel - MinLevel)) + MinLevel;



                int xn = Mathf.Clamp( (int)Math.Round( (x + 1) * halfRes ), 0, _widthXn - 1 );
                int yn = Mathf.Clamp( (int)Math.Round( (y + 1) * halfRes ), 0, _widthXn - 1 );

                // int xn, xp, yn, yp; // sample indices
                short pointHeight = _heightmapArray16bitXn[yn * _widthXn + xn];
                return ((pointHeight / 32767.0) * (MaxLevel - MinLevel)) + MinLevel;
            }
        }

        public void Execute()
        {
            for( int x = 0; x < sideVertices; x++ )
            {
                for( int y = 0; y < sideVertices; y++ )
                {
                    int index = GetIndex( x, y );

                    Vector3Dbl dir = resultVertices[index].normalized;


                    double ypx = (x * edgeLength) + faceCenter.x - halfSize;
                    double ypy = (y * edgeLength) + faceCenter.y - halfSize;
                    Vector3Dbl offset = dir * SampleHeightmap( ypx, ypy );

                    resultVertices[index] += offset;
                }
            }
        }
    }
}