using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Effects.Lights.LightShapes
{
    public sealed class SpotLightShape : ILightShape
    {
        public ConstantEffectValue<float> Radius { get; set; } = new( 10f );
        public ConstantEffectValue<float> SpotlightAngle { get; set; } = new( 10f );

        public void OnInit( LightEffectHandle handle )
        {
            handle.Type = LightType.Spot;

            if( Radius != null )
                handle.Range = Radius.Get();
            if( SpotlightAngle != null )
                handle.ConeAngle = SpotlightAngle.Get();
        }

        public void OnUpdate( LightEffectHandle handle )
        {
            if( Radius != null && Radius.drivers != null )
                handle.Range = Radius.Get();
            if( SpotlightAngle != null && SpotlightAngle.drivers != null )
                handle.ConeAngle = SpotlightAngle.Get();
        }

        [MapsInheritingFrom( typeof( PointLightShape ) )]
        public static SerializationMapping PointLightShapeMapping()
        {
            return new MemberwiseSerializationMapping<PointLightShape>()
                .WithMember( "radius", o => o.Radius );
        }
    }
}