namespace BitterECS.Core
{
    public class DisposableUtility : IEcsPostDestroySystem
    {
        public Priority PrioritySystem => Priority.LAST_TASK;

        public void PostDestroy()
        {
            EcsWorld.Instance.Dispose();
            EcsSystems.Instance.Dispose();
        }
    }
}
