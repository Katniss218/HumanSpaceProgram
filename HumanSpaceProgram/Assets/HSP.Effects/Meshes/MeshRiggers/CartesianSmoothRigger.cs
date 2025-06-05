using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Effects.Meshes.MeshRiggers
{
    /// <summary>
    /// Rigs the mesh using an easing function to smoothly interpolate between start/end distances. <br/>
    /// The distance between each vertex and bones is calculated using the cartesian straight-line metric.
    /// </summary>
    public class CartesianSmoothRigger : IMeshRigger
    {
        /// <summary>
        /// The maximum number of bones that a vertex will be affected by. <br/>
        /// Each vertex will be affected by that many nearest bones.
        /// </summary>
        public int InfluenceCount { get; set; } = 2;

        /// <summary>
        /// The distance where the bone influence starts to fall off from 1 to 0. <br/>
        /// </summary>
        public float FalloffStartDistance { get; set; } = 0.5f;

        /// <summary>
        /// The distance where the bone influence reaches 0. <br/>
        /// </summary>
        public float FalloffEndDistance { get; set; } = 1.0f;

        /// <summary>
        /// Calculates the bone positions of the given bones relative to the given parent transform.
        /// </summary>
        public Vector3[] GetRelativePositions( IReadOnlyList<BindPose> bones )
        {
            if( bones == null )
                throw new ArgumentNullException( nameof( bones ) );

            Vector3[] relativePositions = new Vector3[bones.Count];

            for( int i = 0; i < bones.Count; i++ )
                relativePositions[i] = bones[i].Position;

            return relativePositions;
        }

        /// <summary>
        /// Rigs the mesh, replacing its bone weights (if any exist) with the new values.
        /// </summary>
        public void RigInPlace( Mesh mesh, IReadOnlyList<BindPose> bones )
        {
            int vertexCount = mesh.vertexCount;
            byte maxInfluences = (byte)Math.Min( InfluenceCount, bones.Count );

            NativeArray<byte> bonesPerVertex = new NativeArray<byte>( vertexCount, Allocator.Temp );
            NativeArray<BoneWeight1> boneWeights = new NativeArray<BoneWeight1>( vertexCount * maxInfluences, Allocator.Temp ); // can be optimized by removing bone influences with weight of 0, but requires iterating twice.

            // bone weights are ordered by vertex, each vertex has a number of weights specified by bonesPerVertex, and the next chunk of weights is for the next vertex.

            Vector3[] bonePositions = GetRelativePositions( bones );

            Vector3[] vertices = mesh.vertices;
            int weightIndex = 0;

            for( int vi = 0; vi < vertexCount; vi++ )
            {
                Vector3 vertPos = vertices[vi];

                // Find distances to all bones
                List<(int boneIndex, float distance)> distances = new List<(int, float)>( bones.Count );
                for( int bi = 0; bi < bones.Count; bi++ )
                {
                    float dist = Vector3.Distance( vertPos, bonePositions[bi] );
                    distances.Add( (bi, dist) );
                }

                distances.Sort( ( a, b ) => a.distance.CompareTo( b.distance ) );

                float[] rawWeights = new float[maxInfluences];
                float totalWeight = 0.0f;

                // Compute weights for n closest bones.
                for( int j = 0; j < maxInfluences; j++ )
                {
                    float d = distances[j].distance;
                    float weight;

                    if( d < FalloffStartDistance )
                    {
                        weight = 1.0f - d / FalloffStartDistance;
                    }
                    else if( d > FalloffEndDistance )
                    {
                        weight = 0.0f;
                    }
                    else
                    {
                        float t = (d - FalloffStartDistance) / (FalloffEndDistance - FalloffStartDistance);
                        weight = Mathf.SmoothStep( 1.0f, 0.0f, t );
                    }

                    rawWeights[j] = weight;
                    totalWeight += weight;
                }

                // Normalize
                if( totalWeight > 0.0f )
                {
                    for( int i = 0; i < maxInfluences; i++ )
                    {
                        rawWeights[i] /= totalWeight;
                    }
                }

                bonesPerVertex[vi] = maxInfluences;

                for( int i = 0; i < maxInfluences; i++ )
                {
                    var bw = new BoneWeight1()
                    {
                        boneIndex = distances[i].boneIndex,
                        weight = rawWeights[i]
                    };
                    boneWeights[weightIndex++] = bw;
                }
            }

            // Calculate the Unity bindpose matrices.
            // They are what tells the renderer where the bones are at no deformation (base state).
            // - should be relative to the root bone.
            Matrix4x4[] bindposes = new Matrix4x4[bones.Count];
            for( int i = 0; i < bones.Count; i++ )
            {
                bindposes[i] = Matrix4x4.TRS( bones[i].Position, bones[i].Rotation, bones[i].Scale ).inverse;
            }

            mesh.SetBoneWeights( bonesPerVertex, boneWeights );
            bonesPerVertex.Dispose();
            boneWeights.Dispose();
            mesh.bindposes = bindposes;
        }

        /// <summary>
        /// Creates an exact copy of the mesh and rigs it with the given bones.
        /// </summary>
        public Mesh RigCopy( Mesh mesh, IReadOnlyList<BindPose> bones )
        {
            Mesh newMesh = mesh.GetDeepCopy();

            RigInPlace( newMesh, bones );

            return newMesh;
        }


        [MapsInheritingFrom( typeof( CartesianSmoothRigger ) )]
        public static SerializationMapping ClosestWithFalloffMeshRiggerMapping()
        {
            return new MemberwiseSerializationMapping<CartesianSmoothRigger>()
                .WithMember( "influence_count", o => o.InfluenceCount )
                .WithMember( "falloff_start_distance", o => o.FalloffStartDistance )
                .WithMember( "falloff_end_distance", o => o.FalloffEndDistance );
        }
    }
}