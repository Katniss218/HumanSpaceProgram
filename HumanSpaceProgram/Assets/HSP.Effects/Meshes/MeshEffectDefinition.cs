using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Effects.Meshes
{
    public class MeshEffectDefinition : IMeshEffectData
    {
        /// <summary>
        /// The transform that the mesh effect will follow.
        /// </summary>
        public Transform TargetTransform { get; set; }

        //
        // Driven properties:

        public BoneData[] Bones { get; set; } = null;


        public void OnInit( MeshEffectHandle handle )
        {
            handle.TargetTransform = this.TargetTransform;

            // if has bones - spawn them at initial positions, and rig the mesh using these positions.
        }

        public void OnUpdate( MeshEffectHandle handle )
        {
#warning TODO - meshes can be skinned or normal
            // skinned meshes can have different coordinate frames for the bones, like particles (via a transform).

            // bones can be deformed via curves/drivers
        }

        public IEffectHandle Play()
        {
            return MeshEffectManager.Play( this );
        }

        [MapsInheritingFrom( typeof( MeshEffectDefinition ) )]
        public static SerializationMapping MeshEffectDefinitionMapping()
        {
            return new MemberwiseSerializationMapping<MeshEffectDefinition>()
                .WithMember( "bones", o => o.Bones );
        }
    }
}