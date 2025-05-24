using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Effects.Lights.LightShapes
{
    public sealed class PointLightShape : ILightShape
    {
        public ConstantEffectValue<float> Radius { get; set; } = new( 10f );

        public void OnInit( LightEffectHandle handle )
        {
            handle.Type = LightType.Point;

            if( Radius != null )
                handle.Range = Radius.Get();
        }

        public void OnUpdate( LightEffectHandle handle )
        {
            if( Radius != null && Radius.drivers != null )
            {
                Radius.InitDrivers( handle );
                handle.Range = Radius.Get();
            }
        }


        [MapsInheritingFrom( typeof( PointLightShape ) )]
        public static SerializationMapping PointLightShapeMapping()
        {
            return new MemberwiseSerializationMapping<PointLightShape>()
                .WithMember( "radius", o => o.Radius );
        }
    }
}