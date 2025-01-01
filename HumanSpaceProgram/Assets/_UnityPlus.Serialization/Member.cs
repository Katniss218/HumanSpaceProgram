using System;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Networking.Types;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Serializes a member of type <typeparamref name="TMember"/>, that belongs to a type <typeparamref name="TSource"/>.
    /// </summary>
    /// <typeparam name="TSource">The type that contains the member.</typeparam>
    /// <typeparam name="TMember">The type of the member (field/property/etc).</typeparam>
    public sealed class Member<TSource, TMember> : MemberBase<TSource>
    {
        private readonly int _context = ObjectContext.Default;

        private readonly Getter<TSource, TMember> _getter;
        private readonly Setter<TSource, TMember> _setter;
        private readonly RefSetter<TSource, TMember> _structSetter;

        private readonly Expression<Func<TSource, TMember>> _memberAccessExpr;

        /// <summary>
        /// Checks if the member serialization represents a simple member access (object.member = value), as opposed to something more complicated.
        /// </summary>
        public bool IsSimpleAccess => _memberAccessExpr != null;

        /// <summary>
        /// Gets the serialization context that this member should use.
        /// </summary>
        public int Context => _context;

        private bool _hasCachedMapping;
#warning TODO - members can only cache mappings if the mapping is primitive.
        private SerializationMapping _cachedMapping;

        private void TryCacheMemberMapping()
        {
            Type type = typeof( TMember );
            if( type.IsValueType || (!type.IsInterface && type.BaseType == null) )
            {
                var mapping1 = SerializationMappingRegistry.GetMappingOrNull( _context, typeof( TMember ) );
                var mapping2 = mapping1.GetInstance();
                if( object.ReferenceEquals( mapping1, mapping2 ) ) // This is needed due to GetInstance and mappings that can hold state (like the dict mapping).
                {
                    _hasCachedMapping = true;
                    _cachedMapping = mapping1;
                }
            }
        }

        public override MemberBase<TSource> Copy()
        {
            return (MemberBase<TSource>)this.MemberwiseClone();
        }

        // expression constructors

        /// <param name="member">Example: `o => o.position`.</param>
        public Member( string name, int context, Expression<Func<TSource, TMember>> member )
        {
            this.Name = name;
            _memberAccessExpr = member;
            _context = context;
            TryCacheMemberMapping();
            _getter = AccessorUtils.CreateGetter( member );

            if( typeof( TSource ).IsValueType )
                _structSetter = AccessorUtils.CreateStructSetter( member );
            else
                _setter = AccessorUtils.CreateSetter( member );
        }

        // custom getter/setter constructors

        public Member( string name, int context, Getter<TSource, TMember> getter, Setter<TSource, TMember> setter )
        {
            if( typeof( TSource ).IsValueType )
                throw new InvalidOperationException( $"Member `{typeof( TSource ).FullName}` This constructor can only be used with a reference type TSource." );

            this.Name = name;
            _memberAccessExpr = null;
            _context = context;
            TryCacheMemberMapping();
            _getter = getter;
            _setter = setter;
        }

        public Member( string name, int context, Getter<TSource, TMember> getter, RefSetter<TSource, TMember> setter )
        {
            if( !typeof( TSource ).IsValueType )
                throw new InvalidOperationException( $"Member `{typeof( TSource ).FullName}` This constructor can only be used with a value type TSource." );

            this.Name = name;
            _memberAccessExpr = null;
            _context = context;
            TryCacheMemberMapping();
            _getter = getter;
            _structSetter = setter;
        }

        public Member( string name, int context, Getter<TSource, TMember> getter )
        {
            this.Name = name;
            _memberAccessExpr = null;
            _context = context;
            TryCacheMemberMapping();
            _getter = getter;
        }

        //
        //  Logic
        //

        public override MappingResult Save( TSource sourceObj, SerializedData sourceData, ISaver s, out SerializationMapping mapping, out object memberObj )
        {
            if( !sourceData.TryGetValue( Name, out var memberData ) )
                memberData = null;

            TMember memberObj2 = _getter.Invoke( sourceObj );
            memberObj = memberObj2;

            mapping = SerializationMappingRegistry.GetMapping<TMember>( _context, memberObj2 );

            MappingResult memberResult = mapping.SafeSave<TMember>( memberObj2, ref memberData, s );
            sourceData[Name] = memberData;

            return memberResult;
        }

        public override MappingResult SaveRetry( object memberObj, SerializationMapping mapping, SerializedData sourceData, ISaver s )
        {
            if( !sourceData.TryGetValue( Name, out var memberData ) )
                memberData = null;

            TMember memberObj2 = (TMember)memberObj;
            MappingResult memberResult = mapping.SafeSave<TMember>( memberObj2, ref memberData, s );

            sourceData[Name] = memberData;

            return memberResult;
        }

        public override MappingResult Load( ref TSource sourceObj, bool isInstantiated, SerializedData sourceData, ILoader l, out SerializationMapping mapping, out object memberObj )
        {
            if( !sourceData.TryGetValue( Name, out SerializedData memberData ) )
                memberData = null;

            if( _hasCachedMapping )             // This caching appears to not do much performance-wise.
            {
                mapping = _cachedMapping;
            }
            else
            {
                Type memberType = MappingHelper.GetSerializedType<TMember>( memberData );
                mapping = SerializationMappingRegistry.GetMapping<TMember>( _context, memberType );
            }

            TMember memberObj2 = default;
            MappingResult memberResult = mapping.SafeLoad<TMember>( ref memberObj2, memberData, l, false );
            if( isInstantiated && memberResult == MappingResult.Finished )
            {
                memberObj = null;
                if( _structSetter != null )
                    _structSetter.Invoke( ref sourceObj, (TMember)memberObj2 );
                else if( _setter != null )
                    _setter.Invoke( sourceObj, (TMember)memberObj2 );
            }
            else
            {
                memberObj = memberObj2;
            }

            return memberResult;
        }

        public override MappingResult LoadRetry( ref object memberObj, SerializationMapping mapping, SerializedData sourceData, ILoader l )
        {
            if( !sourceData.TryGetValue( Name, out SerializedData memberData ) )
                memberData = null;

            TMember memberObj2 = (TMember)memberObj;
            MappingResult memberResult = mapping.SafeLoad<TMember>( ref memberObj2, memberData, l, false );
            memberObj = memberObj2;

            return memberResult;
        }

        public override object Get( ref TSource source )
        {
            return _getter.Invoke( source );
        }

        public override void Set( ref TSource source, object member )
        {
            if( _structSetter != null )
                _structSetter.Invoke( ref source, (TMember)member );
            else if( _setter != null )
                _setter.Invoke( source, (TMember)member );
        }
    }
}