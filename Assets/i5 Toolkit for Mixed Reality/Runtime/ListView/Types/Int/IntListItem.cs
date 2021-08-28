using TMPro;
using UnityEngine;

public class IntListItem : ListItem<int>
{
    [SerializeField] private TextMeshPro label;

    public override void UpdateView()
    {
        base.UpdateView();

        if (Visible)
        {
            label.text = ItemContent.ToString();
        }
    }
}
