namespace ShioUI.Traits;

public interface IKeyboardInteractHandler
{
    void OnKeyDown(ref KeyEventArgs args);
    void OnKeyUp(ref KeyEventArgs args);
}
