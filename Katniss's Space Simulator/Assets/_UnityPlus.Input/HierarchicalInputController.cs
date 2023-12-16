using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Input
{
    public class HierarchicalInputController : MonoBehaviour
    {
        static readonly KeyCode[] KEYS = Enum.GetValues( typeof( KeyCode ) )
            .Cast<KeyCode>()
            .ToArray();

        IEnumerable<KeyCode> GetPressedKeys( IEnumerable<KeyCode> targetKeySet )
        {
            List<KeyCode> keys = new List<KeyCode>();
            foreach( var key in targetKeySet )
            {
                if( UnityEngine.Input.GetKeyDown( key ) )
                {
                    keys.Add( key );
                }
            }
            return keys;
        }

        IEnumerable<KeyCode> GetReleasedKeys( IEnumerable<KeyCode> targetKeySet )
        {
            List<KeyCode> keys = new List<KeyCode>();
            foreach( var key in targetKeySet )
            {
                if( UnityEngine.Input.GetKeyUp( key ) )
                {
                    keys.Add( key );
                }
            }
            return keys;
        }

        void Update()
        {
            // remove keys no longer being pressed.
            IEnumerable<KeyCode> keys = GetReleasedKeys( HierarchicalInputManager._keys );
            foreach( var key in keys )
            {
                HierarchicalInputManager._keys.Remove( key );
            }

            if( UnityEngine.Input.anyKeyDown )
            {
                // add keys that were pressed
                keys = GetPressedKeys( KEYS );
                foreach( var key in keys )
                {
                    HierarchicalInputManager._keys.Add( key );
                }
            }

            HierarchicalInputManager.Update();
        }
    }
}