using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    public static class RectTransformEx
    {
        public static Vector2 GetParentSize( this RectTransform rt )
        {
            Transform parent = rt.parent;

            if( parent == null )
                return rt.sizeDelta;

            return GetParentSize( (RectTransform)parent );
        }
    }
}