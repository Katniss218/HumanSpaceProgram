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
        /// <summary>
        /// The number of binary subdivisions per edge of each of the quads.
        /// </summary>
        public int EdgeSubdivisions { get; private set; } = 4;

        LODQuadTree[] _quadTree;
        CelestialBody _celestialBody;

        void Awake()
        {
            // Possibly move this to a child, so it can be disabled without disabling entire CB.
            _celestialBody = this.GetComponent<CelestialBody>();
        }

        void Start()
        {
            zzzTestGameManager t = FindObjectOfType<zzzTestGameManager>();

            _quadTree = new LODQuadTree[6];
            for( int i = 0; i < 6; i++ )
            {
                Vector2 center = Vector2.zero;
                int lN = 0;

                _quadTree[i] = new LODQuadTree();
                _quadTree[i].Root = new LODQuadTree.Node()
                {
                    Center = center,
                    Size = LODQuadUtils.GetSize( lN )
                };
#warning TODO - there is some funkiness with the collider physics (it acts as if the object was unparented (when unparenting, it changes scene position slightly)).


                Material mat = new Material( t.cbShader );
                mat.SetTexture( "_MainTex", t.cbTextures[i] );
                mat.SetFloat( "_Glossiness", 0.05f );
                mat.SetFloat( "_NormalStrength", 0.0f );

                var face = LODQuad.Create( _celestialBody.transform, ((QuadSphereFace)i).ToVector3() * (float)_celestialBody.Radius, this, _celestialBody, EdgeSubdivisions, center, lN, _quadTree[i].Root, (float)_celestialBody.Radius * 2f, mat, (QuadSphereFace)i );

                face.Node = _quadTree[i].Root;
            }
        }

        void Update()
        {
            foreach( var q in _quadTree )
            {
                foreach( var qq in q.GetLeafNodes() ) // This can be optimized for large numbers of subdivs.
                {
                    qq.airfPOI = VesselManager.ActiveVessel.AIRFPosition;
                }
            }
        }

        // ondestroy delete itself?
    }
}