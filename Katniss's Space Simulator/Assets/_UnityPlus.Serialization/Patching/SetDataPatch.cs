using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityPlus.Serialization.ReferenceMaps;

namespace UnityPlus.Serialization.Patching
{
    /// <summary>
    /// A patch that can apply data to existing objects.
    /// </summary>
    public class SetDataPatch : IPatch
    {
        readonly (Guid objId, SerializedData data)[] _changes;

        public SetDataPatch( IEnumerable<(Guid objId, SerializedData data)> changes )
        {
            this._changes = changes.ToArray();
        }

        public void Run( BidirectionalReferenceStore refMap )
        {
            foreach( var change in _changes )
            {
                object obj = refMap.GetObj( change.objId );
                try
                {
                    obj.SetData( change.data, refMap );
                }
                catch( Exception ex )
                {
                    Debug.LogWarning( $"An exception occurred while running a change for object '{change.objId}' in a patch." );
                }
            }
        }
    }
}