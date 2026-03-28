using UnityPlus.Serialization;
using UnityPlus.Serialization.Descriptors;

namespace HSP.Effects
{
    public struct EffectValueDriver<T>
    {
        // translate them to builtin unity particle system stuff.
        public IValueGetter<T> Getter { get; set; }
        public IMappingCurve<T> Curve { get; set; }


        [MapsInheritingFrom( typeof( EffectValueDriver<> ) )]
        public static IDescriptor EffectValueDriverMapping()
        {
            return new MemberwiseDescriptor<EffectValueDriver<T>>()
                .WithMember( "getter", o => o.Getter )
                .WithMember( "curve", o => o.Curve );
        }
    }
}