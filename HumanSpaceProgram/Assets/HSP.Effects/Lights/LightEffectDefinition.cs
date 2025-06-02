using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Effects.Lights
{
    public class LightEffectDefinition : ILightEffectData
    {
        /// <summary>
        /// The transform that the playing audio will follow.
        /// </summary>
        public Transform TargetTransform { get; set; }

        public int CullingMask { get; set; } = int.MaxValue;
        public ILightShape Shape { get; set; }

        //
        // Driven properties:

        public ConstantEffectValue<Vector3> Position { get; set; } = null;
        public ConstantEffectValue<Quaternion> Rotation { get; set; } = null;
        public ConstantEffectValue<float> Intensity { get; set; } = new( 1f );
        public ConstantEffectValue<Color> Color { get; set; } = null;


        public void OnInit( LightEffectHandle handle )
        {
            handle.TargetTransform = this.TargetTransform;
            handle.CullingMask = this.CullingMask;

            if( this.Position != null )
            {
                this.Position.InitDrivers( handle );
                handle.Position = this.Position.Get();
            }
            if( this.Rotation != null )
            {
                this.Rotation.InitDrivers( handle );
                handle.Rotation = this.Rotation.Get();
            }
            if( this.Intensity != null )
            {
                this.Intensity.InitDrivers( handle );
                handle.Intensity = this.Intensity.Get();
            }
            if( this.Color != null )
            {
                this.Color.InitDrivers( handle );
                handle.Color = this.Color.Get();
            }
        }

        public void OnUpdate( LightEffectHandle handle )
        {
            if( this.Intensity != null && this.Intensity.drivers != null )
                handle.Intensity = this.Intensity.Get();
        }

        [MapsInheritingFrom( typeof( LightEffectDefinition ) )]
        public static SerializationMapping LightEffectDefinitionMapping()
        {
            return new MemberwiseSerializationMapping<LightEffectDefinition>()
                .WithMember( "shape", o => o.Shape )
                .WithMember( "position", o => o.Position )
                .WithMember( "rotation", o => o.Rotation )
                .WithMember( "intensity", o => o.Intensity )
                .WithMember( "color", o => o.Color );
        }

        public IEffectHandle Play()
        {
            return LightEffectManager.Play( this );
        }
    }
}