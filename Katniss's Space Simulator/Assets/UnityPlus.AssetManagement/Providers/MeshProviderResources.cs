using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.AssetManagement.Providers
{
    public class MeshProviderResources : IAssetProvider<Mesh>
    {
        public IEnumerable<(string assetID, Mesh obj)> GetAll()
        {
            return new (string, Mesh)[] { };
        }

        public bool Get( string assetID, out Mesh obj )
        {
            obj = Resources.Load<Mesh>( assetID );
            return obj != null;
        }

        public bool GetAssetID( Mesh obj, out string assetID )
        {
            assetID = default;
            return false;
        }
    }
}
