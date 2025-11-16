using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using HSP.ResourceFlow;

/// <summary>
/// Playmode-only visualizer for a single FlowTank.
/// - Use the context menu "Refresh Visualizer" (right-click the component in the inspector) to capture the current state.
/// - Creates a LineRenderer per edge and a small quad showing the fill fraction for that edge.
/// </summary>
[DisallowMultipleComponent]
public class FlowTankVisualizer : MonoBehaviour
{
    [Header( "Target" )]
    [Tooltip( "The FlowTank instance to visualize. Assign at runtime or in inspector." )]
    public HSP.ResourceFlow.FlowTank TargetTank;

    [Header( "Visual Settings" )]
    [Tooltip( "Material used to draw edges (LineRenderer)." )]
    public Material EdgeMaterial;
    [Tooltip( "Material used to draw fill quads." )]
    public Material FillMaterial;
    [Tooltip( "Minimum world thickness for edges/fill visual." )]
    public float Thickness = 0.02f;
    [Tooltip( "Maximum length (world units) of fill quad — fill is scaled along edge length but clamped." )]
    public float MaxFillQuadLength = 2.0f;
    [Tooltip( "Whether to create small colliders on fill quads to allow clicking them." )]
    public bool CreateColliders = true;

    // internal containers
    private GameObject _visualRoot;

    private const string ROOT_NAME = "FlowTankVisualizer_Root";

    #region Unity lifecycle
    private void Awake()
    {
        if( EdgeMaterial == null )
        {
            // simple fallback materials
            EdgeMaterial = new Material( Shader.Find( "Sprites/Default" ) );
            EdgeMaterial.hideFlags = HideFlags.DontSave;
        }
        if( FillMaterial == null )
        {
            FillMaterial = new Material( Shader.Find( "Sprites/Default" ) );
            FillMaterial.hideFlags = HideFlags.DontSave;
            FillMaterial.SetFloat( "_Mode", 2f );
        }

        if( TargetTank == null )
        {
            Vector3[] nodePositions = new Vector3[]
            {
                new Vector3( 0, 0, 0 ),
                new Vector3( 1, 0, 0 ),
                new Vector3( 0, 1, 0 ),
                new Vector3( 0, 0, 1 )
            };
            ResourceInlet[] inlets = new ResourceInlet[]
            {
                new ResourceInlet( 1, new Vector3( 0, 2, 0 ) ),
                new ResourceInlet( 1, new Vector3( 0, -2, 0 ) )
            };
            TargetTank = new FlowTank( 50000 );
            TargetTank.SetNodes( nodePositions, inlets );
            Substance sub = new Substance()
            {
                BulkModulus = 2e6f,
                Density = 1000f,
                ReferencePressure = 101325f,
                SpecificGasConstant = 0f,
                Phase = SubstancePhase.Liquid,
                DisplayName = "TestLiquid",
                UIColor = Color.cyan
            };

            var contents = new SubstanceStateCollection( new SubstanceState( 25000, sub ) );
            TargetTank.Contents = contents;
            TargetTank.FluidAcceleration = Vector3.down * 10;
            TargetTank.DistributeFluids();
        }

        // ensure a root object exists
        EnsureRoot();
    }
    #endregion

    private void EnsureRoot()
    {
        if( _visualRoot == null )
        {
            Transform existing = transform.Find( ROOT_NAME );
            if( existing != null )
                _visualRoot = existing.gameObject;
            else
            {
                _visualRoot = new GameObject( ROOT_NAME );
                _visualRoot.transform.SetParent( transform, false );
            }
        }
    }

