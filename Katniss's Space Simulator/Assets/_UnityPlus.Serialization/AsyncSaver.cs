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
    public class AsyncSaver : IAsyncSaver
    {
        /// <summary>
        /// Specifies where to save the data.
        /// </summary>
        public string SaveDirectory { get; set; }

        public float CurrentActionPercentCompleted { get; set; }
        public float TotalPercentCompleted => (_completedActions + CurrentActionPercentCompleted) / (_objectActions.Count + _dataActions.Count);

        int _completedActions;

        ISaver.State _currentState;

        List<Func<ISaver, IEnumerator>> _dataActions = new List<Func<ISaver, IEnumerator>>();
        List<Func<ISaver, IEnumerator>> _objectActions = new List<Func<ISaver, IEnumerator>>();

        Dictionary<object, Guid> _objectToGuid = new Dictionary<object, Guid>();

        public AsyncSaver( string saveDirectory, IEnumerable<Func<ISaver, IEnumerator>> dataActions, IEnumerable<Func<ISaver, IEnumerator>> objectActions )
        {
            this.SaveDirectory = saveDirectory;

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

            return _objectToGuid.TryGetValue( obj, out id );
        }

        /// <summary>
        /// Registers the specified object in the registry (if not registered already) and returns its reference ID.
        /// </summary>
        /// <remarks>
        /// Call this to map an object to an ID when saving an object reference.
        /// </remarks>
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

            Guid newID = Guid.NewGuid();
            _objectToGuid.Add( obj, newID );
            return newID;
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
            ClearReferenceRegistry();
            _currentState = ISaver.State.SavingData;
            _completedActions = 0;
            CurrentActionPercentCompleted = 0.0f;

            // Should save data before objects, because data will add the objects that are referenced to the registry.
            foreach( var func in _dataActions )
            {
                yield return coroutineContainer.StartCoroutine( func( this ) );
                _completedActions++;
            }

            _currentState = ISaver.State.SavingObjects;

            foreach( var func in _objectActions )
            {
                yield return coroutineContainer.StartCoroutine( func( this ) );
                _completedActions++;
            }

            ClearReferenceRegistry();
            _currentState = ISaver.State.Idle;
#if DEBUG
            Debug.Log( "Finished Saving" );
#endif
        }

        /// <summary>
        /// Starts saving asynchronously, using the specified monobehaviour as a container for the coroutine.
        /// </summary>
        public void SaveAsync( MonoBehaviour coroutineContainer )
        {
            if( _currentState != ISaver.State.Idle )
            {
                throw new InvalidOperationException( $"This saver instance is already running." );
            }
            coroutineContainer.StartCoroutine( SaveCoroutine( coroutineContainer ) );
        }
    }
}