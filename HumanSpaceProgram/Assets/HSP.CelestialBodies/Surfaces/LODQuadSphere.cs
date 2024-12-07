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
        public int EdgeSubdivisions { get; private set; } = 5;

        /// <summary>
        /// The level of subdivision (lN) at which the quad will stop subdividing.
        /// </summary>
        public int HardLimitSubdivLevel { get; set; } = 16;

        public CelestialBody CelestialBody { get; private set; }

#warning TODO - Setter causes desyncs with existing quads
        public LODQuadMode Mode { get; set; }

        public Func<IEnumerable<Vector3Dbl>> PoIGetter { get; set; }

        Vector3Dbl[] _oldPois = null; // Initial value null is important.

        LODQuadTree _currentTree;
        Dictionary<LODQuadTreeNode, LODQuad> _currentQuads = new( new ValueLODQuadTreeNodeComparer() );

        LODQuadRebuilder _builder;
        LODQuadTree _buildingTree;
        LODQuadTreeChanges _buildingChanges;

        public bool IsBuilding => _builder != null;

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
            _currentTree = new LODQuadTree( HardLimitSubdivLevel );
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
            if( _builder == null )
            {
                TryRebuild();
            }

            if( _builder != null ) // also starts the build.
            {
                TryBuild();
            }
        }

        void OnDestroy()
        {
            if( _builder != null )
            {
                _builder.Dispose();
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
#warning TODO - filter only use pois that are within the range of the planet itself. Only use those pois when doing comparisons as well

                var buildingTree = LODQuadTree.FromPois( _currentTree.MaxDepth, localPois );
                var buildingChanges = LODQuadTree.GetDifferences( _currentTree, buildingTree );

                if( buildingChanges.AnythingChanged )
                {
                    _oldPois = localPois;
                    _builder = LODQuadRebuilder.FromChanges( this, jobs, buildingChanges, LODQuadRebuildMode.Visual, LODQuadRebuilder.BuildSettings.IncludeNeighborsOfChanged );
                    _buildingTree = buildingTree;
                    _buildingChanges = buildingChanges;
                }
            }
        }

        private void TryBuild()
        {
            if( !_builder.IsDone )
            {
                _builder.Build();
            }
            if( _builder.IsDone ) // if build finished.
            {
                foreach( var node in _buildingChanges.GetRemovedNodes() )
                {
                    if( _currentQuads.Remove( node, out var quad ) ) // destroy the children of unsubdivided nodes.
                    { 
                        Destroy( quad.gameObject );
                    }
                    else
                    {
                        Debug.LogError( "Quad [exists in old, doesn't exist in new] was already destroyed." );
                    }
                }

                foreach( var node in _buildingChanges.GetBecameLeaf() )
                {
                    if( _currentQuads.TryGetValue( node, out var quad ) )  // activate the existing unsubdivided nodes.
                    {
                        if( quad.IsActive )
                        {
                            Debug.LogError( $"Quad [became a leaf in new] was already active." );
                        }
                        else
                        {
                            quad.Activate();
                        }
                    }
                }

                foreach( var node in _buildingChanges.GetBecameNonLeaf() )
                {
                    if( _currentQuads.TryGetValue( node, out var quad ) )  // activate the existing unsubdivided nodes.
                    {
                        if( !quad.IsActive )
                        {
                            Debug.LogError( $"Quad [became a non-leaf in new] was already inactive." );
                        }
                        else
                        {
                            quad.Deactivate();
                        }
                    }
                }

                foreach( var quad in _builder.GetResults() )
                {
                    if( _currentQuads.Remove( quad.Node, out var existingQuad ) ) // existing leaf was refreshed.
                    {
                        Destroy( existingQuad.gameObject );
                    }

                    _currentQuads.Add( quad.Node, quad );

                    if( quad.Node.IsLeaf )      // activate the leaves of subdivided chains.
                    {
                        quad.Activate();
                    }
                }

                _currentTree = _buildingTree;
                _builder = null;
                _buildingTree = null;
                _buildingChanges = null;
            }
        }
    }
}