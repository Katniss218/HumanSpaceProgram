using System;
using System.IO;
using UnityEngine;

namespace HSP.Vanilla.Content.AssetLoaders.HSPM
{
    public static class Exporter
    {
        private const uint SIGNATURE = 0x4D505348; // "HSPM"

        [Flags]
        private enum MeshFlags : byte
        {
            None = 0,
            HasColors = 1 << 0,
            HasUV2 = 1 << 1,
            HasSkinning = 1 << 2
        }

        public static void Export( string filePath, Mesh mesh )
        {
            if( mesh == null )
                throw new ArgumentNullException( nameof( mesh ) );

            // Ensure directory exists
            string dir = Path.GetDirectoryName( filePath );
            if( !string.IsNullOrEmpty( dir ) && !Directory.Exists( dir ) )
                Directory.CreateDirectory( dir );

            using FileStream fs = new FileStream( filePath, FileMode.Create, FileAccess.Write );
            using BinaryWriter bw = new BinaryWriter( fs );

            // --- Header ---
            bw.Write( SIGNATURE );
            bw.Write( (uint)1 ); // Version

            uint vertexCount = (uint)mesh.vertexCount;
            bw.Write( vertexCount );

            uint submeshCount = (uint)mesh.subMeshCount;
            bw.Write( submeshCount );

            // Detect Flags
            MeshFlags flags = MeshFlags.None;
            if( mesh.colors32 != null && mesh.colors32.Length == vertexCount )
                flags |= MeshFlags.HasColors;
            if( mesh.uv2 != null && mesh.uv2.Length == vertexCount )
                flags |= MeshFlags.HasUV2;
            if( mesh.boneWeights != null && mesh.boneWeights.Length == vertexCount && mesh.bindposes != null && mesh.bindposes.Length > 0 )
                flags |= MeshFlags.HasSkinning;

            bw.Write( (byte)flags );

            // 1. Positions
            Vector3[] vertices = mesh.vertices;
            for( int i = 0; i < vertexCount; i++ )
            {
                bw.Write( vertices[i].x );
                bw.Write( vertices[i].y );
                bw.Write( vertices[i].z );
            }

            // 2. Normals (Ensure they exist or write zero)
            Vector3[] normals = mesh.normals;
            if( normals.Length != vertexCount ) normals = new Vector3[vertexCount];
            for( int i = 0; i < vertexCount; i++ )
            {
                bw.Write( normals[i].x );
                bw.Write( normals[i].y );
                bw.Write( normals[i].z );
            }

            // 3. Tangents
            Vector4[] tangents = mesh.tangents;
            if( tangents.Length != vertexCount ) tangents = new Vector4[vertexCount];
            for( int i = 0; i < vertexCount; i++ )
            {
                bw.Write( tangents[i].x );
                bw.Write( tangents[i].y );
                bw.Write( tangents[i].z );
                bw.Write( tangents[i].w );
            }

            // 4. UVs (0)
            Vector2[] uvs = mesh.uv;
            if( uvs.Length != vertexCount ) uvs = new Vector2[vertexCount];
            for( int i = 0; i < vertexCount; i++ )
            {
                bw.Write( uvs[i].x );
                bw.Write( uvs[i].y );
            }

            // 5. Colors (Optional)
            if( flags.HasFlag( MeshFlags.HasColors ) )
            {
                Color32[] colors = mesh.colors32;
                for( int i = 0; i < vertexCount; i++ )
                {
                    bw.Write( colors[i].r );
                    bw.Write( colors[i].g );
                    bw.Write( colors[i].b );
                    bw.Write( colors[i].a );
                }
            }

            // 5b. UV2 (Optional)
            if( flags.HasFlag( MeshFlags.HasUV2 ) )
            {
                Vector2[] uv2 = mesh.uv2;
                for( int i = 0; i < vertexCount; i++ )
                {
                    bw.Write( uv2[i].x );
                    bw.Write( uv2[i].y );
                }
            }

            // 6. BoneWeights & 7. BindPoses (Optional)
            if( flags.HasFlag( MeshFlags.HasSkinning ) )
            {
                BoneWeight[] boneWeights = mesh.boneWeights;

                // Write weights interleaved: float[VertexCount][4]
                for( int i = 0; i < vertexCount; i++ )
                {
                    bw.Write( boneWeights[i].weight0 );
                    bw.Write( boneWeights[i].weight1 );
                    bw.Write( boneWeights[i].weight2 );
                    bw.Write( boneWeights[i].weight3 );
                }

                // Write indices interleaved: int[VertexCount][4]
                for( int i = 0; i < vertexCount; i++ )
                {
                    bw.Write( boneWeights[i].boneIndex0 );
                    bw.Write( boneWeights[i].boneIndex1 );
                    bw.Write( boneWeights[i].boneIndex2 );
                    bw.Write( boneWeights[i].boneIndex3 );
                }

                // BindPoses
                Matrix4x4[] bindPoses = mesh.bindposes;
                bw.Write( (uint)bindPoses.Length );
                for( int i = 0; i < bindPoses.Length; i++ )
                {
                    // Write 16 floats
                    for( int k = 0; k < 16; k++ )
                    {
                        bw.Write( bindPoses[i][k] );
                    }
                }
            }

            // Submeshes
            for( int i = 0; i < submeshCount; i++ )
            {
                // Note: GetIndices handles triangles/quads/lines automatically as linear list
                int[] indices = mesh.GetIndices( i );

                bw.Write( (uint)indices.Length );

                uint topology = 0;
                MeshTopology mt = mesh.GetTopology( i );
                switch( mt )
                {
                    case MeshTopology.Triangles: topology = 0;
                        break;
                    case MeshTopology.Quads: topology = 1;
                        break;
                    case MeshTopology.Lines: topology = 2;
                        break;
                    default:
                        Debug.LogWarning( $"HSPM Exporter: Unsupported topology {mt}, defaulting to Triangles" );
                        topology = 0;
                        break;
                }
                bw.Write( topology );

                bool use32Bit = vertexCount > 65535;

                for( int k = 0; k < indices.Length; k++ )
                {
                    if( use32Bit )
                        bw.Write( (uint)indices[k] );
                    else
                        bw.Write( (ushort)indices[k] );
                }
            }
        }
    }
}