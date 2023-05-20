using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.AssetManagement.Providers
{
    public class MaterialProviderResources : IAssetProvider<Material>
    {
        public IEnumerable<(string assetID, Material obj)> GetAll()
        {
            return new (string, Material)[] { };
        }

        public bool Get( string assetID, out Material obj )
        {
            obj = Resources.Load<Material>( assetID );
            return obj != null;
        }

        public bool GetAssetID( Material obj, out string assetID )
        {
            assetID = default;
            return false;
        }
    }
}
