using KSS.Control.Controls;
using KSS.Control;
using KSS.Core;
using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Components
{
    public class FVesselSeparator : MonoBehaviour, IPersistsObjects, IPersistsData
    {
        bool _hasSeparated = false;

        [NamedControl( "Separate", "Connect this to the sequencer, or a controller's separation output." )]
        public ControlleeInput<byte> Separate;
        private void SeparateListener( byte _ )
        {
            if( _hasSeparated )
            {
                return;
            }

            VesselHierarchyUtils.SetParent( this.transform, null );
            _hasSeparated = true;
        }

        void Awake()
        {
            Separate = new ControlleeInput<byte>( SeparateListener );
        }

        public SerializedObject GetObjects( IReverseReferenceMap s )
        {
            return new SerializedObject()
            {
                { "separate", s.GetID( Separate ).GetData() },
            };
        }

        public void SetObjects( SerializedObject data, IForwardReferenceMap l )
        {
            if( data.TryGetValue( "separate", out var separate ) )
            {
                Separate = new( SeparateListener );
                l.SetObj( separate.ToGuid(), Separate );
            }
        }

        public SerializedData GetData( IReverseReferenceMap s )
        {
            SerializedObject ret = (SerializedObject)IPersistent_Behaviour.GetData( this, s );

            ret.AddAll( new SerializedObject()
            {
                { "has_separated", this._hasSeparated },
                { "separate", this.Separate.GetData( s ) }
            } );

            return ret;
        }

        public void SetData( SerializedData data, IForwardReferenceMap l )
        {
            IPersistent_Behaviour.SetData( this, data, l );

            if( data.TryGetValue( "has_separated", out var hasSeparated ) )
                this._hasSeparated = (bool)hasSeparated;

            if( data.TryGetValue( "separate", out var separate ) )
                this.Separate.SetData( separate, l );
        }
    }
}