using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// A type of mapping that operates on a compound (non-primitive) type and its constituent members.
    /// </summary>
    /// <typeparam name="TSource">The type being mapped.</typeparam>
    public sealed class GameObjectSerializationMapping : SerializationMapping
    {
        private List<MemberBase<GameObject>> _members = new();
        private bool _objectHasBeenInstantiated;

        int _startIndex;
        Dictionary<int, RetryEntry<object>> _retryMembers;
        Func<SerializedData, ILoader, GameObject> _rawFactory;
        bool _wasActive = false;

        bool _wasFailureNoRetry = false;
        public string IsActiveKey = "is_active";

        public GameObjectSerializationMapping()
        {
        }

        private GameObjectSerializationMapping( GameObjectSerializationMapping copy )
        {
            this.Context = copy.Context;
            this._members = copy._members;
            this._rawFactory = copy._rawFactory;
        }

        public override SerializationMapping GetInstance()
        {
            return new GameObjectSerializationMapping( this );
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

            GameObject sourceObj = (GameObject)(object)obj;

            if( data == null )
            {
                _wasActive = sourceObj.activeSelf;

                data = new SerializedObject();

                data[KeyNames.TYPE] = obj.GetType().SerializeType();
                data[KeyNames.ID] = s.RefMap.GetID( sourceObj ).SerializeGuid();
                data[IsActiveKey] = _wasActive;
            }
            sourceObj.SetActive( false );

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

                    MemberBase<GameObject> member = this._members[i];

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
                MemberBase<GameObject> member = this._members[i];

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
            {
                if( _wasActive )
                {
                    sourceObj.SetActive( true );
                }
                result |= SerializationResult.Finished;
            }
            if( result.HasFlag( SerializationResult.Finished ) && result.HasFlag( SerializationResult.HasFailures ) )
                result |= SerializationResult.Failed;

            return result;
        }

        public override SerializationResult Load<TMember>( ref TMember obj, SerializedData data, ILoader l, bool populate )
        {
            if( data == null )
            {
                return SerializationResult.Finished;
            }

            GameObject sourceObj = (obj == null) ? default : (GameObject)(object)obj;

            // obj can be null here, this is normal.

            // Instantiate the object that contains the members ('parent'), if available.
            // It stores when the factory is invoked instead of checking for null,
            //   because structs are never null, but they may be immutable.
            if( populate )
            {
                _objectHasBeenInstantiated = true;
            }
            else if( !_objectHasBeenInstantiated )
            {
                sourceObj = Instantiate( data, l );
                _objectHasBeenInstantiated = true;
            }
            sourceObj.SetActive( false );

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

                    MemberBase<GameObject> member = this._members[i];

                    SerializationResult memberResult = member.LoadRetry( ref entry.value, entry.mapping, data, l );

                    // Store the member for later in case the object doesn't exist yet.
                    if( _objectHasBeenInstantiated )
                    {
                        if( !memberResult.HasFlag( SerializationResult.Finished ) && !memberResult.HasFlag( SerializationResult.Paused ) )
                        {
                            member.Set( ref sourceObj, entry.value );
                        }
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
                MemberBase<GameObject> member = this._members[i];

                // INFO: This will store the value of the loaded object in the source object if it is instantiated, and the result was successful.
                SerializationResult memberResult = member.Load( ref sourceObj, _objectHasBeenInstantiated, data, l, out var mapping, out var memberObj );

                // Store the member for later in case the object doesn't exist yet.

                if( _objectHasBeenInstantiated )
                {
                    if( !memberResult.HasFlag( SerializationResult.Finished ) && !memberResult.HasFlag( SerializationResult.Paused ) )
                    {
                        member.Set( ref sourceObj, memberObj );
                    }
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
            {
                if( data.TryGetValue( IsActiveKey, out var isActive ) )
                {
                    sourceObj.SetActive( (bool)isActive );
                }
                result |= SerializationResult.Finished;
            }
            if( result.HasFlag( SerializationResult.Finished ) && result.HasFlag( SerializationResult.HasFailures ) )
                result |= SerializationResult.Failed;
            return result;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        GameObject Instantiate( SerializedData data, ILoader l )
        {
            return (GameObject)_rawFactory.Invoke( data, l );
        }

        //

        private void IncludeMember( MemberBase<GameObject> member )
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

        public GameObjectSerializationMapping WithMember<TMember>( string serializedName, Expression<Func<GameObject, TMember>> member )
        {
            IncludeMember( new Member<GameObject, TMember>( serializedName, ObjectContext.Default, member ) );
            return this;
        }

        public GameObjectSerializationMapping WithMember<TMember>( string serializedName, Getter<GameObject, TMember> getter, Setter<GameObject, TMember> setter )
        {
            IncludeMember( new Member<GameObject, TMember>( serializedName, ObjectContext.Default, getter, setter ) );
            return this;
        }

        //

        public GameObjectSerializationMapping WithRawFactory( Func<SerializedData, ILoader, GameObject> customFactory )
        {
            this._rawFactory = customFactory;
            return this;
        }
    }
}