using UnityEngine;

public class TeleportModel
{
    private readonly Transform _position;
    private readonly TeleportPresenter _teleportPresenter;

    public float ScaleFactor { get; private set; }
    public TeleportModel(Transform position, TeleportPresenter teleportPresenter, float scaleFactor)
    {
        _position = position;
        _teleportPresenter = teleportPresenter;
        ScaleFactor = scaleFactor;
    }

    public Vector3 GetPosition()
    {
        return _position.position;
    }
}

