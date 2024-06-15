using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Creates a <see cref="SerializedObject"/> from the child mappings.
    /// </summary>
    /// <typeparam name="TSource">The type of the object being mapped.</typeparam>
    public sealed class MemberwiseSerializationMapping<TSource> : SerializationMapping, IInstantiableSerializationMapping, IEnumerable<(string, MemberBase<TSource>)>
    {
#warning TODO - allow members to keep static values instead of saving them (i.e. force isKinematic to true on every deserialization).

        private readonly List<(string, MemberBase<TSource>)> _items = new();
        public Func<SerializedData, ILoader, object> OnInstantiate { get; private set; } = null;

        public override SerializationStyle SerializationStyle => SerializationStyle.NonPrimitive;

        public MemberwiseSerializationMapping()
        {
            //
            UseBaseTypeFactoryRecursive();
            IncludeBaseMembersRecursive();
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
        private MemberwiseSerializationMapping<TSource> IncludeBaseMembersRecursive()
        {
            Type baseType = typeof( TSource ).BaseType;
            if( baseType == null )
                return this;

            SerializationMapping mapping = SerializationMappingRegistry.GetMappingOrEmpty( this.context, baseType );

            if( mapping.SerializationStyle == SerializationStyle.None ) // empty
                return this;

            if( ReferenceEquals( mapping, this ) ) // mapping for `this` is a cached mapping of base type.
                return this;

            Type mappingType = mapping.GetType();

            if( mappingType.IsConstructedGenericType
             && mappingType.GetGenericTypeDefinition() == typeof( MemberwiseSerializationMapping<> ) )
            {
                Type mappedType = mappingType.GetGenericArguments().First();

                if( !mappedType.IsAssignableFrom( baseType ) )
                    return this;

                FieldInfo listField = mappingType.GetField( "_items", BindingFlags.Instance | BindingFlags.NonPublic );

                IList mapping__items = listField.GetValue( mapping ) as IList;

                Type valueTupleType = typeof( ValueTuple<,> )
                    .MakeGenericType( typeof( string ), typeof( MemberBase<> ).MakeGenericType( mappedType ) );

                FieldInfo item1Field = valueTupleType.GetField( "Item1", BindingFlags.Instance | BindingFlags.Public );
                FieldInfo item2Field = valueTupleType.GetField( "Item2", BindingFlags.Instance | BindingFlags.Public );

                foreach( var item in mapping__items )
                {
                    string name = (string)item1Field.GetValue( item );
                    object member = item2Field.GetValue( item );

                    MethodInfo method = typeof( PassthroughMember<,> )
                        .MakeGenericType( typeof( TSource ), mappedType )
                        .GetMethod( "Create", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic );

                    MemberBase<TSource> m = (MemberBase<TSource>)method.Invoke( null, new object[] { member } );

                    this._items.Add( (name, m) );
                }
            }

            return this;
        }

        /// <summary>
        /// Makes the deserialization use the factory of the nearest base type of <typeparamref name="TSource"/>.
        /// </summary>
        private MemberwiseSerializationMapping<TSource> UseBaseTypeFactoryRecursive()
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

        //
        //  Mapping methods:
        //

        public override SerializedData Save( object obj, ISaver s )
        {
            if( obj == null )
                return null;

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
                if( data == null )
                    return null;

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
            if( obj == null )
                return;
            if( data == null )
                return;

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
            if( obj == null )
                return;
            if( data == null )
                return;

            TSource obj2 = (TSource)obj;

            foreach( var item in _items )
            {
                if( data.TryGetValue( item.Item1, out var memberData ) )
                {
                    item.Item2.LoadReferences( ref obj2, memberData, l );
                }
            }

            obj = obj2;
        }
    }
}