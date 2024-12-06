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

#warning TODO - Setter causes desyncs with existing quads
        public LODQuadMode Mode { get; set; }

        public Func<IEnumerable<Vector3Dbl>> PoIGetter { get; set; }

        Vector3Dbl[] _oldPois = null; // Initial value null is important.

        LODQuadRebuilder _currentBuild;
        LODQuadTreeChanges _currentChanges;

        Dictionary<LODQuadTreeNode, LODQuad> allQuads = new();

        public Transform QuadParent => this.transform;

        public static Shader cbShader;
        public static Texture2D[] cbTex;


        ILODQuadJob[][] jobs = new ILODQuadJob[][]
        {
            new ILODQuadJob[]
            {
                new MakeQuadMesh_Job(),
                new Displace_Job(),
                new SmoothNeighbors_Job(),
            }
        };

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

        private bool PoisChanged( Vector3Dbl[] newPois, double maxDelta )
        {
            if( _oldPois == null )
                return true;

            if( _oldPois.Length != newPois.Length )
                return true;

            for( int i = 0; i < newPois.Length; i++ )
            {
                if( ApproximatelyDifferent( newPois[i], _oldPois[i], maxDelta ) )
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

        void OnDestroy()
        {
            if( _currentBuild != null )
            {
                _currentBuild.Dispose();
            }
        }

        private void TryRebuild()
        {
            Vector3Dbl pos = this.CelestialBody.ReferenceFrameTransform.AbsolutePosition;
            QuaternionDbl rot = this.CelestialBody.ReferenceFrameTransform.AbsoluteRotation;
            double scale = this.CelestialBody.Radius;

            Vector3Dbl[] localPois = PoIGetter.Invoke().Select( p => rot.Inverse() * ((p - pos) / scale) ).ToArray();

            if( PoisChanged( localPois, 0.5 / scale ) )
            {
                LODQuadTreeChanges changes = LODQuadTreeChanges.GetChanges( _quadTree, localPois );

                if( changes.AnythingChanged )
                {
                    _oldPois = localPois;
                    _currentBuild = LODQuadRebuilder.FromChanges( this, jobs, changes, LODQuadRebuildMode.Visual, LODQuadRebuilder.BuildSettings.IncludeNeighborsOfChanged );
                    _currentChanges = changes;
                }
            }
        }

        private void TryBuild()
        {
            if( !_currentBuild.IsDone )
            {
                _currentBuild.Build();
            }
            if( _currentBuild.IsDone ) // if build finished.
            {
                foreach( var node in _currentChanges.GetRemovedNodes() )
                {
                    if( allQuads.Remove( node, out var quad ) ) // destroy the children of unsubdivided nodes.
                    {
                        Destroy( quad.gameObject );
                    }
                }

                foreach( var node in _currentChanges.GetLeafNodesDueToRemoval() )
                {
                    if( allQuads.TryGetValue( node, out var quad ) )  // activate the existing unsubdivided nodes.
                    {
                        if( quad.IsActive )
                        {
                            Debug.LogError( $"Quad was already active." );
                        }
                        else
                        {
                            quad.Activate();
                        }
                    }
                }

                _currentChanges.ApplyTo( _quadTree );

                foreach( var quad in _currentBuild.GetResults() )
                {
                    if( allQuads.Remove( quad.Node, out var existingQuad ) ) // existing leaf was refreshed.
                    {
                        Destroy( existingQuad.gameObject );
                    }

                    if( quad.Node.Parent != null )
                    {
                        if( allQuads.TryGetValue( quad.Node.Parent, out var parentQuad ) ) // deactivate the parents of subdivided nodes.
                        {
                            if( !parentQuad.IsActive )
                            {
                                //Debug.LogError( $"Quad was already inactive." ); // not an error, technically normal (can be when a node subdivides more than once in a frame).
                            }
                            else
                            {
                                parentQuad.Deactivate();
                            }
                        }
                    }

                    allQuads.Add( quad.Node, quad );

                    if( quad.Node.IsLeaf )      // activate the leaves of subdivided chains.
                    {
                        quad.Activate();
                    }
                }
                _currentBuild = null;
            }
        }
    }
}