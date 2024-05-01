using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityPlus.AssetManagement;

namespace KSS.UI
{
    internal static class UIDefaults
    {
        // These are supposed to be used by extension methods, and those extension methods are to be used by the modders.

        internal static TMP_FontAsset TextFont => AssetRegistry.Get<TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" );
        internal static float TextFontSize => 12f;
        internal static Color TextFontColor => Color.white;
    }
}