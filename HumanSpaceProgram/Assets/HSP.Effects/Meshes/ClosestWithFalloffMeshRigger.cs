using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.Effects.Meshes
{
    /// <summary>
    /// Automatically rigs the bones.
    /// </summary>
    public class ClosestWithFalloffMeshRigger
    {
        public int MaxBonesPerVertex { get; set; } = 2;

        public float FalloffStartDistance { get; set; } = 0.5f;
        public float FalloffEndDistance { get; set; } = 1.0f;

        // rigs up to N closest bones per vertex
        // uses a linear value within the falloff distance, and a smoothstep value between the falloff start and end.

        /// <summary>
        /// Calculates the bone positions of the given bones relative to the given parent transform.
        /// </summary>
        public Vector3[] GetRelativePositions( Transform parent, List<Transform> bones )
        {
            if( parent == null )
                throw new ArgumentNullException( nameof( parent ) );
            if( bones == null )
                throw new ArgumentNullException( nameof( bones ) );

            Vector3[] relativePositions = new Vector3[bones.Count];

            for( int i = 0; i < bones.Count; i++ )
            {
                Transform bone = bones[i];
                if( bone == null )
                    throw new ArgumentNullException( nameof( bone ), $"One of the bone transforms (no. {i}) was null." );

                Matrix4x4 localToParent = bone.GetLocalToAncestorMatrix( parent );
                relativePositions[i] = localToParent.MultiplyPoint3x4( Vector3.zero );
            }

            return relativePositions;
        }

        /// <summary>
        /// Rigs the mesh, replacing its bone weights (if any exist) with the new values.
        /// </summary>
        public void RigInPlace( Mesh mesh, Vector3[] bonePositions )
        {
            throw new NotImplementedException();

            var boneCountPerVertex = mesh.GetBonesPerVertex();

            var boneWeights = mesh.GetAllBoneWeights();
            int vertexCount = mesh.vertexCount;

            int boneWeightIndex = 0;

            // Iterate over the vertices
            for( var vi = 0; vi < vertexCount; vi++ )
            {
                var totalWeight = 0f;
                var numberOfBonesForThisVertex = boneCountPerVertex[vi];
                Debug.Log( "This vertex has " + numberOfBonesForThisVertex + " bone influences" );

                // For each vertex, iterate over its BoneWeights
                for( var i = 0; i < numberOfBonesForThisVertex; i++ )
                {
                    var currentBoneWeight = boneWeights[boneWeightIndex];
                    totalWeight += currentBoneWeight.weight;
                    if( i > 0 )
                    {
                        Debug.Assert( boneWeights[boneWeightIndex - 1].weight >= currentBoneWeight.weight );
                    }
                    boneWeightIndex++;
                }
                Debug.Assert( Mathf.Approximately( 1f, totalWeight ) );
            }
        }

        /// <summary>
        /// Creates an exact copy of the mesh and rigs it with the given bones.
        /// </summary>
        public Mesh RigCopy( Mesh mesh, Vector3[] bonePositions )
        {
            Mesh newMesh = new Mesh();
            newMesh.name = mesh.name;

            throw new NotImplementedException();
        }
    }
}