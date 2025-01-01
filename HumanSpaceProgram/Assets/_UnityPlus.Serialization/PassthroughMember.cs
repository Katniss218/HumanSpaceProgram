
using UnityEngine.Networking.Types;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// This is used when you need to store a member of the base type inside the memberwise mapping.
    /// </summary>
    internal class PassthroughMember<TSource, TSourceBase> : MemberBase<TSource> where TSource : class, TSourceBase
    {
        MemberBase<TSourceBase> _member;

        internal static PassthroughMember<TSource, TSourceBase> Create( MemberBase<TSourceBase> member )
        {
            return new PassthroughMember<TSource, TSourceBase>()
            {
                _member = member,
                Name = member.Name
            };
        }

        public override MemberBase<TSource> Copy()
        {
            return (MemberBase<TSource>)this.MemberwiseClone();
        }

        /// <summary>
        /// Calls the getter associated with this member.
        /// </summary>
        public override object Get( ref TSource source )
        {
            TSourceBase source2 = source;
            return _member.Get( ref source2 );
        }

        public override MappingResult Save( TSource sourceObj, SerializedData sourceData, ISaver s, out SerializationMapping mapping, out object memberObj )
        {
            TSourceBase sourceObj2 = sourceObj;
            return _member.Save( sourceObj2, sourceData, s, out mapping, out memberObj );
        }

        public override MappingResult SaveRetry( object memberObj, SerializationMapping mapping, SerializedData sourceData, ISaver s )
        {
            return _member.SaveRetry( memberObj, mapping, sourceData, s );
        }

        public override MappingResult Load( ref TSource sourceObj, bool isInstantiated, SerializedData sourceData, ILoader l, out SerializationMapping mapping, out object memberObj )
        {
            TSourceBase sourceObj2 = sourceObj;
            var res = _member.Load( ref sourceObj2, isInstantiated, sourceData, l, out mapping, out memberObj );
            sourceObj = (TSource)sourceObj2;
            return res;
        }

        public override MappingResult LoadRetry( ref object memberObj, SerializationMapping mapping, SerializedData sourceData, ILoader l )
        {
            return _member.LoadRetry( ref memberObj, mapping, sourceData, l );
        }

        /// <summary>
        /// Calls the setter associated with this member. <br/>
        /// Does nothing if the member is 'readonly'.
        /// </summary>
        public override void Set( ref TSource sourceObj, object member )
        {
            TSourceBase source2 = sourceObj;
            _member.Set( ref source2, member );
            sourceObj = (TSource)source2;
        }
    }
}