//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using UnityEngine;
//using UnityEngine.LowLevel;

//namespace UnityPlus
//{
//    public static class PlayerLoopUtils
//    {
//        private readonly struct PlayerLoopPath : IEquatable<PlayerLoopPath>
//        {
//            private readonly Type[] typeChain;

//            public readonly Type targetTypeInPath;

//            public int ParentChainLength => typeChain.Length;

//            public Type this[int index] => typeChain[index];

//            public PlayerLoopPath( Type targetTypeInPath, params Type[] types )
//            {
//                this.targetTypeInPath = targetTypeInPath;
//                typeChain = types;
//            }

//            public PlayerLoopPath Copy( Type newTargetType )
//            {
//                return new PlayerLoopPath( newTargetType, typeChain );
//            }

//            public PlayerLoopPath ParentPath()
//            {
//                if( typeChain.Length == 0 )
//                    return new PlayerLoopPath( null, typeChain );

//                var newChain = new Type[typeChain.Length - 1];
//                Array.Copy( typeChain, newChain, newChain.Length );
//                return new PlayerLoopPath( typeChain[^1], newChain );
//            }

//            public bool Equals( PlayerLoopPath other )
//            {
//                if( typeChain.Length != other.typeChain.Length )
//                    return false;
//                for( int i = 0; i < typeChain.Length; i++ )
//                {
//                    if( typeChain[i] != other.typeChain[i] )
//                        return false;
//                }
//                return targetTypeInPath == other.targetTypeInPath;
//            }

//            public override int GetHashCode()
//            {
//                int hash = 17;
//                foreach( var type in typeChain )
//                {
//                    hash = hash * 31 + (type?.GetHashCode() ?? 0);
//                }
//                hash = hash * 7 + (targetTypeInPath?.GetHashCode() ?? 0);
//                return hash;
//            }

//            public override bool Equals( object obj )
//            {
//                return obj is PlayerLoopPath other && Equals( other );
//            }

//            public override string ToString()
//            {
//                return string.Join( " -> ", typeChain.Select( t => t != null ? t.Name : "null" ) );
//            }
//        }

//        public static void ResetToDefault()
//        {
//            PlayerLoop.SetPlayerLoop( PlayerLoop.GetDefaultPlayerLoop() );
//            _previous.Clear();
//            _next.Clear();
//            _orphaned.Clear();
//        }

//        // adds a system inside. 'add' is without constraints.

//        public static void AddSystem( ref PlayerLoopSystem systemToAdd )
//        {
//            PlayerLoopSystem loopRoot = PlayerLoop.GetCurrentPlayerLoop();
//            AddSystemNested( new PlayerLoopPath( systemToAdd.type ), ref loopRoot, ref systemToAdd );
//            PlayerLoop.SetPlayerLoop( loopRoot );
//        }

//        public static void AddSystem<T1>( ref PlayerLoopSystem systemToAdd )
//        {
//            PlayerLoopSystem loopRoot = PlayerLoop.GetCurrentPlayerLoop();
//            AddSystemNested( new PlayerLoopPath( systemToAdd.type, typeof( T1 ) ), ref loopRoot, ref systemToAdd );
//            PlayerLoop.SetPlayerLoop( loopRoot );
//        }

//        public static void AddSystem<T1, T2>( ref PlayerLoopSystem systemToAdd )
//        {
//            PlayerLoopSystem loopRoot = PlayerLoop.GetCurrentPlayerLoop();
//            AddSystemNested( new PlayerLoopPath( systemToAdd.type, typeof( T1 ), typeof( T2 ) ), ref loopRoot, ref systemToAdd );
//            PlayerLoop.SetPlayerLoop( loopRoot );
//        }

//        public static void AddSystem<T1, T2, T3>( ref PlayerLoopSystem systemToAdd )
//        {
//            PlayerLoopSystem loopRoot = PlayerLoop.GetCurrentPlayerLoop();
//            AddSystemNested( new PlayerLoopPath( systemToAdd.type, typeof( T1 ), typeof( T2 ), typeof( T3 ) ), ref loopRoot, ref systemToAdd );
//            PlayerLoop.SetPlayerLoop( loopRoot );
//        }

