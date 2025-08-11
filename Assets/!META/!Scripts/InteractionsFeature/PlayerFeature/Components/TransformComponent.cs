using UnityEngine;

public struct TransformComponent
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public TransformComponent(Vector3 position, Quaternion rotation, Vector3 scale) : this()
    {
        this.scale = scale;
        this.rotation = rotation;
        this.position = position;
    }


    public TransformComponent(Vector3 position, Quaternion rotation) : this()
    {
        this.scale = Vector3.one;
        this.rotation = rotation;
        this.position = position;
    }
}
