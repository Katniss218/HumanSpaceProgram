using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Effects.Meshes.SimulationFrames
{
    public class CustomSimulationFrame : IMeshEffectSimulationFrame
    {
        private Quaternion _lastFrameEmitterRotation;
        private float[] _emitterSpeedBuffer;

        private float[] _boneTime;

#warning TODO - maybe reproject the bones every frame using previous frame's data?
        /*
        We have current local bone position and velocity. We have old velocity, old rotation. 

        Transform the old velocity from old frame to new frame.
        Move bone back along current velocity so we're effectively sampling the part of the plume flow that is going to arrive at the bone (plume is moving). 
        Move the bone forward along the transformed velocity. 
        */

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


            // bendy meshes:

            // - use bones to deform, bones rotate/position themselves.
            /*
                define `distance` as distance between the emitter and the bone
                define `delay` (cached) as the time it takes for the plume to travel from the emitter to the bone (`distance`/plume_velocity)
                store (or approximate) the rotation the emitter had at time `T - delay`
                move the bone so it's `distance` away, inline with the rotation calculated step above

                this would need a (Quaternion) rotation history buffer for the emitter, but shouldn't be heavy to compute
                the buffer can be stored at intervals greater than every frame, interpolated between the times.

                // since the plume velocity can change over time, we need to store the velocity history as well.
            */

            foreach( var bone in bones )
            {
#warning INFO - these frame transformations should be 'on top of' the driven bone movements.
                // if the bone position/velocity was always constant, calculating the rotation/position would be easy - just sample the history at time `T - constant_delay`.
                // since the bone position (in 3D) and plume velocity is NOT constant, this is more involved.

                // store the velocity of the plume over time, and integrate that to compute the current distance (basically reverse-engineer how far the bone would've traveled in some time)
                Vector3 pos = default;
                
               // bone.SetTRS( pos, rot, Vector3.one );
            }
        }


        [MapsInheritingFrom( typeof( CustomSimulationFrame ) )]
        public static SerializationMapping CustomSimulationFrameMapping()
        {
            return new MemberwiseSerializationMapping<CustomSimulationFrame>();
        }
    }
}