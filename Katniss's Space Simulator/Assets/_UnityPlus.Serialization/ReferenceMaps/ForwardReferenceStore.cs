using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Serialization.ReferenceMaps
{
    /// <summary>
    /// Stores mappings between serialization groups and object instances.
    /// </summary>
    public class ForwardReferenceStore : IForwardReferenceMap
    {
        private readonly Dictionary<Guid, object> _forward = new Dictionary<Guid, object>();

        public ForwardReferenceStore() { }

        public IEnumerable<(Guid id, object val)> GetAll()
        {
            return _forward.Select( kvp => (kvp.Key, kvp.Value) );
        }

        public void AddAll( IEnumerable<(Guid id, object val)> data )
        {
            foreach( var kvp in data )
            {
                _forward.Add( kvp.id, kvp.val );
            }
        }

        //
        //  FORWARD
        //

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool TryGetObj( Guid id, out object obj )
        {
            return _forward.TryGetValue( id, out obj );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public object GetObj( Guid id )
        {
            if( _forward.TryGetValue( id, out object obj ) )
                return obj;

            return null;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void SetObj( Guid id, object obj )
        {
            if( id == Guid.Empty || obj.IsUnityNull() )
                return;

            _forward.Add( id, obj );
        }
    }
}