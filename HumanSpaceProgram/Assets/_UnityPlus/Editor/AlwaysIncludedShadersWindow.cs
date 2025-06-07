#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityPlus
{
    public class AlwaysIncludedShadersWindow : EditorWindow
    {
        private static IReadOnlyList<Shader> _alwaysIncludedShaders;

        private Vector2 scrollPos;

        [MenuItem( "Window/Rendering/Always Included Shaders" )]
        public static void ShowWindow()
        {
            var window = GetWindow<AlwaysIncludedShadersWindow>();
            window.titleContent = new GUIContent( "Always Included Shaders" );
            window.minSize = new Vector2( 400, 300 );
            window.RefreshShaderList();  // Refresh on open
        }

        private void RefreshShaderList()
        {
            _alwaysIncludedShaders = AlwaysIncludedShadersUtils.GetAlwaysIncludedShaders();
            Repaint();
        }

        private void OnGUI()
        {
            GUILayout.Space( 10 );
            EditorGUILayout.HelpBox(
                $"This tool will scan all {nameof( Shader )} assets in the project and add them to the Always Included Shaders.\n",
                MessageType.Info
            );
            EditorGUILayout.HelpBox(
                $"This will make the builds SLOW AS FUCK, as every variant of every shader has to be compiled separately.",
                MessageType.Warning
            );
            GUILayout.Space( 10 );

            EditorGUILayout.LabelField( $"Always Included Shaders ({_alwaysIncludedShaders?.Count ?? 0})" );
            scrollPos = EditorGUILayout.BeginScrollView( scrollPos, GUILayout.Height( 200 ) );
            if( _alwaysIncludedShaders != null )
            {
                foreach( var shader in _alwaysIncludedShaders )
                {
                    if( shader != null )
                        EditorGUILayout.ObjectField( shader, typeof( Shader ), false );
                }
            }
            EditorGUILayout.EndScrollView();

            GUILayout.FlexibleSpace();

            if( GUILayout.Button( "Refresh Included Shaders", GUILayout.Height( 30 ) ) )
            {
                var shaders = AlwaysIncludedShadersUtils.FindAllShadersInProject();
                AlwaysIncludedShadersUtils.SetAlwaysIncludedShaders( shaders );
                RefreshShaderList();
            }
            if( GUILayout.Button( "Clear Included Shaders", GUILayout.Height( 30 ) ) )
            {
                AlwaysIncludedShadersUtils.SetAlwaysIncludedShaders( new Shader[] { } );
                RefreshShaderList();
            }
        }
    }
}
#endif