using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Stores mappings between serialization groups and object instances.
    /// </summary>
    public class ReverseReferenceStore : IReverseReferenceMap
    {
        private Dictionary<object, Guid> _reverse = new Dictionary<object, Guid>();

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
            _reverse.Add( obj, id );

            return guid;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void SetID( object obj, Guid id )
        {
            if( obj.IsUnityNull() || id == Guid.Empty )
                return;

            _reverse.Add( obj, id );
        }
    }
}