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
    /// This component is NOT supposed to be added by hand. <br />
    /// Use <see cref="ClonedGameObject.Instantiate"/> to instantiate a <see cref="GameObject"/> from an asset.
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
            if( OriginalAsset == null ) // this is broken if the component is deserialized over multiple frames.
            {
                Debug.LogWarning( $"{nameof( ClonedGameObject )} `{this.name}` - The `{nameof( OriginalAsset )}` was left unset (possibly by adding this component explicitly) or the asset was unloaded, destroying the link to the asset. Deleting the marker component..." );
                Destroy( this );
            }
        }

        /// <summary>
        /// Instantiates a gameobject from the specified asset. <br />
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