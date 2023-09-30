using KSS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS
{
    /// <summary>
    /// A part source that loads parts (prefabs) from the `Resources` folder.
    /// </summary>
    public class AssetPartSource : PartFactory.PartSource
    {
        const string PATH = "Prefabs/Parts/";

        private GameObject _objCache;

        public AssetPartSource( string partID ) : base( partID )
        {
        }

        public override Transform Instantiate( Transform parent, Vector3 localPosition, Quaternion localRotation )
        {
#warning TODO - Use UnityPlus.AssetManagement.AssetRegistry, since part prefabs are assets.
            if( _objCache == null )
            {
                _objCache = Resources.Load<GameObject>( PATH + PartID );
            }

            GameObject partGO = UnityEngine.Object.Instantiate( _objCache, parent );

            partGO.transform.localPosition = localPosition;
            partGO.transform.localRotation = localRotation;

            return partGO.transform;
        }
    }
}