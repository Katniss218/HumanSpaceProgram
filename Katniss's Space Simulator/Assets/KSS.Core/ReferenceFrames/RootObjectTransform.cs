using KSS.Core.Physics;
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
    [DisallowMultipleComponent]
    public class RootObjectTransform : MonoBehaviour, IReferenceFrameSwitchResponder
    {
        // Should to be added to any root object that is an actual [physical] object in the scene (not UI elements, empties, etc).

        // Root objects store their AIRF positions, children natively store their local coordinates, which as long as they're not obscenely large, will be fine.
        // - An object with a child at 0.00125f can be sent to 10e25 and brought back, and its child will remain at 0.00125f

        [SerializeField] Vector3Dbl _airfPosition;
        /// <summary>
        /// Gets or sets the position of the object in Absolute Inertial Reference Frame coordinates. Units in [m].
        /// </summary>
        public Vector3Dbl AIRFPosition
        {
            get => this._airfPosition;
            set
            {
                this._airfPosition = value;
                UpdateScenePosition();
            }
        }

        [SerializeField] QuaternionDbl _airfRotation;
        /// <summary>
        /// Gets or sets the rotation of the object in Absolute Inertial Reference Frame coordinates.
        /// </summary>
        public QuaternionDbl AIRFRotation
        {
            get => this._airfRotation;
            set
            {
                this._airfRotation = value;
                UpdateSceneRotation();
            }
        }

        Rigidbody _rb;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private void UpdateScenePosition()
        {
            Vector3 scenePos = (Vector3)SceneReferenceFrameManager.SceneReferenceFrame.InverseTransformPosition( this._airfPosition );
            if( _rb != null )
            {
                // THIS IS CRITICALLY IMPORTANT. Rigidbodies keep their own position/rotation.
                this._rb.position = scenePos;
            }
            this.transform.position = scenePos;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private void UpdateSceneRotation()
        {
            Quaternion sceneRotation = (Quaternion)SceneReferenceFrameManager.SceneReferenceFrame.InverseTransformRotation( this._airfRotation );
            if( _rb != null )
            {
                // THIS IS CRITICALLY IMPORTANT. Rigidbodies keep their own position/rotation.
                this._rb.rotation = sceneRotation;
            }
            this.transform.rotation = sceneRotation;
        }

        /// <summary>
        /// Call this after adding a rigidbody object.
        /// </summary>
        public void RefreshCachedRigidbody()
        {
            _rb = this.GetComponent<Rigidbody>();
        }

        private void RecacheAirfPosRot( IReferenceFrame referenceFrame )
        {
            if( this._rb == null )
            {
                this._airfPosition = referenceFrame.TransformPosition( this.transform.position );
                this._airfRotation = referenceFrame.TransformRotation( this.transform.rotation );
            }
            else
            {
                this._airfPosition = referenceFrame.TransformPosition( this._rb.position );
                this._airfRotation = referenceFrame.TransformRotation( this._rb.rotation );
            }
        }

        void FixedUpdate()
        {
            RecacheAirfPosRot( SceneReferenceFrameManager.SceneReferenceFrame );
        }

        /// <summary>
        /// Callback to the event.
        /// </summary>
        public void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
        {
            RecacheAirfPosRot( data.OldFrame );
            UpdateScenePosition();
            UpdateSceneRotation();
        }

        [SerializationMappingProvider( typeof( RootObjectTransform ) )]
        public static SerializationMapping RootObjectTransformMapping()
        {
            return new CompoundSerializationMapping<RootObjectTransform>()
            {
                ("airf_position", new Member<RootObjectTransform, Vector3Dbl>( o => o.AIRFPosition )),
                ("airf_rotation", new Member<RootObjectTransform, QuaternionDbl>( o => o.AIRFRotation )),
            }
            .IncludeMembers<Behaviour>()
            .UseBaseTypeFactory();
        }
        /*
        public SerializedData GetData( IReverseReferenceMap s )
        {
            SerializedObject ret = (SerializedObject)IPersistent_Behaviour.GetData( this, s );

            ret.AddAll( new SerializedObject()
            {
                { "airf_position", this.AIRFPosition.GetData() },
                { "airf_rotation", this.AIRFRotation.GetData() }
            } );

            return ret;
        }

        public void SetData( SerializedData data, IForwardReferenceMap l )
        {
            IPersistent_Behaviour.SetData( this, data, l );

            if( data.TryGetValue( "airf_position", out var airfPosition ) )
                this.AIRFPosition = airfPosition.AsVector3Dbl();

            if( data.TryGetValue( "airf_rotation", out var airfRotation ) )
                this.AIRFRotation = airfRotation.AsQuaternionDbl();
        }*/
    }
}