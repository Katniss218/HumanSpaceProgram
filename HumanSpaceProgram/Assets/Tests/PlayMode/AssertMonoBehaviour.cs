using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP_Tests_PlayMode
{
    public class AssertMonoBehaviour : MonoBehaviour
    {
        public sealed class FrameInfo
        {
            public int FixedUpdatesSinceLastUpdate { get; }
            public int UpdatesSinceLastFixedUpdate { get; }
            public int TotalFixedUpdates { get; }
            public int TotalUpdates { get; }

            public FrameInfo( int fixedUpdatesSinceLastUpdate, int updatesSinceLastFixedUpdate, int totalFixedUpdates, int totalUpdates )
            {
                FixedUpdatesSinceLastUpdate = fixedUpdatesSinceLastUpdate;
                UpdatesSinceLastFixedUpdate = updatesSinceLastFixedUpdate;
                TotalFixedUpdates = totalFixedUpdates;
                TotalUpdates = totalUpdates;
            }
        }

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
            public Action<FrameInfo> assert;

            public Entry( Func<bool> shouldRun, bool isOneShot, Action<FrameInfo> assert )
            {
                this.shouldRun = shouldRun;
                this.isOneShot = isOneShot;
                this.assert = assert;
            }
        }

        List<Entry> _onFixedUpdate = new();
        List<Entry> _onUpdate = new();
        List<Entry> _onLateUpdate = new();

        int _fixedUpdateCount = 0;
        int _updateCount = 0;

        int _totalFixedUpdateCount = 0;
        int _totalUpdateCount = 0;

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

        public void AddAssert( Step when, Action<FrameInfo> assert )
        {
            AddAssert( when, null, false, assert );
        }

        public void AddAssert( Step when, Func<bool> shouldRun, Action<FrameInfo> assert )
        {
            AddAssert( when, shouldRun, false, assert );
        }

        public void AddAssert( Step when, Func<bool> shouldRun, bool isOneShot, Action<FrameInfo> assert )
        {
            var list = GetList( when );
            list.Add( new Entry( shouldRun, isOneShot, assert ) );
        }

        public void RemoveAssert( Step when, Action<FrameInfo> assert )
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

        private FrameInfo GetFrameInfo()
        {
            return new FrameInfo( _fixedUpdateCount, _updateCount, _totalFixedUpdateCount, _totalUpdateCount );
        }

        void FixedUpdate()
        {
            var frameInfo = GetFrameInfo();

            // Iterate on a snapshot - ToArray() - so the assert can remove itself.
            foreach( var entry in _onFixedUpdate.ToArray() )
            {
                if( entry.shouldRun == null || entry.shouldRun.Invoke() )
                {
                    try
                    {
                        entry.assert?.Invoke( frameInfo );
                    }
                    catch( Exception e )
                    {
                        Debug.LogException( e );
                    }

                    if( entry.isOneShot )
                        RemoveAssert( Step.FixedUpdate, entry.assert );
                }
            }

            _fixedUpdateCount++;
            _totalFixedUpdateCount++;
            _updateCount = 0;
        }

        void Update()
        {
            var frameInfo = GetFrameInfo();

            foreach( var entry in _onUpdate.ToArray() )
            {
                if( entry.shouldRun == null || entry.shouldRun.Invoke() )
                {
                    try
                    {
                        entry.assert?.Invoke( frameInfo );
                    }
                    catch( Exception e )
                    {
                        Debug.LogException( e );
                    }

                    if( entry.isOneShot )
                        RemoveAssert( Step.Update, entry.assert );
                }
            }

            _updateCount++;
            _totalUpdateCount++;
            _fixedUpdateCount = 0;
        }

        void LateUpdate()
        {
            var frameInfo = GetFrameInfo();

            foreach( var entry in _onLateUpdate.ToArray() )
            {
                if( entry.shouldRun == null || entry.shouldRun.Invoke() )
                {
                    try
                    {
                        entry.assert?.Invoke( frameInfo );
                    }
                    catch( Exception e )
                    {
                        Debug.LogException( e );
                    }

                    if( entry.isOneShot )
                        RemoveAssert( Step.LateUpdate, entry.assert );
                }
            }
        }
    }
}