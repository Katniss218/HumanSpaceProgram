using KSS.Control.Controls;
using KSS.Control;
using KSS.Core;
using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Components
{
    public class FVesselSeparator : MonoBehaviour
    {
        bool _hasSeparated = false;

        [NamedControl( "Separate", "Connect this to the sequencer, or a controller's separation output." )]
        public ControlleeInput Separate;
        private void SeparateListener()
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
            Separate = new ControlleeInput( SeparateListener );
        }

        [SerializationMappingProvider( typeof( FVesselSeparator ) )]
        public static SerializationMapping FVesselSeparatorMapping()
        {
            return new MemberwiseSerializationMapping<FVesselSeparator>()
            {
                ("separate", new Member<FVesselSeparator, ControlleeInput>( o => o.Separate )),
                ("has_separated", new Member<FVesselSeparator, bool>( o => o._hasSeparated ))
            }
            .IncludeMembers<Behaviour>()
            .UseBaseTypeFactory();
        }
        /*
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
                l.SetObj( separate.AsGuid(), Separate );
            }
        }

        public SerializedData GetData( IReverseReferenceMap s )
        {
            SerializedObject ret = (SerializedObject)IPersistent_Behaviour.GetData( this, s );

            ret.AddAll( new SerializedObject()
            {
                { "has_separated", _hasSeparated.GetData() },
                { "separate", Separate.GetData( s ) }
            } );

            return ret;
        }

        public void SetData( SerializedData data, IForwardReferenceMap l )
        {
            IPersistent_Behaviour.SetData( this, data, l );

            if( data.TryGetValue( "has_separated", out var hasSeparated ) )
                _hasSeparated = hasSeparated.AsBoolean();

            if( data.TryGetValue( "separate", out var separate ) )
                Separate.SetData( separate, l );
        }*/
    }
}