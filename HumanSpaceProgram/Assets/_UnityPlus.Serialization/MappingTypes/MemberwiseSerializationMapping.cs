using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace UnityPlus.Serialization
{
    public interface IMemberwiseTemp
    {
        Func<SerializedData, ILoader, object> _rawFactory { get; }
        int _factoryStartMemberIndex { get; }
        int _factoryMemberCount { get; }
        Delegate _untypedFactory { get; }
    }

    /// <summary>
    /// A type of mapping that operates on a compound (non-primitive) type and its constituent members.
    /// </summary>
    /// <typeparam name="TSource">The type being mapped.</typeparam>
    public sealed class MemberwiseSerializationMapping<TSource> : SerializationMapping, IMemberwiseTemp
    {
        private List<MemberBase<TSource>> _members = new();
        private bool _objectHasBeenInstantiated;

        object[] _preFactoryMemberStorage;  // members that need to be stored before the factory can be invoked.
        object[] _factoryMemberStorage;     // members passed in to invoke the factory.
        int _startIndex;
        Dictionary<int, RetryEntry<object>> _retryMembers;

        public Func<SerializedData, ILoader, object> _rawFactory { get; private set; } = null;
        public int _factoryStartMemberIndex { get; private set; }
        public int _factoryMemberCount { get; private set; }
        public Delegate _untypedFactory { get; private set; } = null;

        bool _wasFailureNoRetry = false;

        public MemberwiseSerializationMapping()
        {
            UseBaseTypeFactoryRecursive( null );
            IncludeBaseMembersRecursive();
        }

        /// <param name="baseTypeContext">The serialization context to use when retrieving the members/factories for the base type. USE WITH CARE.</param>
        public MemberwiseSerializationMapping( int baseTypeContext )
        {
            UseBaseTypeFactoryRecursive( baseTypeContext );
            IncludeBaseMembersRecursive();
        }

        private MemberwiseSerializationMapping( MemberwiseSerializationMapping<TSource> copy )
        {
            this.Context = copy.Context;
            this._members = copy._members;
            this._rawFactory = copy._rawFactory;
            this._factoryStartMemberIndex = copy._factoryStartMemberIndex;
            this._factoryMemberCount = copy._factoryMemberCount;
            this._untypedFactory = copy._untypedFactory;
        }

        public override SerializationMapping GetInstance()
        {
            return new MemberwiseSerializationMapping<TSource>( this );
        }

        /// <summary>
        /// Makes this type include the members of the specified base type in its serialization.
        /// </summary>
        private MemberwiseSerializationMapping<TSource> IncludeBaseMembersRecursive()
        {
            Type baseType = typeof( TSource ).BaseType;
            if( baseType == null )
                return this;

            SerializationMapping mapping = SerializationMappingRegistry.GetMappingOrNull( this.Context, baseType );

            if( mapping == null )
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

                FieldInfo listField = mappingType.GetField( nameof( _members ), BindingFlags.Instance | BindingFlags.NonPublic );

                IList mapping__members = listField.GetValue( mapping ) as IList;

                foreach( var member in mapping__members )
                {
                    // Would be nice to have this be flattened, instead of one layer of passthrough per inheritance level.
                    MethodInfo method = typeof( PassthroughMember<,> )
                        .MakeGenericType( typeof( TSource ), mappedType )
                        .GetMethod( nameof( PassthroughMember<object, object>.Create ), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic );

                    MemberBase<TSource> m = (MemberBase<TSource>)method.Invoke( null, new object[] { member } );

                    this._members.Add( m );
                }
            }

            return this;
        }

        /// <summary>
        /// Makes the deserialization use the factory of the nearest base type of <typeparamref name="TSource"/>.
        /// </summary>
        private MemberwiseSerializationMapping<TSource> UseBaseTypeFactoryRecursive( int? baseTypeContext )
        {
            Type baseType = typeof( TSource ).BaseType;
            if( baseType == null )
                return this;

            SerializationMapping mapping = SerializationMappingRegistry.GetMappingOrNull( this.Context, baseType );

            if( mapping == null && baseTypeContext != null )
            {
                mapping = SerializationMappingRegistry.GetMappingOrNull( baseTypeContext.Value, baseType );
            }

            // later on, this entire thing is going to be overriden

            if( mapping is IMemberwiseTemp m )
            {
                this._rawFactory = m._rawFactory;
                this._untypedFactory = m._untypedFactory;
                this._factoryStartMemberIndex = m._factoryStartMemberIndex;
                this._factoryMemberCount = m._factoryMemberCount;
                return this;
            }

            return this;
        }

        //
        //  Mapping methods:
        //

        public override SerializationResult Save<TMember>( TMember obj, ref SerializedData data, ISaver s )
        {
            if( obj == null )
            {
                return SerializationResult.Finished;
            }

            TSource sourceObj = (TSource)(object)obj;

            if( data == null )
            {
                data = new SerializedObject();

                var headerStyle = MappingHelper.IsNonNullEligibleForTypeHeader<TMember>();
                if( headerStyle != ObjectHeaderStyle.None )
                {
                    if( headerStyle.HasFlag( ObjectHeaderStyle.TypeField ) )
                        data[KeyNames.TYPE] = obj.GetType().SerializeType();
                    if( headerStyle.HasFlag( ObjectHeaderStyle.IDField ) )
                        data[KeyNames.ID] = s.RefMap.GetID( sourceObj ).SerializeGuid();
                }
            }

            //
            //      RETRY PREVIOUSLY FAILED MEMBERS
            //

            if( _retryMembers != null )
            {
                List<int> retryMembersThatSucceededThisTime = new();

                foreach( (int i, var entry) in _retryMembers )
                {
                    if( entry.pass == s.CurrentPass )
                        continue;

                    MemberBase<TSource> member = this._members[i];

                    SerializationResult memberResult = member.SaveRetry( entry.value, entry.mapping, data, s );
                    if( memberResult.HasFlag( SerializationResult.Failed ) )
                    {
                        entry.pass = s.CurrentPass;
                    }
                    else if( memberResult.HasFlag( SerializationResult.Finished ) )
                    {
                        retryMembersThatSucceededThisTime.Add( i );
                    }

                    if( s.ShouldPause() )
                    {
                        foreach( var ii in retryMembersThatSucceededThisTime )
                        {
                            _retryMembers.Remove( ii );
                        }
                        return SerializationResult.Paused;
                    }
                }

                foreach( var i in retryMembersThatSucceededThisTime )
                {
                    _retryMembers.Remove( i );
                }
            }

            //
            //      PROCESS THE MEMBERS THAT HAVE NOT FAILED YET.
            //

            for( int i = _startIndex; i < this._members.Count; i++ )
            {
                MemberBase<TSource> member = this._members[i];

                SerializationResult memberResult = member.Save( sourceObj, data, s, out var mapping, out var memberObj );
                if( memberResult.HasFlag( SerializationResult.Finished ) )
                {
                    if( memberResult.HasFlag( SerializationResult.Failed ) )
                        _wasFailureNoRetry = true;

                    _startIndex = i + 1;
                }
                else
                {
                    _retryMembers ??= new();
                    _startIndex = i + 1;

                    if( memberResult.HasFlag( SerializationResult.Paused ) )
                        _retryMembers.Add( i, new RetryEntry<object>( memberObj, mapping, -1 ) );
                    else
                        _retryMembers.Add( i, new RetryEntry<object>( memberObj, mapping, s.CurrentPass ) );
                }

                if( s.ShouldPause() )
                {
                    return SerializationResult.Paused;
                }
            }

            SerializationResult result = SerializationResult.NoChange;
            if( _wasFailureNoRetry || _retryMembers != null && _retryMembers.Count != 0 )
                result |= SerializationResult.HasFailures;
            if( _retryMembers == null || _retryMembers.Count == 0 )
                result |= SerializationResult.Finished;

            if( result.HasFlag( SerializationResult.Finished ) && result.HasFlag( SerializationResult.HasFailures ) )
                result |= SerializationResult.Failed;

            return result;
        }

        bool FactoryMembersReadyForInstantiation( List<int> retryIndicesToIgnore = null )
        {
            if( _factoryMemberCount == 0 )
                return true;

            int lastFactoryMemberIndex = (_factoryStartMemberIndex + _factoryMemberCount);

            if( _startIndex < lastFactoryMemberIndex )
            {
                return false;
            }

            if( _retryMembers != null )
            {
                foreach( var i in _retryMembers.Keys )
                {
                    if( i < lastFactoryMemberIndex )
                    {
                        if( retryIndicesToIgnore != null && retryIndicesToIgnore.Contains( i ) )
                            continue;

                        return false;
                    }
                }
            }

            return true;
        }

        public override SerializationResult Load<TMember>( ref TMember obj, SerializedData data, ILoader l, bool populate )
        {
            if( data == null )
            {
                return SerializationResult.Finished;
            }
            if( data is not SerializedObject )
            {
                return SerializationResult.Finished;
            }

            TSource sourceObj = (obj == null) ? default : (TSource)(object)obj;

            // obj can be null here, this is normal.

            // Instantiate the object that contains the members ('parent'), if available.
            // It stores when the factory is invoked instead of checking for null,
            //   because structs are never null, but they may be immutable.
            if( populate )
            {
                _objectHasBeenInstantiated = true;
            }
            else
            {
                if( !_objectHasBeenInstantiated && FactoryMembersReadyForInstantiation() )
                {
                    sourceObj = Instantiate( data, l );
                    _objectHasBeenInstantiated = true;
                }

                if( _factoryStartMemberIndex != 0 )
                {
                    _preFactoryMemberStorage ??= new object[_factoryStartMemberIndex];
                }
                if( _factoryMemberCount != 0 )
                {
                    _factoryMemberStorage ??= new object[_factoryMemberCount];
                }
            }

            //
            //      RETRY PREVIOUSLY FAILED MEMBERS
            //

            if( _retryMembers != null )
            {
                List<int> retryMembersThatSucceededThisTime = new();

                foreach( (int i, var entry) in _retryMembers )
                {
                    if( entry.pass == l.CurrentPass )
                        continue;

                    MemberBase<TSource> member = this._members[i];

                    SerializationResult memberResult = member.LoadRetry( ref entry.value, entry.mapping, data, l );

                    // Store the member for later in case the object doesn't exist yet.
                    if( _objectHasBeenInstantiated )
                    {
#warning TODO - remember load and loadretry are different in that loadretry never assigns. It honestly should always assign inside I think.
                        if( !memberResult.HasFlag( SerializationResult.Finished ) && !memberResult.HasFlag( SerializationResult.Paused ) )
                        {
                            member.Set( ref sourceObj, entry.value );
                        }
                    }
                    else if( !populate )
                    {
                        AssignMemberToTempStorage( i, entry.value );
                    }

                    if( memberResult.HasFlag( SerializationResult.Failed ) )
                    {
                        entry.pass = l.CurrentPass;
                    }
                    else if( memberResult.HasFlag( SerializationResult.Finished ) )
                    {
                        retryMembersThatSucceededThisTime.Add( i );
                        member.Set( ref sourceObj, entry.value ); // Due to LoadRetry not setting the member, we set it here
                    }

                    // Instantiate the object that contains the members ('parent'), if available.
                    if( !populate && !_objectHasBeenInstantiated && FactoryMembersReadyForInstantiation( retryMembersThatSucceededThisTime ) )
                    {
                        AssignMemberToTempStorage( i, entry.value );

                        sourceObj = Instantiate( data, l );
                        CopyTempStorageMembersToObject( ref sourceObj );
                        _objectHasBeenInstantiated = true;
                    }

                    if( l.ShouldPause() )
                    {
                        foreach( var ii in retryMembersThatSucceededThisTime )
                        {
                            _retryMembers.Remove( ii );
                        }
                        obj = (TMember)(object)sourceObj;
                        return SerializationResult.Paused;
                    }
                }

                foreach( var i in retryMembersThatSucceededThisTime )
                {
                    _retryMembers.Remove( i );
                }
            }

            //
            //      PROCESS THE MEMBERS THAT HAVE NOT FAILED YET.
            //

            for( int i = _startIndex; i < this._members.Count; i++ )
            {
                MemberBase<TSource> member = this._members[i];

                // INFO: This will store the value of the loaded object in the source object if it is instantiated, and the result was successful.
                SerializationResult memberResult = member.Load( ref sourceObj, _objectHasBeenInstantiated, data, l, out var mapping, out var memberObj );

                // Store the member for later in case the object doesn't exist yet.

                if( _objectHasBeenInstantiated )
                {
                    if( !memberResult.HasFlag( SerializationResult.Finished ) && !memberResult.HasFlag( SerializationResult.Paused ) )
                    {
#warning TODO - only assign once finished, we don't know how complex the setter is, and if it has error handling/side effects.
                        member.Set( ref sourceObj, memberObj );
                    }
                }
                else if( !populate )
                {
                    AssignMemberToTempStorage( i, memberObj );
                }

                if( memberResult.HasFlag( SerializationResult.Finished ) )
                {
                    if( memberResult.HasFlag( SerializationResult.Failed ) )
                        _wasFailureNoRetry = true;

                    _startIndex = i + 1;
                }
                else
                {
                    _retryMembers ??= new();
                    _startIndex = i + 1;

                    if( memberResult.HasFlag( SerializationResult.Paused ) )
                        _retryMembers.Add( i, new RetryEntry<object>( memberObj, mapping, -1 ) );
                    else
                        _retryMembers.Add( i, new RetryEntry<object>( memberObj, mapping, l.CurrentPass ) );
                }

                // Instantiate the object that contains the members ('parent'), if available.
                // It stores when the factory is invoked instead of checking for null,
                //   because structs are never null, but they may be immutable.
                if( !populate && !_objectHasBeenInstantiated && FactoryMembersReadyForInstantiation() )
                {
                    AssignMemberToTempStorage( i, memberObj );

                    sourceObj = Instantiate( data, l );
                    CopyTempStorageMembersToObject( ref sourceObj );
                    _objectHasBeenInstantiated = true;
                }

                if( l.ShouldPause() )
                {
                    obj = (TMember)(object)sourceObj;
                    return SerializationResult.Paused;
                }
            }

            obj = (TMember)(object)sourceObj;
            SerializationResult result = SerializationResult.NoChange;
            if( _wasFailureNoRetry || _retryMembers != null && _retryMembers.Count != 0 )
                result |= SerializationResult.HasFailures;
            if( _retryMembers == null || _retryMembers.Count == 0 )
                result |= SerializationResult.Finished;

#warning TODO - this failure is propagated high enough, that a sinple 'handleable' failure (e.g. wrong [] {} in a member that can be nulled) gets turned into a huge deal and stops the gameobjects from deserializing correctly.
            // this should fail, but the gameobjects should be destroyed when failed?

            if( result.HasFlag( SerializationResult.Finished ) && result.HasFlag( SerializationResult.HasFailures ) )
                result |= SerializationResult.Failed;
            return result;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private void CopyTempStorageMembersToObject( ref TSource sourceObj )
        {
            // assign the initial members (if members are readonly this will silently do nothing).
            for( int j = 0; j < _factoryStartMemberIndex; j++ )
            {
                _members[j].Set( ref sourceObj, this._preFactoryMemberStorage[j] );
            }
            for( int j = 0; j < _factoryMemberCount; j++ )
            {
                _members[_factoryStartMemberIndex + j].Set( ref sourceObj, this._factoryMemberStorage[j] );
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private void AssignMemberToTempStorage( int i, object memberObj )
        {
            if( i < _factoryStartMemberIndex )
                _preFactoryMemberStorage[i] = memberObj;
            else
                _factoryMemberStorage[i - _factoryStartMemberIndex] = memberObj;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        TSource Instantiate( SerializedData data, ILoader l )
        {
            TSource obj;
            if( _untypedFactory != null )
            {
                obj = (TSource)_untypedFactory.DynamicInvoke( _factoryMemberStorage );
            }
            else if( _rawFactory != null )
            {
                obj = (TSource)_rawFactory.Invoke( data, l );
            }
            else
            {
                if( data == null )
                    return default;

                obj = Activator.CreateInstance<TSource>();
            }

            if( data.TryGetValue( KeyNames.ID, out var id ) )
            {
                l.RefMap.SetObj( id.DeserializeGuid(), obj );
            }

            return obj;
        }

        //

        private void IncludeMember( MemberBase<TSource> member )
        {
            // If member already exists - replace it, otherwise append as new.
            for( int i = 0; i < _members.Count; i++ )
            {
                if( _members[i].Name == member.Name )
                {
                    this._members[i] = member;
                    return;
                }
            }

            this._members.Add( member );
        }

        private void RemoveMember( string name )
        {
            // If member already exists - replace it, otherwise append as new.
            for( int i = 0; i < _members.Count; i++ )
            {
                if( _members[i].Name == name )
                {
                    _members.RemoveAt( i );
                    return;
                }
            }
        }

        public MemberwiseSerializationMapping<TSource> ExcludeMember( string serializedName )
        {
            RemoveMember( serializedName );
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithMember<TMember>( string serializedName, Expression<Func<TSource, TMember>> member )
        {
            IncludeMember( new Member<TSource, TMember>( serializedName, ObjectContext.Default, member ) );
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithMember<TMember>( string serializedName, int context, Expression<Func<TSource, TMember>> member )
        {
            IncludeMember( new Member<TSource, TMember>( serializedName, context, member ) );
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithMember<TMember>( string serializedName, Getter<TSource, TMember> getter, Setter<TSource, TMember> setter )
        {
            IncludeMember( new Member<TSource, TMember>( serializedName, ObjectContext.Default, getter, setter ) );
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithMember<TMember>( string serializedName, int context, Getter<TSource, TMember> getter, Setter<TSource, TMember> setter )
        {
            IncludeMember( new Member<TSource, TMember>( serializedName, context, getter, setter ) );
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithMember<TMember>( string serializedName, Getter<TSource, TMember> getter, RefSetter<TSource, TMember> setter )
        {
            IncludeMember( new Member<TSource, TMember>( serializedName, ObjectContext.Default, getter, setter ) );
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithMember<TMember>( string serializedName, int context, Getter<TSource, TMember> getter, RefSetter<TSource, TMember> setter )
        {
            IncludeMember( new Member<TSource, TMember>( serializedName, context, getter, setter ) );
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithReadonlyMember<TMember>( string serializedName, Getter<TSource, TMember> getter )
        {
            IncludeMember( new Member<TSource, TMember>( serializedName, ObjectContext.Default, getter ) );
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithReadonlyMember<TMember>( string serializedName, int context, Getter<TSource, TMember> getter )
        {
            IncludeMember( new Member<TSource, TMember>( serializedName, context, getter ) );
            return this;
        }

        //

        public MemberwiseSerializationMapping<TSource> WithRawFactory( Func<SerializedData, ILoader, object> customFactory )
        {
            this._rawFactory = customFactory;
            return this;
        }

        private void SetupFactory( Delegate factory, params Type[] types )
        {
            int start = _members.Count - types.Length;
            if( start < 0 )
                throw new Exception( $"Tried to register a factory with {types.Length} parameters, but there's only {_members.Count} members registered." );

            for( int i = 0; i < types.Length; i++ )
            {
                Type memberXType = _members[start + i].GetType();
                if( memberXType.GetGenericTypeDefinition() == typeof( Member<,> ) )
                {
                    Type memberType = memberXType.GetGenericArguments()[1];
                    if( memberType != types[i] )
                    {
                        throw new ArgumentException( $"Mismatched member type '{memberType.FullName}' vs factory parameter type '{types[i].FullName}'. Factory parameters must match the last (in this case) {types.Length} member types." );
                    }
                }
            }

            _factoryStartMemberIndex = start;
            _factoryMemberCount = types.Length;
            _untypedFactory = factory;
        }

        public MemberwiseSerializationMapping<TSource> WithFactory( Func<object> factory )
        {
            // Here the factory doesn't need checking, as it doesn't use members.
            _untypedFactory = factory;
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithFactory<TMember1>( Func<TMember1, object> factory )
        {
            SetupFactory( factory, typeof( TMember1 ) );
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithFactory<TMember1, TMember2>( Func<TMember1, TMember2, object> factory )
        {
            SetupFactory( factory, typeof( TMember1 ), typeof( TMember2 ) );
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithFactory<TMember1, TMember2, TMember3>( Func<TMember1, TMember2, TMember3, object> factory )
        {
            SetupFactory( factory, typeof( TMember1 ), typeof( TMember2 ), typeof( TMember3 ) );
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithFactory<TMember1, TMember2, TMember3, TMember4>( Func<TMember1, TMember2, TMember3, TMember4, object> factory )
        {
            SetupFactory( factory, typeof( TMember1 ), typeof( TMember2 ), typeof( TMember3 ), typeof( TMember4 ) );
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithFactory<TMember1, TMember2, TMember3, TMember4, TMember5>( Func<TMember1, TMember2, TMember3, TMember4, TMember5, object> factory )
        {
            SetupFactory( factory, typeof( TMember1 ), typeof( TMember2 ), typeof( TMember3 ), typeof( TMember4 ), typeof( TMember5 ) );
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithFactory<TMember1, TMember2, TMember3, TMember4, TMember5, TMember6>( Func<TMember1, TMember2, TMember3, TMember4, TMember5, TMember6, object> factory )
        {
            SetupFactory( factory, typeof( TMember1 ), typeof( TMember2 ), typeof( TMember3 ), typeof( TMember4 ), typeof( TMember5 ), typeof( TMember6 ) );
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithFactory<TMember1, TMember2, TMember3, TMember4, TMember5, TMember6, TMember7>( Func<TMember1, TMember2, TMember3, TMember4, TMember5, TMember6, TMember7, object> factory )
        {
            SetupFactory( factory, typeof( TMember1 ), typeof( TMember2 ), typeof( TMember3 ), typeof( TMember4 ), typeof( TMember5 ), typeof( TMember6 ), typeof( TMember7 ) );
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithFactory<TMember1, TMember2, TMember3, TMember4, TMember5, TMember6, TMember7, TMember8>( Func<TMember1, TMember2, TMember3, TMember4, TMember5, TMember6, TMember7, TMember8, object> factory )
        {
            SetupFactory( factory, typeof( TMember1 ), typeof( TMember2 ), typeof( TMember3 ), typeof( TMember4 ), typeof( TMember5 ), typeof( TMember6 ), typeof( TMember7 ), typeof( TMember8 ) );
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithFactory<TMember1, TMember2, TMember3, TMember4, TMember5, TMember6, TMember7, TMember8, TMember9>( Func<TMember1, TMember2, TMember3, TMember4, TMember5, TMember6, TMember7, TMember8, TMember9, object> factory )
        {
            SetupFactory( factory, typeof( TMember1 ), typeof( TMember2 ), typeof( TMember3 ), typeof( TMember4 ), typeof( TMember5 ), typeof( TMember6 ), typeof( TMember7 ), typeof( TMember8 ), typeof( TMember9 ) );
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithFactory<TMember1, TMember2, TMember3, TMember4, TMember5, TMember6, TMember7, TMember8, TMember9, TMember10>( Func<TMember1, TMember2, TMember3, TMember4, TMember5, TMember6, TMember7, TMember8, TMember9, TMember10, object> factory )
        {
            SetupFactory( factory, typeof( TMember1 ), typeof( TMember2 ), typeof( TMember3 ), typeof( TMember4 ), typeof( TMember5 ), typeof( TMember6 ), typeof( TMember7 ), typeof( TMember8 ), typeof( TMember9 ), typeof( TMember10 ) );
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithFactory<TMember1, TMember2, TMember3, TMember4, TMember5, TMember6, TMember7, TMember8, TMember9, TMember10, TMember11>( Func<TMember1, TMember2, TMember3, TMember4, TMember5, TMember6, TMember7, TMember8, TMember9, TMember10, TMember11, object> factory )
        {
            SetupFactory( factory, typeof( TMember1 ), typeof( TMember2 ), typeof( TMember3 ), typeof( TMember4 ), typeof( TMember5 ), typeof( TMember6 ), typeof( TMember7 ), typeof( TMember8 ), typeof( TMember9 ), typeof( TMember10 ), typeof( TMember11 ) );
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithFactory<TMember1, TMember2, TMember3, TMember4, TMember5, TMember6, TMember7, TMember8, TMember9, TMember10, TMember11, TMember12>( Func<TMember1, TMember2, TMember3, TMember4, TMember5, TMember6, TMember7, TMember8, TMember9, TMember10, TMember11, TMember12, object> factory )
        {
            SetupFactory( factory, typeof( TMember1 ), typeof( TMember2 ), typeof( TMember3 ), typeof( TMember4 ), typeof( TMember5 ), typeof( TMember6 ), typeof( TMember7 ), typeof( TMember8 ), typeof( TMember9 ), typeof( TMember10 ), typeof( TMember11 ), typeof( TMember12 ) );
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithFactory<TMember1, TMember2, TMember3, TMember4, TMember5, TMember6, TMember7, TMember8, TMember9, TMember10, TMember11, TMember12, TMember13>( Func<TMember1, TMember2, TMember3, TMember4, TMember5, TMember6, TMember7, TMember8, TMember9, TMember10, TMember11, TMember12, TMember13, object> factory )
        {
            SetupFactory( factory, typeof( TMember1 ), typeof( TMember2 ), typeof( TMember3 ), typeof( TMember4 ), typeof( TMember5 ), typeof( TMember6 ), typeof( TMember7 ), typeof( TMember8 ), typeof( TMember9 ), typeof( TMember10 ), typeof( TMember11 ), typeof( TMember12 ), typeof( TMember13 ) );
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithFactory<TMember1, TMember2, TMember3, TMember4, TMember5, TMember6, TMember7, TMember8, TMember9, TMember10, TMember11, TMember12, TMember13, TMember14>( Func<TMember1, TMember2, TMember3, TMember4, TMember5, TMember6, TMember7, TMember8, TMember9, TMember10, TMember11, TMember12, TMember13, TMember14, object> factory )
        {
            SetupFactory( factory, typeof( TMember1 ), typeof( TMember2 ), typeof( TMember3 ), typeof( TMember4 ), typeof( TMember5 ), typeof( TMember6 ), typeof( TMember7 ), typeof( TMember8 ), typeof( TMember9 ), typeof( TMember10 ), typeof( TMember11 ), typeof( TMember12 ), typeof( TMember13 ), typeof( TMember14 ) );
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithFactory<TMember1, TMember2, TMember3, TMember4, TMember5, TMember6, TMember7, TMember8, TMember9, TMember10, TMember11, TMember12, TMember13, TMember14, TMember15>( Func<TMember1, TMember2, TMember3, TMember4, TMember5, TMember6, TMember7, TMember8, TMember9, TMember10, TMember11, TMember12, TMember13, TMember14, TMember15, object> factory )
        {
            SetupFactory( factory, typeof( TMember1 ), typeof( TMember2 ), typeof( TMember3 ), typeof( TMember4 ), typeof( TMember5 ), typeof( TMember6 ), typeof( TMember7 ), typeof( TMember8 ), typeof( TMember9 ), typeof( TMember10 ), typeof( TMember11 ), typeof( TMember12 ), typeof( TMember13 ), typeof( TMember14 ), typeof( TMember15 ) );
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithFactory<TMember1, TMember2, TMember3, TMember4, TMember5, TMember6, TMember7, TMember8, TMember9, TMember10, TMember11, TMember12, TMember13, TMember14, TMember15, TMember16>( Func<TMember1, TMember2, TMember3, TMember4, TMember5, TMember6, TMember7, TMember8, TMember9, TMember10, TMember11, TMember12, TMember13, TMember14, TMember15, TMember16, object> factory )
        {
            SetupFactory( factory, typeof( TMember1 ), typeof( TMember2 ), typeof( TMember3 ), typeof( TMember4 ), typeof( TMember5 ), typeof( TMember6 ), typeof( TMember7 ), typeof( TMember8 ), typeof( TMember9 ), typeof( TMember10 ), typeof( TMember11 ), typeof( TMember12 ), typeof( TMember13 ), typeof( TMember14 ), typeof( TMember15 ), typeof( TMember16 ) );
            return this;
        }

        /*public MemberwiseSerializationMapping<TSource> WithFactory<TMember1, TMember2, TMember3>( string member1Name, string member2Name, string member3Name, Func<TMember1, TMember2, TMember3, object> factory )
        {
            throw new NotImplementedException();
        }*/
    }
}