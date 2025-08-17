using VContainer;

public interface IWindowBinder
{
    public void Bind(IObjectResolver viewModel);
    public void Open();
    public void Close();
}
