using System;
using UnityEditor;
using UnityEngine;

namespace UnityPlus.Serialization
{
#if UNITY_EDITOR
    [CustomEditor( typeof( PreexistingReference ) )]
    public class PreexistingReferenceEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            PreexistingReference updater = (PreexistingReference)target;

            DrawDefaultInspector();

            GUILayout.Label( "Guid: " + updater.GetPersistentGuid() );
        }
    }
#endif

    /// <summary>
    /// Holds a persistent reference identifier for this component's game object.
    /// </summary>
    /// <remarks>
    /// Used by <see cref="JsonPreexistingGameObjectsStrategy"/>.
    /// </remarks>
    [ExecuteInEditMode]
    public class PreexistingReference : MonoBehaviour
    {
        [SerializeField]
        [HideInInspector]
        byte[] _guidData = null;

        /// <summary>
        /// Gets the persistent (unchanging) global identifier for this object.
        /// </summary>
        public Guid GetPersistentGuid() => new Guid( _guidData );

        /// <summary>
        /// Sets the persistent (unchanging) global identifier for this object.
        /// </summary>
        public void SetPersistentGuid( Guid guid ) => _guidData = guid.ToByteArray();

        void Start()
        {
            if( _guidData == null )
            {
                Debug.LogError( $"Deleting preexisting reference with unassigned GUID - '{this.gameObject.name}'. Please assign a valid guid first." );
                Destroy( this );
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if( _guidData == null )
            {
                _guidData = new byte[16];
                new System.Random().NextBytes( _guidData );
            }
        }
#endif
    }
}