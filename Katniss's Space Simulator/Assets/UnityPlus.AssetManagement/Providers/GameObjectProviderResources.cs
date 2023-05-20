using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.AssetManagement.Providers
{
    public class GameObjectProviderResources : IAssetProvider<GameObject>
    {
        public IEnumerable<(string assetID, GameObject obj)> GetAll()
        {
            return new (string, GameObject)[] { };
        }

        public bool Get( string assetID, out GameObject obj )
        {
            obj = Resources.Load<GameObject>( assetID );
            return obj != null;
        }

        public bool GetAssetID( GameObject obj, out string assetID )
        {
            assetID = default;
            return false;
        }
    }
}
