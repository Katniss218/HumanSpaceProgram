using System;
using UnityEngine;

namespace HSP.CelestialBodies.Surfaces
{
    /// <summary>
    /// The front end for <see cref="LODQuadTreeNode"/>.
    /// </summary>
    [RequireComponent( typeof( MeshFilter ) )]
    public class LODQuad : MonoBehaviour
    {
        public LODQuadSphere QuadSphere { get; private set; }

        public LODQuadMode Mode => QuadSphere.Mode;

        public LODQuadTreeNode Node { get; private set; }

        public CelestialBody CelestialBody { get; private set; }

        MeshFilter _meshFilter;
        MeshCollider _meshCollider;
        MeshRenderer _meshRenderer;

        void Awake()
        {
            _meshFilter = this.GetComponent<MeshFilter>();
            _meshCollider = this.GetComponent<MeshCollider>();
            _meshRenderer = this.GetComponent<MeshRenderer>();
        }

        public void Activate()
        {
            this.gameObject.SetActive( true );
        }

        public void Deactivate()
        {
            this.gameObject.SetActive( false );
        }

        public static LODQuad CreateInactive( LODQuadSphere sphere, LODQuadTreeNode node, Mesh mesh )
        {
            GameObject gameObject = new GameObject( $"LODQuad L{node.SubdivisionLevel}, {node.Face}, ({node.FaceCenter.x:#0.################}, {node.FaceCenter.y:#0.################})" );
            gameObject.transform.SetParent( sphere.QuadParent );

            if( (sphere.Mode & LODQuadMode.Visual) == LODQuadMode.Visual )
            {
                MeshFilter mf = gameObject.AddComponent<MeshFilter>();
                mf.sharedMesh = mesh;

                MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
                //mr.material = mat;
            }

            if( (sphere.Mode & LODQuadMode.Collider) == LODQuadMode.Collider )
            {
                MeshCollider mc = gameObject.AddComponent<MeshCollider>();
                mc.sharedMesh = mesh;
            }

            LODQuad lodQuad = gameObject.AddComponent<LODQuad>();
            lodQuad.transform.localPosition = (Vector3)(node.SphereCenter * sphere.CelestialBody.Radius);
            lodQuad.transform.localRotation = Quaternion.identity;
            lodQuad.transform.localScale = Vector3.one;

            lodQuad.Node = node;
            lodQuad.QuadSphere = sphere;
            lodQuad.CelestialBody = sphere.CelestialBody;

            gameObject.SetActive( false );

            return lodQuad;
        }
    }
}