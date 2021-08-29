using System.Collections.Generic;

public class ListDataSource<T> : DataSource<T>
{
    public List<T> List { get; set; }

    public override int Length => List.Count;

    public override bool Exists(int index)
    {
        return index >= 0 && index < List.Count;
    }

    public override T GetAt(int index)
    {
        return List[index];
    }
}
