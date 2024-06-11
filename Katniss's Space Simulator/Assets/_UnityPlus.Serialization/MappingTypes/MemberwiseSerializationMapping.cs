using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public interface IInstantiableSerializationMapping
    {
        Func<SerializedData, ILoader, object> OnInstantiate { get; }
    }

    /// <summary>
    /// Creates a <see cref="SerializedObject"/> from the child mappings.
    /// </summary>
    /// <typeparam name="TSource">The type of the object being mapped.</typeparam>
    public class MemberwiseSerializationMapping<TSource> : SerializationMapping, IInstantiableSerializationMapping, IEnumerable<(string, MemberBase<TSource>)>
    {
#warning TODO - allow members to keep static values instead of saving them (i.e. force isKinematic to true on every deserialization).

        private readonly List<(string, MemberBase<TSource>)> _items = new();
        public Func<SerializedData, ILoader, object> OnInstantiate { get; private set; } = null;

        public override SerializationStyle SerializationStyle => SerializationStyle.NonPrimitive;

        public MemberwiseSerializationMapping()
        {

        }

        /// <summary>
        /// Makes the deserialization use a custom factory method instead of <see cref="Activator.CreateInstance{T}()"/>.
        /// </summary>
        /// <remarks>
        /// The factory is only needed to create an instance, not to set its internal state. The state should be set using the members.
        /// </remarks>
        /// <param name="customFactory">The method used to create an instance of <typeparamref name="TSource"/> from its serialized representation.</param>
        public MemberwiseSerializationMapping<TSource> WithFactory( Func<SerializedData, ILoader, object> customFactory )
        {
            this.OnInstantiate = customFactory;
            return this;
        }

        /// <summary>
        /// Makes this type include the members of the specified base type in its serialization.
        /// </summary>
        public MemberwiseSerializationMapping<TSource> IncludeMembers<TSourceBase>() where TSourceBase : class
        {
            Type baseType = typeof( TSourceBase );
            if( !baseType.IsAssignableFrom( typeof( TSource ) ) )
            {
                Debug.LogWarning( $"Tried to include members of `{baseType.FullName}` into `{typeof( TSource ).FullName}`, which is not derived from `{baseType.FullName}`." );
                return this;
            }

#warning TODO - do this by default (somehow), without passing the IncludeMembers<TSourceBase> type parameter for every passthroughmember.
            SerializationMapping mapping = SerializationMappingRegistry.GetMappingOrEmpty( this.context, baseType );

            if( ReferenceEquals( mapping, this ) ) // mapping for `this` is a cached mapping of base type.
                return this;

            if( mapping is MemberwiseSerializationMapping<TSourceBase> baseMapping )
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
        /*
        /// <summary>
        /// Makes this type include the members of the specified base type in its serialization.
        /// </summary>
        private MemberwiseSerializationMapping<TSource> IncludeBaseMembersRecursive()
        {
            Type baseType = typeof( TSource ).BaseType;
            if( baseType == null )
                return this;

            SerializationMapping mapping = SerializationMappingRegistry.GetMappingOrEmpty( this.context, baseType );

            if( ReferenceEquals( mapping, this ) ) // mapping for `this` is a cached mapping of base type.
                return this;

            if( mapping is MemberwiseSerializationMapping<TSourceBase> baseMapping )
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
        */
        /// <summary>
        /// Makes the deserialization use the factory of the nearest base type of <typeparamref name="TSource"/>.
        /// </summary>
        public MemberwiseSerializationMapping<TSource> UseBaseTypeFactory()
        {
            do
            {
                Type baseType = typeof( TSource ).BaseType;
                if( baseType == null )
                    return this;

                SerializationMapping mapping = SerializationMappingRegistry.GetMappingOrEmpty( this.context, baseType );

                if( mapping is IInstantiableSerializationMapping m )
                {
                    this.OnInstantiate = m.OnInstantiate;
                    return this;
                }

            } while( this.OnInstantiate == null );

            return this;
        }

        public void Add( (string, MemberBase<TSource>) item )
        {
            if( item.Item1 == null )
                throw new Exception( $"The member name can't be null" );

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

        public override SerializedData Save( object obj, ISaver s )
        {
            SerializedObject root = new SerializedObject();

            TSource sourceObj = (TSource)obj;

            root[KeyNames.ID] = s.RefMap.GetID( sourceObj ).SerializeGuid();
            root[KeyNames.TYPE] = obj.GetType().SerializeType();

            foreach( var item in _items )
            {
                SerializedData data = item.Item2.Save( sourceObj, s );
                root[item.Item1] = data;
            }

            return root;
        }

        public override object Instantiate( SerializedData data, ILoader l )
        {
            TSource obj;
            if( OnInstantiate == null )
            {
                obj = Activator.CreateInstance<TSource>();
                if( data.TryGetValue( KeyNames.ID, out var id ) )
                {
                    l.RefMap.SetObj( id.DeserializeGuid(), obj );
                }
            }
            else
            {
                obj = (TSource)OnInstantiate.Invoke( data, l );
            }

            return obj;
        }

        public override void Load( ref object obj, SerializedData data, ILoader l )
        {
            TSource obj2 = (TSource)obj;
            foreach( var item in _items )
            {
                if( data.TryGetValue( item.Item1, out var memberData ) )
                {
                    item.Item2.Load( ref obj2, memberData, l );
                }
            }
            obj = obj2;
        }

        public override void LoadReferences( ref object obj, SerializedData data, ILoader l )
        {
            var objM = (TSource)obj;

            foreach( var item in _items )
            {
                if( data.TryGetValue( item.Item1, out var memberData ) )
                {
                    item.Item2.LoadReferences( ref objM, memberData, l );
                }
            }

            obj = objM;
        }
    }
}