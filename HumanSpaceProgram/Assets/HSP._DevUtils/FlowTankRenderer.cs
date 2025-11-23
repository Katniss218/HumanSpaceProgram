using System;
using System.Collections.Generic;
using UnityEngine;
using HSP.ResourceFlow;
using HSP.UI;

namespace HSP._DevUtils
{
    /// <summary>
    /// Visualizes a FlowTank's topology and contents.
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
            //TargetTank.FluidAcceleration = new Vector3( 2, 6, 0 );
            //TargetTank.FluidAngularVelocity = new Vector3( 0, 0, 0 );
            TargetTank.FluidAcceleration = new Vector3( 0, -0.5f, 0 );
            TargetTank.FluidAngularVelocity = new Vector3( 0, 2, 0 );
            TargetTank.Contents = new SubstanceStateCollection()
            {
                { sub2, 0.3 * 1 },
                { sub3, 50 },
                { sub, 250 }
            };
            TargetTank.FluidState = new FluidState( pressure: 101325, temperature: 293, velocity: 0 );
            TargetTank.FluidState = new FluidState( pressure: VaporLiquidEquilibrium.ComputePressureOnly( TargetTank.Contents, TargetTank.FluidState, TargetTank.Volume ), temperature: 293, velocity: 0 );
            TargetTank.ForceRecalculateCache();
            Debug.Log( TargetTank.FluidState );

            // --- Renderer Setup ---
            _lineRenderer = GetComponent<PolyLineRenderer>();
            _lineRenderer.useWorldSpaceWidth = true;
            _lineRenderer.lineMaterial = new Material( Shader.Find( "Hidden/HSP/PolyLine" ) );

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
            for( int i = 0; i < 50; i++ )
            {
                TargetTank.ForceRecalculateCache();
            }
        }

        private void LateUpdate()
        {
            if( TargetTank == null || !_initialized ) return;

            // 1. Update Line Colors (Query the stateless tank)
            UpdateFlowColors();

            // 2. Draw Nodes
            DrawBatchedInstanced( _quadMesh, _billboardMat, _nodeMatrices, _nodeProps, ( props ) =>
            {
                props.SetColor( "_Color", NodeColor );
                props.SetFloat( "_InnerRadius", 0.0f );
            } );

            // 3. Draw Inlets
            DrawBatchedInstanced( _quadMesh, _billboardMat, _inletMatrices, _inletProps, ( props, offset, count ) =>
            {
                props.SetColor( "_Color", InletColor );
                float[] slice = new float[count];
                Array.Copy( _inletInnerRadii, offset, slice, 0, count );
                props.SetFloatArray( "_InnerRadius", slice );
            } );
        }

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

            // Access the raw geometry arrays from the Tank
            // Assuming FlowTank exposes these (based on the previous implementation structure)
            var edges = TargetTank.Edges;
            var nodes = TargetTank.Nodes;

            if( edges == null || nodes == null ) return;

            // We need the transform to convert Tank-Space to World-Space
            Matrix4x4 tankToWorld = targetTransform.localToWorldMatrix;

            for( int i = 0; i < edges.Count; i++ )
            {
                var edge = edges[i];
                Vector3 p1 = tankToWorld.MultiplyPoint3x4( nodes[edge.end1].pos );
                Vector3 p2 = tankToWorld.MultiplyPoint3x4( nodes[edge.end2].pos );

                _lineRenderer.AddLine( p1, p2, Color.gray, PipeThickness );

                // Initialize buffer with empty color
                _edgeColorBuffer.Add( Color.black );
            }

            _lineRenderer.ForceRebuildMesh();
        }

        private void UpdateFlowColors()
        {
            // Logic to determine color based on Potential
            var edges = TargetTank.Edges;
            var nodes = TargetTank.Nodes;

            if( edges == null )
                return;

            int edgeCount = edges.Count;
            if( _edgeColorBuffer.Count != edgeCount )
            {
                // Mismatch (topology changed?), rebuild
                RebuildEdges();
                return;
            }

            // To visualize what's in the pipe, we sample the fluid at the CENTER of the pipe.
            // Since the tank is stateless, we just ask "What substance is at this potential?"

            for( int i = 0; i < edgeCount; i++ )
            {
                var edge = edges[i];
                Vector3 p1 = nodes[edge.end1].pos;
                Vector3 p2 = nodes[edge.end2].pos;
                IReadonlySubstanceStateCollection[] average = new IReadonlySubstanceStateCollection[10];
                for( int j = 0; j < 10; j++ )
                {
                    Vector3 samplePoint = Vector3.Lerp( p1, p2, ( j + 0.5f ) / 10f );
                    average[j] = TargetTank.SampleSubstances( samplePoint, 1, 1 );
                }

                IReadonlySubstanceStateCollection subAvg = IReadonlySubstanceStateCollection.Average( average );

                _edgeColorBuffer[i] = FlowColorResolver.GetMixedColor( subAvg );
            }

            _lineRenderer.UpdateLineColors( _edgeColorBuffer );
        }

        private void RebuildNodesAndInlets()
        {
            var nodes = TargetTank.Nodes;
            // Assuming InletNodes is exposed as Dictionary<FlowNode, double> or similar
            var inletsDict = TargetTank.InletNodes;

            if( nodes == null ) return;

            List<Matrix4x4> nodeMats = new List<Matrix4x4>();
            List<Matrix4x4> inletMats = new List<Matrix4x4>();
            List<float> inletRadii = new List<float>();

            Matrix4x4 tankToWorld = targetTransform.localToWorldMatrix;

            foreach( var node in nodes )
            {
                Vector3 worldPos = tankToWorld.MultiplyPoint3x4( node.pos );
                float visualNodeRadius = Mathf.Max( NodeRadius, PipeThickness * 1.2f );
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

        // --- Standard Instancing Helpers (Unchanged) ---

        private void DrawBatchedInstanced( Mesh mesh, Material mat, Matrix4x4[] matrices, MaterialPropertyBlock props, Action<MaterialPropertyBlock> setupProps )
        {
            DrawBatchedInstanced( mesh, mat, matrices, props, ( p, offset, count ) => setupProps( p ) );
        }

        private void DrawBatchedInstanced( Mesh mesh, Material mat, Matrix4x4[] matrices, MaterialPropertyBlock props, Action<MaterialPropertyBlock, int, int> setupProps )
        {
            if( matrices == null || matrices.Length == 0 ) return;
            const int BATCH_SIZE = 1023;
            for( int i = 0; i < matrices.Length; i += BATCH_SIZE )
            {
                int count = Mathf.Min( BATCH_SIZE, matrices.Length - i );
                props.Clear();
                setupProps( props, i, count );

                if( matrices.Length <= BATCH_SIZE )
                {
                    Graphics.DrawMeshInstanced( mesh, 0, mat, matrices, count, props );
                }
                else
                {
                    Matrix4x4[] batchMats = new Matrix4x4[count];
                    Array.Copy( matrices, i, batchMats, 0, count );
                    Graphics.DrawMeshInstanced( mesh, 0, mat, batchMats, count, props );
                }
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