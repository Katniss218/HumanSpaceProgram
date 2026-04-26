using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.LowLevel;

namespace UnityPlus.PlayerLoop
{
    /// <summary>
    /// Converts a flat list of annotated types into a hierarchical PlayerLoopSystem structure, merging it with an existing native loop such that its order is unchanged.
    /// </summary>
    public class PlayerLoopCompiler
    {
        /// <summary>
        /// Any constructed system entry, to be converted into the player loop system hierarchy.
        /// </summary>
        private class CompiledNode
        {
            public Type ID { get; set; }
            public Type TargetBucket { get; set; }
            public Type[] Before { get; set; }
            public Type[] After { get; set; }
            public Type[] Blacklist { get; set; }
            public bool IsNativeAlias { get; set; }
            public Type NativeAliasTarget { get; set; }
            public bool IsSystem { get; set; }
            public PlayerLoopSystem.UpdateFunction Callback { get; set; }
        }

        /// <summary>
        /// A loop system wrapper used for topological sorting of native and custom systems.
        /// </summary>
        private class SortNode
        {
            public Type ID { get; set; }
            public List<Type> Before { get; set; }
            public List<Type> After { get; set; } = new();
            public PlayerLoopSystem System { get; set; }
        }

        private Dictionary<Type, Type> _nativeToAlias;

        private void EnsureAliasesLoaded( IEnumerable<CompiledNode> allNodes )
        {
            _nativeToAlias = new Dictionary<Type, Type>();

            foreach( var node in allNodes )
            {
                if( node.IsNativeAlias )
                {
                    _nativeToAlias[node.NativeAliasTarget] = node.ID;
                }
            }
        }

        private CompiledNode Parse( Type t )
        {
            var sysAttr = (PlayerLoopSystemAttribute)Attribute.GetCustomAttribute( t, typeof( PlayerLoopSystemAttribute ), inherit: false );
            var natAttr = (PlayerLoopNativeAttribute)Attribute.GetCustomAttribute( t, typeof( PlayerLoopNativeAttribute ), inherit: false );

            if( sysAttr == null && natAttr == null )
                return null;

            var node = new CompiledNode()
            {
                ID = t
            };

            if( typeof( IPlayerLoopSystem ).IsAssignableFrom( t ) )
            {
                if( t.IsAbstract || t.IsInterface )
                    return null;

                node.IsSystem = true;

                try
                {
                    var instance = (IPlayerLoopSystem)Activator.CreateInstance( t );
                    node.Callback = instance.Run;
                }
                catch( Exception ex )
                {
                    Debug.LogError( $"[PlayerLoopCompiler] Failed to instantiate IPlayerLoopSystem of type {t.Name}. Must have parameterless constructor. {ex.Message}" );
                }
            }

            if( sysAttr != null )
            {
                sysAttr.Validate( t );
                node.TargetBucket = sysAttr.TargetBucket;
                node.Before = sysAttr.Before ?? Array.Empty<Type>();
                node.After = sysAttr.After ?? Array.Empty<Type>();
                node.Blacklist = sysAttr.Blacklist ?? Array.Empty<Type>();
            }
            else if( natAttr != null )
            {
                natAttr.Validate( t );
                node.TargetBucket = natAttr.TargetBucket;
                node.Before = natAttr.Before ?? Array.Empty<Type>();
                node.After = natAttr.After ?? Array.Empty<Type>();
                node.Blacklist = natAttr.Blacklist ?? Array.Empty<Type>();
                node.IsNativeAlias = natAttr.Alias != null;
                node.NativeAliasTarget = natAttr.Alias;
            }

            return node;
        }

