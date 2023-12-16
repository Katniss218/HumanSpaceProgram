using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Input
{/// <summary>
 /// Contains an ordered list of actions that are possible to trigger for some defined user input action.
 /// </summary>
    public class HierarchicalInputChannel
    {
        List<(int priority, Func<bool> func)> _registeredActions = new List<(int priority, Func<bool> func)>();
        bool _isStale;

        /// <remarks>
        /// The input actions are invoked according to their priorities (highest first) until an input action returns true.
        /// </remarks>
        public void AddAction( int priority, Func<bool> action )
        {
            _registeredActions.Add( (priority, action) );
            _isStale = true;
        }

        /// <remarks>
        /// The input actions are invoked according to their priorities (highest first) until an input action returns true.
        /// </remarks>
        public void AddActions( IEnumerable<(int priority, Func<bool> action)> f )
        {
            _registeredActions.AddRange( f );
            _isStale = true;
        }

        public void RemoveAction( Func<bool> action )
        {
            _registeredActions.Remove( _registeredActions.FirstOrDefault( v => v.func == action ) );
            _isStale = true;
        }

        public void RemoveActions( IEnumerable<Func<bool>> action )
        {
            _registeredActions.RemoveAll( v => action.ToArray().Contains( v.func ) );
            _isStale = true;
        }

        /// <summary>
        /// Invokes the input channel.
        /// </summary>
        /// <remarks>
        /// The input actions are invoked according to their priorities (highest first) until an input action returns true.
        /// </remarks>
        public void Invoke()
        {
            if( _isStale )
            {
                _registeredActions.Sort( ( lhs, rhs ) => -lhs.priority.CompareTo( rhs.priority ) ); // Highest priority first.
                _isStale = false;
            }
            foreach( var action in _registeredActions )
            {
                if( action.func.Invoke() )
                {
                    return;
                }
            }
        }
    }
}