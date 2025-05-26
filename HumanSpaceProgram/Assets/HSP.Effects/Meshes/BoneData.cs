using UnityEngine;

namespace HSP.Effects.Meshes
{
    public struct BoneData
    {
        /// <summary>
        /// The initial pose of the bone. Corresponds to when the mesh is not deformed.
        /// </summary>
        public BindPose BindPose { get; set; }

        public ConstantEffectValue<Vector3> Position { get; set; }
        public ConstantEffectValue<Quaternion> Rotation { get; set; }
        public ConstantEffectValue<Vector3> Scale { get; set; }
    }
}