using System;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Effects.Meshes.SimulationFrames
{
    [Obsolete( "not finished yet" )]
    public class StreamMeshEffectDefinition : IMeshEffectData
    {
        public Transform TargetTransform { get; set; }

        public bool IsSkinned => true;

        public IReadOnlyList<BindPose> BoneBindPoses => throw new NotImplementedException();

        private Quaternion _lastFrameEmitterRotation;
        private float[] _emitterSpeedBuffer;

        private float[] _boneTime;

#warning TODO - reproject the bones every frame using previous frame's data? Additionally, this should be a separate mesh effect definition class, not a simulation frame.
        // that definition will use time (key) instead of position directly to define where the bones should go, and will act like a 'stream'.

        public void OnInit( MeshEffectHandle handle )
        {
            // Bendy meshes only work when skinned, duh!
            if( !handle.IsSkinned )
                return;

            // bones use the time it takes for the plume to reach them from the emitter as the main value driving everything.

            // initial bone position in the mesh still defined by the bindpose, but when the mesh starts being animated, it can get shorter/longer, etc.
            // - We can use distance in the bindpose and the plume velocity to calculate the appropriate time value for each bone.

            _boneTime = new float[handle.Bones.Count];
            for( int i = 0; i < handle.Bones.Count; i++ )
            {
                //  _boneTime[i] = handle.Bones[i].localPosition.magnitude / handle.Speed; // distance / velocity = time
            }
        }

        public void OnUpdate( MeshEffectHandle handle )
        {
            // Bendy meshes only work when skinned, duh!
            if( !handle.IsSkinned )
                return;

            var bones = handle.Bones;


            foreach( var bone in bones )
            {
                /*
                We have current local bone position and velocity. We have old velocity, old rotation. 

                Transform the old velocity from old frame to new frame.
                Move bone back along current velocity so we're effectively sampling the part of the plume flow that is going to arrive at the bone (plume is moving). 
                Move the bone forward along the transformed velocity. 
                */
                // newLocalPos = (currentLocalPos − (currentLocalVel * T_bone)) + (transformedOldVelocity * T_bone) -- something like this
                // where T_bone is the time (key) of the bone
            }
        }

        public void OnDispose( MeshEffectHandle handle )
        {
        }

        public IEffectHandle Play()
        {
            return MeshEffectManager.Play( this );
        }


        [MapsInheritingFrom( typeof( StreamMeshEffectDefinition ) )]
        public static SerializationMapping StreamMeshEffectDefinitionMapping()
        {
            return new MemberwiseSerializationMapping<StreamMeshEffectDefinition>();
        }
    }
}