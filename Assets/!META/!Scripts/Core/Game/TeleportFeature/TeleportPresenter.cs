using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class TeleportPresenter : MonoBehaviour
{
    [SerializeField]
    private TeleportView _teleportPrefab;

    [field: SerializeField]

    public static event Action<TeleportModel> OnTeleported;
    public Dictionary<TeleportView, TeleportModel> AllTeleport { get; } = new Dictionary<TeleportView, TeleportModel>();

    public void Init()
    {
        FindAllTeleport();
        InitAllView();
    }

    public void Teleported(TeleportModel toTeleport)
    {
        OnTeleported?.Invoke(toTeleport);
    }

    public IReadOnlyList<KeyValuePair<TeleportView, TeleportModel>> GetTeleports()
    {
        return AllTeleport.OrderBy(element => element.Key.floorNumber).ToList();
    }

    public void CreateTeleport()
    {
        if (_teleportPrefab == null)
            return;

        var teleportView = Instantiate(_teleportPrefab, transform);
        teleportView.Init(this);
        AllTeleport.Add(teleportView, new(
            teleportView.transform,
            this,
            teleportView.scaleFactor));
    }

    private void OnValidate()
    {
        FindAllTeleport();
    }

    private void InitAllView()
    {
        var views = AllTeleport?.Keys;
        if (views == null)
            return;

        foreach (var vKey in views)
        {
            vKey.Init(this);
        }
    }

    private void FindAllTeleport()
    {
        var allTeleport = GetComponentsInChildren<TeleportView>();
        foreach (var eTeleportView in allTeleport)
        {
            AllTeleport.TryAdd(eTeleportView, new(
                eTeleportView.transform,
                this,
                eTeleportView.scaleFactor));
        }
    }
}
