using System.Linq;
using UnityEngine;
using UnityPlus.Input;
using UnityPlus.Input.Bindings;

namespace UnityPlus.Serialization.Input.Mappings
{
    public static class Mappings_Input
    {
        [MapsInheritingFrom( typeof( KeyDownBinding ) )]
        public static SerializationMapping KeyDownBindingMapping()
        {
            return new MemberwiseSerializationMapping<KeyDownBinding>()
                .WithReadonlyMember( "value", o => o.Value )
                .WithReadonlyMember( "key", o => o.Key )
                .WithFactory<float, KeyCode>( ( value, key ) =>
                {
                    return new KeyDownBinding( value, key );
                } );
        }

        [MapsInheritingFrom( typeof( KeyHoldBinding ) )]
        public static SerializationMapping KeyHoldBindingMapping()
        {
            return new MemberwiseSerializationMapping<KeyHoldBinding>()
                .WithReadonlyMember( "value", o => o.Value )
                .WithReadonlyMember( "key", o => o.Key )
                .WithFactory<float, KeyCode>( ( value, key ) =>
                {
                    return new KeyHoldBinding( value, key );
                } );
        }

        [MapsInheritingFrom( typeof( KeyUpBinding ) )]
        public static SerializationMapping KeyUpBindingMapping()
        {
            return new MemberwiseSerializationMapping<KeyUpBinding>()
                .WithReadonlyMember( "value", o => o.Value )
                .WithReadonlyMember( "key", o => o.Key )
                .WithFactory<float, KeyCode>( ( value, key ) =>
                {
                    return new KeyUpBinding( value, key );
                } );
        }

        [MapsInheritingFrom( typeof( MultipleKeyDownBinding ) )]
        public static SerializationMapping MultipleKeyDownBindingMapping()
        {
            return new MemberwiseSerializationMapping<MultipleKeyDownBinding>()
                .WithReadonlyMember( "value", o => o.Value )
                .WithReadonlyMember( "keys", o => o.KeysOrdered )
                .WithFactory<float, KeyCode[]>( ( value, keys ) =>
                {
                    return new MultipleKeyDownBinding( value, keys );
                } );
        }

        [MapsInheritingFrom( typeof( AxisBinding ) )]
        public static SerializationMapping AxisBindingMapping()
        {
            return new MemberwiseSerializationMapping<AxisBinding>()
                .WithReadonlyMember( "binding_tuples", o => o.BindingTuples.ToArray() )
                .WithFactory<(IInputBinding binding, float valueMultiplier)[]>( bindingTuples =>
                {
                    return new AxisBinding( bindingTuples );
                } );
        }

        [MapsInheritingFrom( typeof( MouseClickBinding ) )]
        public static SerializationMapping MouseClickBindingMapping()
        {
            return new MemberwiseSerializationMapping<MouseClickBinding>()
                .WithReadonlyMember( "value", o => o.Value )
                .WithReadonlyMember( "key", o => o.Key )
                .WithFactory<float, KeyCode>( ( value, key ) =>
                {
                    return new MouseClickBinding( value, key );
                } );
        }

        [MapsInheritingFrom( typeof( MouseDragBinding ) )]
        public static SerializationMapping MouseDragBindingMapping()
        {
            return new MemberwiseSerializationMapping<MouseDragBinding>()
                .WithReadonlyMember( "value", o => o.Value )
                .WithReadonlyMember( "key", o => o.Key )
                .WithFactory<float, KeyCode>( ( value, key ) =>
                {
                    return new MouseDragBinding( value, key );
                } );
        }
    }
}