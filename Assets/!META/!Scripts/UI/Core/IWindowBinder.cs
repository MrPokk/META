using VContainer;

public interface IWindowBinder
{
    public IWindowBinder Bind(IObjectResolver viewModel);
    public void Open();
    public void Close();
}
