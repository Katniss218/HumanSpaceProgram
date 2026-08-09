using System.Collections.Generic;
using System.Linq;
using HSP.Vessels;
using HSP.Vessels.Construction;
using UnityEngine;

namespace HSP.Vanilla
{
    public static class OnVesselConstructionSite
    {
        public const string ENSURE_CONSTRUCTION_SITE = HSPEvent.NAMESPACE_HSP + ".construction_site.ensure";
        public const string HANDLE_SPLIT = HSPEvent.NAMESPACE_HSP + ".construction_site.handle_split";
        public const string HANDLE_MERGE = HSPEvent.NAMESPACE_HSP + ".construction_site.handle_merge";

        [HSPEventListener( HSPEvent_ON_VESSEL_CREATED.ID, ENSURE_CONSTRUCTION_SITE )]
        private static void EnsureConstructionSiteOnVessel( Vessel vessel )
        {
            if( vessel == null ) return;
            if( vessel.GetComponent<FConstructionSite>() == null )
            {
                vessel.gameObject.AddComponent<FConstructionSite>();
            }
        }

        [HSPEventListener( HSPEvent_AFTER_VESSEL_SPLIT.ID, HANDLE_SPLIT )]
        private static void HandleConstructionSiteOnSplit( HSPEvent_AFTER_VESSEL_SPLIT.Data data )
        {
            if( data.OldVessel == null || data.NewVessel == null || data.SplitParts == null )
                return;

            var oldSite = data.OldVessel.GetComponent<FConstructionSite>();
            var newSite = data.NewVessel.GetComponent<FConstructionSite>();

            if( oldSite == null ) return;

            if( newSite == null )
            {
                newSite = data.NewVessel.gameObject.AddComponent<FConstructionSite>();
            }

            var movedParts = oldSite.Parts.Where( p => data.SplitParts.Contains( p ) ).ToList();
            if( movedParts.Count > 0 )
            {
                newSite.AddParts( movedParts );
                oldSite.RemoveParts( movedParts );
                if( oldSite.State != ConstructionState.NotStarted && newSite.State == ConstructionState.NotStarted )
                {
                    newSite.StartConstructing();
                }
            }
        }

        [HSPEventListener( HSPEvent_AFTER_VESSEL_MERGE.ID, HANDLE_MERGE )]
        private static void HandleConstructionSiteOnMerge( HSPEvent_AFTER_VESSEL_MERGE.Data data )
        {
            if( data.RemainingVessel == null || data.MergedVessel == null )
                return;

            var remainingSite = data.RemainingVessel.GetComponent<FConstructionSite>();
            var mergedSite = data.MergedVessel.GetComponent<FConstructionSite>();

            if( remainingSite == null )
            {
                remainingSite = data.RemainingVessel.gameObject.AddComponent<FConstructionSite>();
            }

            if( mergedSite != null )
            {
                remainingSite.MergeWith( mergedSite );
            }
        }
    }
}
