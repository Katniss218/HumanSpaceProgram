using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Stores mappings between serialization groups and object instances.
    /// </summary>
    public class BidirectionalReferenceStore : IForwardReferenceMap, IReverseReferenceMap
    {
        private Dictionary<Guid, object> _forward = new Dictionary<Guid, object>();
        private Dictionary<object, Guid> _reverse = new Dictionary<object, Guid>();

        // forward

        public bool TryGetObj( Guid guid, out object obj )
        {
            return _forward.TryGetValue( guid, out obj );
        }

        public object GetObj( Guid id )
        {
            if( _forward.TryGetValue( id, out object obj ) )
                return obj;

            return null;
        }

        public void SetObj( Guid id, object obj )
        {
            _forward.Add( id, obj ); // same as setid
            _reverse.Add( obj, id );
        }

        // reverse

        public bool TryGetID( object obj, out Guid guid )
        {
            return _reverse.TryGetValue( obj, out guid );
        }

        public Guid GetID( object obj )
        {
            if( _reverse.TryGetValue( obj, out Guid id ) )
                return id;

            Guid guid = Guid.NewGuid();
            _forward.Add( id, obj );
            _reverse.Add( obj, id );

            return guid;
        }

        public void SetID( object obj, Guid id )
        {
            _forward.Add( id, obj ); // same as setobj
            _reverse.Add( obj, id );
        }
    }

    public static class BidirectionalReferenceStore_Ex
    {
        public static void UsePersistentReferenceStore( this ILoader l, IForwardReferenceMap store )
        {
            throw new NotImplementedException();
            // add store to a data method
        }


        public static void UsePersistentReferenceStore( this ISaver l, IReverseReferenceMap store )
        {
            throw new NotImplementedException();
            // add store to a 'first' method, to make sure the data uses its stored references instead of random.
        }
    }
}