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
        void Run( BidirectionalReferenceStore refMap );
    }

    public class CreateGameObjectsPatch : IPatch
    {
        static JsonSeparateFileSerializedDataHandler dataHandler = new JsonSeparateFileSerializedDataHandler();
        static ExplicitHierarchyGameObjectsStrategy strat = new ExplicitHierarchyGameObjectsStrategy( dataHandler, () => throw new Exception() );
        static Loader loader = new Loader( null, null, null, strat.Load_Object, strat.Load_Data );

        SerializedArray _hierarachy;
        SerializedArray _data;

        public void Run( BidirectionalReferenceStore refMap )
        {
            // basically "run a deserialization on the data"

            loader.RefMap = refMap;
            loader.Load();
        }
    }

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

    public class EditObjectsPatch : IPatch
    {
        (Guid objId, SerializedData data)[] _changes;

        public EditObjectsPatch( IEnumerable<(Guid objId, SerializedData data)> changes )
        {
            this._changes = changes.ToArray();
        }

        public void Run( BidirectionalReferenceStore refMap )
        {
            foreach( var change in _changes )
            {
                object obj = refMap.GetObj( change.objId );

                obj.SetData( refMap, change.data );
            }
        }
    }
}