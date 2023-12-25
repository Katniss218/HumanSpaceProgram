using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine
{
    /// <summary>
    /// Maps each transform in a hierarchy to its nearest ancestor component of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the component to map the transforms to.</typeparam>
    public sealed class AncestralMap<T> where T : Component
    {
        Dictionary<T, List<Transform>> _bound;
        List<Transform> _unbound;

        public int KeyCount => _unbound.Any() ? _bound.Count + 1 : _bound.Count;

        private AncestralMap( Dictionary<T, List<Transform>> bound, List<Transform> unbound )
        {
            this._bound = bound;
            this._unbound = unbound;
        }

        public IEnumerable<KeyValuePair<T, List<Transform>>> AsEnumerable()
        {
            if( _unbound.Any() )
                return ((IEnumerable<KeyValuePair<T, List<Transform>>>)_bound).Prepend( new KeyValuePair<T, List<Transform>>( null, _unbound ) );

            return _bound;
        }

        public bool TryGetValue( T key, out List<Transform> value )
        {
            if( key == null )
            {
                value = _unbound;
                return _unbound.Count > 0;
            }

            return _bound.TryGetValue( key, out value );
        }

        /// <summary>
        /// This returns a map that maps each T component in the tree, starting at root, to the descendants that belong to it. <br />
        /// Each descendant belongs to its closest ancestor that has the T component. <br />
        /// Descendants that have the T component are mapped to their own component.
        /// </summary>
        public static AncestralMap<T> Create( Transform root )
        {
            T rootsPart = root.GetComponent<T>();

            Dictionary<T, List<Transform>> map = new Dictionary<T, List<Transform>>();
            List<Transform> unbound = new List<Transform>();

            Stack<(Transform parent, T parentPart)> stack = new Stack<(Transform, T)>();

            stack.Push( (root, rootsPart) ); // Initial entry with null parentPart

            while( stack.Count > 0 )
            {
                (Transform current, T parentPart) = stack.Pop();

                T currentPart = current.GetComponent<T>();
                if( currentPart == null )
                    currentPart = parentPart;

                if( currentPart == null )
                {
                    unbound.Add( current );
                }
                else
                {
                    if( map.TryGetValue( currentPart, out var list ) )
                    {
                        list.Add( current );
                    }
                    else
                    {
                        map.Add( currentPart, new List<Transform>() { current } );
                    }
                }

                foreach( Transform child in current )
                {
                    stack.Push( (child, currentPart) );
                }
            }

            return new AncestralMap<T>( map, unbound );
        }
    }
}
