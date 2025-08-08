using System;
using BitterECS.Core;
using Mirror;

public class EcsNetworkView : NetworkBehaviour, ILinkableView
{
    public EcsViewProperty Properties { get; set; }

    public void Dispose()
    {
        Properties = null;
        Destroy(gameObject);
        GC.SuppressFinalize(this);
    }

    public void Init(EcsViewProperty property)
    {
        Properties ??= property;
    }
}
