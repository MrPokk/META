using System;

[Serializable]
public struct MovingComponent
{
    public float speed;

    public MovingComponent(float speed)
    {
        this.speed = speed;
    }
}
