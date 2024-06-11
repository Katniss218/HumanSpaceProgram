using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine.Profiling;
using UnityEngine.UIElements;

namespace UnityEngine
{
    /// <summary>
    /// Maps a type to a value. Allows hierarchical searching.
    /// </summary>
    public class TypeMap<TContext, T>
    {
        private readonly Dictionary<(TContext, Type), T> _map = new(); // pass in the target type, and creation data.

        public TypeMap()
        {

        }

        public TypeMap( T defaultValue )
        {
            _map[(default( TContext ), typeof( object ))] = defaultValue;
        }

        public bool TryGet( TContext context, Type type, out T value )
        {
            if( type == null )
            {
                throw new ArgumentNullException( nameof( type ) );
            }
            if( type.IsInterface )
            {
                throw new ArgumentException( $"The type to check can't be an interface.", nameof( type ) );
            }

            return _map.TryGetValue( (context, type), out value );
        }


        /// <summary>
        /// Gets the value for the given type (if exists), or default.
        /// </summary>
        /// <param name="type">The type to match.</param>
        /// <returns>The value corresponding to the given type.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public T GetOrDefault( TContext context, Type type )
        {
            if( type == null )
            {
                throw new ArgumentNullException( nameof( type ) );
            }
            if( type.IsInterface )
            {
                throw new ArgumentException( $"The type to check can't be an interface.", nameof( type ) );
            }

            return _map.TryGetValue( (context, type), out var value ) ? value : default;
        }

        public bool TryGetClosest( TContext context, Type type, out T value )
        {
            if( type == null )
            {
                throw new ArgumentNullException( nameof( type ) );
            }
            if( type.IsInterface ) // IsInterface is about half the runtime of the guards, and the guards are about half of the entire runtime of this method.
            {
                throw new ArgumentException( $"The type to check can't be an interface.", nameof( type ) );
            }

            if( _map.Count == 0 )
            {
                value = default;
                return false;
            }

            Type currentTypeToCheck = type;
            bool setLater = false;

            while( !_map.TryGetValue( (context, currentTypeToCheck), out value ) )
            {
                if( currentTypeToCheck.IsGenericType && currentTypeToCheck.IsConstructedGenericType )
                {
                    if( _map.TryGetValue( (context, currentTypeToCheck.GetGenericTypeDefinition()), out value ) )
                    {
                        return true;
                    }
                }

                currentTypeToCheck = currentTypeToCheck.BaseType;
                if( currentTypeToCheck == null )
                {
                    _map[(context, type)] = value;
                    return false;
                }

                setLater = true;
            }
            if( setLater )
            {
                _map[(context, type)] = value;
            }

            return true;
        }

        public T GetClosestOrDefault( TContext context, Type type )
        {
            if( type == null )
            {
                throw new ArgumentNullException( nameof( type ) );
            }
            if( type.IsInterface )
            {
                throw new ArgumentException( $"The type to check can't be an interface.", nameof( type ) );
            }

            if( _map.Count == 0 )
            {
                return default;
            }

            Type currentTypeToCheck = type;
            T value;
            bool setLater = false;

            while( !_map.TryGetValue( (context, currentTypeToCheck), out value ) )
            {
                if( currentTypeToCheck.IsGenericType && currentTypeToCheck.IsConstructedGenericType )
                {
                    if( _map.TryGetValue( (context, currentTypeToCheck.GetGenericTypeDefinition()), out value ) )
                    {
                        return value;
                    }
                }

                currentTypeToCheck = currentTypeToCheck.BaseType;
                if( currentTypeToCheck == null )
                {
                    return default;
                }

                setLater = true;
            }

            if( setLater )
            {
                _map[(context, type)] = value;
            }

            return value;
        }

        /// <summary>
        /// Sets the value for the corresponding type.
        /// </summary>
        /// <typeparam name="TType">The type to set the value for.</typeparam>
        /// <param name="value">The value to set.</param>
        public void Set<TType>( TContext context, T value )
        {
            _map[(context, typeof(TType))] = value;
        }

        /// <summary>
        /// Sets the value for the corresponding type.
        /// </summary>
        /// <param name="type">The type to set the value for.</param>
        /// <param name="value">The value to set.</param>
        public void Set( TContext context, Type type, T value )
        {
            _map[(context, type)] = value;
        }

        public void Clear()
        {
            _map.Clear();
        }
    }
}