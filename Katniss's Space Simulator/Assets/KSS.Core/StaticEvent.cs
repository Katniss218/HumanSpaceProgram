using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.StaticEvents;

namespace KSS.Core
{
    public static class StaticEvent
    {
        public static OverridableEventManager Instance { get; private set; } = new OverridableEventManager();

        public const string NAMESPACE_VANILLA = "vanilla";
        public const string STARTUP_IMMEDIATELY = NAMESPACE_VANILLA + ".startup.immediately";
        public const string STARTUP_MAINMENU = NAMESPACE_VANILLA + ".startup.mainmenu";
    }
}
