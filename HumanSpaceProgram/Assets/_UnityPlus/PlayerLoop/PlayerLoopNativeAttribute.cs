using System;

namespace UnityPlus.PlayerLoop
{
    /// <summary>
    /// Identifies a struct or sealed class as a native PlayerLoopSystem alias or a native bucket.
    /// </summary>
    /// <remarks>
    /// Uses the declared type as the identifier for the system, and the <see cref="TargetBucket"/> property to determine where to place it in the player loop. <br/>
    /// </remarks>
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false )]
    public sealed class PlayerLoopNativeAttribute : Attribute
    {
        /// <summary>
        /// Where to place the system (e.g. Update, FixedUpdate, etc.). Must be a native unity playerloop type, or a custom native alias defined by [PlayerLoopNative]. <br/>
        /// </summary>
        public Type TargetBucket { get; }

        /// <summary>
        /// If this native node is an alias, this specifies the original native type it shadows. Must be a native unity playerloop type. <br/>
        /// </summary>
        public Type Alias { get; set; }

        public Type[] Before { get; set; }
        public Type[] After { get; set; }
        public Type[] Blacklist { get; set; }

        public PlayerLoopNativeAttribute() { }

        public PlayerLoopNativeAttribute( Type targetBucket )
        {
            TargetBucket = targetBucket;
        }

        internal void Validate( Type sourceType )
        {
            if( TargetBucket == null && Alias == null )
                throw new InvalidOperationException( $"{sourceType.Name} is a Native node, but it doesn't specify a TargetBucket or an Alias. Native nodes must specify either a TargetBucket or an Alias." );
            if( TargetBucket != null && Alias != null )
                throw new InvalidOperationException( $"{sourceType.Name} is a Native node, but it specifies both a TargetBucket and an Alias. Native nodes must specify either a TargetBucket or an Alias, but not both." );
            
            ValidateIsNative( TargetBucket, sourceType, nameof( TargetBucket ) );
            ValidateIsNative( Alias, sourceType, nameof( Alias ) );
            if( Before != null )
            {
                foreach( var t in Before )
                    ValidateIsNative( t, sourceType, nameof( Before ) );
            }
            if( After != null )
            {
                foreach( var t in After )
                    ValidateIsNative( t, sourceType, nameof( After ) );
            }
            if( Blacklist != null )
            {
                foreach( var t in Blacklist )
                    ValidateIsNative( t, sourceType, nameof( Blacklist ) );
            }
        }

        private static void ValidateIsNative( Type type, Type sourceType, string propertyName )
        {
            if( type == null )
                return;

            if( Attribute.IsDefined( type, typeof( PlayerLoopSystemAttribute ), inherit: false ) )
            {
                throw new InvalidOperationException( $"{sourceType.Name} is a Native node, but it references a non-native type '{type.Name}' in {propertyName}. Native nodes can only reference raw unity types or other native aliases." );
            }
        }
    }
}