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

        List<ISaver.Action> _dataActions;
        List<ISaver.Action> _objectActions;

        Action _startFunc;
        Action _finishFunc;

        public IReverseReferenceMap RefMap { get; set; }

        public Saver( IReverseReferenceMap refMap, Action startFunc, Action finishFunc, ISaver.Action objectAction, ISaver.Action dataAction )
        {
            this.RefMap = refMap;
            this._startFunc = startFunc;
            this._finishFunc = finishFunc;
            this._objectActions = new List<ISaver.Action>() { objectAction };
            this._dataActions = new List<ISaver.Action>() { dataAction };
        }

        public Saver( IReverseReferenceMap refMap, Action startFunc, Action finishFunc, IEnumerable<ISaver.Action> objectActions, IEnumerable<ISaver.Action> dataActions )
        {
            this.RefMap = refMap;
            this._startFunc = startFunc;
            this._finishFunc = finishFunc;
            this._objectActions = new List<ISaver.Action>( objectActions );
            this._dataActions = new List<ISaver.Action>( dataActions );
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
            //ClearReferenceRegistry();
            _startFunc?.Invoke();

            foreach( var action in _dataActions )
            {
                action?.Invoke( this.RefMap );
            }

            _currentState = ISaver.State.SavingObjects;

            foreach( var action in _objectActions )
            {
                action?.Invoke( this.RefMap );
            }

            _finishFunc?.Invoke();
            //ClearReferenceRegistry();
            _currentState = ISaver.State.Idle;
#if DEBUG
            Debug.Log( "Finished Saving" );
#endif
        }
    }
}