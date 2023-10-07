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

            GUILayout.Label( "Guid: " + updater.GetGuid() );
        }
    }
#endif

    /// <summary>
    /// Represents a gameobject that has a pre-set fixed reference identifier.
    /// </summary>
    public class PreexistingReference : MonoBehaviour
    {
        [SerializeField]
        [HideInInspector]
        byte[] _guidData = null;

        public Guid GetGuid() => new Guid( _guidData );
        public void SetGuid( Guid guid ) => _guidData = guid.ToByteArray();

        void Start()
        {
            if( _guidData == null )
            {
                Debug.LogError( $"Deleting preexisting reference with unassigned GUID - '{this.gameObject.name}'. Please assign a valid guid first." );
                Destroy( this );
            }
        }
    }
}