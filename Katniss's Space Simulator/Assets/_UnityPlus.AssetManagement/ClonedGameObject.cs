using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.AssetManagement
{
    /// <summary>
    /// Marks the GameObject as created from a specified asset (prefab).
    /// </summary>
    /// <remarks>
    /// Not supposed to be added by hand. Please use <see cref="ClonedGameObject.Instantiate"/> to create an object from an asset.
    /// </remarks>
    [DisallowMultipleComponent]
    public class ClonedGameObject : MonoBehaviour
    {
        /// <summary>
        /// A reference to the original asset. Should remain loaded in the registry as long as the <see cref="ClonedGameObject"/> is active.
        /// </summary>
        [field: SerializeField]
        public GameObject OriginalAsset { get; private set; }

        void Start()
        {
            if( OriginalAsset == null )
            {
                Debug.LogWarning( $"{nameof( ClonedGameObject )} `{this.name}` - The `{nameof( OriginalAsset )}` was left unset, or the asset was unloaded destroying the link. Deleting the marker component..." );
                Destroy( this );
            }
        }

        /// <summary>
        /// Instantiates a gameobject from the specified original. <br />
        /// Marks the cloned object as created from the <paramref name="assetRef"/> using a <see cref="ClonedGameObject"/> component.
        /// </summary>
        public static GameObject Instantiate( GameObject assetRef )
        {
            GameObject gameObject = UnityEngine.Object.Instantiate( assetRef );

            ClonedGameObject assetComponent = gameObject.GetComponent<ClonedGameObject>();
            if( assetComponent == null )
            {
                assetComponent = gameObject.AddComponent<ClonedGameObject>();
            }

            assetComponent.OriginalAsset = assetRef;

            return gameObject;
        }
    }
}