        /// <summary>
        /// The main method for converting a flat list of annotated types into a hierarchical PlayerLoopSystem structure.
        /// </summary>
        /// <param name="nativeRoot"></param>
        /// <param name="customNodes"></param>
        /// <param name="handling"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public PlayerLoopSystem Compile( PlayerLoopSystem nativeRoot, IEnumerable<Type> customNodes, BucketHandling handling = BucketHandling.Skip )
        {
            List<CompiledNode> registry = customNodes.Select( Parse ).Where( n => n != null ).ToList();
            Dictionary<Type, CompiledNode> registryDict = registry.ToDictionary( n => n.ID );

            EnsureAliasesLoaded( registry );

            Dictionary<Type, PlayerLoopSystem> nodeMap = new();
            TraverseAndMapNative( nativeRoot, nodeMap );

            HashSet<CompiledNode> activeNodesToInclude = new();

            foreach( var node in registry )
            {
                if( node.IsNativeAlias )
                    continue; // native aliases are just markers replacing unity types.

                if( TryIncludeBucket( node, handling, registryDict, nodeMap, activeNodesToInclude ) )
                {
                    activeNodesToInclude.Add( node );
                }
                else if( handling == BucketHandling.IncludeThrow )
                {
                    throw new InvalidOperationException( $"[PlayerLoopCompiler] Failed to resolve TargetBucket {node.TargetBucket.Name} for node {node.ID.Name}." );
                }
                else if( handling == BucketHandling.IncludeSkip )
                {
                    Debug.LogWarning( $"[PlayerLoopCompiler] Skipping type {node.ID.Name} due to unresolvable bucket dependencies." );
                }
            }

            var customGroups = new Dictionary<Type, List<CompiledNode>>();
            foreach( var n in activeNodesToInclude )
            {
                if( n.TargetBucket == null )
                {
                    if( handling == BucketHandling.IncludeThrow )
                        throw new InvalidOperationException( $"[PlayerLoopCompiler] Node '{n.ID.Name}' has no TargetBucket and cannot be placed in the loop." );

                    continue; // skip nodes with missing buckets.
                }

                if( !customGroups.TryGetValue( n.TargetBucket, out var list ) )
                {
                    list = new List<CompiledNode>();
                    customGroups[n.TargetBucket] = list;
                }
                list.Add( n );
            }

            return RebuildNativeRecursive( nativeRoot, customGroups, nodeMap );
        }

        private bool TryIncludeBucket( CompiledNode node, BucketHandling handling, Dictionary<Type, CompiledNode> registryDict, Dictionary<Type, PlayerLoopSystem> map, HashSet<CompiledNode> activeNodes )
        {
            if( node.TargetBucket == null )
                return true;

            if( map.ContainsKey( node.TargetBucket ) )
                return true;

            if( handling == BucketHandling.Skip )
                return false;

            if( !registryDict.TryGetValue( node.TargetBucket, out var bucketNode ) )
            {
                // Attempt implicit scaffold if it exists in assembly
                bucketNode = Parse( node.TargetBucket );
                if( bucketNode == null )
                    return false;
            }

            // Handle native aliases. If a bucket targets a native alias, we must ensure it's resolved 
            // relative to the active native loop structure.
            if( bucketNode.IsNativeAlias )
            {
                if( map.TryGetValue( bucketNode.NativeAliasTarget, out var nativeSystem ) )
                {
                    // Move the entry to the stable alias key for consistent lookup.
                    map.Remove( bucketNode.NativeAliasTarget );
                    map[bucketNode.ID] = nativeSystem;

                    // Register the stable alias mapping so it propagates throughout the compilation.
                    _nativeToAlias[bucketNode.NativeAliasTarget] = bucketNode.ID;

                    return true;
                }

                if( bucketNode.TargetBucket == null )
                {
                    if( handling == BucketHandling.IncludeThrow )
                        throw new InvalidOperationException( $"[PlayerLoopCompiler] Native bucket alias '{bucketNode.ID.Name}' shadows missing native PlayerLoopSystem type '{bucketNode.NativeAliasTarget.Name}', and no TargetBucket was provided to scaffold it." );
                    return false;
                }
            }

            // If the bucket still doesn't exist, it means it's a custom group that needs to be scaffolded.
            if( !map.ContainsKey( bucketNode.ID ) )
            {
                if( !TryIncludeBucket( bucketNode, handling, registryDict, map, activeNodes ) )
                    return false;

                var newBucketSystem = new PlayerLoopSystem()
                {
                    type = bucketNode.IsNativeAlias ? bucketNode.NativeAliasTarget : bucketNode.ID,
                    updateDelegate = null
                };

                map[bucketNode.ID] = newBucketSystem;
                activeNodes.Add( bucketNode );
            }

            return true;
        }

