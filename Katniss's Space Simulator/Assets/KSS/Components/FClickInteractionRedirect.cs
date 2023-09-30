using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Components
{
    [RequireComponent( typeof( Collider ) )]
    public class FClickInteractionRedirect : MonoBehaviour
    {
        public Transform target;
    }
}