    /// <summary>
    /// Refresh the visualization to reflect the current internal state of the target tank.
    /// Use the context menu in the inspector during Play mode or call this from code.
    /// </summary>
    [ContextMenu( "Refresh Visualizer" )]
    public void RefreshVisualizer()
    {
        if( TargetTank == null )
        {
            Debug.LogWarning( "[FlowTankVisualizer] No TargetTank assigned." );
            return;
        }

        EnsureRoot();

        // Clear old visuals
        for( int i = _visualRoot.transform.childCount - 1; i >= 0; i-- )
            Destroy( _visualRoot.transform.GetChild( i ).gameObject );

        IReadOnlyList<FlowEdge> edges = TargetTank.Edges;
        var nodes = TargetTank.Nodes;
        var contentsInEdges = TargetTank.ContentsInEdges; // IReadonlySubstanceStateCollection[]

        if( edges == null )
        {
            Debug.LogWarning( "[FlowTankVisualizer] Target tank has null Edges." );
            return;
        }
        if( nodes == null )
        {
            Debug.LogWarning( "[FlowTankVisualizer] Target tank has null Nodes." );
            return;
        }
        if( contentsInEdges == null )
        {
            Debug.LogWarning( "[FlowTankVisualizer] Target tank has null contentsInEdges." );
            return;
        }

        float tankVolume = TargetTank.Volume;
        float tankTemp = TargetTank.Temperature;

        // For pressure calculation we need mixture pressure per edge contents. If ComputeMixturePressure requires the whole tank contents,
        // we follow the same approach as your distribution routine: compute pressure from the mixture contents of the entire tank if needed.
        // However we'll attempt a per-edge compute using the edge's collection (if available) and fall back to global Contents if necessary.
        // For simplicity here we compute pressure per-edge from the edge's collection using the same helper method if present.

        int edgeCount = edges.Count;
        for( int ei = 0; ei < edgeCount; ei++ )
        {
            FlowEdge edge = edges[ei];

            Vector3 pA = edge.end1.pos;
            Vector3 pB = edge.end2.pos;

            // Create a container object per-edge
            GameObject edgeGO = new GameObject( $"Edge_{ei}" );
            edgeGO.transform.SetParent( _visualRoot.transform, false );

            // Position is irrelevant — we'll set line renderer positions in world coords
            LineRenderer lr = edgeGO.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.positionCount = 2;
            lr.SetPosition( 0, transform.TransformPoint( pA ) );
            lr.SetPosition( 1, transform.TransformPoint( pB ) );
            lr.widthCurve = new UnityEngine.AnimationCurve( new Keyframe( 0, Thickness ), new Keyframe( 1, Thickness ) );
            lr.material = EdgeMaterial;
            lr.numCapVertices = 4;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;

            // compute filled fraction for this edge
            float filledFraction = 0f;
            try
            {
                float edgeCapacity = Mathf.Max( 1e-9f, edge.Volume ); // avoid div0
                float filledVolume = 0f;

                if( contentsInEdges != null && ei < contentsInEdges.Length && contentsInEdges[ei] != null )
                {
                    var subArr = contentsInEdges[ei].ToArray(); // SubstanceState[]

                    // compute pressure for this subcollection if helper available; else approximate using tank pressure
                    float pressure = 0f;
                    try
                    {
                        if( subArr != null && subArr.Length > 0 )
                        {
                            pressure = HSP.ResourceFlow.SubstanceState.GetMixturePressure( subArr, tankVolume, tankTemp );
                        }
                        else
                        {
                            // no substances: pressure 0
                            pressure = 0f;
                        }
                    }
                    catch( Exception )
                    {
                        // fallback
                        pressure = 0f;
                    }

                    foreach( var ss in subArr )
                    {
                        try
                        {
                            float v = ss.GetVolumeAtPressure( pressure, tankTemp );
                            filledVolume += Mathf.Max( 0f, v );
                        }
                        catch( Exception )
                        {
                            // if GetVolumeAtPressure not present or fails, try approximating from a 'Volume' property
                            var prop = ss.GetType().GetProperty( "Volume" );
                            if( prop != null )
                            {
                                var val = prop.GetValue( ss );
                                if( val is float vf ) filledVolume += vf;
                            }
                        }
                    }
                }

                filledFraction = Mathf.Clamp01( filledVolume / edgeCapacity );
            }
            catch( Exception ex )
            {
                Debug.LogWarning( $"[FlowTankVisualizer] Exception computing filled fraction for edge {ei}: {ex.Message}" );
                filledFraction = 0f;
            }

            // Create fill quad at midpoint; oriented along edge direction.
            Vector3 mid = (transform.TransformPoint( pA ) + transform.TransformPoint( pB )) * 0.5f;
            Vector3 dir = (transform.TransformPoint( pB ) - transform.TransformPoint( pA ));
            float edgeLength = dir.magnitude;
            if( edgeLength <= 1e-6f ) edgeLength = 1e-6f;
            Vector3 forward = dir.normalized;
            // choose an up vector that's not parallel to forward
            Vector3 up = Vector3.up;
            if( Mathf.Abs( Vector3.Dot( forward, up ) ) > 0.99f )
                up = Vector3.forward;
            Vector3 right = Vector3.Cross( up, forward ).normalized; // thickness direction

            // quad length visual: scale by filledFraction * edgeLength, but clamp to MaxFillQuadLength so very long edges don't create huge quads
            float quadLengthWorld = Mathf.Min( MaxFillQuadLength, edgeLength * filledFraction );
            float halfLen = quadLengthWorld * 0.5f;

            GameObject fill = new GameObject( $"EdgeFill_{ei}" );
            fill.transform.SetParent( edgeGO.transform, false );
            fill.transform.position = mid;
            // orient the quad so that its local X is along forward, local Y along right
            fill.transform.rotation = Quaternion.LookRotation( forward, right );

            MeshFilter mf = fill.AddComponent<MeshFilter>();
            MeshRenderer mr = fill.AddComponent<MeshRenderer>();
            mr.sharedMaterial = FillMaterial;

            Mesh quad = BuildQuadMesh( quadLengthWorld, Thickness );
            mf.sharedMesh = quad;

            // Offset the quad along forward so that it "grows" from the lower side of the edge (optional).
            // We'll align it centered at midpoint; user can interpret accordingly.

            if( CreateColliders )
            {
                var col = fill.AddComponent<BoxCollider>();
                col.size = new Vector3( quadLengthWorld, Thickness * 1.5f, 0.02f );
            }

            // color the fill gradient based on filledFraction
            if( mr.sharedMaterial != null )
            {
                // the fallback material (Sprites/Default) uses color
                Color c = Color.Lerp( new Color( 0.8f, 0.8f, 1f ), new Color( 0.1f, 0.2f, 0.8f ), filledFraction );
                mr.sharedMaterial.color = c;
            }

            // attach a small helper to print details on click
            var dbg = edgeGO.AddComponent<EdgeDebugClick>();
            dbg.Visualizer = this;
            dbg.EdgeIndex = ei;
            dbg.EdgeRef = edge;
        } // edges
    }

