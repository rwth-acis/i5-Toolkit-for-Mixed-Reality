public abstract class ListDataSource<T>
{
    public delegate void ItemUpdatedHandler(int index);

    public event ItemUpdatedHandler ItemUpdated;

    public abstract T GetAt(int index);

    public virtual bool ExistsAt(int index)
    {
        return index >= 0;
    }
}