//        private static ref PlayerLoopSystem FindSubtree( PlayerLoopPath pathTypes, ref PlayerLoopSystem root, out bool found )
//        {
//            ref PlayerLoopSystem current = ref root;
//            found = true;

//            for( int i = 0; i < pathTypes.ParentChainLength; i++ )
//            {
//                var list = current.subSystemList;
//                if( list == null )
//                {
//                    found = false;
//                    return ref root; // return a valid ref (root) when not found
//                }

//                int idx = -1;
//                for( int j = 0; j < list.Length; j++ )
//                {
//                    if( list[j].type == pathTypes[i] )
//                    {
//                        idx = j;
//                        break;
//                    }
//                }

//                if( idx == -1 )
//                {
//                    found = false;
//                    return ref root;
//                }

//                // IMPORTANT: use ref to the array element so 'current' becomes an alias to that element
//                current = ref current.subSystemList[idx];
//            }

//            return ref current; // ref to the final nested PlayerLoopSystem
//        }

//        private static void AddSystemNested( PlayerLoopPath pathTypes, ref PlayerLoopSystem root, ref PlayerLoopSystem systemToInsert )
//        {
//            ref PlayerLoopSystem systemParent = ref FindSubtree( pathTypes, ref root, out bool found );

//            RemoveConstraintsIfPresent( pathTypes ); // remove if were present, because we're adding a new one now.

//            // case where the system can't be added and needs to be set as orphaned (parent not added yet).
//            if( !found && pathTypes.ParentChainLength > 0 )
//            {
//                var parentPathTypes = pathTypes.ParentPath();
//                if( !_orphaned.TryGetValue( parentPathTypes, out var orphanedSubsystemList ) )
//                {
//                    _orphaned.Add( parentPathTypes, new PlayerLoopSystem[] { systemToInsert } );
//                }
//                else
//                {
//                    _orphaned[parentPathTypes] = orphanedSubsystemList.Append( systemToInsert ).ToArray();
//                }
//                return;
//            }

//            // If the system was removed, and had subsystems, re-add those 'orphaned' subsystems.
//            if( _orphaned.TryGetValue( pathTypes, out var orphanedSubsystemList2 ) )
//            {
//                systemToInsert.subSystemList = orphanedSubsystemList2;
//                _orphaned.Remove( pathTypes );
//            }

//            List<PlayerLoopSystem> playerLoopSystemList = systemParent.subSystemList?.ToList() ?? new List<PlayerLoopSystem>();

//            for( int i = 0; i < playerLoopSystemList.Count; i++ )
//            {
//                if( playerLoopSystemList[i].type == systemToInsert.type )
//                {
//                    playerLoopSystemList[i] = systemToInsert; // Replace existing
//                    systemParent.subSystemList = playerLoopSystemList.ToArray();
//                    return;
//                }
//            }

//            playerLoopSystemList.Add( systemToInsert );
//            systemParent.subSystemList = playerLoopSystemList.ToArray();

//            TryReAddSiblings( pathTypes, ref root );
//        }

//        // inserts a system at a specific position (before/after/between other systems).

//        public static void InsertSystemAfter( ref PlayerLoopSystem systemToInsert, Type previous )
//        {
//            PlayerLoopSystem loopRoot = PlayerLoop.GetCurrentPlayerLoop();
//            UpsertSystemNested( new PlayerLoopPath( systemToInsert.type ), ref loopRoot, ref systemToInsert, new List<Type>() { previous }, new List<Type>() { } );
//            PlayerLoop.SetPlayerLoop( loopRoot );
//        }

//        public static void InsertSystemBefore( ref PlayerLoopSystem systemToInsert, Type next )
//        {
//            PlayerLoopSystem loopRoot = PlayerLoop.GetCurrentPlayerLoop();
//            UpsertSystemNested( new PlayerLoopPath( systemToInsert.type ), ref loopRoot, ref systemToInsert, new List<Type>(), new List<Type>() { next } );
//            PlayerLoop.SetPlayerLoop( loopRoot );
//        }

