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
    /// A synchronous saver.
    /// </summary>
    public sealed class Saver : ISaver
    {
        ISaver.State _currentState;

        List<Action<ISaver>> _dataActions = new List<Action<ISaver>>();
        List<Action<ISaver>> _objectActions = new List<Action<ISaver>>();

        Action _startFunc;
        Action _finishFunc;

        Dictionary<object, Guid> _objectToGuid = new Dictionary<object, Guid>();

        public Saver( Action startFunc, Action finishFunc, Action<ISaver> objectAction, Action<ISaver> dataAction )
        {
            this._startFunc = startFunc;
            this._finishFunc = finishFunc;

            this._objectActions.Add( objectAction );
            this._dataActions.Add( dataAction );
        }

        public Saver( Action startFunc, Action finishFunc, IEnumerable<Action<ISaver>> objectActions, IEnumerable<Action<ISaver>> dataActions )
        {
            this._startFunc = startFunc;
            this._finishFunc = finishFunc;

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

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Guid GetID( object obj )
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
        public void SetID( object obj, Guid id )
        {
            if( _currentState == ISaver.State.Idle )
            {
                throw new InvalidOperationException( $"Can't save an object (or its ID) when the saver is idle." );
            }

            if( id == Guid.Empty )
                return;

            _objectToGuid.Add( obj, id );
        }

        //
        //  -- -- -- --
        //

        /// <summary>
        /// Performs a save to the current path, and with the current save actions.
        /// </summary>
        public void Save()
        {
#if DEBUG
            Debug.Log( "Saving..." );
#endif
            _currentState = ISaver.State.SavingData;
            ClearReferenceRegistry();
            _startFunc?.Invoke();

            foreach( var action in _dataActions )
            {
                action?.Invoke( this );
            }

            _currentState = ISaver.State.SavingObjects;

            foreach( var action in _objectActions )
            {
                action?.Invoke( this );
            }

            _finishFunc?.Invoke();
            ClearReferenceRegistry();
            _currentState = ISaver.State.Idle;
#if DEBUG
            Debug.Log( "Finished Saving" );
#endif
        }
    }
}