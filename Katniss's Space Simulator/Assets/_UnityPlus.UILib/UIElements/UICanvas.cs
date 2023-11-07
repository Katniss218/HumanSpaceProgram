using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.UILib.Layout;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// A component that identifies a given canvas. Works in tandem with the <see cref="CanvasManager"/>.
    /// </summary>
    [RequireComponent( typeof( Canvas ) )]
    public class UICanvas : UIElement, IUIElementContainer, IUILayoutDriven
    {
        public RectTransform contents => base.rectTransform;

        public List<IUIElementChild> Children { get; } = new List<IUIElementChild>();

        public LayoutDriver LayoutDriver { get; set; }

        /// <summary>
        /// A unique identifier of the canvas. Should be unique among all canvases in all concurrently loaded scenes.
        /// </summary>
        [field: SerializeField]
        public string ID { get; private set; } = null;

        public static explicit operator UICanvas( Canvas canvas )
        {
            return canvas.GetComponent<UICanvas>();
        }
    }
}