//        public static void InsertSystemAfter<T1>( ref PlayerLoopSystem systemToInsert, Type previous )
//        {
//            PlayerLoopSystem loopRoot = PlayerLoop.GetCurrentPlayerLoop();
//            UpsertSystemNested( new PlayerLoopPath( systemToInsert.type, typeof( T1 ) ), ref loopRoot, ref systemToInsert, new List<Type>() { previous }, new List<Type>() { } );
//            PlayerLoop.SetPlayerLoop( loopRoot );
//        }

//        public static void InsertSystemBefore<T1>( ref PlayerLoopSystem systemToInsert, Type next )
//        {
//            PlayerLoopSystem loopRoot = PlayerLoop.GetCurrentPlayerLoop();
//            UpsertSystemNested( new PlayerLoopPath( systemToInsert.type, typeof( T1 ) ), ref loopRoot, ref systemToInsert, new List<Type>(), new List<Type>() { next } );
//            PlayerLoop.SetPlayerLoop( loopRoot );
//        }

//        public static void InsertSystemBetween<T1>( ref PlayerLoopSystem systemToInsert, Type previous, Type next )
//        {
//            PlayerLoopSystem loopRoot = PlayerLoop.GetCurrentPlayerLoop();
//            UpsertSystemNested( new PlayerLoopPath( systemToInsert.type, typeof( T1 ) ), ref loopRoot, ref systemToInsert, new List<Type>() { previous }, new List<Type>() { next } );
//            PlayerLoop.SetPlayerLoop( loopRoot );
//        }

//        public static void InsertSystemAfter<T1, T2>( ref PlayerLoopSystem systemToInsert, Type previous )
//        {
//            PlayerLoopSystem loopRoot = PlayerLoop.GetCurrentPlayerLoop();
//            UpsertSystemNested( new PlayerLoopPath( systemToInsert.type, typeof( T1 ), typeof( T2 ) ), ref loopRoot, ref systemToInsert, new List<Type>() { previous }, new List<Type>() { } );
//            PlayerLoop.SetPlayerLoop( loopRoot );
//        }

//        public static void InsertSystemBefore<T1, T2>( ref PlayerLoopSystem systemToInsert, Type next )
//        {
//            PlayerLoopSystem loopRoot = PlayerLoop.GetCurrentPlayerLoop();
//            UpsertSystemNested( new PlayerLoopPath( systemToInsert.type, typeof( T1 ), typeof( T2 ) ), ref loopRoot, ref systemToInsert, new List<Type>(), new List<Type>() { next } );
//            PlayerLoop.SetPlayerLoop( loopRoot );
//        }

//        public static void InsertSystemBetween<T1, T2>( ref PlayerLoopSystem systemToInsert, Type previous, Type next )
//        {
//            PlayerLoopSystem loopRoot = PlayerLoop.GetCurrentPlayerLoop();
//            UpsertSystemNested( new PlayerLoopPath( systemToInsert.type, typeof( T1 ), typeof( T2 ) ), ref loopRoot, ref systemToInsert, new List<Type>() { previous }, new List<Type>() { next } );
//            PlayerLoop.SetPlayerLoop( loopRoot );
//        }

//        // for each sequence of types (path), store a list of constraints. modify the previous/next lists using those.
//        static Dictionary<PlayerLoopPath, List<Type>> _previous = new();
//        static Dictionary<PlayerLoopPath, List<Type>> _next = new();
//        static Dictionary<PlayerLoopPath, PlayerLoopSystem[]> _orphaned = new();

