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
            using StreamReader reader = new StreamReader( filePath );
            return LoadOBJ( reader, Path.GetFileNameWithoutExtension( filePath ) );
        }

        public static Mesh LoadOBJ( TextReader reader, string meshName )
        {
            List<Vector3> objVertices = new();
            List<Vector2> objUVs = new();
            List<Vector3> objNormals = new();
            List<OBJVertex> objFaces = new();

            string line;
            while( (line = reader.ReadLine()) != null )
            {
                if( string.IsNullOrWhiteSpace( line ) || line.StartsWith( "#" ) )
                    continue;

                string[] tokens = line.Split( (char[])null, StringSplitOptions.RemoveEmptyEntries );
                if( tokens.Length == 0 )
                    continue;

                string prefix = tokens[0].ToLowerInvariant();

                switch( prefix )
                {
                    case "o":
                        if( tokens.Length >= 2 ) objName = tokens[1];
                        break;
                    case "v":
                        if( tokens.Length >= 4 )
                            objVertices.Add( new Vector3(
                                float.Parse( tokens[1], NumberFormat ),
                                float.Parse( tokens[2], NumberFormat ),
                                float.Parse( tokens[3], NumberFormat ) ) );
                        break;
                    case "vt":
                        if( tokens.Length >= 3 )
                            objUVs.Add( new Vector2(
                                float.Parse( tokens[1], NumberFormat ),
                                float.Parse( tokens[2], NumberFormat ) ) );
                        break;
                    case "vn":
                        if( tokens.Length >= 4 )
                            objNormals.Add( new Vector3(
                                float.Parse( tokens[1], NumberFormat ),
                                float.Parse( tokens[2], NumberFormat ),
                                float.Parse( tokens[3], NumberFormat ) ) );
                        break;
                    case "f":
                        if( tokens.Length < 4 )
                            throw new IOException( $"Face with <3 vertices: '{line}'" );
                        if( tokens.Length > 4 )
                            throw new IOException( $"Only triangulated meshes supported: '{line}'" );

                        for( int i = 1; i < tokens.Length; i++ )
                        {
                            string[] split = tokens[i].Split( '/' );
                            OBJVertex face = new OBJVertex() { v = int.Parse( split[0] ), vt = -1, vn = -1 };

                            if( split.Length > 1 && !string.IsNullOrEmpty( split[1] ) )
                                face.vt = int.Parse( split[1] );

                            if( split.Length > 2 && !string.IsNullOrEmpty( split[2] ) )
                                face.vn = int.Parse( split[2] );

                            objFaces.Add( face );
                        }
                        break;
                }
            }

            return BuildMesh( meshName, objVertices, objUVs, objNormals, objFaces );
        }

        private static string objName; // Temp state is okay if called sequentially, but better passed locally. Refactored above to pass meshName but reader logic uses objName variable.

        private static Mesh BuildMesh( string name, List<Vector3> v, List<Vector2> uv, List<Vector3> n, List<OBJVertex> f )
        {
            int distinctTripletCount = 0;
            Dictionary<OBJVertex, int> tripletToIndex = new();

            foreach( var triplet in f )
            {
                if( !tripletToIndex.ContainsKey( triplet ) )
                {
                    tripletToIndex[triplet] = distinctTripletCount++;
                }
            }

            NativeArray<Vector3> finalVertices = new NativeArray<Vector3>( distinctTripletCount, Allocator.Temp );
            NativeArray<Vector2> finalUVs = new NativeArray<Vector2>( distinctTripletCount, Allocator.Temp );
            NativeArray<Vector3> finalNormals = new NativeArray<Vector3>( distinctTripletCount, Allocator.Temp );
            int[] finalTriangles = new int[f.Count]; // Triangles list has same count as faces list since faces are triangles.

            int vi = 0;
            foreach( var triplet in f )
            {
                int vertIndex = tripletToIndex[triplet];

                // Vertex (1-based)
                if( triplet.v >= 1 && triplet.v <= v.Count )
                    finalVertices[vertIndex] = v[triplet.v - 1];

                // UV
                if( triplet.vt >= 1 && triplet.vt <= uv.Count )
                    finalUVs[vertIndex] = uv[triplet.vt - 1];

                // Normal
                if( triplet.vn >= 1 && triplet.vn <= n.Count )
                    finalNormals[vertIndex] = n[triplet.vn - 1];

                finalTriangles[vi++] = vertIndex;
            }

            Mesh mesh = new Mesh();
            mesh.name = string.IsNullOrEmpty( objName ) ? name : objName;
            mesh.SetVertices( finalVertices );
            mesh.SetUVs( 0, finalUVs );
            if( n.Count > 0 )
                mesh.SetNormals( finalNormals );
            mesh.SetTriangles( finalTriangles, 0 );

            if( n.Count == 0 )
                mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();

            return mesh;
        }
    }
}