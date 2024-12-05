using System;
using System.Collections.Generic;
using System.Linq;
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
        public CelestialBody CelestialBody { get; private set; }

        public const float QUAD_RANGE_MULTIPLIER = 2.0f; // 3.0 makes all joints only between the same subdiv.

#warning TODO - remove setter.
        public LODQuadMode Mode { get; set; }

        public Func<IEnumerable<Vector3Dbl>> PoIGetter { get; set; }

        Vector3Dbl[] _poisAtLastChange = null; // Initial value null is important.

        LODQuadRebuilder _currentBuild;
        LODQuadTreeChanges _currentChanges;

        public Transform QuadParent => this.transform;

        public static Shader cbShader;
        public static Texture2D[] cbTex;

        void Awake()
        {
            // Possibly move this to a child, so it can be disabled without disabling entire CB.
            CelestialBody = this.GetComponent<CelestialBody>();
        }

        void Start()
        {
            _quadTree = new LODQuadTree( HardLimitSubdivLevel );
            for( int i = 0; i < 6; i++ )
            {
#warning TODO - Move these to some sort of celestial body definition.
                Material mat = new Material( cbShader );
                mat.SetTexture( "_MainTex", cbTex[i] );
                mat.SetFloat( "_Glossiness", 0.05f );
                mat.SetFloat( "_NormalStrength", 0.0f );


                //Vector2 center = Vector2.zero;
                //int lN = 0;

                //_quadTree[i] = new LODQuadTree_Old( new LODQuadTree_Old.Node( null, center, LODQuadTree_NodeUtils.GetSize( lN ) ) );

#warning TODO - celestial bodies need something that will replace the buildin parenting of colliders with 64-bit parents and update their scene position at all times (fixedupdate + update + lateupdate).

                //LODQuad quad = LODQuad.CreateL0( _celestialBody.transform, this, _celestialBody, _quadTree[i].Root, (float)_celestialBody.Radius * QUAD_RANGE_MULTIPLIER, mat, (Direction3D)i );
                //_activeQuads.Add( quad );

                //RemeshQuad( quad );
            }
        }

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

#warning INFO - you can't 'finish' a rebuild, while another rebuild is active. This basically boild down to "1 rebuild can be active at a given time".
        // This rebuild can be cancelled at any of the stage boundaries, and a new one started.

        void Update()
        {
            if( _currentBuild == null )
            {
                TryRebuild();
            }

            if( _currentBuild != null ) // also starts the build.
            {
                TryBuild();
            }
        }

        void TryRebuild()
        {
            Vector3Dbl[] airfPOIs = PoIGetter.Invoke().ToArray();

            bool allPoisTheSame = NewPoisTheSameAsLastFrame( airfPOIs );

            if( !allPoisTheSame )
            {
                double radius = CelestialBody.Radius;
#warning TODO - rotate pois to be in normalized oriented space
                LODQuadTreeChanges changes = LODQuadTreeChanges.GetChanges( _quadTree, airfPOIs.Select( p => new Vector3Dbl( 1, 0.3, -0.5 ).normalized * 1.000001 ) );

                if( changes.AnythingChanged )
                {
                    _currentBuild = LODQuadRebuilder.FromChanges( this, jobs, changes, LODQuadRebuildMode.Visual );
                    _currentChanges = changes;
                }
            }
        }

        void TryBuild()
        {
            if( !_currentBuild.IsDone )
            {
                _currentBuild.Build();
            }
            if( _currentBuild.IsDone ) // if build finished.
            {
                IEnumerable<LODQuad> builtQuads = _currentBuild.GetResults();
                _currentChanges.ApplyChanges( _quadTree ); // apply before querying the tree's nodes.
                foreach( var quad in builtQuads )
                {
                    if( quad.Node.IsLeaf )
                    {
                        quad.Activate();
                    }
                }
                //_currentBuild = null;
            }
        }

        public ILODQuadJob[][] jobs = new ILODQuadJob[][]
        {
            new ILODQuadJob[] { new MakeQuadMesh_Job(),
           // new Displace_Job(),
           // new SmoothNeighbors_Job(),
            }
        };

        // ondestroy delete itself?
    }
}