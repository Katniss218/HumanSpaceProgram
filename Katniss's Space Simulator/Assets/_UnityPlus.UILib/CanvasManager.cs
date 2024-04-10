using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityPlus.UILib.UIElements;
using Object = UnityEngine.Object;

namespace UnityPlus.UILib
{
    /// <summary>
    /// Manages the various UI canvases used by the game.
    /// </summary>
    public static class CanvasManager
    {
        private static Dictionary<string, UICanvas> _canvasDict = new();

        /// <summary>
        /// Retrieves a canvas with the specified ID.
        /// </summary>
        /// <remarks>
        /// Tries to find the canvas is it isn't cached yet.
        /// </remarks>
        public static UICanvas Get( string id )
        {
            if( _canvasDict.TryGetValue( id, out UICanvas canvas ) )
            {
                if( !canvas.IsNullOrDestroyed() )
                {
                    return canvas;
                }
            }

            // If no canvas is found, we should try to find it, because it might've been loaded/created after the previous invocation of this method.
            TryRegisterUnknownCanvases();

            if( _canvasDict.TryGetValue( id, out canvas ) )
            {
                if( canvas == null )
                {
                    _canvasDict.Remove( id );
                    throw new ArgumentException( $"A canvas with the ID `{id}` doesn't exist." );
                }
                return canvas;
            }

            throw new ArgumentException( $"A canvas with the ID `{id}` doesn't exist." );
        }

        private static void TryRegisterUnknownCanvases()
        {
            UICanvas[] canvasLayers = Object.FindObjectsOfType<UICanvas>();
            foreach( var canvasLayer in canvasLayers )
            {
                if( _canvasDict.TryGetValue( canvasLayer.ID, out UICanvas c ) )
                {
                    if( c.IsNullOrDestroyed() )
                    {
                        _canvasDict.Remove( canvasLayer.ID );
                    }
                    else
                    {
                        continue;
                    }
                }
                if( canvasLayer.ID == null )
                {
                    Debug.LogWarning( $"Can't register a canvas `{canvasLayer.gameObject.name}` before its ID is set." );
                    continue;
                }
                try
                {
                    UICanvas canvas = canvasLayer.GetComponent<UICanvas>();

                    Register( canvasLayer.ID, canvas );
                }
                catch( Exception ex )
                {
                    Debug.LogError( $"Registering a canvas `{canvasLayer.gameObject.name}` threw an exception." );
                    Debug.LogException( ex );
                }
            }
        }

        /// <summary>
        /// Registers a canvas under a specified ID.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        public static void Register( string id, UICanvas canvas )
        {
            if( _canvasDict.ContainsKey( id ) )
            {
                throw new InvalidOperationException( $"Can't register a canvas under the name `{id}`. A canvas with this name is already registered." );
            }

            _canvasDict[id] = canvas;
        }
    }
}