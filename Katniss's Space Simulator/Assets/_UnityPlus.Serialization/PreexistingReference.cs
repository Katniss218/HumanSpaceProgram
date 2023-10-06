using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        [SerializeField] [HideInInspector]
        byte[] _guidData = null;

        public Guid GetGuid() => new Guid( _guidData );

        private void Awake()
        {
            if( _guidData == null )
            {
                Debug.LogError( $"Preexisting reference with unassigned GUID - '{this.gameObject.name}'. Please assign a valid guid." );
                Destroy( this );
            }
        }

        private void OnValidate()
        {
            if( _guidData == null )
            {
                Guid guid = Guid.NewGuid();
                _guidData = guid.ToByteArray();
            }
        }
    }
}