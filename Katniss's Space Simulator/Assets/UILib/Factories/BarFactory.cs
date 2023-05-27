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
        public static (RectTransform t, ValueBarSegment bar)
            CreateHorizontal( RectTransform parent, string name, float left, float right, UILayoutInfo layoutInfo, UIStyle style )
        {
            (GameObject rootGO, RectTransform rootT) = UIHelper.CreateUI( parent, name, layoutInfo );

            Image img = rootGO.AddComponent<Image>();
            img.raycastTarget = false;
            img.sprite = style.BarBackground;
            img.type = Image.Type.Sliced;

            (GameObject maskGO, RectTransform maskT) = UIHelper.CreateUI( rootT, "mask", new UILayoutInfo( new Vector2( 0, 0 ), new Vector2( 0, 1 ), Vector2.zero, new Vector2( rootT.rect.width, 0 ) ) );

            Image imgM = maskGO.AddComponent<Image>();
            imgM.raycastTarget = false;
            imgM.sprite = null;

            Mask mask = maskGO.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            (GameObject imgGO, RectTransform imgT) = UIHelper.CreateUI( maskT, "foreground", new UILayoutInfo( new Vector2( 0, 0 ), new Vector2( 0, 1 ), Vector2.zero, new Vector2( rootT.rect.width, 0 ) ) );

            Image img2 = imgGO.AddComponent<Image>();
            img2.raycastTarget = false;
            img2.sprite = style.Bar;
            img2.type = Image.Type.Sliced;

            ValueBar bar = rootGO.AddComponent<ValueBar>();
            ValueBarSegment barS = maskGO.AddComponent<ValueBarSegment>();

            barS.SetImage( imgT );
            barS.LeftPadding = style.BarLeftPadding;
            barS.RightPadding = style.BarRightPadding;
            barS.Left = left;
            barS.Right = right;

            return (rootT, barS);
        }
    }
}