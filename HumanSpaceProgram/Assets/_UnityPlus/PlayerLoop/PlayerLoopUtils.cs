using System.Text;
using UnityEngine;
using UnityEngine.LowLevel;

namespace UnityPlus.PlayerLoop
{
    public static class PlayerLoopUtils
    {
        public static void PrintCurrentPlayerLoop()
        {
            PrintPlayerLoop( UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop() );
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