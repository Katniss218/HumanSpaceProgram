using System;
using UnityEngine;
using UnityPlus.Serialization.DataHandlers;
using UnityPlus.Serialization.ReferenceMaps;
using UnityPlus.Serialization.Strategies;

namespace UnityPlus.Serialization.Patching
{
    /// <summary>
    /// A patch that can create new <see cref="GameObject"/>s.
    /// </summary>
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
}
