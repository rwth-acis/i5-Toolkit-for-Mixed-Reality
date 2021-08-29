public class IndexDataSource : DataSource<int>
{
    public override int Length => -1;

    public override bool Exists(int index)
    {
        return index >= 0;
    }

    public override int GetAt(int index)
    {
        return index;
    }
}
