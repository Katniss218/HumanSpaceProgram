using KSS.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KSS.GameplayScene
{
    /// <summary>
    /// Manages the registered and active gameplay scene viewport tools.
    /// </summary>
    public class GameplaySceneToolManager : SingletonMonoBehaviour<GameplaySceneToolManager>
    {
        private List<MonoBehaviour> _availableTools = new List<MonoBehaviour>();
        private MonoBehaviour _activeTool = null;

        public static Type ActiveToolType { get => instance._activeTool.GetType(); }

        /// <summary>
        /// Registers a tool with the specified type for future use.
        /// </summary>
        public static void RegisterTool<T>() where T : MonoBehaviour
        {
            if( instance == null )
            {
                throw new InvalidOperationException( $"{nameof( GameplaySceneToolManager )} is accessible only in the gameplay scene." );
            }

            foreach( var tool in instance._availableTools )
            {
                if( tool.GetType() == typeof( T ) )
                {
                    throw new InvalidOperationException( $"The tool of type {typeof( T ).FullName} has already been registered." );
                }
            }

            MonoBehaviour comp = instance.gameObject.AddComponent<T>();
            comp.enabled = false;

            instance._availableTools.Add( comp );
        }

        /// <summary>
        /// Selects a tool of a given type.
        /// </summary>
        /// <remarks>
        /// Tool instances are persisted. Selecting a tool, and going back to a previous one keeps its data.
        /// </remarks>
        /// <returns>The instance of the tool that was enabled.</returns>
        public static T UseTool<T>() where T : MonoBehaviour
        {
            return (T)UseTool( typeof( T ) );
        }

        /// <summary>
        /// Selects a tool of a given type.
        /// </summary>
        /// <remarks>
        /// Tool instances are persisted. Selecting a tool, and going back to a previous one keeps its data.
        /// </remarks>
        /// <returns>The instance of the tool that was enabled.</returns>
        public static object UseTool( Type toolType )
        {
            if( instance == null )
            {
                throw new InvalidOperationException( $"{nameof( GameplaySceneToolManager )} is accessible only in the gameplay scene." );
            }

            Type baseToolType = typeof( MonoBehaviour );
            if( !(baseToolType.IsAssignableFrom( toolType )) )
            {
                throw new ArgumentException( $"Can't register a tool that is not a {baseToolType.FullName}." );
            }

            MonoBehaviour tool = null;
            foreach( var t in instance._availableTools )
            {
                if( t.GetType() == toolType )
                {
                    tool = t;
                    break;
                }
            }

            if( tool == null )
            {
                throw new InvalidOperationException( $"A tool of type {toolType.FullName} has not been registered. Please register a tool type before trying to use it." );
            }

            // Tool already being used.
            if( instance._activeTool == tool )
            {
                return instance._activeTool;
            }

            if( instance._activeTool != null )
            {
                instance._activeTool.enabled = false;
            }

            instance._activeTool = tool;
            instance._activeTool.enabled = true;
            HSPEvent.EventManager.TryInvoke( HSPEvent.GAMEPLAY_AFTER_TOOL_CHANGED );
            return instance._activeTool;
        }
    }
}