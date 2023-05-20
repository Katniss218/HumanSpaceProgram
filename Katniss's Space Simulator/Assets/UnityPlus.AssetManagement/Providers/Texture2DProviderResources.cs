using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.AssetManagement.Providers
{
    public class Texture2DProviderResources : IAssetProvider<Texture2D>
    {
        public IEnumerable<(string assetID, Texture2D obj)> GetAll()
        {
            return new (string, Texture2D)[] { };
        }

        public bool Get( string assetID, out Texture2D obj )
        {
            obj = Resources.Load<Texture2D>( assetID );
            return obj != null;
        }

        public bool GetAssetID( Texture2D obj, out string assetID )
        {
            assetID = default;
            return false;
        }
    }
}
