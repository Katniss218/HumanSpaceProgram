using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace UnityPlus.AssetManagement
{
    public abstract class AssetSource : MonoBehaviour
    {
#if UNITY_EDITOR
        protected void AddEntry( ref List<AssetRegisterer.Entry> entries, string assetID, UnityEngine.Object asset )
        {
            if( asset is DefaultAsset // folders (maybe more)
             || asset is UnityEditorInternal.AssemblyDefinitionAsset
             || asset is MonoScript ) // C# files, but not scriptable object instances.
            {
                return;
            }

            entries.Add( new AssetRegisterer.Entry( assetID, asset ) );
        }

        public abstract IEnumerable<AssetRegisterer.Entry> GetEntries();
#endif
    }
}
