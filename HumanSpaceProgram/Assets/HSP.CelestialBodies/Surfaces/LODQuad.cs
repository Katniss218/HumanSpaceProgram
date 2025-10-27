using HSP.ReferenceFrames;
using System;
using UnityEngine;

namespace HSP.CelestialBodies.Surfaces
{
    /// <summary>
    /// The front end for <see cref="LODQuadTreeNode"/>.
    /// A single quad of a LOD sphere.
    /// </summary>
    public sealed class LODQuad : MonoBehaviour
    {
        /// <summary>
        /// The LOD sphere that this quad belongs to.
        /// </summary>
        public LODQuadSphere QuadSphere { get; private set; }

        /// <summary>
        /// The backend node that this quad is associated with.
        /// </summary>
        public LODQuadTreeNode Node { get; private set; }

        Mesh _mesh;

        MeshFilter _meshFilter;
        MeshCollider _meshCollider;
        MeshRenderer _meshRenderer;

        public void ResetMaterial()
        {
            if( this._meshRenderer != null )
            {
                this._meshRenderer.sharedMaterial = this.QuadSphere.Materials?[(int)this.Node.Face];
            }
        }

        /// <summary>
        /// Checks if the quad is active (visible/enabled).
        /// </summary>
        /// <remarks>
        /// If the quad is not active, it usually means that it's subdivided, and its child quads are active instead.
        /// </remarks>
        public bool IsActive => this.gameObject.activeSelf;

        void OnDestroy()
        {
            Destroy( _mesh ); // Destroying the mesh prevents a memory leak (One would think that a mesh would have a destructor to handle it, but I guess not).
        }

        public void Activate()
        {
            this.gameObject.SetActive( true );
            ResetPositionAndRotation();
        }

        public void Deactivate()
        {
            this.gameObject.SetActive( false );
        }

        public void ResetPositionAndRotation()
        {
            var refTransform = QuadSphere.CelestialBody.ReferenceFrameTransform;
            IReferenceFrame bodyReferenceFrame = refTransform.OrientedInertialReferenceFrame();
            IReferenceFrame sceneReferenceFrame = refTransform.SceneReferenceFrameProvider.GetSceneReferenceFrame();

            ResetPositionAndRotationImpl( bodyReferenceFrame, sceneReferenceFrame );
        }

        public static void ResetPositionAndRotationAll( LODQuadSphere quadSphere )
        {
            if( quadSphere == null )
                throw new System.ArgumentNullException( nameof( quadSphere ) );

            var refTransform = quadSphere.CelestialBody.ReferenceFrameTransform;
            IReferenceFrame bodyReferenceFrame = refTransform.OrientedInertialReferenceFrame();
            IReferenceFrame sceneReferenceFrame = refTransform.SceneReferenceFrameProvider.GetSceneReferenceFrame();

            foreach( var node in quadSphere.CurrentQuads.Values )
            {
                node.Quad.ResetPositionAndRotationImpl( bodyReferenceFrame, sceneReferenceFrame );
            }
        }

        private void ResetPositionAndRotationImpl( IReferenceFrame bodyReferenceFrame, IReferenceFrame sceneReferenceFrame )
        {
            Vector3Dbl absolutePos = bodyReferenceFrame.TransformPosition( Node.SphereCenter * QuadSphere.CelestialBody.Radius );
            QuaternionDbl absoluteRot = bodyReferenceFrame.TransformRotation( QuaternionDbl.identity );

            Vector3 scenePos = (Vector3)sceneReferenceFrame.InverseTransformPosition( absolutePos );
            Quaternion sceneRot = (Quaternion)sceneReferenceFrame.InverseTransformRotation( absoluteRot );

            this.transform.SetPositionAndRotation( scenePos, sceneRot );
        }

        public static LODQuad CreateInactive( LODQuadSphere sphere, LODQuadTreeNode node, Mesh mesh )
        {
            if( sphere.QuadParent == null )
                throw new InvalidOperationException( $"Can't create a {nameof( LODQuad )} - sphere parent is null." );

            GameObject gameObject = new GameObject( $"{nameof( LODQuad )} {node.Face}, L{node.SubdivisionLevel}, ({node.FaceCenter.x:#0.################}, {node.FaceCenter.y:#0.################})" );
            gameObject.transform.localPosition = (Vector3)(node.SphereCenter * sphere.CelestialBody.Radius);
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = Vector3.one;
            gameObject.transform.SetParent( sphere.QuadParent, false );
            gameObject.SetLayer( (int)sphere.Layer );
            gameObject.SetActive( false );

            LODQuad lodQuad = gameObject.AddComponent<LODQuad>();
            lodQuad._mesh = mesh;

            lodQuad.Node = node;
            lodQuad.QuadSphere = sphere;

            if( (sphere.Mode & LODQuadMode.Visual) == LODQuadMode.Visual )
            {
                lodQuad._meshFilter = gameObject.AddComponent<MeshFilter>();
                lodQuad._meshFilter.sharedMesh = mesh;

                lodQuad._meshRenderer = gameObject.AddComponent<MeshRenderer>();
                lodQuad.ResetMaterial();
            }

            if( (sphere.Mode & LODQuadMode.Collider) == LODQuadMode.Collider )
            {
                lodQuad._meshCollider = gameObject.AddComponent<MeshCollider>();
                lodQuad._meshCollider.sharedMesh = mesh;
            }

            return lodQuad;
        }
    }
}