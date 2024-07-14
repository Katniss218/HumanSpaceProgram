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
    public class DefaultAssetSource : AssetSource
    {
        [field: SerializeField]
        public string AssetPrefix { get; set; } = "builtin::";

#if UNITY_EDITOR
        public override IEnumerable<AssetRegisterer.Entry> GetEntries()
        {
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
                assetID = $"{AssetPrefix}{assetID}";

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
                assetID = $"{AssetPrefix}{assetID}";

                AddEntry( ref entries, assetID, asset );
            }

            return entries.ToArray();
        }
#endif
    }
}