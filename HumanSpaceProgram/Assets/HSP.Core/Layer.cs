using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSP.Core
{
    /// <summary>
    /// Named GameObject layers
    /// </summary>
    public enum Layer : int
    {
        DEFAULT = 0,
        Unity_TransparentFx = 1,
        Unity_IgnoreRaycast = 2,
        /// <summary>
        /// Singleton scene managers.
        /// </summary>
        MANAGERS = 3,
        Unity_Water = 4,
        Unity_UI = 5,
        _6 = 6,
        _7 = 7,
        _8 = 8,
        _9 = 9,
        /// <summary>
        /// A celestial body (or any of its child objects).
        /// </summary>
        CELESTIAL_BODY = 10,
        /// <summary>
        /// A light that illuminates <see cref="CELESTIAL_BODY"/>.
        /// </summary>
        CELESTIAL_BODY_LIGHT = 11,
        _12 = 12,
        _13 = 13,
        _14 = 14,
        _15 = 15,
        _16 = 16,
        _17 = 17,
        _18 = 18,
        _19 = 19,
        /// <summary>
        /// A vessel/building/etc (or any of its child objects).
        /// </summary>
        PART_OBJECT = 20,
        /// <summary>
        /// A light that illuminates <see cref="PART_OBJECT_LIGHT"/>.
        /// </summary>
        PART_OBJECT_LIGHT = 21,
        _22 = 22,
        _23 = 23,
        _24 = 24,
        /// <summary>
        /// A vessel/building/etc that is being held by the cursor in the design scene (or any of its child objects).
        /// </summary>
        VESSEL_DESIGN_HELD = 25,
        _26 = 26,
        _27 = 27,
        HIDDEN_SPECIAL_1 = 28,
        HIDDEN_SPECIAL_2 = 29,
        HIDDEN_SPECIAL_3 = 30,
        POST_PROCESSING = 31,
    }
}