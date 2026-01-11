public struct StateComponent
{
    public State state;

    public StateComponent(State state)
    {
        this.state = state;
    }

    public enum State
    {
        Idle,
        Moving,
    }
}
