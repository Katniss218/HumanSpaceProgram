using KSS.Core;

namespace KSS
{
    public static class HSPEventVanilla
    {
        /// <summary>
        /// Invoked after a construction site is created.
        /// </summary>
        public const string GAMEPLAY_AFTER_CONSTRUCTION_SITE_CREATED = HSPEvent.NAMESPACE_VANILLA + ".gameplayscene.after_csite_created";

        /// <summary>
        /// Invoked after a construction site is destroyed.
        /// </summary>
        public const string GAMEPLAY_AFTER_CONSTRUCTION_SITE_DESTROYED = HSPEvent.NAMESPACE_VANILLA + ".gameplayscene.after_csite_destroyed";
    }
}