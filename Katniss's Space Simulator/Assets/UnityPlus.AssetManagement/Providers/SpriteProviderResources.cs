using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.AssetManagement.Providers
{
    public class SpriteProviderResources : IAssetProvider<Sprite>
    {
        public IEnumerable<(string assetID, Sprite obj)> GetAll()
        {
            return new (string, Sprite)[] { };
        }

        public bool Get( string assetID, out Sprite obj )
        {
            obj = Resources.Load<Sprite>( assetID );
            return obj != null;
        }

        public bool GetAssetID( Sprite obj, out string assetID )
        {
            assetID = default;
            return false;
        }
    }
}
