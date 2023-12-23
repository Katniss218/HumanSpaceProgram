using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSS.Core
{
    public static class HierarchicalInputChannelID
    {
        // they could use namespaced IDs 🤔

        public const string COMMON_LEFT_CLICK = "clmb";
        public const string COMMON_RIGHT_CLICK = "crmb";
        public const string COMMON_MIDDLE_CLICK = "cmmb";
    }

    public static class HierarchicalInputPriority
    {
        public const int MIN = 10;

        public const int VERY_LOW = 10000000;
        public const int LOW = 20000000;
        public const int MEDIUM = 30000000;
        public const int HIGH = 40000000;
        public const int VERY_HIGH = 50000000;

        public const int MAX = 59999990;
    }
}