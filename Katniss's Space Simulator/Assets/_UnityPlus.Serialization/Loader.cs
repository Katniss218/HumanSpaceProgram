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

        List<ILoader.Action> _objectPasses;
        List<ILoader.Action> _referencePasses;

        Action _startFunc;
        Action _finishFunc;

        public IForwardReferenceMap RefMap { get; set; }

        public Loader( IForwardReferenceMap refMap, ILoader.Action objectPass, ILoader.Action referencePass )
        {
            this.RefMap = refMap;
            this._startFunc = null;
            this._finishFunc = null;
            this._objectPasses = new List<ILoader.Action>() { objectPass };
            this._referencePasses = new List<ILoader.Action>() { referencePass };
        }
        
        public Loader( IForwardReferenceMap refMap, Action startFunc, Action finishFunc, ILoader.Action objectPass, ILoader.Action referencePass )
        {
            this.RefMap = refMap;
            this._startFunc = startFunc;
            this._finishFunc = finishFunc;
            this._objectPasses = new List<ILoader.Action>() { objectPass };
            this._referencePasses = new List<ILoader.Action>() { referencePass };
        }

        public Loader( IForwardReferenceMap refMap, IEnumerable<ILoader.Action> objectPasses, IEnumerable<ILoader.Action> referencePasses )
        {
            this.RefMap = refMap;
            this._startFunc = null;
            this._finishFunc = null;
            this._objectPasses = new List<ILoader.Action>( objectPasses );
            this._referencePasses = new List<ILoader.Action>( referencePasses );
        }
        
        public Loader( IForwardReferenceMap refMap, Action startFunc, Action finishFunc, IEnumerable<ILoader.Action> objectPasses, IEnumerable<ILoader.Action> referencePasses )
        {
            this.RefMap = refMap;
            this._startFunc = startFunc;
            this._finishFunc = finishFunc;
            this._objectPasses = new List<ILoader.Action>( objectPasses );
            this._referencePasses = new List<ILoader.Action>( referencePasses );
        }

        //
        //  -- -- -- --
        //

        public void Load()
        {
#if DEBUG
     //       Debug.Log( "Loading..." );
#endif
            _currentState = ILoader.State.LoadingObjects;
            _startFunc?.Invoke();

            // Create objects first, since data will reference them, so they must exist to be dereferenced.
            foreach( var action in _objectPasses )
            {
                action?.Invoke( this.RefMap );
            }

            _currentState = ILoader.State.LoadingReferences;

            foreach( var action in _referencePasses )
            {
                action?.Invoke( this.RefMap );
            }

            _finishFunc?.Invoke();
            _currentState = ILoader.State.Idle;
#if DEBUG
      //      Debug.Log( "Finished Loading" );
#endif
        }
    }
}