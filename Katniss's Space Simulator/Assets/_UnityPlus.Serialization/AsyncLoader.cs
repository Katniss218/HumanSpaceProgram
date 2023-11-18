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

        List<IAsyncLoader.Action> _objectActions;
        List<IAsyncLoader.Action> _dataActions;

        Action _startFunc;
        Action _finishFunc;

        public IForwardReferenceMap RefMap { get; set; }

        /// <param name="startFunc">A function delegate that can pause the game completely.</param>
        /// <param name="finishFunc">A function delegate that can unpause the game, and bring it to its previous state.</param>
        public AsyncLoader( IForwardReferenceMap refMap, Action startFunc, Action finishFunc, IAsyncLoader.Action objectAction, IAsyncLoader.Action dataAction )
        {
            if( startFunc == null )
                throw new ArgumentNullException( nameof( startFunc ), $"Start delegate can't be null. {nameof( AsyncLoader )} requires the function to pause to serialize correctly." );
            if( finishFunc == null )
                throw new ArgumentNullException( nameof( finishFunc ), $"Finish delegate can't be null. {nameof( AsyncLoader )} requires the function to unpause to serialize correctly." );

            this.RefMap = refMap;
            this._startFunc = startFunc;
            this._finishFunc = finishFunc;
            this._objectActions = new List<IAsyncLoader.Action>() { objectAction };
            this._dataActions = new List<IAsyncLoader.Action>() { objectAction };
        }

        /// <param name="startFunc">A function delegate that can pause the game completely.</param>
        /// <param name="finishFunc">A function delegate that can unpause the game, and bring it to its previous state.</param>
        public AsyncLoader( IForwardReferenceMap refMap, Action startFunc, Action finishFunc, IEnumerable<IAsyncLoader.Action> objectActions, IEnumerable<IAsyncLoader.Action> dataActions )
        {
            if( startFunc == null )
                throw new ArgumentNullException( nameof( startFunc ), $"Start delegate can't be null. {nameof( AsyncLoader )} requires the function to pause to serialize correctly." );
            if( finishFunc == null )
                throw new ArgumentNullException( nameof( finishFunc ), $"Finish delegate can't be null. {nameof( AsyncLoader )} requires the function to unpause to serialize correctly." );

            this.RefMap = refMap;
            this._startFunc = startFunc;
            this._finishFunc = finishFunc;
            this._objectActions = new List<IAsyncLoader.Action>( objectActions );
            this._dataActions = new List<IAsyncLoader.Action>( dataActions );
        }

        //
        //  -- -- -- --
        //

        private IEnumerator LoadCoroutine( MonoBehaviour coroutineContainer )
        {
#if DEBUG
            Debug.Log( "Loading..." );
#endif
            CurrentState = ILoader.State.LoadingObjects;
            //ClearReferenceRegistry();
            _startFunc();
            _completedActions = 0;
            CurrentActionPercentCompleted = 0.0f;

            foreach( var func in _objectActions )
            {
                yield return coroutineContainer.StartCoroutine( func( this.RefMap ) );
                _completedActions++;
            }

            CurrentState = ILoader.State.LoadingData;

            foreach( var func in _dataActions )
            {
                yield return coroutineContainer.StartCoroutine( func( this.RefMap ) );
                _completedActions++;
            }

            _finishFunc();
            //ClearReferenceRegistry();
            CurrentState = ILoader.State.Idle;
#if DEBUG
            Debug.Log( "Finished Loading" );
#endif
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