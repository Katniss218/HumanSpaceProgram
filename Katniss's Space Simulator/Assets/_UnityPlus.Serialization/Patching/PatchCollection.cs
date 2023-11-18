using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.Serialization.Patching;

namespace UnityPlus.Serialization
{
    public class PatchCollection
    {
        private readonly Dictionary<object, List<IPatch>> _patches;

        public PatchCollection()
        {
            _patches = new Dictionary<object, List<IPatch>>();
        }

        public PatchCollection( Dictionary<object, List<IPatch>> patches )
        {
            _patches = patches;
        }

        public bool TryGetPatchesFor( object obj, out IEnumerable<IPatch> patches )
        {
            bool keyFound = _patches.TryGetValue( obj, out List<IPatch> patches2 );
            patches = patches2;
            return keyFound;
        }

        public void AddPatchFor( object obj, IPatch patch )
        {
            if( _patches.TryGetValue( obj, out List<IPatch> patches ) )
            {
                patches.Add( patch );
            }
            else
            {
                _patches.Add( obj, new List<IPatch>() { patch } );
            }
        }

        public static PatchCollection Combine( params PatchCollection[] patches )
        {
            var dict = new Dictionary<object, List<IPatch>>( patches[0]._patches.Count * 2 );
            for( int i = 0; i < patches.Length; i++ )
            {
                foreach( var kvp in patches[i]._patches )
                {
                    // already here - append patches to the end.
                    if( dict.TryGetValue( kvp.Key, out var value ) )
                    {
                        value.AddRange( kvp.Value );
                    }
                    // new - create patches.
                    else
                    {
                        dict.Add( kvp.Key, kvp.Value );
                    }
                }
            }
            return new PatchCollection( dict );
        }
    }
}