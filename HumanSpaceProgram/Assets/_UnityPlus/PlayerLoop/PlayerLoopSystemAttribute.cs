using System;

namespace UnityPlus.PlayerLoop
{
    /// <summary>
    /// Identifies a struct or sealed class as a custom PlayerLoopSystem or a bucket into which such systems can be inserted.
    /// </summary>
    /// <remarks>
    /// Uses the declared type as the identifier for the system, and the <see cref="TargetBucket"/> property to determine where to place it in the player loop. <br/>
    /// </remarks>
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false )]
    public class PlayerLoopSystemAttribute : Attribute
    {
        /// <summary>
        /// Where to place the system (e.g. Update, FixedUpdate, etc.). Can also be a custom bucket defined by another class with [PlayerLoopSystem] or [PlayerLoopNative]. <br/>
        /// If null, the system will be placed at the root of the player loop.
        /// </summary>
        public Type TargetBucket { get; }

        public Type[] Before { get; set; }
        public Type[] After { get; set; }
        public Type[] Blacklist { get; set; }

        public PlayerLoopSystemAttribute( Type targetBucket )
        {
            TargetBucket = targetBucket;
        }

        internal void Validate( Type sourceType )
        {
            ValidateIsCustom( TargetBucket, sourceType, nameof( TargetBucket ) );
            if( Before != null )
            {
                foreach( var t in Before )
                    ValidateIsCustom( t, sourceType, nameof( Before ) );
            }
            if( After != null )
            {
                foreach( var t in After )
                    ValidateIsCustom( t, sourceType, nameof( After ) );
            }
            if( Blacklist != null )
            {
                foreach( var t in Blacklist )
                    ValidateIsCustom( t, sourceType, nameof( Blacklist ) );
            }
        }

        private static void ValidateIsCustom( Type type, Type sourceType, string propertyName )
        {
            if( type == null )
                return;

            if( !Attribute.IsDefined( type, typeof( PlayerLoopSystemAttribute ), inherit: false ) &&
                !Attribute.IsDefined( type, typeof( PlayerLoopNativeAttribute ), inherit: false ) )
            {
                throw new InvalidOperationException( $"{sourceType.Name} is a System node, but it references a native type '{type.Name}' in {propertyName}. System nodes can only reference types decorated with [{nameof( PlayerLoopNativeAttribute )}] or [{nameof( PlayerLoopSystemAttribute )}]." );
            }
        }
    }
}