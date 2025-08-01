﻿using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityPlus.UILib.UIElements;
using Object = UnityEngine.Object;

namespace UnityPlus.UILib
{
    /// <summary>
    /// Manages the various UI canvases used by the game.
    /// </summary>
    public static partial class CanvasManager
    {
        private static Dictionary<Scene, Dictionary<string, UICanvas>> _perSceneCanvases = new();
        private static Dictionary<Type, MethodInfo> _factoryMethod = new();

        public static TCanvas GetOrCreate<TCanvas>( Scene scene, string id ) where TCanvas : UICanvas
        {
            if( !scene.IsValid() || !scene.isLoaded )
                throw new ArgumentException( "The scene must be valid and loaded.", nameof( scene ) );

            if( string.IsNullOrEmpty( id ) )
                throw new ArgumentException( "The ID must not be null or empty.", nameof( id ) );

            if( TryGet<TCanvas>( scene, id, out var canvas ) )
            {
                if( !canvas.IsNullOrDestroyed() )
                {
                    return canvas;
                }

                // the scene might've been unloaded and the canvas destroyed.
                GetCanvasDict( scene ).Remove( id );
            }

            canvas = CreateCanvas<TCanvas>( scene, id );

            Register( scene, id, canvas );
            return canvas;
        }

        private static Dictionary<string, UICanvas> GetCanvasDict( Scene scene )
        {
            if( !_perSceneCanvases.TryGetValue( scene, out var canvases ) )
            {
                canvases = new Dictionary<string, UICanvas>();
                _perSceneCanvases[scene] = canvases;
            }
            return canvases;
        }

        /// <summary>
        /// Retrieves a canvas with the specified ID.
        /// </summary>
        /// <remarks>
        /// Tries to find the canvas is it isn't cached yet.
        /// </remarks>
        public static bool TryGet<TCanvas>( Scene scene, string id, out TCanvas canvas ) where TCanvas : UICanvas
        {
            if( !scene.IsValid() || !scene.isLoaded )
                throw new ArgumentException( "The scene must be valid and loaded.", nameof( scene ) );

            if( string.IsNullOrEmpty( id ) )
                throw new ArgumentException( "The ID must not be null or empty.", nameof( id ) );

            Dictionary<string, UICanvas> canvasesInScene = GetCanvasDict( scene );

            if( canvasesInScene.TryGetValue( id, out var canvas2 ) )
            {
                if( !canvas2.IsNullOrDestroyed() )
                {
                    if( canvas2 is not TCanvas )
                    {
                        canvas = default;
                        return false;
                    }
                    canvas = (TCanvas)canvas2;
                    return true;
                }
            }

            // If no canvas is found, we should try to find it, because it might've been loaded/created after the previous invocation of this method.
            TryRegisterUnknownPerSceneCanvases();

            if( canvasesInScene.TryGetValue( id, out canvas2 ) )
            {
                if( canvas2 == null )
                {
                    canvasesInScene.Remove( id );
                    canvas = default;
                    return false;
                }

                if( canvas2 is not TCanvas )
                {
                    canvas = default;
                    return false;
                }
                canvas = (TCanvas)canvas2;
                return true;
            }

            canvas = default;
            return false;
        }

        /// <summary>
        /// Retrieves a canvas with the specified ID.
        /// </summary>
        /// <remarks>
        /// Tries to find the canvas is it isn't cached yet.
        /// </remarks>
        public static TCanvas Get<TCanvas>( Scene scene, string id ) where TCanvas : UICanvas
        {
            if( !scene.IsValid() || !scene.isLoaded )
                throw new ArgumentException( "The scene must be valid and loaded.", nameof( scene ) );

            if( string.IsNullOrEmpty( id ) )
                throw new ArgumentException( "The ID must not be null or empty.", nameof( id ) );

            Dictionary<string, UICanvas> canvasesInScene = GetCanvasDict( scene );

            if( canvasesInScene.TryGetValue( id, out UICanvas canvas ) )
            {
                if( !canvas.IsNullOrDestroyed() )
                {
                    if( canvas is not TCanvas )
                    {
                        throw new ArgumentException( $"Tried to get canvas with id '{id}' and type '{typeof( TCanvas ).Name}', but the canvas was of type '{canvas.GetType().Name}'." );
                    }
                    return (TCanvas)canvas;
                }
            }

            // If no canvas is found, we should try to find it, because it might've been loaded/created after the previous invocation of this method.
            TryRegisterUnknownPerSceneCanvases();

            if( canvasesInScene.TryGetValue( id, out canvas ) )
            {
                if( canvas == null )
                {
                    canvasesInScene.Remove( id );
                    throw new ArgumentException( $"A canvas with the ID `{id}` doesn't exist." );
                }

                if( canvas is not TCanvas )
                {
                    throw new ArgumentException( $"Tried to get canvas with id '{id}' and type '{typeof( TCanvas ).Name}', but the canvas was of type '{canvas.GetType().Name}'." );
                }
                return (TCanvas)canvas;
            }

            throw new ArgumentException( $"A canvas with the ID `{id}` doesn't exist." );
        }

        /// <summary>
        /// Registers a canvas under a specified ID.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        public static void Register( Scene scene, string id, UICanvas canvas )
        {
            if( !scene.IsValid() || !scene.isLoaded )
                throw new ArgumentException( "The scene must be valid and loaded.", nameof( scene ) );

            if( string.IsNullOrEmpty( id ) )
                throw new ArgumentException( "The ID must not be null or empty.", nameof( id ) );

            if( canvas == null )
                throw new ArgumentNullException( nameof( canvas ) );

            Dictionary<string, UICanvas> canvasesInScene = GetCanvasDict( scene );

            if( canvasesInScene.ContainsKey( id ) )
            {
                throw new InvalidOperationException( $"Can't register a canvas under the name `{id}`. A canvas with this name is already registered." );
            }

            canvasesInScene[id] = canvas;
        }

        private static void TryRegisterUnknownPerSceneCanvases()
        {
            UICanvas[] canvasLayers = Object.FindObjectsOfType<UICanvas>();
            foreach( var canvasLayer in canvasLayers )
            {
                Scene scene = canvasLayer.gameObject.scene;
                Dictionary<string, UICanvas> canvasesInScene = GetCanvasDict( scene );

                if( canvasesInScene.TryGetValue( canvasLayer.ID, out UICanvas c ) )
                {
                    if( c.IsNullOrDestroyed() )
                    {
                        canvasesInScene.Remove( canvasLayer.ID );
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

                    Register( scene, canvasLayer.ID, canvas );
                }
                catch( Exception ex )
                {
                    Debug.LogError( $"Registering a canvas `{canvasLayer.gameObject.name}` threw an exception." );
                    Debug.LogException( ex );
                }
            }
        }
    }
}