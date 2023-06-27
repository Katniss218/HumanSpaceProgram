using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace UnityPlus.Serialization
{
    public class CoroutineRunner : MonoBehaviour
    {
        [Serializable]
        public class CoroutineAction : UnityEvent<MonoBehaviour> { }

        public CoroutineAction Coroutines;

        [field: SerializeField]
        public bool RunOnStart { get; set; } = false;

        public void StartCoroutines()
        {
            Coroutines.Invoke( this );
        }

        void Start()
        {
            if( RunOnStart )
            {
                StartCoroutines();
            }
        }
    }
}