using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Unity.Jobs;
using UnityEngine;

namespace HSP.CelestialBodies.Surfaces
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
        public int HardLimitSubdivLevel { get; set; } = 20;

        [field: SerializeField]
        LODQuadTree _quadTree;
        CelestialBody _celestialBody;

        public const float QUAD_RANGE_MULTIPLIER = 2.0f; // 3.0 makes all joints only between the same subdiv.

        public LODQuadMode Mode { get; }

        public static Shader cbShader;
        public static Texture2D[] cbTex;

        void Awake()
        {
            // Possibly move this to a child, so it can be disabled without disabling entire CB.
            _celestialBody = this.GetComponent<CelestialBody>();
        }

        void Start()
        {
            _quadTree = new LODQuadTree_Old[6];
            for( int i = 0; i < 6; i++ )
            {
#warning TODO - Move these to some sort of celestial body definition.
                Material mat = new Material( cbShader );
                mat.SetTexture( "_MainTex", cbTex[i] );
                mat.SetFloat( "_Glossiness", 0.05f );
                mat.SetFloat( "_NormalStrength", 0.0f );


                Vector2 center = Vector2.zero;
                int lN = 0;

                _quadTree[i] = new LODQuadTree_Old( new LODQuadTree_Old.Node( null, center, LODQuadTree_NodeUtils.GetSize( lN ) ) );

#warning TODO - celestial bodies need something that will replace the buildin parenting of colliders with 64-bit parents and update their scene position at all times (fixedupdate + update + lateupdate).

                LODQuad quad = LODQuad.CreateL0( _celestialBody.transform, this, _celestialBody, _quadTree[i].Root, (float)_celestialBody.Radius * QUAD_RANGE_MULTIPLIER, mat, (Direction3D)i );
                _activeQuads.Add( quad );

                RemeshQuad( quad );
            }
        }

        List<LODQuad> _activeQuads = new List<LODQuad>();

        public Func<IEnumerable<Vector3Dbl>> PoIGetter { get; set; }

#warning TODO - integrate a poi getter.
        /*private static IEnumerable<Vector3Dbl> GetVesselPOIs()
        {
            return VesselManager.LoadedVessels.Select( v => v.AIRFPosition );
        }*/

        private static bool ApproximatelyDifferent( Vector3Dbl lhs, Vector3Dbl rhs )
        {
            const double UPDATE_THRESHOLD = 0.5;

            return Math.Abs( lhs.x - rhs.x ) >= UPDATE_THRESHOLD
                || Math.Abs( lhs.y - rhs.y ) >= UPDATE_THRESHOLD
                || Math.Abs( lhs.z - rhs.z ) >= UPDATE_THRESHOLD;
        }

        private bool NewPoisTheSameAsLastFrame( Vector3Dbl[] airfPOIs ) // with moving vessels, we will need to use POIs in celestial body space.
        {
            // Checks if the new pois are close enough to the old pois that we don't need to change the subdivisions.
            if( _poisAtLastChange == null )
                return false;

            if( _poisAtLastChange.Length != airfPOIs.Length )
                return false;

            for( int i = 0; i < airfPOIs.Length; i++ )
            {
                if( ApproximatelyDifferent( airfPOIs[i], _poisAtLastChange[i] ) )
                {
                    return false;
                }
            }

            return true;
        }

#warning INFO - only 1 rebuild per quadtree (so 1 visual and 1 physical) at any given time.
        // However, this rebuild can be cancelled at any of the stage boundaries, and a new one started.

        bool _wasChangedLastFrame = true; // initial value true is important for initial subdivision. This can only be removed if the CB is pre-subdivided to the initial pois ahead of time.
        Vector3Dbl[] _poisAtLastChange = null; // Initial value null is important.

        void Update()
        {
            List<LODQuad> newActiveQuads = new List<LODQuad>( _activeQuads );
            List<LODQuad> needRemeshing = new List<LODQuad>();

            Vector3Dbl[] airfPOIs = PoIGetter.Invoke().ToArray();

            bool allPoisTheSame = NewPoisTheSameAsLastFrame( airfPOIs );

            bool wasChangedThisFrame = false;
            // Optimization is applied:
            // - The quads only need to be checked if they were changed in the last frame (potentially still not subdivided enough / too much), or if the pois changed by some threshold.

            // TODO - Another optimization could be to not check every ~800 quads (for large set of objects), but check their virtual parents first - the number of parents is exponentially less.

            // TODO - Another optimization could be to not subdivide further if there is no additional detail that would become visible.
            if( !allPoisTheSame || _wasChangedLastFrame )
            {
                foreach( var quad in _activeQuads )
                {
                    if( quad.Node.Value == null ) // marked as destroyed.
                        continue;

                    quad.AirfPOIs = airfPOIs;

                    if( quad.CurrentState is LODQuad.State.Idle )
                    {
                        continue;
                    }

                    if( quad.CurrentState is LODQuad.State.Active )
                    {
                        if( quad.ShouldSubdivide() )
                        {
                            quad.Subdivide( ref newActiveQuads, ref needRemeshing );
                            wasChangedThisFrame = true;
                            continue;
                        }

                        if( quad.ShouldUnsubdivide() )
                        {
                            quad.Unsubdivide( ref newActiveQuads, ref needRemeshing );
                            wasChangedThisFrame = true;
                            continue;
                        }
                    }
                }
            }

            if( wasChangedThisFrame )
            {
                _poisAtLastChange = airfPOIs;
            }
            _wasChangedLastFrame = wasChangedThisFrame;

            // this filtering stuff is kinda ugly.
            // And it's fucking retarded, because if I just check the _activeQuads, it breaks.
            _activeQuads = newActiveQuads.Where( q => q.Node.Value != null ).ToList();
            needRemeshing = needRemeshing.Where( q => q.Node.Value != null ).Distinct().ToList();

            foreach( var quad in needRemeshing )
            {
                Contract.Assert( quad.Node.Value != null, $"Quads to rebuild ({nameof( needRemeshing )}) must not contain destroyed quads." );

                RemeshQuad( quad );
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

        public ILODQuadJob[] jobs = new ILODQuadJob[]
        {
            new MakeQuadMesh_Job(),
            new Displace_Job(),
            new SmoothNeighbors_Job(),
        };

        public JobHandle[] handles = new JobHandle[]
        {
            default,
            default,
            default,
        };

        void RemeshQuad( LODQuad quad )
        {
            var rebuildState = new LODQuad.State.Rebuild()
            {
                jobs = this.jobs.ToArray(),
                handles = this.handles.ToArray(),
            };

            quad.SetState( rebuildState );
        }

        // ondestroy delete itself?
    }
}