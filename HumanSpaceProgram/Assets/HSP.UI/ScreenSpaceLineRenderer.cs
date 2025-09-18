using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace HSP.UI
{
    [RequireComponent( typeof( Camera ) )]
    [DisallowMultipleComponent]
    public class ScreenSpaceLineRenderer : MonoBehaviour
    {
        [StructLayout( LayoutKind.Sequential )]
        private struct LineInstance
        {
            public Vector3[] points;
            public Color color;
            public float thicknessPx;
            public int pointCount;
        }

        public enum CornerType
        {
            Mitered,
            // Beveled
        }

        static readonly int ID_GlobalColor = Shader.PropertyToID( "_GlobalColor" );
        static readonly int ID_DepthBias = Shader.PropertyToID( "_DepthBias" );

        public Material lineMaterial;
        public Color globalColor = Color.white;
        public float depthBias = 0.0005f;

        public CornerType cornerType = CornerType.Mitered;

        List<LineInstance> _lines = new();

        Camera _cam;
        CommandBuffer _commandBuffer;
        MaterialPropertyBlock _propertyBlock;

        Mesh _mesh;
        Vector3[] _verts;
        Color[] _cols;
        Vector4[] _uvChannel1; // next point xyz, thickness in w
        Vector4[] _uvChannel2; // prev point xyz, side sign in w
        int[] _idx;

        int _capacity = 0;
        bool _meshStale = false;

        /// <summary>
        /// Adds a multi-point line.
        /// </summary>
        public void AddLine( Vector3[] points, Color color, float thicknessPx = 2f )
        {
            if( points == null || points.Length < 2 ) return;

            var line = new LineInstance
            {
                points = new Vector3[points.Length],
                color = color,
                thicknessPx = thicknessPx,
                pointCount = points.Length
            };

            Array.Copy( points, line.points, points.Length );
            _lines.Add( line );
            _meshStale = true;
        }

        /// <summary>
        /// Adds a simple 2-point line.
        /// </summary>
        public void AddLine( Vector3 worldA, Vector3 worldB, Color color, float thicknessPx = 2f )
        {
            AddLine( new Vector3[] { worldA, worldB }, color, thicknessPx );
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
                _mesh.name = "ScreenSpaceLineMesh";
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
            foreach( var line in _lines )
            {
                if( line.pointCount >= 2 && IsLineVisible( line ) )
                {
                    totalVerts += line.pointCount * 2; // 2 vertices per point (left/right)
                    totalIdx += (line.pointCount - 1) * 6; // 2 triangles per segment
                }
            }

            // Reallocate arrays if capacity changed.
            if( totalVerts > _capacity )
            {
                _capacity = Mathf.NextPowerOfTwo( totalVerts );
                _verts = new Vector3[_capacity];
                _cols = new Color[_capacity];
                _uvChannel1 = new Vector4[_capacity];
                _uvChannel2 = new Vector4[_capacity];
                _idx = new int[_capacity * 3];
            }

            int vertexOffset = 0;
            int indexOffset = 0;
            foreach( var line in _lines )
            {
                if( line.pointCount < 2 || !IsLineVisible( line ) ) continue;

                GenerateLineMesh( line, vertexOffset, indexOffset, out int newVertCount, out int newIndexCount );
                vertexOffset += newVertCount;
                indexOffset += newIndexCount;
            }

            _mesh.Clear( false );
            _mesh.SetVertices( _verts, 0, vertexOffset );
            _mesh.SetColors( _cols, 0, vertexOffset );
            _mesh.SetUVs( 1, _uvChannel1, 0, vertexOffset );
            _mesh.SetUVs( 2, _uvChannel2, 0, vertexOffset );
            _mesh.SetIndices( _idx, 0, indexOffset, MeshTopology.Triangles, 0, false );
            _mesh.RecalculateBounds();

            _meshStale = false;
        }

        private bool IsLineVisible( LineInstance line )
        {
            if( _cam == null ) 
                return true;

            Vector3 camPos = _cam.transform.position;
            Vector3 camForward = _cam.transform.forward;

            for( int i = 0; i < line.pointCount; i++ )
            {
                Vector3 toPoint = line.points[i] - camPos;
                if( Vector3.Dot( toPoint, camForward ) > 0 ) // Any point in front of camera.
                {
                    return true;
                }
            }

            return false;
        }

        private void GenerateLineMesh( LineInstance line, int vertexOffset, int indexOffset, out int vertCount, out int indexCount )
        {
            var points = line.points;
            int pointCount = line.pointCount;
            float thickness = line.thicknessPx;
            Color color = line.color;

            vertCount = pointCount * 2;
            indexCount = (pointCount - 1) * 6;
            for( int i = 0; i < pointCount; ++i )
            {
                Vector3 current = points[i];
                Vector3 next = i < pointCount - 1 ? points[i + 1] : current;
                Vector3 prev = i > 0 ? points[i - 1] : current;

                int vertIdx = vertexOffset + i * 2;

                // left vertex (+1 side)
                _verts[vertIdx] = current;
                _cols[vertIdx] = color;
                _uvChannel1[vertIdx] = new Vector4( next.x, next.y, next.z, thickness );
                _uvChannel2[vertIdx] = new Vector4( prev.x, prev.y, prev.z, +1f );

                // right vertex (-1 side)
                _verts[vertIdx + 1] = current;
                _cols[vertIdx + 1] = color;
                _uvChannel1[vertIdx + 1] = new Vector4( next.x, next.y, next.z, thickness );
                _uvChannel2[vertIdx + 1] = new Vector4( prev.x, prev.y, prev.z, -1f );
            }

            // Generate triangles.
            for( int i = 0; i < pointCount - 1; ++i )
            {
                int baseIdx = vertexOffset + i * 2;
                int triIdx = indexOffset + i * 6;

                _idx[triIdx + 0] = baseIdx + 0;
                _idx[triIdx + 1] = baseIdx + 1;
                _idx[triIdx + 2] = baseIdx + 2;

                _idx[triIdx + 3] = baseIdx + 2;
                _idx[triIdx + 4] = baseIdx + 1;
                _idx[triIdx + 5] = baseIdx + 3;
            }
        }

        void Awake()
        {
            if( !TryGetComponent<Camera>( out _cam ) )
                _cam = Camera.main;
        }

        void OnEnable()
        {
            if( _cam != null )
                _cam.depthTextureMode |= DepthTextureMode.Depth;
            CreateMeshIfNotExists();
        }

        void OnDisable()
        {
            if( _mesh != null )
            {
                DestroyImmediate( _mesh );
                _mesh = null;
            }

            if( _commandBuffer != null )
            {
                _commandBuffer.Release();
                _commandBuffer = null;
            }

            _verts = null;
            _cols = null;
            _uvChannel1 = null;
            _uvChannel2 = null;
            _idx = null;
            _capacity = 0;
        }

        void OnPreRender()
        {
            if( lineMaterial == null )
            {
                return;
            }
            if( _cam == null ) _cam = Camera.current;
            if( _cam == null )
            {
                return;
            }

            if( _lines.Count == 0 )
            {
                return;
            }

            if( _commandBuffer == null )
            {
                _commandBuffer = new CommandBuffer();
                _commandBuffer.name = "ScreenSpaceLineRenderer";
            }
            this._cam.RemoveCommandBuffer( CameraEvent.AfterForwardOpaque, _commandBuffer );

            RebuildMeshIfNeeded();

            if( _propertyBlock == null )
                _propertyBlock = new MaterialPropertyBlock();
            _propertyBlock.Clear();
            _propertyBlock.SetColor( ID_GlobalColor, globalColor );
            _propertyBlock.SetFloat( ID_DepthBias, depthBias );

            _commandBuffer.Clear();

            if( _cam.targetTexture != null )
            {
                _commandBuffer.SetRenderTarget( _cam.targetTexture );
            }
            else
            {
                _commandBuffer.SetRenderTarget( BuiltinRenderTextureType.CameraTarget );
            }

            _commandBuffer.DrawMesh( _mesh, Matrix4x4.identity, lineMaterial, 0, 0, _propertyBlock );

            this._cam.AddCommandBuffer( CameraEvent.AfterForwardOpaque, _commandBuffer );
        }

#if UNITY_EDITOR
        [ContextMenu( "Create Test Lines" )]
        public void CreateTestLines()
        {
            Debug.Log( "Creating test lines..." );
            Clear();

            // Create lines relative to camera position and orientation
            Vector3 camPos = _cam.transform.position;
            Vector3 camForward = _cam.transform.forward;
            Vector3 camRight = _cam.transform.right;
            Vector3 camUp = _cam.transform.up;

            // Create a sphere of lines in front of the camera
            Vector3 center = camPos + camForward * 15f;

            for( int i = 0; i < 200; ++i )
            {
                float a = i / 200f * Mathf.PI * 2f;
                float b = (i % 20) / 20f * Mathf.PI; // elevation angle

                Vector3 offset = new Vector3( Mathf.Cos( a ) * Mathf.Sin( b ), Mathf.Sin( a ) * Mathf.Sin( b ), Mathf.Cos( b ) ) * 10f;
                Vector3 p0 = center + offset;
                Vector3 p1 = center + offset * 1.1f; // slightly longer line

                AddLine( p0, p1, Color.HSVToRGB( i / 200f, 1f, 1f ), 5f + (i % 3) );
            }
            Debug.Log( $"Created {_lines.Count} test lines" );
        }

        [ContextMenu( "Create Simple Test Lines" )]
        public void CreateSimpleTestLines()
        {
            Debug.Log( "Creating simple test lines..." );
            Clear();
            // Create simple lines in front of camera
            Vector3 camPos = _cam.transform.position;
            Vector3 camForward = _cam.transform.forward;
            Vector3 camRight = _cam.transform.right;
            Vector3 camUp = _cam.transform.up;

            // Create a simple cross pattern
            Vector3 center = camPos + camForward * 10f;
            AddLine( center - camRight * 5f, center + camRight * 5f, Color.red, 10f );
            AddLine( center - camUp * 5f, center + camUp * 5f, Color.green, 10f );
            AddLine( center - camForward * 2f, center + camForward * 2f, Color.blue, 10f );

            Debug.Log( $"Created {_lines.Count} simple test lines" );
        }

        [ContextMenu( "Create Curve Test Lines" )]
        public void CreateCurveTestLines()
        {
            Debug.Log( "Creating curve test lines..." );
            Clear();

            Vector3 camPos = _cam.transform.position;
            Vector3 camForward = _cam.transform.forward;
            Vector3 camRight = _cam.transform.right;
            Vector3 camUp = _cam.transform.up;

            Vector3 center = camPos + camForward * 15f;

            // Create a sine wave curve
            Vector3[] sineWave = new Vector3[20];
            for( int i = 0; i < 20; ++i )
            {
                float t = i / 19f;
                float x = (t - 0.5f) * 20f;
                float y = Mathf.Sin( t * Mathf.PI * 4f ) * 5f;
                sineWave[i] = center + camRight * x + camUp * y;
            }
            AddLine( sineWave, Color.cyan, 8f );

            // Create a circle
            Vector3[] circle = new Vector3[32];
            for( int i = 0; i < 32; ++i )
            {
                float angle = i / 32f * Mathf.PI * 2f;
                float x = Mathf.Cos( angle ) * 8f;
                float y = Mathf.Sin( angle ) * 8f;
                circle[i] = center + camRight * x + camUp * y;
            }
            AddLine( circle, Color.yellow, 6f );

            // Create a zigzag
            Vector3[] zigzag = new Vector3[10];
            for( int i = 0; i < 10; ++i )
            {
                float x = (i / 9f - 0.5f) * 16f;
                float y = (i % 2 == 0 ? 1f : -1f) * 3f;
                zigzag[i] = center + camRight * x + camUp * y;
            }
            AddLine( zigzag, Color.magenta, 12f );

            Debug.Log( $"Created {_lines.Count} curve test lines" );
        }
#endif
    }
}