//        private static void AddConstraintsIfNotPresent( PlayerLoopPath pathTypes, List<Type> previous, List<Type> next )
//        {
//            if( previous != null && previous.Count > 0 )
//            {
//                _previous[pathTypes] = previous.ToList();
//                foreach( var leafType in previous )
//                {
//                    var path = pathTypes.Copy( leafType );
//                    if( !_next.ContainsKey( path ) )
//                        _next[path] = new List<Type>();
//                    if( !_next[path].Contains( pathTypes.targetTypeInPath ) )
//                        _next[path].Add( pathTypes.targetTypeInPath );
//                }
//            }
//            if( next != null && next.Count > 0 )
//            {
//                _next[pathTypes] = next.ToList();
//                foreach( var leafType in next )
//                {
//                    var path = pathTypes.Copy( leafType );
//                    if( !_previous.ContainsKey( path ) )
//                        _previous[path] = new List<Type>();
//                    if( !_previous[path].Contains( pathTypes.targetTypeInPath ) )
//                        _previous[path].Add( pathTypes.targetTypeInPath );
//                }
//            }
//        }
//        private static void RemoveConstraintsIfPresent( PlayerLoopPath pathTypes )
//        {
//            if( _previous.TryGetValue( pathTypes, out var previous ) )
//            {
//                _previous.Remove( pathTypes );
//                if( previous != null )
//                {
//                    foreach( var leafType in previous )
//                    {
//                        var path = pathTypes.Copy( leafType );
//                        if( _next.TryGetValue( path, out var nextList ) )
//                        {
//                            nextList.Remove( pathTypes.targetTypeInPath );
//                            if( nextList.Count == 0 )
//                                _next.Remove( path );
//                        }
//                    }
//                }
//            }
//            if( _next.TryGetValue( pathTypes, out var next ) )
//            {
//                _next.Remove( pathTypes );
//                if( next != null )
//                {
//                    foreach( var leafType in next )
//                    {
//                        var path = pathTypes.Copy( leafType );
//                        if( _previous.TryGetValue( path, out var prevList ) )
//                        {
//                            prevList.Remove( pathTypes.targetTypeInPath );
//                            if( prevList.Count == 0 )
//                                _previous.Remove( path );
//                        }
//                    }
//                }
//            }
//        }

//        private static void UpsertSystemNested( PlayerLoopPath pathTypes, ref PlayerLoopSystem root, ref PlayerLoopSystem systemToInsert, List<Type> previous, List<Type> next )
//        {
//            ref PlayerLoopSystem systemParent = ref FindSubtree( pathTypes, ref root, out bool found );

//            // case where the system can't be added and needs to be set as orphaned (parent not added yet).
//            if( !found && pathTypes.ParentChainLength > 0 )
//            {
//                var parentPathTypes = pathTypes.ParentPath();
//                if( !_orphaned.TryGetValue( parentPathTypes, out var orphanedSubsystemList ) )
//                {
//                    AddConstraintsIfNotPresent( pathTypes, previous, next );
//                    _orphaned.Add( parentPathTypes, new PlayerLoopSystem[] { systemToInsert } );
//                }
//                else
//                {
//                    AddConstraintsIfNotPresent( pathTypes, previous, next );
//                    _orphaned[parentPathTypes] = orphanedSubsystemList.Append( systemToInsert ).ToArray();
//                }
//                return;
//            }

//            if( _previous.TryGetValue( pathTypes, out var prevFromOtherPaths ) )
//                previous.AddRange( prevFromOtherPaths );
//            if( _next.TryGetValue( pathTypes, out var nextFromOtherPaths ) )
//                next.AddRange( nextFromOtherPaths );

//            AddConstraintsIfNotPresent( pathTypes, previous, next );

//            // If this system was previously removed, and had subsystems, re-add those 'orphaned' subsystems.
//            if( _orphaned.TryGetValue( pathTypes, out var orphanedSubsystemList2 ) )
//            {
//                systemToInsert.subSystemList = orphanedSubsystemList2;
//                _orphaned.Remove( pathTypes );
//            }

//            // instead of the index finding and insert, copy, append anywhere and sort, then the sorted array is the final one.
//#warning TODO - maybe add using topological sorting with the new overload?
//            List<PlayerLoopSystem> playerLoopSystemList = systemParent.subSystemList?.ToList() ?? new List<PlayerLoopSystem>();

//            for( int j = 0; j < playerLoopSystemList.Count; j++ )
//            {
//                if( playerLoopSystemList[j].type == systemToInsert.type )
//                {
//                    playerLoopSystemList[j] = systemToInsert; // Replace existing
//                    systemParent.subSystemList = playerLoopSystemList.ToArray();
//                    return;
//                }
//            }

