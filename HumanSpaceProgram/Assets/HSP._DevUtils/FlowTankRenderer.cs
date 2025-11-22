using System;
using System.Collections.Generic;
using UnityEngine;
using HSP.ResourceFlow;
using HSP.UI;


namespace HSP._DevUtils
{
    /// <summary>
    /// Visualizes a FlowTank's topology and contents.
    /// Uses PolyLineRenderer for edges and GPU Instancing for nodes/inlets.
    /// </summary>
    [RequireComponent( typeof( PolyLineRenderer ) )]
    public class FlowTankRenderer : MonoBehaviour
    {
        public Transform targetTransform;
        [Header( "Data Source" )]
        public FlowTank TargetTank;

        [Header( "Line Settings" )]
        [Tooltip( "Thickness of the pipes in Meters." )]
        public float PipeThickness = 0.05f;

        [Header( "Node Settings" )]
        [Tooltip( "Radius of the nodes in Meters." )]
        public float NodeRadius = 0.1f;
        public Color NodeColor = new Color( 0.5f, 0.5f, 0.5f, 1f );

        [Header( "Inlet Settings" )]
        public Color InletColor = new Color( 1f, 0.6f, 0.0f, 1f );

        // --- Internal ---
        private PolyLineRenderer _lineRenderer;

        // Reusable buffers
        private List<Color> _edgeColorBuffer = new List<Color>();

        // Instancing Data
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
            Vector3[] triangulationPositions = new Vector3[]
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(1f, 0f, 0f),
                new Vector3(0f, 1f, 0f),
                new Vector3(0f, 0f, 1f)
            };
            triangulationPositions = new Vector3[]
            {
                new Vector3(1f, 0f, 0f),
                new Vector3(-1f, 0f, 0f),
                new Vector3(0f, 0f, 1f),
                new Vector3(0f, 0f, -1f),

                new Vector3(1f, 1f, 0f),
                new Vector3(-1f, 1f, 0f),
                new Vector3(0f, 1f, 1f),
                new Vector3(0f, 1f, -1f),

                new Vector3(1f, -1f, 0f),
                new Vector3(-1f, -1f, 0f),
                new Vector3(0f, -1f, 1f),
                new Vector3(0f, -1f, -1f),
            };
            ResourceInlet[] inlets = new ResourceInlet[]
            {
                new ResourceInlet( 1, new Vector3( 0, 2, 0 ) ),
                new ResourceInlet( 1, new Vector3( 0, -2, 0 ) ),
            };
            ISubstance sub = new Substance( "h20" )
            {
                DisplayColor = Color.blue,
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
            TargetTank.FluidAngularVelocity = new Vector3( 0, 0, 0 );
            TargetTank.Contents = new SubstanceStateCollection()
            {
                { sub2, 0.3 * 100 },
                { sub, 250 }
            };
            TargetTank.FluidState = new FluidState( pressure: 101325, temperature: 293, velocity: 0 );
            TargetTank.FluidState = new FluidState( pressure: VaporLiquidEquilibrium.ComputePressureOnly( TargetTank.Contents, TargetTank.FluidState, TargetTank.Volume ), temperature: 293, velocity: 5 );
            TargetTank.DistributeContents();

            _lineRenderer = GetComponent<PolyLineRenderer>();

            // Ensure renderer is in World Space mode
            _lineRenderer.useWorldSpaceWidth = true;
            _lineRenderer.lineMaterial = new Material( Shader.Find( "Hidden/HSP/PolyLine" ) );

            // Setup Instancing Mesh (Simple Quad)
            _quadMesh = CreateQuadMesh();
            _billboardMat = new Material( Shader.Find( "Hidden/HSP/NodeBillboard" ) );
            _billboardMat.enableInstancing = true;

            _nodeProps = new MaterialPropertyBlock();
            _inletProps = new MaterialPropertyBlock();

            if( TargetTank != null )
            {
                InitializeVisuals();
            }
        }

        public void FixedUpdate()
        {
            for( int i = 0; i < 10; i++ )
            {
                TargetTank.DistributeContents();
            }
        }

        private void LateUpdate()
        {
            if( TargetTank == null || !_initialized ) return;

            // 1. Update Line Colors (Flow)
            UpdateFlowColors();

            // 2. Draw Nodes (Instanced with Batching)
            DrawBatchedInstanced( _quadMesh, _billboardMat, _nodeMatrices, _nodeProps, ( props ) =>
            {
                props.SetColor( "_Color", NodeColor );
                props.SetFloat( "_InnerRadius", 0.0f );
            } );

            // 3. Draw Inlets (Instanced with Batching)
            // Note: We must pass the sliced float array for _InnerRadius
            DrawBatchedInstanced( _quadMesh, _billboardMat, _inletMatrices, _inletProps, ( props, offset, count ) =>
            {
                props.SetColor( "_Color", InletColor );

                // Create a temporary slice for the float array property
                // (MaterialPropertyBlock.SetFloatArray copies the data, so this is safe but allocates per batch)
                float[] slice = new float[count];
                Array.Copy( _inletInnerRadii, offset, slice, 0, count );
                props.SetFloatArray( "_InnerRadius", slice );
            } );
        }

        // Helper for simple props
        private void DrawBatchedInstanced( Mesh mesh, Material mat, Matrix4x4[] matrices, MaterialPropertyBlock props, Action<MaterialPropertyBlock> setupProps )
        {
            DrawBatchedInstanced( mesh, mat, matrices, props, ( p, offset, count ) => setupProps( p ) );
        }

