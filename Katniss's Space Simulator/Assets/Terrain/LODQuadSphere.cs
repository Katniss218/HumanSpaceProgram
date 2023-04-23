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
        LODQuad[] _l0faces = new LODQuad[6];

        CelestialBody _celestialBody;

        public int DefaultSubdivisions { get; private set; } = 6;

        void Awake()
        {
            // Possibly move this to a child, so it can be disabled without disabling entire CB.
            _celestialBody = this.GetComponent<CelestialBody>();
        }

        void Start()
        {
            Vector3Dbl[] offsets = new Vector3Dbl[6]
            {
                new Vector3Dbl( _celestialBody.Radius, 0, 0 ),
                new Vector3Dbl( -_celestialBody.Radius, 0, 0 ),
                new Vector3Dbl( 0, _celestialBody.Radius, 0 ),
                new Vector3Dbl( 0, -_celestialBody.Radius, 0 ),
                new Vector3Dbl( 0, 0, _celestialBody.Radius ),
                new Vector3Dbl( 0, 0, -_celestialBody.Radius ),
            };

            for( int i = 0; i < 6; i++ )
            {
                GameObject quadGO = CreateMeshPart( _celestialBody.transform );
                quadGO.transform.localPosition = (Vector3)offsets[i];

                MeshRenderer mr = quadGO.AddComponent<MeshRenderer>();
                mr.material = FindObjectOfType<zzzTestGameManager>().CBMaterial;

                quadGO.AddComponent<MeshCollider>();

                LODQuad q = quadGO.AddComponent<LODQuad>();
                q.SetL0( offsets[i], DefaultSubdivisions, _celestialBody.Radius );
            }
        }

        private static GameObject CreateMeshPart( Transform cbTransform )
        {
            GameObject go = new GameObject( "mesh" );
            go.transform.SetParent( cbTransform );
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            return go;
        }
    }
}
