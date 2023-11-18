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
    }

    public class CreateGameObjectsPatch : IPatch
    {
        SerializedArray _hierarachy;
        SerializedArray _data;

        public void Run( Patcher patcher )
        {
            // basically "run a deserialization on the data"

            JsonSeparateFileSerializedDataHandler dataHandler = new JsonSeparateFileSerializedDataHandler();
            ExplicitHierarchyGameObjectsStrategy strat = new ExplicitHierarchyGameObjectsStrategy( dataHandler, () => throw new Exception() );
#warning TODO - add synchronous methods to this strat.
            //Loader loader = new Loader( patcher.ReferenceStore, null, null, strat.Load_Object, strat.Load_Data );
            //loader.Load();
        }
    }

    public class DestroyGameObjectsPatch : IPatch
    {
        Guid[] _objectIds;

        public void Run( Patcher patcher )
        {
            foreach( var objectId in _objectIds )
            {
                GameObject go = (GameObject)patcher.ReferenceStore.GetObj( objectId );
                UnityEngine.Object.Destroy( go );
            }
        }
    }

    public class EditObjectsPatch : IPatch
    {
        (Guid objId, SerializedData data)[] _changes;

        public EditObjectsPatch( IEnumerable<(Guid objId, SerializedData data)> changes )
        {
            this._changes = changes.ToArray();
        }

        public void Run( Patcher patcher )
        {
            foreach( var change in _changes )
            {
                object obj = patcher.ReferenceStore.GetObj( change.objId );

                obj.SetData( patcher.ReferenceStore, change.data );
            }
        }
    }
}