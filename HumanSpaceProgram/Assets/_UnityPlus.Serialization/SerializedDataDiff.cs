using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Configuration options for the <see cref="SerializedDataDiffer"/>.
    /// </summary>
    public sealed class SerializedDataDiffConfig
    {
        /// <summary>
        /// A set of paths that should be completely ignored by the differ.
        /// </summary>
        public HashSet<SerializedDataPath> IgnorePaths { get; set; } = new();

        /// <summary>
        /// A set of keys that should be ignored when comparing <see cref="SerializedObject"/>s (e.g. "$type", "$id").
        /// </summary>
        public HashSet<string> IgnoreKeys { get; set; } = new();

        /// <summary>
        /// A custom predicate to decide if a difference should be reported at a given path.
        /// Return true to continue normal diffing, false to ignore this path and its children.
        /// </summary>
        public Func<SerializedDataPath, SerializedData, SerializedData, bool> ShouldDiff { get; set; }

        /// <summary>
        /// A custom comparer that can override the default comparison logic for a given path.
        /// Return true if equal, false if different, or null to use default logic.
        /// </summary>
        public Func<SerializedDataPath, SerializedData, SerializedData, bool?> CustomComparer { get; set; }
    }

    /// <summary>
    /// A utility for finding differences between two <see cref="SerializedData"/> trees.
    /// </summary>
    public static class SerializedDataDiff
    {
        /// <summary>
        /// Compares two <see cref="SerializedData"/> trees and returns a list of differences.
        /// </summary>
        public static List<SerializedDataDifference> Diff( SerializedData a, SerializedData b, SerializedDataDiffConfig options = null )
        {
            var differences = new List<SerializedDataDifference>();
            options ??= new SerializedDataDiffConfig();

            var visited = new HashSet<(SerializedData, SerializedData)>();
            DiffRecursive( a, b, new SerializedDataPath(), options, differences, visited );

            return differences;
        }

        private static void DiffRecursive( SerializedData a, SerializedData b, SerializedDataPath path, SerializedDataDiffConfig options, List<SerializedDataDifference> differences, HashSet<(SerializedData, SerializedData)> visited )
        {
            if( a != null && b != null )
            {
                if( visited.Contains( (a, b) ) )
                    return;
                visited.Add( (a, b) );
            }

            if( options.IgnorePaths.Contains( path ) )
                return;

            if( options.ShouldDiff != null && !options.ShouldDiff( path, a, b ) )
                return;

            if( options.CustomComparer != null )
            {
                bool? customResult = options.CustomComparer( path, a, b );
                if( customResult.HasValue )
                {
                    if( !customResult.Value )
                    {
                        differences.Add( new SerializedDataDifference( path, a, b, SerializedDataDifferenceKind.ValueMismatch ) );
                    }
                    return;
                }
            }

            // Fast path: if they are equal, no need to check children
            if( ReferenceEquals( a, b ) )
                return;

            if( a != null && b != null && a.Equals( b ) )
                return;

            if( a == null && b == null )
                return;

            if( a == null )
            {
                differences.Add( new SerializedDataDifference( path, a, b, SerializedDataDifferenceKind.MissingInA ) );
                return;
            }

            if( b == null )
            {
                differences.Add( new SerializedDataDifference( path, a, b, SerializedDataDifferenceKind.MissingInB ) );
                return;
            }

            if( a.GetType() != b.GetType() )
            {
                differences.Add( new SerializedDataDifference( path, a, b, SerializedDataDifferenceKind.TypeMismatch ) );
                return;
            }

            if( a is SerializedPrimitive primitiveA && b is SerializedPrimitive primitiveB )
            {
                if( !primitiveA.Equals( primitiveB ) )
                {
                    differences.Add( new SerializedDataDifference( path, a, b, SerializedDataDifferenceKind.ValueMismatch ) );
                }
                return;
            }

            if( a is SerializedObject objA && b is SerializedObject objB )
            {
                var allKeys = objA.Keys.Concat( objB.Keys ).Distinct();
                foreach( var key in allKeys )
                {
                    if( options.IgnoreKeys.Contains( key ) )
                        continue;

                    var childPath = path.Append( SerializedDataPathSegment.Named( key ) );

                    bool inA = objA.TryGetValue( key, out var valA );
                    bool inB = objB.TryGetValue( key, out var valB );

                    if( inA && inB )
                    {
                        DiffRecursive( valA, valB, childPath, options, differences, visited );
                    }
                    else if( inA )
                    {
                        differences.Add( new SerializedDataDifference( childPath, valA, null, SerializedDataDifferenceKind.MissingInB ) );
                    }
                    else // inB
                    {
                        differences.Add( new SerializedDataDifference( childPath, null, valB, SerializedDataDifferenceKind.MissingInA ) );
                    }
                }
                return;
            }

            if( a is SerializedArray arrA && b is SerializedArray arrB )
            {
                int maxCount = Math.Max( arrA.Count, arrB.Count );
                for( int i = 0; i < maxCount; i++ )
                {
                    var childPath = path.Append( SerializedDataPathSegment.Indexed( i ) );

                    if( i < arrA.Count && i < arrB.Count )
                    {
                        DiffRecursive( arrA[i], arrB[i], childPath, options, differences, visited );
                    }
                    else if( i < arrA.Count )
                    {
                        differences.Add( new SerializedDataDifference( childPath, arrA[i], null, SerializedDataDifferenceKind.MissingInB ) );
                    }
                    else // i < arrB.Count
                    {
                        differences.Add( new SerializedDataDifference( childPath, null, arrB[i], SerializedDataDifferenceKind.MissingInA ) );
                    }
                }
            }
        }
    }
}