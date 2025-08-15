public abstract class UIScreen : UIBase
{
    public override void Show(object data = null)
    {
        base.Show(data);
        transform.SetAsLastSibling();
    }
}
