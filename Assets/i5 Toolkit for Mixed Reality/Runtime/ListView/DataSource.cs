public abstract class DataSource<T>
{
    public delegate void ItemUpdatedHandler(int index);

    public event ItemUpdatedHandler ItemUpdated;

    public abstract T GetAt(int index);

    public abstract bool Exists(int index);
}
