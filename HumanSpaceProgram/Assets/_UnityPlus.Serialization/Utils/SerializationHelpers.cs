using System;

namespace UnityPlus.Serialization
{
    public static class SerializationHelpers
    {
        /// <summary>
        /// Extracts the underlying SerializedArray from a data node.
        /// Enforces format based on <paramref name="forceStandardJson"/>.
        /// </summary>
        public static SerializedArray GetValueNode( SerializedData data, bool forceStandardJson )
        {
            if( forceStandardJson )
            {
                // Expect direct array
                return data as SerializedArray;
            }
            else
            {
                // Expect wrapper object
                if( data is SerializedObject obj && obj.TryGetValue( KeyNames.VALUE, out SerializedData inner ) && inner is SerializedArray innerArr )
                    return innerArr;

                return null;
            }
        }

        /// <summary>
        /// Creates a Data Node for a collection.
        /// If context settings or cycle detection requires it, creates a Boxed Object wrapper. 
        /// Otherwise returns a plain SerializedArray.
        /// </summary>
        /// <returns>The Root Node (wrapper or array) that should be linked to the parent.</returns>
        public static SerializedData CreateCollectionNode( object target, IReverseReferenceMap referenceMap, bool forceStandardJson, out SerializedArray arrayToPopulate )
        {
            arrayToPopulate = new SerializedArray();

            if( !forceStandardJson && target != null )
            {
                // Boxed Collection (Circular Ref Support)
                var wrapper = new SerializedObject();
                Guid id = referenceMap.GetID( target );

                Persistent_Guid.WriteIdHeader( wrapper, id );
                wrapper[KeyNames.VALUE] = arrayToPopulate;

                return wrapper;
            }

            // Standard: Array
            return arrayToPopulate;
        }
    }
}