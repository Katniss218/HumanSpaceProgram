using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace HSP.UI
{
    [RequireComponent( typeof( Camera ) )]
    public class PolyLineRenderer : MonoBehaviour
    {
        [StructLayout( LayoutKind.Sequential )]
        private struct LineInstance
        {
            public Vector3[] points;
            public Color color;
            public float thickness; // px or meters depending on mode
            public int pointCount;
        }

        static readonly int ID_GlobalColor = Shader.PropertyToID( "_GlobalColor" );
        static readonly int ID_DepthBias = Shader.PropertyToID( "_DepthBias" );
        static readonly int ID_WorldSpaceWidth = Shader.PropertyToID( "_WorldSpaceWidth" );

        [Tooltip( "Shader to use. Should be Hidden/HSP/PolyLine" )]
        public Material lineMaterial;

        [Header( "Settings" )]
        [Tooltip( "If true, thickness is in Meters. If false, in Screen Pixels." )]
        public bool useWorldSpaceWidth = true;
        public Color globalColor = Color.white;
        public float depthBias = 0.0005f;

        // Internal State
        List<LineInstance> _lines = new();

        Camera _cam;
        CommandBuffer _commandBuffer;
        MaterialPropertyBlock _propertyBlock;

        Mesh _mesh;
        // Mesh buffers
        Vector3[] _verts;
        Color[] _cols;
        Vector4[] _uvChannel1;
        Vector4[] _uvChannel2;
        int[] _idx;

        int _capacity = 0;
        bool _meshStale = false;

        /// <summary>
        /// Fast-path to update colors of existing lines without rebuilding topology.
        /// Assumes strict 1:1 mapping: colors[i] corresponds to the i-th line added.
        /// </summary>
        public void UpdateLineColors( IList<Color> newColors )
        {
            if( _lines.Count != newColors.Count )
                return;
            if( _mesh == null || _cols == null )
                return;

            bool changed = false;

            // We need to update two places:
            // 1. The internal struct (so rebuilds persist color)
            // 2. The mesh buffer (for immediate rendering)

            // We assume 2 vertices per point. 
            // A 2-point line has vertices at indices: [lineIndex * 4] to [lineIndex * 4 + 3]
            int vertIdx = 0;

            for( int i = 0; i < _lines.Count; i++ )
            {
                Color c = newColors[i];

                // 1. Update Struct
                var line = _lines[i];
                line.color = c;
                _lines[i] = line;

                // 2. Update Buffer
                // NOTE: This logic assumes all lines are simple 2-point segments.
                // If you have multi-point lines, this fast-path needs more complex offset tracking.
                int count = line.pointCount;
                for( int k = 0; k < count * 2; k++ )
                {
                    if( vertIdx < _cols.Length )
                        _cols[vertIdx++] = c;
                }

                changed = true;
            }

            if( changed && _mesh.vertexCount > 0 )
            {
                // Only upload the valid range of colors corresponding to the active vertices
                _mesh.SetColors( _cols, 0, _mesh.vertexCount );
            }
        }

        public void AddLine( Vector3 worldA, Vector3 worldB, Color color, float thickness = 0.05f )
        {
            var line = new LineInstance
            {
                points = new Vector3[] { worldA, worldB },
                color = color,
                thickness = thickness,
                pointCount = 2
            };
            _lines.Add( line );
            _meshStale = true;
        }

        public void Clear()
        {
            _lines.Clear();
            _meshStale = true;
        }

        public void ForceRebuildMesh()
        {
            _meshStale = true;
        }

        private void CreateMeshIfNotExists()
        {
            if( _mesh == null )
            {
                _mesh = new Mesh();
                _mesh.name = "PolyLineMesh";
                _mesh.MarkDynamic();
                _mesh.hideFlags = HideFlags.DontSave;
            }
        }

        private void RebuildMeshIfNeeded()
        {
            if( !_meshStale )
                return;

            CreateMeshIfNotExists();

            int totalVerts = 0;
            int totalIdx = 0;

            // 1. Calculate required size
            foreach( var line in _lines )
            {
                if( line.pointCount >= 2 )
                {
                    totalVerts += line.pointCount * 2;
                    totalIdx += (line.pointCount - 1) * 6;
                }
            }

            // 2. Resize buffers
            if( totalVerts > _capacity )
            {
                _capacity = Mathf.NextPowerOfTwo( totalVerts );
                _verts = new Vector3[_capacity];
                _cols = new Color[_capacity];
                _uvChannel1 = new Vector4[_capacity];
                _uvChannel2 = new Vector4[_capacity];
                _idx = new int[_capacity * 3]; // approx ratio for tris
            }

            // 3. Fill buffers
            int vOffset = 0;
            int iOffset = 0;

            foreach( var line in _lines )
            {
                if( line.pointCount < 2 )
                    continue;
                GenerateLineMesh( line, vOffset, iOffset, out int addedV, out int addedI );
                vOffset += addedV;
                iOffset += addedI;
            }

            // 4. Upload to Mesh
            _mesh.Clear( false );
            _mesh.SetVertices( _verts, 0, vOffset );
            _mesh.SetColors( _cols, 0, vOffset );
            _mesh.SetUVs( 1, _uvChannel1, 0, vOffset );
            _mesh.SetUVs( 2, _uvChannel2, 0, vOffset );
            _mesh.SetIndices( _idx, 0, iOffset, MeshTopology.Triangles, 0, false );
            _mesh.RecalculateBounds();

            _meshStale = false;
        }

        private void GenerateLineMesh( LineInstance line, int vBase, int iBase, out int vCount, out int iCount )
        {
            var pts = line.points;
            int count = line.pointCount;
            float thick = line.thickness;
            Color col = line.color;

            vCount = count * 2;
            iCount = (count - 1) * 6;

            for( int i = 0; i < count; ++i )
            {
                Vector3 curr = pts[i];
                Vector3 next = (i < count - 1) ? pts[i + 1] : curr;
                Vector3 prev = (i > 0) ? pts[i - 1] : curr;

                int idx = vBase + i * 2;

                // Vert 1 (+Side)
                _verts[idx] = curr;
                _cols[idx] = col;
                _uvChannel1[idx] = new Vector4( next.x, next.y, next.z, thick );
                _uvChannel2[idx] = new Vector4( prev.x, prev.y, prev.z, 1f );

                // Vert 2 (-Side)
                _verts[idx + 1] = curr;
                _cols[idx + 1] = col;
                _uvChannel1[idx + 1] = new Vector4( next.x, next.y, next.z, thick );
                _uvChannel2[idx + 1] = new Vector4( prev.x, prev.y, prev.z, -1f );
            }

            // Triangles
            for( int i = 0; i < count - 1; ++i )
            {
                int v = vBase + i * 2;
                int t = iBase + i * 6;

                _idx[t + 0] = v + 0;
                _idx[t + 1] = v + 1;
                _idx[t + 2] = v + 2;

                _idx[t + 3] = v + 2;
                _idx[t + 4] = v + 1;
                _idx[t + 5] = v + 3;
            }
        }

        void Awake()
        {
            if( !TryGetComponent( out _cam ) ) _cam = Camera.main;
            if( lineMaterial == null )
                lineMaterial = new Material( Shader.Find( "Hidden/HSP/PolyLine" ) );
        }
        void OnEnable()
        {
            if( _commandBuffer == null )
            {
                _commandBuffer = new CommandBuffer { name = "PolyLineRenderer" };
            }

            if( _cam != null )
            {
                _cam.RemoveCommandBuffer( CameraEvent.AfterForwardOpaque, _commandBuffer ); // Safety cleanup
                _cam.AddCommandBuffer( CameraEvent.AfterForwardOpaque, _commandBuffer );
            }
        }

        void OnDisable()
        {
            if( _cam != null && _commandBuffer != null )
            {
                _cam.RemoveCommandBuffer( CameraEvent.AfterForwardOpaque, _commandBuffer );
            }
        }

        void OnPreRender()
        {
            // Safety check if Camera changed or component was added at runtime
            if( _cam != Camera.current && Camera.current != null ) return;

            if( lineMaterial == null || _lines.Count == 0 ) return;

            RebuildMeshIfNeeded();

            // Just update the content, don't add/remove the buffer itself
            _commandBuffer.Clear();

            if( _propertyBlock == null ) _propertyBlock = new MaterialPropertyBlock();

            _propertyBlock.Clear();
            _propertyBlock.SetColor( ID_GlobalColor, globalColor );
            _propertyBlock.SetFloat( ID_DepthBias, depthBias );
            _propertyBlock.SetFloat( ID_WorldSpaceWidth, useWorldSpaceWidth ? 1.0f : 0.0f );

            _commandBuffer.DrawMesh( _mesh, Matrix4x4.identity, lineMaterial, 0, 0, _propertyBlock );
        }
    }
}