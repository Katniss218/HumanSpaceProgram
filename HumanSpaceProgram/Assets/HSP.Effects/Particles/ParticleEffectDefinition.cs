using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Effects.Particles
{
    public class ParticleEffectDefinition : IParticleEffectData
    {
        // particles need to specify a frame
        // following an object's position, rotation, in planet space, etc.

        public Material Material { get; set; }

        /// <summary>
        /// Whether the audio should loop or not.
        /// </summary>
        public bool Loop { get; set; }

        /// <summary>
        /// The transform that the playing audio will follow.
        /// </summary>
        public Transform TargetTransform { get; set; }


        // list all properties...

        public ConstantEffectValue<float> size = new();


        public void OnInit( ParticleEffectHandle handle )
        {
            handle.TargetTransform = this.TargetTransform;
            handle.Material = this.Material;
            // material and other
        }

        public void OnUpdate( ParticleEffectHandle handle )
        {
            handle.Size = this.size.Get();
            // build cached entries:
            // - pass in the particle system to bake the reference to its instance into the setter
            // - bake the getting of the value into the lambda as well.
            // - bake the comparison with new/old too.

            /*foreach( var cachedEntry in _cachedEntriesWithDrivers )
            {
                cachedEntry.Setter.Invoke();
            }*/
            //_ps.main.startSize.constantMin = _definition.size.GetMin();
            //_ps.main.startSize.constantMax = _definition.size.GetMax();
        }


        [MapsInheritingFrom( typeof( ParticleEffectDefinition ) )]
        public static SerializationMapping ParticleEffectDefinitionMapping()
        {
            return new MemberwiseSerializationMapping<ParticleEffectDefinition>()
                .WithMember( "material", ObjectContext.Asset, o => o.Material )
                .WithMember( "loop", o => o.Loop )
                .WithMember( "target_transform", o => o.TargetTransform )
                .WithMember( "size", o => o.size );
        }
    }
}