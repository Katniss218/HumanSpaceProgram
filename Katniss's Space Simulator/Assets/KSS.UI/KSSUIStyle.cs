using System.Collections;
using System.Collections.Generic;
using UILib;
using UnityEngine;

namespace KSS.UI
{
    [CreateAssetMenu( fileName = "style", menuName = "KSS/UI Style", order = 100 )]
    public class KSSUIStyle : UIStyle
    {
        public Sprite PartWindowBackground;
        public Sprite PartWindowFunctionalityBackground;

        public Sprite AttitudeIndicatorBackground;

        public Sprite AltitudeIndicatorBackground;

        public Sprite VelocityIndicatorBackground;

        public Color BarColorInert;
        public Color BarColorFuel;
        public Color BarColorOxidizer;
        public Color BarColorCombined;

        public float BarOpacitySolid;
        public float BarOpacityLiquid;
        public float BarOpacityGas;
    }
}