    /// <summary>
    /// Print details about an edge (index) to the console.
    /// You can call this from other code, or click the visualized edge if colliders are present.
    /// </summary>
    public void PrintEdgeDetails( int edgeIndex )
    {
        if( TargetTank == null )
        {
            Debug.Log( "[FlowTankVisualizer] No TargetTank assigned." );
            return;
        }
        var edges = TargetTank.Edges;
        var contentsInEdges = TargetTank.ContentsInEdges;
        if( edges == null || edgeIndex < 0 || edgeIndex >= edges.Count )
        {
            Debug.LogWarning( "[FlowTankVisualizer] Invalid edge index." );
            return;
        }

        var edge = edges[edgeIndex];
        Debug.Log( $"[FlowTankVisualizer] Edge {edgeIndex}: endpoints {edge.end1.pos}, {edge.end2.pos}, Volume cap = {edge.Volume}" );

        if( contentsInEdges != null && edgeIndex < contentsInEdges.Length && contentsInEdges[edgeIndex] != null )
        {
            var arr = contentsInEdges[edgeIndex].ToArray();
            float pressure = 0f;
            try
            {
                pressure = HSP.ResourceFlow.SubstanceState.GetMixturePressure( arr, TargetTank.Volume, TargetTank.Temperature );
            }
            catch { pressure = 0f; }

            float totalVol = 0f;
            foreach( var ss in arr )
            {
                float v = 0f;
                try { v = ss.GetVolumeAtPressure( pressure, TargetTank.Temperature ); }
                catch
                {
                    var prop = ss.GetType().GetProperty( "Volume" );
                    if( prop != null )
                    {
                        var val = prop.GetValue( ss );
                        if( val is float vf ) v = vf;
                    }
                }
                totalVol += v;
                string name = ss.Substance?.ToString() ?? ss.ToString();
                Debug.Log( $"   Substance: {name}   vol@p={v:0.000}" );
            }
            Debug.Log( $"   Total edge content volume = {totalVol:0.000}" );
        }
        else
        {
            Debug.Log( "   No contents recorded for this edge." );
        }
    }

    #region Helpers
    private Mesh BuildQuadMesh( float length, float thickness )
    {
        // Build a simple quad mesh centered at origin, extending along +X by half length and -X by half length,
        // and thickness along Y (small). Z is zero.
        float halfLen = length * 0.5f;
        float halfTh = thickness * 0.5f;

        Mesh m = new Mesh();
        m.name = "FillQuadMesh";
        Vector3[] v = new Vector3[4]
        {
            new Vector3(-halfLen, -halfTh, 0f),
            new Vector3(halfLen, -halfTh, 0f),
            new Vector3(-halfLen, halfTh, 0f),
            new Vector3(halfLen, halfTh, 0f)
        };
        int[] tri = new int[6] { 0, 2, 1, 1, 2, 3 };
        Vector2[] uv = new Vector2[4] { new Vector2( 0, 0 ), new Vector2( 1, 0 ), new Vector2( 0, 1 ), new Vector2( 1, 1 ) };
        m.vertices = v;
        m.triangles = tri;
        m.uv = uv;
        m.RecalculateNormals();
        return m;
    }
    #endregion
}

/// <summary>
/// Small helper component added to each edge GameObject to allow clicking to print edge details.
/// Uses OnMouseDown (physics) — requires colliders and a camera with physics raycasting.
/// </summary>
public class EdgeDebugClick : MonoBehaviour
{
    public FlowTankVisualizer Visualizer;
    public int EdgeIndex;
    public HSP.ResourceFlow.FlowEdge EdgeRef;

    private void OnMouseDown()
    {
        if( Visualizer != null )
        {
            Visualizer.PrintEdgeDetails( EdgeIndex );
        }
    }
}
