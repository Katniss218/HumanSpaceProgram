using System;
using System.Collections.Generic;

namespace HSP.ResourceFlow
{
    public static class SubstancePhaseMap
    {
        private static readonly Dictionary<(string substanceID, SubstancePhase targetPhase), ISubstance> _map = new();

        /// <summary>
        /// Registers a partnership between two substance phases. This is typically called at startup by a content loader.
        /// </summary>
        /// <param name="substance">The source substance.</param>
        /// <param name="partnerPhase">The phase of the partner.</param>
        /// <param name="partner">The partner substance instance.</param>
        public static void RegisterPhasePartner( ISubstance substance, SubstancePhase partnerPhase, ISubstance partner )
        {
            if( substance == null || partner == null )
                return;

            _map[(substance.ID, partnerPhase)] = partner;
        }

        /// <summary>
        /// Gets the substance that represents the target phase partner of the given substance.
        /// </summary>
        /// <param name="substance">The source substance.</param>
        /// <param name="phase">The desired phase of the partner.</param>
        /// <returns>The partner substance, or null if no partner is registered for that phase.</returns>
        public static ISubstance GetPartnerPhase( ISubstance substance, SubstancePhase phase )
        {
            if( substance == null )
                return null;

            if( _map.TryGetValue( (substance.ID, phase), out var partner ) )
            {
                return partner;
            }
            return null;
        }

        /// <summary>
        /// Clears all registered phase partners. Intended for use in test environments to ensure test isolation.
        /// </summary>
        public static void Clear()
        {
            _map.Clear();
        }
    }
}