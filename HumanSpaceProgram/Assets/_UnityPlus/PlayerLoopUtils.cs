using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.LowLevel;

namespace UnityPlus
{
    public static class PlayerLoopUtils
    {
        public static void AddSystem( in PlayerLoopSystem systemToAdd )
        {
            PlayerLoopSystem loopRoot = PlayerLoop.GetCurrentPlayerLoop();
            AddSystemNested( new Type[] { }, 0, ref loopRoot, in systemToAdd );
            PlayerLoop.SetPlayerLoop( loopRoot );
        }
        
        public static void AddSystem<T1>( in PlayerLoopSystem systemToAdd )
        {
            PlayerLoopSystem loopRoot = PlayerLoop.GetCurrentPlayerLoop();
            AddSystemNested( new[] { typeof( T1 ) }, 0, ref loopRoot, in systemToAdd );
            PlayerLoop.SetPlayerLoop( loopRoot );
        }
        
        public static void AddSystem<T1, T2>( in PlayerLoopSystem systemToAdd )
        {
            PlayerLoopSystem loopRoot = PlayerLoop.GetCurrentPlayerLoop();
            AddSystemNested( new[] { typeof( T1 ), typeof( T2 ) }, 0, ref loopRoot, in systemToAdd );
            PlayerLoop.SetPlayerLoop( loopRoot );
        }
        
        public static void AddSystem<T1, T2, T3>( in PlayerLoopSystem systemToAdd )
        {
            PlayerLoopSystem loopRoot = PlayerLoop.GetCurrentPlayerLoop();
            AddSystemNested( new[] { typeof( T1 ), typeof( T2 ), typeof( T3 ) }, 0, ref loopRoot, in systemToAdd );
            PlayerLoop.SetPlayerLoop( loopRoot );
        }

        private static void AddSystemNested( Type[] types, int typeIndex, ref PlayerLoopSystem systemParent, in PlayerLoopSystem systemToInsert )
        {
            if( typeIndex == types.Length )
            {
                List<PlayerLoopSystem> playerLoopSystemList = systemParent.subSystemList?.ToList() ?? new List<PlayerLoopSystem>();

                playerLoopSystemList.Add( systemToInsert );
                systemParent.subSystemList = playerLoopSystemList.ToArray();
                return;
            }

            for( int i = 0; i < systemParent.subSystemList.Length; i++ )
            {
                if( systemParent.subSystemList[i].type == types[typeIndex] )
                {
                    AddSystemNested( types, typeIndex + 1, ref systemParent.subSystemList[i], in systemToInsert );
                }
            }
        }

        public static void InsertSystemAfter<T1>( in PlayerLoopSystem systemToInsert, Type previous )
        {
            PlayerLoopSystem loopRoot = PlayerLoop.GetCurrentPlayerLoop();
            InsertSystemNested( new[] { typeof( T1 ) }, 0, ref loopRoot, in systemToInsert, new List<Type>() { previous }, new Type[] { } );
            PlayerLoop.SetPlayerLoop( loopRoot );
        }

        public static void InsertSystemBefore<T1>( in PlayerLoopSystem systemToInsert, Type next )
        {
            PlayerLoopSystem loopRoot = PlayerLoop.GetCurrentPlayerLoop();
            InsertSystemNested( new[] { typeof( T1 ) }, 0, ref loopRoot, in systemToInsert, new List<Type>(), new Type[] { next } );
            PlayerLoop.SetPlayerLoop( loopRoot );
        }

        public static void InsertSystemBetween<T1>( in PlayerLoopSystem systemToInsert, Type previous, Type next )
        {
            PlayerLoopSystem loopRoot = PlayerLoop.GetCurrentPlayerLoop();
            InsertSystemNested( new[] { typeof( T1 ) }, 0, ref loopRoot, in systemToInsert, new List<Type>() { previous }, new Type[] { next } );
            PlayerLoop.SetPlayerLoop( loopRoot );
        }

        public static void InsertSystemAfter<T1, T2>( in PlayerLoopSystem systemToInsert, Type previous )
        {
            PlayerLoopSystem loopRoot = PlayerLoop.GetCurrentPlayerLoop();
            InsertSystemNested( new[] { typeof( T1 ), typeof( T2 ) }, 0, ref loopRoot, in systemToInsert, new List<Type>() { previous }, new Type[] { } );
            PlayerLoop.SetPlayerLoop( loopRoot );
        }

        public static void InsertSystemBefore<T1, T2>( in PlayerLoopSystem systemToInsert, Type next )
        {
            PlayerLoopSystem loopRoot = PlayerLoop.GetCurrentPlayerLoop();
            InsertSystemNested( new[] { typeof( T1 ), typeof( T2 ) }, 0, ref loopRoot, in systemToInsert, new List<Type>(), new Type[] { next } );
            PlayerLoop.SetPlayerLoop( loopRoot );
        }

