using HSP.ReferenceFrames;
using HSP.Time;
using HSP.Vanilla;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HSP_Tests_PlayMode
{
    public class AssertMonoBehaviour : MonoBehaviour
    {
        public IReferenceFrameTransform sut;

        void FixedUpdate()
        {
            if( sut == null )
                return;

            Debug.Log( TimeManager.UT + " - " + sut.Position + " : " + sut.AbsolutePosition );
        }

#warning TODO - make a list of delegates that will assert, that will be called at different times
        // asserts should use the current time to verify they know what values they should have.
        // the time since start (unityengine.time.time) can be used to cross check.
        // expected values at each time can be baked into the delegates in the lambda closure.
    }
}