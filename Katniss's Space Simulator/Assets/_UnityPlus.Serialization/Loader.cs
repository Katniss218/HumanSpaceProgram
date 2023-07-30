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
    public class Loader : ILoader
    {
        ILoader.State _currentState;

        List<Action<ILoader>> _objectActions = new List<Action<ILoader>>();
        List<Action<ILoader>> _dataActions = new List<Action<ILoader>>();

        Dictionary<Guid, object> _guidToObject = new Dictionary<Guid, object>();

        public Loader( IEnumerable<Action<ILoader>> objectActions, IEnumerable<Action<ILoader>> dataActions )
        {
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

        /// <summary>
        /// Registers the specified object with the specified ID.
        /// </summary>
        /// <remarks>
        /// Call this method when loading an object that might be referenced.
        /// </remarks>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void SetID( object obj, Guid id )
        {
            if( _currentState != ILoader.State.LoadingObjects )
            {
                throw new InvalidOperationException( $"You can only set an ID while creating the objects. Please move the functionality to an object action" );
            }

            _guidToObject.Add( id, obj );
        }

        /// <summary>
        /// Returns the previously registered object.
        /// </summary>
        /// <remarks>
        /// Call this method to deserialize a previously loaded object reference.
        /// </remarks>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public object Get( Guid id )
        {
            if( _guidToObject.TryGetValue( id, out object obj ) )
            {
                return obj;
            }
#if DEBUG
            Debug.Log( $"Tried to get a reference to object `{id:D}` before it was loaded." );
#endif
            return null;
        }

        //
        //  -- -- -- --
        //

        /// <summary>
        /// Performs a load from the current path, and with the current save actions.
        /// </summary>
        public void Load()
        {
            ClearReferenceRegistry();
            _currentState = ILoader.State.LoadingObjects;

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

            ClearReferenceRegistry();
            _currentState = ILoader.State.Idle;
        }
    }
}