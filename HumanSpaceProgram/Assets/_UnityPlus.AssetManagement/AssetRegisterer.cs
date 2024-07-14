using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace UnityPlus.AssetManagement
{
#if UNITY_EDITOR
    [CustomPropertyDrawer( typeof( AssetRegisterer.Entry ) )]
    public class EntryDrawer : PropertyDrawer
    {
        public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
        {
            EditorGUI.BeginProperty( position, label, property );

            // Calculate the width for each field
            float labelWidth = 0.666666f;

            // Create rectangles for the two fields
            Rect assetIDRect = new Rect( position.x, position.y, (position.width * labelWidth), position.height );
            Rect assetRect = new Rect( position.x + (position.width * labelWidth), position.y, (position.width * (1 - labelWidth)), position.height );

            // Get the serialized properties for the two fields
            SerializedProperty assetIDProperty = property.FindPropertyRelative( nameof( AssetRegisterer.Entry.assetID ) );
            SerializedProperty assetProperty = property.FindPropertyRelative( nameof( AssetRegisterer.Entry.asset ) );

            // Draw the two fields side by side without labels
            EditorGUI.PropertyField( assetIDRect, assetIDProperty, GUIContent.none );
            EditorGUI.PropertyField( assetRect, assetProperty, GUIContent.none );

            EditorGUI.EndProperty();
        }
    }

    [CustomEditor( typeof( AssetRegisterer ) )]
    public class AssetRegistererEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            AssetRegisterer assetRegisterer = (AssetRegisterer)target;

            // Display the default inspector GUI elements
            DrawDefaultInspector();

            // Add a button to call a method
            if( GUILayout.Button( "Refresh Assets" ) )
            {
                assetRegisterer.UpdateEntries();
                EditorSceneManager.MarkSceneDirty( assetRegisterer.gameObject.scene );
            }

            // Add a button to call a method
            if( GUILayout.Button( "Clear Assets" ) )
            {
                assetRegisterer.ClearEntries();
                EditorSceneManager.MarkSceneDirty( assetRegisterer.gameObject.scene );
            }
        }
    }
#endif

    /// <summary>
    /// Registers a specific set of assets (Unity Objects) when its first initialized.
    /// </summary>
    [DefaultExecutionOrder( int.MinValue )]
    public class AssetRegisterer : MonoBehaviour
    {
        /// <summary>
        /// Describes a specific `asset ID - asset reference` pair.
        /// </summary>
        [Serializable]
        public struct Entry
        {
            /// <summary>
            /// The asset ID to register under.
            /// </summary>
            public string assetID;

            /// <summary>
            /// The asset to register.
            /// </summary>
            public UnityEngine.Object asset;

            public Entry( string assetId, UnityEngine.Object asset )
            {
                this.assetID = assetId;
                this.asset = asset;
            }
        }

        [SerializeField]
        private Entry[] _assetsToRegister;

        [SerializeField]
        public AssetSource[] assetSources;

        [field: SerializeField]
        public bool AutoUpdate { get; set; } = false;

        void Awake()
        {
            if( _assetsToRegister == null )
            {
                return;
            }

            foreach( var entry in _assetsToRegister )
            {
                if( entry.assetID == null )
                {
                    Debug.LogWarning( $"Null Asset ID present in the list of Assets to register (Asset: {entry.asset})." );
                    continue;
                }
                if( entry.asset == null )
                {
                    Debug.LogWarning( $"Null Asset present in the list of Assets to register (Asset ID: {entry.asset})." );
                    continue;
                }
                AssetRegistry.Register( entry.assetID, entry.asset );
            }

            // Allows to garbage collect them later, if unloaded from the registry.
            _assetsToRegister = null;
        }

#if UNITY_EDITOR

        internal void UpdateEntries()
        {
            if( assetSources == null )
            {
                _assetsToRegister = null;
                return;
            }

            List<Entry> entries = new();

            foreach( var assetSource in assetSources )
            {
                if( assetSource == null )
                    continue;

                var e = assetSource.GetEntries();

                foreach( var entry in e )
                {
                    if( entries.Any( e => e.assetID == entry.assetID ) )
                        continue;

                    entries.Add(entry);
                }
            }

            _assetsToRegister = entries.ToArray();
        }

        internal void ClearEntries()
        {
            _assetsToRegister = null;
        }

        void OnValidate()
        {
            if( !AutoUpdate )
            {
                return;
            }

            if( Application.isPlaying )
            {
                return;
            }

            UpdateEntries();
        }
#endif
    }
}