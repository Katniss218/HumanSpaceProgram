using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.LowLevel;

namespace UnityPlus
{
    public static class PlayerLoopUtils
    {
#warning TODO - Add a way to insert 'before' / 'after' a specified system type when inserting into a subsystem list.

        // https://github.com/adammyhre/Unity-Improved-Timers/blob/master/Runtime/PlayerLoopUtils.cs

        public static bool InsertSystem<T>( ref PlayerLoopSystem loop, in PlayerLoopSystem systemToInsert, int index )
        {
            if( loop.type != typeof( T ) )
                return InsertSystemNested<T>( ref loop, systemToInsert, index );

            var playerLoopSystemList = new List<PlayerLoopSystem>();
            if( loop.subSystemList != null )
                playerLoopSystemList.AddRange( loop.subSystemList );
            playerLoopSystemList.Insert( index, systemToInsert );
            loop.subSystemList = playerLoopSystemList.ToArray();
            return true;
        }

        private static bool InsertSystemNested<T>( ref PlayerLoopSystem loop, in PlayerLoopSystem systemToInsert, int index )
        {
            if( loop.subSystemList == null )
                return false;

            for( int i = 0; i < loop.subSystemList.Length; ++i )
            {
                if( !InsertSystem<T>( ref loop.subSystemList[i], in systemToInsert, index ) )
                    continue;
                return true;
            }

            return false;
        }

        public static void RemoveSystem<T>( ref PlayerLoopSystem loop, in PlayerLoopSystem systemToRemove )
        {
            if( loop.subSystemList == null )
                return;

            var playerLoopSystemList = new List<PlayerLoopSystem>( loop.subSystemList );
            for( int i = 0; i < playerLoopSystemList.Count; ++i )
            {
                if( playerLoopSystemList[i].type == systemToRemove.type && playerLoopSystemList[i].updateDelegate == systemToRemove.updateDelegate )
                {
                    playerLoopSystemList.RemoveAt( i );
                    loop.subSystemList = playerLoopSystemList.ToArray();
                }
            }

            RemoveSystemNested<T>( ref loop, systemToRemove );
        }

        private static void RemoveSystemNested<T>( ref PlayerLoopSystem loop, PlayerLoopSystem systemToRemove )
        {
            if( loop.subSystemList == null )
                return;

            for( int i = 0; i < loop.subSystemList.Length; ++i )
            {
                RemoveSystem<T>( ref loop.subSystemList[i], systemToRemove );
            }
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
