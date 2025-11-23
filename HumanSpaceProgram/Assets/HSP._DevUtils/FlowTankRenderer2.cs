using System;
using System.Collections.Generic;
using UnityEngine;
using HSP.ResourceFlow;
using HSP.UI;

namespace HSP._DevUtils
{
    /// <summary>
    /// Visualizes a FlowTank by performing purely random sampling (Monte Carlo style) 
    /// within the valid geometry of the tank.
    /// </summary>
    public class FlowTankRenderer2 : MonoBehaviour
    {
        public Transform targetTransform;

        [Header( "Data Source" )]
        public FlowTank TargetTank;

        [Header( "Sampling Settings" )]
        [Tooltip( "World space size of the sample particles." )]
        public float ParticleSize = 0.1f;

        [Tooltip( "Total number of random samples to attempt to place within the tank." )]
        public int TotalSampleCount = 2000;

        [Header( "Node Settings" )]
        [Tooltip( "Radius of the nodes in Meters." )]
        public float NodeRadius = 0.1f;
        public Color NodeColor = new Color( 0.5f, 0.5f, 0.5f, 1f );

        [Header( "Inlet Settings" )]
        public Color InletColor = new Color( 1f, 0.6f, 0.0f, 1f );

        // --- Internal ---

        // Random Sampling Data
        private Vector3[] _samplePositions;
        private Matrix4x4[] _particleMatrices;
        private Vector4[] _particleColors;
        private MaterialPropertyBlock _particleProps;

        // Node Instancing Data
        private Mesh _quadMesh;
        private Material _billboardMat;
        private Matrix4x4[] _nodeMatrices;
        private MaterialPropertyBlock _nodeProps;
        private Matrix4x4[] _inletMatrices;
        private float[] _inletInnerRadii;
        private MaterialPropertyBlock _inletProps;

        private bool _initialized = false;

        void Start()
        {
            const int cellsX = 2, cellsY = 3, cellsZ = 2;
            var triangulationPositions = new Vector3[cellsX * cellsY * cellsZ];
            int index = 0;  // Running index into the 1D array

            for( int z = 0; z < cellsZ; z++ )
            {
                for( int y = 0; y < cellsY; y++ )
                {
                    for( int x = 0; x < cellsX; x++ )
                    {
                        // Center offset of the current cell
                        Vector3 cellCenter = new Vector3(
                            x - ((cellsX - 1) / 2f),      // each cell is 2 units wide in X
                            y - ((cellsY - 1) / 2f),      // each layer is 2 units tall in Y
                            z - ((cellsZ - 1) / 2f) );     // each cell is 2 units deep in Z
                        if( y % 2 == 0 )
                        {
                            //cellCenter.x += 0.5f; // Offset every other layer in X for staggered effect
                            // cellCenter.z += 0.5f; // Offset every other layer in Z for staggered effect
                        }

                        // Add all 12 local positions, shifted by the cell's center
                        triangulationPositions[index] = cellCenter;
                        index++;
                    }
                }
            }
            ResourceInlet[] inlets = new ResourceInlet[]
            {
                new ResourceInlet( 1, new Vector3( 0, cellsY, 0 ) ),
                new ResourceInlet( 1, new Vector3( 0, -cellsY, 0 ) ),
            };

            for( int i = 0; i < triangulationPositions.Length; i++ )
            {
                Vector3 randomOffset = new Vector3(
                    UnityEngine.Random.Range( -0.2f, 0.2f ),
                    UnityEngine.Random.Range( -0.2f, 0.2f ),
                    UnityEngine.Random.Range( -0.2f, 0.2f ) );
                randomOffset *= 0.0f;
                triangulationPositions[i] += randomOffset;
            }
            ISubstance sub = new Substance( "h20" )
            {
                DisplayColor = new Color( 0, 0, 1 ),
                DisplayName = "Water",
                ReferenceDensity = 1000f,
                Phase = SubstancePhase.Liquid,
                BulkModulus = 2e9f,
                MolarMass = 0.01801528f
            };
            ISubstance sub2 = new Substance( "h2" )
            {
                DisplayColor = new Color( 0, 1, 1 ),
                DisplayName = "Hydrogen",
                SpecificGasConstant = 4124f,
                Phase = SubstancePhase.Gas,
                MolarMass = 0.00201588f
            };
            ISubstance sub3 = new Substance( "rp1" )
            {
                DisplayColor = new Color( 1, 0.5f, 0 ),
                DisplayName = "Kerosene",
                ReferenceDensity = 810f,
                Phase = SubstancePhase.Liquid,
                BulkModulus = 2e9f,
                MolarMass = 0.170f
            };
            TargetTank = new FlowTank( 0.5 );
            TargetTank.SetNodes( triangulationPositions, inlets );
            TargetTank.FluidAcceleration = new Vector3( 2, 6, 0 );
            //TargetTank.FluidAcceleration = new Vector3( 0, -6, 0 );
            TargetTank.FluidAngularVelocity = new Vector3( 0, 0, 0 );
            TargetTank.Contents = new SubstanceStateCollection()
            {
                { sub2, 0.3 * 1 },
                { sub3, 100 },
                { sub, 250 }
            };
            TargetTank.FluidState = new FluidState( pressure: 101325, temperature: 293, velocity: 0 );
            TargetTank.FluidState = new FluidState( pressure: VaporLiquidEquilibrium.ComputePressureOnly( TargetTank.Contents, TargetTank.FluidState, TargetTank.Volume ), temperature: 293, velocity: 0 );
            TargetTank.ForceRecalculateCache();
            Debug.Log( TargetTank.FluidState );

            // --- Renderer Setup ---

            _quadMesh = CreateQuadMesh();
            _billboardMat = new Material( Shader.Find( "Hidden/HSP/NodeBillboard" ) );
            _billboardMat.enableInstancing = true;

            _nodeProps = new MaterialPropertyBlock();
            _inletProps = new MaterialPropertyBlock();
            _particleProps = new MaterialPropertyBlock();

            if( TargetTank != null )
            {
                InitializeVisuals();
            }
        }

