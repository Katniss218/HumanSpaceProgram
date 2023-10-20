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
    /// An asynchronous saver.
    /// </summary>
    /// <remarks>
    /// Handles the pausing of the application to serialize correctly.
    /// </remarks>
    public sealed class AsyncSaver : IAsyncSaver
    {
        public float CurrentActionPercentCompleted { get; set; }
        public float TotalPercentCompleted => (_completedActions + CurrentActionPercentCompleted) / (_objectActions.Count + _dataActions.Count);

        int _completedActions;

        public ISaver.State CurrentState { get; private set; }

        List<Func<ISaver, IEnumerator>> _dataActions = new List<Func<ISaver, IEnumerator>>();
        List<Func<ISaver, IEnumerator>> _objectActions = new List<Func<ISaver, IEnumerator>>();

        Action _pauseFunc;
        Action _unpauseFunc;

        Dictionary<object, Guid> _objectToGuid = new Dictionary<object, Guid>();

        /// <param name="pauseFunc">A function delegate that can pause the game completely.</param>
        /// <param name="unpauseFunc">A function delegate that can unpause the game, and bring it to its previous state.</param>
        public AsyncSaver( Action pauseFunc, Action unpauseFunc, IEnumerable<Func<ISaver, IEnumerator>> objectActions, IEnumerable<Func<ISaver, IEnumerator>> dataActions )
        {
            if( pauseFunc == null )
                throw new ArgumentNullException( nameof( pauseFunc ), $"Pause delegate can't be null. {nameof( AsyncSaver )} requires the application to be paused to deserialize correctly." );
            if( unpauseFunc == null )
                throw new ArgumentNullException( nameof( unpauseFunc ), $"Unpause delegate can't be null. {nameof( AsyncSaver )} requires the application to be paused to deserialize correctly." );

            this._pauseFunc = pauseFunc;
            this._unpauseFunc = unpauseFunc;

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
            if( CurrentState == ISaver.State.Idle )
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
            if( CurrentState == ISaver.State.Idle )
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
            if( CurrentState == ISaver.State.Idle )
            {
                throw new InvalidOperationException( $"Can't save an object (or its ID) when the saver is idle." );
            }

            if( _objectToGuid.TryGetValue( obj, out Guid id ) )
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
        public IEnumerator SaveCoroutine( MonoBehaviour coroutineContainer )
        {
#if DEBUG
            Debug.Log( "Saving..." );
#endif
            _pauseFunc();
            ClearReferenceRegistry();
            CurrentState = ISaver.State.SavingData;
            _completedActions = 0;
            CurrentActionPercentCompleted = 0.0f;

            // Should save data before objects, because data will add the objects that are referenced to the registry.
            foreach( var func in _dataActions )
            {
                yield return coroutineContainer.StartCoroutine( func( this ) );
                _completedActions++;
            }

            CurrentState = ISaver.State.SavingObjects;

            foreach( var func in _objectActions )
            {
                yield return coroutineContainer.StartCoroutine( func( this ) );
                _completedActions++;
            }

            ClearReferenceRegistry();
            CurrentState = ISaver.State.Idle;
#if DEBUG
            Debug.Log( "Finished Saving" );
#endif
            _unpauseFunc();
        }

        /// <summary>
        /// Starts saving asynchronously, using the specified monobehaviour as a container for the coroutine.
        /// </summary>
        public void SaveAsync( MonoBehaviour coroutineContainer )
        {
            if( CurrentState != ISaver.State.Idle )
            {
                throw new InvalidOperationException( $"This saver instance is already running." );
            }

            coroutineContainer.StartCoroutine( SaveCoroutine( coroutineContainer ) );
        }
    }
}