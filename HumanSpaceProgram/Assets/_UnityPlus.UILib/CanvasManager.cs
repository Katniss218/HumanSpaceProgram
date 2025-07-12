using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityPlus.UILib.UIElements;
using Object = UnityEngine.Object;

namespace UnityPlus.UILib
{
    /// <summary>
    /// Manages the various UI canvases used by the game.
    /// </summary>
    public static partial class CanvasManager
    {
        private static Dictionary<string, UICanvas> _canvases = new();

        public static TCanvas GetOrCreate<TCanvas>( string id ) where TCanvas : UICanvas
        {
            if( string.IsNullOrEmpty( id ) )
                throw new ArgumentException( "The ID must not be null or empty.", nameof( id ) );

            if( TryGet<TCanvas>( id, out var canvas ) )
            {
                return canvas;
            }

            canvas = CreateCanvas<TCanvas>( UnityEngine.SceneManagement.SceneManager.GetActiveScene(), id );

            Register( id, canvas );
            return canvas;
        }

        /// <summary>
        /// Retrieves a canvas with the specified ID.
        /// </summary>
        /// <remarks>
        /// Tries to find the canvas is it isn't cached yet.
        /// </remarks>
        public static bool TryGet<TCanvas>( string id, out TCanvas canvas ) where TCanvas : UICanvas
        {
            if( string.IsNullOrEmpty( id ) )
                throw new ArgumentException( "The ID must not be null or empty.", nameof( id ) );

            if( _canvases.TryGetValue( id, out var canvas2 ) )
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
            TryRegisterUnknownCanvases();

            if( _canvases.TryGetValue( id, out canvas2 ) )
            {
                if( canvas2 == null )
                {
                    _canvases.Remove( id );
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
        public static TCanvas Get<TCanvas>( string id ) where TCanvas : UICanvas
        {
            if( string.IsNullOrEmpty( id ) )
                throw new ArgumentException( "The ID must not be null or empty.", nameof( id ) );

            if( _canvases.TryGetValue( id, out UICanvas canvas ) )
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
            TryRegisterUnknownCanvases();

            if( _canvases.TryGetValue( id, out canvas ) )
            {
                if( canvas == null )
                {
                    _canvases.Remove( id );
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
        public static void Register( string id, UICanvas canvas )
        {
            if( string.IsNullOrEmpty( id ) )
                throw new ArgumentException( "The ID must not be null or empty.", nameof( id ) );

            if( canvas == null )
                throw new ArgumentNullException( nameof( canvas ) );

            if( _canvases.ContainsKey( id ) )
            {
                throw new InvalidOperationException( $"Can't register a canvas under the name `{id}`. A canvas with this name is already registered." );
            }

            _canvases[id] = canvas;
        }

        private static void TryRegisterUnknownCanvases()
        {
            UICanvas[] canvasLayers = Object.FindObjectsOfType<UICanvas>();
            foreach( var canvasLayer in canvasLayers )
            {
                if( _canvases.TryGetValue( canvasLayer.ID, out UICanvas c ) )
                {
                    if( c.IsNullOrDestroyed() )
                    {
                        _canvases.Remove( canvasLayer.ID );
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

        private static TCanvas CreateCanvas<TCanvas>( UnityEngine.SceneManagement.Scene scene, string id ) where TCanvas : UICanvas
        {
            TCanvas canvas;
            Type type = typeof( TCanvas );
            // Would be nice if we could use static interface members here instead of reflection.
            if( !_factoryMethod.TryGetValue( type, out MethodInfo method ) )
            {
                try
                {
                    method = typeof( TCanvas ).GetMethod( "Create", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof( UnityEngine.SceneManagement.Scene ), typeof( string ) }, null );
                    if( method == null )
                    {
                        throw new ArgumentException( $"The type '{typeof( TCanvas ).Name}' does not have a `static {typeof( TCanvas ).Name} Create( Scene scene, string id )` method." );
                    }
                }
                catch( Exception ex )
                {
                    throw new ArgumentException( $"The type '{typeof( TCanvas ).Name}' does not have a `static {typeof( TCanvas ).Name} Create( Scene scene, string id )` method.", ex );
                }
            }

            try
            {
                canvas = (TCanvas)method.Invoke( null, new object[] { scene, id } );
            }
            catch( Exception ex )
            {
                throw new ArgumentException( $"An exception occurred when trying to Create() a UICanvas of type `{type.Name}`.", ex );
            }

            return canvas;
        }
    }
}