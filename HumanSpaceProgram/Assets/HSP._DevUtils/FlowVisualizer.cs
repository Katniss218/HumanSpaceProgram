using System.Collections.Generic;
using UnityEngine;
using HSP.ResourceFlow;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HSP._DevUtils
{
    /// <summary>
    /// Visualizer for FlowNode / FlowEdge / FlowTetrahedron produced by
    /// ComputeDelaunayTetrahedralization(IList<Vector3>).
    ///
    /// Attach to an empty GameObject. Populate InputPoints in the inspector,
    /// or call Rebuild() from code. The visualizer draws gizmos in the editor,
    /// and can optionally create runtime GameObjects for a persistent runtime view.
    /// </summary>
    [ExecuteAlways]
    public class FlowVisualizer : MonoBehaviour
    {
        [Header( "Input" )]
        [Tooltip( "Points to tetrahedralize. The visualizer will call ComputeDelaunayTetrahedralization on these." )]
        public List<Vector3> InputPoints = new List<Vector3>();

        [Header( "Toggle what to show" )]
        public bool ShowNodes = true;
        public bool ShowEdges = true;
        public bool ShowTetrahedra = true;

        [Header( "Appearance" )]
        public float NodeRadius = 0.08f;
        public float EdgeWidth = 0.02f;
        public Color NodeColor = Color.yellow;
        public Color EdgeColor = Color.white;
        public Color TetFillColor = new Color( 0.1f, 0.5f, 1f, 0.18f );
        public Color TetWireColor = new Color( 0.1f, 0.5f, 1f, 1f );

        [Header( "Runtime options" )]
        [Tooltip( "If true, a mesh + line renderers will be created under this GameObject for runtime viewing." )]
        public bool CreateRuntimeObjects = false;

        // Internal storage from the tetrahedralizer
        private List<FlowNode> _nodes;
        private List<FlowEdge> _edges;
        private List<FlowTetrahedron> _tets;

        // Containers for runtime-created GameObjects
        private GameObject _runtimeContainer;
        private readonly List<GameObject> _runtimeTetObjects = new List<GameObject>();
        private readonly List<GameObject> _runtimeEdgeObjects = new List<GameObject>();

        // Names used for child GameObjects (helps editor cleanup)
        private const string RuntimeContainerName = "FlowVisualizer_Runtime";

        #region Unity lifecycle

        private void OnEnable()
        {
            // Rebuild if inspector already has points.
            Rebuild();
        }

        private void OnDisable()
        {
            if( !Application.isPlaying )
                DestroyRuntimeObjectsImmediate();
            else
                DestroyRuntimeObjects();
        }

        private void OnDestroy()
        {
            DestroyRuntimeObjects();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Recompute the tetrahedralization and update visualization.
        /// </summary>
        [ContextMenu( "Rebuild Visualization" )]
        public void Rebuild()
        {
            // Prevent calling into the tetrahedralizer with empty or null
            if( InputPoints == null || InputPoints.Count == 0 )
            {
                ClearCachedData();
                DestroyRuntimeObjects();
                return;
            }

            // Call the provided function:
            // NOTE: This method must exist in the same namespace/project: 
            // public static (List<FlowNode> nodes, List<FlowEdge> edges, List<FlowTetrahedron> tets) ComputeDelaunayTetrahedralization(IList<Vector3> inputPoints)
            var result = ComputeDelaunaySafely( InputPoints );

            _nodes = result.nodes;
            _edges = result.edges;
            _tets = result.tets;

            if( CreateRuntimeObjects )
                BuildRuntimeObjects();
            else
                DestroyRuntimeObjects();
        }

        #endregion

        #region Gizmo drawing (Editor-friendly)

        private void OnDrawGizmos()
        {
            // If no cached data, attempt to compute for quick preview (editor only)
            if( _nodes == null || _edges == null || _tets == null )
            {
                // Don't do heavy computation in play mode repeatedly; rely on Rebuild
#if UNITY_EDITOR
                if( !Application.isPlaying )
                    Rebuild(); // editor friendly
#endif
            }

            DrawGizmos();
        }

        private void DrawGizmos()
        {
            if( _nodes == null || _edges == null || _tets == null )
                return;

            // Nodes
            if( ShowNodes )
            {
                Gizmos.color = NodeColor;
                foreach( var n in _nodes )
                {
                    if( n == null ) continue;
                    Gizmos.DrawSphere( transform.TransformPoint( n.pos ), NodeRadius );
                }
            }

            // Edges
            if( ShowEdges )
            {
                Gizmos.color = EdgeColor;
                foreach( var e in _edges )
                {
                    if( e.end1 == null || e.end2 == null ) continue;
                    Gizmos.DrawLine( transform.TransformPoint( e.end1.pos ), transform.TransformPoint( e.end2.pos ) );
                }
            }

            // Tetrahedra - draw wireframe edges (and optional faint face fill by drawing triangles with Gizmos)
            if( ShowTetrahedra )
            {
                // Slightly darker fill via DrawMesh won't be available reliably; instead use lines for wireframe
                Gizmos.color = TetWireColor;
                foreach( var t in _tets )
                {
                    if( t == null ) continue;
                    DrawTetrahedronWireframeGizmo( t );
                }
            }
        }

        private void DrawTetrahedronWireframeGizmo( FlowTetrahedron t )
        {
            Vector3 v0 = transform.TransformPoint( t.v0.pos );
            Vector3 v1 = transform.TransformPoint( t.v1.pos );
            Vector3 v2 = transform.TransformPoint( t.v2.pos );
            Vector3 v3 = transform.TransformPoint( t.v3.pos );

            // edges: v0-v1, v0-v2, v0-v3, v1-v2, v1-v3, v2-v3
            Gizmos.DrawLine( v0, v1 );
            Gizmos.DrawLine( v0, v2 );
            Gizmos.DrawLine( v0, v3 );
            Gizmos.DrawLine( v1, v2 );
            Gizmos.DrawLine( v1, v3 );
            Gizmos.DrawLine( v2, v3 );
        }

        #endregion

        #region Runtime object creation / cleanup

        private void BuildRuntimeObjects()
        {
            // Clean first
            DestroyRuntimeObjectsImmediate();

            // Create container
            _runtimeContainer = new GameObject( RuntimeContainerName );
            _runtimeContainer.transform.SetParent( transform, false );

            // Tetrahedron faces (semi-transparent meshes)
            foreach( var t in _tets )
            {
                if( t == null ) continue;
                var go = CreateTetGameObject( t );
                go.transform.SetParent( _runtimeContainer.transform, false );
                _runtimeTetObjects.Add( go );
            }

            // Edges as LineRenderers (so they appear at runtime)
            foreach( var e in _edges )
            {
                if( e.end1 == null || e.end2 == null ) continue;
                var go = CreateEdgeLineObject( e );
                go.transform.SetParent( _runtimeContainer.transform, false );
                _runtimeEdgeObjects.Add( go );
            }
        }

        private GameObject CreateTetGameObject( FlowTetrahedron t )
        {
            var go = new GameObject( "Tet" );
            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            mr.material = CreateTransparentMaterial( TetFillColor );

            // Build mesh out of 4 triangular faces
            var mesh = new Mesh();
            vector3sToMesh( t, mesh );
            mf.sharedMesh = mesh;

            // Also add a small wireframe overlay using LineRenderer
            var wire = new GameObject( "wire" ).AddComponent<LineRenderer>();
            wire.transform.SetParent( go.transform, false );
            wire.positionCount = 12; // 6 edges * 2
            wire.loop = false;
            wire.useWorldSpace = false;
            wire.widthMultiplier = EdgeWidth * 0.6f;
            wire.material = CreateUnlitColorMaterial( TetWireColor );
            wire.numCapVertices = 2;
            wire.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            wire.receiveShadows = false;

            Vector3[] edges = new Vector3[]
            {
                t.v0.pos, t.v1.pos,
                t.v0.pos, t.v2.pos,
                t.v0.pos, t.v3.pos,
                t.v1.pos, t.v2.pos,
                t.v1.pos, t.v3.pos,
                t.v2.pos, t.v3.pos,
            };
            wire.SetPositions( edges );

            return go;
        }

        // Helper: create mesh vertices & triangles for tetrahedron
        private void vector3sToMesh( FlowTetrahedron t, Mesh mesh )
        {
            // 4 vertices: v0..v3
            var v0 = t.v0.pos;
            var v1 = t.v1.pos;
            var v2 = t.v2.pos;
            var v3 = t.v3.pos;

            // Triangles (4 faces): each face is a triangle made of three of the four vertices
            // We'll keep each face's vertex order so normals point outward in most cases.
            var vertices = new Vector3[] { v0, v1, v2, v3 };
            var triangles = new int[]
            {
                0,1,2, // face v0,v1,v2
                0,3,1, // face v0,v3,v1
                0,2,3, // face v0,v2,v3
                1,3,2  // face v1,v3,v2
            };

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }

        private GameObject CreateEdgeLineObject( FlowEdge e )
        {
            var go = new GameObject( "Edge" );
            var lr = go.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.useWorldSpace = false;
            Vector3 p0 = e.end1.pos;
            Vector3 p1 = e.end2.pos;
            lr.SetPosition( 0, p0 );
            lr.SetPosition( 1, p1 );
            lr.widthMultiplier = EdgeWidth;
            lr.material = CreateUnlitColorMaterial( EdgeColor );
            lr.numCapVertices = 2;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            return go;
        }

        private void DestroyRuntimeObjectsImmediate()
        {
            // In Editor, destroy immediate; in play mode, destroy normally
            if( _runtimeContainer == null ) return;

#if UNITY_EDITOR
            if( !Application.isPlaying )
            {
                // Destroy children
                for( int i = _runtimeContainer.transform.childCount - 1; i >= 0; --i )
                {
                    var child = _runtimeContainer.transform.GetChild( i );
                    Undo.DestroyObjectImmediate( child.gameObject );
                }
                Undo.DestroyObjectImmediate( _runtimeContainer );
            }
            else
#endif
            {
                DestroyRuntimeObjects();
            }

            _runtimeTetObjects.Clear();
            _runtimeEdgeObjects.Clear();
            _runtimeContainer = null;
        }

        private void DestroyRuntimeObjects()
        {
            if( _runtimeContainer == null ) return;
            foreach( var g in _runtimeTetObjects )
                if( g != null ) Destroy( g );
            foreach( var g in _runtimeEdgeObjects )
                if( g != null ) Destroy( g );
            if( _runtimeContainer != null ) Destroy( _runtimeContainer );

            _runtimeTetObjects.Clear();
            _runtimeEdgeObjects.Clear();
            _runtimeContainer = null;
        }

        #endregion

        #region Utilities: materials & safe compute

        private Material CreateTransparentMaterial( Color color )
        {
            var mat = new Material( Shader.Find( "Standard" ) );
            // Configure standard shader to be transparent
            mat.SetFloat( "_Mode", 3 ); // 3 == transparent in Standard shader
            mat.SetInt( "_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha );
            mat.SetInt( "_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha );
            mat.SetInt( "_ZWrite", 0 );
            mat.DisableKeyword( "_ALPHATEST_ON" );
            mat.EnableKeyword( "_ALPHABLEND_ON" );
            mat.DisableKeyword( "_ALPHAPREMULTIPLY_ON" );
            mat.renderQueue = 3000;
            var c = color;
            mat.color = c;
            return mat;
        }

        private Material CreateUnlitColorMaterial( Color color )
        {
            // Try to find an unlit shader for crisp lines; fallback to Standard
            var shader = Shader.Find( "Unlit/Color" ) ?? Shader.Find( "Sprites/Default" ) ?? Shader.Find( "Standard" );
            var mat = new Material( shader );
            mat.color = color;
            return mat;
        }

        /// <summary>
        /// Safety wrapper to call the tetrahedralizer. If the method throws or returns null lists,
        /// returns empty lists to avoid breaking the visualizer.
        /// </summary>
        private (List<FlowNode> nodes, List<FlowEdge> edges, List<FlowTetrahedron> tets) ComputeDelaunaySafely( IList<Vector3> pts )
        {
            try
            {
                var r = ComputeDelaunayTetrahedralization( pts );
                return (r.nodes ?? new List<FlowNode>(), r.edges ?? new List<FlowEdge>(), r.tets ?? new List<FlowTetrahedron>());
            }
            catch( System.Exception ex )
            {
                Debug.LogWarning( $"FlowVisualizer: ComputeDelaunayTetrahedralization threw an exception: {ex.Message}" );
                Debug.LogException( ex );
                return (new List<FlowNode>(), new List<FlowEdge>(), new List<FlowTetrahedron>());
            }
        }

        private void ClearCachedData()
        {
            _nodes = null;
            _edges = null;
            _tets = null;
        }

        #endregion

        #region Helper: compute function placeholder (calls your implementation)

        // IMPORTANT:
        // This method must exist in your codebase: the exact signature you provided in the prompt.
        // If it's implemented in another class, ensure the namespace / class is accessible.
        // The visualizer expects it to be available at compile-time, so it's declared extern-style here.
        //
        // If your project already contains the function, the below method call will bind to it.
        // If not, you must implement such a static method somewhere:
        // public static (List<FlowNode> nodes, List<FlowEdge> edges, List<FlowTetrahedron> tets) ComputeDelaunayTetrahedralization(IList<Vector3> inputPoints)
        //
        // For the sake of explicitness (and to avoid compile-time errors if you haven't implemented it),
        // we declare an external call. Replace or remove this wrapper as needed in your project.
        private static (List<FlowNode> nodes, List<FlowEdge> edges, List<FlowTetrahedron> tets) ComputeDelaunayTetrahedralization( IList<Vector3> inputPoints )
        {
            return DelaunayTetrahedralizer.ComputeDelaunayTetrahedralization( inputPoints );
        }

        #endregion

        #region Notes about tetrahedron volume equation

        /*
         * The FlowTetrahedron.GetVolume method uses the standard parallelepiped / scalar triple product formula.
         *
         * parallelepipedVolume = dot( v1 - v0, cross( v2 - v0, v3 - v0 ) )
         *
         * Explanation:
         *  - v0, v1, v2, v3 are position vectors of the tetrahedron vertices.
         *  - (v1 - v0) is one edge vector from v0 to v1.
         *  - cross( v2 - v0, v3 - v0 ) produces a vector orthogonal to the plane formed by (v2 - v0) and (v3 - v0);
         *    its magnitude equals the area of the parallelogram spanned by those two vectors.
         *  - dot( v1 - v0, cross(...) ) computes the scalar triple product, which equals the signed volume
         *    of the parallelepiped defined by the three vectors.
         *  - The tetrahedron is 1/6 of that parallelepiped (because parallelepiped built from three edge vectors
         *    contains 6 congruent tetrahedra), so volume = abs(parallelepipedVolume) / 6.
         *
         * This is the formula used by FlowTetrahedron.GetVolume.
         */

        #endregion
    }
}
