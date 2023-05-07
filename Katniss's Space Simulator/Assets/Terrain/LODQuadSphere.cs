using KatnisssSpaceSimulator.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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

        /// <summary>
        /// The level of subdivision (lN) at which the quad will stop subdividing.
        /// </summary>
        public int HardLimitSubdivLevel { get; set; } = 32;

        [field: SerializeField]
        LODQuadTree[] _quadTree;
        CelestialBody _celestialBody;

        public const float QUAD_RANGE_MULTIPLIER = 2.0f; // 3.0 makes all joints only between the same subdiv.

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
                // temporary, there's no data structure to hold this stuff yet.
                Material mat = new Material( t.cbShader );
                mat.SetTexture( "_MainTex", t.cbTextures[i] );
                mat.SetFloat( "_Glossiness", 0.05f );
                mat.SetFloat( "_NormalStrength", 0.0f );


                Vector2 center = Vector2.zero;
                int lN = 0;

                _quadTree[i] = new LODQuadTree( new LODQuadTree.Node( null, center, LODQuadTree_NodeUtils.GetSize( lN ) ) );

#warning TODO - there is some funkiness with the collider physics (it acts as if the object was unparented (when unparenting, it changes scene position slightly)).

                LODQuad face = LODQuad.CreateL0( _celestialBody.transform, this, _celestialBody, _quadTree[i].Root, (float)_celestialBody.Radius * QUAD_RANGE_MULTIPLIER, mat, (Direction3D)i );
                _activeQuads.Add( face );
            }
        }

        List<LODQuad> _activeQuads = new List<LODQuad>();

        void Update()
        {
            List<LODQuad> newActiveQuads = new List<LODQuad>( _activeQuads ); // todo - kinda unoptimal.
            List<LODQuad> needRemeshing = new List<LODQuad>();

            foreach( var quad in _activeQuads )
            {
                if( quad.Node.Value == null ) // marked as destroyed.
                    continue;

                quad.AirfPOIs = new Vector3Dbl[] { VesselManager.ActiveVessel.AIRFPosition };

                if( quad.CurrentState is LODQuad.State.Idle )
                {
                    continue;
                }

                if( quad.CurrentState is LODQuad.State.Active )
                {
                    if( quad.ShouldSubdivide() )
                    {
                        quad.Subdivide( ref newActiveQuads, ref needRemeshing );
                        continue;
                    }

                    if( quad.ShouldUnsubdivide() )
                    {
                        quad.Unsubdivide( ref newActiveQuads, ref needRemeshing );
                        continue;
                    }
                }
            }

            // this filtering stuff is kinda ugly.
            // And it's fucking retarded, because if I just check the _activeQuads, it breaks.
            _activeQuads = newActiveQuads.Where( q => q.Node.Value != null ).ToList();
            needRemeshing = needRemeshing.Where( q => q.Node.Value != null ).ToList();

            foreach( var quad in needRemeshing.Distinct() )
            {
                Contract.Assert( quad.Node.Value != null, $"Quads to rebuild ({nameof( needRemeshing )}) must not contain destroyed quads." );

                var rebuildState = new LODQuad.State.Rebuild();

                rebuildState.Job = new MakeQuadMesh_Job();

                quad.SetState( rebuildState );
            }
        }

        void LateUpdate()
        {
            foreach( var quad in _activeQuads )
            {
                Contract.Assert( quad.Node.Value != null, $"Active quads ({nameof( _activeQuads )}) must not contain destroyed quads." );

                quad.SetState( new LODQuad.State.Active() );
            }
        }

        // ondestroy delete itself?
    }
}