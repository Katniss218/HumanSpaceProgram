using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{
    public delegate void LoadAction<TSource>( ref TSource obj, SerializedData data, IForwardReferenceMap l );
    public delegate void LoadReferencesAction<TSource>( ref TSource obj, SerializedData data, IForwardReferenceMap l );

    /// <summary>
    /// Maps the source type to a SerializedData directly, using methods.
    /// </summary>
    /// <typeparam name="TSource">The type of the object being mapped.</typeparam>
    public class PrimitiveStructSerializationMapping<TSource> : SerializationMapping
    {
        /// <summary>
        /// The function invoked to convert the C# object into its serialized representation.
        /// </summary>
        public Func<TSource, IReverseReferenceMap, SerializedData> OnSave { get; set; }

        /// <summary>
        /// The function invoked to convert the serialized representation back into its corresponding C# object.
        /// </summary>
        public Func<SerializedData, IForwardReferenceMap, TSource> OnInstantiate { get; set; }

        public override SerializationStyle SerializationStyle => SerializationStyle.PrimitiveStruct;

        public PrimitiveStructSerializationMapping()
        {

        }

        public override SerializedData Save( object obj, IReverseReferenceMap s )
        {
            return OnSave.Invoke( (TSource)obj, s );
        }

        public override object Instantiate( SerializedData data, IForwardReferenceMap l )
        {
            if( OnInstantiate != null )
                return OnInstantiate.Invoke( data, l );
            return default( TSource );
        }

        public override void Load( ref object obj, SerializedData data, IForwardReferenceMap l )
        {
            throw new InvalidOperationException( $"Load is not supported on `{nameof( PrimitiveStructSerializationMapping<TSource> )}`." );
        }

        public override void LoadReferences( ref object obj, SerializedData data, IForwardReferenceMap l )
        {
            throw new InvalidOperationException( $"LoadReferences is not supported on `{nameof( PrimitiveStructSerializationMapping<TSource> )}`." );
        }
    }


    /// <summary>
    /// Maps the source type to a SerializedData directly, using methods.
    /// </summary>
    /// <typeparam name="TSource">The type of the object being mapped.</typeparam>
    public class PrimitiveObjectSerializationMapping<TSource> : SerializationMapping
    {
        /// <summary>
        /// The function invoked to convert the C# object into its serialized representation.
        /// </summary>
        public Func<TSource, IReverseReferenceMap, SerializedData> OnSave { get; set; }

        /// <summary>
        /// The function invoked to convert the serialized representation back into its corresponding C# object.
        /// </summary>
        public Func<SerializedData, IForwardReferenceMap, TSource> OnInstantiate { get; set; }

        public override SerializationStyle SerializationStyle => SerializationStyle.PrimitiveObject;

        public PrimitiveObjectSerializationMapping()
        {

        }

        public override SerializedData Save( object obj, IReverseReferenceMap s )
        {
            return OnSave.Invoke( (TSource)obj, s );
        }

        public override object Instantiate( SerializedData data, IForwardReferenceMap l )
        {
            if( OnInstantiate != null )
                return OnInstantiate.Invoke( data, l );
            return default( TSource );
        }

        public override void Load( ref object obj, SerializedData data, IForwardReferenceMap l )
        {
            throw new InvalidOperationException( $"Load is not supported on `{nameof( PrimitiveObjectSerializationMapping<TSource> )}`." );
        }

        public override void LoadReferences( ref object obj, SerializedData data, IForwardReferenceMap l )
        {
            throw new InvalidOperationException( $"LoadReferences is not supported on `{nameof( PrimitiveObjectSerializationMapping<TSource> )}`." );
        }
    }

    /// <summary>
    /// Maps the source type to a SerializedData directly, using methods.
    /// </summary>
    /// <typeparam name="TSource">The type of the object being mapped.</typeparam>
    public class NonPrimitiveSerializationMapping<TSource> : SerializationMapping
    {
        /// <summary>
        /// The function invoked to convert the C# object into its serialized representation.
        /// </summary>
        public Func<TSource, IReverseReferenceMap, SerializedData> OnSave { get; set; }

        /// <summary>
        /// The function invoked to convert the serialized representation back into its corresponding C# object.
        /// </summary>
        public Func<SerializedData, IForwardReferenceMap, TSource> OnInstantiate { get; set; }

        /// <summary>
        /// Loads the members.
        /// </summary>
        public LoadAction<TSource> OnLoad { get; set; }

        /// <summary>
        /// Loads the references.
        /// </summary>
        public LoadReferencesAction<TSource> OnLoadReferences { get; set; }

        public override SerializationStyle SerializationStyle => SerializationStyle.NonPrimitive;

        public NonPrimitiveSerializationMapping()
        {

        }

        public override SerializedData Save( object obj, IReverseReferenceMap s )
        {
            return OnSave.Invoke( (TSource)obj, s );
        }

        public override object Instantiate( SerializedData data, IForwardReferenceMap l )
        {
            if( OnInstantiate != null )
                return OnInstantiate.Invoke( data, l );
            return default( TSource );
        }

        public override void Load( ref object obj, SerializedData data, IForwardReferenceMap l )
        {
            if( OnLoad != null )
            {
                // obj can be null here, this is normal.
                var obj2 = (TSource)obj;
                OnLoad.Invoke( ref obj2, data, l );
                obj = obj2;
            }
        }

        public override void LoadReferences( ref object obj, SerializedData data, IForwardReferenceMap l )
        {
            if( OnLoadReferences != null )
            {
                // obj can be null here, this is normal.
                var obj2 = (TSource)obj;
                OnLoadReferences.Invoke( ref obj2, data, l );
                obj = obj2;
            }
        }
    }
}