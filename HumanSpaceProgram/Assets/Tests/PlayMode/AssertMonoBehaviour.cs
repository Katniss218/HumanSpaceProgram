using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP_Tests_PlayMode
{
    public class AssertMonoBehaviour : MonoBehaviour
    {
        public Func<double> TimeProvider;

        List<Action<double>> _onFixedUpdate = new();
        List<Action<double>> _onUpdate = new();

        private bool _enabled = false;

        public void OnFixedUpdate( Action<double> action )
        {
            _onFixedUpdate.Add( action );
        }
        public void OnUpdate( Action<double> action )
        {
            _onUpdate.Add( action );
        }

        public void Enable()
        {
            _enabled = true;
        }
        public void Disable()
        {
            _enabled = false;
        }

        private void Awake()
        {
            if( TimeProvider == null )
            {
                TimeProvider = () => UnityEngine.Time.time;
            }
        }

        private void FixedUpdate()
        {
            if( !_enabled || _onFixedUpdate == null )
                return;

            double time = TimeProvider();
            foreach( var action in _onFixedUpdate )
            {
                action?.Invoke( time );
            }
        }

        private void Update()
        {
            if( !_enabled || _onUpdate == null )
                return;

            double time = TimeProvider();
            foreach( var action in _onUpdate )
            {
                action?.Invoke( time );
            }
        }
    }
}