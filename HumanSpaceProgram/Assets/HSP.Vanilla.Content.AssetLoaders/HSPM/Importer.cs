using System;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace HSP.Vanilla.Content.AssetLoaders.HSPM
{
    public static class Importer
    {
        // Signature "HSPM"
        private const uint SIGNATURE = 0x4D505348;

        [Flags]
        private enum MeshFlags : byte
        {
            None = 0,
            HasColors = 1 << 0,
            HasUV2 = 1 << 1,
            HasSkinning = 1 << 2
        }

        public static Mesh Load( string filePath )
        {
            using FileStream fs = new FileStream( filePath, FileMode.Open, FileAccess.Read );
            return Load( fs, Path.GetFileNameWithoutExtension( filePath ) );
        }

        public static Mesh Load( Stream stream, string name )
        {
            using BinaryReader br = new BinaryReader( stream, System.Text.Encoding.Default, true );

            uint signature = br.ReadUInt32();
            if( signature != SIGNATURE )
                throw new IOException( $"Invalid HSPM signature." );

            uint version = br.ReadUInt32();
            if( version != 1 )
                throw new IOException( $"Unsupported HSPM version: {version}" );

            uint vertexCount = br.ReadUInt32();
            uint submeshCount = br.ReadUInt32();
            MeshFlags flags = (MeshFlags)br.ReadByte();

            Mesh mesh = new Mesh();
            mesh.name = name;

            if( vertexCount > 65535 )
                mesh.indexFormat = IndexFormat.UInt32;

            Vector3[] positions = new Vector3[vertexCount];
            for( int i = 0; i < vertexCount; i++ )
                positions[i] = new Vector3( br.ReadSingle(), br.ReadSingle(), br.ReadSingle() );
            mesh.SetVertices( positions );

            Vector3[] normals = new Vector3[vertexCount];
            for( int i = 0; i < vertexCount; i++ )
                normals[i] = new Vector3( br.ReadSingle(), br.ReadSingle(), br.ReadSingle() );
            mesh.SetNormals( normals );

            Vector4[] tangents = new Vector4[vertexCount];
            for( int i = 0; i < vertexCount; i++ )
                tangents[i] = new Vector4( br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle() );
            mesh.SetTangents( tangents );

            Vector2[] uvs = new Vector2[vertexCount];
            for( int i = 0; i < vertexCount; i++ )
                uvs[i] = new Vector2( br.ReadSingle(), br.ReadSingle() );
            mesh.SetUVs( 0, uvs );

            if( flags.HasFlag( MeshFlags.HasColors ) )
            {
                Color32[] colors = new Color32[vertexCount];
                for( int i = 0; i < vertexCount; i++ )
                    colors[i] = new Color32( br.ReadByte(), br.ReadByte(), br.ReadByte(), br.ReadByte() );
                mesh.SetColors( colors );
            }

            if( flags.HasFlag( MeshFlags.HasUV2 ) )
            {
                Vector2[] uv2 = new Vector2[vertexCount];
                for( int i = 0; i < vertexCount; i++ )
                    uv2[i] = new Vector2( br.ReadSingle(), br.ReadSingle() );
                mesh.SetUVs( 1, uv2 );
            }

            if( flags.HasFlag( MeshFlags.HasSkinning ) )
            {
                BoneWeight[] boneWeights = new BoneWeight[vertexCount];

                float[,] weights = new float[vertexCount, 4];
                for( int i = 0; i < vertexCount; i++ )
                {
                    weights[i, 0] = br.ReadSingle();
                    weights[i, 1] = br.ReadSingle();
                    weights[i, 2] = br.ReadSingle();
                    weights[i, 3] = br.ReadSingle();
                }

                int[,] indices = new int[vertexCount, 4];
                for( int i = 0; i < vertexCount; i++ )
                {
                    indices[i, 0] = br.ReadInt32();
                    indices[i, 1] = br.ReadInt32();
                    indices[i, 2] = br.ReadInt32();
                    indices[i, 3] = br.ReadInt32();
                }

                for( int i = 0; i < vertexCount; i++ )
                {
                    boneWeights[i].weight0 = weights[i, 0];
                    boneWeights[i].weight1 = weights[i, 1];
                    boneWeights[i].weight2 = weights[i, 2];
                    boneWeights[i].weight3 = weights[i, 3];
                    boneWeights[i].boneIndex0 = indices[i, 0];
                    boneWeights[i].boneIndex1 = indices[i, 1];
                    boneWeights[i].boneIndex2 = indices[i, 2];
                    boneWeights[i].boneIndex3 = indices[i, 3];
                }
                mesh.boneWeights = boneWeights;

                uint boneCount = br.ReadUInt32();
                Matrix4x4[] bindPoses = new Matrix4x4[boneCount];
                for( int i = 0; i < boneCount; i++ )
                {
                    Matrix4x4 m = new Matrix4x4();
                    for( int k = 0; k < 16; k++ )
                        m[k] = br.ReadSingle();
                    bindPoses[i] = m;
                }
                mesh.bindposes = bindPoses;
            }

            mesh.subMeshCount = (int)submeshCount;
            for( int i = 0; i < submeshCount; i++ )
            {
                uint indexCount = br.ReadUInt32();
                uint topology = br.ReadUInt32();

                int[] indices = new int[indexCount];
                bool use32Bit = vertexCount > 65535;

                for( int k = 0; k < indexCount; k++ )
                {
                    indices[k] = use32Bit ? (int)br.ReadUInt32() : (int)br.ReadUInt16();
                }

                MeshTopology meshTopo = MeshTopology.Triangles;
                switch( topology )
                {
                    case 0: meshTopo = MeshTopology.Triangles;
                        break;
                    case 1: meshTopo = MeshTopology.Quads;
                        break;
                    case 2: meshTopo = MeshTopology.Lines;
                        break;
                }

                mesh.SetIndices( indices, meshTopo, i );
            }

            mesh.RecalculateBounds();
            mesh.UploadMeshData( true );

            return mesh;
        }
    }
}