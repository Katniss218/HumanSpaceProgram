using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Core.ReferenceFrames
{
    /// <summary>
    /// Implements <see cref="GameObject"/> part of the scene reference frame (floating origin / krakensbane).
    /// </summary>
    /// <remarks>
    /// Add this to any object that is supposed to be affected by the <see cref="SceneReferenceFrameManager"/>.
    /// </remarks>
    public class RootObjectTransform : MonoBehaviour, IReferenceFrameSwitchResponder
    {
        // Should to be added to any root object that is an actual [physical] object in the scene (not UI elements, empties, etc).

        Vector3Dbl _airfPosition;
        Quaternion _airfRotation;


        Rigidbody _rb;

        public Vector3Dbl GetAIRFPosition()
        {
            return this._airfPosition;
        }

        /// <summary>
        /// Sets the position of the vessel in Absolute Inertial Reference Frame coordinates. Units in [m].
        /// </summary>
        public void SetAIRFPosition( Vector3Dbl airfPosition )
        {
            this._airfPosition = airfPosition;

            UpdateScenePosition();
        }


        public Quaternion GetAIRFRotation()
        {
            return this._airfRotation;
        }

        /// <summary>
        /// Sets the rotation of the vessel in Absolute Inertial Reference Frame coordinates.
        /// </summary>
        public void SetAIRFRotation( Quaternion airfRotation )
        {
            this._airfRotation = airfRotation;

            UpdateSceneRotation();
        }


        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private void UpdateScenePosition()
        {
            Vector3 scenePos = SceneReferenceFrameManager.SceneReferenceFrame.InverseTransformPosition( this._airfPosition );
            if( _rb != null )
            {
                // THIS IS CRITICALLY IMPORTANT.
                // Rigidbodies keep their own position/rotation and will overwrite the object's position/rotation sometimes.
                this._rb.position = scenePos;
            }
            this.transform.position = scenePos;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void UpdateSceneRotation()
        {
            Quaternion sceneRotation = SceneReferenceFrameManager.SceneReferenceFrame.InverseTransformRotation( this._airfRotation );
            if( _rb != null )
            {
                // THIS IS CRITICALLY IMPORTANT.
                // Rigidbodies keep their own position/rotation and will overwrite the object's position/rotation sometimes.
                this._rb.rotation = sceneRotation;
            }
            this.transform.rotation = sceneRotation;
        }


        void Awake()
        {
            // in case RB is added by requirecomponent.
            _rb = this.GetComponent<Rigidbody>();
        }

        void Start()
        {
            // In case RB is added later (I don't want to lazily check every time though).
            _rb = this.GetComponent<Rigidbody>();
        }

        // we need to move the object if the reference frame is moving, and move/rotate it, if the reference frame is rotating.




        /// <summary>
        /// Callback to the event.
        /// </summary>
        public void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
        {
            UpdateScenePosition();
           // UpdateSceneRotation();
        }
    }
}