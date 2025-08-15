public abstract class UIPopup : UIBase
{
    public override void Show(object data = null)
    {
        base.Show(data);
        transform.SetAsLastSibling();
    }
}