        public void FixedUpdate()
        {
            // Simulation updates if needed
        }

        private void LateUpdate()
        {
            if( TargetTank == null || !_initialized ) return;

            // 1. Update Volume Particles (Position & Color)
            UpdateVolumeParticles();

            // 2. Draw Nodes
            DrawBatchedInstanced( _quadMesh, _billboardMat, _nodeMatrices, _nodeProps, ( props ) =>
            {
                props.SetColor( "_Colorasd", NodeColor );
                props.SetFloat( "_InnerRadius", 0.0f );
            } );

            // 3. Draw Inlets
            DrawBatchedInstanced( _quadMesh, _billboardMat, _inletMatrices, _inletProps, ( props, offset, count ) =>
            {
                props.SetColor( "_Colorasd", InletColor );
                float[] slice = new float[count];
                Array.Copy( _inletInnerRadii, offset, slice, 0, count );
                props.SetFloatArray( "_InnerRadius", slice );
            } );

            // 4. Draw Volume Particles
            DrawBatchedInstanced( _quadMesh, _billboardMat, _particleMatrices, _particleProps, ( props, offset, count ) =>
            {
                Vector4[] slice = new Vector4[count];
                //Array.Copy( _particleColors, offset, slice, 0, count );
                for( int i = 0; i < count; i++ )
                {
                    slice[i] = Color.red;
                }
                slice[0] = Color.green;
                props.SetVectorArray( "_Colorasd", slice );
                props.SetFloat( "_InnerRadius", 0.0f );
            } );




            int count = 8;
            Matrix4x4[] mats = new Matrix4x4[count];
            Vector4[] cols = new Vector4[count];
            for( int i = 0; i < count; ++i )
            {
                Vector3 pos = new Vector3( i * 0.5f, 0, 0 );
                mats[i] = Matrix4x4.TRS( pos, Quaternion.identity, Vector3.one * 0.25f );
                cols[i] = new Vector4( i / 8.0f, 1 - i / 8.0f, (i % 2 == 0) ? 1 : 0, 1 );
            }
            var mpb = new MaterialPropertyBlock();
            mpb.SetVectorArray( "_Colorasd", cols );
            Graphics.DrawMeshInstanced( _quadMesh, 0, _billboardMat, mats, count, mpb );
        }

        public void InitializeVisuals()
        {
            if( TargetTank == null ) return;

            GenerateRandomSamples();
            RebuildNodesAndInlets();

            _initialized = true;
        }

