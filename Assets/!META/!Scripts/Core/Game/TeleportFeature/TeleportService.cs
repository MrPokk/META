using System;
using System.Collections.Generic;

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

    public IReadOnlyList<TeleportPoint> GetTeleports()
    {
        return _teleports;
    }

    public void ExecuteTeleport(TeleportPoint teleportPoint)
    {
        OnTeleport?.Invoke(teleportPoint);
    }
}