//            /*int index = playerLoopSystemList.Count; // default to end
//            if( _previous.Count > 0 || _next.Count > 0 )
//            {
//                // find index of first 'next' and last 'previous' (of ones that exist).
//                // If all 'previous' come before any 'next', the insert is valid.
//                int lastPreviousIndex = -1;
//                int firstNextIndex = -1;
//                int previousCount = 0;
//                int nextCount = 0;
//                for( int j = 0; j < playerLoopSystemList.Count; j++ )
//                {
//                    if( previous.Contains( playerLoopSystemList[j].type ) )
//                    {
//                        previousCount++;
//                        if( j > lastPreviousIndex )
//                            lastPreviousIndex = j;
//                    }
//                    if( next.Contains( playerLoopSystemList[j].type ) )
//                    {
//                        nextCount++;
//                        if( firstNextIndex == -1 )
//                            firstNextIndex = j;
//                    }
//                }
//                if( (lastPreviousIndex != -1 && firstNextIndex != -1 && lastPreviousIndex >= firstNextIndex)
//                    || previousCount != previous.Count || nextCount != next.Count )
//                {
//                    // add self as orphan if can't add.
//                    var parentPathTypes = pathTypes.ParentPath();
//                    if( !_orphaned.TryGetValue( parentPathTypes, out var orphanedSubsystemList ) )
//                    {
//                        _orphaned.Add( parentPathTypes, new PlayerLoopSystem[] { systemToInsert } );
//                    }
//                    else
//                    {
//                        _orphaned[parentPathTypes] = orphanedSubsystemList.Append( systemToInsert ).ToArray();
//                    }
//                    return;
//                }
//                index = lastPreviousIndex + 1;
//            }

//            playerLoopSystemList.Insert( index, systemToInsert );
//            systemParent.subSystemList = playerLoopSystemList.ToArray();

//            TryReAddSiblings( pathTypes, ref root );*/

//            playerLoopSystemList.Add( systemToInsert );
//            var parentPath = pathTypes.ParentPath();
//            if( _orphaned.TryGetValue( parentPath, out var orphanedSiblings ) )
//            {
//                playerLoopSystemList.AddRange( orphanedSiblings );
//            }

//            playerLoopSystemList = ITopologicallySortable_Ex.SortDependencies<PlayerLoopSystem, Type>( playerLoopSystemList, n => n.GetType(), 
//                playerLoopSystem =>
//            {

//            },
//                playerLoopSystem =>
//            {

//            } );
//        }

//        private static void TryReAddSiblings( PlayerLoopPath pathTypes, ref PlayerLoopSystem root )
//        {
//            var parentPath = pathTypes.ParentPath();
//            if( _orphaned.TryGetValue( parentPath, out var orphanedSiblings ) )
//            {
//                // Remove the bucket first to avoid duplicate-processing while we attempt to re-insert.
//                _orphaned.Remove( parentPath );

//                foreach( var orphanSys in orphanedSiblings )
//                {
//                    var childPath = pathTypes.Copy( orphanSys.type );

//                    // Make a local copy (value type) to pass by ref.
//                    var orphanCopy = orphanSys;

//                    // Try to insert the orphaned sibling. If it still can't be placed,
//                    // UpsertSystemNested will put it back into _orphaned.
//                    UpsertSystemNested( childPath, ref root, ref orphanCopy, new List<Type>(), new List<Type>() );
//                }
//            }
//        }

//        // removes a system inside.

//        public static void RemoveSystem( ref PlayerLoopSystem systemToRemove )
//        {
//            PlayerLoopSystem loopRoot = PlayerLoop.GetCurrentPlayerLoop();
//            RemoveSystemNested( new PlayerLoopPath( systemToRemove.type ), ref loopRoot, ref systemToRemove );
//            PlayerLoop.SetPlayerLoop( loopRoot );
//        }

