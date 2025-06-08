using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Unity.Collections;
using UnityEngine;

namespace HSP.Vanilla.Content.AssetLoaders.OBJ
{
    public static class Importer
    {
        private static readonly NumberFormatInfo NumberFormat = CultureInfo.InvariantCulture.NumberFormat;

        public static Mesh LoadOBJ( string filePath )
        {
            string[] fileLines = File.ReadAllLines( filePath );
            string objName = Path.GetFileNameWithoutExtension( filePath );

            // OBJ uses 1-based indices, Unity uses 0-based indices.

            // These lists store the parsed OBJ data, as they can appear in any order.
            List<Vector3> objVertices = new List<Vector3>();
            List<Vector2> objUVs = new List<Vector2>();
            List<Vector3> objNormals = new List<Vector3>();
            List<OBJVertex> objFaces = new List<OBJVertex>();

            foreach( string line in fileLines )
            {
                if( string.IsNullOrEmpty( line ) || line.StartsWith( "#" ) )
                    continue;

                // Tokenize by spaces (any amount of whitespace).
                string[] tokens = line.Split( (char[])null, StringSplitOptions.RemoveEmptyEntries );

                if( tokens.Length == 0 )
                    continue;

                string prefix = tokens[0].ToLowerInvariant();

                switch( prefix )
                {
                    case "o":
                        if( tokens.Length < 2 )
                            throw new IOException( $"OBJ Importer: Malformed object name: '{line}'" );

                        objName = tokens[1];
                        break;
                    case "v": // Vertex position. Expect at least 3 floats: x, y, z. 
                              // (Optionally a w coordinate that we ignore.)
                        if( tokens.Length < 4 )
                            throw new IOException( $"OBJ Importer: Malformed vertex line: '{line}'" );

                        float x = float.Parse( tokens[1], NumberFormat );
                        float y = float.Parse( tokens[2], NumberFormat );
                        float z = float.Parse( tokens[3], NumberFormat );
                        objVertices.Add( new Vector3( x, y, z ) );
                        break;

                    case "vt": // Texture coordinate. Expect at least 2 floats: u, v.
                               // OBJ and Unity UV coordinate systems are the same.
                        if( tokens.Length < 3 )
                            throw new IOException( $"OBJ Importer: Malformed uv line: '{line}'" );

                        float u = float.Parse( tokens[1], NumberFormat );
                        float v = float.Parse( tokens[2], NumberFormat );
                        objUVs.Add( new Vector2( u, v ) );
                        break;

                    case "vn": // Vertex normal. Expect 3 floats.
                        if( tokens.Length < 4 )
                            throw new IOException( $"OBJ Importer: Malformed normal line: '{line}'" );

                        float nx = float.Parse( tokens[1], NumberFormat );
                        float ny = float.Parse( tokens[2], NumberFormat );
                        float nz = float.Parse( tokens[3], NumberFormat );
                        objNormals.Add( new Vector3( nx, ny, nz ) );
                        break;

                    case "f": // Faces
                        if( tokens.Length < 4 )
                            throw new IOException( $"OBJ Importer: Face with fewer than 3 vertices: '{line}'" );

                        if( tokens.Length > 4 )
                            throw new IOException( $"OBJ Importer: Only fully triangulated meshes are supported: '{line}'" );

                        for( int i = 1; i < tokens.Length; i++ )
                        {
                            // tokens[i] is something like "17/5/3"  or "17/5"  or "17//3"  or "17"
                            //            corresponds to   "v/vt/vn" or  "v/vt" or  "v//vn" or "v"

                            string[] splitFaceIndex = tokens[i].Split( '/' );
                            switch( splitFaceIndex.Length )
                            {
                                case 1: // vertices

                                    OBJVertex face = new OBJVertex() { v = int.Parse( splitFaceIndex[0] ), vt = -1, vn = -1 };
                                    objFaces.Add( face );
                                    break;
                                case 2: // vertices, uvs

                                    face = new OBJVertex() { v = int.Parse( splitFaceIndex[0] ), vt = int.Parse( splitFaceIndex[1] ), vn = -1 };
                                    objFaces.Add( face );
                                    break;
                                case 3: // vertices, uvs, normals
                                        // OR vertices, ___, normals
                                    if( splitFaceIndex[1] == "" )
                                        face = new OBJVertex() { v = int.Parse( splitFaceIndex[0] ), vt = -1, vn = int.Parse( splitFaceIndex[2] ) };
                                    else
                                        face = new OBJVertex() { v = int.Parse( splitFaceIndex[0] ), vt = int.Parse( splitFaceIndex[1] ), vn = int.Parse( splitFaceIndex[2] ) };
                                    objFaces.Add( face );
                                    break;
                            }
                        }
                        break;

                    default:
                        // Unsupported data gets caught here.
                        break;
                }
            }

            // Convert from the OBJ's mixed-index faces, to Unity's same-index triangles.
            int distinctTripletCount = 0;
            Dictionary<OBJVertex, int> tripletToIndex = new();

            foreach( var triplet in objFaces )
            {
                if( !tripletToIndex.TryGetValue( triplet, out _ ) )
                {
                    tripletToIndex[triplet] = distinctTripletCount;
                    distinctTripletCount++;
                }
            }

            NativeArray<Vector3> finalVertices = new NativeArray<Vector3>( distinctTripletCount, Allocator.Temp );
            NativeArray<Vector2> finalUVs = new NativeArray<Vector2>( distinctTripletCount, Allocator.Temp );
            NativeArray<Vector3> finalNormals = new NativeArray<Vector3>( distinctTripletCount, Allocator.Temp );
            int[] finalTriangles = new int[objFaces.Count * 3];

            int vi = 0;
            foreach( var triplet in objFaces )
            {
                // Vertex
                int vertIndex = tripletToIndex[triplet];
                if( triplet.v == -1 )
                    finalVertices[vertIndex] = Vector3.zero;
                else if( triplet.v < 1 || triplet.v > objVertices.Count )
                    throw new IndexOutOfRangeException( $"Vertex index out of range: v={triplet.v} (valid: 1..{objVertices.Count})" );
                else
                    finalVertices[vertIndex] = objVertices[triplet.v - 1];

                // UV
                if( triplet.vt == -1 )
                    finalUVs[vertIndex] = Vector3.zero;
                else if( triplet.vt < 1 || triplet.vt > objUVs.Count )
                    throw new IndexOutOfRangeException( $"UV index out of range: vt={triplet.vt} (valid: 1..{objUVs.Count})" );
                else
                    finalUVs[vertIndex] = objUVs[triplet.vt - 1];

                // Normal
                if( triplet.vn == -1 )
                    finalNormals[vertIndex] = Vector3.zero;
                else if( triplet.vn < 1 || triplet.vn > objNormals.Count )
                    throw new IndexOutOfRangeException( $"UV index out of range: vn={triplet.vn} (valid: 1..{objNormals.Count})" );
                else
                    finalNormals[vertIndex] = objNormals[triplet.vn - 1];

                // Triangle index
                finalTriangles[vi] = vertIndex;
                vi++;
            }

            Mesh mesh = new Mesh();
            mesh.name = objName;

            // TODO - This can probably be optimized by setting the vertex buffer structs directly, instead of copying the data from out managed lists.
            mesh.SetVertices( finalVertices );
            mesh.SetUVs( 0, finalUVs );
            if( objNormals.Count > 0 )
            {
                mesh.SetNormals( finalNormals );
            }

            mesh.SetTriangles( finalTriangles, 0 ); // All triangles into submesh 0

            if( objNormals.Count == 0 )
            {
                mesh.RecalculateNormals();
            }

            mesh.RecalculateTangents();
            mesh.RecalculateBounds();

            return mesh;
        }
    }
}
