using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityPlus.AssetManagement.AssetSources
{
    public class CustomAssetSource : AssetSource
    {
        [field: SerializeField]
        public string ConstrainPath { get; set; } = "Assets/Resources/";

        [field: SerializeField]
        public string AssetPrefix { get; set; } = "builtin::";

#if UNITY_EDITOR
        private void LoadCustomAsset( ref List<AssetRegisterer.Entry> entries, string path )
        {
            // convert a path into asset(s).
            // some paths might have multiple assets to load.

            UnityEngine.Object[] assets;
            Type mainAssetType = AssetDatabase.GetMainAssetTypeAtPath( path );
            if( mainAssetType == typeof( SceneAsset ) )
            {
                assets = new[] { AssetDatabase.LoadMainAssetAtPath( path ) };
            }
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
                    if( assets[i] == null )
                        continue;

                    if( assets[i].GetType() == typeof( Mesh ) ) // should add multiple but whatever
                    {
                        asset = assets[i];
                        break;
                    }
                }
            }

            int start = ConstrainPath.Length;
            int end = path.LastIndexOf( '.' );
            string assetID = end == -1 ? path[start..] : path[start..end];
            assetID = $"{AssetPrefix}{assetID}";

            AddEntry( ref entries, assetID, asset );
        }

        public override IEnumerable<AssetRegisterer.Entry> GetEntries()
        {
            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();

            List<AssetRegisterer.Entry> entries = new List<AssetRegisterer.Entry>();

            foreach( var path in allAssetPaths )
            {
                if( !path.StartsWith( ConstrainPath ) )
                {
                    continue;
                }

                LoadCustomAsset( ref entries, path );
            }

            return entries.ToArray();
        }
#endif
    }
}