        /// <summary>
        /// Generates random points within the AABB of the tank, discards those outside the actual tetrahedra geometry.
        /// </summary>
        private void GenerateRandomSamples()
        {
            var nodes = TargetTank.Nodes;
            var tets = TargetTank.Tetrahedra;
            if( nodes == null || nodes.Count == 0 || tets == null ) return;

            // 1. Calculate AABB
            Vector3 min = new Vector3( float.MaxValue, float.MaxValue, float.MaxValue );
            Vector3 max = new Vector3( float.MinValue, float.MinValue, float.MinValue );

            foreach( var node in nodes )
            {
                min = Vector3.Min( min, node.pos );
                max = Vector3.Max( max, node.pos );
            }

            // 2. Rejection Sampling
            List<Vector3> validPositions = new List<Vector3>();
            int attempts = 0;
            int maxAttempts = TotalSampleCount * 5; // Prevent infinite loops if geometry is weird
            while( validPositions.Count < TotalSampleCount && attempts < maxAttempts )
            {
                attempts++;

                // Random point in AABB
                Vector3 p = new Vector3(
                    UnityEngine.Random.Range( min.x, max.x ),
                    UnityEngine.Random.Range( min.y, max.y ),
                    UnityEngine.Random.Range( min.z, max.z )
                );

                // Check if inside any tetrahedron
                if( IsPointInsideTank( p, tets ) )
                {
                    validPositions.Add( p );
                }
            }

            _samplePositions = validPositions.ToArray();
            int count = _samplePositions.Length;

            _particleMatrices = new Matrix4x4[count];
            _particleColors = new Vector4[count];

            // Initialize matrices once, they don't move in this specific visualizer style
            // unless the tank itself moves (handled in UpdateVolumeParticles via localToWorld).
            Vector3 scaleVec = Vector3.one * ParticleSize;
            for( int i = 0; i < count; i++ )
            {
                // We set the TRS in Update because the parent Transform might move
                _particleMatrices[i] = Matrix4x4.identity;
            }

            Debug.Log( $"Generated {_samplePositions.Length} random samples inside tank bounds." );
        }

        private bool IsPointInsideTank( Vector3 p, IReadOnlyList<FlowTetrahedron> tets )
        {
            // Brute force check: is point inside ANY tetrahedron?
            // Optimization: In a real system, use a spatial hash or BVH. 
            // Since this is init-time only, brute force is acceptable for reasonable N.
            for( int i = 0; i < tets.Count; i++ )
            {
                if( IsPointInsideTetrahedron( p, tets[i] ) )
                    return true;
            }
            return false;
        }

        private bool IsPointInsideTetrahedron( Vector3 p, FlowTetrahedron tet )
        {
            // Get vertices
            Vector3 a = tet.v0.pos;
            Vector3 b = tet.v1.pos;
            Vector3 c = tet.v2.pos;
            Vector3 d = tet.v3.pos;

            // Compute Barycentric weights
            // Solve P = a*w0 + b*w1 + c*w2 + d*w3
            // Relative to 'd': P-d = w0(a-d) + w1(b-d) + w2(c-d)

            Vector3 v0 = a - d;
            Vector3 v1 = b - d;
            Vector3 v2 = c - d;
            Vector3 v3 = p - d;

            // Cramer's rule or dot products to solve 3x3 system
            // Mat3 M = [v0 v1 v2]
            // M * w = v3

            // Scalar Triple Product for volume (det)
            float detM = Vector3.Dot( v0, Vector3.Cross( v1, v2 ) );

            // If det is near zero, flat tet, ignore
            if( Mathf.Abs( detM ) < 1e-6f ) return false;

            float w0 = Vector3.Dot( v3, Vector3.Cross( v1, v2 ) ) / detM;
            float w1 = Vector3.Dot( v0, Vector3.Cross( v3, v2 ) ) / detM;
            float w2 = Vector3.Dot( v0, Vector3.Cross( v1, v3 ) ) / detM;
            float w3 = 1.0f - (w0 + w1 + w2);

            // Point is inside if all weights are between 0 and 1
            // (Actually just >= 0 is enough since they sum to 1)
            return (w0 >= -1e-4f && w1 >= -1e-4f && w2 >= -1e-4f && w3 >= -1e-4f);
        }

