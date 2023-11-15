using KSS.Core.ReferenceFrames;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.GameplayScene
{
    [RequireComponent( typeof( RootObjectTransform ) )]
    [DisallowMultipleComponent]
    public class ConstructionSite : MonoBehaviour
    {
        // construction site stores the patches to apply after each gameobject is completed.

        // if something is being attached to an object, the entire object becomes nonfunctional.
        // doesn't matter if it's only an engine being attached to the side of a vab somewhere.

        // construction site is always added to the root object.
        // there can be only one construction site per object. But the set of constructed objects can be expanded and shrunk dynamically.

        Dictionary<GameObject, IPatch[]> patchesLeftToApply; // list of patches to apply when the construction of the specific object is finished.

        Patcher patcher;

        public void Add( GameObject toConstructRoot, Transform parent )
        {
            if( parent.root != this.transform.root )
                throw new InvalidOperationException( $"Can't start construction if parent doesn't belong to this construction site." );

            // appends the specified object to the list of things under construction, and specifies under which object to parent it.
        }

        /// <summary>
        /// Removes the specified object, and all its children from construction.
        /// </summary>
        /// <param name="inProgressRoot">The root object of the subhierarchy to remove from construction.</param>
        public void Remove( GameObject inProgressRoot )
        {
            if( inProgressRoot.transform.root != this.transform.root )
                throw new InvalidOperationException( $"Can't remove construction if the object doesn't belong to this construction site." );

            if( notInProgress( inProgressRoot ) )
                throw new Exception();

            UnityEngine.Object.Destroy( inProgressRoot );
        }
    }
}
