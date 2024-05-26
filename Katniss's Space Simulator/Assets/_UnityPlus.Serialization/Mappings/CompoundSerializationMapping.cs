using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public interface ISerializationMappingWithCustomFactory
    {
        Func<SerializedData, IForwardReferenceMap, object> CustomFactory { get; }
    }

    /// <summary>
    /// Creates a <see cref="SerializedObject"/> from the child mappings.
    /// </summary>
    /// <typeparam name="TSource">The type of the object being mapped.</typeparam>
    public class CompoundSerializationMapping<TSource> : SerializationMapping, IEnumerable<(string, MemberBase<TSource>)>, ISerializationMappingWithCustomFactory
    {
        private readonly List<(string, MemberBase<TSource>)> _items = new();
        public Func<SerializedData, IForwardReferenceMap, object> CustomFactory { get; private set; } = null;

        public override SerializationStyle SerializationStyle => SerializationStyle.NonPrimitive;

        public CompoundSerializationMapping()
        {

        }

        /// <summary>
        /// Makes the deserialization use a custom factory method instead of <see cref="Activator.CreateInstance{T}()"/>.
        /// </summary>
        /// <remarks>
        /// The factory is only needed to create an instance, not to set its internal state. The state should be set using the members.
        /// </remarks>
        /// <param name="customFactory">The method used to create an instance of <typeparamref name="TSource"/> from its serialized representation.</param>
        public CompoundSerializationMapping<TSource> WithFactory( Func<SerializedData, IForwardReferenceMap, object> customFactory )
        {
            this.CustomFactory = customFactory;
            return this;
        }

        /// <summary>
        /// Makes this type include the members of the specified base type in its serialization.
        /// </summary>
        public CompoundSerializationMapping<TSource> IncludeMembers<TSourceBase>() where TSourceBase : class
        {
            Type baseType = typeof( TSourceBase );
            if( !baseType.IsAssignableFrom( typeof( TSource ) ) )
            {
                Debug.LogWarning( $"Tried to include members of `{baseType.FullName}` into `{typeof( TSource ).FullName}`, which is not derived from `{baseType.FullName}`." );
                return this;
            }

#warning TODO - do this by default (somehow), without passing the IncludeMembers<TSourceBase> type parameter for every passthroughmember.
            SerializationMapping mapping = SerializationMappingRegistry.GetMappingOrEmpty( baseType );

            if( ReferenceEquals( mapping, this ) ) // mapping for `this` is a cached mapping of base type.
                return this;

            if( mapping is CompoundSerializationMapping<TSourceBase> baseMapping )
            {
                foreach( var item in baseMapping._items )
                {
                    var member = item.Item2;

                    MemberBase<TSource> m = PassthroughMember<TSource, TSourceBase>.Create( member );

                    this._items.Add( (item.Item1, m) );
                }
            }

            return this;
        }

        /// <summary>
        /// Makes the deserialization use the factory of the nearest base type of <typeparamref name="TSource"/>.
        /// </summary>
        public CompoundSerializationMapping<TSource> UseBaseTypeFactory()
        {
            do
            {
                Type baseType = typeof( TSource ).BaseType;
                if( baseType == null )
                    return this;

                SerializationMapping mapping = SerializationMappingRegistry.GetMappingOrEmpty( baseType );

                if( mapping is ISerializationMappingWithCustomFactory m )
                {
                    this.CustomFactory = m.CustomFactory;
                    return this;
                }

            } while( this.CustomFactory == null );

            return this;
        }

        public void Add( (string, MemberBase<TSource>) item )
        {
            _items.Add( item );
        }

        public IEnumerator<(string, MemberBase<TSource>)> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public override SerializedData Save( object obj, IReverseReferenceMap s )
        {
            SerializedObject root = new SerializedObject();

            TSource sourceObj = (TSource)obj;

            root[KeyNames.ID] = s.GetID( sourceObj ).SerializeGuid();
            root[KeyNames.TYPE] = obj.GetType().SerializeType();

            foreach( var item in _items )
            {
                if( item.Item2 is IMappedMember<TSource> member )
                {
                    SerializedData data = member.Save( sourceObj, s );
                    root[item.Item1] = data;
                }
                else if( item.Item2 is IMappedReferenceMember<TSource> memberRef )
                {
                    SerializedData data = memberRef.Save( sourceObj, s );
                    root[item.Item1] = data;
                }
            }

            return root;
        }

        public override object Instantiate( SerializedData data, IForwardReferenceMap l )
        {
            TSource obj;
            if( CustomFactory == null )
            {
                obj = Activator.CreateInstance<TSource>();
                if( data.TryGetValue( KeyNames.ID, out var id ) )
                {
                    l.SetObj( id.DeserializeGuid(), obj );
                }
            }
            else
            {
                obj = (TSource)CustomFactory.Invoke( data, l );
            }

            return obj;
        }

        public override void Load( ref object obj, SerializedData data, IForwardReferenceMap l )
        {
            TSource obj2 = (TSource)obj;
            foreach( var item in _items )
            {
                if( item.Item2 is IMappedMember<TSource> member )
                {
                    if( data.TryGetValue( item.Item1, out var memberData ) )
                    {
                        member.Load( ref obj2, memberData, l );
                    }
                }
            }
            obj = obj2;
        }

        public override void LoadReferences( ref object obj, SerializedData data, IForwardReferenceMap l )
        {
            var objM = (TSource)obj;

            foreach( var item in _items )
            {
                if( item.Item2 is IMappedReferenceMember<TSource> member )
                {
                    if( data.TryGetValue( item.Item1, out var memberData ) )
                    {
                        member.LoadReferences( ref objM, memberData, l );
                    }
                }
            }

            obj = objM;
        }
    }
}