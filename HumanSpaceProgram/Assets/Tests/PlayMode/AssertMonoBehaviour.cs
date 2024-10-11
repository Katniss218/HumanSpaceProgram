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

            Debug.Log( TimeManager.UT + " - " + sut.Position );
        }
    }
}