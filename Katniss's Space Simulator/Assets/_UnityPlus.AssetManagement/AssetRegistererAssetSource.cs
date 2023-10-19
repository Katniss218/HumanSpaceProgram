using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
//using UnityEditorInternal;
#endif

namespace UnityPlus.AssetManagement
{
#if UNITY_EDITOR
    [CustomEditor( typeof( AssetRegistererAssetSource ) )]
    public class AssetRegistererAssetSourceEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            AssetRegistererAssetSource updater = (AssetRegistererAssetSource)target;

            // Display the default inspector GUI elements
            DrawDefaultInspector();

            // Add a button to call a method
            if( GUILayout.Button( "Refresh Assets" ) )
            {
                updater.TryUpdateEntries();
            }
        }
    }
#endif

    public class AssetRegistererAssetSource : MonoBehaviour
    {
        [SerializeField]
        AssetRegisterer _registerer;

        [SerializeField]
        bool _autoUpdate = false;

        [SerializeField]
        string _constrainPath = "Assets/Resources/";

        [SerializeField]
        string _assetPrefix = "builtin::";

#if UNITY_EDITOR
        void AddEntry( ref List<AssetRegisterer.Entry> entries, string assetID, UnityEngine.Object asset )
        {
            if( asset is DefaultAsset // folders (maybe more)
             || asset is UnityEditorInternal.AssemblyDefinitionAsset
             || asset is MonoScript ) // C# files, but not scriptable object instances.
            {
                return;
            }

            entries.Add( new AssetRegisterer.Entry() { assetID = assetID, asset = asset } );
        }

        void LoadCustomAsset( ref List<AssetRegisterer.Entry> entries, string path )
        {
            // convert a path into asset(s).
            // some paths might have multiple assets to load.

            UnityEngine.Object[] assets;
            Type mainAssetType = AssetDatabase.GetMainAssetTypeAtPath( path );
            if( mainAssetType == typeof( SceneAsset ) )
            {
                assets = new[] { AssetDatabase.LoadMainAssetAtPath( path ) };
            }
            /*else if( mainAssetType == typeof( GameObject ) )
            {
                assets = new[] { AssetDatabase.LoadMainAssetAtPath( path ) };
            }*/
            else
            {
                assets = AssetDatabase.LoadAllAssetsAtPath( path );
            }

            if( assets.Length == 0 )
                return;

            // texture2D assets also have Sprite assets sometimes. in this case, we want a sprite.
            UnityEngine.Object asset = assets[0];
            if( mainAssetType == typeof( Texture2D ) )
            {
                for( int i = 1; i < assets.Length; i++ ) // start at 1, skips main.
                {
                    if( assets[i].GetType() == typeof( Sprite ) )
                    {
                        asset = assets[i];
                        break;
                    }
                }
            }
            // mesh assets are grouped under gameobjects, idk why but they are.
            else if( mainAssetType == typeof( GameObject ) )
            {
                for( int i = 1; i < assets.Length; i++ ) // start at 1, skips main.
                {
                    if( assets[i].GetType() == typeof( Mesh ) ) // should add multiple but whatever
                    {
                        asset = assets[i];
                        break;
                    }
                }
            }

            int start = _constrainPath.Length;
            int end = path.LastIndexOf( '.' );
            string assetID = end == -1 ? path[start..] : path[start..end];
            assetID = $"{_assetPrefix}{assetID}";

            AddEntry( ref entries, assetID, asset );
        }

        internal void TryUpdateEntries()
        {
            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();

            List<AssetRegisterer.Entry> entries = new List<AssetRegisterer.Entry>();

            var unityDefaultResources = AssetDatabase.LoadAllAssetsAtPath( "Library/unity default resources" );
            foreach( var asset in unityDefaultResources )
            {
                if( asset.name.StartsWith( "Hidden/" ) || asset.name.Contains( "Legacy " ) )
                {
                    continue;
                }

                int end = asset.name.LastIndexOf( '.' );
                string assetID = end == -1 ? asset.name : asset.name[..end];
                assetID = $"{_assetPrefix}{assetID}";

                AddEntry( ref entries, assetID, asset );
            }

            var unityBuiltinExtra = AssetDatabase.LoadAllAssetsAtPath( "Resources/unity_builtin_extra" );
            foreach( var asset in unityBuiltinExtra )
            {
                if( asset.name.StartsWith( "Hidden/" ) || asset.name.StartsWith( "Legacy Shaders/" ) )
                {
                    continue;
                }

                int end = asset.name.LastIndexOf( '.' );
                string assetID = end == -1 ? asset.name : asset.name[..end];
                assetID = $"{_assetPrefix}{assetID}";

                AddEntry( ref entries, assetID, asset );
            }

            foreach( var path in allAssetPaths )
            {
                if( !path.StartsWith( _constrainPath ) )
                {
                    continue;
                }

                LoadCustomAsset( ref entries, path );
            }

            _registerer.TrySetAssetsToRegister( entries.ToArray() );
        }

        void Awake()
        {
            var reg = this.GetComponents<AssetRegisterer>();
            if( reg.Length == 1 ) // if more, let user choose.
            {
                this._registerer = reg[0];
            }
        }

        void OnValidate()
        {
            if( _registerer != null && _autoUpdate )
            {
                TryUpdateEntries();
            }
        }
#endif
    }
}
