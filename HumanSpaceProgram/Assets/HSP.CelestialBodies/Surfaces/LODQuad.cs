using UnityEngine;

namespace HSP.CelestialBodies.Surfaces
{
    /// <summary>
    /// The front end for <see cref="LODQuadTreeNode"/>.
    /// </summary>
    [RequireComponent( typeof( MeshFilter ) )]
    //[RequireComponent( typeof( MeshCollider ) )]
    //[RequireComponent( typeof( MeshRenderer ) )]
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
    }
}