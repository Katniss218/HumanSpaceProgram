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
    /// <summary>
    /// An asynchronous loader.
    /// </summary>
    /// <remarks>
    /// Handles the pausing of the application to deserialize correctly.
    /// </remarks>
    public sealed class AsyncLoader : IAsyncLoader
    {
        public float CurrentActionPercentCompleted { get; set; }
        public float TotalPercentCompleted => (_completedActions + CurrentActionPercentCompleted) / (_objectActions.Count + _dataActions.Count);

        int _completedActions;

        public ILoader.State CurrentState { get; private set; }

        List<Func<ILoader, IEnumerator>> _objectActions = new List<Func<ILoader, IEnumerator>>();
        List<Func<ILoader, IEnumerator>> _dataActions = new List<Func<ILoader, IEnumerator>>();

        Action _pauseFunc;
        Action _unpauseFunc;

        Dictionary<Guid, object> _guidToObject = new Dictionary<Guid, object>();

        /// <param name="pauseFunc">A function delegate that can pause the game completely.</param>
        /// <param name="unpauseFunc">A function delegate that can unpause the game, and bring it to its previous state.</param>
        public AsyncLoader( Action pauseFunc, Action unpauseFunc, IEnumerable<Func<ILoader, IEnumerator>> objectActions, IEnumerable<Func<ILoader, IEnumerator>> dataActions )
        {
            if( pauseFunc == null )
                throw new ArgumentNullException( nameof( pauseFunc ), $"Pause delegate can't be null. {nameof(AsyncLoader)} requires the application to be paused to serialize correctly." );
            if( unpauseFunc == null )
                throw new ArgumentNullException( nameof( unpauseFunc ), $"Unpause delegate can't be null. {nameof( AsyncLoader )} requires the application to be paused to serialize correctly." );

            this._pauseFunc = pauseFunc;
            this._unpauseFunc = unpauseFunc;

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
        public void SetReferenceID( object obj, Guid id )
        {
            if( CurrentState != ILoader.State.LoadingObjects )
            {
                throw new InvalidOperationException( $"You can only set an ID while creating the objects. Please move the functionality to an object action" );
            }

            if( id == Guid.Empty )
                return;

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

        //
        //  -- -- -- --
        //

        private IEnumerator LoadCoroutine( MonoBehaviour coroutineContainer )
        {
#if DEBUG
            Debug.Log( "Loading..." );
#endif
            _pauseFunc();
            ClearReferenceRegistry();
            CurrentState = ILoader.State.LoadingObjects;
            _completedActions = 0;
            CurrentActionPercentCompleted = 0.0f;

            foreach( var func in _objectActions )
            {
                yield return coroutineContainer.StartCoroutine( func( this ) );
                _completedActions++;
            }

            CurrentState = ILoader.State.LoadingData;

            foreach( var func in _dataActions )
            {
                yield return coroutineContainer.StartCoroutine( func( this ) );
                _completedActions++;
            }

            ClearReferenceRegistry();
            CurrentState = ILoader.State.Idle;
#if DEBUG
            Debug.Log( "Finished Loading" );
#endif
            _unpauseFunc();
        }

        /// <summary>
        /// Starts loading asynchronously, using the specified monobehaviour as a container for the coroutine.
        /// </summary>
        public void LoadAsync( MonoBehaviour coroutineContainer )
        {
            if( CurrentState != ILoader.State.Idle )
            {
                throw new InvalidOperationException( $"This loader instance is already running." );
            }
            coroutineContainer.StartCoroutine( LoadCoroutine( coroutineContainer ) );
        }
    }
}