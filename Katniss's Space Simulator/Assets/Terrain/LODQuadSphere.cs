using KatnisssSpaceSimulator.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Terrain
{
    /// <summary>
    /// Wraps around 6 faces of a sphere.
    /// </summary>
    [RequireComponent( typeof( CelestialBody ) )]
    public class LODQuadSphere : MonoBehaviour
    {
        public int DefaultSubdivisions { get; private set; } = 7;

        LODQuad[] _l0faces;
        CelestialBody _celestialBody;

        void Awake()
        {
            // Possibly move this to a child, so it can be disabled without disabling entire CB.
            _celestialBody = this.GetComponent<CelestialBody>();
        }

        void Start()
        {
            _l0faces = new LODQuad[6];
            for( int i = 0; i < 6; i++ )
            {
                CreateMeshPart( _celestialBody.transform, (QuadSphereFace)i );
            }
        }

        private LODQuad CreateMeshPart( Transform cbTransform, QuadSphereFace face )
        {
            GameObject go = new GameObject( "mesh" );
            go.transform.SetParent( cbTransform );
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.material = FindObjectOfType<zzzTestGameManager>().CBMaterial;

            go.AddComponent<MeshCollider>();

            Vector3 dir = face.ToVector3();
            Vector3Dbl offset = ((Vector3Dbl)dir) * _celestialBody.Radius;

            LODQuad q = go.AddComponent<LODQuad>();
            q.SetL0( offset, DefaultSubdivisions, _celestialBody.Radius );
            _l0faces[(int)face] = q;

            return q;
        }
    }
}
