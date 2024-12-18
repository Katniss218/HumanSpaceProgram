using HSP.CelestialBodies.Surfaces;
using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;

namespace HSP.Vanilla.CelestialBodies
{
    public class LODQuadModifier_Heightmap : ILODQuadModifier
    {
        public LODQuadMode QuadMode => LODQuadMode.VisualAndCollider;

        public Texture2D HeightmapXn { get; set; }

        public Texture2D HeightmapXp { get; set; }

        public Texture2D HeightmapYn { get; set; }

        public Texture2D HeightmapYp { get; set; }

        public Texture2D HeightmapZn { get; set; }

        public Texture2D HeightmapZp { get; set; }

        public double MinLevel { get; set; }

        public double MaxLevel { get; set; }

        public ILODQuadJob GetJob()
        {
            return new Job( this );
        }

        /// <summary>
        /// Displaces the vertices along the direction from the center of the body.
        /// </summary>
        public struct Job : ILODQuadJob
        {
            [ReadOnly]
            NativeArray<ushort> _heightmapArray16bitXn;
            [ReadOnly]
            NativeArray<ushort> _heightmapArray16bitXp;
            [ReadOnly]
            NativeArray<ushort> _heightmapArray16bitYn;
            [ReadOnly]
            NativeArray<ushort> _heightmapArray16bitYp;
            [ReadOnly]
            NativeArray<ushort> _heightmapArray16bitZn;
            [ReadOnly]
            NativeArray<ushort> _heightmapArray16bitZp;

            int _widthXn, _heightXn;
            int _widthXp, _heightXp;
            int _widthYn, _heightYn;
            int _widthYp, _heightYp;
            int _widthZn, _heightZn;
            int _widthZp, _heightZp;
            double maxlevel;
            double minlevel;

            double radius;

            int totalVertices;
            Vector2 faceCenter;
            float edgeLength;
            float halfSize;

            int sideVertices;
            int sideEdges;
            Direction3D face;

            NativeArray<Vector3Dbl> resultVertices;

            public Job( LODQuadModifier_Heightmap modifier )
            {
                //if( modifier.HeightmapXn.format == TextureFormat.R16 )
                _heightmapArray16bitXn = modifier.HeightmapXn.GetPixelData<ushort>( 0 );
                //if( modifier.HeightmapXp.format == TextureFormat.R16 )
                _heightmapArray16bitXp = modifier.HeightmapXp.GetPixelData<ushort>( 0 );
                //if( modifier.HeightmapYn.format == TextureFormat.R16 )
                _heightmapArray16bitYn = modifier.HeightmapYn.GetPixelData<ushort>( 0 );
                //if( modifier.HeightmapYp.format == TextureFormat.R16 )
                _heightmapArray16bitYp = modifier.HeightmapYp.GetPixelData<ushort>( 0 );
                //if( modifier.HeightmapZn.format == TextureFormat.R16 )
                _heightmapArray16bitZn = modifier.HeightmapZn.GetPixelData<ushort>( 0 );
                //if( modifier.HeightmapZp.format == TextureFormat.R16 )
                _heightmapArray16bitZp = modifier.HeightmapZp.GetPixelData<ushort>( 0 );

                minlevel = modifier.MinLevel;
                maxlevel = modifier.MaxLevel;

                _widthXn = modifier.HeightmapXn.width;
                _widthXp = modifier.HeightmapXp.width;
                _widthYn = modifier.HeightmapYn.width;
                _widthYp = modifier.HeightmapYp.width;
                _widthZn = modifier.HeightmapZn.width;
                _widthZp = modifier.HeightmapZp.width;

                _heightXn = modifier.HeightmapXn.height;
                _heightXp = modifier.HeightmapXp.height;
                _heightYn = modifier.HeightmapYn.height;
                _heightYp = modifier.HeightmapYp.height;
                _heightZn = modifier.HeightmapZn.height;
                _heightZp = modifier.HeightmapZp.height;

                face = default;
                radius = default;
                halfSize = default;
                sideEdges = default;
                faceCenter = default;
                edgeLength = default;
                sideVertices = default;
                totalVertices = default;
                resultVertices = default;
            }

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
            }

            public void Finish( LODQuadRebuildData r )
            {
            }

