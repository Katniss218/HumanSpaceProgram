using System;
using UnityEngine;
using UnityPlus.Serialization.ReferenceMaps;

namespace UnityPlus.Serialization.Patching
{
    /// <summary>
    /// A patch that can destroy existing <see cref="GameObject"/>s.
    /// </summary>
    public class DestroyGameObjectsPatch : IPatch
    {
        Guid[] _objectIds;

        public void Run( BidirectionalReferenceStore refMap )
        {
            foreach( var objectId in _objectIds )
            {
                GameObject go = (GameObject)refMap.GetObj( objectId );
                UnityEngine.Object.Destroy( go );
            }
        }
    }
}