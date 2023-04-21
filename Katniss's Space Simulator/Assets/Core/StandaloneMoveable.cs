using KatnisssSpaceSimulator.Core.Physics;
using KatnisssSpaceSimulator.Core.ReferenceFrames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Core
{
    [RequireComponent(typeof(Rigidbody))]
    public class StandaloneMoveable : MonoBehaviour
    {
        // this is basically either a celestial body, or a vessel. Something that moves on its own and is not parented to anything else.

        IReferenceFrame referenceFrame; // reference frame will need a reference point/object.

        /*ForceSolver forceSolver = new ForceSolver()
        {
            Mass = 5
        };*/

        Rigidbody rb;

        void Awake()
        {
            rb = this.GetComponent<Rigidbody>();
            rb.useGravity = false;
            referenceFrame = new DefaultFrame();
        }

        void FixedUpdate()
        {
            if( Input.GetKey( KeyCode.W ) )
            {
                rb.AddForce( Vector3.up * 6.0f );
                //forceSolver.AddForce(  );
            }



#warning TODO - this also needs to support floaring origin/krakensbane equivalent (resetting in-game velocity/position) to keep it running accurately.
            // frames of reference maybe can be used for that.

            // a rotating frame of reference will impart forces on the object just because it is rotating.
            // the reference frame needs to store high precision position / rotation of the object.
            // - Every object's position will be transformed by this frame to get its "true" position, which might have arbitrary precision (and in reverse too, inverse to get local position).

            // There is only one global reference frame for the scene.
            
            // objects that are not centered on the reference frame need to be updated every frame (possibly less if they're very distant and can't be seen) to remain correct.
            // - if the frame is centered on the active vessel, then "world" space in Unity needs to be transformed into local space for that frame.
            // - - This can be done by applying forces/changing positions manually.


            // Bottom line is that we need to make the Unity's world space act like the local space of the selected reference frame.



            rb.AddForce( Vector3.down * 9.81f / 5.0f );
            //forceSolver.AddForce( (Vector3.down * 9.81f) / (float)forceSolver.Mass );

            // do more stuff here.

            // Advance and update transform values.
            //forceSolver.Advance( Time.fixedDeltaTime );

            //this.transform.position = referenceFrame.TransformPosition( forceSolver.Position );
            // rotation?
        }
    }
}
