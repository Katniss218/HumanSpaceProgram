using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Core
{
    /// <summary>
    /// Manages loading, unloading, switching, etc of vessels.
    /// </summary>
    public class VesselManager : SerializedManager, IPersistent
    {
        public static Vessel ActiveVessel { get; set; }

        static List<Vessel> _vessels;

        public static Vessel[] GetLoadedVessels()
        {
            return _vessels.ToArray();
        }

        internal static void Register( Vessel vessel )
        {
            if( _vessels == null )
                _vessels = new List<Vessel>();

            _vessels.Add( vessel );
        }

        internal static void Unregister( Vessel vessel )
        {
            if( _vessels != null )
                _vessels.Remove( vessel );
        }

        internal static GameObject[] GetAllRootGameObjects()
        {

            GameObject[] gos = new GameObject[_vessels.Count];
            for( int i = 0; i < _vessels.Count; i++ )
            {
                gos[i] = _vessels[i].gameObject;
            }
            return gos;
        }

        public SerializedData GetData( ISaver s )
        {
            return new SerializedObject()
            {
                { "active_vessel", s.WriteObjectReference( ActiveVessel ) }
            };
        }

        public void SetData( ILoader l, SerializedData data )
        {
            if( data.TryGetValue( "active_vessel", out var activeVessel ) )
                ActiveVessel = (Vessel)l.ReadObjectReference( activeVessel );
        }
    }
}