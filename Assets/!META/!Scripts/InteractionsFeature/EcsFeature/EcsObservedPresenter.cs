using System.Collections.Generic;
using BitterECS.Core;

public class EcsObservedPresenter : EcsPresenter
{
    protected override void Registration()
    {
        AddLimitedType<EcsObservedEntity>();
    }
}
