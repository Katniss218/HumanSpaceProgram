using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Core.ReferenceFrames
{
    /// <summary>
    /// Implements <see cref="GameObject"/> part of the scene reference frame (floating origin / krakensbane).
    /// </summary>
    /// <remarks>
    /// Add this to any object that is supposed to be affected by the <see cref="SceneReferenceFrameManager"/>.
    /// </remarks>
    public class RootObjectTransform : MonoBehaviour, IPersistent, IReferenceFrameSwitchResponder
    {
        // Should to be added to any root object that is an actual [physical] object in the scene (not UI elements, empties, etc).

        // Root objects store their AIRF positions, children natively store their local coordinates, which as long as they're not obscenely large, will be fine.
        // - An object with a child at 0.00125f can be sent to 10e25 and brought back, and its child will remain at 0.00125f

        [SerializeField] Vector3Dbl _airfPosition;
        [SerializeField] QuaternionDbl _airfRotation;

        Rigidbody _rb;

        /// <summary>
        /// Gets or sets the position of the object in Absolute Inertial Reference Frame coordinates. Units in [m].
        /// </summary>
        public Vector3Dbl AIRFPosition
        {
            get
            {
                return this._airfPosition;
            }
            set
            {
                this._airfPosition = value;

                UpdateScenePosition();
            }
        }

        /// <summary>
        /// Gets or sets the rotation of the object in Absolute Inertial Reference Frame coordinates.
        /// </summary>
        public QuaternionDbl AIRFRotation
        {
            get
            {
                return this._airfRotation;
            }
            set
            {
                this._airfRotation = value;

                UpdateSceneRotation();
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private void UpdateScenePosition()
        {
            Vector3 scenePos = (Vector3)SceneReferenceFrameManager.SceneReferenceFrame.InverseTransformPosition( this._airfPosition );
            if( _rb != null )
            {
                // THIS IS CRITICALLY IMPORTANT.
                // Rigidbodies keep their own position/rotation and will overwrite the object's position/rotation sometimes.
                this._rb.position = scenePos;
            }
            this.transform.position = scenePos; // This is also important to happen always.
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private void UpdateSceneRotation()
        {
            Quaternion sceneRotation = (Quaternion)SceneReferenceFrameManager.SceneReferenceFrame.InverseTransformRotation( this._airfRotation );
            if( _rb != null )
            {
                // THIS IS CRITICALLY IMPORTANT.
                // Rigidbodies keep their own position/rotation and will overwrite the object's position/rotation sometimes.
                this._rb.rotation = sceneRotation;
            }
            this.transform.rotation = sceneRotation; // This is also important to happen always.
        }

        void Awake()
        {
            // in case RB is added by requirecomponent.
            _rb = this.GetComponent<Rigidbody>();
        }

        void Start()
        {
            // In case RB is added later (I don't want to lazily check every time it's retrieved, RB is not required).
            _rb = this.GetComponent<Rigidbody>();
        }

        // we need to move the object if the reference frame is moving, and move/rotate it, if the reference frame is rotating.
        void FixedUpdate()
        {
            this.AIRFPosition = SceneReferenceFrameManager.SceneReferenceFrame.TransformPosition( this.transform.position );
            this.AIRFRotation = SceneReferenceFrameManager.SceneReferenceFrame.TransformRotation( this.transform.rotation );
        }

        //void LateUpdate()
        //{
#warning TODO - AIRFPosition seems to lag behind one frame, behind the correct position. 
        // Transforming the scene position on demand instead of getting the AIRF value seems to work some of the time.
        // Updating it also in lateupdate also seems to fix it, but it breaks loaded position, making the craft drop underground somewhat.

        //    this.AIRFPosition = SceneReferenceFrameManager.SceneReferenceFrame.TransformPosition( this.transform.position );
        //    this.AIRFRotation = SceneReferenceFrameManager.SceneReferenceFrame.TransformRotation( this.transform.rotation );
        //}


        /// <summary>
        /// Callback to the event.
        /// </summary>
        public void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
        {
            UpdateScenePosition();
            UpdateSceneRotation();
        }

        public SerializedData GetData( IReverseReferenceMap s )
        {
            return new SerializedObject()
            {
                { "airf_position", s.WriteVector3Dbl( this.AIRFPosition ) },
                { "airf_rotation", s.WriteQuaternionDbl( this.AIRFRotation ) }
            };
        }

        public void SetData( IForwardReferenceMap l, SerializedData data )
        {
            if( data.TryGetValue( "airf_position", out var airfPosition ) )
                this.AIRFPosition = l.ReadVector3Dbl( airfPosition );

            if( data.TryGetValue( "airf_rotation", out var airfRotation ) )
                this.AIRFRotation = l.ReadQuaternionDbl( airfRotation );
        }
    }
}