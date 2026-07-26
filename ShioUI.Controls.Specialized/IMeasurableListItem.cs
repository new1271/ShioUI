namespace ShioUI.Controls.Specialized;

public interface IMeasurableListItem<T> : IListItem where T : IMeasuringContext
{
    int AdjustHeight(T context);

    int ResetHeight(T context);
}