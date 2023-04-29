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
        /// The number of binary subdivisions per edge of each of the quads at l0.
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
            _quadTree = new LODQuadTree[6];
            for( int i = 0; i < 6; i++ )
            {
                Vector2 center = Vector2.zero;
                int lN = 0;
                var face = LODQuad.Create( _celestialBody.transform, ((QuadSphereFace)i).ToVector3() * (float)_celestialBody.Radius, this, _celestialBody, EdgeSubdivisions, center, lN, (float)_celestialBody.Radius * 2f, (QuadSphereFace)i );
                _quadTree[i] = new LODQuadTree();
                _quadTree[i].Root = new LODQuadTree.Node()
                {
                    Value = face,
                    Center = center,
                    Size = LODQuad.GetSize( lN )
                };
                face.Node = _quadTree[i].Root;
            }
        }

        void Update()
        {
            foreach( var q in _quadTree )
            {
                foreach( var qq in q.GetLeafNodes() ) // todo - optimize.
                {
                    if( qq != null )
                    {
                        qq.airfPOI = VesselManager.ActiveVessel.AIRFPosition;
                    }
                }
            }
        }

        // ondestroy delete itself?
    }
}