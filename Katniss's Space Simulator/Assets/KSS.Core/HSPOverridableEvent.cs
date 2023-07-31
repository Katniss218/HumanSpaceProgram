using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.OverridableEvents;

namespace KSS.Core
{
    public static class HSPOverridableEvent
    {
        public static OverridableEventManager EventManager { get; private set; } = new OverridableEventManager();

        public const string NAMESPACE_VANILLA = "vanilla";

        public const string EVENT_STARTUP_IMMEDIATELY = NAMESPACE_VANILLA + ".startup.immediately";
        public const string EVENT_STARTUP_MAINMENU = NAMESPACE_VANILLA + ".startup.mainmenu";

        public const string EVENT_TIMELINE_LOADER_CREATE = NAMESPACE_VANILLA + ".timeline.loader.create";
        public const string EVENT_TIMELINE_SAVER_CREATE = NAMESPACE_VANILLA + ".timeline.loader.create";
    }
}