        public static void InsertSystemBetween<T1, T2>( in PlayerLoopSystem systemToInsert, Type previous, Type next )
        {
            PlayerLoopSystem loopRoot = PlayerLoop.GetCurrentPlayerLoop();
            InsertSystemNested( new[] { typeof( T1 ), typeof( T2 ) }, 0, ref loopRoot, in systemToInsert, new List<Type>() { previous }, new Type[] { next } );
            PlayerLoop.SetPlayerLoop( loopRoot );
        }

        private static void InsertSystemNested( Type[] types, int typeIndex, ref PlayerLoopSystem systemParent, in PlayerLoopSystem systemToInsert, List<Type> previous, Type[] next )
        {
            if( typeIndex == types.Length )
            {
                if( systemParent.subSystemList == null )
                {
                    throw new ArgumentException( $"Can't insert a system before/after to a leaf node." );
                }

                List<Type> previousYetToEncounter = previous;
                int index = 0;

                for( int i = 0; i < systemParent.subSystemList.Length; i++ )
                {
                    // If we encounter anything that should be 'after', but before encountering everything that should be 'before' - error.
                    if( next.Contains( systemParent.subSystemList[i].type ) && previousYetToEncounter.Any() )
                    {
                        throw new ArgumentException( $"Circular dependency." );
                    }

                    if( !previousYetToEncounter.Any() ) // If there's nothing to go 'before', ignore anything else that might be 'after'.
                    {
                        if( !next.Any() )
                        {
                            index = i;
                            break;
                        }
                        else if( next.Contains( systemParent.subSystemList[i].type ) )
                        {
                            index = i;
                            break;
                        }
                    }

                    if( previousYetToEncounter.Contains( systemParent.subSystemList[i].type ) )
                    {
                        previousYetToEncounter.Remove( systemParent.subSystemList[i].type );
                    }
                }

                List<PlayerLoopSystem> playerLoopSystemList = systemParent.subSystemList?.ToList() ?? new List<PlayerLoopSystem>();

                playerLoopSystemList.Insert( index, systemToInsert );
                systemParent.subSystemList = playerLoopSystemList.ToArray();
                return;
            }

            for( int i = 0; i < systemParent.subSystemList.Length; i++ )
            {
                if( systemParent.subSystemList[i].type == types[typeIndex] )
                {
                    InsertSystemNested( types, typeIndex + 1, ref systemParent.subSystemList[i], in systemToInsert, previous, next );
                }
            }
        }


        public static void RemoveSystem<T1>( in PlayerLoopSystem systemToRemove )
        {
            PlayerLoopSystem loopRoot = PlayerLoop.GetCurrentPlayerLoop();
            RemoveSystemNested( new[] { typeof( T1 ) }, 0, ref loopRoot, in systemToRemove );
            PlayerLoop.SetPlayerLoop( loopRoot );
        }

        public static void RemoveSystem<T1, T2>( in PlayerLoopSystem systemToRemove )
        {
            PlayerLoopSystem loopRoot = PlayerLoop.GetCurrentPlayerLoop();
            RemoveSystemNested( new[] { typeof( T1 ), typeof( T2 ) }, 0, ref loopRoot, in systemToRemove );
            PlayerLoop.SetPlayerLoop( loopRoot );
        }

        private static void RemoveSystemNested( Type[] types, int typeIndex, ref PlayerLoopSystem systemParent, in PlayerLoopSystem systemToRemove )
        {
            if( typeIndex == types.Length )
            {
                if( systemParent.subSystemList == null )
                {
                    throw new ArgumentException( $"Can't remove a subsystem from a leaf node." );
                }

                var typeToRemove = systemToRemove.type;
                var delegateToRemove = systemToRemove.updateDelegate;

                List<PlayerLoopSystem> parentSubsystemList = systemParent.subSystemList.ToList();
                parentSubsystemList.RemoveAll( s => s.type == typeToRemove && s.updateDelegate == delegateToRemove );
                systemParent.subSystemList = parentSubsystemList.ToArray();

                return;
            }

            for( int i = 0; i < systemParent.subSystemList.Length; i++ )
            {
                if( systemParent.subSystemList[i].type == types[typeIndex] )
                {
                    RemoveSystemNested( types, typeIndex + 1, ref systemParent.subSystemList[i], in systemToRemove );
                }
            }
        }

        // https://github.com/adammyhre/Unity-Improved-Timers/blob/master/Runtime/PlayerLoopUtils.cs

        public static void PrintCurrentPlayerLoop()
        {
            PrintPlayerLoop( PlayerLoop.GetCurrentPlayerLoop() );
        }
        
        public static void PrintPlayerLoop( PlayerLoopSystem loop )
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine( "Unity Player Loop" );
            foreach( PlayerLoopSystem subSystem in loop.subSystemList )
            {
                PrintSubsystem( subSystem, sb, 0 );
            }
            Debug.Log( sb.ToString() );
        }

        private static void PrintSubsystem( PlayerLoopSystem system, StringBuilder sb, int level )
        {
            sb.Append( ' ', level * 2 ).AppendLine( system.type.ToString() );
            if( system.subSystemList == null || system.subSystemList.Length == 0 ) return;

            foreach( PlayerLoopSystem subSystem in system.subSystemList )
            {
                PrintSubsystem( subSystem, sb, level + 1 );
            }
        }
    }
}