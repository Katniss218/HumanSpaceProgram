using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Effects.Lights.LightShapes
{
    public sealed class SpotLightShape : ILightShape
    {
        public ConstantEffectValue<float> Radius { get; set; } = new( 10f );
        public ConstantEffectValue<float> Angle { get; set; } = new( 10f );

        public void OnInit( LightEffectHandle handle )
        {
            handle.Type = LightType.Spot;

            if( Radius != null )
            {
                Radius.InitDrivers( handle );
                handle.Range = Radius.Get();
            }
            if( Angle != null )
            {
                Angle.InitDrivers( handle );
                handle.ConeAngle = Angle.Get();
            }
        }

        public void OnUpdate( LightEffectHandle handle )
        {
            if( Radius != null && Radius.drivers != null )
                handle.Range = Radius.Get();
            if( Angle != null && Angle.drivers != null )
                handle.ConeAngle = Angle.Get();
        }


        [MapsInheritingFrom( typeof( SpotLightShape ) )]
        public static SerializationMapping SpotLightShapeMapping()
        {
            return new MemberwiseSerializationMapping<SpotLightShape>()
                .WithMember( "radius", o => o.Radius )
                .WithMember( "angle", o => o.Angle );
        }
    }
}