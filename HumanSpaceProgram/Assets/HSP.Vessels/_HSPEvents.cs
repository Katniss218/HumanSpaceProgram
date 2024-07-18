
namespace HSP.Vessels
{
    public static class HSPEvent_VesselCreated
    {
        public const string EventID = HSPEvent.NAMESPACE_HSP + ".vessel_created";
    }

    public static class HSPEvent_VesselDestroyed
    {
        public const string EventID = HSPEvent.NAMESPACE_HSP + ".vessel_destroyed";
    }

    public static class HSPEvent_VesselHierarchyChanged
    {
#warning TODO - maybe remove the dependency on hspevent.eventmanager and make this type-safe? 
        // (by having a separate event for each... event. And getting rid of the eventmanager completely)
        // It would also need some marker attribute on the events to tell the HSPEventListener attribute where to add the listeners.

        public const string EventID = HSPEvent.NAMESPACE_HSP + ".vessel_hierachy_changed";
    }
}