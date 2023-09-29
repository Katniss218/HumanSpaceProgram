using UnityEngine;

namespace UnityPlus.UILib
{
    /// <summary>
    /// A component that identifies a given canvas. Works in tandem with the <see cref="CanvasManager"/>.
    /// </summary>
    [RequireComponent( typeof( Canvas ) )]
    public class CanvasID : MonoBehaviour
    {
        /// <summary>
        /// A unique identifier of the canvas. Should be unique among all canvases in all concurrently loaded scenes.
        /// </summary>
        [field: SerializeField]
        public string ID { get; set; } = null;
    }
}