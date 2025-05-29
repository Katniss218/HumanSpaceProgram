using HSP.ReferenceFrames;
using HSP.Time;
using HSP.Vanilla;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityPlus;

namespace HSP_Tests_PlayMode
{
    public class AssertMonoBehaviour : SingletonMonoBehaviour<AssertMonoBehaviour>
    {
        public delegate void AssertDelegate( int frame );

        public IReferenceFrameTransform sut;

        public AssertDelegate updateAssert;
        public AssertDelegate fixedUpdateAssert;
        public AssertDelegate lateUpdateAssert;


        void FixedUpdate()
        {
            if( sut == null )
                return;

            Debug.Log( TimeManager.UT + " - " + sut.Position + " : " + sut.AbsolutePosition );
        }



        /*private void OnEnable()
        {
            PlayerLoopUtils.InsertSystemBefore<FixedUpdate>( in _playerLoopSystem, typeof( FixedUpdate.ScriptRunBehaviourFixedUpdate ) );
            PlayerLoopUtils.InsertSystemAfter<FixedUpdate>( in _playerLoopSystem, typeof( FixedUpdate.ScriptRunBehaviourFixedUpdate ) );
        }

        private void OnDisable()
        {

        }

        private static PlayerLoopSystem _playerLoopSystem = new PlayerLoopSystem()
        {
            type = typeof( SceneReferenceFrameManager ),
            updateDelegate = ImmediatelyAfterUnityPhysicsStep,
            subSystemList = null
        };
        */

    }
}