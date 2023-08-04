using UnityEngine;

namespace UnityPlus.UILib
{
    [RequireComponent( typeof( Canvas ) )]
    public class CanvasID : MonoBehaviour
    {
        [field: SerializeField] 
        public string ID { get; set; } = null;
    }
}