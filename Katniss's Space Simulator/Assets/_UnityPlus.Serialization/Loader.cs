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
    /// A synchronous loader.
    /// </summary>
    public sealed class Loader : ILoader
    {
        ILoader.State _currentState;

        List<Action<ILoader>> _objectActions = new List<Action<ILoader>>();
        List<Action<ILoader>> _dataActions = new List<Action<ILoader>>();

        Action _startFunc;
        Action _finishFunc;

        Dictionary<Guid, object> _guidToObject = new Dictionary<Guid, object>();

        public Loader( Action startFunc, Action finishFunc, Action<ILoader> objectAction, Action<ILoader> dataAction )
        {
            this._startFunc = startFunc;
            this._finishFunc = finishFunc;

            this._objectActions.Add( objectAction );
            this._dataActions.Add( dataAction );
        }

        public Loader( Action startFunc, Action finishFunc, IEnumerable<Action<ILoader>> objectActions, IEnumerable<Action<ILoader>> dataActions )
        {
            this._startFunc = startFunc;
            this._finishFunc = finishFunc;

            // Loader should load objects before data.
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
            _guidToObject.Clear();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool TryGetObj( Guid id, out object obj )
        {
            if( id == Guid.Empty )
            {
                obj = null;
                return false;
            }
            return _guidToObject.TryGetValue( id, out obj );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public object GetObj( Guid id )
        {
            if( id == Guid.Empty )
                return null;
            if( _guidToObject.TryGetValue( id, out object obj ) )
            {
                return obj;
            }
#if DEBUG
            Debug.Log( $"Tried to get a reference to object `{id:D}` before it was loaded." );
#endif
            return null;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void SetObj( Guid id, object obj )
        {
            if( _currentState != ILoader.State.LoadingObjects )
            {
                throw new InvalidOperationException( $"You can only set an ID while creating the objects. Please move the functionality to an object action" );
            }

            if( id == Guid.Empty )
                return;

            _guidToObject.Add( id, obj );
        }

        //
        //  -- -- -- --
        //

        public void Load()
        {
#if DEBUG
            Debug.Log( "Loading..." );
#endif
            _currentState = ILoader.State.LoadingObjects;
            ClearReferenceRegistry();
            _startFunc?.Invoke();

            // Create objects first, since data will reference them, so they must exist to be dereferenced.
            foreach( var action in _objectActions )
            {
                action?.Invoke( this );
            }

            _currentState = ILoader.State.LoadingData;

            foreach( var action in _dataActions )
            {
                action?.Invoke( this );
            }

            _finishFunc?.Invoke();
            ClearReferenceRegistry();
            _currentState = ILoader.State.Idle;
#if DEBUG
            Debug.Log( "Finished Loading" );
#endif
        }
    }
}