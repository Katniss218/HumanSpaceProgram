using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.UILib.Layout;

namespace UnityPlus.UILib.UIElements
{
    public class UICanvas : UIElement, IUIElementContainer, IUILayoutDriven
    {
        public RectTransform contents => base.rectTransform;

        public List<IUIElementChild> Children { get; }

        public LayoutDriver LayoutDriver { get; set; }

        public UICanvas( Canvas canvas ) : base( (RectTransform)canvas.transform )
        {
            Children = new List<IUIElementChild>();
        }


        public static explicit operator UICanvas( Canvas canvas )
        {
            return new UICanvas( canvas );
        }
    }
}