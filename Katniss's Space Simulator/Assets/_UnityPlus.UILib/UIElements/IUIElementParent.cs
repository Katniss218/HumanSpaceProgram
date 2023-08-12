using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    public interface IUIElementParent
    {
        public RectTransform contents { get; }
        public GameObject gameObject { get; }

        public List<UIElement> Children { get; }
    }
}
