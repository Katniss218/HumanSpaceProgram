using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityPlus.AssetManagement;

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

        Vector3Dbl[] _oldPois = null; // Initial value null is important.

        LODQuadRebuilder _currentBuild;
        LODQuadTreeChanges _currentChanges;

        public Transform QuadParent => this.transform;

        public static Shader cbShader;
        public static Texture2D[] cbTex;

        void Awake()
        {
            for( int i = 0; i < 6; i++ )
            {
#warning TODO - Move these to some sort of celestial body definition.
                Material mat = new Material( cbShader );
                mat.SetTexture( "_MainTex", cbTex[i] );
                mat.SetFloat( "_Glossiness", 0.05f );
                mat.SetFloat( "_NormalStrength", 0.0f );
                AssetRegistry.Register( $"Vanilla::CBMATERIAL{i}", mat );
            }

            // Possibly move this to a child, so it can be disabled without disabling entire CB.
            CelestialBody = this.GetComponent<CelestialBody>();
        }

        void Start()
        {
            _quadTree = new LODQuadTree( HardLimitSubdivLevel );
        }

        private static bool ApproximatelyDifferent( Vector3Dbl lhs, Vector3Dbl rhs, double threshold )
        {
            return Math.Abs( lhs.x - rhs.x ) >= threshold
                || Math.Abs( lhs.y - rhs.y ) >= threshold
                || Math.Abs( lhs.z - rhs.z ) >= threshold;
        }

        private bool PoisChanged( Vector3Dbl[] newPois )
        {
            if( _oldPois == null )
                return true;

            if( _oldPois.Length != newPois.Length )
                return true;

            for( int i = 0; i < newPois.Length; i++ )
            {
                if( ApproximatelyDifferent( newPois[i], _oldPois[i], 0.5 ) )
                {
                    return true;
                }
            }

            return false;
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
            Vector3Dbl pos = this.CelestialBody.ReferenceFrameTransform.AbsolutePosition;
            QuaternionDbl rot = this.CelestialBody.ReferenceFrameTransform.AbsoluteRotation;
            double scale = this.CelestialBody.Radius;

            Vector3Dbl[] localPois = PoIGetter.Invoke().Select( p => rot.Inverse() * ((p - pos) / scale) ).ToArray();

            if( PoisChanged( localPois ) )
            {
                LODQuadTreeChanges changes = LODQuadTreeChanges.GetChanges( _quadTree, localPois );

                if( changes.AnythingChanged )
                {
                    _oldPois = localPois;
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
#warning TODO - get quads to delete as well.
                }
                _currentBuild = null;
            }
        }

        public ILODQuadJob[][] jobs = new ILODQuadJob[][]
        {
            new ILODQuadJob[] { new MakeQuadMesh_Job(),
            new Displace_Job(),
            new SmoothNeighbors_Job(),
            }
        };

        // ondestroy delete itself?
    }
}