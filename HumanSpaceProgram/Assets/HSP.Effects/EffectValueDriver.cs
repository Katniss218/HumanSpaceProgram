using UnityPlus.Serialization;

namespace HSP.Effects
{
    public struct EffectValueDriver<T>
    {
        // translate them to builtin unity particle system stuff.
        public IValueGetter<T> Getter { get; set; }
        public IMappingCurve<T> Curve { get; set; }


        [MapsInheritingFrom( typeof( EffectValueDriver<> ) )]
        public static SerializationMapping EffectValueDriverMapping()
        {
            return new MemberwiseSerializationMapping<EffectValueDriver<T>>()
                .WithMember( "getter", o => o.Getter )
                .WithMember( "curve", o => o.Curve );
        }
    }
}