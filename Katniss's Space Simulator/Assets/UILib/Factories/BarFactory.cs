using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UILib.Factories
{
    public static class BarFactory
    {
        public static (RectTransform t, ValueBar bar)
            CreateEmptyHorizontal( RectTransform parent, string name, UILayoutInfo layoutInfo, UIStyle style )
        {
            (GameObject rootGO, RectTransform rootT) = UIHelper.CreateUI( parent, name, layoutInfo );

            Image img = rootGO.AddComponent<Image>();
            img.raycastTarget = false;
            img.sprite = style.BarBackground;
            img.type = Image.Type.Sliced;

            ValueBar bar = rootGO.AddComponent<ValueBar>();
            bar.PaddingLeft = 5f;
            bar.PaddingRight = 5f;

            return (rootT, bar);
        }
    }
}