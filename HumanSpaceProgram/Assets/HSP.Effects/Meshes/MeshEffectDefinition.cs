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


        public void OnInit( MeshEffectHandle handle )
        {
            handle.TargetTransform = this.TargetTransform;
        }

        public void OnUpdate( MeshEffectHandle handle )
        {
#warning TODO - meshes can be skinned or normal
            // skinned meshes can have different coordinate frames for the bones, like particles (via a transform).

            // bones can be deformed via curves/drivers
        }

        [MapsInheritingFrom( typeof( MeshEffectHandle ) )]
        public static SerializationMapping MeshEffectHandleMapping()
        {
            return new MemberwiseSerializationMapping<MeshEffectHandle>()
                /*.WithMember( "audio_clip", ObjectContext.Asset, o => o.Clip )
                .WithMember( "audio_channel", o => o.Channel )
                .WithMember( "volume", o => o.Volume )
                .WithMember( "pitch", o => o.Pitch )
                .WithMember( "loop", o => o.Loop )*/;
        }

        public IEffectHandle Play()
        {
            return MeshEffectManager.Play( this );
        }
    }
}