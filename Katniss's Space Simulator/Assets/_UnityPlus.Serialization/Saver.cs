using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// A synchronous saver.
    /// </summary>
    public sealed class Saver : ISaver
    {
        ISaver.State _currentState;

        List<Action<ISaver>> _dataActions = new List<Action<ISaver>>();
        List<Action<ISaver>> _objectActions = new List<Action<ISaver>>();

        Dictionary<object, Guid> _objectToGuid = new Dictionary<object, Guid>();

        public Saver( IEnumerable<Action<ISaver>> dataActions, IEnumerable<Action<ISaver>> objectActions )
        {
            foreach( var action in objectActions )
            {
                this._objectActions.Add( action );
            }
            foreach( var action in dataActions )
            {
                this._dataActions.Add( action );
            }
        }

        //
        //  -- -- -- --
        //

        private void ClearReferenceRegistry()
        {
            _objectToGuid.Clear();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool TryGetID( object obj, out Guid id )
        {
            if( _currentState == ISaver.State.Idle )
            {
                throw new InvalidOperationException( $"Can't save an object (or its ID) when the saver is idle." );
            }

            if( obj == null )
            {
                id = Guid.Empty;
                return true;
            }

            return _objectToGuid.TryGetValue( obj, out id );
        }

        /// <summary>
        /// Registers the specified object in the registry (if not registered already) and returns its reference ID.
        /// </summary>
        /// <remarks>
        /// Call this to map an object to an ID when saving an object reference.
        /// </remarks>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Guid GetReferenceID( object obj )
        {
            if( _currentState == ISaver.State.Idle )
            {
                throw new InvalidOperationException( $"Can't save an object (or its ID) when the saver is idle." );
            }

            if( _objectToGuid.TryGetValue( obj, out Guid id ) )
            {
                return id;
            }

            if( obj == null )
                return Guid.Empty;

            Guid newID = Guid.NewGuid();
            _objectToGuid.Add( obj, newID );
            return newID;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Guid GetReferenceID( object obj, Guid guid )
        {
            if( _currentState == ISaver.State.Idle )
            {
                throw new InvalidOperationException( $"Can't save an object (or its ID) when the saver is idle." );
            }

            if( _objectToGuid.TryGetValue( obj, out Guid id ) ) // if registered, return old guid.
            {
                return id;
            }

            _objectToGuid.Add( obj, guid );
            return guid;
        }

        //
        //  -- -- -- --
        //

        /// <summary>
        /// Performs a save to the current path, and with the current save actions.
        /// </summary>
        public void Save()
        {
            ClearReferenceRegistry();
            _currentState = ISaver.State.SavingData;

            foreach( var action in _dataActions )
            {
                action?.Invoke( this );
            }

            _currentState = ISaver.State.SavingObjects;

            foreach( var action in _objectActions )
            {
                action?.Invoke( this );
            }

            ClearReferenceRegistry();
            _currentState = ISaver.State.Idle;
        }
    }
}