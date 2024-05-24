using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{
    public delegate void LoadReferencesAction<TSource>( ref TSource obj, SerializedData data, IForwardReferenceMap l );

    /// <summary>
    /// Maps the source type to a SerializedData directly, using methods.
    /// </summary>
    /// <typeparam name="TSource">The type of the object being mapped.</typeparam>
    public class DirectSerializationMapping<TSource> : SerializationMapping
    {
        /// <summary>
        /// The function invoked to convert the C# object into its serialized representation.
        /// </summary>
        public Func<TSource, IReverseReferenceMap, SerializedData> SaveFunc { get; set; }

        /// <summary>
        /// The function invoked to convert the serialized representation back into its corresponding C# object.
        /// </summary>
        public Func<SerializedData, IForwardReferenceMap, TSource> LoadFunc { get; set; }

        /// <summary>
        /// The function invoked to fill in the references in the created object.
        /// </summary>
        public LoadReferencesAction<TSource> LoadReferencesFunc { get; set; }

        public DirectSerializationMapping()
        {

        }

        public override SerializedData Save( object obj, IReverseReferenceMap s )
        {
            return SaveFunc.Invoke( (TSource)obj, s );
        }

        public override object Load( SerializedData data, IForwardReferenceMap l )
        {
            if( LoadFunc != null )
                return LoadFunc.Invoke( data, l );
            return default( TSource );
        }

        public override void LoadReferences( ref object obj, SerializedData data, IForwardReferenceMap l )
        {
            if( LoadReferencesFunc != null )
            {
                // obj may be null here.
                var obj2 = (TSource)obj;
                LoadReferencesFunc.Invoke( ref obj2, data, l );
                obj = obj2;
            }
            // Do nothing ...
        }
    }
}