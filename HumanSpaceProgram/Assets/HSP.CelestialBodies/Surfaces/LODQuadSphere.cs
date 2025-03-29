using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HSP.CelestialBodies.Surfaces
{
    public static class HSPEvent_ON_LOD_QUAD_PARENT_CREATED
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".lodquadparentcreated";
    }

    /// <summary>
    /// Adds a solid surface to a celestial body.
    /// </summary>
    [RequireComponent( typeof( CelestialBody ) )]
    public sealed class LODQuadSphere : MonoBehaviour
    {
        int _edgeSubdivisions = 4;
        /// <summary>
        /// Gets or sets the number of binary subdivisions per edge of each of the quads.
        /// </summary>
        /// <remarks>
        /// Setting this member will force a rebuild.
        /// </remarks>
        public int EdgeSubdivisions
        {
            get => _edgeSubdivisions;
            set
            {
                if( IsBuilding )
                    throw new InvalidOperationException( $"Can't set {nameof( EdgeSubdivisions )} of a LOD sphere while it's building." );

                _edgeSubdivisions = value;
                ClearAllQuads();
            }
        }

        int _maxDepth = 15;
        /// <summary>
        /// Gets or sets the maximum allowed subdivision level for this LOD sphere.
        /// </summary>
        /// <remarks>
        /// Setting this member will force a rebuild.
        /// </remarks>
        public int MaxDepth
        {
            get => _maxDepth;
            set
            {
                if( IsBuilding )
                    throw new InvalidOperationException( $"Can't set {nameof( MaxDepth )} of a LOD sphere while it's building." );

                _maxDepth = value;
                ClearAllQuads();
            }
        }

        /// <summary>
        /// Gets the celestial body that this LOD sphere belongs to.
        /// </summary>
        public CelestialBody CelestialBody { get; private set; }

        /// <summary>
        /// The object that the built quads are parented to.
        /// </summary>
        public Transform QuadParent { get; private set; }

        /// <summary>
        /// Specifies how the quads of this LOD sphere behaves.
        /// </summary>
        public LODQuadMode Mode { get; private set; }

        /// <remarks>
        /// Calling this method will force a rebuild.
        /// </remarks>
        public void SetMode( LODQuadMode mode )
        {
            if( IsBuilding )
                throw new InvalidOperationException( $"Can't set {nameof( Mode )} of a LOD sphere while it's building." );

            this.Mode = mode;
            ClearAllQuads();
        }

        private ILODQuadModifier[][] _jobs;
        /// <summary>
        /// Gets the LOD sphere's jobs filtered for the LOD sphere's current build mode.
        /// </summary>
        public (ILODQuadModifier[] jobs, int[] firstJobPerStage) GetJobsForBuild()
        {
            return ILODQuadModifier.FilterJobs( _jobs, Mode );
        }

        /// <summary>
        /// Sets the jobs used by this LOD sphere
        /// </summary>
        /// <remarks>
        /// Calling this method will force a rebuild.
        /// </remarks>
        /// <param name="jobs">The jobs to copy when setting. Must not be null or contain any nulls.</param>
        public void SetJobs( params ILODQuadModifier[][] jobs )
        {
            if( IsBuilding )
                throw new InvalidOperationException( $"Can't set {nameof( _jobs )} of a LOD sphere while it's building." );

            _jobs = jobs;
            ClearAllQuads();
        }

        /// <summary>
        /// The getter is invoked to get the points of interest that this LOD sphere should subdivide towards.
        /// </summary>
        /// <remarks>
        /// The points returned by the getter should be in absolute space.
        /// </remarks>
        public Func<IEnumerable<Vector3Dbl>> PoIGetter { get; set; }

        /// <summary>
        /// Checks if the LOD sphere is currently building any new quads.
        /// </summary>
        public bool IsBuilding => _builder != null;

        private Material[] _materials;
        public Material[] Materials
        {
            get => _materials;
            set
            {
                _materials = value;
                foreach( var lod in _currentQuads )
                {
                    lod.Value.Quad.ResetMaterial();
                }
            }
        }

        Vector3Dbl[] _oldPois = null; // Initial value null is important.

        Dictionary<LODQuadTreeNode, LODQuadRebuildData> _currentQuads = new( new ValueLODQuadTreeNodeComparer() );
        public IReadOnlyDictionary<LODQuadTreeNode, LODQuadRebuildData> CurrentQuads => _currentQuads;
        LODQuadTree _currentTree;

        LODQuadRebuilder _builder;
        LODQuadTree _buildingTree;
        LODQuadTreeChanges _buildingChanges;

        void Awake()
        {
            // Possibly move this to a child, so it can be disabled without disabling entire CB.
            CelestialBody = this.GetComponent<CelestialBody>();
        }

        void Start()
        {
            _currentTree = new LODQuadTree( MaxDepth );
            TryCreateQuadParentGameObject();
        }

        void Update()
        {
            if( PoIGetter == null )
                return;

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
            _builder?.Dispose();
            foreach( var oldRebuildData in _currentQuads.Values )
            {
                oldRebuildData.Dispose();
            }
        }

        void OnDisable()
        {
            if( IsBuilding )
            {
                _builder.Dispose();
                Debug.LogWarning( $"{nameof( LODQuadSphere )} on celestial body '{CelestialBody.ID}' was disabled while building." );
            }
        }

        private void TryCreateQuadParentGameObject()
        {
            if( QuadParent != null )
                return;

            GameObject parent = new GameObject( $"QUADPARENT-{this.CelestialBody.ID}" );
            QuadParent = parent.transform;
            HSPEvent.EventManager.TryInvoke( HSPEvent_ON_LOD_QUAD_PARENT_CREATED.ID, this );
        }

        private void ClearAllQuads()
        {
            foreach( var kvp in _currentQuads )
            {
                Destroy( kvp.Value.Quad );
                kvp.Value.Dispose();
            }
            _currentQuads.Clear();
            _currentTree = new LODQuadTree( MaxDepth );
        }

        private void TryRebuild()
        {
            if( _builder != null )
                throw new InvalidOperationException( $"Tried to start building while already building." );
            if( PoIGetter == null )
                throw new InvalidOperationException( $"Tried to start building while the PoI getter was not set." );

            if( _jobs == null )
                return;

            Vector3Dbl pos = this.CelestialBody.ReferenceFrameTransform.AbsolutePosition;
            QuaternionDbl invRot = this.CelestialBody.ReferenceFrameTransform.AbsoluteRotation.Inverse();
            double scale = this.CelestialBody.Radius;

#warning TODO - something with pois to determine how much each quad should subdivide towards it (stopping subdiv level) (may be useful for camera frustum biasing and for fast moving objects)
            Vector3Dbl[] localPois = PoIGetter.Invoke()
                .Select( p => invRot * ((p - pos) / scale) )
                .Where( p => p.magnitude < 2 * LODQuadTreeNode.SUBDIV_RANGE_MULTIPLIER )
                .ToArray();

            if( PoisChanged( localPois, 0.5 / scale ) )
            {
                var buildingTree = LODQuadTree.FromPois( _currentTree.MaxDepth, localPois );
                var buildingChanges = LODQuadTree.GetDifferences( _currentTree, buildingTree );

                if( buildingChanges.AnythingChanged )
                {
                    _oldPois = localPois;
                    _builder = new LODQuadRebuilder( this, buildingChanges, LODQuadRebuilder.BuildSettings.IncludeNodesWithChangedNeighbors );
                    _buildingTree = buildingTree;
                    _buildingChanges = buildingChanges;
                }
            }
        }

        private void TryBuild()
        {
            if( _builder == null )
                throw new InvalidOperationException( $"Tried to build without starting a build first." );

            if( _jobs == null )
                return;

            if( !_builder.IsDone )
            {
                _builder.Build();
            }

            if( _builder.IsDone ) // Building might've been finished by the previous call (in this frame), in which case collect the results immediately.
            {
                // Destroy quads that don't exist anymore.
                foreach( var node in _buildingChanges.GetRemovedNodes() )
                {
                    if( _currentQuads.Remove( node, out var quad ) )
                    {
                        Destroy( quad.Quad.gameObject );
                        quad.Dispose();
                    }
                    else
                    {
                        Debug.LogError( "Quad [exists in old, doesn't exist in new] was already destroyed." );
                    }
                }

                // Activate quads that were made into leaves.
                foreach( var node in _buildingChanges.GetBecameLeaf() )
                {
                    if( _currentQuads.TryGetValue( node, out var quad ) )
                    {
                        if( quad.Quad.IsActive )
                        {
                            Debug.LogError( $"Quad [became a leaf in new] was already active." );
                        }
                        else
                        {
                            quad.Quad.Activate();
                        }
                    }
                }

                // Deactivate quads that stopped being leaves.
                foreach( var node in _buildingChanges.GetBecameNonLeaf() )
                {
                    if( _currentQuads.TryGetValue( node, out var quad ) )
                    {
                        if( quad.Quad.IsActive )
                        {
                            quad.Quad.Deactivate();
                        }
                        else
                        {
                            Debug.LogError( $"Quad [became a non-leaf in new] was already inactive." );
                        }
                    }
                }

                // Activate the newly built leaves.
                // Some quads might've been *rebuilt* (existed before, got refreshed)
                foreach( var newBuilt in _builder.GetResults() )
                {
                    if( _currentQuads.Remove( newBuilt.Node, out var existingQuad ) )
                    {
                        Destroy( existingQuad.Quad.gameObject );
                        existingQuad.Dispose();
                    }

                    _currentQuads.Add( newBuilt.Node, newBuilt );

                    if( newBuilt.Node.IsLeaf )
                    {
                        newBuilt.Quad.Activate();
                    }
                }

                _currentTree = _buildingTree;
                _builder = null;
                _buildingTree = null;
                _buildingChanges = null;
            }
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

        private static bool ApproximatelyDifferent( Vector3Dbl lhs, Vector3Dbl rhs, double threshold )
        {
            return Math.Abs( lhs.x - rhs.x ) >= threshold
                || Math.Abs( lhs.y - rhs.y ) >= threshold
                || Math.Abs( lhs.z - rhs.z ) >= threshold;
        }
    }
}