        private PlayerLoopSystem RebuildNativeRecursive( PlayerLoopSystem nativeNode, Dictionary<Type, List<CompiledNode>> customGroups, Dictionary<Type, PlayerLoopSystem> nodeMap )
        {
            List<CompiledNode> customChildren = null;
            if( nativeNode.type != null )
            {
                Type identity = _nativeToAlias.TryGetValue( nativeNode.type, out var stableAlias )
                    ? stableAlias
                    : nativeNode.type;

                customChildren = customGroups.TryGetValue( identity, out var list )
                    ? list
                    : new List<CompiledNode>();
            }

            PlayerLoopSystem[] nativeChildren = nativeNode.subSystemList ?? Array.Empty<PlayerLoopSystem>();

            if( nativeChildren.Length > 0 || customChildren.Count > 0 )
            {
                nativeNode.subSystemList = SortBucket( nativeChildren, customChildren, customGroups, nodeMap );
            }

            return nativeNode;
        }

        private PlayerLoopSystem RebuildCustomRecursive( CompiledNode customNode, Dictionary<Type, List<CompiledNode>> customGroups, Dictionary<Type, PlayerLoopSystem> nodeMap )
        {
            var system = new PlayerLoopSystem()
            {
                type = customNode.ID,
                updateDelegate = customNode.Callback
            };

            List<CompiledNode> customChildren = customGroups.TryGetValue( customNode.ID, out var list )
                ? list
                : new List<CompiledNode>();

            if( customChildren.Count > 0 )
            {
                system.subSystemList = SortBucket( Array.Empty<PlayerLoopSystem>(), customChildren, customGroups, nodeMap );
            }

            return system;
        }

        private PlayerLoopSystem[] SortBucket( PlayerLoopSystem[] nativeChildren, List<CompiledNode> customChildren, Dictionary<Type, List<CompiledNode>> customGroups, Dictionary<Type, PlayerLoopSystem> nodeMap )
        {
            List<SortNode> sortNodes = new();

            for( int i = 0; i < nativeChildren.Length; i++ )
            {
                PlayerLoopSystem rebuiltNative = RebuildNativeRecursive( nativeChildren[i], customGroups, nodeMap );
                Type nativeIdentity = _nativeToAlias.TryGetValue( rebuiltNative.type, out var alias )
                    ? alias
                    : rebuiltNative.type;

                var sNode = new SortNode()
                {
                    ID = nativeIdentity,
                    System = rebuiltNative
                };

                if( i > 0 )
                {
                    Type prevIdentity = _nativeToAlias.TryGetValue( nativeChildren[i - 1].type, out var pAlias ) ? pAlias : nativeChildren[i - 1].type;
                    sNode.After.Add( prevIdentity );
                }
                sortNodes.Add( sNode );
            }

            if( customChildren != null )
            {
                foreach( var custom in customChildren )
                {
                    PlayerLoopSystem rebuiltCustom = RebuildCustomRecursive( custom, customGroups, nodeMap );
                    sortNodes.Add( new SortNode()
                    {
                        ID = custom.ID,
                        Before = custom.Before?.ToList() ?? new List<Type>(),
                        After = custom.After?.ToList() ?? new List<Type>(),
                        System = rebuiltCustom
                    } );
                }
            }

            List<SortNode> sortedList = sortNodes.SortDependencies( n => n.ID, n => n.Before, n => n.After, out var circularErrors );

            if( circularErrors.Any() )
            {
                Debug.LogError( $"[PlayerLoopCompiler] Circular dependency detected involving: {string.Join( ", ", circularErrors.Select( e => e.ID.Name ) )}" );
            }

            return sortedList.Select( n => n.System ).ToArray();
        }

        private void TraverseAndMapNative( PlayerLoopSystem current, Dictionary<Type, PlayerLoopSystem> map )
        {
            if( current.type != null )
            {
                Type identity = _nativeToAlias.TryGetValue( current.type, out var stableAlias )
                    ? stableAlias
                    : current.type;

                map[identity] = current;
            }

            if( current.subSystemList == null )
                return;

            foreach( var sub in current.subSystemList )
            {
                TraverseAndMapNative( sub, map );
            }
        }
    }
}