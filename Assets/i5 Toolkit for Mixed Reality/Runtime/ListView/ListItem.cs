using TMPro;
using UnityEngine;

public abstract class ListItem<T> : MonoBehaviour
{
    public ListView<T> Parent { get; private set; }

    public int Index { get; private set; }

    public T ItemContent
    {
        get
        {
            return Parent.DataSource.GetAt(Index);
        }
    }

    public virtual bool Visible
    {
        get => gameObject.activeSelf;
        set
        {
            gameObject.SetActive(value);
        }
    }

    public virtual void SetUp(ListView<T> parent, int index)
    {
        Parent = parent;
        Index = index;
        UpdateView();
    }

    public virtual void UpdateView()
    {
        Visible = Parent != null && Parent.DataSource != null && Parent.DataSource.Exists(Index);
    }
}
