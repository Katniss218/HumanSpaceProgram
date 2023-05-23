using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Core.Serialization
{
    /// <summary>
    /// A part source that loads parts (prefabs) from the `Resources` folder.
    /// </summary>
    public class AssetPartSource : PartFactory.PartSource
    {
        public string ResourcePath { get; set; }

        private GameObject _objCache;

        public AssetPartSource( string resourcePath )
        {
            this.ResourcePath = resourcePath;
        }

        public override Part Instantiate( Transform parent, Vector3 localPosition, Quaternion localRotation )
        {
            if( _objCache == null )
            {
                _objCache = Resources.Load<GameObject>( ResourcePath );
            }

            GameObject partGO = UnityEngine.Object.Instantiate( _objCache, parent );
            Part part = partGO.GetComponent<Part>();
            if( part == null )
            {
                throw new InvalidOperationException( $"A part with the path '{ResourcePath}' didn't have a {nameof( Part )} component." );
            }
            partGO.transform.localPosition = localPosition;
            partGO.transform.localRotation = localRotation;

            return part;
        }
    }
}
