using KSS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.AssetManagement;

namespace KSS
{
    /// <summary>
    /// A part source that loads parts (prefabs) from the `Resources` folder.
    /// </summary>
    public class AssetPartSource : PartFactory.PartSource
    {
        public AssetPartSource( string assetID ) : base( assetID )
        {
        }

        public override Transform Instantiate( Transform parent, Vector3 localPosition, Quaternion localRotation )
        {
            GameObject partGO = UnityEngine.Object.Instantiate( AssetRegistry.Get<GameObject>( PartID ), parent );

            partGO.transform.localPosition = localPosition;
            partGO.transform.localRotation = localRotation;

            return partGO.transform;
        }
    }
}