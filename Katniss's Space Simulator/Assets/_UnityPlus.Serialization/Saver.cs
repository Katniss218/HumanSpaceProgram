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

        List<ISaver.Action> _passes;

        Action _startFunc;
        Action _finishFunc;

        public IReverseReferenceMap RefMap { get; set; }

        public Saver( IReverseReferenceMap refMap, ISaver.Action pass )
        {
            this.RefMap = refMap;
            this._startFunc = null;
            this._finishFunc = null;
            this._passes = new List<ISaver.Action>() { pass };
        }
        
        public Saver( IReverseReferenceMap refMap, Action startFunc, Action finishFunc, ISaver.Action pass )
        {
            this.RefMap = refMap;
            this._startFunc = startFunc;
            this._finishFunc = finishFunc;
            this._passes = new List<ISaver.Action>() { pass };
        }

        public Saver( IReverseReferenceMap refMap, IEnumerable<ISaver.Action> passes )
        {
            this.RefMap = refMap;
            this._startFunc = null;
            this._finishFunc = null;
            this._passes = new List<ISaver.Action>( passes );
        }

        public Saver( IReverseReferenceMap refMap, Action startFunc, Action finishFunc, IEnumerable<ISaver.Action> passes )
        {
            this.RefMap = refMap;
            this._startFunc = startFunc;
            this._finishFunc = finishFunc;
            this._passes = new List<ISaver.Action>( passes );
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
            _currentState = ISaver.State.Saving;
            _startFunc?.Invoke();

            foreach( var action in _passes )
            {
                action?.Invoke( this.RefMap );
            }

            _finishFunc?.Invoke();
            _currentState = ISaver.State.Idle;
#if DEBUG
            Debug.Log( "Finished Saving" );
#endif
        }
    }
}