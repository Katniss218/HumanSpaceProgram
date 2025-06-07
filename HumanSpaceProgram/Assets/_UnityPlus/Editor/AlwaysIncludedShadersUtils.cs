#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityPlus
{

    /// <summary>
    /// Contains the core logic for finding every Shader in the project and assigning them to GraphicsSettings.alwaysIncludedShaders.
    /// </summary>
    public static class AlwaysIncludedShadersUtils
    {
        public static string GraphicsSettingsAssetPath => "ProjectSettings/GraphicsSettings.asset";

        public static List<Shader> FindAllShadersInProject()
        {
            List<Shader> shaders = new();

            var unityDefaultResources = AssetDatabase.LoadAllAssetsAtPath( "Library/unity default resources" );
            foreach( var shader in unityDefaultResources.OfType<Shader>() )
            {
                if( shader.name.StartsWith( "Hidden/" ) || shader.name.Contains( "Legacy " ) )
                    continue;

                shaders.Add( shader );
            }

            var unityBuiltinExtra = AssetDatabase.LoadAllAssetsAtPath( "Resources/unity_builtin_extra" );
            foreach( var shader in unityBuiltinExtra.OfType<Shader>() )
            {
                if( shader.name.StartsWith( "Hidden/" ) || shader.name.StartsWith( "Legacy Shaders/" ) )
                    continue;

                shaders.Add( shader );
            }

            return shaders;
        }

        public static IReadOnlyList<Shader> GetAlwaysIncludedShaders()
        {
            var graphicsSettings = AssetDatabase.LoadAssetAtPath<GraphicsSettings>( GraphicsSettingsAssetPath );
            if( graphicsSettings == null )
            {
                Debug.LogWarning( $"The graphics settings asset at path '{GraphicsSettingsAssetPath}' couldn't be found." );
                return null;
            }

            UnityEditor.SerializedObject serializedGraphicsSettings = new UnityEditor.SerializedObject( graphicsSettings );
            UnityEditor.SerializedProperty alwaysIncludedShadersProp = serializedGraphicsSettings.FindProperty( "m_AlwaysIncludedShaders" );

            List<Shader> shaders = new List<Shader>( alwaysIncludedShadersProp.arraySize );
            for( int i = 0; i < alwaysIncludedShadersProp.arraySize; i++ )
            {
                UnityEditor.SerializedProperty shaderProp = alwaysIncludedShadersProp.GetArrayElementAtIndex( i );
                Shader shader = (Shader)shaderProp.objectReferenceValue;

                if( shader != null )
                    shaders.Add( shader );
            }

            return shaders;
        }

        public static void SetAlwaysIncludedShaders( IReadOnlyList<Shader> shaders )
        {
            if( shaders == null )
                throw new ArgumentNullException( nameof( shaders ) );

            var graphicsSettings = AssetDatabase.LoadAssetAtPath<GraphicsSettings>( GraphicsSettingsAssetPath );
            if( graphicsSettings == null )
            {
                Debug.LogWarning( $"The graphics settings asset at path '{GraphicsSettingsAssetPath}' couldn't be found." );
                return;
            }

            UnityEditor.SerializedObject serializedGraphicsSettings = new UnityEditor.SerializedObject( graphicsSettings );
            UnityEditor.SerializedProperty alwaysIncludedShadersProp = serializedGraphicsSettings.FindProperty( "m_AlwaysIncludedShaders" );

            alwaysIncludedShadersProp.arraySize = shaders.Count;
            for( int i = 0; i < shaders.Count; i++ )
            {
                Shader shader = shaders[i];
                UnityEditor.SerializedProperty shaderProp = alwaysIncludedShadersProp.GetArrayElementAtIndex( i );
                shaderProp.objectReferenceValue = shader;

                Debug.Log( $"Shader '{shader.name}' has been added to the Always Included Shaders." );
            }

            serializedGraphicsSettings.ApplyModifiedProperties();
            EditorUtility.SetDirty( graphicsSettings );
            AssetDatabase.SaveAssets();

            Debug.Log( $"[AlwaysIncludeShaders] Set {shaders.Count} shaders as Always Included Shaders." );
        }
    }
}
#endif