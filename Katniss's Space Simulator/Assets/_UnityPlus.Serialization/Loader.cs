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

        List<ILoader.Action> _objectActions;
        List<ILoader.Action> _dataActions;

        Action _startFunc;
        Action _finishFunc;

        public IForwardReferenceMap RefMap { get; set; }

        public Loader( IForwardReferenceMap refMap, Action startFunc, Action finishFunc, ILoader.Action objectAction, ILoader.Action dataAction )
        {
            this.RefMap = refMap;
            this._startFunc = startFunc;
            this._finishFunc = finishFunc;
            this._objectActions = new List<ILoader.Action>() { objectAction };
            this._dataActions = new List<ILoader.Action>() { dataAction };
        }

        public Loader( IForwardReferenceMap refMap, Action startFunc, Action finishFunc, IEnumerable<ILoader.Action> objectActions, IEnumerable<ILoader.Action> dataActions )
        {
            this.RefMap = refMap;
            this._startFunc = startFunc;
            this._finishFunc = finishFunc;
            this._objectActions = new List<ILoader.Action>( objectActions );
            this._dataActions = new List<ILoader.Action>( dataActions );
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
            //ClearReferenceRegistry();
            _startFunc?.Invoke();

            // Create objects first, since data will reference them, so they must exist to be dereferenced.
            foreach( var action in _objectActions )
            {
                action?.Invoke( this.RefMap );
            }

            _currentState = ILoader.State.LoadingData;

            foreach( var action in _dataActions )
            {
                action?.Invoke( this.RefMap );
            }

            _finishFunc?.Invoke();
            //ClearReferenceRegistry();
            _currentState = ILoader.State.Idle;
#if DEBUG
            Debug.Log( "Finished Loading" );
#endif
        }
    }
}