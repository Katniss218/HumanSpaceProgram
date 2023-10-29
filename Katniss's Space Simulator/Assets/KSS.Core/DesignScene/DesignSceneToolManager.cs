using KSS.Core.DesignScene.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Core.DesignScene
{
    /// <summary>
    /// Manages the available design scene viewport tools, and which is currently active.
    /// </summary>
    public class DesignSceneToolManager : MonoBehaviour
    {
        #region SINGLETON UGLINESS
        private static DesignSceneToolManager ___instance;
        private static DesignSceneToolManager instance
        {
            get
            {
                if( ___instance == null )
                {
                    ___instance = FindObjectOfType<DesignSceneToolManager>();
                }
                return ___instance;
            }
        }
        #endregion

        private List<MonoBehaviour> _tools = new List<MonoBehaviour>();
        private MonoBehaviour _currentTool = null;

        /// <summary>
        /// This can return null if no tool is selected.
        /// </summary>
        public static Type ActiveToolType { get => instance?._currentTool?.GetType(); }

        /// <summary>
        /// Registers a tool with the specified type.
        /// </summary>
        public static void RegisterTool<T>() where T : MonoBehaviour
        {
            if( instance == null )
            {
                throw new InvalidOperationException( $"{nameof( DesignSceneToolManager )} is accessible only in the design scene." );
            }

            foreach( var tool in instance._tools )
            {
                if( tool.GetType() == typeof( T ) )
                {
                    throw new InvalidOperationException( $"The tool of type {typeof( T ).FullName} has already been registered." );
                }
            }
            MonoBehaviour comp = instance.gameObject.AddComponent<T>();
            comp.enabled = false;

            instance._tools.Add( comp );
        }

        /// <summary>
        /// Selects a tool of a given type.
        /// </summary>
        /// <remarks>
        /// Tool instances are persisted. Selecting a tool, and going back to a previous one keeps its data.
        /// </remarks>
        public static void UseTool<T>() where T : MonoBehaviour
        {
            UseTool( typeof( T ) );
        }

        /// <summary>
        /// Selects a tool of a given type.
        /// </summary>
        /// <remarks>
        /// Tool instances are persisted. Selecting a tool, and going back to a previous one keeps its data.
        /// </remarks>
        public static void UseTool( Type toolType )
        {
            if( instance == null )
            {
                throw new InvalidOperationException( $"{nameof( DesignSceneToolManager )} is accessible only in the design scene." );
            }
            Type baseToolType = typeof( MonoBehaviour );
            if( !(baseToolType.IsAssignableFrom( toolType )) )
            {
                throw new ArgumentException( $"Can't register a tool that is not a {baseToolType.FullName}." );
            }

            MonoBehaviour tool = null;
            foreach( var t in instance._tools )
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

            if( instance._currentTool != null )
            {
                instance._currentTool.enabled = false;
            }
            instance._currentTool = tool;
            instance._currentTool.enabled = true;
            HSPEvent.EventManager.TryInvoke( HSPEvent.DESIGN_TOOL_CHANGED );
        }

        void Start()
        {
            try
            {
                UseTool<PickTool>();
            }
            catch( InvalidOperationException )
            {
                Debug.LogError( $"Couldn't find a registered {typeof( PickTool ).FullName}. Please do not unregister the {typeof( PickTool ).FullName}." );
            }
        }

        void Update()
        {
            TrySelectToolByNumberKey();
        }

        private void TrySelectToolByNumberKey()
        {
            if( Input.GetKeyDown( KeyCode.Alpha1 ) )
            {
                try
                {
                    UseTool( _tools[0].GetType() );
                }
                catch { }
            }
            if( Input.GetKeyDown( KeyCode.Alpha2 ) )
            {
                try
                {
                    UseTool( _tools[1].GetType() );
                }
                catch { }
            }
            if( Input.GetKeyDown( KeyCode.Alpha3 ) )
            {
                try
                {
                    UseTool( _tools[2].GetType() );
                }
                catch { }
            }
            if( Input.GetKeyDown( KeyCode.Alpha4 ) )
            {
                try
                {
                    UseTool( _tools[3].GetType() );
                }
                catch { }
            }
            if( Input.GetKeyDown( KeyCode.Alpha5 ) )
            {
                try
                {
                    UseTool( _tools[4].GetType() );
                }
                catch { }
            }
            if( Input.GetKeyDown( KeyCode.Alpha6 ) )
            {
                try
                {
                    UseTool( _tools[5].GetType() );
                }
                catch { }
            }
            if( Input.GetKeyDown( KeyCode.Alpha7 ) )
            {
                try
                {
                    UseTool( _tools[6].GetType() );
                }
                catch { }
            }
            if( Input.GetKeyDown( KeyCode.Alpha8 ) )
            {
                try
                {
                    UseTool( _tools[7].GetType() );
                }
                catch { }
            }
            if( Input.GetKeyDown( KeyCode.Alpha9 ) )
            {
                try
                {
                    UseTool( _tools[8].GetType() );
                }
                catch { }
            }
            if( Input.GetKeyDown( KeyCode.Alpha0 ) )
            {
                try
                {
                    UseTool( _tools[9].GetType() );
                }
                catch { }
            }
        }
    }
}