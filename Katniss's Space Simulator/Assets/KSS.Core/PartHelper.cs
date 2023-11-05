using KSS.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.AssetManagement;

namespace KSS.Core
{
    /// <summary>
    /// Use this class to register and access parts.
    /// </summary>
    public static class PartHelper
    {
        public static PartMetadata GetPartMetadata( string partId )
        {
            return AssetRegistry.Get<PartMetadata>( $"part::m/{partId}" );
        }
        public static PartMetadata[] GetAllParts()
        {
            var assetsAndIds = AssetRegistry.GetAll<PartMetadata>( $"part::m/" ); 
            PartMetadata[] assets = new PartMetadata[assetsAndIds.Length];
            for( int i = 0; i < assetsAndIds.Length; i++ )
            {
                assets[i] = assetsAndIds[i].asset;
            }
            return assets;
        }
        public static GameObject InstantiatePart( string partId )
        {
            return AssetRegistry.Get<GameObject>( $"part::h/{partId}" );
        }

        public static void RegisterPart( PartMetadata partMetadata, Func<GameObject> partInstantiator )
        {
            AssetRegistry.Register( $"part::m/{partMetadata.ID}", partMetadata );
            AssetRegistry.RegisterLazy( $"part::h/{partMetadata.ID}", partInstantiator, isCacheable: false );
        }
    }
}