        // Generic Batcher
        private void DrawBatchedInstanced( Mesh mesh, Material mat, Matrix4x4[] matrices, MaterialPropertyBlock props, Action<MaterialPropertyBlock, int, int> setupProps )
        {
            if( matrices == null || matrices.Length == 0 ) return;

            const int BATCH_SIZE = 1023;
            for( int i = 0; i < matrices.Length; i += BATCH_SIZE )
            {
                int count = Mathf.Min( BATCH_SIZE, matrices.Length - i );

                // Setup properties for this specific batch
                props.Clear();
                setupProps( props, i, count ); // Pass offset and count to helper

                // Draw the slice
                // Note: DrawMeshInstanced overload allows an offset into the array, 
                // but existing Unity versions usually require the array to be passed directly.
                // However, standard API takes (Mesh, submesh, mat, Matrix[], count, props).
                // It draws the *first* 'count' elements. 
                // To draw a middle slice without copying matrices, we usually need to use the overload:
                // DrawMeshInstanced(mesh, submesh, material, matrices, count, props, shadowCastingMode, receiveShadows, lightProbeUsage, camera);
                // BUT: This overload *always* starts at index 0 of the matrix array in standard C# API.

                // OPTIMIZATION: To avoid allocs copying Matrix4x4 arrays, we usually pre-allocate batches 
                // or use Graphics.DrawMeshInstancedIndirect (ComputeBuffer).
                // For simplicity here, we will use a list copy if batch > 1. 

                if( matrices.Length <= BATCH_SIZE )
                {
                    Graphics.DrawMeshInstanced( mesh, 0, mat, matrices, count, props );
                }
                else
                {
                    // Fallback for large sets: We must copy the matrix slice 
                    // because the API doesn't support "start index" for matrix array.
                    Matrix4x4[] batchMats = new Matrix4x4[count];
                    Array.Copy( matrices, i, batchMats, 0, count );
                    Graphics.DrawMeshInstanced( mesh, 0, mat, batchMats, count, props );
                }
            }
        }

        /// <summary>
        /// Rebuilds the topology (Lines and Matrices). Call this if the Tank structure changes.
        /// </summary>
        public void InitializeVisuals()
        {
            if( TargetTank == null ) return;

            RebuildEdges();
            RebuildNodesAndInlets();

            _initialized = true;
        }

        private void RebuildEdges()
        {
            _lineRenderer.Clear();
            _edgeColorBuffer.Clear();

            var edges = TargetTank.Edges;
            if( edges == null ) return;

            foreach( var edge in edges )
            {
                // Edges in FlowTank usually connect Node1 and Node2
                // Assuming FlowNode has a 'Position' Vector3 property.
                // If 'Position' is local to the tank, we might need TargetTank.transform.TransformPoint(),
                // but usually simulation nodes are kept in Local space and the renderer transforms them, 
                // OR the line renderer works in World space.
                // Since PolyLineRenderer works in World Space, we must transform.

                // NOTE: Adjust this based on your FlowNode definition. 
                // If FlowNode.Position is local to the tank object:
                Vector3 p1 = targetTransform.TransformPoint( edge.end1.pos );
                Vector3 p2 = targetTransform.TransformPoint( edge.end2.pos );

                _lineRenderer.AddLine( p1, p2, Color.gray, PipeThickness );
                _edgeColorBuffer.Add( Color.gray );
            }

            _lineRenderer.ForceRebuildMesh();
        }

        private void RebuildNodesAndInlets()
        {
            var nodes = TargetTank.Nodes;
            var inletsDict = TargetTank.InletNodes; // Assumes this field is accessible or exposed via property

            if( nodes == null ) return;

            List<Matrix4x4> nodeMats = new List<Matrix4x4>();
            List<Matrix4x4> inletMats = new List<Matrix4x4>();
            List<float> inletRadii = new List<float>();

            foreach( var node in nodes )
            {
                Vector3 worldPos = targetTransform.TransformPoint( node.pos );

                // 1. Standard Node Visualization
                // Render node slightly larger than pipe
                float visualNodeRadius = Mathf.Max( NodeRadius, PipeThickness * 1.2f );
                float nodeScale = visualNodeRadius * 2.0f;

                nodeMats.Add( Matrix4x4.TRS( worldPos, Quaternion.identity, Vector3.one * nodeScale ) );

                // 2. Inlet Visualization
                // Check if this node is an inlet
                if( inletsDict != null && inletsDict.TryGetValue( node, out double area ) )
                {
                    // Calculate physical radius from area: A = PI * r^2  ->  r = Sqrt(A/PI)
                    float physRadius = (float)Math.Sqrt( area / Math.PI );

                    // Visual requirement: Outer Radius >= Node Radius
                    float outerRadius = Mathf.Max( visualNodeRadius, physRadius );

                    // Visual requirement: Inner Radius = 0.5 * Node Radius
                    float innerRadius = visualNodeRadius * 0.5f;

                    // Calculate Shader normalized Inner Radius (0..1 relative to Outer)
                    float normalizedInner = innerRadius / outerRadius;

                    float inletScale = outerRadius * 2.0f;

                    inletMats.Add( Matrix4x4.TRS( worldPos, Quaternion.identity, Vector3.one * inletScale ) );
                    inletRadii.Add( normalizedInner );
                }
            }

            _nodeMatrices = nodeMats.ToArray();
            _inletMatrices = inletMats.ToArray();
            _inletInnerRadii = inletRadii.ToArray();
        }

        private void UpdateFlowColors()
        {
            var contents = TargetTank.ContentsInEdges;
            if( contents == null || contents.Length != _edgeColorBuffer.Count ) return;

            for( int i = 0; i < contents.Length; i++ )
            {
                _edgeColorBuffer[i] = FlowColorResolver.GetMixedColor( contents[i] );
            }

            _lineRenderer.UpdateLineColors( _edgeColorBuffer );
        }

        private Mesh CreateQuadMesh()
        {
            // Procedural Quad (0.5 extent) centered at 0,0
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