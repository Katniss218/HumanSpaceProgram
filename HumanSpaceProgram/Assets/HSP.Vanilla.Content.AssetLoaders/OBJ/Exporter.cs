using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace HSP.Vanilla.Content.AssetLoaders.OBJ
{
    public static class Exporter
    {
#warning TODO - swap to textwriter to match importer
        public static void Export( string filePath, Mesh mesh )
        {
            if( mesh == null )
                throw new ArgumentNullException( nameof( mesh ) );

            // Ensure directory exists
            string dir = Path.GetDirectoryName( filePath );
            if( !string.IsNullOrEmpty( dir ) && !Directory.Exists( dir ) )
                Directory.CreateDirectory( dir );

            StringBuilder sb = new StringBuilder();

            sb.AppendLine( "# Exported by HSP OBJ Exporter" );
            sb.AppendLine( $"o {mesh.name}" );

            // Vertices
            foreach( Vector3 v in mesh.vertices )
            {
                // Write internal unity coordinates directly to roundtrip with the simple importer
                // which swaps Y/Z or X axis depending on parser implementation, 
                // but our importer seems to read raw xyz.
                sb.AppendLine( $"v {v.x} {v.y} {v.z}" );
            }

            // UVs
            foreach( Vector2 uv in mesh.uv )
            {
                sb.AppendLine( $"vt {uv.x} {uv.y}" );
            }

            // Normals
            foreach( Vector3 vn in mesh.normals )
            {
                sb.AppendLine( $"vn {vn.x} {vn.y} {vn.z}" );
            }

            // Faces
            // OBJ indices are 1-based
            int submeshCount = mesh.subMeshCount;
            for( int i = 0; i < submeshCount; i++ )
            {
                // Unity GetTriangles returns a list of indices that form triangles (stride 3)
                int[] triangles = mesh.GetTriangles( i );

                for( int t = 0; t < triangles.Length; t += 3 )
                {
                    int i0 = triangles[t] + 1;
                    int i1 = triangles[t + 1] + 1;
                    int i2 = triangles[t + 2] + 1;

                    // Format: f v/vt/vn
                    // We assume dense arrays (uv/normals exist for all verts) if the array isn't empty.

                    bool hasUV = mesh.uv.Length > 0;
                    bool hasNormals = mesh.normals.Length > 0;

                    if( hasUV && hasNormals )
                    {
                        sb.AppendLine( $"f {i0}/{i0}/{i0} {i1}/{i1}/{i1} {i2}/{i2}/{i2}" );
                    }
                    else if( hasUV && !hasNormals )
                    {
                        sb.AppendLine( $"f {i0}/{i0} {i1}/{i1} {i2}/{i2}" );
                    }
                    else if( !hasUV && hasNormals )
                    {
                        sb.AppendLine( $"f {i0}//{i0} {i1}//{i1} {i2}//{i2}" );
                    }
                    else
                    {
                        sb.AppendLine( $"f {i0} {i1} {i2}" );
                    }
                }
            }

            File.WriteAllText( filePath, sb.ToString() );
        }
    }
}