            public void Dispose()
            {
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            private int GetIndex( int x, int y )
            {
                return (x * sideEdges) + x + y;
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            private double Lerp( double a, double b, double f )
            {
                return a + f * (b - a);
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            private double SampleHeightmapLinear( double x, double y )
            {
                double halfRes = _widthXn * 0.5;
                double u = (x + 1) * halfRes - 0.5f;
                double v = (y + 1) * halfRes - 0.5f;

                int x0 = Mathf.Clamp( (int)Math.Floor( u ), 0, _widthXn - 1 );
                int y0 = Mathf.Clamp( (int)Math.Floor( v ), 0, _widthXn - 1 );

                int x1 = Mathf.Clamp( x0 + 1, 0, _widthXn - 1 );
                int y1 = Mathf.Clamp( y0 + 1, 0, _widthXn - 1 );

                ushort h00, h01, h10, h11;
                switch( face )
                {
                    case Direction3D.Xn:
                        h00 = _heightmapArray16bitXn[y0 * _widthXn + x0];
                        h10 = _heightmapArray16bitXn[y0 * _widthXn + x1];
                        h01 = _heightmapArray16bitXn[y1 * _widthXn + x0];
                        h11 = _heightmapArray16bitXn[y1 * _widthXn + x1];
                        break;
                    case Direction3D.Xp:
                        h00 = _heightmapArray16bitXp[y0 * _widthXp + x0];
                        h10 = _heightmapArray16bitXp[y0 * _widthXp + x1];
                        h01 = _heightmapArray16bitXp[y1 * _widthXp + x0];
                        h11 = _heightmapArray16bitXp[y1 * _widthXp + x1];
                        break;
                    case Direction3D.Yn:
                        h00 = _heightmapArray16bitYn[y0 * _widthYn + x0];
                        h10 = _heightmapArray16bitYn[y0 * _widthYn + x1];
                        h01 = _heightmapArray16bitYn[y1 * _widthYn + x0];
                        h11 = _heightmapArray16bitYn[y1 * _widthYn + x1];
                        break;
                    case Direction3D.Yp:
                        h00 = _heightmapArray16bitYp[y0 * _widthYp + x0];
                        h10 = _heightmapArray16bitYp[y0 * _widthYp + x1];
                        h01 = _heightmapArray16bitYp[y1 * _widthYp + x0];
                        h11 = _heightmapArray16bitYp[y1 * _widthYp + x1];
                        break;
                    case Direction3D.Zn:
                        h00 = _heightmapArray16bitZn[y0 * _widthZn + x0];
                        h10 = _heightmapArray16bitZn[y0 * _widthZn + x1];
                        h01 = _heightmapArray16bitZn[y1 * _widthZn + x0];
                        h11 = _heightmapArray16bitZn[y1 * _widthZn + x1];
                        break;
                    case Direction3D.Zp:
                        h00 = _heightmapArray16bitZp[y0 * _widthZp + x0];
                        h10 = _heightmapArray16bitZp[y0 * _widthZp + x1];
                        h01 = _heightmapArray16bitZp[y1 * _widthZp + x0];
                        h11 = _heightmapArray16bitZp[y1 * _widthZp + x1];
                        break;
                    default:
                        throw new ArgumentException( $"invalid face {face}" );
                }

                double dx = u - x0;
                double dy = v - y0;

                double h0 = Lerp( h00, h10, dx );
                double h1 = Lerp( h01, h11, dx );
                double interpolatedHeight = Lerp( h0, h1, dy );

                return ((interpolatedHeight / 65535.0) * (maxlevel - minlevel)) + minlevel;
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            private double SampleHeightmapPoint( double x, double y )
            {
                double halfRes = _widthXn * 0.5;
                double u = (x + 1) * halfRes;
                double v = (y + 1) * halfRes;
                int x0 = Mathf.Clamp( (int)Math.Floor( u ), 0, _widthXn - 1 );
                int y0 = Mathf.Clamp( (int)Math.Floor( v ), 0, _widthXn - 1 );

                ushort pointHeight;
                switch( face )
                {
                    case Direction3D.Xn:
                        pointHeight = _heightmapArray16bitXn[y0 * _widthXn + x0];
                        break;
                    case Direction3D.Xp:
                        pointHeight = _heightmapArray16bitXp[y0 * _widthXp + x0];
                        break;
                    case Direction3D.Yn:
                        pointHeight = _heightmapArray16bitYn[y0 * _widthYn + x0];
                        break;
                    case Direction3D.Yp:
                        pointHeight = _heightmapArray16bitYp[y0 * _widthYp + x0];
                        break;
                    case Direction3D.Zn:
                        pointHeight = _heightmapArray16bitZn[y0 * _widthZn + x0];
                        break;
                    case Direction3D.Zp:
                        pointHeight = _heightmapArray16bitZp[y0 * _widthZp + x0];
                        break;
                    default:
                        throw new ArgumentException( $"invalid face {face}" );
                }

                return ((pointHeight / 65535.0) * (maxlevel - minlevel)) + minlevel;
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
                        //Vector3Dbl offset = dir * SampleHeightmapPoint( ypx, ypy );
                        Vector3Dbl offset = dir * SampleHeightmapLinear( ypx, ypy );

                        resultVertices[index] += offset;
                    }
                }
            }
        }
    }
}