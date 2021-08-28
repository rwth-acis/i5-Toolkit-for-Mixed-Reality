public class IndexDataSource : ListDataSource<int>
{
    public override int GetAt(int index)
    {
        return index;
    }
}
