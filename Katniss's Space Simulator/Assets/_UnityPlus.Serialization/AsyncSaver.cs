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

        List<IAsyncSaver.Action> _dataActions;
        List<IAsyncSaver.Action> _objectActions;

        Action _startFunc;
        Action _finishFunc;

        public IReverseReferenceMap RefMap { get; set; }

        /// <param name="startFunc">A function delegate that can pause the game completely.</param>
        /// <param name="finishFunc">A function delegate that can unpause the game, and bring it to its previous state.</param>
        public AsyncSaver( IReverseReferenceMap refMap, Action startFunc, Action finishFunc, IAsyncSaver.Action objectAction, IAsyncSaver.Action dataAction )
        {
            if( startFunc == null )
                throw new ArgumentNullException( nameof( startFunc ), $"Start delegate can't be null. {nameof( AsyncSaver )} requires the function to pause to deserialize correctly." );
            if( finishFunc == null )
                throw new ArgumentNullException( nameof( finishFunc ), $"Finish delegate can't be null. {nameof( AsyncSaver )} requires the function to unpause to deserialize correctly." );

            this.RefMap = refMap;
            this._startFunc = startFunc;
            this._finishFunc = finishFunc;
            this._objectActions = new List<IAsyncSaver.Action>() { objectAction };
            this._dataActions = new List<IAsyncSaver.Action>() { objectAction };
        }

        /// <param name="startFunc">A function delegate that can pause the game completely.</param>
        /// <param name="finishFunc">A function delegate that can unpause the game, and bring it to its previous state.</param>
        public AsyncSaver( IReverseReferenceMap refMap, Action startFunc, Action finishFunc, IEnumerable<IAsyncSaver.Action> objectActions, IEnumerable<IAsyncSaver.Action> dataActions )
        {
            if( startFunc == null )
                throw new ArgumentNullException( nameof( startFunc ), $"Start delegate can't be null. {nameof( AsyncSaver )} requires the function to pause to deserialize correctly." );
            if( finishFunc == null )
                throw new ArgumentNullException( nameof( finishFunc ), $"Finish delegate can't be null. {nameof( AsyncSaver )} requires the function to unpause to deserialize correctly." );

            this.RefMap = refMap;
            this._startFunc = startFunc;
            this._finishFunc = finishFunc;
            this._objectActions = new List<IAsyncSaver.Action>( objectActions );
            this._dataActions = new List<IAsyncSaver.Action>( dataActions );
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
            CurrentState = ISaver.State.SavingData;
            //ClearReferenceRegistry();
            _startFunc?.Invoke();
            _completedActions = 0;
            CurrentActionPercentCompleted = 0.0f;

            // Should save data before objects, because data will add the objects that are referenced to the registry.
            foreach( var func in _dataActions )
            {
                yield return coroutineContainer.StartCoroutine( func( this.RefMap ) );
                _completedActions++;
            }

            CurrentState = ISaver.State.SavingObjects;

            foreach( var func in _objectActions )
            {
                yield return coroutineContainer.StartCoroutine( func( this.RefMap ) );
                _completedActions++;
            }

            _finishFunc?.Invoke();
            //ClearReferenceRegistry();
            CurrentState = ISaver.State.Idle;
#if DEBUG
            Debug.Log( "Finished Saving" );
#endif
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