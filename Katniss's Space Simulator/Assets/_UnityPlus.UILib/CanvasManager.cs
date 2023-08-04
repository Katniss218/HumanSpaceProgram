using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityPlus.UILib
{
    /// <summary>
    /// Manages the various canvases used by the game.
    /// </summary>
    public static class CanvasManager
    {
        static Dictionary<string, Canvas> _canvasDict = new Dictionary<string, Canvas>();

        /// <summary>
        /// Registers a canvas under a specified ID.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        public static void RegisterCanvas( string id, Canvas canvas )
        {
            if( _canvasDict.ContainsKey( id ) )
            {
                throw new InvalidOperationException( $"Can't register a canvas under the name `{id}`. A canvas with this name is already registered." );
            }

            _canvasDict[id] = canvas;
        }

        private static void TryRegisterUnknownCanvases()
        {
            CanvasID[] canvasLayers = Object.FindObjectsOfType<CanvasID>();
            foreach( var canvasLayer in canvasLayers )
            {
                if( canvasLayer.ID == null )
                {
                    Debug.LogWarning( $"Can't register a canvas `{canvasLayer.gameObject.name}` before its identifier is initialized." );
                }
                try
                {
                    Canvas canvas = canvasLayer.GetComponent<Canvas>();

                    RegisterCanvas( canvasLayer.ID, canvas );
                }
                catch( Exception ex )
                {
                    Debug.LogError( $"Registering a canvas `{canvasLayer.gameObject.name}` threw an exception." );
                    Debug.LogException( ex );
                }
            }
        }

        /// <summary>
        /// Retrieves a canvas with the specified ID.
        /// </summary>
        /// <remarks>
        /// Tries to find the canvas is it isn't cached yet.
        /// </remarks>
        public static Canvas GetCanvas( string id )
        {
            if( _canvasDict.TryGetValue( id, out Canvas canvas ) )
            {
                if( canvas == null )
                {
                    _canvasDict.Remove( id );
                    throw new ArgumentException( $"A canvas with the ID `{id}` doesn't exist." );
                }
                return canvas;
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
    }
}