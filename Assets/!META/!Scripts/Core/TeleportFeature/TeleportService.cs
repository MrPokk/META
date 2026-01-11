using System;
using System.Collections.Generic;
using System.Linq;

public class TeleportService
{
    public event Action<TeleportPoint> OnTeleport;

    private readonly List<TeleportPoint> _teleports = new();

    public void RegisterTeleport(TeleportPoint teleportPoint)
    {
        _teleports.Add(teleportPoint);
    }

    public void UnregisterTeleport(TeleportPoint teleportPoint)
    {
        _teleports.Remove(teleportPoint);
    }

    public IReadOnlyList<TeleportPoint> GetSortTeleports(Comparison<TeleportPoint> comparison)
    {
        var sortTeleport = _teleports.ToList();
        sortTeleport.Sort(comparison);
        return sortTeleport;
    }

    public IReadOnlyList<TeleportPoint> GetTeleports()
    {
        return _teleports.ToArray();
    }

    public void ExecuteTeleport(TeleportPoint teleportPoint)
    {
        OnTeleport?.Invoke(teleportPoint);
    }
}
