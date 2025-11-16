namespace HSP.ResourceFlow
{
    public interface IBuildsFlowNetwork
    {
        BuildFlowResult BuildFlowNetwork( FlowNetworkBuilder c );
        bool IsValid( FlowNetworkSnapshot snapshot );
        void ApplySnapshot( FlowNetworkSnapshot snapshot );
    }
}