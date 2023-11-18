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
    public class BidirectionalReferenceStore : IForwardReferenceMap, IReverseReferenceMap
    {
        private readonly Dictionary<Guid, object> _forward = new Dictionary<Guid, object>();
        private readonly Dictionary<object, Guid> _reverse = new Dictionary<object, Guid>();

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

            _forward.Add( id, obj ); // same as setid
            _reverse.Add( obj, id );
        }

        //
        //  REVERSE
        //

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool TryGetID( object obj, out Guid id )
        {
            return _reverse.TryGetValue( obj, out id );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Guid GetID( object obj )
        {
            if( obj.IsUnityNull() )
                throw new ArgumentNullException( nameof( obj ), $"obj can't be null." );

            if( _reverse.TryGetValue( obj, out Guid id ) )
                return id;

            Guid guid = Guid.NewGuid();
            _forward.Add( id, obj );
            _reverse.Add( obj, id );

            return guid;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void SetID( object obj, Guid id )
        {
            if( obj.IsUnityNull() || id == Guid.Empty )
                return;

            _forward.Add( id, obj ); // same as setobj
            _reverse.Add( obj, id );
        }
    }
}