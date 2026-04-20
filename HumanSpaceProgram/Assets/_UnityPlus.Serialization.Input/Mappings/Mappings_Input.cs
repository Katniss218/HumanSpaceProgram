using System.Linq;
using UnityEngine;
using UnityPlus.Input;
using UnityPlus.Input.Bindings;
using UnityPlus.Serialization.Descriptors;

namespace UnityPlus.Serialization.Input.Mappings
{
    public static class Mappings_Input
    {
        [MapsInheritingFrom( typeof( KeyDownBinding ) )]
        public static IDescriptor KeyDownBindingMapping()
        {
            return new MemberwiseDescriptor<KeyDownBinding>()
                .WithReadonlyMember( "value", o => o.Value )
                .WithReadonlyMember( "key", o => o.Key )
                .WithFactory<float, KeyCode>( ( value, key ) =>
                {
                    return new KeyDownBinding( value, key );
                }, "value", "key" );
        }

        [MapsInheritingFrom( typeof( KeyHoldBinding ) )]
        public static IDescriptor KeyHoldBindingMapping()
        {
            return new MemberwiseDescriptor<KeyHoldBinding>()
                .WithReadonlyMember( "value", o => o.Value )
                .WithReadonlyMember( "key", o => o.Key )
                .WithFactory<float, KeyCode>( ( value, key ) =>
                {
                    return new KeyHoldBinding( value, key );
                }, "value", "key" );
        }

        [MapsInheritingFrom( typeof( KeyUpBinding ) )]
        public static IDescriptor KeyUpBindingMapping()
        {
            return new MemberwiseDescriptor<KeyUpBinding>()
                .WithReadonlyMember( "value", o => o.Value )
                .WithReadonlyMember( "key", o => o.Key )
                .WithFactory<float, KeyCode>( ( value, key ) =>
                {
                    return new KeyUpBinding( value, key );
                }, "value", "key" );
        }

        [MapsInheritingFrom( typeof( MultipleKeyDownBinding ) )]
        public static IDescriptor MultipleKeyDownBindingMapping()
        {
            return new MemberwiseDescriptor<MultipleKeyDownBinding>()
                .WithReadonlyMember( "value", o => o.Value )
                .WithReadonlyMember( "keys", o => o.KeysOrdered )
                .WithFactory<float, KeyCode[]>( ( value, keys ) =>
                {
                    return new MultipleKeyDownBinding( value, keys );
                }, "value", "keys" );
        }

        [MapsInheritingFrom( typeof( AxisBinding ) )]
        public static IDescriptor AxisBindingMapping()
        {
            return new MemberwiseDescriptor<AxisBinding>()
                .WithReadonlyMember( "binding_tuples", o => o.BindingTuples.ToArray() )
                .WithFactory<(IInputBinding binding, float valueMultiplier)[]>( bindingTuples =>
                {
                    return new AxisBinding( bindingTuples );
                }, "binding_tuples" );
        }

        [MapsInheritingFrom( typeof( MouseClickBinding ) )]
        public static IDescriptor MouseClickBindingMapping()
        {
            return new MemberwiseDescriptor<MouseClickBinding>()
                .WithReadonlyMember( "value", o => o.Value )
                .WithReadonlyMember( "key", o => o.Key )
                .WithFactory<float, KeyCode>( ( value, key ) =>
                {
                    return new MouseClickBinding( value, key );
                }, "value", "key" );
        }

        [MapsInheritingFrom( typeof( MouseDragBinding ) )]
        public static IDescriptor MouseDragBindingMapping()
        {
            return new MemberwiseDescriptor<MouseDragBinding>()
                .WithReadonlyMember( "value", o => o.Value )
                .WithReadonlyMember( "key", o => o.Key )
                .WithFactory<float, KeyCode>( ( value, key ) =>
                {
                    return new MouseDragBinding( value, key );
                }, "value", "key" );
        }
    }
}