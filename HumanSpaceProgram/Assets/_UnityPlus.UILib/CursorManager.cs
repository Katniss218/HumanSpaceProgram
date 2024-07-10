using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.UILib
{
    public static class CursorManager
    {
        private static readonly Dictionary<string, (Texture2D texture, Vector2 hotspot)> _cursors = new();

        private static string _activeCursorType;

        public static string ActiveCursorType => _activeCursorType;

        /// <summary>
        /// Adds a new named cursor with a texture and pivot.
        /// </summary>
        /// <param name="cursorType">The name of the cursor to register.</param>
        /// <param name="texture">The texture to associate with the cursor.</param>
        /// <param name="pivot">The texture coordinate of the "tip" of the cursor. (0, 0) is at the top-left.</param>
        public static void AddCursor( string cursorType, Texture2D texture, Vector2 pivot )
        {
            _cursors[cursorType] = (texture, pivot);
        }

        /// <summary>
        /// Sets the active cursor and its texture.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the cursor type is not registered.</exception>
        public static void SetActiveCursor( string cursorType )
        {
            if( !_cursors.TryGetValue( cursorType, out var cursor ) )
            {
                throw new ArgumentException( $"Unknown cursor type '{cursorType}'. Valid cursors: ({string.Join( ", ", _cursors.Keys.Select( s => $"'{s}'" ) )})", nameof( cursorType ) );
            }

            Cursor.SetCursor( cursor.texture, cursor.hotspot, CursorMode.Auto );
            _activeCursorType = cursorType;
        }
    }
}