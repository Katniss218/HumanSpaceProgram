using HSP.ReferenceFrames;
using System;
using UnityEngine;
using UnityPlus.AssetManagement;

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

        /// <summary>
        /// Checks if the quad is active (visible/enabled).
        /// </summary>
        /// <remarks>
        /// If the quad is not active, it usually means that it's subdivided, and its child quads are active instead.
        /// </remarks>
        public bool IsActive => this.gameObject.activeSelf;

        void Awake()
        {
            _meshFilter = this.GetComponent<MeshFilter>();
            _meshCollider = this.GetComponent<MeshCollider>();
            _meshRenderer = this.GetComponent<MeshRenderer>();
        }

        void FixedUpdate()
        {
            Vector3Dbl relativePos = Node.SphereCenter * QuadSphere.CelestialBody.Radius;
            var refFrame = QuadSphere.CelestialBody.ReferenceFrameTransform.OrientedInertialReferenceFrame();
            Vector3Dbl airfPos = refFrame.TransformPosition( relativePos );
            QuaternionDbl airfRot = refFrame.TransformRotation( QuaternionDbl.identity );
            Vector3 scenePos = (Vector3)SceneReferenceFrameManager.ReferenceFrame.InverseTransformPosition( airfPos );
            Quaternion sceneRot = (Quaternion)SceneReferenceFrameManager.ReferenceFrame.InverseTransformRotation( airfRot );
            this.transform.SetPositionAndRotation( scenePos, sceneRot );
        }

        void OnDestroy()
        {
            Destroy( _mesh ); // Destroying the mesh prevents a memory leak (One would think that a mesh would have a destructor to handle it, but I guess not).
        }

        public void Activate()
        {
            this.gameObject.SetActive( true );

            Vector3Dbl relativePos = Node.SphereCenter * QuadSphere.CelestialBody.Radius;
            var refFrame = QuadSphere.CelestialBody.ReferenceFrameTransform.OrientedInertialReferenceFrame();
            Vector3Dbl airfPos = refFrame.TransformPosition( relativePos );
            QuaternionDbl airfRot = refFrame.TransformRotation( QuaternionDbl.identity );
            Vector3 scenePos = (Vector3)SceneReferenceFrameManager.ReferenceFrame.InverseTransformPosition( airfPos );
            Quaternion sceneRot = (Quaternion)SceneReferenceFrameManager.ReferenceFrame.InverseTransformRotation( airfRot );
            this.transform.SetPositionAndRotation( scenePos, sceneRot );
        }

        public void Deactivate()
        {
            this.gameObject.SetActive( false );
        }

        public static LODQuad CreateInactive( LODQuadSphere sphere, LODQuadTreeNode node, Mesh mesh )
        {
            GameObject gameObject = new GameObject( $"LODQuad {node.Face}, L{node.SubdivisionLevel}, ({node.FaceCenter.x:#0.################}, {node.FaceCenter.y:#0.################})" );
            gameObject.transform.SetParent( sphere.QuadParent );

            LODQuad lodQuad = gameObject.AddComponent<LODQuad>();
            lodQuad._mesh = mesh;
            lodQuad.transform.localPosition = (Vector3)(node.SphereCenter * sphere.CelestialBody.Radius);
            lodQuad.transform.localRotation = Quaternion.identity;
            lodQuad.transform.localScale = Vector3.one;

            lodQuad.Node = node;
            lodQuad.QuadSphere = sphere;

            if( (sphere.Mode & LODQuadMode.Visual) == LODQuadMode.Visual )
            {
                MeshFilter mf = gameObject.AddComponent<MeshFilter>();
                mf.sharedMesh = mesh;

#warning TODO - add proper way to get textures/materials
                MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
                mr.material = AssetRegistry.Get<Material>( $"Vanilla::CBMATERIAL{(int)node.Face}" );
            }

            if( (sphere.Mode & LODQuadMode.Collider) == LODQuadMode.Collider )
            {
                MeshCollider mc = gameObject.AddComponent<MeshCollider>();
                mc.sharedMesh = mesh;
            }

            lodQuad.Deactivate();

            return lodQuad;
        }
    }
}