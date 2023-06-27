using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public class AsyncLoader : IAsyncLoader
    {
        /// <summary>
        /// Specifies where to save the data.
        /// </summary>
        public string SaveDirectory { get; set; }

        public float CurrentActionPercentCompleted { get; set; }
        public float TotalPercentCompleted => (_completedActions + CurrentActionPercentCompleted) / (_objectActions.Count + _dataActions.Count);

        int _completedActions;

        ILoader.State _currentState;

        List<Func<ILoader, IEnumerator>> _objectActions = new List<Func<ILoader, IEnumerator>>();
        List<Func<ILoader, IEnumerator>> _dataActions = new List<Func<ILoader, IEnumerator>>();

        Dictionary<Guid, object> _guidToObject = new Dictionary<Guid, object>();

        public AsyncLoader( string saveDirectory, IEnumerable<Func<ILoader, IEnumerator>> objectActions, IEnumerable<Func<ILoader, IEnumerator>> dataActions )
        {
            this.SaveDirectory = saveDirectory;

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

        private IEnumerator LoadCoroutine( MonoBehaviour coroutineContainer )
        {
#if DEBUG
            Debug.Log( "Loading..." );
#endif
            ClearReferenceRegistry();
            _currentState = ILoader.State.LoadingObjects;
            _completedActions = 0;
            CurrentActionPercentCompleted = 0.0f;

            foreach( var func in _objectActions )
            {
                yield return coroutineContainer.StartCoroutine( func( this ) );
                _completedActions++;
            }

            _currentState = ILoader.State.LoadingData;

            foreach( var func in _dataActions )
            {
                yield return coroutineContainer.StartCoroutine( func( this ) );
                _completedActions++;
            }

            ClearReferenceRegistry();
            _currentState = ILoader.State.Idle;
#if DEBUG
            Debug.Log( "Finished Loading" );
#endif
        }

        /// <summary>
        /// Starts loading asynchronously, using the specified monobehaviour as a container for the coroutine.
        /// </summary>
        public void LoadAsync( MonoBehaviour coroutineContainer )
        {
            if( _currentState != ILoader.State.Idle )
            {
                throw new InvalidOperationException( $"This loader instance is already running." );
            }
            coroutineContainer.StartCoroutine( LoadCoroutine( coroutineContainer ) );
        }
    }
}