        private void UpdateVolumeParticles()
        {
            if( _samplePositions == null || _samplePositions.Length == 0 ) return;

            Matrix4x4 tankToWorld = targetTransform.localToWorldMatrix;
            Vector3 scaleVec = Vector3.one * ParticleSize;
            Quaternion rotation = Quaternion.identity;

            for( int i = 0; i < _samplePositions.Length; i++ )
            {
                Vector3 localPos = _samplePositions[i];

                // Sample the fluid simulation at this exact coordinate
                //IReadonlySubstanceStateCollection contents = TargetTank.SampleSubstances( localPos, 1, 1 );
                // var pot = TargetTank.GetPotentialAt( localPos ); // Optional usage
                //Color col = FlowColorResolver.GetMixedColor( contents );

                // Update Transform
                Vector3 worldPos = tankToWorld.MultiplyPoint3x4( localPos );
                _particleMatrices[i] = Matrix4x4.TRS( worldPos, rotation, scaleVec );
                _particleColors[i] = new Vector4( worldPos.x, worldPos.y, worldPos.z, 1 ); // Visual tests indicate the colors are distributed randomly and without any obvious pattern (white noise-like).
            }
        }

        private void RebuildNodesAndInlets()
        {
            var nodes = TargetTank.Nodes;
            var inletsDict = TargetTank.InletNodes;

            if( nodes == null ) return;

            List<Matrix4x4> nodeMats = new List<Matrix4x4>();
            List<Matrix4x4> inletMats = new List<Matrix4x4>();
            List<float> inletRadii = new List<float>();

            Matrix4x4 tankToWorld = targetTransform.localToWorldMatrix;

            foreach( var node in nodes )
            {
                Vector3 worldPos = tankToWorld.MultiplyPoint3x4( node.pos );
                float visualNodeRadius = NodeRadius;
                float nodeScale = visualNodeRadius * 2.0f;

                nodeMats.Add( Matrix4x4.TRS( worldPos, Quaternion.identity, Vector3.one * nodeScale ) );

                // Check Inlets
                if( inletsDict != null && inletsDict.TryGetValue( node, out double area ) )
                {
                    float physRadius = (float)Math.Sqrt( area / Math.PI );
                    float outerRadius = Mathf.Max( visualNodeRadius, physRadius );
                    float innerRadius = visualNodeRadius * 0.5f;
                    float normalizedInner = innerRadius / outerRadius;

                    inletMats.Add( Matrix4x4.TRS( worldPos, Quaternion.identity, Vector3.one * (outerRadius * 2.0f) ) );
                    inletRadii.Add( normalizedInner );
                }
            }

            _nodeMatrices = nodeMats.ToArray();
            _inletMatrices = inletMats.ToArray();
            _inletInnerRadii = inletRadii.ToArray();
        }

        // --- Standard Instancing Helpers ---

        private void DrawBatchedInstanced( Mesh mesh, Material mat, Matrix4x4[] matrices, MaterialPropertyBlock props, Action<MaterialPropertyBlock> setupProps )
        {
            DrawBatchedInstanced( mesh, mat, matrices, props, ( p, offset, count ) => setupProps( p ) );
        }

        private void DrawBatchedInstanced( Mesh mesh, Material mat, Matrix4x4[] matrices, MaterialPropertyBlock props, Action<MaterialPropertyBlock, int, int> setupProps )
        {
            for( int i = 0; i < Math.Min( 8, _samplePositions.Length ); ++i )
                Debug.Log( i + " pos=" + _samplePositions[i] + " color=" + _particleColors[i] );

            if( matrices == null || matrices.Length == 0 ) return;
            const int BATCH_SIZE = 1023;
            for( int i = 0; i < matrices.Length; i += BATCH_SIZE )
            {
                int count = Mathf.Min( BATCH_SIZE, matrices.Length - i );
                props.Clear();
                setupProps( props, i, count );

                Matrix4x4[] batchMats = new Matrix4x4[count];
                Array.Copy( matrices, i, batchMats, 0, count );
                Graphics.DrawMeshInstanced( mesh, 0, mat, batchMats, count, props );
            }
        }

        private Mesh CreateQuadMesh()
        {
            Mesh m = new Mesh();
            m.name = "GeneratedQuad";
            m.vertices = new Vector3[] {
                new Vector3(-0.5f, -0.5f, 0), new Vector3(0.5f, -0.5f, 0),
                new Vector3(-0.5f, 0.5f, 0), new Vector3(0.5f, 0.5f, 0)
            };
            m.uv = new Vector2[] {
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 1), new Vector2(1, 1)
            };
            m.triangles = new int[] { 0, 2, 1, 2, 3, 1 };
            m.RecalculateBounds();
            return m;
        }
    }
}