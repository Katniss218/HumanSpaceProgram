using System;
using System.Collections.Generic;

namespace UnityPlus.Serialization.Descriptors
{
    /// <summary>
    /// A generic descriptor for rectangular multi-dimensional data structures (Grids, Matrices, etc.)
    /// that are serialized as a metadata object containing dimensions and a flat collection of values.
    /// </summary>
    public class MultiDimensionalDescriptor<TContainer, TElement> : CompositeDescriptor
    {
        public override Type MappedType => typeof( TContainer );

        /// <summary>
        /// A delegate that creates the container instance given the dimensions.
        /// </summary>
        public Func<int[], TContainer> Factory { get; set; }

        /// <summary>
        /// A delegate that retrieves the dimensions of the container.
        /// </summary>
        public Func<TContainer, int[]> GetLengths { get; set; }

        /// <summary>
        /// A delegate that retrieves a flat array of all elements in the container.
        /// </summary>
        public Func<TContainer, TElement[]> GetFlatValues { get; set; }

        /// <summary>
        /// A delegate that populates the container from a flat array of elements.
        /// </summary>
        public Action<TContainer, TElement[]> SetFlatValues { get; set; }

        public override object CreateInitialTarget( SerializedData data, SerializationContext ctx )
        {
            if( Factory == null ) return null;

            var obj = data as SerializedObject;
            if( obj == null && data is SerializedObject wrapper && wrapper.TryGetValue( KeyNames.VALUE, out var inner ) )
            {
                obj = inner as SerializedObject;
            }

            if( obj != null && obj.TryGetValue( "lengths", out var lengthsData ) )
            {
                var lengthsArr = lengthsData as SerializedArray;
                if( lengthsArr != null )
                {
                    int[] lengths = new int[lengthsArr.Count];
                    for( int i = 0; i < lengthsArr.Count; i++ )
                    {
                        lengths[i] = (int)(SerializedPrimitive)lengthsArr[i];
                    }
                    return Factory( lengths );
                }
            }

            return null;
        }

        public override int GetStepCount( object target ) => 2;

        public override IMemberInfo GetMemberInfo( int stepIndex )
        {
            if( stepIndex == 0 ) return new LengthsMemberInfo( GetLengths );
            if( stepIndex == 1 ) return new ValuesMemberInfo( GetFlatValues, SetFlatValues );
            return null;
        }

        private class LengthsMemberInfo : IMemberInfo
        {
            private readonly Func<TContainer, int[]> _getLengths;

            public LengthsMemberInfo( Func<TContainer, int[]> getLengths )
            {
                _getLengths = getLengths;
            }

            public string Name => "lengths";
            public int Index => 0;
            public Type DeclaredType => typeof( int[] );
            public bool RequiresWriteBack => false;
            public IDescriptor TypeDescriptor => null;

            public ContextKey GetContext( object target ) => default;

            public object GetValue( object target )
            {
                return _getLengths?.Invoke( (TContainer)target );
            }

            public void SetValue( ref object target, object value )
            {
                // Lengths are typically used during construction.
            }
        }

        private class ValuesMemberInfo : IMemberInfo
        {
            private readonly Func<TContainer, TElement[]> _getFlatValues;
            private readonly Action<TContainer, TElement[]> _setFlatValues;

            public ValuesMemberInfo( Func<TContainer, TElement[]> getFlatValues, Action<TContainer, TElement[]> setFlatValues )
            {
                _getFlatValues = getFlatValues;
                _setFlatValues = setFlatValues;
            }

            public string Name => "values";
            public int Index => 1;
            public Type DeclaredType => typeof( TElement[] );
            public bool RequiresWriteBack => true;
            public IDescriptor TypeDescriptor => null;

            public ContextKey GetContext( object target ) => default;

            public object GetValue( object target )
            {
                return _getFlatValues?.Invoke( (TContainer)target );
            }

            public void SetValue( ref object target, object value )
            {
                _setFlatValues?.Invoke( (TContainer)target, (TElement[])value );
            }
        }
    }
}