//        public static void RemoveSystem<T1>( ref PlayerLoopSystem systemToRemove )
//        {
//            PlayerLoopSystem loopRoot = PlayerLoop.GetCurrentPlayerLoop();
//            RemoveSystemNested( new PlayerLoopPath( systemToRemove.type, typeof( T1 ) ), ref loopRoot, ref systemToRemove );
//            PlayerLoop.SetPlayerLoop( loopRoot );
//        }

//        public static void RemoveSystem<T1, T2>( ref PlayerLoopSystem systemToRemove )
//        {
//            PlayerLoopSystem loopRoot = PlayerLoop.GetCurrentPlayerLoop();
//            RemoveSystemNested( new PlayerLoopPath( systemToRemove.type, typeof( T1 ), typeof( T2 ) ), ref loopRoot, ref systemToRemove );
//            PlayerLoop.SetPlayerLoop( loopRoot );
//        }

//        private static void RemoveSystemNested( PlayerLoopPath pathTypes, ref PlayerLoopSystem root, ref PlayerLoopSystem systemToRemove )
//        {
//            ref PlayerLoopSystem systemParent = ref FindSubtree( pathTypes, ref root, out bool found );

//            var typeToRemove = systemToRemove.type;
//            if( !found )
//            {
//                var parentPathTypes = pathTypes.ParentPath();
//                if( _orphaned.TryGetValue( parentPathTypes, out var orphanedSubsystemList ) )
//                {
//                    var arr = orphanedSubsystemList.Where( s => s.type != typeToRemove ).ToArray();
//                    if( arr.Length == 0 )
//                        _orphaned.Remove( parentPathTypes );
//                    else
//                        _orphaned[parentPathTypes] = arr;
//                }

//                if( _orphaned.Remove( pathTypes ) )
//                {
//                    RemoveConstraintsIfPresent( pathTypes );
//                }
//                return;
//            }

//            if( systemParent.subSystemList == null )
//            {
//                return;
//            }

//            var delegateToRemove = systemToRemove.updateDelegate;

//            List<PlayerLoopSystem> playerLoopSystemList = systemParent.subSystemList?.ToList() ?? new List<PlayerLoopSystem>();
//            int index = -1;
//            for( int i = 0; i < playerLoopSystemList.Count; i++ )
//            {
//                if( playerLoopSystemList[i].type == typeToRemove )
//                {
//                    index = i;
//                    break;
//                }
//            }

//            if( index == -1 )
//                throw new ArgumentException( $"The specified subsystem was not found." );

//            // Store subsystems that are children of the removed system. In case the system is added again, we can re-add those subsystems.
//            var subSystems = playerLoopSystemList[index].subSystemList;
//            if( subSystems != null && subSystems.Length > 0 )
//            {
//                _orphaned[pathTypes] = subSystems;
//            }
//            RemoveConstraintsIfPresent( pathTypes );

//            playerLoopSystemList.RemoveAt( index );
//            systemParent.subSystemList = playerLoopSystemList.ToArray();

//            TryReAddSiblings( pathTypes, ref root );
//        }

//        // https://github.com/adammyhre/Unity-Improved-Timers/blob/master/Runtime/PlayerLoopUtils.cs

//        public static void PrintCurrentPlayerLoop()
//        {
//            PrintPlayerLoop( PlayerLoop.GetCurrentPlayerLoop() );
//        }

//        public static void PrintPlayerLoop( PlayerLoopSystem loop )
//        {
//            StringBuilder sb = new StringBuilder();
//            sb.AppendLine( "Unity Player Loop" );
//            if( loop.subSystemList != null )
//            {
//                foreach( PlayerLoopSystem subSystem in loop.subSystemList )
//                {
//                    PrintSubsystem( subSystem, sb, 0 );
//                }
//            }
//            Debug.Log( sb.ToString() );
//        }

//        private static void PrintSubsystem( PlayerLoopSystem system, StringBuilder sb, int level )
//        {
//            sb.Append( ' ', level * 2 ).AppendLine( system.type.ToString() );
//            if( system.subSystemList == null || system.subSystemList.Length == 0 ) return;

//            foreach( PlayerLoopSystem subSystem in system.subSystemList )
//            {
//                PrintSubsystem( subSystem, sb, level + 1 );
//            }
//        }
//    }
//}