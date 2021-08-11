using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ListItem : MonoBehaviour
{
    [SerializeField] private TextMeshPro label;

    public ListView Parent { get; private set; }

    public int Index { get; private set; }

    public void SetUp(ListView parent, int index)
    {
        Parent = parent;
        Index = index;
        UpdateView();
    }

    private void UpdateView()
    {
        label.text = Index.ToString();
    }
}
