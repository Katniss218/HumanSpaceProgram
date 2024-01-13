using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Input
{
    /// <summary>
    /// Contains an ordered list of actions that may execute, if they're not blocked by a higher priority action.
    /// </summary>
    public class HierarchicalActionChannel
    {
#warning TODO - pass input state to the channels directly
        List<(int priority, Func<bool> func)> _actions = new List<(int priority, Func<bool> func)>();

        bool _isStale;

        /// <summary>
        /// Adds an input action with the given priority.
        /// </summary>
        /// <remarks>
        /// The input actions are invoked according to their priorities (highest first) until an input action returns true.
        /// </remarks>
        public void AddAction( int priority, Func<bool> action )
        {
            _actions.Add( (priority, action) );
            _isStale = true;
        }

        /// <summary>
        /// Adds input actions with given priorities.
        /// </summary>
        /// <remarks>
        /// The input actions are invoked according to their priorities (highest first) until an input action returns true.
        /// </remarks>
        public void AddActions( IEnumerable<(int priority, Func<bool> action)> f )
        {
            _actions.AddRange( f );
            _isStale = true;
        }

        /// <summary>
        /// Removes a given input action.
        /// </summary>
        public void RemoveAction( Func<bool> action )
        {
            _actions.Remove( _actions.FirstOrDefault( v => v.func == action ) );
            _isStale = true;
        }

        /// <summary>
        /// Removes given input actions.
        /// </summary>
        public void RemoveActions( IEnumerable<Func<bool>> action )
        {
            _actions.RemoveAll( v => action.ToArray().Contains( v.func ) );
            _isStale = true;
        }

        /// <summary>
        /// Returns a new <see cref="HierarchicalActionChannel"/> containing the combined actions of all the parameter channels.
        /// </summary>
        public static HierarchicalActionChannel Combine( params HierarchicalActionChannel[] channels )
        {
            return Combine( (IEnumerable<HierarchicalActionChannel>)channels );
        }

        /// <summary>
        /// Returns a new <see cref="HierarchicalActionChannel"/> containing the combined actions of all the parameter channels.
        /// </summary>
        public static HierarchicalActionChannel Combine( IEnumerable<HierarchicalActionChannel> channels )
        {
            HierarchicalActionChannel combinedChannel = new HierarchicalActionChannel();
            foreach( var channel in channels )
            {
                combinedChannel.AddActions( channel._actions );
            }
            return combinedChannel;
        }

        void FixStaleness()
        {
            _actions.Sort( ( lhs, rhs ) => -lhs.priority.CompareTo( rhs.priority ) );
            _isStale = false;
        }

        /// <summary>
        /// Sequentially invokes the registered actions according to their priorities (highest first) until one action returns <see cref="true"/> or all actions are invoked.
        /// </summary>
        public void Invoke()
        {
            if( _isStale )
            {
                FixStaleness();
            }

            foreach( var action in _actions )
            {
                if( action.func.Invoke() )
                {
                    return;
                }
            }
        }
    }
}