using System.Collections.Generic;
using System.Text;

namespace UnityPlus.Serialization
{
    public static class ExecutionStack_Ex
    {
        /// <summary>
        /// Reconstructs the path to the current cursor as a structured SerializedDataPath.
        /// </summary>
        public static SerializedDataPath BuildSerializedDataPath( this ExecutionStack stack )
        {
            if( stack == null || stack.Count == 0 )
                return new SerializedDataPath();

            var frames = stack.Frames;
            var segments = new List<SerializedDataPathSegment>( frames.Count );

            // Frame 0 is the Root object. The path segments describe how to get *from* root *to* the leaf.
            // Therefore, we skip index 0.
            for( int i = 1; i < frames.Count; i++ )
            {
                var cursor = frames[i];
                IMemberInfo member = cursor.TargetObj.Member;

                if( member != null )
                {
                    if( member.Name != null )
                    {
                        segments.Add( SerializedDataPathSegment.Named( member.Name ) );
                    }
                    else if( member.Index != -1 )
                    {
                        segments.Add( SerializedDataPathSegment.Indexed( member.Index ) );
                    }
                    else
                    {
                        // Fallback for unknown/dynamic members that lack info
                        segments.Add( SerializedDataPathSegment.Named( "?" ) );
                    }
                }
            }

            return new SerializedDataPath( segments );
        }

        /// <summary>
        /// Reconstructs the string path to the current cursor (e.g. "Root.Player.Inventory[0]").
        /// </summary>
        public static string BuildPath( this ExecutionStack stack )
        {
            // Delegate string formatting to the path struct itself.
            return BuildSerializedDataPath( stack ).ToString();
        }
    }
}