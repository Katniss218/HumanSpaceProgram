using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Effects.Meshes
{
    /// <summary>
    /// Represents the transformational data of a bone in its 'base' pose.
    /// </summary>
    public struct BindPose
    {
        /// <summary>
        /// The position of the bone, in its 'base' pose, relative to the root bone.
        /// </summary>
        public Vector3 Position { get; set; }
        /// <summary>
        /// The rotation of the bone, in its 'base' pose, relative to the root bone.
        /// </summary>
        public Quaternion Rotation { get; set; }
        /// <summary>
        /// The scale of the bone, in its 'base' pose, relative to the root bone.
        /// </summary>
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
}