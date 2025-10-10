using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP_Tests_PlayMode
{
    public class AssertMonoBehaviour : MonoBehaviour
    {
        public enum Step
        {
            FixedUpdate,
            Update,
            LateUpdate
        }

        private struct Entry
        {
            public Func<bool> shouldRun;
            public bool isOneShot;
            public Action assert;

            public Entry( Func<bool> shouldRun, bool isOneShot, Action assert )
            {
                this.shouldRun = shouldRun;
                this.isOneShot = isOneShot;
                this.assert = assert;
            }
        }

        List<Entry> _onFixedUpdate = new();
        List<Entry> _onUpdate = new();
        List<Entry> _onLateUpdate = new();

        private List<Entry> GetList( Step when )
        {
            switch( when )
            {
                case Step.FixedUpdate:
                    return _onFixedUpdate;
                case Step.Update:
                    return _onUpdate;
                case Step.LateUpdate:
                    return _onLateUpdate;
                default:
                    throw new ArgumentException( $"Unknown step" );
            }
        }

        public void AddAssert( Step when, Action assert )
        {
            AddAssert( when, null, false, assert );
        }

        public void AddAssert( Step when, Func<bool> shouldRun, Action assert )
        {
            AddAssert( when, shouldRun, false, assert );
        }

        public void AddAssert( Step when, Func<bool> shouldRun, bool isOneShot, Action assert )
        {
            var list = GetList( when );
            list.Add( new Entry( shouldRun, isOneShot, assert ) );
        }

        public void RemoveAssert( Step when, Action assert )
        {
            var list = GetList( when );
            for( int i = list.Count - 1; i >= 0; i-- )
            {
                if( list[i].assert == assert )
                    list.RemoveAt( i );
            }
        }

        public void Enable()
        {
            this.enabled = true;
        }

        public void Disable()
        {
            this.enabled = false;
        }

        void Awake() // Disabled initially, need to manually start.
        {
            this.enabled = false;
        }

        void FixedUpdate()
        {
            // Iterate on a snapshot - ToArray() - so the assert can remove itself.
            foreach( var entry in _onFixedUpdate.ToArray() )
            {
                if( entry.shouldRun == null || entry.shouldRun.Invoke() )
                {
                    entry.assert?.Invoke();

                    if( entry.isOneShot )
                        RemoveAssert( Step.FixedUpdate, entry.assert );
                }
            }
        }

        void Update()
        {
            foreach( var entry in _onUpdate.ToArray() )
            {
                if( entry.shouldRun == null || entry.shouldRun.Invoke() )
                {
                    entry.assert?.Invoke();

                    if( entry.isOneShot )
                        RemoveAssert( Step.Update, entry.assert );
                }
            }
        }

        void LateUpdate()
        {
            foreach( var entry in _onLateUpdate.ToArray() )
            {
                if( entry.shouldRun == null || entry.shouldRun.Invoke() )
                {
                    entry.assert?.Invoke();

                    if( entry.isOneShot )
                        RemoveAssert( Step.LateUpdate, entry.assert );
                }
            }
        }
    }
}