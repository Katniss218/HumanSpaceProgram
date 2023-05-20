using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.AssetManagement.Providers
{
    public class FontAssetProviderResources : IAssetProvider<TMPro.TMP_FontAsset>
    {
        public IEnumerable<(string assetID, TMPro.TMP_FontAsset obj)> GetAll()
        {
            return new (string, TMPro.TMP_FontAsset)[] { };
        }

        public bool Get( string assetID, out TMPro.TMP_FontAsset obj )
        {
            obj = Resources.Load<TMPro.TMP_FontAsset>( assetID );
            return obj != null;
        }

        public bool GetAssetID( TMPro.TMP_FontAsset obj, out string assetID )
        {
            assetID = default;
            return false;
        }
    }
}