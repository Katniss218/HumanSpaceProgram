using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization.DataHandlers;
using UnityPlus.Serialization.Strategies;

namespace UnityPlus.Serialization
{
    public interface IPatch
    {
        void Run( Patcher patcher );

        public static Dictionary<object, IPatch> CreateForwardPatches( (SerializedData o, SerializedData d) modified, (SerializedData o, SerializedData d) original )
        {
            // creates a patch based on the difference between modified and original.
            throw new NotImplementedException();

            // this creates a dict of patches that turn `original` into `modified`.
            // swapping the terms should create the inverse patch.
        }

        public static IPatch[] Invert( IEnumerable<IPatch> patches )
        {
            // creates the inverse of the specified list of patches
            throw new NotImplementedException();

            // for create object patches, replace them with destroy.
            // for destroy, replace them with create.
            // for edit - we need to store the previous data somewhere.
        }
    }

    public class CreateGameObjectsPatch : IPatch
    {
        SerializedArray hierarachy;
        SerializedArray data;

        public void Run( Patcher patcher )
        {
            // basically "run a deserialization on the data"

            JsonSeparateFileSerializedDataHandler dataHandler = new JsonSeparateFileSerializedDataHandler();
            ExplicitHierarchyGameObjectsStrategy strat = new ExplicitHierarchyGameObjectsStrategy( dataHandler, () => throw new Exception() );
#warning TODO - add synchronous methods to this strat.
            Loader loader = new Loader( null, null, strat.Load_Object, strat.Load_Data );
            loader.UsePersistentReferenceStore( patcher.ReferenceStore ); // Adds the serialization map to the reference store.
            loader.Load();
        }
    }

    public class DestroyGameObjectsPatch : IPatch
    {
        Guid[] objectIds;

        public void Run( Patcher patcher )
        {
            foreach( var objectId in objectIds )
            {
                GameObject go = (GameObject)patcher.ReferenceStore.GetObj( objectId );
                UnityEngine.Object.Destroy( go );
            }
        }
    }

    public class EditObjectsPatch : IPatch
    {
        (Guid objId, SerializedData data)[] changes;

        public void Run( Patcher patcher )
        {
            foreach( var change in changes )
            {
                object obj = patcher.ReferenceStore.GetObj( change.objId );

#warning TODO - Add a generalized (extension) method to apply the data to an arbitrary object type, making use of hardcoded setdata for builtin types and the IPersistent interface for custom.
                obj.SetData( patcher.ReferenceStore, change.data );
            }
        }
    }
}