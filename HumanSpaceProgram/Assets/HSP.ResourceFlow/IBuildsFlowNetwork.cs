namespace HSP.ResourceFlow
{
    public interface IBuildsFlowNetwork
    {
        BuildFlowResult BuildFlowNetwork( FlowNetworkBuilder c );
        bool IsValid( FlowNetworkSnapshot snapshot );
        void ApplySnapshot( FlowNetworkSnapshot snapshot );

        /// <summary>
        /// Called before the solver step to allow the component to push its current state
        /// (e.g., physics vectors, pump pressure) into its simulation objects within the snapshot.
        /// This is for cheap, non-structural updates.
        /// </summary>
        void SynchronizeState( FlowNetworkSnapshot snapshot );
    }
}