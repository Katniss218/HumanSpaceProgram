using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Effects.Meshes
{
    public struct BindPose
    {
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Scale { get; set; }


        [MapsInheritingFrom( typeof( BindPose ) )]
        public static SerializationMapping BindPoseMapping()
        {
            return new MemberwiseSerializationMapping<BindPose>()
                .WithMember( "position", o => o.Position )
                .WithMember( "rotation", o => o.Rotation )
                .WithMember( "scale", o => o.Scale );
        }
    }

    public class BoneData
    {
        /// <summary>
        /// The initial pose of the bone. Corresponds to when the mesh is not deformed.
        /// </summary>
        public BindPose BindPose { get; set; }

        public ConstantEffectValue<Vector3> Position { get; set; } = null;
        public ConstantEffectValue<Quaternion> Rotation { get; set; } = null;
        public ConstantEffectValue<Vector3> Scale { get; set; } = null;


        [MapsInheritingFrom( typeof( BoneData ) )]
        public static SerializationMapping BoneDataMapping()
        {
            return new MemberwiseSerializationMapping<BoneData>()
                .WithMember( "bind_pose", o => o.BindPose )
                .WithMember( "position", o => o.Position )
                .WithMember( "rotation", o => o.Rotation )
                .WithMember( "scale", o => o.Scale );
        }
    }
}